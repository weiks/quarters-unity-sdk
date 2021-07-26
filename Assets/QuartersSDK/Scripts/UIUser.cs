using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace QuartersSDK.UI {
    public class UIUser : MonoBehaviour {
        
        public Image Avatar;
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
            Quarters.Instance.CurrentUser.OnBalanceUpdated += RefreshCoins;
        }
        
        private void OnDisable() {
            Quarters.OnUserLoaded -= RefreshUser;
            Quarters.Instance.CurrentUser.OnBalanceUpdated -= RefreshCoins;
        }


        
        private void RefreshUser(User user) {

            UsernameText.text = user.GamerTag;
            
            RefreshCoins(Quarters.Instance.CurrentUser.Balance);
            
            
            //refresh avatar
            StartCoroutine(Quarters.Instance.GetAvatar(delegate(Texture avatar) {

                Rect rect = new Rect(0.0f, 0.0f, avatar.width, avatar.height);
                
                Sprite avatarSprite = Sprite.Create((Texture2D)avatar, rect, new Vector2(0.5f, 0.5f));

                Avatar.sprite = avatarSprite;

            }, null));


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
