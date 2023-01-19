using BepInEx;
using MyRoR2;
using Utils;
using Newtonsoft.Json;

namespace Ror2Mod2
{
	[BepInDependency("com.bepis.r2api")]
	[BepInPlugin("com.woodyscales.contextmod", "Context Reporting", "1.0.0")]
	[BepInDependency(R2API.R2API.PluginGUID)]
	[BepInDependency("com.rune580.riskofoptions")]
	public class ContextMod : BaseUnityPlugin
	{
		private readonly Configuration configuration;

		private bool musicMuted;

		private ContextHelper contextHelper;

		protected ContextMod() => configuration = new Configuration(Config);

		public IPC.Server Server { get; private set; }

		private Logger SafeLogger => x => Logger.LogDebug(x ?? "null");

		public void Awake()
		{
			contextHelper = new ContextHelper(SafeLogger);
			Server = new IPC.Server(5008);
			Server.TryStart.CreateRun().RunToCompletion(true);
			contextHelper.NewContext += ContextHelper_NewContext;

			On.RoR2.UI.PauseScreenController.OnEnable += PauseScreenController_OnEnable;

			On.RoR2.UI.PauseScreenController.OnDisable += PauseScreenController_OnDisable;
		}

		public void Update()
		{
			if (!musicMuted && RoR2.Console.instance != null)
			{
				var convar = RoR2.Console.instance.FindConVar("volume_music");

				// set in game music volume to 0 so we hear the new music only.
				if (convar != null)
				{
					convar.SetString("0");
					musicMuted = true;
				}
			}
		}

		private void ContextHelper_NewContext(Context obj)
		{
			Server.Broadcast(new IPC.Message(nameof(Context), JsonConvert.SerializeObject(obj)));
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