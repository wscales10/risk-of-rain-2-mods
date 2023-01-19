using MyRoR2;
using Newtonsoft.Json;
using Ror2Mod2;
using Rules;
using Rules.Examples;
using Rules.RuleTypes.Interfaces;
using Spotify;
using Spotify.Commands;
using SpotifyControlWinForms.Properties;
using System.Xml.Linq;
using Utils;

namespace SpotifyControlWinForms
{
	public abstract class Unit<TIn, TOut>
	{
		public event Action<TOut>? Trigger;

		public abstract void Ingest(TIn input);

		protected void Output(TOut output) => Trigger?.Invoke(output);
	}

	public class Unit1<TIn, TOut> : Unit<TIn, TOut>
	{
		private readonly IRulePicker<TIn, TOut> rulePicker;

		private TIn? cached;

		public Unit1(IRulePicker<TIn, TOut> rulePicker)
		{
			this.rulePicker = rulePicker;
		}

		public override void Ingest(TIn input)
		{
			var output = rulePicker.Rule.GetOutput(cached, input);
			cached = input;
			Output(output);
		}
	}

	internal class SpotifyControl
	{
		private readonly Unit1<Context, string> unit1;

		public SpotifyControl()
		{
			Client = new IPC.Client(5008);
			Client.ReceivedRequest += Client_ReceivedRequest;
			Client.TryStart.CreateRun().Run(update =>
			{
				this.Log(update.Args);
				return true;
			});

			var rulePicker1 = new MutableRulePicker<Context, string>();
			unit1 = new(rulePicker1);
			unit1.Trigger += Unit1_Trigger;
			SetRule(rulePicker1, Settings.Default.Rule1Location, RuleParser.RoR2ToString, Ror2Rule.Instance);

			var rulePicker2 = new MutableRulePicker<string, ICommandList>();
			var playlists = new List<Playlist>();
			SetRule(rulePicker2, Settings.Default.Rule2Location, RuleParser.StringToSpotify, MimicRule.Instance);
			SetPlaylists(playlists, Settings.Default.Rule2Location);
			Music = new(rulePicker2, playlists, x => this.Log(x));
			Music.ConnectionUpdateHandler += Music_ConnectionUpdateHandler;
			Form.SetConnectionStatus(Music.TryInit());
			Form.TryReconnect += () => Form.SetConnectionStatus(Music.TryInit());

			Form.Rule1LocationChanged += s => SetRule(rulePicker1, Settings.Default.Rule1Location, RuleParser.RoR2ToString);

			Form.Rule2LocationChanged += s => SetRule(rulePicker2, Settings.Default.Rule2Location, RuleParser.StringToSpotify);
		}

		public Form1 Form { get; } = new();

		public SpotifyController<string> Music { get; private set; }

		public IPC.Client Client { get; private set; }

		private bool Music_ConnectionUpdateHandler(Utils.Coroutines.ProgressUpdate arg)
		{
			if (arg.Args is Exception e)
			{
				MessageBox.Show(e.Message, e.GetType().GetDisplayName());
				return false;
			}

			return true;
		}

		private void Unit1_Trigger(string obj) => _ = Music.Update(obj);

		private void SetPlaylists(List<Playlist> playlists, string uri)
		{
			playlists.Clear();
			if (!string.IsNullOrEmpty(uri))
			{
				this.Log($"Playlists Location: {uri}");
				var playlistsFile = new FileInfo(uri).Directory?.GetFiles("playlists.xml").FirstOrDefault();
				if (playlistsFile is not null)
				{
					var imported = XElement.Load(playlistsFile.FullName).Elements().Select(x => new Playlist(x));
					if (imported is not null)
					{
						playlists.AddRange(imported);
					}
				}
			}
		}

		private void SetRule<TIn, TOut>(MutableRulePicker<TIn, TOut> rulePicker, string uri, RuleParser<TIn, TOut> ruleParser, IRule<TIn, TOut>? defaultRule = null)
		{
			if (uri is null)
			{
				throw new FileNotFoundException();
			}

			IRule<TIn, TOut>? rule;

			if (string.IsNullOrEmpty(uri))
			{
				rule = defaultRule;
			}
			else
			{
				this.Log($"Rule Location: {uri}");
				rule = ruleParser.Parse(XElement.Load(uri));
			}

			if (rule is not null)
			{
				rulePicker.Rule = rule;
			}
		}

		private IEnumerable<IPC.Message> Client_ReceivedRequest(IEnumerable<IPC.Message> arg)
		{
			foreach (var message in arg)
			{
				switch (message.Key)
				{
					case nameof(Context):
						unit1.Ingest(JsonConvert.DeserializeObject<Context>(message.Value));
						break;

					case "pause":
						Music.Pause();
						break;

					case "resume":
						Music.Resume();
						break;

					default:
						throw new NotSupportedException($"message key {message.Key} not supported");
				}
			}

			yield break;
		}
	}
}