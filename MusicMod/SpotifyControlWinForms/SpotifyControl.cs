using Rules;
using Rules.RuleTypes.Interfaces;
using SpotifyControlWinForms.Properties;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Xml.Linq;
using Utils;
using Utils.Properties;
using RuleExamples;
using Spotify.Commands;
using SpotifyControlWinForms.Units;
using SpotifyControlWinForms.Connections;

namespace SpotifyControlWinForms
{
    public class ConcreteKeyedCollection<TKey, TItem> : KeyedCollection<TKey, TItem> where TKey : notnull
    {
        private readonly Func<TItem, TKey> getKeyForItem;

        public ConcreteKeyedCollection(Func<TItem, TKey> getKeyForItem)
        {
            this.getKeyForItem = getKeyForItem ?? throw new ArgumentNullException(nameof(getKeyForItem));
        }

        protected override TKey GetKeyForItem(TItem item)
        {
            return getKeyForItem(item);
        }
    }

    public class SpotifyControl
    {
        private readonly ConcreteKeyedCollection<string, UnitBase> units;

        private readonly SetProperty<MusicPicker<string>?> activeMusicPicker = new();

        public SpotifyControl()
        {
            var riskOfRain2MusicPicker = new MusicPicker<string>("RoR2MusicPicker", RuleExamples.RiskOfRain2.MimicRule.Instance, RuleParsers.StringToSpotify).Init(GetLocation);
            var minecraftMusicPicker = new MusicPicker<string>("MinecraftMusicPicker", RuleExamples.Minecraft.MimicRule.Instance, RuleParsers.StringToSpotify).Init(GetLocation);

            units = new(u => u.Name)
            {
                RoR2Categoriser.Instance.Init(GetLocation),
                MinecraftCategoriser.Instance.Init(GetLocation),
                riskOfRain2MusicPicker,
                minecraftMusicPicker,
                OverwatchMithrixHandler.Instance
            };

            Units = new(units);

            units.OfType<MusicPicker<string>>().ForEach(p => p.IsEnabledChanging += MusicPicker_IsEnabledChanging);
            units.OfType<MusicPicker<string>>().ForEach(p => p.IsEnabledChanged += MusicPicker_IsEnabledChanged);
            units.OfType<MusicPicker<string>>().ForEach(p => p.CanToggleIsEnabledEvent += MusicPicker_CanToggleIsEnabledEvent);
            units.OfType<MusicPicker<string>>().ForEach(p => p.Trigger += MusicPicker_Trigger);

            var riskOfRain2Connection = new RiskOfRain2Connection(new IPC.Client(5008, nameof(RiskOfRain2Connection)));
            riskOfRain2Connection.Output += RoR2Categoriser.Instance.Ingest;
            riskOfRain2Connection.ConnectionAttempted += Connection_ConnectionAttempted;
            var minecraftConnection = new MinecraftConnection(new IPC.Client(5009, nameof(MinecraftConnection)));
            minecraftConnection.Output += MinecraftCategoriser.Instance.Ingest;
            minecraftConnection.ConnectionAttempted += Connection_ConnectionAttempted;
            var overwatchConnection = new OverwatchConnection(SelectWorkshopLogFolder, s => MessageBox.Show(s));
            overwatchConnection.Output += OverwatchMithrixHandler.Instance.Ingest;
            overwatchConnection.ConnectionAttempted += Connection_ConnectionAttempted;
            Connections = new(new ConnectionBase[] { MusicConnection.Instance, riskOfRain2Connection, minecraftConnection, overwatchConnection });
        }

        public ReadOnlyCollection<UnitBase> Units { get; }

        public ReadOnlyCollection<ConnectionBase> Connections { get; }

        internal static StringCollection RuleLocations => Settings.Default.RuleLocations ??= new();

        internal static void SetRule<TIn, TOut>(MutableRulePicker<TIn, TOut> rulePicker, string? uri, RuleParser<TIn, TOut> ruleParser, IRule<TIn, TOut>? defaultRule = null)
        {
            IRule<TIn, TOut>? rule;

            if (string.IsNullOrEmpty(uri))
            {
                rule = defaultRule;
            }
            else
            {
                rulePicker.Log($"Rule Location: {uri}");
                rule = ruleParser.Parse(XElement.Load(uri));
            }

            if (rule is not null)
            {
                rulePicker.Rule = rule;
            }
        }

