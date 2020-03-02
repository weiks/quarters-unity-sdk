using System.Collections;
using System.Collections.Generic;
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

        //TODO solve the parsing problem generic way
        ButtonText.text = $"{product.metadata.localizedPrice} {product.definition.storeSpecificId}";
        
    }
    
    

}
