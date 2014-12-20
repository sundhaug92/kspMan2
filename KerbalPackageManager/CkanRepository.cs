using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KerbalPackageManager
{
    internal class CkanRepository : Repository
    {
        private WebClient wc = new WebClient();

        public CkanRepository()
        {
            this.Name = "CKAN";
            this.Maintainer = "CKAN";
            this.UseStockCache = false;
            Packages = new List<Package>();
            if (File.GetLastWriteTime(".kpm\\cache\\ckan\\ckan-meta.zip") < DateTime.Now.AddHours(-1))
            {
                try
                {
                    if (!Directory.Exists(".kpm\\cache\\ckan\\")) Directory.CreateDirectory(".kpm\\cache\\ckan\\");
                    wc.DownloadFile("https://github.com/KSP-CKAN/CKAN-meta/archive/master.zip", ".kpm\\cache\\ckan\\ckan-meta.zip");
                }
                catch (WebException we)
                {
                    Console.WriteLine(we.Status);
                }
                using (Stream stream = File.OpenRead(".kpm\\cache\\ckan\\ckan-meta.zip"))
                {
                    ZipArchive zip = new ZipArchive(stream);
                    string targetDirectory = ".kpm\\cache\\ckan\\ckan-meta\\";
                    foreach (ZipArchiveEntry file in zip.Entries)
                    {
                        string toFilename = file.FullName;
                        if (toFilename.EndsWith("/"))
                        {
                            if (!Directory.Exists(targetDirectory + toFilename)) Directory.CreateDirectory(targetDirectory + toFilename);
                        }
                        else
                        {
                            Console.WriteLine("CKAN:" + file);
                            if (toFilename.Contains('/') && !Directory.Exists(targetDirectory + toFilename.Substring(0, toFilename.LastIndexOf('/')))) Directory.CreateDirectory(targetDirectory + toFilename.Substring(0, toFilename.LastIndexOf('/')));
                            file.ExtractToFile(targetDirectory + toFilename, true);
                        }
                    }
                }
            }
            foreach (var fileName in Directory.EnumerateFiles(".kpm\\cache\\ckan\\ckan-meta\\CKAN-meta-master"))
            {
                if (fileName.EndsWith(".ckan"))
                {
                    string Name = "", Author = "", Version = "", Homepage = "", Download = "", Identifier = "";
                    List<UnresolvedPackage> dependencies = new List<UnresolvedPackage>();

                    var jObj = JObject.Parse(File.ReadAllText(fileName));
                    try { Name = (string)jObj["name"]; }
                    catch (Exception e) { }
                    try { Author = (string)jObj["author"]; }
                    catch (Exception e) { }
                    try { Version = (string)jObj["version"]; }
                    catch (Exception e) { }
                    try { Homepage = (string)jObj["homepage"]; }
                    catch (Exception e) { }
                    try { Download = (string)jObj["download"]; }
                    catch (Exception e) { }
                    try { Identifier = (string)jObj["identifier"]; }
                    catch (Exception e) { }
                    try
                    {
                        foreach (var depends in jObj["depends"])
                        {
                            dependencies.Add(new UnresolvedPackage((string)depends["name"]));
                        }
                    }
                    catch (Exception e) { }
                    Package package = new Package(Name, Author, new Uri("http://example.com/"), Version, new Uri(Homepage != null ? Homepage : "http://example.com"), new Uri(Download), dependencies.ToArray(), InstallTarget.Unknown, Identifier);
                    Packages.Add(package);
                }
            }
        }

        public override Package SearchByName(string PackageName)
        {
            return (from package in this.Packages where package.Name.ToLower().Contains(PackageName.ToLower()) || package.Name.ToLower() == PackageName.ToLower() || package.Alias.ToLower().Contains(PackageName.ToLower()) || package.Alias.ToLower() == PackageName.ToLower() select package).FirstOrDefault();
        }
    }
}