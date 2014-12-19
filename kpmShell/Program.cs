﻿using KerbalPackageManager;
using System;

namespace kpmShell
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Manager.Load();
            Console.Clear();
            Package.FromKerbalStuff(398).DownloadAndInstall();
            while (true)
            {
                PrintHelp();
                Console.Write("=");
                string cmd = Console.ReadLine();
                Console.WriteLine();
                var sCmd = cmd.Split(' ');
                if (sCmd[0] == "load-manager") Manager.Load();
                if (sCmd[0] == "save-manager") Manager.Save();
                if (sCmd[0] == "install-package")
                {
                    Manager.Resolve(cmd.Substring("install-package ".Length).Trim()).DownloadAndInstall();
                }
                if (sCmd[0] == "list-installed-packages")
                {
                    foreach (var pkg in Manager.InstalledPackages) Console.WriteLine(pkg);
                }

                //Does not work??
                /*if (sCmd[0] == "reinstall-all")
                {
                    var packages = Manager.InstalledPackages;
                    foreach (Package pkg in packages) new UnresolvedPackage(pkg.Name).Resolve().DownloadAndInstall();
                }
                if (sCmd[0] == "update-all")
                {
                    var packages = Manager.InstalledPackages;
                    foreach (Package pkg in packages) pkg.DownloadAndInstall();
                }*/
                if (sCmd[0] == "exit" || sCmd[0] == "quit") break;
            }

            Manager.Save();
        }

        private static void PrintHelp()
        {
            Console.WriteLine("Commands: ");
            Console.WriteLine("install-package:\t\tInstall package");
            Console.WriteLine("list-installed-packages:\tList installed packages");
            Console.WriteLine("load-manager:\t\t\tLoad manager state from disk");
            Console.WriteLine("save-manager:\t\t\tSave manager state to disk");
            Console.WriteLine("exit:\t\t\t\texit");
            Console.WriteLine("quit:\t\t\t\tquit");
        }
    }
}