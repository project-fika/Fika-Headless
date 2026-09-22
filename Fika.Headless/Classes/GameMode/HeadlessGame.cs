using Audio.RadioSystem;
using Audio.SpatialSystem;
using BepInEx.Logging;
using Comfort.Common;
using CommonAssets.Scripts.Game;
using Dissonance.Networking.Client;
using Diz.Jobs;
using EFT;
using EFT.Airdrop;
using EFT.AssetsManager;
using EFT.Bots;
using EFT.CameraControl;
using EFT.EnvironmentEffect;
using EFT.Interactive;
using EFT.InventoryLogic;
using EFT.Settings;
using EFT.UI;
using EFT.UI.Screens;
using EFT.Utilities;
using EFT.Weather;
using Fika.Core;
using Fika.Core.Main.GameMode;
using Fika.Core.Main.Patches.BTR;
using Fika.Core.Main.Patches.Overrides;
using Fika.Core.Main.Players;
using Fika.Core.Main.Utils;
using Fika.Core.Modding;
using Fika.Core.Modding.Events;
using Fika.Core.Networking;
using Fika.Core.Networking.Http;
using Fika.Core.Networking.Models;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using JsonType;
using Koenigz.PerfectCulling;
using Koenigz.PerfectCulling.EFT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.LowLevel;

namespace Fika.Headless.Classes.GameMode;

public class HeadlessGame : AbstractGame, IFikaGame, IClientHearingTable
{
    public HeadlessGame(IntPtr pointer) : base(pointer)
    {
    }

    public override string LocationObjectId
    {
        get
        {
            return string.Empty;
        }
    }

    public override GameUI GameUi
    {
        get
        {
            return null;
        }
    }

    public override string ProfileId
    {
        get
        {
            return string.Empty;
        }
    }

    public List<int> ExtractedPlayers { get; } = [];

    public ExitStatus ExitStatus { get; set; } = ExitStatus.Survived;

    public string ExitLocation { get; set; }

    public ESeason Season
    {
        get
        {
            return GameController.Season;
        }
        set
        {
            GameController.Season = value;
        }
    }

    public SeasonsSettings SeasonsSettings
    {
        get
        {
            return GameController.SeasonsSettings;
        }

        set
        {
            GameController.SeasonsSettings = value;
        }
    }

    private ManualLogSource _logger { get; set; }

    public IEftSession BackendSession { get; set; }

    public BaseGameController GameController { get; set; }
    public GameDateTime GameDateTime
    {
        get
        {
            return (GameController as HeadlessGameController).GameDateTime;
        }
        set
        {
            (GameController as HeadlessGameController).GameDateTime = value;
        }
    }
    public GameWorld GameWorld { get; private set; }

    private LocalRaidSettings _localRaidSettings;
    private Callback<ExitStatus, Il2CppSystem.TimeSpan, ClientMetrics> _exitCallback;
    private LocationSettings.Location _location;
    private EDateTime _tarkovDateTime;
    private DateTime _dateTime;
    private float _voipDistance;
    private readonly Dictionary<string, DateTime> _factoryTimes = new()
    {
        {
            "factory4_day",
            new DateTime(2016, 8, 4, 15, 28, 0, DateTimeKind.Utc)
        },
        {
            "factory4_night",
            new DateTime(2016, 8, 4, 3, 28, 0, DateTimeKind.Utc)
        }
    };

