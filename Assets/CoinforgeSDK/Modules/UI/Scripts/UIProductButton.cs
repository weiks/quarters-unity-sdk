using System.Collections;
using System.Collections.Generic;
using CoinforgeSDK;
using CoinforgeSDK.UI;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class UIProductButton : MonoBehaviour, IListCell {
    
    public Text ButtonText;
    [HideInInspector]
    public Product product;
    
    
    public void Init(object data) {

        product = (Product)data;

        CurrencyConfig config = Coinforge.Instance.CurrencyConfig;

        int quantity = CoinforgeIAP.Instance.ParseCoinsQuantity(product, config);
        string productDisplayName = quantity == 1 ? config.DisplayNameSingular : config.DisplayNamePlural;
        
        ButtonText.text = $"{product.metadata.localizedPriceString}  -  {quantity} {productDisplayName}";
        
    }
    
    

}
