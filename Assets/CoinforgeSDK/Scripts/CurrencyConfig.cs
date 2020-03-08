using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

   
[CreateAssetMenu(fileName = "DefaultCurrency", menuName = "Coinforge/Currency", order = 1)]
public class CurrencyConfig : ScriptableObject {

    public string APIBaseUrl;
    public string Code;
    public string DisplayNameSingular;
    public string DisplayNamePlural;

    public Sprite CurrencyIcon;
    public Sprite CurrencyLogo;
    public Sprite BrandLogo;

    public Color BackgroundsColor;
    public Color BrandColor;

    public List<string> IAPProductIds;
    


}