    public static HeadlessGame Create(GameWorld gameWorld, GameDateTime backendDateTime,
        LocationSettings.Location location, TimeAndWeatherSettings timeAndWeather, WavesSettings wavesSettings,
        EDateTime dateTime, Callback<ExitStatus, Il2CppSystem.TimeSpan, ClientMetrics> callback, float fixedDeltaTime,
        EUpdateQueue updateQueue, IEftSession backEndSession, TimeSpan sessionTime, LocalRaidSettings localRaidSettings,
        RaidSettings raidSettings)
    {
        FikaGlobals.NetworkManager.RaidSide = localRaidSettings.playerSide;        

        var game = Create<HeadlessGame>(updateQueue, new Il2CppSystem.Nullable<Il2CppSystem.TimeSpan>(sessionTime.ToIl2Cpp()));
        game._logger = Logger.CreateLogSource(nameof(HeadlessGame));
        game.GameWorld = gameWorld;

        var gameTime = backendDateTime;
        if (timeAndWeather.HourOfDay != -1)
        {
            game._logger.LogInfo($"Using custom time, hour of day: {timeAndWeather.HourOfDay}");
            var currentTime = backendDateTime.StatedGameDateTime;
            DateTime newTime = new(currentTime.Year, currentTime.Month, currentTime.Day, timeAndWeather.HourOfDay,
                currentTime.Minute, currentTime.Second, currentTime.Millisecond);
            gameTime = new(backendDateTime.StatedRealDateTime, newTime.ToIl2Cpp(), backendDateTime.TimeFactor);
            gameTime.Reset(newTime.ToIl2Cpp());
            dateTime = EDateTime.CURR;
        }

        const float num = 1.5f;
        foreach (var wildSpawnWave in location.waves)
        {
            wildSpawnWave.slots_min = (int)(wildSpawnWave.slots_min * num);
            wildSpawnWave.slots_max = (int)(wildSpawnWave.slots_max * num);
        }

        game.BackendSession = backEndSession;
        game._exitCallback = callback;
        game._location = location;
        game._tarkovDateTime = dateTime;
        game.FixedDeltaTime = fixedDeltaTime;
        game.HandleLocationData(location, wavesSettings.BotAmount);
        if (!Singleton<GlobalEventDispatcher>.Instantiated)
        {
            Singleton<GlobalEventDispatcher>.Create(new GlobalEventDispatcher());
        }

        game.GameController = new HeadlessGameController(game, updateQueue, gameWorld, backEndSession, location, wavesSettings, gameTime)
        {
            Location = location
        };
        game.GameDateTime = gameTime;
        game._localRaidSettings = localRaidSettings;
        game.DoWeatherThings(timeAndWeather.IsRandomTime, timeAndWeather.IsRandomWeather);
        WorldInteractiveObject.InteractionShouldBeConfirmed = false;

        float hearingDistance = FikaGlobals.VOIPHandler.PushToTalkSettings.HearingDistance;
        game._voipDistance = (hearingDistance * hearingDistance) + 9;

        ClientHearingTable.Instance = game;

        if (game.GameController.IsServer)
        {
            gameWorld.World.RegisterNetworkInteractionObjects();
        }

        if (timeAndWeather.TimeFlowType != ETimeFlowType.x1)
        {
            var newFlow = timeAndWeather.TimeFlowType.ToTimeFlow();
            gameWorld.GameDateTime.TimeFactor = newFlow;
            game._logger.LogInfo($"Using custom time flow: {newFlow}");
        }

        if (FikaBackendUtils.CustomRaidSettings.UseCustomWeather && game.GameController.IsServer)
        {
            game._logger.LogInfo("Custom weather enabled, initializing curves");
            (game.GameController as HostGameController).SetupCustomWeather(timeAndWeather);
        }

        FikaGlobals.FikaGame = game;
        FikaEventDispatcher.DispatchEvent(new FikaGameCreatedEvent(game));

        game.GameController.RaidSettings = raidSettings;
        game.GameController.ThrownGrenades = [];
        game.gameObject.AddComponent<HeadlessRaidController>();

        return game;
    }

    private void HandleLocationData(LocationSettings.Location location, EBotAmount botAmount)
    {
        location.OldSpawn = location.OfflineOldSpawn;
        location.NewSpawn = location.OfflineNewSpawn;
        var num = 1f;
        switch (botAmount)
        {
            case EBotAmount.NoBots:
            case EBotAmount.Low:
                num = Singleton<GlobalConfiguration>.Instance != null ? Singleton<GlobalConfiguration>.Instance.WAVE_COEF_LOW : BotInternalSettingsController.Core.WAVE_COEF_LOW;
                break;
            case EBotAmount.Medium:
                num = Singleton<GlobalConfiguration>.Instance != null ? Singleton<GlobalConfiguration>.Instance.WAVE_COEF_MID : BotInternalSettingsController.Core.WAVE_COEF_MID;
                break;
            case EBotAmount.High:
                num = Singleton<GlobalConfiguration>.Instance != null ? Singleton<GlobalConfiguration>.Instance.WAVE_COEF_HIGH : BotInternalSettingsController.Core.WAVE_COEF_HIGH;
                break;
            case EBotAmount.Horde:
                num = Singleton<GlobalConfiguration>.Instance != null ? Singleton<GlobalConfiguration>.Instance.WAVE_COEF_HORDE : BotInternalSettingsController.Core.WAVE_COEF_HORDE;
                break;
        }

        location.BotMax = (int)(location.BotMax * num);
    }

