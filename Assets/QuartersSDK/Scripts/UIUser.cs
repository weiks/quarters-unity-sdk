using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace QuartersSDK.UI {
    public class UIUser : MonoBehaviour {

        public Image CurrencyLogo;
        public Text UsernameText;
        public Text CoinsCount;
        public Text DeltaDiferenceText;

        private long currentCoins;
        private Sequence toastSequence = null;
        
        private RectTransform rectTransform {
            get {
                return (RectTransform)this.transform;
            }
        }

        private void OnEnable() {
        
            DeltaDiferenceText.text = "";
       
            Quarters.OnUserLoaded += RefreshUser;

            if (Quarters.Instance != null) {
                if (Quarters.Instance.CurrentUser != null) {
                    RefreshUser(Quarters.Instance.CurrentUser);
                }
            }
        }
        
        private void OnDisable() {
            Quarters.OnUserLoaded -= RefreshUser;
        }


        
        private void RefreshUser(User user) {
            
            CurrencyLogo.sprite = Quarters.Instance.CurrencyConfig.CurrencyIcon;
            
            UsernameText.text = user.GamerTag;
            

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
            
            if (currentCoins != availableCoins) {
                //coins updated
                CoinsCount.rectTransform.DOPunchScale(Vector3.one * 0.5f, 0.3f, 0, 0);
            }

            CoinsCount.text = String.Format("{0:n0}", availableCoins);;
            currentCoins = availableCoins;
        }


   

        public void ToastPresent(int delta, Action OnAnimationComplete) {
            
            if (toastSequence != null) toastSequence.Kill();

            string deltaText = "";
            
            if (delta > 0) {
                deltaText += "+" + delta;
            }
            else if (delta < 0) {
                deltaText += delta;
            }

            DeltaDiferenceText.text = deltaText;

            float topMargin = 0;
            
            UISafeArea safeArea = this.transform.parent.GetComponent<UISafeArea>();
            if (safeArea != null) {
                topMargin = safeArea.topMarginRectSize;
            }
            

            Vector2 hiddenPosition = new Vector2(rectTransform.anchoredPosition.x, rectTransform.rect.height - topMargin);
            rectTransform.anchoredPosition = hiddenPosition;

            Sequence sequence = DOTween.Sequence();
            sequence.Append(rectTransform.DOAnchorPosY(0, 0.33f));
            sequence.AppendInterval(2f);
            sequence.Append(rectTransform.DOAnchorPosY(hiddenPosition.y, 0.33f));

        }
        
        
        
       
    }

}
