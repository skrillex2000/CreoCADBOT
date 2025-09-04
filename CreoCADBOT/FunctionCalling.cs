using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CreoCADBOT
{
    public class FunctionCalling
    {
        [StructLayout(LayoutKind.Sequential)]
        class Masterstruct
        {
            public string child;
            public string Id;
        }
        public static class allingClass1
        {
            public static string GetChildParts()
            {
                IntPtr data;
                int n_Comps;
                Creo.FetchChildParts(out data,out n_Comps);

                Masterstruct[] mans = new Masterstruct[n_Comps];
                IntPtr curr = data;

                for (int i = 0; i < n_Comps; i++)
                {
                    mans[i] = new Masterstruct();
                    mans[i] = (Masterstruct)Marshal.PtrToStructure(curr, typeof(Masterstruct));
                    Marshal.DestroyStructure(curr, typeof(Masterstruct));
                    curr = (IntPtr)((long)curr + Marshal.SizeOf(mans[i]));
                }
                Marshal.FreeCoTaskMem(data);

                string childParts = string.Empty;
                foreach (Masterstruct m in mans)
                {
                    childParts += m.child + ',';
                }
                foreach (Masterstruct m in mans)
                {
                    GlobalVariable.Masterstruct str = new GlobalVariable.Masterstruct();
                    str.child = m.child;
                    str.Id = m.Id;
                    GlobalVariable.childrens.Add(str);
                }
                DataTable table = new DataTable();
                table.Columns.Add("ChildPart");
                table.Columns.Add("ID");

                foreach (Masterstruct m in mans)
                {
                    table.Rows.Add(m.child, m.Id);
                }
                return $"The child parts are {childParts}";
            }

        }
       
    }
}