    private void DoWeatherThings(bool isRandomTime, bool isRandomWeather)
    {
        System.Random random = new();
        if (isRandomTime)
        {
            _dateTime = new DateTime(2016, 4, 30, random.Next(1, 24), random.Next(1, 59), 0, DateTimeKind.Utc);
        }
        else if (!_factoryTimes.TryGetValue(_location.Id, out _dateTime))
        {
            var calculated = GameDateTime.Calculate().ToManaged();
            _dateTime = _tarkovDateTime == EDateTime.CURR ? calculated : calculated.AddHours(12.0);
        }
        GameDateTime = new GameDateTime(GameDateTime.StatedRealDateTime, _dateTime.ToIl2Cpp(), GameDateTime.TimeFactor, GameDateTime.Debug);
        GameWorld.GameDateTime = GameDateTime;
        if (WeatherController.Instance != null || MonoBehaviourSingleton<TODSkySimple>.Instance != null)
        {
            TODSkyProvider.Instance.CurrentTime.GameDateTime = GameDateTime;
            var randomTestWeatherNodes = WeatherNode.GetRandomTestWeatherNodes(600, 12);
            if (!isRandomWeather)
            {
                var time = randomTestWeatherNodes[0].Time;
                randomTestWeatherNodes[0] = BackendSession.Weather;
                randomTestWeatherNodes[0].Time = time;
            }
            if (WeatherController.Instance != null)
            {
                WeatherController.Instance.SetWeatherNodes(randomTestWeatherNodes);
            }
        }
    }

