using BepInEx;
using Rules;
using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Mutable;
using System.Configuration;
using System.IO;
using System.Xml.Linq;
using Utils;
using static Rules.Examples;

namespace Ror2Mod2
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.woodyscales.musicmod", "Music Mod", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID)]
    public class MusicMod : BaseUnityPlugin
    {
        public SpotifyController Music;

        private readonly Configuration configuration;

        protected MusicMod()
        {
            configuration = new Configuration(Config);
        }

        private Logger SafeLogger => x => Logger.LogDebug(x ?? "null");

        public void Awake()
        {
            string uri = configuration.RuleLocation;

            if (uri is null)
            {
                throw new FileNotFoundException();
            }

            IRule rule;

            if (string.IsNullOrEmpty(uri))
            {
                rule = MimicRule;
            }
            else
            {
                this.Log(uri);
                rule = Rule.FromXml(XElement.Load(uri));
            }

            Music = new SpotifyController(new SingleRulePicker(rule), SafeLogger);

            On.RoR2.UI.PauseScreenController.OnEnable += PauseScreenController_OnEnable;

            On.RoR2.UI.PauseScreenController.OnDisable += PauseScreenController_OnDisable;
        }

        private void PauseScreenController_OnDisable(On.RoR2.UI.PauseScreenController.orig_OnDisable orig, RoR2.UI.PauseScreenController self)
        {
            orig(self);
            Music.Resume();
        }

        private void PauseScreenController_OnEnable(On.RoR2.UI.PauseScreenController.orig_OnEnable orig, RoR2.UI.PauseScreenController self)
        {
            orig(self);
            Music.Pause();
        }
    }
}