using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace ImaginationOverflow.UniversalDeepLinking.Editor
{
    /// <summary>
    /// Pos prcessor selector
    /// </summary>
    public class BuildProcessor
    {
        /// <summary>
        /// Use this event when you require to sync with the end of the Deep Link injection post processing.
        /// </summary>
        public static event EventHandler PostBuildProcessCompleted;
        /// <summary>
        /// 
        /// </summary>
        internal static bool FireCompletionEventAfterCall { get; set; }
        [PostProcessBuild(Int32.MaxValue - 10)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            FireCompletionEventAfterCall = true;
            AppLinkingConfiguration configuration = Storage.ConfigurationStorage.Load();

            var processor = GetProcessor(target);

            if (processor == null)
                return;
            EditorHelpers.SetPluginName("UniversalDeepLinking");

            processor.PostBuildProcess(configuration, pathToBuiltProject);

            if (FireCompletionEventAfterCall)
                TriggerOnPostBuildProcessCompleted();

        }

        internal static void TriggerOnPostBuildProcessCompleted()
        {
            if (PostBuildProcessCompleted != null)
            {
                UnityEngine.Debug.Log("Calling PostBuildProcessCompleted");
                PostBuildProcessCompleted(null, EventArgs.Empty);
            }
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
            if (target == BuildTarget.StandaloneOSXIntel || target == BuildTarget.StandaloneOSXIntel64)
                return new MacOsPosBuild();

#endif
            return null;
        }
    }



}
