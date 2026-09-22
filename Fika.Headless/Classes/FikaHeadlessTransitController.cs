using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Comfort.Common;
using CommonAssets.Scripts.Game;
using EFT;
using EFT.GlobalEvents;
using EFT.Interactive;
using EFT.InventoryLogic;
using Fika.Core.Main.Components;
using Fika.Core.Main.Players;
using Fika.Core.Main.Utils;
using Fika.Core.Networking;
using Fika.Core.Networking.Packets.Communication;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using JsonType;

namespace Fika.Headless.Classes;

/// <summary>
/// Transit controller for the <see cref="GameMode.HeadlessGame"/>
/// </summary>
public class FikaHeadlessTransitController : TransitController
{
    public FikaHeadlessTransitController(IntPtr pointer) : base(pointer)
    {
    }

    public FikaHeadlessTransitController(TransitGlobalSettings settings, Il2CppReferenceArray<LocationSettings.Location.TransitParameters> parameters,
        LocalRaidSettings localRaidSettings) : base(Il2CppInjection.Allocate<FikaHeadlessTransitController>())
    {
        ClassInjector.DerivedConstructorBody(this);
        _server = Singleton<FikaServer>.Instance;
        _playersInTransitZone = [];
        _transittedPlayers = [];
        ClassInjector.InvokeBaseConstructor<TransitController>(this, settings, parameters);
        _localRaidSettings = localRaidSettings;
        IsEvent = localRaidSettings.transitionType.HasFlagNoBox(ELocationTransition.Event);

        TransferItemsController.InitItemControllerServer(FikaGlobals.TransitTraderId, FikaGlobals.TransitTraderName);
        _onHeadlessPlayerEnter = new Action<TransitPoint, Player>(HeadlessOnPlayerEnter);
        _onHeadlessPlayerExit = new Action<TransitPoint, Player>(HeadlessOnPlayerExit);
        OnPlayerEnter += _onHeadlessPlayerEnter;
        OnPlayerExit += _onHeadlessPlayerExit;
    }

    public bool IsEvent { get; }

    public LocalRaidSettings LocalRaidSettings
    {
        get
        {
            return _localRaidSettings;
        }
    }

    private readonly LocalRaidSettings _localRaidSettings;
    private readonly FikaServer _server;
    private readonly Il2CppSystem.Action<TransitPoint, Player> _onHeadlessPlayerEnter;
    private readonly Il2CppSystem.Action<TransitPoint, Player> _onHeadlessPlayerExit;
    private readonly Dictionary<Player, int> _playersInTransitZone;
    private bool _headlessTransit;
    private readonly List<int> _transittedPlayers;
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

        if (!TryGetAccessToLocation(player, point.parameters.id, out _))
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
            var fikaPlayer = player.TryCast<FikaPlayer>();
            if (fikaPlayer != null)
            {
                fikaPlayer.UpdateBtrTraderServiceData().HandleExceptions();
            }

            TransitEventPacket packet = new()
            {
                EventType = TransitEventPacket.ETransitEventType.Interaction,
                TransitEvent = new TransitInteractionEvent()
                {
                    PlayerRaidId = player.RaidId,
                    PointId = point.parameters.id,
                    Type = TransitInteractionEvent.EType.Show
                }
            };

