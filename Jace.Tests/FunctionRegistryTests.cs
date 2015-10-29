using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public class FunctionRegistryTests
    {
        [TestMethod]
        public void TestAddFunc2()
        {
            ObjectRegistry registry = new ObjectRegistry(false);
            
            Func<double, double, double> testFunction = (a, b) => a * b;
            registry.RegisterFunction("test", testFunction);

            FunctionInfo functionInfo = (registry.GetObjectInfo("test") as FunctionInfo);
            
            Assert.IsNotNull(functionInfo);
            Assert.AreEqual("test", functionInfo.Name);
            Assert.AreEqual(2, functionInfo.NumberOfParameters);
            Assert.AreEqual(testFunction, functionInfo.Function);
        }

        [TestMethod]
        public void TestOverwritable()
        {
            ObjectRegistry registry = new ObjectRegistry(false);

            Func<double, double, double> testFunction1 = (a, b) => a * b;
            Func<double, double, double> testFunction2 = (a, b) => a * b;
            registry.RegisterFunction("test", testFunction1);
            registry.RegisterFunction("test", testFunction2);
        }

        [TestMethod]
        public void TestNotOverwritable()
        {
            ObjectRegistry registry = new ObjectRegistry(false);

            Func<double, double, double> testFunction1 = (a, b) => a * b;
            Func<double, double, double> testFunction2 = (a, b) => a * b;

            registry.RegisterFunction("test", testFunction1, false);

            AssertExtensions.ThrowsException<Exception>(() =>
                {
                    registry.RegisterFunction("test", testFunction2, false);
                });
        }
    }
}
