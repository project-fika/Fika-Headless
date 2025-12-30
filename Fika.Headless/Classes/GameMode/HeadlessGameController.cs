using System;
using System.Linq;
using System.Threading.Tasks;
using Comfort.Common;
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

namespace Fika.Headless.Classes.GameMode;

internal class HeadlessGameController(IFikaGame game, EUpdateQueue updateQueue, GameWorld gameWorld, ISession session,
    LocationSettingsClass.Location location, WavesSettings wavesSettings, GameDateTime gameDateTime)
    : HostGameController(game, updateQueue, gameWorld, session, location, wavesSettings, gameDateTime)
{
    public override void SetupEventsAndExfils(Player player)
    {
        Logger.LogInfo("[SERVER] SpawnPoint: " + _spawnPoint.Id + ", InfiltrationPoint: " + InfiltrationPoint);
        _abstractGame.GameTimer.Start();

        /*ExfiltrationControllerClass exfilController = ExfiltrationControllerClass.Instance;*/

        /*ExfiltrationPoint[] exfilPoints = exfilController.EligiblePoints(string.Empty);
        SecretExfiltrationPoint[] secretExfilPoints = [.. exfilController.SecretEligiblePoints(), .. exfilController.GetScavSecretExits()];*/

        if (TransitControllerAbstractClass.Exist(out FikaHeadlessTransitController transitController))
        {
            transitController.Init();
            foreach (var activePlayer in CoopHandler.HumanPlayers)
            {
                var initEvent = new TransitInitEvent
                {
                    PlayerId = activePlayer.Id,
                    Points = Location.transitParameters.Where(x => x.active).ToDictionary(k => k.id),
                    TransitionCount = (ushort)transitController.LocalRaidSettings.transition.transitionCount,
                    EventPlayer = transitController.LocalRaidSettings.transition.transitionType.HasFlagNoBox(ELocationTransition.Event)
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
            }
        }

        if (Location.EventTrapsData != null)
        {
            LabyrinthSyncableTrapClass.InitLabyrinthSyncableTraps(Location.EventTrapsData);
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
        _spawnPoints = SpawnPointManagerClass.CreateFromScene(new DateTime?(EFTDateTimeClass.LocalDateTimeFromUnixTime(Location.UnixDateTime)),
                                Location.SpawnPointParams);
        var spawnSafeDistance = (Location.SpawnSafeDistanceMeters > 0) ? Location.SpawnSafeDistanceMeters : 100;
        SpawnSettingsStruct settings = new(Location.MinDistToFreePoint,
            Location.MaxDistToFreePoint, Location.MaxBotPerZone, spawnSafeDistance,
            Location.NoGroupSpawn, Location.OneTimeSpawn);
        SpawnSystem = SpawnSystemCreatorClass.CreateSpawnSystem(settings, FikaGlobals.GetApplicationTime, Singleton<GameWorld>.Instance, _botsController, _spawnPoints);

        var side = Singleton<IFikaNetworkManager>.Instance.RaidSide == ESideType.Pmc ? EPlayerSide.Usec : EPlayerSide.Savage;

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

        var startTime = EFTDateTimeClass.UtcNow.AddSeconds((double)timeBeforeDeployLocal);
        GameTime = startTime;
        server.GameStartTime = startTime;
        SessionTime = abstractGame.GameTimer.SessionTime;

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

    public override void InitializeTransitSystem(GameWorld gameWorld, BackendConfigSettingsClass instance, Profile profile, LocalRaidSettings localRaidSettings, LocationSettingsClass.Location location)
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
            TransitControllerAbstractClass.DisableTransitPoints();
        }
    }
}
