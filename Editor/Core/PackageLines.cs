using System;

namespace RedPanda.PackageHandler
{
    [Serializable]
    public class PackageLines
    {
        public string companyName;
        public string packageName;
        public string url;

        public PackageLines(string companyName, string packageName, string url)
        {
            this.companyName = companyName;
            this.packageName = packageName;
            this.url = url;
        }
    }
}