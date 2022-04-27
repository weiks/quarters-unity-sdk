using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;


namespace QuartersSDK {
    public class CurrencyConfig : ScriptableObject {
        
        public string Code;
        public string DisplayNameSingular;
        public string DisplayNamePlural;

        [Header("Branding")] public Sprite CurrencyIcon;
        

    }
}


