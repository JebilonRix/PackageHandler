using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace RedPanda.PackageHandler
{
    public static class JsonHelper
    {
        private const string directoryName = "Unity Package Handler";

        public static void SaveToJson<T>(List<T> listToSave, string filename)
        {
            string content = ToJson<T>(listToSave.ToArray(), true);
            WriteFile(GetPath(filename), content);
        }

        public static List<T> LoadFromJson<T>(string fileName)
        {
            string content = ReadFile(GetPath(fileName));

            if (string.IsNullOrEmpty(content) || content == "{}")
                return new List<T>();

            List<T> res = FromJson<T>(content).ToList();

            return res;
        }

        public static T[] FromJson<T>(string json)
        {
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
            return wrapper.Items;
        }

        public static string ToJson<T>(T[] array)
        {
            Wrapper<T> wrapper = new()
            {
                Items = array
            };

            return JsonUtility.ToJson(wrapper);
        }

        public static string ToJson<T>(T[] array, bool prettyPrint)
        {
            Wrapper<T> wrapper = new()
            {
                Items = array
            };

            return JsonUtility.ToJson(wrapper, prettyPrint);
        }

        [Serializable]
        private class Wrapper<T>
        {
            public T[] Items;
        }

        private static string GetPath(string fileName)
        {
            //Check if the file exists in my documents folder.
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/" + directoryName;

            //if file doesn't exist in "my documents" folder, creates directory.
            if (!File.Exists(path))
                Directory.CreateDirectory(path);

            return path + "/" + fileName;
        }

        private static string ReadFile(string path)
        {
            //Reads the file.
            if (!File.Exists(path))
                return "";

            using StreamReader reader = new(path);
            string content = reader.ReadToEnd();
            reader.Close();
            return content;
        }

        private static void WriteFile(string path, string content)
        {
            //Writes to the file.
            FileStream fileStream = new(path, FileMode.Create);
            using StreamWriter writer = new(fileStream);
            writer.Write(content);
            writer.Close();
        }
    }
}