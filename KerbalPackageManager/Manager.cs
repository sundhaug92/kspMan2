using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KerbalPackageManager
{
    public static class Manager
    {
        private static List<Repository> repos = new List<Repository>();

        public static void Load()
        {
            Console.WriteLine("Loading repositories");
            if (!Directory.Exists(Path.Combine(".kpm", "cache"))) Directory.CreateDirectory(Path.Combine(".kpm", "cache"));
            foreach (var repo in Directory.EnumerateFiles(Path.Combine(".kpm", "cache")))
            {
                var r = JObject.Parse(File.ReadAllText(repo)).ToObject<JsonRepository>();
                Console.WriteLine("Loading {0}", r.Name);
                if (r.uri != null)
                {
                    if (r.LastSyncronized < DateTime.Now.AddSeconds(-5))
                    {
                        Console.WriteLine("{0} too old, pulling from net", r.Name);
                        r = new JsonRepository(r.uri);
                    }
                    repos.Add(r);
                }
            }
            foreach (var repo in File.ReadAllLines(Path.Combine(".kpm", "Repositories.txt")))
            {
                Repository rep;
                if (repo == "@kerbalstuff")
                {
                    rep = new KerbalStuffRepository();
                }
                else if (repo == "@ckan")
                {
                    rep = new CkanRepository();
                }
                else
                {
                    rep = new JsonRepository(repo);
                }
                Console.WriteLine("Loading {0}", rep.Name);

                if (!repos.Any(r => r.Name == rep.Name))
                {
                    repos.Add(rep);
                }
            }
            Console.WriteLine("Loading installed packages");
            InstalledPackages = new List<Package>();
            if (File.Exists(Path.Combine(".kpm", "Installed.json")))
            {
                string json = File.ReadAllText(Path.Combine(".kpm", "Installed.json"));
                if (json == "") json = "[]";
                InstalledPackages = JArray.Parse(json).ToObject<List<Package>>();
            }
        }

        public static Package Resolve(string PackageName)
        {
            Console.WriteLine("Resolving {0}", PackageName);
            Package bestPackage = null;
            foreach (Repository repo in Repositories)
            {
                var pkg = repo.SearchByName(PackageName);
                if (pkg != null)
                {
                    if (bestPackage == null) bestPackage = pkg;
                    else if (bestPackage.Version.CompareTo(pkg.Version) < 0) bestPackage = pkg;
                }
            }
            if (bestPackage != null) return bestPackage;
            Console.WriteLine("Did not find {0}", PackageName);
            return null;
        }

        public static IEnumerable<Repository> Repositories { get { return repos; } }

        internal static string GetConfigString(string p)
        {
            if (File.Exists(Path.Combine(".kpm", "config", p + ".txt")))
                return File.ReadAllText(Path.Combine(".kpm", "config" + p + ".txt"));
            else return "";
        }

        public static List<Package> InstalledPackages;

        internal static bool SameOrNewerInstalled(Package package)
        {
            if (InstalledPackages == null) return false;
            if ((from pkg in InstalledPackages where pkg.Name == package.Name && package.Version.CompareTo(pkg.Version) <= 0 select pkg).Count() > 0)
            {
                Console.WriteLine("{0} version {1} or newer already installed", package, package.Version);
                return true;
            }
            Console.WriteLine("{0} version {1} needs installation", package, package.Version);
            return false;
        }

        public static void Save()
        {
            Console.WriteLine("Saving installed-packages list");
            File.Create(Path.Combine(".kpm", "Installed.jsons")).Close();
            File.WriteAllText(Path.Combine(".kpm", "Installed.json"), JArray.FromObject(InstalledPackages).ToString());
            Console.WriteLine("Saving repositories");

            string cacheDir = Path.Combine(".kpm", "cache");
            foreach (Repository rep in repos)
            {
                if (!rep.UseStockCache) continue;
                if (!File.Exists(cacheDir + rep.Name)) File.Create(rep.Name).Close();
                File.WriteAllText(cacheDir + rep.Name, JObject.FromObject(rep).ToString());
            }
        }
    }
}