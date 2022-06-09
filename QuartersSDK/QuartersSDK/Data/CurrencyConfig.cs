using UnityEngine;

namespace QuartersSDK.Data
{
    public class CurrencyConfig : ScriptableObject
    {
        public string Code;
        public string DisplayNameSingular;
        public string DisplayNamePlural;

        [Header("Branding")] public Sprite CurrencyIcon;
    }
}
