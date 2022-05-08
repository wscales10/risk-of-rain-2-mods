using NCalc;

namespace Maths
{
    public static class Methods
    {
        public static double Evaluate(string function, string variable, double value)
        {
            var expression = new Expression(function);
            expression.Parameters[variable] = value;
            return (double)expression.Evaluate();
        }
    }
}