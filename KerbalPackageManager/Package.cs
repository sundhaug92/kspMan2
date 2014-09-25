using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;

namespace KerbalPackageManager
{
    public class Package
    {
        internal Package(JObject pgkInfo)
        {
            Name = pgkInfo.GetValue("Name").ToObject<string>();
            Maintainer = pgkInfo.GetValue("Maintainer").ToObject<string>();
            LicenceUri = pgkInfo.GetValue("LicenceUri").ToObject<Uri>();
            ForumThreadUri = pgkInfo.GetValue("ForumThreadUri").ToObject<Uri>();
            Version = pgkInfo.GetValue("Version").ToObject<string>();
            DownloadUri = pgkInfo.GetValue("DownloadUri").ToObject<Uri>();

            List<UnresolvedPackage> deps = new List<UnresolvedPackage>();
            foreach (var pkg in pgkInfo.GetValue("Dependencies").ToList())
            {
                deps.Add(new UnresolvedPackage(pkg.ToObject<string>()));
            }
        }

        public string Name { get; private set; }

        public string Maintainer { get; private set; }

        public Uri LicenceUri { get; private set; }

        public string Version { get; private set; }

        public Uri ForumThreadUri { get; private set; }

        public Uri DownloadUri { get; private set; }

        public UnresolvedPackage[] Dependencies { get; private set; }

        public void DownloadAndInstall()
        {
            Console.WriteLine("Checking installation state of {0}", Name);
            if (Manager.SameOrNewerInstalled(this)) return;
            Console.WriteLine("Checking dependencies for {0} and installing them", Name);
            if (Dependencies != null) foreach (UnresolvedPackage dependency in Dependencies)
                {
                    dependency.Resolve().DownloadAndInstall();
                }
            else Console.WriteLine("No dependencies");

            string kspDirectory = Manager.GetConfigString("kspDirectory");
            string targetDirectory = null;
            if (this.InstallTarget == InstallTarget.GameData) targetDirectory = kspDirectory + "GameData\\";
            else if (this.InstallTarget == InstallTarget.Main) targetDirectory = kspDirectory;

            Console.WriteLine("Downloading {0}", Name);
            new WebClient().DownloadFile(DownloadUri, ".kpm\\tmp.zip");
            Console.WriteLine("Unzipping {0}", Name);
            var zip = ZipFile.Open(".kpm\\tmp.zip", ZipArchiveMode.Read);
            foreach (ZipArchiveEntry file in zip.Entries)
            {
                Console.WriteLine("File> {0}", file.FullName);
                if (file.FullName.EndsWith("/"))
                {
                    if (!Directory.Exists(targetDirectory + file.FullName)) Directory.CreateDirectory(targetDirectory + file.FullName);
                }
                else file.ExtractToFile(targetDirectory + file.FullName, true);
            }
            Console.WriteLine("Recording installation");
            Manager.InstalledPackages.Add(this);
        }

        private InstallTarget InstallTarget;
    }

    public enum InstallTarget
    {
        Main, GameData
    }
}