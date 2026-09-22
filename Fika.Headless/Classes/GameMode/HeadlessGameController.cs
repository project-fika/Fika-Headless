using Comfort.Common;
using CommonAssets.Scripts.Game;
using CommonAssets.Scripts.Game.LabyrinthEvent;
using EFT;
using EFT.Game.Spawning;
using EFT.GlobalEvents;
using EFT.Interactive;
using EFT.UI;
using Fika.Core.Main.GameMode;
using Fika.Core.Main.Utils;
using Fika.Core.Networking;
using Fika.Core.Networking.Packets.Backend;
using Fika.Core.Networking.Packets.Communication;
using JsonType;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fika.Headless.Classes.GameMode;

internal class HeadlessGameController : HostGameController
{
    public HeadlessGameController(IntPtr pointer) : base(pointer)
    {
    }

    public HeadlessGameController(IFikaGame game, EUpdateQueue updateQueue, GameWorld gameWorld, IEftSession session,
        LocationSettings.Location location, WavesSettings wavesSettings, GameDateTime gameDateTime)
        : base(Il2CppInjection.Allocate<HeadlessGameController>(), game, updateQueue, gameWorld, session, location, wavesSettings, gameDateTime)
    {
    }

    public override void SetupEventsAndExfils(Player player)
    {
        Logger.LogInfo("[SERVER] SpawnPoint: " + _spawnPoint.Id + ", InfiltrationPoint: " + InfiltrationPoint);
        _abstractGame.GameTimer.Start(GameTime.ToIl2Cpp(), SessionTime.ToIl2Cpp());

        /*ExfiltrationController exfilController = ExfiltrationController.Instance;*/

        /*ExfiltrationPoint[] exfilPoints = exfilController.EligiblePoints(string.Empty);
        SecretExfiltrationPoint[] secretExfilPoints = [.. exfilController.SecretEligiblePoints(), .. exfilController.GetScavSecretExits()];*/

        if (TransitController.Exist(out FikaHeadlessTransitController transitController))
        {
            transitController.Init();
            foreach (var activePlayer in CoopHandler.HumanPlayers)
            {
                var initEvent = new TransitInitEvent
                {
                    PlayerRaidId = activePlayer.RaidId,
                    Points = ActiveTransitPoints(transitController),
                    CompletedQuestRequirementMetByPointId = CompletedQuestRequirements(activePlayer),
                    TransitionCount = (ushort)transitController.LocalRaidSettings.transition.transitionCount,
                    EventPlayer = transitController.IsEvent
                };

                var writer = NetworkUtils.EventDataWriter;
                writer.Reset();
                initEvent.Serialize(ref writer);
                writer.Flush();

                var syncPacket = new SyncEventPacket
                {
                    Type = 0,
                    Data = new byte[writer.BytesWritten]
                };
                Array.Copy(writer.Buffer, syncPacket.Data, writer.BytesWritten);
                _server.SendData(ref syncPacket, DeliveryMethod.ReliableOrdered);

                var updateEvent = new TransitUpdateEvent
                {
                    PlayerRaidId = activePlayer.RaidId,
                    EventOnly = transitController.IsEvent,
                    Points = ActiveTransitPoints(transitController)
                };

                writer.Reset();
                updateEvent.Serialize(ref writer);
                writer.Flush();

                syncPacket.Type = 1;
                Array.Copy(writer.Buffer, syncPacket.Data, writer.BytesWritten);
                _server.SendData(ref syncPacket, DeliveryMethod.ReliableOrdered);
            }
        }

        if (Location.EventTrapsData != null)
        {
            LabyrinthSyncableTraps.InitLabyrinthSyncableTraps(Location.EventTrapsData);
            _gameWorld.SyncModule = new();
        }
        _abstractGame.Status = GameStatus.Started;

        ConsoleScreen.ApplyStartCommands();
    }

    public void ActivateBots()
    {
        _botsController.Bots.CheckActivation();
    }

