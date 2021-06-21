using System.Collections;
using System.Collections.Generic;
using QuartersSDK;
using QuartersSDK.UI;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class UIProductButton : MonoBehaviour, IListCell {
    
    public Text ButtonText;
    [HideInInspector]
    public Product product;
    
    
    public void Init(object data) {

        product = (Product)data;

        CurrencyConfig config = Quarters.Instance.CurrencyConfig;

        int quantity = QuartersIAP.Instance.ParseCoinsQuantity(product, config);
        string productDisplayName = quantity == 1 ? config.DisplayNameSingular : config.DisplayNamePlural;
        
        ButtonText.text = $"{product.metadata.localizedPriceString}  -  {quantity} {productDisplayName}";
        
    }
    
    

}
