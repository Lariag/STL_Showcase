using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STL_Showcase.Data.DataObjects
{
    [Serializable]
    public class LinkedProgramData
    {
        public string ProgramName { get; set; }
        public string ProgramFullPath { get; set; }
        public bool SupportSTL { get; set; }
        public bool SupportOBJ { get; set; }
        public bool Support3MF { get; set; }
        public bool SupportDirectory { get; set; }
    }
}
