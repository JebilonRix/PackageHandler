using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RedPanda.PackageHandler
{
    public class PackageHandlerWindow : EditorWindow
    {
        #region Fields And Properties

        [SerializeField] private IdeType _ideType = IdeType.VisualStudio;
        [SerializeField] private List<UnityPackageTypes> _unityPackages = new();
        [SerializeField] private List<PackageLines> _gitPackagesToInstall = new();
        [SerializeField] private string _companyName;
        [SerializeField] private string _packageName;
        [SerializeField] private string _url;

        private readonly List<string> _unityPackagesToInstall = new();
        private const string _fileName = "GitLinesSave.json";
        private SerializedObject _window;

        #endregion Fields And Properties

        #region Unity Methods

        [MenuItem("MyTools/PackageHandler")]
        public static void Open() => GetWindow(typeof(PackageHandlerWindow), false, "Package Handler");
        private void OnEnable() => _window = new SerializedObject(this); //Sets the window as serialized object
        private void OnGUI() => EditorLayout(); //Includes editor gui layout elements

        #endregion Unity Methods

        #region Private Methods

        private async void ClearPackages() => await DefaultPackage.ReplacePackagesManifestFromGit("4a4f95adb4f1b530bf9914a99f5a069d");
        private void EditorLayout()
        {
            position.Set(0, 0, 100, 100);

            if (GUILayout.Button("Clear PackageManager"))
            {
                ClearPackages();
            }
            if (GUILayout.Button("Add Packages To PackageManager"))
            {
                InstallGitPackages(); // Sets packages from git

                InstallUnityPackages(SetUnityPackages(_ideType)); //Sets unity packages

                InstallPackage.InstallUnityPackages(_unityPackagesToInstall.ToArray());

                _unityPackagesToInstall.Clear();
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.PropertyField(_window.FindProperty(nameof(_ideType)));
            EditorGUILayout.Space(10);
            EditorGUILayout.PropertyField(_window.FindProperty(nameof(_unityPackages)));
            EditorGUILayout.Space(10);
            EditorGUILayout.PropertyField(_window.FindProperty(nameof(_gitPackagesToInstall)));
            EditorGUILayout.Space(10);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Git Package Options", EditorStyles.boldLabel);

                if (GUILayout.Button("Save"))
                {
                    JsonHelper.SaveToJson<PackageLines>(_gitPackagesToInstall, _fileName);
                    _window.Update();

                    AssetDatabase.Refresh();
                }
                if (GUILayout.Button("Load"))
                {
                    _gitPackagesToInstall = JsonHelper.LoadFromJson<PackageLines>(_fileName);
                    _window.Update();
                }
            }

            if (_window.ApplyModifiedProperties())
            {
                _window.Update();
            }
        }
        private void InstallUnityPackages(string[] array)
        {
            if (array == null)
            {
                return;
            }

            for (int i = 0; i < array.Length; i++)
            {
                _unityPackagesToInstall.Add(array[i]);
            }
        }
        private string[] SetUnityPackages(IdeType type)
        {
            //Removes duplicates
            List<UnityPackageTypes> noDuplicates = _unityPackages.Distinct().ToList();
            //Count of new list
            int length = noDuplicates.Count;

            //Adjusts to array which is returned.
            string[] packages = new string[length + 2];

            //Sets the package of ide that is selected.
            switch (type)
            {
                case IdeType.VisualStudio:
                    packages[packages.Length - 2] = "ide.visualstudio";
                    break;

                case IdeType.VisualStudioCode:
                    packages[packages.Length - 2] = "ide.vscode";
                    break;

                case IdeType.Rider:
                    packages[packages.Length - 2] = "ide.rider";
                    break;
            }

            //this is for debugging from ide
            packages[packages.Length - 1] = "test-framework";

            for (int i = 0; i < length; i++)
            {
                packages[i] = GetUnityPackageName(noDuplicates[i]);
            }

            return packages;
        }
        private static string GetUnityPackageName(UnityPackageTypes type)
        {
            return type switch
            {
                UnityPackageTypes.Cinemachine => "cinemachine",
                UnityPackageTypes.Recorder => "recorder",
                UnityPackageTypes.Sprite2D => "2d.sprite",
                UnityPackageTypes.TextMeshPro => "textmeshpro",
                UnityPackageTypes.UnityUI => "ugui",
                _ => "ugui",
            };
        }
        private void InstallGitPackages()
        {
            int count = _gitPackagesToInstall.Count;

            if (count == 0)
            {
                return;
            }

            //sets structure of lines
            string[] companyNames = new string[count];
            string[] packageNames = new string[count];
            string[] urls = new string[count];

            for (int i = 0; i < count; i++)
            {
                companyNames[i] = _gitPackagesToInstall[i].companyName;
                packageNames[i] = _gitPackagesToInstall[i].packageName;
                urls[i] = _gitPackagesToInstall[i].url;
            }

            //Sends to installer
            InstallPackage.InstallGitPackages(companyNames, packageNames, urls);
        }

        #endregion Private Methods
    }
}