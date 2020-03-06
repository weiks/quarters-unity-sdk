using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System;

namespace CoinforgeSDK.UI.Internal {
	public class Utilities : Editor {



		public static void CreateViewClass(string className) {

			string name = className.Replace(" ","_");
			name = name.Replace("-","_");
			string path = "Assets/Scripts/UI/";
			string copyPath = path + name+".cs";

			if (!Directory.Exists(path)) {
				//creating new folder
				Directory.CreateDirectory(path);
			}

			//grab template
			string guid = AssetDatabase.FindAssets("UIViewSubclassTemplate")[0];
			string templatePath = AssetDatabase.GUIDToAssetPath(guid);

			TextAsset template = (TextAsset)AssetDatabase.LoadMainAssetAtPath(templatePath);

			if( File.Exists(copyPath) == false ){ 
				Debug.Log("Creating Classfile: " + copyPath);
				string amendedText = template.text.Replace("SubclassViewName", name);
				File.WriteAllText(copyPath, amendedText);
				AssetDatabase.Refresh();
			}
			else {
				Debug.LogError(name + " class already exist in the project directory");
			}

		}


	}



}
