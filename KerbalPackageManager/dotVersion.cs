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
            if (jObj["VERSION"] != null)
            {
                string ver;
                try
                {
                    ver = (string)jObj["VERSION"]["MAJOR"];
                    if (jObj["VERSION"]["MINOR"] != null) ver += "." + jObj["VERSION"]["MINOR"];
                    if (jObj["VERSION"]["PATCH"] != null) ver += "." + jObj["VERSION"]["PATCH"];
                    if (jObj["VERSION"]["BUILD"] != null) ver += "." + jObj["VERSION"]["BUILD"];
                }
                catch { ver = (string)jObj["VERSION"]; }
                Version = new Version(ver);
            }
            else Version = null;
        }

        public string Name { get; set; }

        public Uri Url { get; set; }

        public Version Version { get; set; }
    }
}