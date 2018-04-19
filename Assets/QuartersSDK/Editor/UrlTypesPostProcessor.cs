using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Callbacks;


#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

using UnityEngine;
using UnityEditor;
using PlistCS;

namespace AssemblyCSharpEditor
{
    public class UrlTypesPostProcessor
    {
        [PostProcessBuild(1000)]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string path) {
   
            if (buildTarget == BuildTarget.iOS) {
                // set plist path
                string projectPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";
                string plistPath = path + "/info.plist";

                // read plist
                Dictionary<string, object> dict;
                dict = (Dictionary<string, object>)Plist.readPlist(plistPath);

                // update plist
                dict["CFBundleURLTypes"] = new List<object> {
                    new Dictionary<string,object> {
                        { "CFBundleURLName", PlayerSettings.iPhoneBundleIdentifier },
                        { "CFBundleURLSchemes", new List<object> { PlayerSettings.iPhoneBundleIdentifier } }
                    }
                };

                // write plist
                Plist.writeXml(dict, plistPath);
            }

			else if (buildTarget == BuildTarget.Android) {

//				Debug.Log("Android post process");

			}
        }



    }
}