            _server.SendData(ref packet, DeliveryMethod.ReliableOrdered);
            return;
        }

        pointsById[point.parameters.id].GroupEnter(player);
    }

    private bool TryGetAccessToLocation(Player player, int pointId, out string keyId)
    {
        keyId = string.Empty;
        if (!TryGetAccessRequirements(pointId, out var requirements))
        {
            return true;
        }

        if (player.Side == EPlayerSide.Savage)
        {
            return false;
        }

        var profile = player.Profile;
        var items = profile.Inventory.GetPlayerItems(EPlayerItems.Equipment);
        if (!LocationAccessRequirementsUtility.TryFindSatisfiedRequirement(items, requirements, new Func<Item, bool>(profile.Examined),
            out var item, out _, out _) || item == null)
        {
            return false;
        }

        keyId = item.Id;
        return true;
    }

    private bool TryGetAccessRequirements(int pointId, out Il2CppSystem.Collections.Generic.IReadOnlyDictionary<string, int> requirements)
    {
        requirements = null;
        if (!TarkovApplication.Exist(out var tarkovApplication))
        {
            return false;
        }

        var session = tarkovApplication.Session;
        if (!session.LocationSettings.locations.TryGetValue(pointsById[pointId].parameters.target, out var location) || location == null)
        {
            return false;
        }

        requirements = location.GetAccessRequirements(session.GameMode, LocationSettings.Location.EAccessUsageType.Transit);
        return requirements != null && requirements.Count > 0;
    }

    private void HeadlessOnPlayerExit(TransitPoint point, Player player)
    {
#if DEBUG
        FikaGlobals.LogInfo($"{player.Profile.Info.Nickname} left transit point {point.Description}");
#endif

        if (_playersInTransitZone.TryGetValue(player, out var value))
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
                PlayerRaidId = player.RaidId,
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
                PlayerRaidId = playerId,
                PointId = pointId,
                Type = TransitInteractionEvent.EType.InactivePoint
            }
        };

        _server.SendData(ref packet, DeliveryMethod.ReliableOrdered);
    }

    public override void Sizes(int pointId, Il2CppSystem.Collections.Generic.Dictionary<int, byte> sizes)
    {
        _currentTransitPointId = pointId;
        TransitEventPacket packet = new()
        {
            EventType = TransitEventPacket.ETransitEventType.GroupSize,
            TransitEvent = new TransitGroupSizeEvent()
            {
                PointId = pointId,
                Sizes = sizes
            }
        };

        _server.SendData(ref packet, DeliveryMethod.ReliableOrdered);
    }

    public override void Timers(int pointId, Il2CppSystem.Collections.Generic.Dictionary<int, ushort> timers)
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

    public override void InteractWithTransit(Player player, InteractWithTransitPacket packet)
    {
        var point = pointsById[packet.pointId];
        if (point == null)
        {
            return;
        }

        if (transitPlayers.ContainsKey(player.ProfileId))
        {
            return;
        }

        if (!CheckForPlayers(player, packet.pointId))
        {
            return;
        }

        transitPlayers[player.ProfileId] = player.Id;
        profileKeys[player.ProfileId] = packet.keyId;
        pointsById[packet.pointId].GroupEnter(player);
        ExfiltrationController.Instance.BannedPlayers.Add(player.Id);
        ExfiltrationController.Instance.CancelExtractionForPlayer(player);

        TransitEventPacket confirmPacket = new()
        {
            EventType = TransitEventPacket.ETransitEventType.Interaction,
            TransitEvent = new TransitInteractionEvent()
            {
                PlayerRaidId = player.RaidId,
                PointId = packet.pointId,
                Type = TransitInteractionEvent.EType.Confirm
            }
        };

        _server.SendData(ref confirmPacket, DeliveryMethod.ReliableOrdered);
    }

    private bool CheckForPlayers(Player player, int pointId)
    {
        var humanPlayers = 0;
        foreach (var fikaPlayer in FikaGlobals.NetworkManager.CoopHandler.HumanPlayers)
        {
            if (fikaPlayer.HealthController.IsAlive)
            {
                humanPlayers++;
            }
        }

        var playersInPoint = 0;
        foreach (var item in _playersInTransitZone)
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
                    Messages = messages.ToIl2CppDictionary()
                }
            };

            _server.SendData(ref messagePacket, DeliveryMethod.ReliableOrdered);
            return false;
        }

        return true;
    }

    public override Il2CppSystem.Threading.Tasks.Task Transit(TransitPoint point, int playersCount, string hash,
        Il2CppSystem.Collections.Generic.Dictionary<string, ProfileKey> keys, Player player)
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

        return Il2CppSystem.Threading.Tasks.Task.CompletedTask;
    }

    private void ExtractHeadlessClient(TransitPoint point)
    {
        _headlessTransit = true;
        _usedPoint = point.parameters.name;

        var location = point.parameters.location;
        if (TarkovApplication.Exist(out var tarkovApplication))
        {
            tarkovApplication.TransitionStatus = new TransitionStatus(location, false, _localRaidSettings.playerSide, ERaidMode.Local, _localRaidSettings.timeVariant);
        }

        FikaBackendUtils.IsTransit = true;
        _ = DelayHeadlessExtract();
    }

    private async Task DelayHeadlessExtract()
    {
        var coopHandler = FikaGlobals.NetworkManager.CoopHandler;
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

        StopHeadlessGameFromTransit();
    }

    private void StopHeadlessGameFromTransit()
    {
        FikaGlobals.FikaGame.Stop(string.Empty, ExitStatus.Transit, _usedPoint);
    }

    public override void Dispose()
    {
        OnPlayerEnter -= _onHeadlessPlayerEnter;
        OnPlayerExit -= _onHeadlessPlayerExit;
    }

    public void Init()
    {
        EnablePoints(true);
    }
}
