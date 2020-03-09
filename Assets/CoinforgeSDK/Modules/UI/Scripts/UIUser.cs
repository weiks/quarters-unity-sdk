using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace CoinforgeSDK.UI {
    public class UIUser : MonoBehaviour {

        public Image CurrencyLogo;
        public Text UsernameText;
        public Text CoinsCount;

        private long currentCoins;


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
            
            CurrencyLogo.sprite = Coinforge.Instance.CurrencyConfig.CurrencyIcon;
            
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
            
            if (currentCoins != availableCoins) {
                //coins updated
                CoinsCount.rectTransform.DOPunchScale(Vector3.one * 0.5f, 0.3f, 0, 0);
            }
            
            
            CoinsCount.text = String.Format("{0:n0}", availableCoins);;
            currentCoins = availableCoins;

        }
    }

}
