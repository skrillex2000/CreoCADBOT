using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreoCADBOT
{
    public class GlobalVariable
    {
        public class Masterstruct
        {
            public string child;
            public string Id;
        }

        public static List<Masterstruct> childrens = new List<Masterstruct>();
    }
}
