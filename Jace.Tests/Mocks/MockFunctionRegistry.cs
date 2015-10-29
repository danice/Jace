using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jace.Execution;

namespace Jace.Tests.Mocks
{
    public class MockFunctionRegistry : IObjectRegistry
    {
        private HashSet<string> functionNames;

        public MockFunctionRegistry()
            : this(new string[] { "sin", "cos", "csc", "sec", "asin", "acos", "tan", "cot", "atan", "acot", "loge", "log10", "logn", "sqrt", "abs" })
        {
        }

        public MockFunctionRegistry(IEnumerable<string> functionNames)
        {
            this.functionNames = new HashSet<string>(functionNames);
        }

        public FunctionInfo GetObjectInfo(string functionName)
        {
            return new FunctionInfo(functionName, false, null);
        }

        public bool IsObjectName(string functionName)
        {
            return functionNames.Contains(functionName);
        }

        public void RegisterFunction(string functionName, Delegate function)
        {
            throw new NotImplementedException();
        }

        public void RegisterFunction(string functionName, Delegate function, bool isOverWritable)
        {
            throw new NotImplementedException();
        }

        public void RegisterFunction(string functionName, int numberOfParameters)
        {
            throw new NotImplementedException();
        }

        RegistryBaseInfo IObjectRegistry.GetObjectInfo(string functionName)
        {
            throw new NotImplementedException();
        }


        public void RegisterMatrix(string matrixName, int rows, int cols, params double[] values)
        {
            throw new NotImplementedException();
        }

        public void RegisterMatrix(string matrixName, bool isOverWritable, int rows, int cols, params double[] values)
        {
            throw new NotImplementedException();
        }


        public FunctionInfo GetFunctionInfo(string functionName)
        {
            throw new NotImplementedException();
        }

        public MatrixInfo GetMatrixInfo(string functionName)
        {
            throw new NotImplementedException();
        }
    }
}
