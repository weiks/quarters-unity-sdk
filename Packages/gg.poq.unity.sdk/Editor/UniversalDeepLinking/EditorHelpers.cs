using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

namespace ImaginationOverflow.UniversalDeepLinking.Editor
{
    public static class EditorHelpers
    {
        private static string _pluginPath;

        public static string PluginPath
        {
            get
            {
                if (string.IsNullOrEmpty(_pluginPath))
                    throw new InvalidOperationException("Plugin Path not set");
                return _pluginPath;
            }
        }

        public static string AssetPluginPath
        {
            get
            {
                if (string.IsNullOrEmpty(_pluginPath))
                    throw new InvalidOperationException("Plugin Path not set");
                return "Assets/" + _pluginPath;
            }
        }

        public static void SetPluginName(string name)
        {
            var assetsWithPluginName = AssetDatabase.FindAssets(name, null);

            foreach (var guid in assetsWithPluginName)
            {
                var asset = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetDatabase.IsValidFolder(asset))
                {
                    _pluginPath = asset.Replace("Assets/", "");
                    break;
                }
            }
        }
    }
}
