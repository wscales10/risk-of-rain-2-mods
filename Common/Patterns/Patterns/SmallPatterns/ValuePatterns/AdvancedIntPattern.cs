using MathNet.Symbolics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Patterns.Patterns.SmallPatterns.ValuePatterns
{
	public class AdvancedIntPattern : StructValuePattern<int>
	{
		private SymbolicExpression symbolicExpression;

		protected override bool defineWith(string stringDefinition) => throw new NotImplementedException();

		protected override bool isMatch(int value) => throw new NotImplementedException();
	}
}