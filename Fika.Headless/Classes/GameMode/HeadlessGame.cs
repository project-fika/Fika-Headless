using Audio.SpatialSystem;
using BepInEx.Logging;
using Comfort.Common;
using Dissonance.Networking.Client;
using EFT;
using EFT.AssetsManager;
using EFT.Bots;
using EFT.EnvironmentEffect;
using EFT.Interactive;
using EFT.InventoryLogic;
using EFT.UI;
using EFT.Weather;
using Fika.Core;
using Fika.Core.Coop.GameMode;
using Fika.Core.Coop.Patches;
using Fika.Core.Coop.Players;
using Fika.Core.Coop.Utils;
using Fika.Core.Modding;
using Fika.Core.Modding.Events;
using Fika.Core.Networking;
using Fika.Core.Networking.Http;
using HarmonyLib;
using JsonType;
using Koenigz.PerfectCulling;
using Koenigz.PerfectCulling.EFT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace Fika.Headless.Classes.GameMode
{
    public class HeadlessGame : AbstractGame, IFikaGame, IClientHearingTable
    {
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

        public SeasonsSettingsClass SeasonsSettings
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

        public ISession BackendSession { get; set; }

        public BaseGameController GameController { get; set; }
        public GameDateTime GameDateTime { get; private set; }
        public GameWorld GameWorld { get; private set; }

        private LocalRaidSettings _localRaidSettings;
        private Callback<ExitStatus, TimeSpan, MetricsClass> _exitCallback;
        private LocationSettingsClass.Location _location;
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
            LocationSettingsClass.Location location, TimeAndWeatherSettings timeAndWeather, WavesSettings wavesSettings,
            EDateTime dateTime, Callback<ExitStatus, TimeSpan, MetricsClass> callback, float fixedDeltaTime,
            EUpdateQueue updateQueue, ISession backEndSession, TimeSpan sessionTime, LocalRaidSettings localRaidSettings,
            RaidSettings raidSettings)
        {
            Singleton<IFikaNetworkManager>.Instance.RaidSide = localRaidSettings.playerSide;

            HeadlessGame game = Create<HeadlessGame>(updateQueue, sessionTime);
            game._logger = BepInEx.Logging.Logger.CreateLogSource(nameof(HeadlessGame));
            game.GameWorld = gameWorld;

            float num = 1.5f;
            foreach (WildSpawnWave wildSpawnWave in location.waves)
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
            if (!Singleton<BotEventHandler>.Instantiated)
            {
                Singleton<BotEventHandler>.Create(new BotEventHandler());
            }

            HeadlessGameController headlessGameController = new(game, updateQueue, gameWorld, backEndSession, location, wavesSettings, backendDateTime)
            {
                Location = location
            };
            game.GameDateTime = backendDateTime;
            game.GameController = headlessGameController;
            game._localRaidSettings = localRaidSettings;
            game.DoWeatherThings(timeAndWeather.IsRandomTime, timeAndWeather.IsRandomWeather);
            WorldInteractiveObject.InteractionShouldBeConfirmed = false;

            float hearingDistance = FikaGlobals.VOIPHandler.PushToTalkSettings.HearingDistance;
            game._voipDistance = hearingDistance * hearingDistance + 9;

            ClientHearingTable.Instance = game;

            if (game.GameController.IsServer)
            {
                gameWorld.World_0.method_0();
            }

            if (timeAndWeather.TimeFlowType != ETimeFlowType.x1)
            {
                float newFlow = timeAndWeather.TimeFlowType.ToTimeFlow();
                gameWorld.GameDateTime.TimeFactor = newFlow;
                game._logger.LogInfo($"Using custom time flow: {newFlow}");
            }

            if (OfflineRaidSettingsMenuPatch_Override.UseCustomWeather && game.GameController.IsServer)
            {
                game._logger.LogInfo("Custom weather enabled, initializing curves");
                (game.GameController as HostGameController).SetupCustomWeather(timeAndWeather);
            }

            Singleton<IFikaGame>.Create(game);
            FikaEventDispatcher.DispatchEvent(new FikaGameCreatedEvent(game));

            game.GameController.RaidSettings = raidSettings;
            game.GameController.ThrownGrenades = [];
            game.gameObject.AddComponent<HeadlessRaidController>();

            return game;
        }

        private void HandleLocationData(LocationSettingsClass.Location location, EBotAmount botAmount)
        {
            location.OldSpawn = location.OfflineOldSpawn;
            location.NewSpawn = location.OfflineNewSpawn;
            float num = 1f;
            switch (botAmount)
            {
                case EBotAmount.NoBots:
                case EBotAmount.Low:
                    num = Singleton<BackendConfigSettingsClass>.Instance != null ? Singleton<BackendConfigSettingsClass>.Instance.WAVE_COEF_LOW : LocalBotSettingsProviderClass.Core.WAVE_COEF_LOW;
                    break;
                case EBotAmount.Medium:
                    num = Singleton<BackendConfigSettingsClass>.Instance != null ? Singleton<BackendConfigSettingsClass>.Instance.WAVE_COEF_MID : LocalBotSettingsProviderClass.Core.WAVE_COEF_MID;
                    break;
                case EBotAmount.High:
                    num = Singleton<BackendConfigSettingsClass>.Instance != null ? Singleton<BackendConfigSettingsClass>.Instance.WAVE_COEF_HIGH : LocalBotSettingsProviderClass.Core.WAVE_COEF_HIGH;
                    break;
                case EBotAmount.Horde:
                    num = Singleton<BackendConfigSettingsClass>.Instance != null ? Singleton<BackendConfigSettingsClass>.Instance.WAVE_COEF_HORDE : LocalBotSettingsProviderClass.Core.WAVE_COEF_HORDE;
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
                _dateTime = _tarkovDateTime == EDateTime.CURR ? GameDateTime.Calculate() : GameDateTime.Calculate().AddHours(12.0);
            }
            GameDateTime = new GameDateTime(GameDateTime.DateTime_0, _dateTime, GameDateTime.TimeFactor, GameDateTime.Boolean_0);
            GameWorld.GameDateTime = GameDateTime;
            if (WeatherController.Instance != null || MonoBehaviourSingleton<TODSkySimple>.Instance != null)
            {
                GClass4.Instance.CurrentTime.GameDateTime = GameDateTime;
                WeatherClass[] randomTestWeatherNodes = WeatherClass.GetRandomTestWeatherNodes(600, 12);
                if (!isRandomWeather)
                {
                    long time = randomTestWeatherNodes[0].Time;
                    randomTestWeatherNodes[0] = BackendSession.Weather;
                    randomTestWeatherNodes[0].Time = time;
                }
                if (WeatherController.Instance != null)
                {
                    WeatherController.Instance.method_0(randomTestWeatherNodes);
                }
            }
        }

        public async Task Init(BotControllerSettings botsSettings, string backendUrl)
        {
            _logger.LogInfo("Unloading unused resources");
            await Resources.UnloadUnusedAssets().Await();

            Status = GameStatus.Running;
            UnityEngine.Random.InitState((int)EFTDateTimeClass.Now.Ticks);

            await GameController.SetupCoopHandler(this);
            GameWorld gameWorld = Singleton<GameWorld>.Instance;
            gameWorld.LocationId = _location.Id;
            ExfiltrationControllerClass.Instance.InitAllExfiltrationPoints(_location._Id, _location.exits, _location.SecretExits,
            !GameController.IsServer, _location.DisabledScavExits);

            _logger.LogInfo($"Location: {_location.Name}");
            BackendConfigSettingsClass instance = Singleton<BackendConfigSettingsClass>.Instance;

            GameController.InitShellingController(instance, gameWorld, _location);
            GameController.InitHalloweenEvent(instance, gameWorld, _location);
            GameController.InitBTRController(instance, gameWorld, _location);

            if ((FikaBackendUtils.IsHeadless || FikaBackendUtils.IsHeadlessGame) && FikaPlugin.Instance.EnableTransits)
            {
                GameController.InitializeTransitSystem(gameWorld, instance, null, _localRaidSettings, _location);
            }

            GameController.InitializeRunddans(instance, gameWorld, _location);

            gameWorld.ClientBroadcastSyncController = new ClientBroadcastSyncControllerClass();

            ApplicationConfigClass config = BackendConfigAbstractClass.Config;
            if (config.FixedFrameRate > 0f)
            {
                FixedDeltaTime = 1f / config.FixedFrameRate;
            }

            GameController.CreateSpawnSystem(null);

            await GameController.WaitForHostToStart();

            LocationSettingsClass.Location location = _localRaidSettings.selectedLocation;
            await GameController.InitializeLoot(location);
            await LoadLoot(location);

            GameController.CoopHandler.ShouldSync = true;
            await StartBotSystemsAndCountdown(botsSettings);

            Singleton<IBotGame>.Instance.BotsController.CoversData.Patrols.RestoreLoot(location.Loot, LocationScene.GetAllObjects<LootableContainer>(false));
            AirdropEventClass airdropEventClass = new()
            {
                AirdropParameters = _location.airdropParameters
            };
            airdropEventClass.Init(true);
            (Singleton<GameWorld>.Instance as ClientGameWorld).ClientSynchronizableObjectLogicProcessor.ServerAirdropManager = airdropEventClass;
            GameWorld.SynchronizableObjectLogicProcessor.Ginterface262_0 = Singleton<FikaServer>.Instance;

            await RunMemoryCleanup();

            int timeBeforeDeployLocal = FikaBackendUtils.IsReconnect ? 3 : Singleton<BackendConfigSettingsClass>.Instance.TimeBeforeDeployLocal;
#if DEBUG
            timeBeforeDeployLocal = 3;
#endif
            await (GameController as HeadlessGameController).WaitForHeadlessInit(timeBeforeDeployLocal);

            FikaBackendUtils.GroupPlayers.Clear();

            Singleton<SharedGameSettingsClass>.Instance.Graphics.Controller.ChangeFramerate(true);
            MonoBehaviourSingleton<EnvironmentUI>.Instance.ShowEnvironment(false);
            MonoBehaviourSingleton<PreloaderUI>.Instance.SetMenuTaskBarVisibility(false);

            FikaEventDispatcher.DispatchEvent(new FikaRaidStartedEvent(true));

            _ = Task.Run(GameController.CreateStashes);

            if (GameController.CoopHandler.HumanPlayers.Count > 0)
            {
                CoopPlayer player = GameController.CoopHandler.HumanPlayers[0];
                Transform cameraTransform = CameraClass.Instance.Camera.transform;
                cameraTransform.SetParent(player.gameObject.transform, false);
                cameraTransform.localPosition = new(0f, 1.7f, 0f);
                cameraTransform.rotation = Quaternion.identity;
            }

            (GameController as HeadlessGameController).SyncTraps();
        }

        private Task RunMemoryCleanup()
        {
            _logger.LogInfo("Running memory cleanup and asset unloading");

            MemoryControllerClass.RunHeapPreAllocation();
            MemoryControllerClass.Collect(true);
            MemoryControllerClass.EmptyWorkingSet();

            MemoryControllerClass.GCEnabled = false;
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            InitializeCameraAndUnloadAssets();
            TaskCompletionSource taskCompletionSource = new();
            StartCoroutine(FinishRaidSetup(taskCompletionSource.Complete));
            return taskCompletionSource.Task;
        }

        private void InitializeCameraAndUnloadAssets()
        {
            CameraClass.Instance.SetCameraFromSettings(Singleton<LevelSettings>.Instance);
            CameraClass.Instance.IsActive = true;

            PerfectCullingAdaptiveGrid instance = PerfectCullingAdaptiveGrid.Instance;
            if (instance != null)
            {
                GameObject.Destroy(instance);
            }

            PerfectCullingCrossSceneSampler cameraInstance = PerfectCullingCrossSceneSampler.Instance;
            if (cameraInstance != null)
            {
                PerfectCullingCrossSceneSampler pCCS = cameraInstance.GetComponent<PerfectCullingCrossSceneSampler>();
                if (pCCS != null)
                {
                    GameObject.Destroy(pCCS);
                }

                PerfectCullingCamera pCC = cameraInstance.GetComponent<PerfectCullingCamera>();
                if (pCC != null)
                {
                    GameObject.Destroy(pCC);
                }
            }

            if (Singleton<SpatialAudioSystem>.Instantiated)
            {
                SpatialAudioSystem sAS = Singleton<SpatialAudioSystem>.Instance;
                GClass1118 sASManager = Traverse.Create(sAS).Field<GClass1118>("gclass1118_0").Value;
                if (sASManager != null)
                {
                    _logger.LogInfo($"SpatialAudio: Destroying {sASManager.Dictionary_0.Count} rooms");
                    foreach ((ISpatialAudioRoom room, List<ISpatialAudioRoom> roomList) in sASManager.Dictionary_0)
                    {
                        foreach (ISpatialAudioRoom rooms in roomList)
                        {
                            List<ISpatialPortal> portals = rooms.GetPortals();
                            foreach (ISpatialPortal portal in portals)
                            {
                                GameObject.Destroy((MonoBehaviour)portal);
                            }
                        }

                        if (room != null)
                        {
                            GameObject.Destroy((MonoBehaviour)room);
                        }
                    }
                }
                // This calls the Dispose() method
                GameObject.Destroy(sAS);
            }
        }

        private IEnumerator FinishRaidSetup(Action complete)
        {
            yield return GameController.FinishRaidSetup();
            yield return FinishHeadlessRaidSetup(complete);
        }

        private IEnumerator FinishHeadlessRaidSetup(Action complete)
        {
            yield return new WaitForSeconds(Singleton<BackendConfigSettingsClass>.Instance.TimeBeforeDeployLocal);
            (GameController as HeadlessGameController).ActivateBots();
            GameController.SetupEventsAndExfils(null);
            complete?.Invoke();
        }

        private async Task StartBotSystemsAndCountdown(BotControllerSettings botsSettings)
        {
            await GameController.StartBotSystemsAndCountdown(botsSettings, GameWorld);
        }

        private async Task LoadLoot(LocationSettingsClass.Location location)
        {
            if (BackendConfigAbstractClass.Config.NoLootForLocalGame)
            {
                foreach (LootItemPositionClass lootItemPositionClass in location.Loot
                    .Where(new Func<LootItemPositionClass, bool>(IsLootItemContainer))
                    .ToList()
                    )
                {
                    LootContainerItemClass lootContainerItemClass = lootItemPositionClass.Item as LootContainerItemClass;
                    StashGridClass[] grids = lootContainerItemClass.Grids;
                    for (int i = 0; i < grids.Length; i++)
                    {
                        grids[i].RemoveAll();
                    }
                    Slot[] slots = lootContainerItemClass.Slots;
                    for (int i = 0; i < slots.Length; i++)
                    {
                        slots[i].RemoveItem(false);
                    }
                }
            }

            Item[] array = [.. location.Loot.Select(ItemFromPositionClass)];
            ResourceKey[] array2 = [.. array.OfType<GClass3118>().GetAllItemsFromCollections()
                .Concat(array
                    .Where((IsItemSpecialContainer))
                )
                .SelectMany(GetResourceKeys)];
            if (array2.Length != 0)
            {
                PlayerLoopSystem playerLoopSystem = PlayerLoop.GetCurrentPlayerLoop();
                GClass657.FindParentPlayerLoopSystem(playerLoopSystem, typeof(EarlyUpdate.UpdateTextureStreamingManager), out PlayerLoopSystem playerLoopSystem2, out int num);
                PlayerLoopSystem[] array3 = new PlayerLoopSystem[playerLoopSystem2.subSystemList.Length];
                if (num != -1)
                {
                    Array.Copy(playerLoopSystem2.subSystemList, array3, playerLoopSystem2.subSystemList.Length);
                    PlayerLoopSystem playerLoopSystem3 = new()
                    {
                        updateDelegate = new PlayerLoopSystem.UpdateFunction(StaticUpdateFunction),
                        type = typeof(UpdateType)
                    };
                    playerLoopSystem2.subSystemList[num] = playerLoopSystem3;
                    PlayerLoop.SetPlayerLoop(playerLoopSystem);
                }
                await Singleton<PoolManagerClass>.Instance.LoadBundlesAndCreatePools(PoolManagerClass.PoolsCategory.Raid,
                    PoolManagerClass.AssemblyType.Local, array2, JobPriorityClass.General,
                    new GClass3858<LoadingProgressStruct>(HandleProgress, default),
                    default);
                if (num != -1)
                {
                    Array.Copy(array3, playerLoopSystem2.subSystemList, playerLoopSystem2.subSystemList.Length);
                    PlayerLoop.SetPlayerLoop(playerLoopSystem);
                }
                playerLoopSystem = default;
                playerLoopSystem2 = default;
                array3 = null;
            }
            GClass1398 gclass = GameWorld.method_4(location.Loot);
            GameWorld.method_5(gclass, true);
        }

        private void HandleProgress(LoadingProgressStruct progress)
        {
            // Do nothing
        }

        private void StaticUpdateFunction()
        {

        }

        private class UpdateType()
        {

        }

        private IEnumerable<ResourceKey> GetResourceKeys(Item item)
        {
            return item.Template.AllResources;
        }

        private bool IsItemSpecialContainer(Item item)
        {
            return item is not GClass3118;
        }

        public bool IsLootItemContainer(LootItemPositionClass x)
        {
            return x.Item is LootContainerItemClass;
        }

        public Item ItemFromPositionClass(LootItemPositionClass x)
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
                RaidTransitionInfoClass data = FikaBackendUtils.TransitData;
                data.transitionType = ELocationTransition.Common;
                data.transitionCount++;
                data.visitedLocations = [.. data.visitedLocations, GameController.Location.Id];
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
                CoopPlayer[] players = [.. GameController.CoopHandler.Players.Values];
                foreach (CoopPlayer player in players)
                {
                    if (player == null)
                    {
                        continue;
                    }

                    player.Dispose();
                    AssetPoolObject.ReturnToPool(player.gameObject, true);
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

            GameUI gameUI = GameUI.Instance;

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
            BackendConfigAbstractClass.Config.UseSpiritPlayer = false;

            CurrentScreenSingletonClass.Instance.CloseAllScreensForced();

            CleanUp();

            if (!FikaBackendUtils.IsTransit)
            {
                PlayerLeftRequest body = new(FikaBackendUtils.Profile.ProfileId);
                FikaRequestHandler.RaidLeave(body);
            }

            _exitCallback(new(exitStatus, new(), null));
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
}
