using KerbalStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KerbalPackageManager
{
    internal class KerbalStuffRepository : Repository
    {
        public KerbalStuffRepository()
        {
            this.Maintainer = "KerbalStuff";
            Name = "KerbalStuff";
            this.UseStockCache = false;
        }

        public override Package SearchByName(string Name)
        {
            var mod = KerbalStuffReadOnly.ModSearch(Name).FirstOrDefault();
            return Package.FromKerbalStuff(mod.Id);
        }
    }
}