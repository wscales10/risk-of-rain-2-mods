using BepInEx;
using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Mutable;
using Spotify;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Utils;
using static Rules.Examples;

namespace Ror2Mod2
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.woodyscales.spotifyintegration", "Spotify Integration", "1.0.0")]
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInDependency("com.rune580.riskofoptions")]
    public class MusicMod : BaseUnityPlugin
    {
        private readonly Configuration configuration;

        private bool musicMuted;

        protected MusicMod()
        {
            configuration = new Configuration(Config);
        }

        public SpotifyController Music { get; private set; }

        private Logger SafeLogger => x => Logger.LogDebug(x ?? "null");

        public void Awake()
        {
            var rulePicker = new MutableRulePicker();
            var playlists = new List<Playlist>();
            SetRule(rulePicker, playlists);
            Music = new SpotifyController(rulePicker, playlists, SafeLogger);
            configuration.ConfigurationPageRequested += Music.OpenConfigurationPage;
            configuration.RuleLocationChanged += () => SetRule(rulePicker, playlists);

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

        private void SetRule(MutableRulePicker rulePicker, List<Playlist> playlists)
        {
            string uri = configuration.RuleLocation;

            if (uri is null)
            {
                throw new FileNotFoundException();
            }

            IRule rule;

            playlists.Clear();
            if (string.IsNullOrEmpty(uri))
            {
                rule = MimicRule;
            }
            else
            {
                this.Log($"Rule Location: {uri}");
                rule = Rule.FromXml(XElement.Load(uri));
                var playlistsFile = new FileInfo(uri).Directory.GetFiles("playlists.xml").FirstOrDefault();
                if (!(playlistsFile is null))
                {
                    var imported = XElement.Load(playlistsFile.FullName).Elements().Select(x => new Playlist(x));
                    if (!(imported is null))
                    {
                        playlists.AddRange(imported);
                    }
                }
            }

            rulePicker.Rule = rule;
        }

        private void PauseScreenController_OnDisable(On.RoR2.UI.PauseScreenController.orig_OnDisable orig, RoR2.UI.PauseScreenController self)
        {
            orig(self);

            if (RoR2.PlatformSystems.networkManager.isNetworkActive)
            {
                Music.Resume();
            }
        }

        private void PauseScreenController_OnEnable(On.RoR2.UI.PauseScreenController.orig_OnEnable orig, RoR2.UI.PauseScreenController self)
        {
            orig(self);
            Music.Pause();
        }
    }
}