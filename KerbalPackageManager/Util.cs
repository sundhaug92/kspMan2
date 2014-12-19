using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KerbalPackageManager
{
    internal class Util
    {
        internal static void CopyProperties(object from, Package to)
        {
            Type type = from.GetType();
            foreach (var property in type.GetProperties())
            {
                property.SetValue(to, property.GetValue(from));
            }
        }
    }
}