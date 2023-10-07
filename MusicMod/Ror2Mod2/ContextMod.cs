using BepInEx;
using MyRoR2;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Utils;

namespace Ror2Mod2
{
    [BepInPlugin("com.woodyscales.contextmod", "Context Reporting", "0.0.1")]
    public class ContextMod : BaseUnityPlugin
    {
        private bool muteMusic;

        private ContextHelper contextHelper;

        public IPC.Server Server { get; private set; }

        private Logger SafeLogger => x => Logger.LogDebug(x ?? "null");

        public void Awake()
        {
            contextHelper = new ContextHelper(SafeLogger);
            Server = new IPC.Server(5008, nameof(ContextMod));
            Server.ReceivedRequest += Server_ReceivedRequest;
            Server.TryStart.CreateRun().RunToCompletion(true);
            contextHelper.NewContext += ContextHelper_NewContext;

            On.RoR2.UI.PauseScreenController.OnEnable += PauseScreenController_OnEnable;

            On.RoR2.UI.PauseScreenController.OnDisable += PauseScreenController_OnDisable;
        }

        public void Update()
        {
            if (muteMusic && RoR2.Console.instance != null)
            {
                var convar = RoR2.Console.instance.FindConVar("volume_music");

                // set in game music volume to 0 so we hear the new music only.
                if (convar != null)
                {
                    convar.SetString("0");
                    muteMusic = false;
                }
            }
        }

        private IEnumerable<IPC.Message> Server_ReceivedRequest(IEnumerable<IPC.Message> arg)
        {
            foreach (var message in arg)
            {
                switch (message.Key)
                {
                    case "mute":
                        muteMusic = true;
                        break;
                }
            }

            return Enumerable.Empty<IPC.Message>();
        }

        private void ContextHelper_NewContext(RoR2Context obj)
        {
            Server.Broadcast(new IPC.Message(nameof(RoR2Context), Json.ToJson(obj)));
        }

        private void PauseScreenController_OnDisable(On.RoR2.UI.PauseScreenController.orig_OnDisable orig, RoR2.UI.PauseScreenController self)
        {
            orig(self);

            if (RoR2.PlatformSystems.networkManager.isNetworkActive)
            {
                Server.Broadcast(new IPC.Message("resume"));
            }
        }

        private void PauseScreenController_OnEnable(On.RoR2.UI.PauseScreenController.orig_OnEnable orig, RoR2.UI.PauseScreenController self)
        {
            orig(self);

            Server.Broadcast(new IPC.Message("pause"));
        }
    }
}