    public override void CreateSpawnSystem(Profile profile)
    {
        _spawnPoints = SpawnPointsCollection.CreateFromScene(new Il2CppSystem.Nullable<Il2CppSystem.DateTime>(DateTimeExtensions.LocalDateTimeFromUnixTime(Location.UnixDateTime)),
                                Location.SpawnPointParams);
        var spawnSafeDistance = (Location.SpawnSafeDistanceMeters > 0) ? Location.SpawnSafeDistanceMeters : 100;
        SpawnSystemSettings settings = new(Location.MinDistToFreePoint,
            Location.MaxDistToFreePoint, Location.MaxBotPerZone, spawnSafeDistance,
            Location.NoGroupSpawn, Location.OneTimeSpawn);
        SpawnSystem = SpawnSystemFactory.CreateSpawnSystem(settings, new System.Func<float>(FikaGlobals.GetApplicationTime), Singleton<GameWorld>.Instance, _botsController, _spawnPoints);

        var side = FikaGlobals.NetworkManager.RaidSide == ESideType.Pmc ? EPlayerSide.Usec : EPlayerSide.Savage;

        _spawnPoint = SpawnSystem.SelectSpawnPoint(ESpawnCategory.Player, side,
            null, null, null, null, null);
        InfiltrationPoint = string.IsNullOrEmpty(_spawnPoint.Infiltration) ? "MissingInfiltration" : _spawnPoint.Infiltration;
    }

    public Task WaitForHeadlessInit(float timeBeforeDeployLocal)
    {
        if (_fikaGame is not AbstractGame abstractGame)
        {
            throw new NullReferenceException("AbstractGame was missing");
        }

        var server = Singleton<FikaServer>.Instance;
        server.HostReady = true;

        var startTime = DateTimeExtensions.UtcNow.AddSeconds((double)timeBeforeDeployLocal);
        GameTime = startTime.ToManaged();
        server.GameStartTime = startTime.ToManaged();
        SessionTime = abstractGame.GameTimer.SessionTime.ToManaged();

        InformationPacket packet = new()
        {
            RaidStarted = RaidStarted,
            ReadyPlayers = server.ReadyClients,
            HostReady = server.HostReady,
            GameTime = GameTime.Value,
            SessionTime = SessionTime.Value,
            GameDateTime = GameDateTime
        };

        server.SendData(ref packet, DeliveryMethod.ReliableOrdered);
        LootData = null;

        return Task.CompletedTask;
    }

    public override void InitializeTransitSystem(GameWorld gameWorld, GlobalConfiguration instance, Profile profile, LocalRaidSettings localRaidSettings, LocationSettings.Location location)
    {
        bool transitActive;
        if (instance == null)
        {
            transitActive = false;
        }
        else
        {
            var transitSettings = instance.transitSettings;
            transitActive = transitSettings != null && transitSettings.active;
        }
        if (transitActive)
        {
            gameWorld.TransitController = new FikaHeadlessTransitController(instance.transitSettings, location.transitParameters, localRaidSettings);
        }
        else
        {
            Logger.LogInfo("Transits are disabled");
            TransitController.DisableTransitPoints();
        }
    }

    private Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<LocationSettings.Location.TransitParameters> TransitParameters()
    {
        return Location.transitParameters ?? new(0);
    }

    private Il2CppSystem.Collections.Generic.Dictionary<int, LocationSettings.Location.TransitParameters> ActiveTransitPoints(EFT.TransitController transitController)
    {
        return TransitParameters().Where(x => x.active && transitController.pointsById.ContainsKey(x.id)).ToDictionary(k => k.id).ToIl2CppDictionary();
    }

    private Il2CppSystem.Collections.Generic.Dictionary<int, bool> CompletedQuestRequirements(Player player)
    {
        var result = new Il2CppSystem.Collections.Generic.Dictionary<int, bool>();
        foreach (var parameters in TransitParameters())
        {
            if (parameters.active && !string.IsNullOrEmpty(parameters.completedQuestId))
            {
                result[parameters.id] = new TransitCompletedQuestRequirement(parameters.completedQuestId).Met(player);
            }
        }

        return result;
    }
}
