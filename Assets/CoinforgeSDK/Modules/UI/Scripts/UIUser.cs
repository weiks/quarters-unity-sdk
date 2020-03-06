using System;
using System.Collections;
using System.Collections.Generic;
// using QuartersSDK.Currency;
using UnityEngine;
using UnityEngine.UI;

namespace QuartersSDK.UI {
    public class UIUser : MonoBehaviour {

        public Image CurrencyLogo;
        public Text UsernameText;
        public Text CoinsCount;


        private void OnEnable() {
            Quarters.OnUserLoaded += RefreshUser;

            if (Quarters.Instance.CurrentUser != null) {
                RefreshUser(Quarters.Instance.CurrentUser);
            }
        }
        
        private void OnDisable() {
            Quarters.OnUserLoaded -= RefreshUser;
        }


        
        private void RefreshUser(User user) {
            
            CurrencyLogo.sprite = Quarters.Instance.CurrencyConfig.CurrencyLogo;
            
            if (user.IsGuestUser) {
                UsernameText.text = "Guest";
            }
            else {
                UsernameText.text = user.displayName;
            }

            Quarters.Instance.CurrentUser.OnAccountsLoaded += AccountsLoaded;
            if (user.accounts.Count > 0) {
                AccountsLoaded();
                RefreshCoins(Quarters.Instance.CurrentUser.MainAccount.AvailableCoins);
            }
        }

        
        private void AccountsLoaded() {
           
            Quarters.Instance.CurrentUser.MainAccount.OnAvailableCoinsUpdated += RefreshCoins;
        }
        
        
        private void RefreshCoins(long availableCoins) {
            CoinsCount.text = availableCoins.ToString();
        }
    }

}
