using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using Comfort.Common;
using Diz.Jobs;
using Diz.Utils;
using EFT;
using EFT.UI;
using Fika.Core;
using Fika.Core.Coop.GameMode;
using Fika.Core.Coop.Patches;
using Fika.Core.Coop.Utils;
using Fika.Core.Networking;
using Fika.Core.Networking.Http;
using Fika.Core.Networking.Models;
using Fika.Core.Networking.Websocket.Headless;
using Fika.Core.Patching;
using Fika.Core.UI.Patches;
using Fika.Headless.Classes;
using Fika.Headless.Patches;
using HarmonyLib;
using Newtonsoft.Json;
using SPT.Custom.Patches;
using SPT.Custom.Utils;
using SPT.SinglePlayer.Patches.ScavMode;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Fika.Headless
{
    [BepInPlugin("com.fika.headless", "Fika.Headless", HeadlessVersion)]
    [BepInDependency("com.fika.core", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.SPT.custom", BepInDependency.DependencyFlags.HardDependency)]
    public class FikaHeadlessPlugin : BaseUnityPlugin
    {
        public const string HeadlessVersion = "1.3.6";

        public static FikaHeadlessPlugin Instance { get; private set; }
        public static ManualLogSource FikaHeadlessLogger;
        public static bool IsRunningWindows
        {
            get
            {
                return SystemInfo.operatingSystemFamily == OperatingSystemFamily.Windows;
            }
        }
        public bool CanHost { get; internal set; }

        private HeadlessWebSocket FikaHeadlessWebSocket;
        private float gcCounter;
        private float gcPoint;
        private Coroutine verifyConnectionsRoutine;
        private bool invalidPluginsFound;
        private int currentRaidCount = 0;
        private int restartAfterAmountOfRaids = 0;

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

        protected void Awake()
        {
            Instance = this;
            gcCounter = 0;

            FikaHeadlessLogger = Logger;

            GetHeadlessRestartAfterRaidAmount();
            SetupConfig();

            gcPoint = RAMCleanInterval.Value * 60f;

            DisableFikaCorePatches();
            DisableSPTPatches();

            PatchManager manager = new(this, true);
            manager.EnablePatches();

            if (!ShouldBotsSleep.Value)
            {
                PatchManager botManager = new(this);
                botManager.EnablePatch(new BotStandBy_Update_Transpiler());
            }

            Logger.LogInfo($"Fika.Headless loaded! OS: {SystemInfo.operatingSystem}");
            if (!IsRunningWindows)
            {
                Logger.LogWarning("You are not running an officially supported operating system by Fika. Minimal support will be given. Please cleanup your '/Logs' folder manually.");
            }
            else
            {
                CleanupLogFiles();
            }

            FikaBackendUtils.IsHeadless = true;
        }

        private void DisableSPTPatches()
        {
            new MemoryCollectionPatch().Disable();
            new SetPreRaidSettingsScreenDefaultsPatch().Disable();
            new DisablePMCExtractsForScavsPatch().Disable();
        }

        /// <summary>
        /// Disables patches from Fika.Core that the headless does not need
        /// </summary>
        private static void DisableFikaCorePatches()
        {
            PatchManager manager = new("com.fika.core", "Fika.Core");
            manager.AddPatch(new TarkovApplication_method_18_Patch());
            manager.AddPatch(new MenuScreen_Awake_Patch());
            manager.AddPatch(new TarkovApplication_LocalGameCreator_Patch());
            manager.DisablePatches();
        }

#if DEBUG
        private void StartDebugGame()
        {
            string rawData = @"{""Type"":""HeadlessStartRaid"",""StartHeadlessRequest"":{""headlessSessionID"":""6840a12f76cac3fada302293"",""time"":""CURR"",""locationId"":""5b0fc42d86f7744a585f9105"",""spawnPlace"":""SamePlace"",""metabolismDisabled"":false,""timeAndWeatherSettings"":{""isRandomTime"":false,""isRandomWeather"":false,""cloudinessType"":""Clear"",""rainType"":""NoRain"",""windType"":""Light"",""fogType"":""NoFog"",""timeFlowType"":""x1"",""hourOfDay"":-1},""botSettings"":{""isScavWars"":false,""botAmount"":""AsOnline""},""wavesSettings"":{""botAmount"":""AsOnline"",""botDifficulty"":""AsOnline"",""isBosses"":true,""isTaggedAndCursed"":false},""side"":""Pmc"",""customWeather"":false}}";
            StartRaid data = JsonConvert.DeserializeObject<StartRaid>(rawData);

            OnFikaStartRaid(data.StartHeadlessRequest);
        } 
#endif

        /// <summary>
        /// Gets all quest templates from the server
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        private async Task GetQuestTemplates(Class303 session)
        {
            Logger.LogInfo("Getting quest templates");
            List<RawQuestClass> list = await session.method_3<List<RawQuestClass>>(new()
            {
                Url = session.Gclass1358_0.Main + "/fika/headless/questtemplates",
                ParseInBackground = true,
                Params = new Class59<bool>(true),
                Retries = new byte?(LegacyParamsStruct.DefaultRetries)
            });
            Logger.LogInfo($"Received {list.Count} quest templates");

            GClass3772.Instance.GlobalQuestTemplates.AddRange(list);
        }

        /// <summary>
        /// Cleans up all old log files
        /// </summary>
        private void CleanupLogFiles()
        {
            string exePath = AppContext.BaseDirectory;
            string logsPath = Path.Combine(exePath, "Logs");
            if (!Directory.Exists(logsPath))
            {
                Logger.LogError("CleanupLogFiles: Could not finds '/Logs' folder!");
                return;
            }

            DirectoryInfo logsDir = new(logsPath);
            foreach (DirectoryInfo dir in logsDir.EnumerateDirectories())
            {
                try
                {
                    Logger.LogInfo($"CleanupLogFiles: Deleting {dir.Name}");
                    dir.Delete(true);
                }
                catch
                {
                    Logger.LogWarning($"CleanupLogFiles: Could not delete {dir.Name}, it's probably being used");
                }
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
                new AcceptableValueRange<int>(5, 30)));

            ShouldBotsSleep = Config.Bind("Headless", "Bot sleeping", false,
                new ConfigDescription("Should the headless host allow bots to sleep? (BSG bot sleeping logic)"));

            ShouldDestroyGraphics = Config.Bind("Headless", "Destroy Graphics", true,
                new ConfigDescription("If the headless plugin should run patches to disable various graphical elements"));

            DestroyRenderersOnSceneLoad = Config.Bind("Headless", "Destroy Renderers", true,
                new ConfigDescription("If the headless plugin should hook scene loading to disable unnecessary renderers as well as unloading all materials (Requires 'Destroy Graphics' to be enabled)"));
        }

        protected void Update()
        {
            gcCounter += Time.unscaledDeltaTime;

            if (gcCounter > (gcPoint) && !FikaGlobals.IsInRaid())
            {
                Logger.LogDebug("Clearing memory");
                gcCounter -= gcPoint;
                Resources.UnloadUnusedAssets().Await();
                MemoryControllerClass.Collect(2, GCCollectionMode.Forced, true, true, true);
            }
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
                if (!TarkovApplication.Exist(out TarkovApplication tarkovApplication))
                {
                    Logger.LogError("OnFikaStartRaid: Could not find TarkovApplication");
                    return;
                }

                if (!CanHost)
                {
                    Logger.LogError("The headless client was not ready to host yet");
                    return;
                }

                ISession session = tarkovApplication.Session;
                if (session == null)
                {
                    Logger.LogError("Session was null when starting the raid");
                    return;
                }

                if (!session.LocationSettings.locations.TryGetValue(request.LocationId, out LocationSettingsClass.Location location))
                {
                    Logger.LogError($"Failed to find location {request.LocationId}");
                    return;
                }

                OfflineRaidSettingsMenuPatch_Override.UseCustomWeather = request.CustomWeather;

                Logger.LogInfo($"Starting on location {location.Name}");
                CanHost = false;
                _ = BeginFikaStartRaid(request, session, tarkovApplication);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return;
            }
        }

        /// <summary>
        /// Verifies that this headless client is valid for hosting
        /// </summary>
        /// <returns></returns>
        public async Task RunPluginValidation()
        {
            Logger.LogInfo("Running plugin validation");
            while (!FikaPlugin.Instance.LocalesLoaded)
            {
                await Task.Delay(100);
            }
            await VerifyPlugins();

            await Task.Delay(1000);

            FikaPlugin.AutoExtract.Value = true;
            FikaPlugin.QuestTypesToShareAndReceive.Value = 0;
            FikaPlugin.ConnectionTimeout.Value = 30;
            FikaPlugin.UseNamePlates.Value = false;
            FikaPlugin.UseFikaGC.Value = false;

            FikaPlugin.Instance.AllowFreeCam = true;
            FikaPlugin.Instance.AllowSpectateFreeCam = true;

            Logger.LogInfo("Plugin validation completed");

            if (!TarkovApplication.Exist(out TarkovApplication tarkovApplication))
            {
                Logger.LogWarning("Could not find TarkovApplication");
                return;
            }

            // Temporarily disabled
            //await GetQuestTemplates((Class303)tarkovApplication.Session);

            // Artifical 5 second delay to let the game work an extra bit
            await Task.Delay(TimeSpan.FromSeconds(5));

            AsyncWorker.RunInMainTread(CreateHeadlessWebsocket);
        }

        /// <summary>
        /// Creates and connects the <see cref="HeadlessWebSocket"/>
        /// </summary>
        private void CreateHeadlessWebsocket()
        {
            FikaHeadlessWebSocket = new();
            if (!invalidPluginsFound)
            {
                FikaHeadlessWebSocket.Connect();
            }
        }

        /// <summary>
        /// Verifies that no invalid plugins are loaded
        /// </summary>
        private Task VerifyPlugins()
        {
            Logger.LogInfo("Verifying plugins");

            List<string> invalidPluginList =
            [
                "com.Amanda.Graphics",
                "com.Amanda.Sense",
                "VIP.TommySoucy.MoreCheckmarks",
                "com.kmyuhkyuk.EFTApi",
                "com.mpstark.DynamicMaps",
                "IhanaMies.LootValue",
                "com.cactuspie.ramcleanerinterval",
                "com.TYR.DeClutter"
            ];
            PluginInfo[] pluginInfos = [.. Chainloader.PluginInfos.Values];
            List<string> unsupportedMods = [];

            foreach (PluginInfo Info in pluginInfos)
            {
                if (invalidPluginList.Contains(Info.Metadata.GUID))
                {
                    unsupportedMods.Add($"{Info.Metadata.Name}, GUID: {Info.Metadata.GUID}");
                }
            }

            if (unsupportedMods.Count > 0)
            {
                string modsString = string.Join("; ", unsupportedMods);
                Logger.LogFatal($"{unsupportedMods.Count} invalid plugins found, this headless host will not be available for hosting! Remove these mods: {modsString}");
                invalidPluginsFound = true;
                if (IsRunningWindows)
                {
                    MessageBoxHelper.Show($"{unsupportedMods.Count} invalid plugins found, this headless host will not be available for hosting! Check your log files for more information.",
                        "HEADLESS ERROR", MessageBoxHelper.MessageBoxType.OK);
                }
                Thread.Sleep(-1);
                return Task.CompletedTask;
            }

            invalidPluginsFound = false;

            Logger.LogInfo("Plugins verified successfully");

            return Task.CompletedTask;
        }

        /// <summary>
        /// Verifies that the <see cref="GameWorld"/> has started loading and that at least one peer has connected
        /// </summary>
        /// <param name="tarkovApplication"></param>
        /// <returns></returns>
        private IEnumerator VerifyPlayersRoutine(TarkovApplication tarkovApplication)
        {
            yield break;
            yield return new WaitForSeconds(300);
            if (Singleton<FikaServer>.Instance.NetServer.ConnectedPeersCount < 1)
            {
                int attempts = 0;
                while ((CoopGame)Singleton<IFikaGame>.Instance == null && attempts < 5)
                {
                    yield return new WaitForSeconds(5);
                    attempts++;
                    if (attempts >= 5)
                    {
                        Logger.LogError("More than 5 attempts were required to get the CoopGame instance. Something is probably very wrong!");
                    }
                }

                // TODO: Fix
                /*CoopGame coopGame = (CoopGame)Singleton<IFikaGame>.Instance;
                if (coopGame != null)
                {
                    coopGame.StopFromCancel(FikaBackendUtils.Profile.ProfileId, ExitStatus.Runner);
                }*/
                Logger.LogWarning("The were no connections after 5 minutes, attempting to terminate session...");
            }
        }

        private async Task BeginFikaStartRaid(StartHeadlessRequest request, ISession session, TarkovApplication tarkovApplication)
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
                OnlinePveRaid = false
            };

            raidSettings.BotSettings.BotAmount = request.WavesSettings.BotAmount;

            Traverse.Create(tarkovApplication).Field<RaidSettings>("_raidSettings").Value = raidSettings;

            Logger.LogInfo("Initialized raid settings");

            if (FikaPlugin.ForceIP.Value != "")
            {
                // We need to handle DNS entries as well
                string ip = FikaPlugin.ForceIP.Value;
                try
                {
                    IPAddress[] dnsAddress = Dns.GetHostAddresses(FikaPlugin.ForceIP.Value);
                    if (dnsAddress.Length > 0)
                    {
                        ip = dnsAddress[0].ToString();
                    }
                }
                catch
                {

                }

                if (!IPAddress.TryParse(ip, out _))
                {
                    Singleton<PreloaderUI>.Instance.ShowCriticalErrorScreen("ERROR FORCING IP",
                        $"'{ip}' is not a valid IP address to connect to! Check your 'Force IP' setting.",
                        ErrorScreen.EButtonType.OkButton, 10f);
                    return;
                }
            }

            if (FikaPlugin.ForceBindIP.Value != "Disabled")
            {
                if (!IPAddress.TryParse(FikaPlugin.ForceBindIP.Value, out _))
                {
                    Singleton<PreloaderUI>.Instance.ShowCriticalErrorScreen("ERROR BINDING",
                        $"'{FikaPlugin.ForceBindIP.Value}' is not a valid IP address to bind to! Check your 'Force Bind IP' setting.",
                        ErrorScreen.EButtonType.OkButton, 10f);
                    return;
                }
            }

            Logger.LogInfo($"Starting with: {JsonConvert.SerializeObject(raidSettings)}");

            Task createMatchTask = FikaBackendUtils.CreateMatch(session.Profile.ProfileId, session.Profile.Info.Nickname, raidSettings);
            while (!createMatchTask.IsCompleted)
            {
                await Task.Delay(100);
            }

            FikaBackendUtils.IsHeadlessGame = true;

            verifyConnectionsRoutine = StartCoroutine(VerifyPlayersRoutine(tarkovApplication));

            try
            {
                Singleton<JobScheduler>.Instance.SetForceMode(true, -1f);
                Logger.LogInfo($"Starting raid on {raidSettings.SelectedLocation.Name.Localized()}");
                await tarkovApplication.method_41(raidSettings.TimeAndWeatherSettings);
                Logger.LogInfo("Raid init complete, starting raid");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Exception caught during raid init: {ex.Message}");
                Logger.LogError(ex);
                tarkovApplication.method_39("Local game matching", ex);
            }
            Singleton<JobScheduler>.Instance.SetForceMode(false, -1f);
        }

        public void OnSessionResultExitStatus_Show()
        {
            currentRaidCount++;
            if (restartAfterAmountOfRaids != 0)
            {
                if (currentRaidCount >= restartAfterAmountOfRaids)
                {
                    Application.Quit();
                }
            }
        }

        private void GetHeadlessRestartAfterRaidAmount()
        {
            RestartAfterRaidAmountModel headlessConfig = FikaRequestHandler.GetHeadlessRestartAfterRaidAmount();
            restartAfterAmountOfRaids = headlessConfig.Amount;
        }
    }
}