    public async Task Init(BotControllerSettings botsSettings)
    {
        _logger.LogInfo("Unloading unused resources");
        var unload = Resources.UnloadUnusedAssets();
        while (!unload.isDone)
        {
            await Task.Yield();
        }

        Status = GameStatus.Running;
        UnityEngine.Random.InitState((int)DateTimeExtensions.Now.Ticks);

        await GameController.SetupCoopHandler(this);
        var gameWorld = Singleton<GameWorld>.Instance;
        gameWorld.LocationId = _location.Id;
        ExfiltrationController.Instance.InitAllExfiltrationPoints(_location._Id, _location.exits, _location.SecretExits,
        !GameController.IsServer, _location.DisabledScavExits);

        _logger.LogInfo($"Location: {_location.Name}");
        var instance = Singleton<GlobalConfiguration>.Instance;

        GameController.InitWorldControllers(gameWorld);
        GameController.InitShellingController(instance, gameWorld, _location);
        GameController.InitHalloweenEvent(instance, gameWorld, _location);
        GameController.InitBTRController(instance, gameWorld, _location);

        if ((FikaBackendUtils.IsHeadless || FikaBackendUtils.IsHeadlessGame) && FikaPlugin.Instance.Settings.EnableTransits)
        {
            GameController.InitializeTransitSystem(gameWorld, instance, null, _localRaidSettings, _location);
        }

        GameController.InitializeRunddans(instance, gameWorld, _location);
        GameController.InitializePasscodes(gameWorld, _location);

        Singleton<FikaServer>.Instance.RaidInitialized = true;

        gameWorld.ClientBroadcastSyncController = new ClientBroadcastSyncController();

        var config = AppEnvironment.Config;
        if (config.FixedFrameRate > 0f)
        {
            FixedDeltaTime = 1f / config.FixedFrameRate;
        }

        GameController.CreateSpawnSystem(null);

        if (FikaGlobals.NetworkManager.AllowVOIP)
        {
            _logger.LogInfo("VOIP enabled, initializing...");
            try
            {
                await FikaGlobals.NetworkManager.InitializeVOIP();
            }
            catch (Exception ex)
            {
                _logger.LogError($"There was an error initializing the VOIP module: {ex.Message}");
            }
        }

        await GameController.WaitForHostToStart();

        var location = _localRaidSettings.selectedLocation;
        await GameController.InitializeLoot(location);
        await LoadLoot(location);

        GameController.CoopHandler.ShouldSync = true;
        await StartBotSystemsAndCountdown(botsSettings);

        Singleton<IBotGame>.Instance.BotsController.CoversData.Patrols.RestoreLoot(location.Loot,
            LocationScene.GetAllObjects<LootableContainer>(false));
        ServerAirdropManager airdropEventClass = new()
        {
            AirdropParameters = _location.airdropParameters
        };
        airdropEventClass.Init(true);
        Singleton<GameWorld>.Instance.TryCast<ClientGameWorld>().ClientSynchronizableObjectLogicProcessor.ServerAirdropManager = airdropEventClass;
        GameWorld.SynchronizableObjectLogicProcessor.AirdropDataSender = Singleton<FikaServer>.Instance;

        var timeBeforeDeployLocal = Singleton<GlobalConfiguration>.Instance.TimeBeforeDeployLocal;
#if DEBUG
        timeBeforeDeployLocal = 3;
#endif

        await RunMemoryCleanup();
        await (GameController as HeadlessGameController).WaitForHeadlessInit(timeBeforeDeployLocal);

        _logger.LogInfo("Headless client is ready");

        TaskCompletionSource taskCompletionSource = new();
        StartCoroutine(FinishRaidSetup(() => taskCompletionSource.SetResult(true)).ToIl2Cpp());
        await taskCompletionSource.Task;

        FikaBackendUtils.GroupPlayers.Clear();

        MonoBehaviourSingleton<EnvironmentUI>.Instance.ShowEnvironment(false);
        MonoBehaviourSingleton<PreloaderUI>.Instance.SetMenuTaskBarVisibility(false);

        FikaEventDispatcher.DispatchEvent(new FikaRaidStartedEvent(true));

        NetManagerUtils.DisableLoadingScreenUI();

        StartCoroutine(GameController.CreateStashes().ToIl2Cpp());

        if (GameController.CoopHandler.HumanPlayers.Count > 0)
        {
            var player = GameController.CoopHandler.HumanPlayers[0];
            var cameraTransform = CameraManager.Instance.Camera.transform;
            cameraTransform.SetParent(player.gameObject.transform, false);
            cameraTransform.localPosition = new(0f, 1.7f, 0f);
            cameraTransform.rotation = Quaternion.identity;
        }

        StartCoroutine((GameController as HeadlessGameController).SyncTraps().ToIl2Cpp());
    }

    private Task RunMemoryCleanup()
    {
        _logger.LogInfo("Running memory cleanup and asset unloading");

        InGameMemoryManagement.RunHeapPreAllocation();
        InGameMemoryManagement.Collect(true);
        InGameMemoryManagement.EmptyWorkingSet();

        InGameMemoryManagement.GCEnabled = false;
        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
        InitializeCameraAndUnloadAssets();

        return Task.CompletedTask;
    }

    private void InitializeCameraAndUnloadAssets()
    {
        CameraManager.Instance.SetCameraFromSettings(Singleton<LevelSettings>.Instance);
        CameraManager.Instance.IsActive = true;

        var instance = PerfectCullingAdaptiveGrid.Instance;
        if (instance != null)
        {
            GameObject.Destroy(instance);
        }

        var cameraInstance = PerfectCullingCrossSceneSampler.Instance;
        if (cameraInstance != null)
        {
            var pCCS = cameraInstance.GetComponent<PerfectCullingCrossSceneSampler>();
            if (pCCS != null)
            {
                GameObject.Destroy(pCCS);
            }

            var pCC = cameraInstance.GetComponent<PerfectCullingCamera>();
            if (pCC != null)
            {
                GameObject.Destroy(pCC);
            }
        }

        if (Singleton<SpatialAudioSystem>.Instantiated)
        {
            var spatialAudioSystem = Singleton<SpatialAudioSystem>.Instance;
            var audioRoomStorage = spatialAudioSystem._audioRoomStorage;
            if (audioRoomStorage != null)
            {
                _logger.LogInfo($"SpatialAudio: Destroying {audioRoomStorage._orderedConnections.Count} rooms");
                foreach (var connection in audioRoomStorage._orderedConnections)
                {
                    foreach (var rooms in connection.Value)
                    {
                        foreach (var portal in rooms.GetPortals())
                        {
                            GameObject.Destroy(portal.Cast<MonoBehaviour>());
                        }
                    }

                    if (connection.Key != null)
                    {
                        GameObject.Destroy(connection.Key.Cast<MonoBehaviour>());
                    }
                }
            }
            // This calls the Dispose() method
            GameObject.Destroy(spatialAudioSystem);
        }
    }

