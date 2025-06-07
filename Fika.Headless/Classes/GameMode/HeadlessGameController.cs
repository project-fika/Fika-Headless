using Comfort.Common;
using EFT;
using EFT.Game.Spawning;
using EFT.Interactive;
using EFT.Interactive.SecretExfiltrations;
using EFT.UI;
using Fika.Core.Coop.GameMode;
using Fika.Core.Coop.HostClasses;
using Fika.Core.Coop.Utils;
using Fika.Core.Networking;
using LiteNetLib;
using System;
using System.Threading.Tasks;

namespace Fika.Headless.Classes.GameMode
{
    internal class HeadlessGameController(IFikaGame game, EUpdateQueue updateQueue, GameWorld gameWorld, ISession session,
        LocationSettingsClass.Location location, WavesSettings wavesSettings, GameDateTime gameDateTime)
        : HostGameController(game, updateQueue, gameWorld, session, location, wavesSettings, gameDateTime)
    {
        public override void SetupEventsAndExfils(Player player)
        {
            Logger.LogInfo("[SERVER] SpawnPoint: " + _spawnPoint.Id + ", InfiltrationPoint: " + InfiltrationPoint);

            ExfiltrationControllerClass exfilController = ExfiltrationControllerClass.Instance;

            ExfiltrationPoint[] exfilPoints = exfilController.EligiblePoints(string.Empty);
            SecretExfiltrationPoint[] secretExfilPoints = [.. exfilController.SecretEligiblePoints(), .. exfilController.GetScavSecretExits()];

            if (TransitControllerAbstractClass.Exist(out FikaHostTransitController transitController))
            {
                transitController.Init();
                // TODO: Sync to clients!!!
            }

            _abstractGame.Status = GameStatus.Started;
            _botsController.Bots.CheckActivation();

            ConsoleScreen.ApplyStartCommands();
        }

        public override void CreateSpawnSystem(Profile profile)
        {
            _spawnPoints = SpawnPointManagerClass.CreateFromScene(new DateTime?(EFTDateTimeClass.LocalDateTimeFromUnixTime(Location.UnixDateTime)),
                                    Location.SpawnPointParams);
            int spawnSafeDistance = (Location.SpawnSafeDistanceMeters > 0) ? Location.SpawnSafeDistanceMeters : 100;
            SpawnSettingsStruct settings = new(Location.MinDistToFreePoint,
                Location.MaxDistToFreePoint, Location.MaxBotPerZone, spawnSafeDistance,
                Location.NoGroupSpawn, Location.OneTimeSpawn);
            SpawnSystem = SpawnSystemCreatorClass.CreateSpawnSystem(settings, FikaGlobals.GetApplicationTime, Singleton<GameWorld>.Instance, _botsController, _spawnPoints);

            EPlayerSide side = Singleton<IFikaNetworkManager>.Instance.RaidSide == ESideType.Pmc ? EPlayerSide.Usec : EPlayerSide.Savage;

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

            FikaServer server = Singleton<FikaServer>.Instance;
            server.HostReady = true;

            DateTime startTime = EFTDateTimeClass.UtcNow.AddSeconds((double)timeBeforeDeployLocal);
            GameTime = startTime;
            server.GameStartTime = startTime;
            SessionTime = abstractGame.GameTimer.SessionTime;

            InformationPacket packet = new()
            {
                RaidStarted = RaidStarted,
                ReadyPlayers = server.ReadyClients,
                HostReady = server.HostReady,
                GameTime = GameTime.Value,
                SessionTime = SessionTime.Value
            };

            server.SendDataToAll(ref packet, DeliveryMethod.ReliableOrdered);
            LootData = null;

            return Task.CompletedTask;
        }
    }
}
