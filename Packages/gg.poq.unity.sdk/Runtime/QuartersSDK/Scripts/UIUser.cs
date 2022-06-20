using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace QuartersSDK.UI {
    public class UIUser : MonoBehaviour {
        public Image Avatar;
        public Text UsernameText;
        public Text CoinsCount;
        public Text DeltaDiferenceText;
        public Sprite emptyAvatar;
        public Animation Animation;

        private long currentCoins;


        private RectTransform rectTransform {
            get { return (RectTransform) this.transform; }
        }

        private void OnEnable() {
            QuartersInit.OnInitComplete += Init;
        }

        private void OnDestroy() {
            Quarters.OnUserLoaded -= RefreshUser;
            Quarters.OnBalanceUpdated -= RefreshCoins;
        }


        private void Init() {
            DeltaDiferenceText.text = "";

            Quarters.OnUserLoaded += RefreshUser;
            Quarters.OnBalanceUpdated += RefreshCoins;
        }


        private void RefreshUser(User user) {
            UsernameText.text = user.GamerTag;

            RefreshCoins(Quarters.Instance.CurrentUser.Balance);

            Quarters.Instance.GetAccountBalanceCall(RefreshCoins, delegate(string error) { Debug.LogError(error); });
        }


        private void RefreshCoins(long availableCoins) {
            CoinsCount.text = String.Format("{0:n0}", availableCoins);
            currentCoins = availableCoins;
            Debug.Log($"Current coins: {currentCoins}");
        }


        public void ToastPresent(int delta, Action OnAnimationComplete) {
            Debug.Log($"Delta: {delta}");

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

            CoinsCount.text = String.Format("{0:n0}", currentCoins - delta);
            Animation.Play("ToastMessage");
        }


        //animation event
        public void UpdateBalance() {
            CoinsCount.text = String.Format("{0:n0}", currentCoins);
            DeltaDiferenceText.text = string.Empty;
        }
    }
}