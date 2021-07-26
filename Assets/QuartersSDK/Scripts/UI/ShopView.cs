using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using QuartersSDK.UI;
using QuartersSDK;
using UnityEngine.Purchasing;

namespace QuartersSDK.UI {
    public class ShopView : UIView {

        [SerializeField] private Text ShopTitleText;
        [SerializeField] private UIList productsList;
        [SerializeField] private Image logo;
        [SerializeField] private AspectRatioFitter logoAspectRatioFitter;

        public override void ViewWillAppear(UIView sourceView = null) {
            base.ViewWillAppear(sourceView);

            ShopTitleText.text = $"Buy " + Quarters.Instance.CurrencyConfig.DisplayNamePlural;
            productsList.Populate(new ArrayList(QuartersIAP.Instance.products));

            Sprite logoSprite = QuartersInit.Instance.CurrencyConfig.CurrencyLogo;
            float aspectRatio = (float) logoSprite.rect.width / (float) logoSprite.rect.height;
            logoAspectRatioFitter.aspectRatio = aspectRatio;
            logo.sprite = logoSprite;

        }



        public void ProductButtonTapped(UIProductButton productButton) {

            ModalView.instance.ShowActivity();
            QuartersIAP.Instance.BuyProduct(productButton.product, PurchaseSucessfullDelegate, PurchaseFailedDelegate);

        }

        private void PurchaseFailedDelegate(string error) {
            ModalView.instance.ShowAlert("Purchase Error", error, new string[] {"OK"}, null);
        }

        private void PurchaseSucessfullDelegate(Product product, string txid) {

            //refresh balance
            Quarters.Instance.GetAccountBalanceCall(delegate(long newBalance) {
                ModalView.instance.HideActivity();

                int quantity = QuartersIAP.Instance.ParseCoinsQuantity(product, QuartersInit.Instance.CurrencyConfig);
                
                SpendRewardView.Instance.Present(quantity);
                
            }, delegate(string balanceError) {
                ModalView.instance.ShowAlert("Balance Error", balanceError, new string[] {"OK"}, null);
            });
        }
    }
}