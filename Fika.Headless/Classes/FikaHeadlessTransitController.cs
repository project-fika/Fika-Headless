using Comfort.Common;
using Diz.Utils;
using EFT;
using EFT.GlobalEvents;
using EFT.Interactive;
using EFT.InventoryLogic;
using Fika.Core.Main.Components;
using Fika.Core.Main.GameMode;
using Fika.Core.Main.Players;
using Fika.Core.Main.Utils;
using Fika.Core.Networking;
using Fika.Core.Networking.Packets.Communication;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fika.Headless.Classes;

/// <summary>
/// Transit controller for the <see cref="GameMode.HeadlessGame"/>
/// </summary>
public class FikaHeadlessTransitController : TransitControllerAbstractClass
{
    public FikaHeadlessTransitController(BackendConfigSettingsClass.TransitSettingsClass settings,
        LocationSettingsClass.Location.TransitParameters[] parameters,
        LocalRaidSettings localRaidSettings)
        : base(settings, parameters)
    {
        _localRaidSettings = localRaidSettings;

        TransferItemsController.InitItemControllerServer(FikaGlobals.TransitTraderId, FikaGlobals.TransitTraderName);
        OnPlayerEnter += HeadlessOnPlayerEnter;
        OnPlayerExit += HeadlessOnPlayerExit;
    }

    private readonly LocalRaidSettings _localRaidSettings;
    private readonly FikaServer _server = Singleton<FikaServer>.Instance;
    private readonly Dictionary<Player, int> _playersInTransitZone = [];
    private bool _headlessTransit;
    private readonly List<int> _transittedPlayers = [];
    private string _usedPoint;

    public int AliveTransitPlayers
    {
        get
        {
            return _transittedPlayers.Count;
        }
    }

    private void HeadlessOnPlayerEnter(TransitPoint point, Player player)
    {
#if DEBUG
        FikaGlobals.LogInfo($"{player.Profile.Info.Nickname} entered transit point {point.Description}");
#endif

        if (!method_11(player, point.parameters.id, out string _))
        {
#if DEBUG
            FikaGlobals.LogWarning("Player is not eligible for this transit point");
#endif
            return;
        }

        if (!_playersInTransitZone.ContainsKey(player))
        {
            _playersInTransitZone.Add(player, point.parameters.id);
        }

        if (!transitPlayers.ContainsKey(player.ProfileId))
        {
            if (player is FikaPlayer coopPlayer)
            {
                coopPlayer.UpdateBtrTraderServiceData().HandleExceptions();
            }
            TransitEventPacket packet = new()
            {
                EventType = TransitEventPacket.ETransitEventType.Interaction,
                TransitEvent = new TransitInteractionEvent()
                {
                    PlayerId = player.Id,
                    PointId = point.parameters.id,
                    Type = TransitInteractionEvent.EType.Show
                }
            };

            _server.SendData(ref packet, DeliveryMethod.ReliableOrdered);
            return;
        }
        Dictionary_0[point.parameters.id].GroupEnter(player);
    }

    private bool method_11(Player player, int pointId, out string keyId)
    {
        keyId = string.Empty;
        if (!method_10(pointId, out var accessKeys))
        {
            return true;
        }
        if (player.Side == EPlayerSide.Savage)
        {
            return false;
        }
        IEnumerable<Item> playerItems = player.InventoryController.Inventory.GetPlayerItems(EPlayerItems.Equipment);
        Item item = playerItems?
            .Where(item => player.InventoryController.Examined(item) && accessKeys.Contains(item.StringTemplateId))
            .FirstOrDefault();
        if (item == null)
        {
            return false;
        }
        keyId = item.Id;
        return true;
    }

    private bool method_10(int pointId, out string[] accessKeys)
    {
        accessKeys = null;
        if (!method_9(Dictionary_0[pointId].parameters.target, out LocationSettingsClass.Location location))
        {
            return false;
        }
        accessKeys = ((location != null) ? location.AccessKeys : null);
        return accessKeys != null && accessKeys.Length != 0;
    }

    private bool method_9(string locationId, out LocationSettingsClass.Location location)
    {
        return Singleton<ClientApplication<ISession>>.Instance.GetClientBackEndSession().LocationSettings.locations.TryGetValue(locationId, out location);
    }

    private void HeadlessOnPlayerExit(TransitPoint point, Player player)
    {
#if DEBUG
        FikaGlobals.LogInfo($"{player.Profile.Info.Nickname} left transit point {point.Description}");
#endif

        if (_playersInTransitZone.TryGetValue(player, out int value))
        {
            if (value == point.parameters.id)
            {
                _playersInTransitZone.Remove(player);
            }
        }

        if (transitPlayers.ContainsKey(player.ProfileId))
        {
            point.GroupExit(player);
        }

        TransitEventPacket packet = new()
        {
            EventType = TransitEventPacket.ETransitEventType.Interaction,
            TransitEvent = new TransitInteractionEvent()
            {
                PlayerId = player.Id,
                PointId = point.parameters.id,
                Type = TransitInteractionEvent.EType.Hide
            }
        };

        _server.SendData(ref packet, DeliveryMethod.ReliableOrdered);
    }


