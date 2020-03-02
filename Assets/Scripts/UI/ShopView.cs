using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using QuartersSDK.UI;
using QuartersSDK;
using UnityEngine.Purchasing;

public class ShopView : UIView {

    [SerializeField]
    private Text ShopTitleText;

    [SerializeField] private UIList productsList;
    
    public override void ViewWillAppear(UIView sourceView = null) {
        base.ViewWillAppear(sourceView);
        
        ShopTitleText.text = $"Buy " + Quarters.Instance.CurrencyConfig.DisplayNamePlural;
        productsList.Populate(new ArrayList(QuartersIAP.Instance.products));
    }



    public void ProductButtonTapped(UIProductButton productButton) {
        
        ModalView.instance.ShowActivity();
        QuartersIAP.Instance.BuyProduct(productButton.product, PurchaseSucessfullDelegate, PurchaseFailedDelegate);
        
    }

    private void PurchaseFailedDelegate(string error) {
        ModalView.instance.ShowAlert("Purchase Error", error, new string[]{"OK"}, null);
    }

    private void PurchaseSucessfullDelegate(Product product, string txid) {
        
        //refresh balance
        Quarters.Instance.GetAccountBalance(delegate(User.Account.Balance newBalance) {
            
            ModalView.instance.HideActivity();
            
        }, delegate(string balanceError) {
            ModalView.instance.ShowAlert("Balance Error", balanceError, new string[]{"OK"}, null);
        });
    }
}