using EFT;
using EFT.Interactive;
using EFT.Interactive.SecretExfiltrations;
using EFT.UI;
using Fika.Core.Coop.GameMode;
using Fika.Core.Coop.HostClasses;

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

            ExfilManager.Run(exfilPoints, secretExfilPoints);
            _abstractGame.Status = GameStatus.Started;
            _botsController.Bots.CheckActivation();

            ConsoleScreen.ApplyStartCommands();
        }
    }
}
