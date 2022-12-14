using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEditor.PackageManager;
using UnityEngine;

namespace RedPanda.PackageHandler
{
    public static class DefaultPackage
    {
        //This my git user name and gist project id https://gist.github.com/JebilonRix/4a4f95adb4f1b530bf9914a99f5a069d
        public static async Task ReplacePackagesManifestFromGit(string id, string user = "JebilonRix")
        {
            //Url of gist repository.
            string url = $"https://gist.github.com/{user}/{id}/raw";
            string contents = await GetContents(url);
            ReplacePackageFile(contents);
        }

        private static async Task<string> GetContents(string url)
        {
            //Gets access of url.
            using HttpClient client = new();
            HttpResponseMessage response = await client.GetAsync(url);

            //Reads string from response.
            string contents = await response.Content.ReadAsStringAsync();
            return contents;
        }

        private static void ReplacePackageFile(string contents)
        {
            //Finds manifest.
            string existing = Path.Combine(Application.dataPath, "../Packages/manifest.json");

            //Writes lines which exists in gist repository to manifest.
            File.WriteAllText(existing, contents);

            Client.Resolve();
        }
    }
}