    public override void InactivePointNotification(int playerId, int pointId)
    {
        TransitEventPacket packet = new()
        {
            EventType = TransitEventPacket.ETransitEventType.Interaction,
            TransitEvent = new TransitInteractionEvent()
            {
                PlayerId = playerId,
                PointId = pointId,
                Type = TransitInteractionEvent.EType.InactivePoint
            }
        };

        _server.SendData(ref packet, DeliveryMethod.ReliableOrdered);
    }

    public override void Sizes(Dictionary<int, byte> sizes)
    {
        TransitEventPacket packet = new()
        {
            EventType = TransitEventPacket.ETransitEventType.GroupSize,
            TransitEvent = new TransitGroupSizeEvent()
            {
                Sizes = sizes
            }
        };

        _server.SendData(ref packet, DeliveryMethod.ReliableOrdered);
    }

    public override void Timers(int pointId, Dictionary<int, ushort> timers)
    {
        TransitEventPacket packet = new()
        {
            EventType = TransitEventPacket.ETransitEventType.GroupTimer,
            TransitEvent = new TransitGroupTimerEvent()
            {
                PointId = pointId,
                Timers = timers
            }
        };

        _server.SendData(ref packet, DeliveryMethod.ReliableOrdered);
    }

    public override void InteractWithTransit(Player player, TransitInteractionPacketStruct packet)
    {
        TransitPoint point = Dictionary_0[packet.pointId];
        if (point == null)
        {
            return;
        }

        if (!CheckForPlayers(player, packet.pointId))
        {
            return;
        }

        transitPlayers[player.ProfileId] = player.Id;
        profileKeys[player.ProfileId] = packet.keyId;
        Dictionary_0[packet.pointId].GroupEnter(player);
        ExfiltrationControllerClass.Instance.BannedPlayers.Add(player.Id);
        ExfiltrationControllerClass.Instance.CancelExtractionForPlayer(player);
    }

    private bool CheckForPlayers(Player player, int pointId)
    {
        int humanPlayers = 0;
        foreach (FikaPlayer coopPlayer in Singleton<IFikaNetworkManager>.Instance.CoopHandler.HumanPlayers)
        {
            if (coopPlayer.HealthController.IsAlive)
            {
                humanPlayers++;
            }
        }

        int playersInPoint = 0;
        foreach (KeyValuePair<Player, int> item in _playersInTransitZone)
        {
            if (item.Key.HealthController.IsAlive)
            {
                if (item.Value == pointId)
                {
                    playersInPoint++;
                }
            }
        }

        if (playersInPoint < humanPlayers)
        {
            Dictionary<int, TransitMessagesEvent.EType> messages = [];
            messages.Add(player.Id, TransitMessagesEvent.EType.NonAllTeammates);

            TransitEventPacket messagePacket = new()
            {
                EventType = TransitEventPacket.ETransitEventType.Messages,
                TransitEvent = new TransitMessagesEvent()
                {
                    Messages = messages
                }
            };

            _server.SendData(ref messagePacket, DeliveryMethod.ReliableOrdered);
            return false;
        }

        return true;
    }

    public override void Transit(TransitPoint point, int playersCount, string hash, Dictionary<string, ProfileKey> keys, Player player)
    {
        TransitEventPacket packet = new()
        {
            EventType = TransitEventPacket.ETransitEventType.Extract,
            PlayerId = player.PlayerId,
            TransitId = point.parameters.id
        };

        _transittedPlayers.Add(player.Id);

        _server.SendData(ref packet, DeliveryMethod.ReliableOrdered);

        if (!_headlessTransit)
        {
            ExtractHeadlessClient(point);
        }
    }

    private void ExtractHeadlessClient(TransitPoint point)
    {
        _headlessTransit = true;
        _usedPoint = point.parameters.name;

        string location = point.parameters.location;
        if (TarkovApplication.Exist(out TarkovApplication tarkovApplication))
        {
            tarkovApplication.transitionStatus = new(location, false, _localRaidSettings.playerSide, ERaidMode.Local, _localRaidSettings.timeVariant);
        }

        FikaBackendUtils.IsTransit = true;
        _ = Task.Run(DelayHeadlessExtract);
    }

    private async Task DelayHeadlessExtract()
    {
        CoopHandler coopHandler = Singleton<IFikaNetworkManager>.Instance.CoopHandler;
        if (coopHandler == null)
        {
            FikaGlobals.LogError("CoopHandler was null, quitting after 3 seconds");
            await Task.Delay(3000);
        }
        else
        {
            while (coopHandler.AmountOfHumans > 0)
            {
                await Task.Delay(1000);
            }
        }
        AsyncWorker.RunInMainTread(StopHeadlessGameFromTransit);
    }

    private void StopHeadlessGameFromTransit()
    {
        Singleton<IFikaGame>.Instance.Stop(string.Empty, ExitStatus.Transit, _usedPoint);
    }

    public override void Dispose()
    {
        OnPlayerEnter -= HeadlessOnPlayerEnter;
        OnPlayerExit -= HeadlessOnPlayerExit;
    }

    public void Init()
    {
        EnablePoints(true);
    }
}
