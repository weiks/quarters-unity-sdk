using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using QuartersSDK;

namespace QuartersSDK.UI {
    public class SpendRewardView : UIView {

        public static SpendRewardView Instance;
        [SerializeField] private UIUser uiUser;


        public override void Awake() {
            base.Awake();
            Instance = this;
        }

        public void Present(int deltaDifference) {
        
            ViewWillAppear();
            SetVisible(true);
            ViewAppeared();
            
            uiUser.ToastPresent(deltaDifference, delegate {
                
                //animation complete
                ViewWillDissappear();
                SetVisible(false);
                ViewDisappeared();

            });
        }
        
        
    }
}
