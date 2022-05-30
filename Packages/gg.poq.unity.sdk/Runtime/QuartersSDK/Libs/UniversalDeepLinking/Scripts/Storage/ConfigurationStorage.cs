using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ImaginationOverflow.UniversalDeepLinking.Storage
{
    public class ConfigurationStorage
    {
        private static string[] SaveFolders = { "Resources", "ImaginationOverflow" };
        private static string SaveFile = "UniversalDeepLink.bytes";
        private static string _fileLocation;

        public static void Save(AppLinkingConfiguration config)
        {
#if DEBUG
            var data = JsonUtility.ToJson(config, true);
#else
            var data = JsonUtility.ToJson(config, false);
#endif
            var file = GetConfigurationLocation();
            if (File.Exists(file) == false)
                EnsureDirectories();

            File.WriteAllText(file, data);

        }

        public static AppLinkingConfiguration Load()
        {
            AppLinkingConfiguration config;
            if (Application.platform == RuntimePlatform.WindowsEditor ||
                Application.platform == RuntimePlatform.OSXEditor ||
                Application.platform == RuntimePlatform.LinuxEditor)
            {
                var fileName = GetConfigurationLocation();

                if (File.Exists(fileName) == false)
                    return new AppLinkingConfiguration();

                config = JsonUtility.FromJson<AppLinkingConfiguration>(File.ReadAllText(fileName));
            }
            else
            {
                var ta = UnityEngine.Resources.Load<TextAsset>("ImaginationOverflow/UniversalDeepLink");
                config = JsonUtility.FromJson<AppLinkingConfiguration>(ta.text);
            }
            config.EnsureAllPlats();
            return config;
        }

        private static void EnsureDirectories()
        {
            var folder = Application.dataPath;
            for (int i = 0; i < SaveFolders.Length; i++)
            {
                folder = Path.Combine(folder, SaveFolders[i]);
                if (Directory.Exists(folder) == false)
                    Directory.CreateDirectory(folder);
            }
        }

        private static string GetConfigurationLocation()
        {
            if (string.IsNullOrEmpty(_fileLocation) == false)
                return _fileLocation;

            var folder = Application.dataPath;

            for (int i = 0; i < SaveFolders.Length; i++)
            {
                folder = Path.Combine(folder, SaveFolders[i]);
            }

            return _fileLocation = Path.Combine(folder, SaveFile);
        }



        public static string CombinePaths(params string[] paths)
        {
            if (paths == null)
            {
                throw new ArgumentNullException("paths");
            }
            return paths.Aggregate(Path.Combine);
        }
    }
}
