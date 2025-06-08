using Comfort.Common;
using EFT;
using EFT.GlobalEvents;
using EFT.Interactive;
using EFT.InventoryLogic;
using Fika.Core.Coop.GameMode;
using Fika.Core.Coop.Players;
using Fika.Core.Coop.Utils;
using Fika.Core.Networking;
using LiteNetLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fika.Headless.Classes
{
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

            OnPlayerEnter += HeadlessOnPlayerEnter;
            OnPlayerExit += HeadlessOnPlayerExit;
        }

        private readonly LocalRaidSettings _localRaidSettings;
        private readonly FikaServer _server = Singleton<FikaServer>.Instance;
        private readonly Dictionary<Player, int> _playersInTransitZone = [];
        private bool _headlessTransit;
        private readonly List<int> _transittedPlayers = [];

        public int AliveTransitPlayers
        {
            get
            {
                return _transittedPlayers.Count;
            }
        }

        private void HeadlessOnPlayerEnter(TransitPoint point, Player player)
        {
            if (!method_11(player, point.parameters.id, out string _))
            {

            }
            else
            {
                if (!method_11(player, point.parameters.id, out string _))
                {
                    return;
                }
            }

            if (!_playersInTransitZone.ContainsKey(player))
            {
                _playersInTransitZone.Add(player, point.parameters.id);
            }

            if (!transitPlayers.ContainsKey(player.ProfileId))
            {
                if (player is CoopPlayer coopPlayer)
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

                _server.SendDataToAll(ref packet, DeliveryMethod.ReliableOrdered);
                return;
            }
            Dictionary_0[point.parameters.id].GroupEnter(player);
        }

        private bool method_11(Player player, int pointId, out string keyId)
        {
            GClass1709.Class1068 @class = new()
            {
                player = player
            };
            keyId = string.Empty;
            if (!method_10(pointId, out @class.accessKeys))
            {
                return true;
            }
            if (@class.player.Side == EPlayerSide.Savage)
            {
                return false;
            }
            IEnumerable<Item> playerItems = @class.player.InventoryController.Inventory.GetPlayerItems(EPlayerItems.Equipment);
            Item item = playerItems?.FirstOrDefault(new Func<Item, bool>(@class.method_0));
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

            _server.SendDataToAll(ref packet, DeliveryMethod.ReliableOrdered);
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

            _server.SendDataToAll(ref packet, DeliveryMethod.ReliableOrdered);
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

            _server.SendDataToAll(ref packet, DeliveryMethod.ReliableOrdered);
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

            _server.SendDataToAll(ref packet, DeliveryMethod.ReliableOrdered);
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

            TransitEventPacket eventPacket = new()
            {
                EventType = TransitEventPacket.ETransitEventType.Interaction,
                TransitEvent = new TransitInteractionEvent()
                {
                    PlayerId = player.Id,
                    PointId = packet.pointId,
                    Type = TransitInteractionEvent.EType.Confirm
                }
            };

            _server.SendDataToAll(ref eventPacket, DeliveryMethod.ReliableOrdered);
        }

        private bool CheckForPlayers(Player player, int pointId)
        {
            int humanPlayers = 0;
            foreach (CoopPlayer coopPlayer in Singleton<IFikaNetworkManager>.Instance.CoopHandler.HumanPlayers)
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

                _server.SendDataToAll(ref messagePacket, DeliveryMethod.ReliableOrdered);
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

            _server.SendDataToAll(ref packet, DeliveryMethod.ReliableOrdered);

            if (FikaBackendUtils.IsHeadless && !_headlessTransit)
            {
                ExtractHeadlessClient(point);
            }
        }

        private void ExtractHeadlessClient(TransitPoint point)
        {
            _headlessTransit = true;

            string location = point.parameters.location;
            if (TarkovApplication.Exist(out TarkovApplication tarkovApplication))
            {
                tarkovApplication.transitionStatus = new(location, false, _localRaidSettings.playerSide, ERaidMode.Local, _localRaidSettings.timeVariant);
            }

            Singleton<IFikaGame>.Instance.Stop(string.Empty, ExitStatus.Transit, point.parameters.name);
        }
    }
}
