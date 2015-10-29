using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jace.Execution
{
    public class FunctionInfo : RegistryBaseInfo
    {
        int numberOfParameters;
        public FunctionInfo(string functionName, bool isOverWritable, Delegate function) : base(functionName, isOverWritable)
        {
            this.Function = function;

#if NETFX_CORE
            numberOfParameters = function.GetMethodInfo().GetParameters().Length;
#else
            numberOfParameters = function.Method.GetParameters().Length;
#endif

        }

        public Delegate Function { get; private set; }

        protected override int GetNumberOfParameters()
        {
            return numberOfParameters;
        }
    }
}