    private IEnumerator FinishRaidSetup(Action complete)
    {
        yield return GameController.FinishRaidSetup();
        yield return FinishHeadlessRaidSetup(complete);
    }

    private IEnumerator FinishHeadlessRaidSetup(Action complete)
    {
        yield return new WaitForSeconds(Singleton<GlobalConfiguration>.Instance.TimeBeforeDeployLocal);
        (GameController as HeadlessGameController).ActivateBots();
        GameController.SetupEventsAndExfils(null);
        complete?.Invoke();
    }

    private async Task StartBotSystemsAndCountdown(BotControllerSettings botsSettings)
    {
        await GameController.StartBotSystemsAndCountdown(botsSettings, GameWorld);
    }

    private async Task LoadLoot(LocationSettings.Location location)
    {
        if (AppEnvironment.Config.NoLootForLocalGame)
        {
            foreach (var lootItemPositionClass in location.Loot
                .Where(new Func<JsonLootItem, bool>(IsLootItemContainer))
                .ToList()
                )
            {
                var lootContainerItemClass = lootItemPositionClass.Item.TryCast<LootContainer>();
                var grids = lootContainerItemClass.Grids;
                for (var i = 0; i < grids.Length; i++)
                {
                    grids[i].RemoveAll();
                }
                var slots = lootContainerItemClass.Slots;
                for (var i = 0; i < slots.Length; i++)
                {
                    slots[i].RemoveItem(false);
                }
            }
        }

        Item[] array = [.. location.Loot.Select(ItemFromPositionClass)];
        ResourceKey[] array2 = [.. array.OfType<ContainerCollection>().ToIl2CppList().GetAllItemsFromCollections()
            .Concat(array
                .Where(IsItemSpecialContainer)
            )
            .SelectMany(GetResourceKeys)];
        if (array2.Length != 0)
        {
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            var streamingEntry = FindPlayerLoopEntry(playerLoop, "UpdateTextureStreamingManager", out var streamingIndex);
            var streamingUpdate = IntPtr.Zero;
            if (streamingEntry != null)
            {
                streamingUpdate = SetPlayerLoopUpdate(streamingEntry, streamingIndex, IntPtr.Zero);
                PlayerLoop.SetPlayerLoop(playerLoop);
            }

            LoadingScreenUI.Instance.UpdateAndBroadcast(50f);
            try
            {
                await Singleton<ObjectsFactory>.Instance.LoadBundlesAndCreatePools(ObjectsFactory.PoolsCategory.Raid,
                    ObjectsFactory.AssemblyType.Local, array2.ToIl2CppList(), JobYieldPriority.General,
                    null, new Il2CppSystem.Threading.CancellationToken());
            }
            finally
            {
                if (streamingEntry != null)
                {
                    SetPlayerLoopUpdate(streamingEntry, streamingIndex, streamingUpdate);
                    PlayerLoop.SetPlayerLoop(playerLoop);
                }
            }
            LoadingScreenUI.Instance.UpdateAndBroadcast(75f);
        }
        var questLoot = GameWorld.GetQuestLootReady(location.Loot);
        GameWorld.SpawnLoot(questLoot, true);
    }

    private static Il2CppReferenceArray<PlayerLoopSystem> FindPlayerLoopEntry(PlayerLoopSystem root, string typeName, out int index)
    {
        index = -1;
        foreach (var system in root.subSystemList ?? new Il2CppReferenceArray<PlayerLoopSystem>(0))
        {
            var entries = system.subSystemList;
            if (entries == null)
            {
                continue;
            }

            for (var i = 0; i < entries.Length; i++)
            {
                if (entries[i].type?.Name == typeName)
                {
                    index = i;
                    return entries;
                }
            }
        }

        return null;
    }

