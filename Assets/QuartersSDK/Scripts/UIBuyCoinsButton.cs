using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuartersSDK;
using UnityEngine.UI;
using UnityEngine.Purchasing;

namespace QuartersSDK.UI {
    
    [RequireComponent(typeof(Button))]
    public class UIBuyCoinsButton : MonoBehaviour {
        
        public CurrencyConfig CurrencyConfig;
        public Text PriceText;
        public Text QuantityText;
        
        //select the product
        public string ProductId {
            get {
                if (CurrencyConfig == null) return null;

                return CurrencyConfig.GetProductId(Quantity);
            }
        }
        [HideInInspector] public int Quantity;


        private Button button {
            get { return this.GetComponent<Button>(); }
        }
        
        private void OnEnable() {
            QuantityText.text = Quantity.ToString();
            Product product = QuartersIAP.Instance.GetProduct(ProductId);
            PriceText.text = product.metadata.localizedPriceString;
        }


        public void ButtonTapped() {
            
            Session session = new Session();

            CurrencyConfig config = Quarters.Instance.CurrencyConfig;
            
            ProceedToPurchase();
            

            
            
            
           
        }
        
        private void ProceedToPurchase() {
            ModalView.instance.ShowActivity();
            Product product = QuartersIAP.Instance.GetProduct(ProductId);
            QuartersIAP.Instance.BuyProduct(product, PurchaseSucessfullDelegate, PurchaseFailedDelegate);
        }
        

        private void PurchaseFailedDelegate(string error) {
            ModalView.instance.ShowAlert("Purchase Error", error, new string[] {"OK"}, null);
        }

        private void PurchaseSucessfullDelegate(Product product, string txid) {
            //refresh balance
            Quarters.Instance.GetAccountBalance(delegate(User.Account.Balance newBalance) {
                ModalView.instance.HideActivity();
                
                SpendRewardView.Instance.Present(Quantity);
                
            }, delegate(string balanceError) {
                ModalView.instance.ShowAlert("Balance Error", balanceError, new string[] {"OK"}, null);
            });
        }

        public void OnQuantityUpdated() {
            QuantityText.text = Quantity.ToString();
        }

    }
}

