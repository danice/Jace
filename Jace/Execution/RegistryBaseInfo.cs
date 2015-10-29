using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jace.Execution
{
    public abstract class RegistryBaseInfo
    {
        public RegistryBaseInfo(string name, bool isOverWritable)
        {
            this.Name = name;            
            this.IsOverWritable = isOverWritable;            
        }

        public string Name { get; private set; }

        public int NumberOfParameters { 
            get     {
                return GetNumberOfParameters();
            }
        }

        protected abstract int GetNumberOfParameters();

        public bool IsOverWritable { get; set; }
        
    }
}
