using System;
using System.Collections;
using System.Collections.Generic;
// using QuartersSDK.Currency;
using UnityEngine;
using UnityEngine.UI;

namespace CoinforgeSDK.UI {
    public class UIUser : MonoBehaviour {

        public Image CurrencyLogo;
        public Text UsernameText;
        public Text CoinsCount;


        private void OnEnable() {
            Coinforge.OnUserLoaded += RefreshUser;

            if (Coinforge.Instance.CurrentUser != null) {
                RefreshUser(Coinforge.Instance.CurrentUser);
            }
        }
        
        private void OnDisable() {
            Coinforge.OnUserLoaded -= RefreshUser;
        }


        
        private void RefreshUser(User user) {
            
            CurrencyLogo.sprite = Coinforge.Instance.CurrencyConfig.CurrencyLogo;
            
            if (user.IsGuestUser) {
                UsernameText.text = "Guest";
            }
            else {
                UsernameText.text = user.displayName;
            }

            Coinforge.Instance.CurrentUser.OnAccountsLoaded += AccountsLoaded;
            if (user.accounts.Count > 0) {
                AccountsLoaded();
                RefreshCoins(Coinforge.Instance.CurrentUser.MainAccount.AvailableCoins);
            }
        }

        
        private void AccountsLoaded() {
           
            Coinforge.Instance.CurrentUser.MainAccount.OnAvailableCoinsUpdated += RefreshCoins;
        }
        
        
        private void RefreshCoins(long availableCoins) {
            CoinsCount.text = availableCoins.ToString();
        }
    }

}
