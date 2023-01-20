﻿using Rules;
using Rules.RuleTypes.Mutable;
using System;
using System.Collections;
using System.Linq;
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
			var output = new ItemButtonWrapper<Rule<TContext, TOut>>(buttonGetter());

			switch (selectedInfo)
			{
				case Type type:
					output.SetValue(Rule<TContext, TOut>.Create(type));
					break;

				case "paste":
					var item = NavigationContext.GetClipboardItem();

					if (item is null)
					{
						return null;
					}
					else
					{
						Rule<TContext, TOut> rule;
						try
						{
							rule = (Rule<TContext, TOut>)Info.GetRuleParser<TContext, TOut>().Parse(item);
						}
						catch
						{
							return null;
						}

						output.SetValue(rule);
					}

					break;
			}

			return output;
		}

		public IEnumerable GetItems() => Info.SupportedRuleTypes.Select(t => (Named<object>)new TypeWrapper(t)).With(new Named<object>("Paste...", "paste"));

		IReadableControlWrapper IPickerInfo.CreateWrapper(object selectedInfo) => CreateWrapper(selectedInfo);
	}
}