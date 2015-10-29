using System;

namespace Jace.Execution
{
    public interface IObjectRegistry
    {
        RegistryBaseInfo GetObjectInfo(string functionName);
        FunctionInfo GetFunctionInfo(string functionName);
        MatrixInfo GetMatrixInfo(string functionName);
        bool IsObjectName(string functionName);        
        void RegisterFunction(string functionName, Delegate function);
        void RegisterFunction(string functionName, Delegate function, bool isOverWritable);
        void RegisterMatrix(string matrixName, int rows, int cols, params double[] values);
        void RegisterMatrix(string matrixName, bool isOverWritable, int rows, int cols, params double[] values);
    }
}
