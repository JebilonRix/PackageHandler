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

        public static void InstallGitPackages(string[] companyName, string[] packageName, string[] url) => AddGitPackages(companyName, packageName, url);
        public static async void InstallUnityPackages(string[] packageName)
        {
            for (int i = 0; i < packageName.Length; i++)
            {
                await AddPackage(packageName[i]);
            }
        }

        #endregion Public Methods

        #region Private Methods

        private static string Name(string companyName, string packageName) => $"com.{companyName}.{packageName}";
        private static async Task AddPackage(string packageName)
        {
            AddRequest request = Client.Add(Name("unity", packageName));

            while (!request.IsCompleted)
            {
                await Task.Delay(100);
            }

            //Debug.Log($"{packageName} is loaded.");
        }
        private static void AddGitPackages(string[] companyName, string[] packageName, string[] url)
        {
            string path = Path.GetFullPath("Packages/manifest.json");

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

            char dot = '.';
            char quotes = '"';

            //Reads all lines from manifest.
            string[] allLines = File.ReadAllLines(path);

            for (int i = 2; i < allLines.Length - 2; i++)
            {
                allLines[i] = allLines[i].Trim();
                string[] texts = allLines[i].Split(new char[] { dot, quotes });

                //Debug.Log(texts[2]);

                if (texts[2] != "unity")
                {
                    companyNamesFromManifest.Add(texts[2]);
                    packageNamesFromManifest.Add(texts[3]);

                    //Debug.Log(texts[2] + "." + texts[3]);
                }
            }

            for (int i = 0; i < localPackageName.Count; i++)
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

            if (localPackageName.Count == 0)
            {
                return;
            }

            //Prepares lines
            string lines = "";

            for (int i = 0; i < localPackageName.Count; i++)
            {
                string name = Name(localCompanyName[i], localPackageName[i]);

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

            using StreamReader sr = new(path);
            string text = sr.ReadToEnd();
            text = text.Replace(keyword, keyword + "\n" + lines);
            text.Trim();
            sr.Close();

            //Writes to the file.
            using StreamWriter sw = new(path);
            sw.Write(text);
            sw.Close();

            //Debug.Log("Packages are added to project from git.");

            Client.Resolve();
        }

        #endregion Private Methods
    }
}