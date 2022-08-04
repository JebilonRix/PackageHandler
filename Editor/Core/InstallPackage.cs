using System.IO;
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

            Debug.Log($"{packageName} is loaded.");
        }
        private static void AddGitPackages(string[] companyName, string[] packageName, string[] url)
        {
            string path = Path.GetFullPath("Packages/manifest.json");

            if (!File.Exists(path))
            {
                Debug.Log("manifest.json is not exist");
                return;
            }

            string keyword = "\"dependencies\": {";
            string lines = "";

            for (int i = 0; i < packageName.Length; i++)
            {
                string name = Name(companyName[i], packageName[i]);

                lines += $"\t\t\"{name}\" : " + $"\"{url[i]}\",\n";
            }

            using StreamReader sr = new(path);
            string text = sr.ReadToEnd();
            text = text.Replace(keyword, keyword + "\n" + lines);
            sr.Close();

            using StreamWriter sw = new(path);
            sw.Write(text);
            sw.Close();

            Debug.Log("Packages are added to project from git.");

            Client.Resolve();
        }

        #endregion Private Methods
    }
}