using R2API.Networking.Interfaces;
using RoR2;
using RoR2.UI;
using System;
using System.Linq;
using UnityEngine.Networking;

namespace UntitledMod
{
    public class SyncPickupPickerPanelInfoMessage : INetMessage
    {
        public SyncPickupPickerPanelInfoMessage(NetworkInstanceId playerId, NetworkInstanceId netId)
        {
            this.playerId = playerId;
            this.netId = netId;
            this.info = Array.Empty<bool>();
        }

        private NetworkInstanceId playerId;

        private NetworkInstanceId netId;

        private bool[] info;

        public static Reader Reader { get; set; }

        public void Deserialize(NetworkReader reader)
        {
            this.netId = reader.ReadNetworkId();
            this.playerId = reader.ReadNetworkId();
            this.info = reader.ReadString().Select(x => x switch
                {
                    '0' => false,
                    '1' => true,
                    _ => throw new ArgumentOutOfRangeException(nameof(x), x, "Unexpected character in string."),
                }).ToArray();
        }

        public void OnReceived()
        {
            var gameObject = Util.FindNetworkObject(this.netId);
            var panel = gameObject.GetComponent<PickupPickerPanel>();

            if(this.info.Length > 0)
            {
                Reader.SetPickupPanelInfo(panel, this.info);
            }
            else
            {
                var pickupPickerOptions = panel.pickupOptions;

                var playerGameObject = Util.FindNetworkObject(this.playerId);
                var playerCharacterMasterController = playerGameObject.GetComponent<PlayerCharacterMasterController>();

                this.info = Reader.GetPickupPanelInfo(playerCharacterMasterController, pickupPickerOptions.Select(x => x.pickupIndex));
                this.Send(panel.pickerController.connectionToClient);
            }
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(this.netId);
            writer.Write(this.playerId);
            writer.Write(string.Concat(this.info.Select(x => x ? '1' : '0')));
        }
    }
}