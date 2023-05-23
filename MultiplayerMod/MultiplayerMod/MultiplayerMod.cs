using BepInEx;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.UI;
using System;
using UnityEngine.Networking;

namespace MultiplayerMod
{
	[BepInPlugin("com.woodyscales.multiplayermod", "Multiplayer Helper", "0.0.1")]
	public class MultiplayerMod : BaseUnityPlugin
	{
		public void Awake()
		{
			NetworkingAPI.RegisterMessageType<PauseMessage>();
			NetworkingAPI.RegisterMessageType<ResumeMessage>();
			On.RoR2.PauseManager.CCTogglePause += PauseManager_CCTogglePause;
			On.RoR2.UI.PauseScreenController.OnDisable += PauseScreenController_OnDisable;
			IL.RoR2.UI.PauseScreenController.OnEnable += PauseScreenController_OnEnable;
		}

		private void PauseScreenController_OnEnable(ILContext il)
		{
			var c = new ILCursor(il);

			c.GotoNext(x => x.MatchCall<NetworkServer>("get_dontListen"),
				x => x.MatchStsfld<PauseScreenController>("paused"));
			c.Remove();
			c.EmitDelegate<Func<bool>>(() =>
			{
				_ = NetworkServer.dontListen;
				return true;
			});
		}

		private void PauseScreenController_OnDisable(On.RoR2.UI.PauseScreenController.orig_OnDisable orig, RoR2.UI.PauseScreenController self)
		{
			new ResumeMessage().ForServer.Send(NetworkDestination.Server);
			orig(self);
		}

		private void PauseManager_CCTogglePause(On.RoR2.PauseManager.orig_CCTogglePause orig, ConCommandArgs args)
		{
			bool bypass = args.TryGetArgString(0) == "bypass";

			if (bypass || !NetworkManager.singleton.isNetworkActive)
			{
				orig(args);
				return;
			}

			PauseMessageBase message;

			if (PauseManager.isPaused)
			{
				message = new ResumeMessage();
			}
			else
			{
				message = new PauseMessage();
			}

			if (NetworkServer.active)
			{
				message.ForClient.Send(NetworkDestination.Clients);
				orig(args);
			}
			else
			{
				message.ForServer.Send(NetworkDestination.Server);
			}
		}
	}
}