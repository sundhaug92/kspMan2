using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace KerbalPackageManager
{
    public class Repository
    {
        public string Name { get; private set; }

        public string Maintainer { get; private set; }

        public Package[] Packages { get; private set; }

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
    }
}