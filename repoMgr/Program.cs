using KerbalPackageManager;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace repoMgr
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Repository repo = new Repository();
            while (true)
            {
                PrintHelp();
                string cmd = RequestStringFromUser();
                var sCmd = cmd.Split(' ');
                if (sCmd[0] == "exit" || sCmd[0] == "quit") break;
                if (sCmd[0] == "load-repo-file") repo = JObject.Parse(File.ReadAllText(sCmd[1])).ToObject<Repository>();
                if (sCmd[0] == "save-repo-file") File.WriteAllText(sCmd[1], JObject.FromObject(repo).ToString());
                if (sCmd[0] == "edit-repo-meta")
                {
                    if (sCmd[1] == "name")
                    {
                        Console.Write("Name=");
                        repo.Name = Console.ReadLine();
                    }
                    if (sCmd[1] == "maintainer")
                    {
                        Console.Write("Maintainer=");
                        repo.Maintainer = Console.ReadLine();
                    }
                }
                if (sCmd[0] == "add-package")
                {
                    string Name = RequestStringFromUser("Name"), Maintainer = RequestStringFromUser("Maintainer"), Version = RequestStringFromUser("Version");
                    UnresolvedPackage[] Dependencies = RequestUnresolvedPackageFromUser("Dependency");
                    KerbalPackageManager.InstallTarget InstallTarget = RequestInstallTargetFromUser("InstallTarget");
                    Uri LicenceUri = RequestUriFromUser("LicenceUri"), ForumThreadUri = RequestUriFromUser("ForumThreadUri"), DownloadUri = RequestUriFromUser("DownloadUri");

                    repo.AddPackage(new Package(Name, Maintainer, LicenceUri, Version, ForumThreadUri, DownloadUri, Dependencies, InstallTarget));
                }
                if (sCmd[0] == "del-package")
                {
                    string Name = RequestStringFromUser("Name");
                    repo.Packages = (from package in repo.Packages where package.Name != Name select package).ToArray();
                }

                if (sCmd[0] == "import-kerbalstuff-package")
                {
                    var ksPkg = JObject.Parse(new WebClient().DownloadString("https://kerbalstuff.com/api/mod/" + sCmd[1]));
                    Package pkg = new Package(
                        ksPkg.GetValue("name").ToObject<string>(),
                        ksPkg.GetValue("author").ToObject<string>(),
                        RequestUriFromUser("LicenceUri"),
                        ksPkg.GetValue("versions").First.Value<string>("friendly_version"),
                        RequestUriFromUser("ForumThreadUri"),
                        new Uri("https://kerbalstuff.com/" + ksPkg.GetValue("versions").First.Value<string>("download_path")),
                        RequestUnresolvedPackageFromUser("Dependency"),
                        RequestInstallTargetFromUser("InstallTarget")
                        );
                    repo.AddPackage(pkg);
                }
            }
        }

        private static UnresolvedPackage[] RequestUnresolvedPackageFromUser(string prompt = "", UnresolvedPackage[] standard = null)
        {
            List<UnresolvedPackage> deps = new List<UnresolvedPackage>();
            string str;
            while ((str = RequestStringFromUser(prompt)) != "") deps.Add(new UnresolvedPackage(str));
            return deps.ToArray();
        }

        private static InstallTarget RequestInstallTargetFromUser(string prompt = "", InstallTarget standard = InstallTarget.GameData)
        {
            var str = RequestStringFromUser(prompt, standard.ToString());
            if (str == "") return standard;
            return (InstallTarget)Enum.Parse(typeof(InstallTarget), str);
        }

        private static Uri RequestUriFromUser(string prompt = "", Uri standard = null)
        {
            string str;
            if (standard == null) str = RequestStringFromUser(prompt);
            else str = RequestStringFromUser(prompt, standard.ToString());
            if (str == "") return standard;
            else return new Uri(str);
        }

        private static string RequestStringFromUser(string prompt = "", string standard = "")
        {
            if (standard != "") Console.Write("{0}[{1}]=", prompt, standard);
            else Console.Write("{0}=", prompt);
            string ret = Console.ReadLine();
            if (ret == "") return standard;
            return ret;
        }

        private static void PrintHelp()
        {
        }
    }
}