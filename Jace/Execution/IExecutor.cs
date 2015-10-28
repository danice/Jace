using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jace.Operations;

namespace Jace.Execution
{
    public interface IExecutor
    {
        double Execute(Operation operation, IObjectRegistry functionRegistry);
        double Execute(Operation operation, IObjectRegistry functionRegistry, IDictionary<string, double> variables);

        Func<IDictionary<string, double>, double> BuildFormula(Operation operation, IObjectRegistry functionRegistry);
    }
}
