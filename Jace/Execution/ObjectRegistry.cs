using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Jace.Execution
{
    public class ObjectRegistry : IObjectRegistry
    {
        private readonly bool caseSensitive;
        private readonly Dictionary<string, RegistryBaseInfo> objects;

        public ObjectRegistry(bool caseSensitive)
        {
            this.caseSensitive = caseSensitive;
            this.objects = new Dictionary<string, RegistryBaseInfo>();
        }

        public RegistryBaseInfo GetObjectInfo(string functionName)
        {
            if (string.IsNullOrEmpty(functionName))
                throw new ArgumentNullException("functionName");

            RegistryBaseInfo functionInfo = null;
            return objects.TryGetValue(ConvertFunctionName(functionName), out functionInfo) ? functionInfo : null;
        }


        public void Register(RegistryBaseInfo obj)
        {
            if (string.IsNullOrEmpty(obj.Name))
                throw new ArgumentNullException("Name");

            var objectName = ConvertFunctionName(obj.Name);

            if (objects.ContainsKey(objectName) && !objects[objectName].IsOverWritable)
            {
                string message = string.Format("The object \"{0}\" cannot be overwriten.", objectName);
                throw new Exception(message);
            }
            
            if (obj is FunctionInfo)
            {                
                CheckRegisterFunction((obj as FunctionInfo));
            }            

            if (objects.ContainsKey(objectName))
                objects[objectName] = obj;
            else
                objects.Add(objectName, obj);
                        
        }

        void CheckRegisterFunction(FunctionInfo functionInfo)
        {
            var function = functionInfo.Function;
            if (function == null)
                throw new ArgumentNullException("function");

            Type funcType = function.GetType();

            if (!funcType.FullName.StartsWith("System.Func"))
                throw new ArgumentException("Only System.Func delegates are permitted.", "function");

#if NETFX_CORE
            foreach (Type genericArgument in funcType.GenericTypeArguments)
#else
            foreach (Type genericArgument in funcType.GetGenericArguments())
#endif
                if (genericArgument != typeof(double))
                    throw new ArgumentException("Only doubles are supported as function arguments", "function");

#if NETFX_CORE
            int numberOfParameters = function.GetMethodInfo().GetParameters().Length;
#else
            int numberOfParameters = function.Method.GetParameters().Length;
#endif            
            if (objects.ContainsKey(functionInfo.Name) && objects[functionInfo.Name].NumberOfParameters != numberOfParameters)
            {
                string message = string.Format("The number of parameters cannot be changed when overwriting a method.");
                throw new Exception(message);
            }            

            
        }

        public void RegisterFunction(string functionName, Delegate function)
        {            
            RegisterFunction(functionName, function, true);
        }

        public void RegisterFunction(string functionName, Delegate function, bool isOverWritable)
        {            
            Register(new FunctionInfo(functionName, isOverWritable, function));
        }

        public void RegisterMatrix(string matrixName, int rows, int cols, params double[] values)
        {
            RegisterMatrix(matrixName, true, rows, cols, values);
        }

        public void RegisterMatrix(string matrixName, bool isOverWritable, int rows, int cols, params double[] values)
        {
            var matrixInfo = new MatrixInfo(matrixName, isOverWritable, rows, cols);
            matrixInfo.Values = values;
            Register(matrixInfo);
        }

        public bool IsObjectName(string functionName)
        {
            if (string.IsNullOrEmpty(functionName))
                throw new ArgumentNullException("functionName");

            return objects.ContainsKey(ConvertFunctionName(functionName));
        }

        private string ConvertFunctionName(string functionName)
        {
            return caseSensitive ? functionName : functionName.ToLowerInvariant();
        }

        public FunctionInfo GetFunctionInfo(string name)
        {
            return (GetObjectInfo(name) as FunctionInfo);
        }

        public MatrixInfo GetMatrixInfo(string name)
        {
            return (GetObjectInfo(name) as MatrixInfo);
        }
    }
}
