using Rules;
using Rules.RuleTypes.Interfaces;
using Spotify.Commands;
using Utils;

namespace SpotifyControlWinForms.Units
{
	internal class MusicPicker<TCategory> : RuleUnit<TCategory, ICommandList>
	{
		private readonly string[] namesOfPropertiesWhichDoNotTriggerAnUpdate;

		public MusicPicker(string name, IRule<TCategory, ICommandList>? defaultRule, RuleParser<TCategory, ICommandList> ruleParser, params string[] namesOfPropertiesWhichDoNotTriggerAnUpdate) : base(name, ruleParser)
		{
			DefaultRule = defaultRule;
			this.namesOfPropertiesWhichDoNotTriggerAnUpdate = namesOfPropertiesWhichDoNotTriggerAnUpdate;
		}

		public override IRule<TCategory, ICommandList>? DefaultRule { get; }

		protected override ICommandList Transform(TCategory input)
		{
			this.Log($"[{DateTime.Now}] {nameof(Transform)}");
			if (input?.Equals(default) ?? true)
			{
				return new CommandList(new StopCommand()).ToReadOnly();
			}

			return base.Transform(input);
		}

		protected override void HandleInput(TCategory input, IList<string> changedPropertyNames)
		{
			if (!changedPropertyNames.All(name => namesOfPropertiesWhichDoNotTriggerAnUpdate.Contains(name)))
			{
				base.HandleInput(input, changedPropertyNames);
			}
		}
	}
}