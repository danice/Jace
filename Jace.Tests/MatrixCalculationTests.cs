using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Jace.Operations;
using Jace.Execution;

#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#elif __ANDROID__
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace Jace.Tests
{
    [TestClass]
    public class MatrixCalculationTests
    {        

        [TestMethod]
        public void TestMatrixGetItemInterpreted()
        {
            CalculationEngine engine = new CalculationEngine(CultureInfo.InvariantCulture,
                ExecutionMode.Interpreted, false, false);            
            engine.AddMatrix("M", 3, 3, 1, 2, 3, 4, 5, 6, 7, 8, 9);            

            double result = engine.Calculate("M(2,3)");
            Assert.AreEqual(6.0, result);
        }

        [TestMethod]
        public void TestMatrixGetItemCompiled()
        {
            CalculationEngine engine = new CalculationEngine(CultureInfo.InvariantCulture,
                ExecutionMode.Compiled, false, false);
            engine.AddMatrix("M", 3, 3, 1, 2, 3, 4, 5, 6, 7, 8, 9);            

            double result = engine.Calculate("M(2,3)");
            Assert.AreEqual(6.0, result);
        }

        [TestMethod]
        public void TestMatrixGetItemWitFunctionCompiled()
        {
            CalculationEngine engine = new CalculationEngine(CultureInfo.InvariantCulture,
                ExecutionMode.Compiled, false, false);
            engine.AddMatrix("M", 3, 3, 1, 2, 3, 4, 5, 6, 7, 8, 9);            

            double result = engine.Calculate("M(1,1+2)");
            Assert.AreEqual(3.0, result);
        }

        [TestMethod]
        public void TestMatrixGetItemWitVariablesCompiled()
        {
            //arrange
            CalculationEngine engine = new CalculationEngine(CultureInfo.InvariantCulture,
                ExecutionMode.Compiled, false, false);
            engine.AddMatrix("M", 3, 3, 1, 2, 3, 4, 5, 6, 7, 8, 9);
            
            //act
            double result = engine.Calculate("M(a,b)", new Dictionary<string, double>    {
                { "a", 2 },
                { "b", 2 }
            });


            //assert
            Assert.AreEqual(5.0, result);
        }

        [TestMethod]
        public void TestMatrixGetItemReturnCompiled()
        {
            //arrange
            CalculationEngine engine = new CalculationEngine(CultureInfo.InvariantCulture,
                ExecutionMode.Compiled, false, false);
            engine.AddMatrix("M", 3, 3, 1, 2, 3, 4, 5, 6, 7, 8, 9);
            engine.AddMatrix("N", 3, 3, 1, 2, 3, 4, 5, 6, 7, 8, 9);

            //act
            double result = engine.Calculate("M(1,2)+N(2,2)");


            //assert
            Assert.AreEqual(7.0, result);
        }

        
    }
}
