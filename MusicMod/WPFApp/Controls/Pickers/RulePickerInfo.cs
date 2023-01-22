using Rules.RuleTypes.Mutable;
using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Utils;
using WPFApp.Controls.Wrappers;

namespace WPFApp.Controls.Pickers
{
	internal class RulePickerInfo<TContext, TOut> : IPickerInfo
	{
		private readonly Func<Button> buttonGetter;

		public RulePickerInfo(NavigationContext navigationContext, Func<Button> buttonGetter)
		{
			NavigationContext = navigationContext ?? throw new ArgumentNullException(nameof(navigationContext));
			this.buttonGetter = buttonGetter;
		}

		public string DisplayMemberPath => nameof(Named<object>.Name);

		public string SelectedValuePath => nameof(Named<object>.Value);

		public NavigationContext NavigationContext { get; }

		public IControlWrapper CreateWrapper(object selectedInfo)
		{
			IControlWrapper output;
			Rule<TContext, TOut> rule;

			switch (selectedInfo)
			{
				case Type type:
					rule = Rule<TContext, TOut>.Create(type);
					break;

				case "paste":
					var item = NavigationContext.GetClipboardItem();

					if (item is null)
					{
						return null;
					}
					else
					{
						try
						{
							rule = Info.GetRuleParser<TContext, TOut>().Parse(item);
						}
						catch
						{
							return null;
						}
					}

					break;

				case Rule<TContext, TOut> r:
					rule = r;
					break;

				default:
					throw new NotSupportedException();
			}

			if (rule.GetType().IsGenericType(typeof(Bucket<,>)) && typeof(TOut) == typeof(string))
			{
				output = new BucketWrapper<TContext, string, TextBox>(new TextWrapper(new() { VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, MinWidth = 150, TextAlignment = TextAlignment.Center }));
			}
			else
			{
				output = new ItemButtonWrapper<Rule<TContext, TOut>>(buttonGetter());
			}

			output.SetValue(rule);

			return output;
		}

		public IEnumerable GetItems() => Info.SupportedRuleTypes.Select(t => (Named<object>)new TypeWrapper(t)).With(new Named<object>("Paste...", "paste"));

		IReadableControlWrapper IPickerInfo.CreateWrapper(object selectedInfo) => CreateWrapper(selectedInfo);
	}
}