    private static IntPtr SetPlayerLoopUpdate(Il2CppReferenceArray<PlayerLoopSystem> entries, int index, IntPtr update)
    {
        var entry = entries[index];
        var previous = entry.updateFunction;
        entry.updateFunction = update;
        entries[index] = entry;
        return previous;
    }

    private IEnumerable<ResourceKey> GetResourceKeys(Item item)
    {
        return item.Template.AllResources;
    }

    private bool IsItemSpecialContainer(Item item)
    {
        return item.TryCast<ContainerCollection>() == null;
    }

    public bool IsLootItemContainer(JsonLootItem x)
    {
        return x.Item.TryCast<LootContainer>() != null;
    }

    public Item ItemFromPositionClass(JsonLootItem x)
    {
        return x.Item;
    }

    public bool IsHeard()
    {
        return false;
    }

    public void ReportAbuse()
    {
        // Do nothing
    }

    public void Stop(string profileId, ExitStatus exitStatus, string exitName, float delay = 0)
    {
        FikaEventDispatcher.DispatchEvent(new FikaGameEndedEvent(GameController.IsServer, exitStatus, exitName));

        if (exitStatus < ExitStatus.Transit)
        {
            FikaBackendUtils.IsTransit = false;
        }

        if (FikaBackendUtils.IsTransit)
        {
            var data = FikaBackendUtils.TransitData;
            data.transitionType = ELocationTransition.Common;
            data.transitionCount++;
            data.visitedLocations = new Il2CppStringArray([.. data.visitedLocations ?? (IEnumerable<string>)[], GameController.Location.Id]);
            FikaBackendUtils.TransitData = data;
        }
        else
        {
            FikaBackendUtils.ResetTransitData();
        }

        _logger.LogDebug("Stop");

        GameController.DestroyDebugComponent();

        (GameController as HostGameController).StopBotsSystem(false);

        if (GameController.CoopHandler != null)
        {
            // Create a copy to prevent errors when the dictionary is being modified (which happens when using spawn mods)
            FikaPlayer[] players = [.. GameController.CoopHandler.Players.Values];
            foreach (var player in players)
            {
                if (player == null)
                {
                    continue;
                }

                try
                {
                    player.Dispose();
                    AssetPoolObject.ReturnToPool(player.gameObject, true);
                }
                catch (Exception ex)
                {
                    FikaHeadlessPlugin.PlayerDisposeError = true;
                    _logger.LogError($"Stop: There was an error while disposing player: {ex}");
                }
            }
        }
        else
        {
            _logger.LogError("Stop: Could not find CoopHandler!");
        }

        if (!FikaBackendUtils.IsTransit)
        {
            Destroy(GameController.CoopHandler);
        }

        var gameUI = GameUI.Instance;

        Status = GameStatus.Stopping;
        GameTimer.TryStop();
        if (gameUI.TimerPanel.isActiveAndEnabled)
        {
            gameUI.TimerPanel.Close();
        }
        if (EnvironmentManager.Instance != null)
        {
            EnvironmentManager.Instance.Stop();
        }
        AppEnvironment.Config.UseSpiritPlayer = false;

        EftScreenManager.Instance.CloseAllScreensForced();

        CleanUp();

        if (!FikaBackendUtils.IsTransit)
        {
            PlayerLeftRequest body = new(FikaBackendUtils.Profile.ProfileId);
            FikaRequestHandler.RaidLeave(body);
        }

        _exitCallback.Invoke(new Result<ExitStatus, Il2CppSystem.TimeSpan, ClientMetrics>(exitStatus, new Il2CppSystem.TimeSpan(), null));
        UIEventSystem.Instance.Enable();
    }

    private void CleanUp()
    {
        GameController.CleanUp();
        FikaBackendUtils.CleanUpVariables();
        BTRSide_Patches.Passengers.Clear();
    }

    public override void FixedUpdate()
    {
        // Do nothing
    }
}
