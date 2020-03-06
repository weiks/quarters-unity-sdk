using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;

public static class ModuleManager {

    public static bool IsPlayfabSDKInstalled {
        get {
            foreach(Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach(Type type in assembly.GetTypes())  {
                    if (type.Namespace == "Playfab") return true;
                }
            }
            return false;
        }
    }

	
}
