using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CoinforgeSDK {


    public class EditorUtils {
        
        [MenuItem("Coinforge/Deauthorize Coinforge User")]
        private static void Deauthorize() {
            
            Session.Invalidate();
            Debug.Log("Coinforge user deauthorized");
            
        }
    }
}