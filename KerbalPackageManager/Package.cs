﻿using KerbalStuff;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;

namespace KerbalPackageManager
{
    public class Package
    {
        public Package(string Name, string Maintainer, Uri LicenceUri, string Version, Uri ForumThreadUri, Uri DownloadUri, UnresolvedPackage[] Dependencies, InstallTarget InstallTarget)
        {
            this.Name = Name; this.Maintainer = Maintainer; this.LicenceUri = LicenceUri; this.Version = Version; this.ForumThreadUri = ForumThreadUri; this.DownloadUri = DownloadUri; this.Dependencies = Dependencies; this.InstallTarget = InstallTarget;
        }

        internal Package(JObject pgkInfo)
        {
            Name = pgkInfo.GetValue("Name").ToObject<string>();
            Maintainer = pgkInfo.GetValue("Maintainer").ToObject<string>();
            LicenceUri = pgkInfo.GetValue("LicenceUri").ToObject<Uri>();
            ForumThreadUri = pgkInfo.GetValue("ForumThreadUri").ToObject<Uri>();
            Version = pgkInfo.GetValue("Version").ToObject<string>();
            DownloadUri = pgkInfo.GetValue("DownloadUri").ToObject<Uri>();
            InstallTarget = pgkInfo.GetValue("InstallTarget").ToObject<InstallTarget>();
            List<UnresolvedPackage> deps = new List<UnresolvedPackage>();
            foreach (var pkg in pgkInfo.GetValue("Dependencies").ToList())
            {
                deps.Add(new UnresolvedPackage(pkg.Value<string>("Name")));
            }
            Dependencies = deps.ToArray();
            OneDirLevelUp = pgkInfo.GetValue("OneDirLevelUp").ToObject<bool>();
            try
            {
                KerbalStuffId = pgkInfo.GetValue("KerbalStuffId").ToObject<long>();
            }
            catch (Exception e) { }

            //Weird reflection-hack
            if (KerbalStuffId > 0)
            {
                var pkg = Package.FromKerbalStuff(KerbalStuffId);
                Util.CopyProperties(pkg, this);
            }
        }

        public string Name { get; private set; }

        public string Maintainer { get; private set; }

        public Uri LicenceUri { get; private set; }

        public string Version { get; private set; }

        public Uri ForumThreadUri { get; private set; }

        public Uri DownloadUri { get; private set; }

        public UnresolvedPackage[] Dependencies { get; private set; }

        public Package[] ContainsPackage { get; private set; }

        public bool OneDirLevelUp { get; private set; }

        private InstallTarget InstallTarget;

        public long KerbalStuffId { get; private set; }

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
            if (!kspDirectory.EndsWith("\\")) kspDirectory += "\\";
            string targetDirectory = null;
            if (this.InstallTarget == InstallTarget.GameData) targetDirectory = kspDirectory + "GameData\\";
            else if (this.InstallTarget == InstallTarget.Main) targetDirectory = kspDirectory;

            Console.WriteLine("Downloading {0}", Name);
            new WebClient().DownloadFile(DownloadUri, ".kpm\\tmp.zip");
            Console.WriteLine("Unzipping {0}", Name);
            using (var zip = ZipFile.Open(".kpm\\tmp.zip", ZipArchiveMode.Read))
            {
                if (InstallTarget == InstallTarget.Unknown)
                {
                    bool hasGameData = false;
                    foreach (ZipArchiveEntry file in zip.Entries)
                    {
                        if (file.FullName.ToLower().Contains("gamedata"))
                        {
                            if (!file.FullName.ToLower().StartsWith("gamedata")) OneDirLevelUp = true;
                            hasGameData = true;
                        }
                    }
                    if (hasGameData) targetDirectory = kspDirectory;
                    else targetDirectory = kspDirectory + "GameData\\";
                }
                foreach (ZipArchiveEntry file in zip.Entries)
                {
                    string toFilename = file.FullName;
                    if (OneDirLevelUp && toFilename.Contains("/")) toFilename = toFilename.Substring(toFilename.IndexOf("/") + 1);
                    if (!toFilename.Contains('/')) continue;

                    Console.WriteLine("File> {0}, {1}", file.FullName, targetDirectory + toFilename);
                    if (toFilename.EndsWith("/"))
                    {
                        if (!Directory.Exists(targetDirectory + toFilename)) Directory.CreateDirectory(targetDirectory + toFilename);
                    }
                    else
                    {
                        if (toFilename.Contains('/') && !Directory.Exists(targetDirectory + toFilename.Substring(0, toFilename.LastIndexOf('/')))) Directory.CreateDirectory(targetDirectory + toFilename.Substring(0, toFilename.LastIndexOf('/')));
                        file.ExtractToFile(targetDirectory + toFilename, true);
                    }
                }
            }
            Console.WriteLine("Recording installation");
            Manager.InstalledPackages.Add(this);
        }

        public override string ToString()
        {
            return Name;
        }

        public static Package FromKerbalStuff(long ksId)
        {
            var mod = KerbalStuffReadOnly.ModInfo(ksId);
            var package = new Package(mod.Name, mod.Author, new Uri("http://example.com"), mod.Versions[0].FriendlyVersion, new Uri("http://example.com"), new Uri(KerbalStuffReadOnly.RootUri + mod.Versions[0].DownloadPath), null, InstallTarget.Unknown);
            return package;
        }
    }

    public enum InstallTarget
    {
        Unknown, Main, GameData
    }
}