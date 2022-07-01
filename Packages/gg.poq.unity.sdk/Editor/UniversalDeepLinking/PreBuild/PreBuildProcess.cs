using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace ImaginationOverflow.UniversalDeepLinking.Editor.PreBuild
{
    public class PreBuildProcess : IPreprocessBuild
    {
        public int callbackOrder
        {
            get { return int.MaxValue - 10; }
        }

        public void OnPreprocessBuild(BuildTarget target, string path)
        {
            EditorHelpers.SetPluginName("UniversalDeepLinking");

            var processor = GetProcessor(target);

            if (processor == null)
                return;


            AppLinkingConfiguration configuration = Storage.ConfigurationStorage.Load();


            processor.OnPreprocessBuild(configuration, path);
        }

        private static IPreBuild GetProcessor(BuildTarget target)
        {
            if (target == BuildTarget.Android)
                return new AndroidPreBuild();

#if UNITY_2018_3_OR_NEWER && !UDL_DLL_BUILD

            if ((target == BuildTarget.StandaloneWindows || target == BuildTarget.StandaloneWindows64) &&
                PlayerSettings.GetApiCompatibilityLevel(BuildTargetGroup.Standalone) == ApiCompatibilityLevel.NET_Standard_2_0)
            {
                UnityEngine.Debug.LogError("Universal Deep Linking: Windows standalone builds need to .NET 4.x Api Compatibility Level, please update your *Player Settings* accordingly.");
            }
#endif
            HandleMacIl2cpp();

            return null;
        }

        private static void HandleMacIl2cpp()
        {
            if (EditorUserBuildSettings.selectedBuildTargetGroup != BuildTargetGroup.Standalone)
                return;

            var isIl2cpp = PlayerSettings.GetScriptingBackend(BuildTargetGroup.Standalone) ==
                           ScriptingImplementation.IL2CPP;

            //#if UDL_DLL_BUILD
            //            if (isIl2cpp)
            //            {
            //                Debug.LogWarning("Il2cpp selected, please ensure the following: " +
            //                                 "\nThe MacIL2CPP.dylib is set to be included in the compilation for MacOs (by default it isn't)" +
            //                                 "\nThe ImaginationOverflow.UniversalDeepLinking.Platform library under /Plugins/ImaginationOverflow/UniversalDeepLinking/libs/Standalone/Mac is not set to be included on this build" +
            //                                 "\nThe ImaginationOverflow.UniversalDeepLinking.Platform library under /Plugins/ImaginationOverflow/UniversalDeepLinking/libs/Standalone/Mac/il2cpp is set to be included on this build ");
            //            }
            //#endif
            var staticLibPath = EditorHelpers.AssetPluginPath + "/libs/Standalone/MacIL2CPP.dylib";

            var staticLib = (AssetImporter.GetAtPath(staticLibPath) as PluginImporter);
#if UDL_DLL_BUILD
            var dll = (AssetImporter.GetAtPath(EditorHelpers.AssetPluginPath +
                                               "/libs/Standalone/Mac/il2cpp/ImaginationOverflow.UniversalDeepLinking.Platform.dll")
                as PluginImporter);
            var originalDll = (AssetImporter.GetAtPath(EditorHelpers.AssetPluginPath +
                                                       "/libs/Standalone/Mac/ImaginationOverflow.UniversalDeepLinking.Platform.dll")
                as PluginImporter);
            SetIl2cppConfig(false, dll);
            SetIl2cppConfig(false, originalDll);
            SetIl2cppConfig(isIl2cpp, dll);
            SetIl2cppConfig(!isIl2cpp, originalDll);
#endif

            if (staticLib != null)
            {
                SetIl2cppConfig(isIl2cpp, staticLib);
            }
            return;
        }

        private static void SetIl2cppConfig(bool isCpp, PluginImporter dll)
        {
#if !UNITY_2018_1_OR_NEWER
            var target = BuildTarget.StandaloneOSX;
#else
            var target = BuildTarget.StandaloneOSX;
#endif
            dll.ClearSettings();
            dll.SetCompatibleWithAnyPlatform(false);

            if (isCpp)
            {
                dll.SetCompatibleWithPlatform(target, true); //StandaloneOSX
            }

            dll.SaveAndReimport();
        }

    }
}