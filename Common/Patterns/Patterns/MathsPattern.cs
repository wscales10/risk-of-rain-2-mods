using System.Xml.Linq;
using NCalc;

namespace Patterns.Patterns
{
    public class MathsPattern<T> : Pattern<T>, IPattern<T?> where T : struct
    {
        public MathsPattern(string expressionString = "") => ExpressionString = expressionString;

        public string ExpressionString { get; set; }

        public override bool IsMatch(T value)
        {
            var expression = new Expression(ExpressionString);

            expression.Parameters["x"] = value;

            return (bool)expression.Evaluate();
        }

        public override XElement ToXml() => new XElement("Maths", ExpressionString);

        public override string ToString() => ExpressionString;

        public bool IsMatch(T? value) => value is T x && IsMatch(x);

        IPattern<T?> IPattern<T?>.Simplify() => this;
    }
}