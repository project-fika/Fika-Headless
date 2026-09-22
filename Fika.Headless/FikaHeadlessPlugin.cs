using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using Comfort.Common;
using Diz.Jobs;
using EFT;
using EFT.Communications;
using EFT.UI;
using Fika.Core;
using Fika.Core.Main.Patches.LocalGame;
using Fika.Core.Main.Patches.Overrides;
using Fika.Core.Main.Utils;
using Fika.Core.Networking;
using Fika.Core.Networking.Http;
using Fika.Core.Networking.Models.Headless;
using Fika.Core.Networking.Packets.Communication;

#if DEBUG
using Fika.Core.Networking.Websocket.Headless;
#endif
using Fika.Core.UI.Patches;
using Fika.Core.UI.Patches.MainMenuUI;
using Fika.Headless.Classes;
using Fika.Headless.Patches;
using Newtonsoft.Json;
using SPTushonka.Custom.Patches;
using SPTushonka.Custom.Utils;
using SPTushonka.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Scripting;

namespace Fika.Headless;

[BepInPlugin("com.fika.headless", "Fika.Headless", HeadlessVersion)]
[BepInDependency("com.fika.core", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency("com.sptushonka.custom", BepInDependency.DependencyFlags.HardDependency)]
public class FikaHeadlessPlugin : BasePlugin
{
    public const string HeadlessVersion = "1.5.2";

    public static FikaHeadlessPlugin Instance { get; private set; }
    public static ManualLogSource FikaHeadlessLogger;
    public static bool PlayerDisposeError;
    public static bool IsRunningWindows
    {
        get
        {
            return SystemInfo.operatingSystemFamily == OperatingSystemFamily.Windows;
        }
    }
    public bool CanHost { get; internal set; }
    public int CurrentRaidCount { get; private set; }

    private HeadlessWebSocket _fikaHeadlessWebSocket;
    private float _gcCounter;
    private float _gcPoint;
    private bool _invalidPluginsFound;
    private int _restartAfterAmountOfRaids;
    private bool _hasVerified;

    public static ConfigEntry<int> UpdateRate { get; private set; }
    public static ConfigEntry<int> RAMCleanInterval { get; private set; }
    public static ConfigEntry<bool> ShouldBotsSleep { get; private set; }
    public static ConfigEntry<bool> ShouldDestroyGraphics { get; private set; }
    public static ConfigEntry<bool> DestroyRenderersOnSceneLoad { get; private set; }

#if DEBUG
    public BetterAudio BetterAudio
    {
        get
        {
            return Singleton<BetterAudio>.Instance;
        }
    }

    public GUISounds GUISounds
    {
        get
        {
            return Singleton<GUISounds>.Instance;
        }
    }
#endif

    public override void Load()
    {
        Instance = this;
        _gcCounter = 0;

        TrySetConsoleTitle();

        FikaHeadlessLogger = Log;

        GetHeadlessRestartAfterRaidAmount();
        SetupConfig();

        _gcPoint = RAMCleanInterval.Value * 60f;

        DisableFikaCorePatches();
        DisableSPTPatches();

        Il2CppInjection.RegisterAll(typeof(FikaHeadlessPlugin).Assembly, Log);
        AddComponent<HeadlessTicker>();

        PatchManager manager = new(this, true);
        manager.EnablePatches();

        if (!ShouldBotsSleep.Value)
        {
            PatchManager botManager = new(this);
            botManager.EnablePatch(new BotStandBy_Update_Transpiler());
        }

        Log.LogInfo($"Fika.Headless loaded! OS: {SystemInfo.operatingSystem}");
        if (!IsRunningWindows)
        {
            Log.LogWarning("You are not running an officially supported operating system by Fika. Minimal support will be given.");
        }

        CleanupLogFiles();
        FikaBackendUtils.IsHeadless = true;
    }

    private void TrySetConsoleTitle()
    {
        var title = Environment.GetCommandLineArgs()
            .FirstOrDefault(a => a.StartsWith("-title=", StringComparison.OrdinalIgnoreCase))
            ?["-title=".Length..];

        if (!string.IsNullOrEmpty(title))
        {
            Log.LogInfo($"Using custom window title: {title}");
            Console.Title = $"Fika Headless {HeadlessVersion} - {title}";
        }
    }

    internal void Tick()
    {
        _gcCounter += Time.unscaledDeltaTime;
        if (_gcCounter > _gcPoint)
        {
            _gcCounter -= _gcPoint;
            if (FikaGlobals.IsInRaid)
            {
                GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
                GarbageCollector.CollectIncremental(1000000);
                GarbageCollector.GCMode = GarbageCollector.Mode.Disabled;
            }
            else if (!FikaBackendUtils.IsTransit)
            {
                Resources.UnloadUnusedAssets();
                InGameMemoryManagement.Collect(2, Il2CppSystem.GCCollectionMode.Forced, true, true, true);
            }
        }
    }

    private static void DisableSPTPatches()
    {
        new SetPreRaidSettingsScreenDefaultsPatch().Disable();
    }

    /// <summary>
    /// Disables patches from Fika.Core that the headless does not need
    /// </summary>
    private static void DisableFikaCorePatches()
    {
        new TarkovApplication_InitNotificationManager_Patch().Disable();
        new MenuScreen_Awake_Patch().Disable();
        new TarkovApplication_LocalGameCreator_Patch().Disable();
    }

#if DEBUG
    private void StartDebugGame()
    {
        var rawData = @"{""Type"":""HeadlessStartRaid"",""StartHeadlessRequest"":{""headlessSessionID"":""6840a12f76cac3fada302293"",""time"":""CURR"",""locationId"":""5b0fc42d86f7744a585f9105"",""spawnPlace"":""SamePlace"",""metabolismDisabled"":false,""timeAndWeatherSettings"":{""isRandomTime"":false,""isRandomWeather"":false,""cloudinessType"":""Clear"",""rainType"":""NoRain"",""windType"":""Light"",""fogType"":""NoFog"",""timeFlowType"":""x1"",""hourOfDay"":-1},""botSettings"":{""isScavWars"":false,""botAmount"":""AsOnline""},""wavesSettings"":{""botAmount"":""AsOnline"",""botDifficulty"":""AsOnline"",""isBosses"":true,""isTaggedAndCursed"":false},""side"":""Pmc"",""customWeather"":false}}";
        var data = System.Text.Json.JsonSerializer.Deserialize<StartRaid>(rawData, HeadlessWebSocket.RequestOptions);

        OnFikaStartRaid(data.StartHeadlessRequest);
    }
#endif

    /// <summary>
    /// Cleans up all old log files
    /// </summary>
    private void CleanupLogFiles()
    {
        try
        {
            var exePath = AppContext.BaseDirectory;
            var logsPath = Path.Combine(exePath, "Logs");

            if (!Directory.Exists(logsPath))
            {
                Log.LogError("CleanupLogFiles: '/Logs' folder not found!");
                return;
            }

            DirectoryInfo logsDir = new(logsPath);

            var directoriesToDelete = logsDir.EnumerateDirectories()
                                         .OrderByDescending(d => d.LastWriteTime)
                                         .Skip(2); // keep the latest 3 folders, 1 is added which leaves 3

            foreach (var dir in directoriesToDelete)
            {
                try
                {
                    Log.LogInfo($"CleanupLogFiles: Deleting directory '{dir.FullName}'");
                    dir.Delete(true);
                }
                catch (IOException ioEx)
                {
                    Log.LogWarning($"CleanupLogFiles: I/O error deleting '{dir.FullName}': {ioEx.Message}");
                }
                catch (UnauthorizedAccessException uaEx)
                {
                    Log.LogWarning($"CleanupLogFiles: Access denied for '{dir.FullName}': {uaEx.Message}");
                }
                catch (Exception ex)
                {
                    Log.LogWarning($"CleanupLogFiles: Unexpected error for '{dir.FullName}': {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Log.LogError($"CleanupLogFiles: Fatal error: {ex.Message}");
        }
    }

    /// <summary>
    /// Initializes the <see cref="ConfigFile"/> settings
    /// </summary>
    private void SetupConfig()
    {
        UpdateRate = Config.Bind("Headless", "Update Rate", 60,
            new ConfigDescription("How often the server should update (frame cap / tick rate). Only works if your machine can actually reach the selected setting",
            new AcceptableValueRange<int>(30, 120)));

        RAMCleanInterval = Config.Bind("Headless", "RAM Clean Interval", 15,
            new ConfigDescription("How often in minutes the RAM cleaner should run outside of raids",
            new AcceptableValueRange<int>(2, 30)));

        ShouldBotsSleep = Config.Bind("Headless", "Bot sleeping", false,
            new ConfigDescription("Should the headless host allow bots to sleep? (BSG bot sleeping logic)"));

        ShouldDestroyGraphics = Config.Bind("Headless", "Destroy Graphics", true,
            new ConfigDescription("If the headless plugin should run patches to disable various graphical elements"));

        DestroyRenderersOnSceneLoad = Config.Bind("Headless", "Destroy Renderers", true,
            new ConfigDescription("If the headless plugin should hook scene loading to disable unnecessary renderers as well as unloading all materials (Requires 'Destroy Graphics' to be enabled)"));
    }

    /// <summary>
    /// When a request to start a raid is received
    /// </summary>
    /// <param name="request"></param>
    /// <exception cref="NullReferenceException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public void OnFikaStartRaid(StartHeadlessRequest request)
    {
        try
        {
            if (!TarkovApplication.Exist(out var tarkovApplication))
            {
                Log.LogError("OnFikaStartRaid: Could not find TarkovApplication");
                return;
            }

            if (!CanHost)
            {
                Log.LogError("The headless client was not ready to host yet");
                return;
            }

            var session = tarkovApplication.Session;
            if (session == null)
            {
                Log.LogError("Session was null when starting the raid");
                return;
            }

            if (!session.LocationSettings.locations.TryGetValue(request.LocationId, out var location))
            {
                Log.LogError($"Failed to find location {request.LocationId}");
                return;
            }

            FikaBackendUtils.CustomRaidSettings = request.CustomRaidSettings;
            Log.LogInfo($"Received CustomRaidSettings: {request.CustomRaidSettings}");

            Log.LogInfo($"Starting on location {location.Name}");
            CanHost = false;
            ToggleFramelimit(false);
            _ = BeginFikaStartRaid(request, session, tarkovApplication);
        }
        catch (Exception ex)
        {
            Log.LogError(ex.Message);
        }
    }

    /// <summary>
    /// Verifies that this headless client is valid for hosting
    /// </summary>
    /// <returns></returns>
    public async Task RunPluginValidation()
    {
        if (_hasVerified)
        {
            return;
        }

        Log.LogInfo("Running plugin validation");
        while (!FikaPlugin.Instance.LocalesLoaded)
        {
            await Task.Delay(100);
        }
        await VerifyPlugins();

        await Task.Delay(1000);

        FikaPlugin.Instance.Settings.AutoExtract.Value = true;
        FikaPlugin.Instance.Settings.QuestTypesToShareAndReceive.Value = 0;
        FikaPlugin.Instance.Settings.ConnectionTimeout.Value = 30;
        FikaPlugin.Instance.Settings.UseNamePlates.Value = false;

        FikaPlugin.Instance.Settings.AllowFreeCam = true;
        FikaPlugin.Instance.Settings.AllowSpectateFreeCam = true;

        Log.LogInfo("Plugin validation completed");

        if (!TarkovApplication.Exist(out var tarkovApplication))
        {
            Log.LogWarning("Could not find TarkovApplication");
            return;
        }

        // Temporarily disabled
        //await GetQuestTemplates((Class303)tarkovApplication.Session);

        // Artifical 5 second delay to let the game work an extra bit
        await Task.Delay(TimeSpan.FromSeconds(5));

        MainThread.Post(CreateHeadlessWebsocket);
        _hasVerified = true;
    }

    /// <summary>
    /// Creates and connects the <see cref="HeadlessWebSocket"/>
    /// </summary>
    private void CreateHeadlessWebsocket()
    {
        _fikaHeadlessWebSocket = new();
        if (!_invalidPluginsFound)
        {
            _fikaHeadlessWebSocket.Connect();
        }
    }

    /// <summary>
    /// Verifies that no invalid plugins are loaded
    /// </summary>
    private Task VerifyPlugins()
    {
        Log.LogInfo("Verifying plugins");

        List<string> invalidPluginList =
        [
            "com.Amanda.Graphics",
            "com.Amanda.Sense",
            "VIP.TommySoucy.MoreCheckmarks",
            "com.kmyuhkyuk.EFTApi",
            "com.mpstark.DynamicMaps",
            "IhanaMies.LootValue",
            "com.cactuspie.ramcleanerinterval",
            "com.TYR.DeClutter",
            "SPTVRAMCleaner.UniqueGUID",
            "harmonyzt.sptleaderboard",
            "com.acidphantasm.stattrack"
        ];
        PluginInfo[] pluginInfos = [.. IL2CPPChainloader.Instance.Plugins.Values];
        List<string> unsupportedMods = [];

        foreach (var Info in pluginInfos)
        {
            if (invalidPluginList.Contains(Info.Metadata.GUID))
            {
                unsupportedMods.Add($"{Info.Metadata.Name}, GUID: {Info.Metadata.GUID}");
            }
        }

        if (unsupportedMods.Count > 0)
        {
            var modsString = string.Join("; ", unsupportedMods);
            Log.LogFatal($"{unsupportedMods.Count} invalid plugins found, this headless host will not be available for hosting! Remove these mods: {modsString}");
            _invalidPluginsFound = true;
            if (IsRunningWindows)
            {
                MessageBoxHelper.Show($"{unsupportedMods.Count} invalid plugins found, this headless host will not be available for hosting! Check your log files for more information.",
                    "HEADLESS ERROR", MessageBoxHelper.MessageBoxType.OK);
            }
            Thread.Sleep(-1);
            return Task.CompletedTask;
        }

        _invalidPluginsFound = false;

        Log.LogInfo("Plugins verified successfully");

        return Task.CompletedTask;
    }

    private async Task BeginFikaStartRaid(StartHeadlessRequest request, IEftSession session, TarkovApplication tarkovApplication)
    {
        RaidSettings raidSettings = new()
        {
            Side = request.Side,
            PlayersSpawnPlace = request.SpawnPlace,
            MetabolismDisabled = request.MetabolismDisabled,
            BotSettings = request.BotSettings,
            WavesSettings = request.WavesSettings,
            TimeAndWeatherSettings = request.TimeAndWeatherSettings,
            SelectedDateTime = request.Time,
            SelectedLocation = session.LocationSettings.locations.Values.FirstOrDefault(location => location._Id == request.LocationId),
            isInTransition = false,
            RaidMode = ERaidMode.Local,
            IsPveOffline = true,
            OnlinePveRaid = false,
            transitionType = request.UseEvent ? ELocationTransition.Event : ELocationTransition.None
        };

        raidSettings.BotSettings.BotAmount = request.WavesSettings.BotAmount;

        tarkovApplication._raidSettings = raidSettings;

        Log.LogInfo("Initialized raid settings");

        if (FikaPlugin.Instance.Settings.ForceIP.Value != "")
        {
            // We need to handle DNS entries as well
            var ip = FikaPlugin.Instance.Settings.ForceIP.Value;
            try
            {
                var endpoint = NetUtils.ResolveAddress(ip);
            }
            catch
            {
                var message = $"'{ip}' is not a valid IP address to connect to! Check your 'Force IP' setting.";
                Singleton<PreloaderUI>.Instance.ShowCriticalErrorScreen("ERROR FORCING IP",
                    message,
                    ErrorScreen.EButtonType.OkButton, 10f);
                Log.LogError(message);
                return;
            }
        }

        if (FikaPlugin.Instance.Settings.ForceBindIP.Value != "Disabled")
        {
            if (!IPAddress.TryParse(FikaPlugin.Instance.Settings.ForceBindIP.Value, out _))
            {
                var message = $"'{FikaPlugin.Instance.Settings.ForceBindIP.Value}' is not a valid IP address to bind to! Check your 'Force Bind IP' setting.";
                Singleton<PreloaderUI>.Instance.ShowCriticalErrorScreen("ERROR BINDING",
                    message,
                    ErrorScreen.EButtonType.OkButton, 10f);
                Log.LogError(message);
                return;
            }
        }

        Log.LogInfo($"Starting with: {JsonConvert.SerializeObject(raidSettings)}");

        await FikaBackendUtils.CreateMatch(session.Profile.ProfileId,
            session.Profile.Info.Nickname, raidSettings);

        Log.LogInfo("Match successfully created, loading raid");

        FikaBackendUtils.IsHeadlessGame = true;

        //_verifyConnectionsRoutine = StartCoroutine(VerifyPlayersRoutine(tarkovApplication));

        try
        {
            tarkovApplication.CurrentRaidNum++;
            tarkovApplication.CurrentTotalRaidNum++;
            Singleton<JobScheduler>.Instance.SetForceMode(true);
            Log.LogInfo($"Starting raid on {raidSettings.SelectedLocation.Name.Localized()}");
            _ = WaitForPlayersToConnect();
            await tarkovApplication.LocalGameMatching(raidSettings.TimeAndWeatherSettings);
            Log.LogInfo("Raid init complete, starting raid");
            CurrentRaidCount++;
        }
        catch (Exception ex)
        {
            Log.LogError($"Exception caught during raid init: {ex.Message}");
            Log.LogError(ex);
            if (Singleton<FikaServer>.Instantiated)
            {
                MessagePacket packet = new()
                {
                    Message = LocaleUtils.UI_ERROR_RAID_INIT,
                    NotificationDurationType = ENotificationDurationType.Default,
                    NotificationIconType = ENotificationIconType.Alert,
                    Color = Color.red
                };
                Singleton<FikaServer>.Instance.SendData(ref packet, DeliveryMethod.ReliableOrdered);
            }

            Application.Quit(1);
            //tarkovApplication.method_39("Local game matching", ex);
        }
        Singleton<JobScheduler>.Instance.SetForceMode(false);
    }

    private async Task WaitForPlayersToConnect()
    {
        await Task.Delay(TimeSpan.FromSeconds(45));
        if (Singleton<FikaServer>.Instantiated && Singleton<FikaServer>.Instance.NetServer.ConnectedPeersCount == 0)
        {
            Log.LogWarning("No connections after 2 minutes, terminating");
            await Task.Delay(TimeSpan.FromSeconds(5));
            MainThread.Post(() => Application.Quit());
        }
    }

    public void OnReady()
    {
        ToggleFramelimit(true);

        if (PlayerDisposeError)
        {
            Log.LogError("There were some errors in the last raid while disposing of players. This is most likely caused by any content mods that adds new equipment. The headless will now shut down to prevent memory leaks.");
            Application.Quit();
            return;
        }

        if (CurrentRaidCount == 0)
        {
            return;
        }

        Log.LogInfo($"Headless has done {CurrentRaidCount} raids, and is set to restart after {_restartAfterAmountOfRaids}");
        if (_restartAfterAmountOfRaids != 0 && CurrentRaidCount >= _restartAfterAmountOfRaids && !FikaBackendUtils.IsTransit)
        {
            Application.Quit();
        }
    }

    public void ToggleFramelimit(bool enabled)
    {        
        if (enabled)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 1;
            if (Singleton<JobScheduler>.Instantiated)
            {
                Singleton<JobScheduler>.Instance.SetTargetFrameRate(1);
            }
        }
        else
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = UpdateRate.Value;
            if (Singleton<JobScheduler>.Instantiated)
            {
                Singleton<JobScheduler>.Instance.SetTargetFrameRate(UpdateRate.Value);
            }
        }
        Log.LogInfo($"Setting frame limiter to {(enabled ? "enabled" : "disabled")}, current target is {Application.targetFrameRate}Hz");
    }

    private void GetHeadlessRestartAfterRaidAmount()
    {
        var headlessConfig = FikaRequestHandler.GetHeadlessRestartAfterRaidAmount();
        _restartAfterAmountOfRaids = headlessConfig.Amount;
    }
}
