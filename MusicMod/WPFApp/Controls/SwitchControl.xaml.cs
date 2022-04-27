using Patterns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;
using Utils;
using WPFApp.Controls.Wrappers;

namespace WPFApp.Controls
{
	/// <summary>
	/// Interaction logic for SwitchControl.xaml
	/// </summary>
	public partial class SwitchControl : UserControl
	{
		public SwitchControl() => InitializeComponent();

		public void SetValue<T, TOut>(Switch<T, TOut> value, Func<IPattern<T>, string> stringifier1, Func<TOut, string> stringifier2)
		{
			var rows = rowGroup.Rows;
			rows.Clear();

			foreach (var c in value.Cases)
			{
				var row = new TableRow();
				var resultCell = MakeTableCell(stringifier2(c.Output));
				row.Cells.Add(resultCell);
				var cell2 = MakeTableCell(", ");
				row.Cells.Add(cell2);
				var conditionCell = MakeTableCell(stringifier1(c.Arr.Single()));
				row.Cells.Add(conditionCell);
				rows.Add(row);
			}

			if (value.HasDefault)
			{
				var row = new TableRow();
				var resultCell = MakeTableCell(stringifier2(value.DefaultOut));
				row.Cells.Add(resultCell);
				var cell2 = MakeTableCell(", ");
				row.Cells.Add(cell2);
				var conditionCell = MakeTableCell("else");
				row.Cells.Add(conditionCell);
				rows.Add(row);
			}
		}

		public bool TryGetValue<T, TOut>(Parser<TOut> parser2, out Switch<T, TOut> output)
		{
			List<Case<IPattern<T>, TOut>> cases = new();
			TOut defaultOut = default;
			bool hasDefault = false;

			foreach (var row in rowGroup.Rows)
			{
				if (!parser2(GetCellContent(row.Cells[0]), out var result))
				{
					output = null;
					return false;
				}

				string condition = GetCellContent(row.Cells[2]);

				if (condition == "else")
				{
					if (hasDefault)
					{
						this.Log("already has default");
					}

					defaultOut = result;
					hasDefault = true;
				}
				else
				{
					if (!Info.PatternParser.TryParse<T>(condition, out var pattern))
					{
						output = null;
						return false;
					}

					cases.Add(new(result, pattern));
				}
			}

			output = hasDefault
				? new Switch<T, TOut>(defaultOut, cases.ToArray())
				: new Switch<T, TOut>(cases.ToArray());
			return true;
		}

		private static TableCell MakeTableCell(string content) => new(new Paragraph(new Run(content)));

		private static string GetCellContent(TableCell tableCell)
		{
			var runs = ((Paragraph)tableCell.Blocks.Single()).Inlines.Cast<Run>();
			return string.Concat(runs.Select(r => r.Text));
		}
	}
}