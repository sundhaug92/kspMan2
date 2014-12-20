using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KerbalPackageManager
{
    public class Repository
    {
        public string Name { get; set; }

        public string Maintainer { get; set; }

        protected List<Package> Packages { get; set; }

        public DateTime LastSyncronized { get; set; }

        public string uri { get; set; }
        public override string ToString()
        {
            return Name;
        }
        public bool UseStockCache { get; set; }

        public virtual Package SearchByName(string PackageName) { throw new NotImplementedException(); }
    }
}
