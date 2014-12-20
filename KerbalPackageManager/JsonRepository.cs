using KerbalStuff;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;

namespace KerbalPackageManager
{
    public class JsonRepository : Repository
    {

        public JsonRepository(string uri)
        {
            this.UseStockCache = true;
            var jString = (new WebClient()).DownloadString(uri);
            JObject jObj = JObject.Parse(jString);
            this.uri = uri.ToString();
            try { Name = jObj.GetValue("Name").ToObject<string>(); }
            catch (Exception e) { Debug.WriteLine(e.Message); }
            try { Maintainer = jObj.GetValue("Maintainer").ToObject<string>(); }
            catch (Exception e) { Debug.WriteLine(e.Message); }
            try
            {
                LastSyncronized = DateTime.Parse(jObj.GetValue("Maintainer").ToObject<string>());
            }
            catch (Exception e) { Debug.WriteLine(e.Message); }

            List<Package> pkgs = new List<Package>();
            foreach (var pkg in jObj.GetValue("Packages").Children())
            {
                pkgs.Add(new Package(pkg.ToObject<JObject>()));
            }
            Packages = pkgs;
            LastSyncronized = DateTime.Now;
        }

        public JsonRepository()
        {
            Packages = new List<Package>();
        }

        public void AddPackage(Package pkg)
        {
            Packages.Add(pkg);
        }
        public override Package SearchByName(string PackageName)
        {
            return (from package in this.Packages where package.Name == PackageName select package).FirstOrDefault();
        }
    }
}