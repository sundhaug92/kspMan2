using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;

namespace KerbalPackageManager
{
    public class Repository
    {
        public string Name { get; set; }

        public string Maintainer { get; set; }

        public Package[] Packages { get; set; }

        public Repository(Uri uri)
        {
            var jString = (new WebClient()).DownloadString(uri.ToString());
            JObject jObj = JObject.Parse(jString);
            try { Name = jObj.GetValue("Name").ToObject<string>(); }
            catch (Exception e) { Debug.WriteLine(e.Message); }
            try { Maintainer = jObj.GetValue("Maintainer").ToObject<string>(); }
            catch (Exception e) { Debug.WriteLine(e.Message); }

            List<Package> pkgs = new List<Package>();
            foreach (var pkg in jObj.GetValue("Packages").Children())
            {
                pkgs.Add(new Package(pkg.ToObject<JObject>()));
            }
            Packages = pkgs.ToArray();
        }

        public Repository()
        {
            Packages = new Package[0];
        }

        public void AddPackage(Package pkg)
        {
            List<Package> pkgs = new List<Package>(Packages);
            pkgs.Add(pkg);
            Packages = pkgs.ToArray();
        }
    }
}