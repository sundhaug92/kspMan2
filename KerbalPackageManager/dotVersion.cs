using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace KerbalPackageManager
{
    internal class dotVersion
    {
        public dotVersion(string toFilename)
        {
            var jObj = JObject.Parse(File.ReadAllText(toFilename));
            Name = (string)jObj["NAME"];
            Url = (Uri)jObj["URL"];
            var ver = jObj["VERSION"];
            Version = new Version((int)ver["MAJOR"], (int)ver["MINOR"], (int)ver["PATCH"], (int)ver["BUILD"]);
        }

        public string Name { get; set; }

        public Uri Url { get; set; }

        public Version Version { get; set; }
    }
}