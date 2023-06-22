using BepInEx;
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
	[BepInDependency(NetworkingAPI.PluginGUID)]
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

		private void PauseScreenController_OnDisable(On.RoR2.UI.PauseScreenController.orig_OnDisable orig, PauseScreenController self)
		{
			Logging.Record($"Entered method {nameof(PauseScreenController_OnDisable)}");
			PauseMessageBase message = ResumeMessage.Create().ForServer;
			Logging.Record("Sending message to server");
			message.Send(NetworkDestination.Server);
			Logging.Record($"Message {message.Guid} sent to server, calling orig");
			orig(self);
		}

		private void PauseManager_CCTogglePause(On.RoR2.PauseManager.orig_CCTogglePause orig, ConCommandArgs args)
		{
			string arg = args.TryGetArgString(0);
			Logging.Record($"arg: '{arg}'");

			bool bypass = arg?.EndsWith("bypass") ?? false;

			if (bypass || !NetworkManager.singleton.isNetworkActive)
			{
				orig(args);
				return;
			}

			PauseMessageBase message;

			bool shouldPause = (arg?.StartsWith("pause") ?? false) || (!(arg?.StartsWith("resume") ?? false) && !PauseManager.isPaused);

			if (shouldPause)
			{
				Logging.Record($"Creating PauseMessage");
				message = PauseMessage.Create();
			}
			else
			{
				Logging.Record($"Creating ResumeMessage");
				message = ResumeMessage.Create();
			}

			if (NetworkServer.active)
			{
				Logging.Record("Sending message to clients");
				message.ForClient.Send(NetworkDestination.Clients);

				if (arg?.StartsWith("resume") ?? false)
				{
					Logging.Record($"Message {message.Guid} sent to clients");
				}
				else
				{
					Logging.Record($"Message {message.Guid} sent to clients, calling orig");
					orig(args);
				}
			}
			else
			{
				Logging.Record("Sending message to server");
				message.ForServer.Send(NetworkDestination.Server);
				Logging.Record($"Message {message.Guid} sent to server");
			}
		}
	}
}