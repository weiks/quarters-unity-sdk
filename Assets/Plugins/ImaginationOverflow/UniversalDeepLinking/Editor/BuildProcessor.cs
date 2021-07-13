using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;

namespace ImaginationOverflow.UniversalDeepLinking.Editor
{
    /// <summary>
    /// Pos prcessor selector
    /// </summary>
    public class BuildProcessor
    {
        [PostProcessBuild(Int32.MaxValue - 10)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            AppLinkingConfiguration configuration = Storage.ConfigurationStorage.Load();

            var processor = GetProcessor(target);

            if (processor == null)
                return;

            EditorHelpers.SetPluginName("UniversalDeepLinking");

            processor.PostBuildProcess(configuration, pathToBuiltProject);
        }



        private static IPosBuilder GetProcessor(BuildTarget target)
        {
            if (target == BuildTarget.WSAPlayer)
                return new UwpBuilder();
#if UNITY_EDITOR && (UNITY_IOS || UNITY_TVOS) || UDL_DLL_BUILD
            if (target == BuildTarget.iOS || target == BuildTarget.tvOS)
                return new iOSBuilder(target);
#endif

#if UNITY_2018_1_OR_NEWER
            if (target == BuildTarget.StandaloneOSX)
                return new MacOsPosBuild();
#else
            if (target == BuildTarget.StandaloneOSXIntel || target == BuildTarget.StandaloneOSXIntel64 )
                return new MacOsPosBuild();
            
#endif
            return null;
        }
    }



}
