using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Maths.Tests
{
    [TestClass]
    public class ThatMethods
    {
        [TestMethod]
        public void DividesIntegersAsDoubles()
        {
            var actualResult = Methods.Evaluate("1 / 2", "ms", 0);
            Assert.AreEqual(0.5, actualResult);
        }

        [TestMethod]
        public void CanEvaluateIntegerConstantExpressions()
        {
            var actualResult = Methods.Evaluate("0", "ms", 0);
            Assert.AreEqual(0, actualResult);
        }
    }
}