        internal static string? GetLocation(IRuleUnit unit)
        {
            var locations = RuleLocations;

            for (int i = 0; i < locations.Count; i += 2)
            {
                if (locations[i] == unit.Name)
                {
                    return locations[i + 1];
                }
            }

            locations.Add(unit.Name);
            locations.Add(null);
            return null;
        }

        internal static void SetLocation(IRuleUnit unit, string? location)
        {
            var locations = RuleLocations;

            for (int i = 0; i < locations.Count; i += 2)
            {
                if (locations[i] == unit.Name)
                {
                    locations[i + 1] = location;
                    return;
                }
            }

            locations.Add(unit.Name);
            locations.Add(location);
        }

        internal void Init()
        {
            RoR2Categoriser.Instance.Trigger += Unit_Trigger;
            MinecraftCategoriser.Instance.Trigger += Unit_Trigger;
            OverwatchMithrixHandler.Instance.Trigger += OverwatchMithrixHandler_Trigger;
            activeMusicPicker.OnSet += ActiveMusicPicker_OnSet;

            // TODO: set active / enabled
        }

        // TODO: this should depend on whether a music unit is connected for this game - will be tricky to implement!
        private static void Connection_ConnectionAttempted(ConnectionBase sender, bool result)
        {
            if (result)
            {
                (sender as IpcConnection)?.SendMessage("mute");
            }
        }

        private void OverwatchMithrixHandler_Trigger(UnitUpdateInfo<MyRoR2.RoR2Context?> obj)
        {
            if (obj.Output is not null)
            {
                RoR2Categoriser.Instance.Ingest(obj.Output.Value);
            }
        }

        private ConditionalValue<string> SelectWorkshopLogFolder()
        {
            string settingValue = Settings.Default.WorkshopLogFolder;

            if (string.IsNullOrWhiteSpace(settingValue) || !Directory.Exists(settingValue))
            {
                var dialog = new FolderBrowserDialog() { Description = "Select Workshop Folder", ShowNewFolderButton = false };
                switch (dialog.ShowDialog())
                {
                    case DialogResult.OK:
                        Settings.Default.WorkshopLogFolder = settingValue = dialog.SelectedPath;
                        Settings.Default.Save();
                        break;

                    default:
                        return new();
                }
            }

            return new(settingValue);
        }

        private bool MusicPicker_CanToggleIsEnabledEvent(UnitBase arg1)
        {
            if (arg1.IsEnabled)
            {
                return true;
            }
            else
            {
                return !units.OfType<MusicPicker<string>>().Except(new[] { arg1 }).Any(u => u.IsEnabled);
            }
        }

        private void MusicPicker_Trigger(UnitUpdateInfo<ICommandList> obj)
        {
            MusicConnection.Instance.Music.Play(obj.Output);
        }

        private void MusicPicker_IsEnabledChanged(object source, bool newValue)
        {
            activeMusicPicker.Set(units.OfType<MusicPicker<string>>().SingleOrDefault(x => x.IsEnabled));
        }

        private bool MusicPicker_IsEnabledChanging(bool arg)
        {
            return !arg || units.OfType<MusicPicker<string>>().All(x => !x.IsEnabled);
        }

        private void ActiveMusicPicker_OnSet(MusicPicker<string>? oldValue, MusicPicker<string>? newValue)
        {
            MusicConnection.Instance.SetPlaylists(newValue is null ? null : GetLocation(newValue));

            foreach (var musicPicker in units.OfType<MusicPicker<string>>())
            {
                musicPicker.UpdateIsEnabledTogglability();
            }
        }

        private void Unit_Trigger(UnitUpdateInfo<string> obj)
        {
            this.Log($"[{DateTime.Now}] {nameof(Unit_Trigger)}");
            activeMusicPicker.Get()?.Ingest(obj.Output);
        }
    }
}