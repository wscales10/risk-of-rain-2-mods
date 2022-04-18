using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Utils;

namespace Patterns
{
	public class Switch<T, TOut> : IXmlExportable<TOut>
	{
		private readonly List<Case<IPattern<T>, TOut>> cases;

		public Switch(params Case<IPattern<T>, TOut>[] cases)
		{
			this.cases = cases.ToList();
		}

		public Switch(TOut defaultOut, params Case<IPattern<T>, TOut>[] cases) : this(cases)
		{
			DefaultOut = defaultOut;
			HasDefault = true;
		}

		public bool HasDefault { get; }

		public TOut DefaultOut { get; }

		public ReadOnlyCollection<Case<IPattern<T>, TOut>> Cases => cases.ToReadOnlyCollection();

		public static Switch<T, TOut> Parse(XElement element, Func<XElement, TOut> outFunc, PatternParser patternParser)
		{
			if (element.Name != "Switch")
			{
				throw new XmlException();
			}

			var cases = new List<Case<IPattern<T>, TOut>>();
			var patterns = new List<IPattern<T>>();
			XElement defaultElement = null;
			foreach (var child in element.Elements())
			{
				switch (child.Name.ToString())
				{
					case "Case":
						patterns.Add(patternParser.Parse<T>(child.OnlyChild()));
						break;

					case "Output":
						cases.Add(new Case<IPattern<T>, TOut>(outFunc(child.OnlyChild()), patterns.ToArray()));
						patterns.Clear();
						break;

					case "Default" when defaultElement is null:
						defaultElement = child;
						break;

					default:
						throw new XmlException();
				}
			}

			if (defaultElement is null)
			{
				return new Switch<T, TOut>(cases.ToArray());
			}
			else
			{
				return new Switch<T, TOut>(outFunc(defaultElement.OnlyChild()), cases.ToArray());
			}
		}

		public TOut GetOut(T seenValue)
		{
			foreach (var @case in cases)
			{
				if (@case.Arr.Any(allowedValue => allowedValue.IsMatch(seenValue)))
				{
					return @case.Output;
				}
			}

			return DefaultOut;
		}

		public XElement ToXml(Func<TOut, XElement> outFunc)
		{
			var switchElement = new XElement("Switch");

			foreach (var @case in cases)
			{
				foreach (var input in @case.Arr)
				{
					switchElement.Add(new XElement("Case", input.ToXml()));
				}

				switchElement.Add(new XElement("Output", outFunc(@case.Output)));
			}

			if (HasDefault)
			{
				switchElement.Add(new XElement("Default", outFunc(DefaultOut)));
			}

			return switchElement;
		}

		public Switch<T, TOut2> Select<TOut2>(Func<TOut, TOut2> func)
		{
			return new Switch<T, TOut2>(func(DefaultOut), cases.Select(c => new Case<IPattern<T>, TOut2>(func(c.Output), c.Arr)).ToArray());
		}
	}
}