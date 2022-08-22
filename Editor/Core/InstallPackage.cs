using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace RedPanda.PackageHandler
{
    public static class InstallPackage
    {
        #region Public Methods

        //Installs git packages.
        public static void InstallGitPackages(string[] companyName, string[] packageName, string[] url) => AddGitPackages(companyName, packageName, url);

        //Installs unity packages one by one.
        public static async void InstallUnityPackages(string[] packageName)
        {
            for (int i = 0; i < packageName.Length; i++)
            {
                await AddPackage(packageName[i]);
            }
        }

        #endregion Public Methods

        #region Private Methods

        //Sets name of package.
        private static string Name(string companyName, string packageName) => $"com.{companyName}.{packageName}";

        //Loads unity package.
        private static async Task AddPackage(string packageName)
        {
            //Creats a request loading of unity package.
            AddRequest request = Client.Add(Name("unity", packageName));

            //Waits the loading of unity package.
            while (!request.IsCompleted)
            {
                await Task.Delay(100);
            }

            Debug.Log($"{packageName} is loaded.");
        }
        private static void AddGitPackages(string[] companyName, string[] packageName, string[] url)
        {
            //Gets manfiest.
            string path = Path.GetFullPath("Packages/manifest.json");

            //Checks if manifest is existed.
            if (!File.Exists(path))
            {
                Debug.Log("manifest.json is not exist");
                return;
            }

            //Creates lists from arrays.
            List<string> localCompanyName = companyName.ToList();
            List<string> localPackageName = packageName.ToList();
            List<string> localUrl = url.ToList();

            //Creates lists from manifests.
            List<string> companyNamesFromManifest = new();
            List<string> packageNamesFromManifest = new();

            //Chars of seperators.
            char dot = '.';
            char quotes = '"';

            //Reads all lines from manifest.
            string[] allLines = File.ReadAllLines(path);

            //First two and last two lines is seperated. Because it does not include package names.
            for (int i = 2; i < allLines.Length - 2; i++)
            {
                //Trims of whitespaces.
                allLines[i] = allLines[i].Trim();

                //Splits the lines.
                string[] texts = allLines[i].Split(new char[] { dot, quotes });

                //If line is not unity package adds to installer.
                if (texts[2] != "unity")
                {
                    //Adds company name. Second index is company name.
                    companyNamesFromManifest.Add(texts[2]);

                    //Adds package name. Third index is package name.
                    packageNamesFromManifest.Add(texts[3]);
                }
            }

            //If manifest includes package name which will be added to installer, it will be removed from new package names.
            for (int i = localPackageName.Count - 1; i >= 0; i--)
            {
                for (int j = 0; j < packageNamesFromManifest.Count; j++)
                {
                    if (packageNamesFromManifest[j] == localPackageName[i])
                    {
                        localCompanyName.RemoveAt(i);
                        localPackageName.RemoveAt(i);
                        localUrl.RemoveAt(i);
                    }
                }
            }

            //If there is no new package, it stops.
            if (localPackageName.Count == 0)
            {
                return;
            }

            //Prepares lines
            string lines = "";

            for (int i = 0; i < localPackageName.Count; i++)
            {
                //Sets the package name.
                string name = Name(localCompanyName[i], localPackageName[i]);

                //Adds lines with skiping lines unless it is last index.
                if (i == localPackageName.Count - 1)
                {
                    lines += $"\t\t\"{name}\" : " + $"\"{localUrl[i]}\",";
                }
                else
                {
                    lines += $"\t\t\"{name}\" : " + $"\"{localUrl[i]}\",\n";
                }
            }

            //Reads the file and adds lines to manifest
            string keyword = "\"dependencies\": {";

            //Reads the manifest and adds lines of git packages references.
            using StreamReader sr = new(path);
            string text = sr.ReadToEnd();
            text = text.Replace(keyword, keyword + "\n" + lines);
            text.Trim();
            sr.Close();

            //Writes to the file.
            using StreamWriter sw = new(path);
            sw.Write(text);
            sw.Close();

            Debug.Log("Packages are added to project from git.");

            Client.Resolve();
        }

        #endregion Private Methods
    }
}
