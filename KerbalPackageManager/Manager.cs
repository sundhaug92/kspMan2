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
            foreach (var repo in File.ReadAllLines(".kpm\\Repositories.txt"))
            {
                repos.Add(new Repository(new Uri(repo)));
            }
            Console.WriteLine("Loading installed packages");
            InstalledPackages = new List<Package>();
            if (File.Exists(".kpm\\Installed.jsons")) foreach (var line in File.ReadAllLines(".kpm\\Installed.jsons"))
                {
                    InstalledPackages.Add(new Package(JObject.Parse(line)));
                }
        }

        public static Package Resolve(string PackageName)
        {
            Console.WriteLine("Resolving {0}", PackageName);
            foreach (Repository repo in Repositories)
            {
                if (repo.Packages != null)
                {
                    var pkg = (from package in repo.Packages where package.Name == PackageName select package).FirstOrDefault();
                    Console.WriteLine("Found {0}", pkg);
                    if (pkg != null) return pkg;
                }
            }

            Console.WriteLine("Did not find {0}", PackageName);
            return null;
        }

        public static IEnumerable<Repository> Repositories { get { return repos; } }

        internal static string GetConfigString(string p)
        {
            return File.ReadAllText(".kpm\\config\\" + p + ".txt");
        }

        public static List<Package> InstalledPackages;

        internal static bool SameOrNewerInstalled(Package package)
        {
            if (InstalledPackages == null) return false;
            if ((from pkg in InstalledPackages where pkg.Name == package.Name && package.Version.CompareTo(pkg) < 0 select pkg).Count() > 0)
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
            File.Create(".kpm\\Installed.jsons");
            string[] tmp = new string[InstalledPackages.Count()];
            for (int i = 0; i < tmp.Length; i++) tmp[i] = JObject.FromObject(InstalledPackages[i]).ToString();
        }
    }
}