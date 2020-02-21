using UnityEngine;
using System.Collections;
using UnityEditor;

namespace QuartersSDK.UI.Internal {
	public class ToolkitColor : MonoBehaviour {


		public static Color shade {
			get {
				if (EditorGUIUtility.isProSkin) return new Color(0.15f, 0.15f, 0.15f);
				else return new Color(0.65f, 0.65f, 0.65f);
			}
		}

	}
}