using System;
using System.Collections;
using System.Collections.Generic;
using QuartersSDK;
using QuartersSDK.UI;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Purchasing;


public class UISideMenu : MonoBehaviour {

    public UIStoryboard Storyboard;
    public RectTransform MenuRect;
    public Image Tint;
    private float tweenTime = 0.15f;
    public Text ShopButtonText;

    public Button SignUpButton;
    public Button LogoutButton;
    

    private Vector2 hiddenPosition {
        get {
            return Vector2.zero;
        }
    }

    private Vector2 visiblePosition {
        get {
            return new Vector2(-MenuRect.sizeDelta.x, 0);
        }
    }

    private bool IsVisible = false;


    private void Awake() {
        HideMenu(false);
    }


    public void ShowMenu() {
        SetMenuAppearance(true, true);
        
        QuartersSession session = new QuartersSession();
        SignUpButton.gameObject.SetActive(session.IsGuestSession);
        LogoutButton.interactable = !session.IsGuestSession;

        ShopButtonText.text = $"Buy " + Quarters.Instance.CurrencyConfig.DisplayNamePlural;
    }


    public void HideMenu(bool isAnimated) {
        SetMenuAppearance(false, isAnimated);
    }



    private void SetMenuAppearance(bool isVisible, bool isAnimated) {

        float transitionTime = isAnimated ? tweenTime : 0;

        Vector2 targetPosition = isVisible ? visiblePosition : hiddenPosition;

        Hashtable tweenHash = iTween.Hash("from", MenuRect.anchoredPosition.x, "to", targetPosition.x, "time", transitionTime, "isLocal", true, "onupdate", "UpdatePosition", "onupdatetarget", this.gameObject);
        this.IsVisible = isVisible;
        iTween.ValueTo(MenuRect.gameObject, tweenHash);

        float startAlpha = Tint.color.a;
        float targetAlpha = isVisible ? tweenTime : 0;
        iTween.ValueTo(this.gameObject, iTween.Hash("from", startAlpha, "to", targetAlpha, "time", transitionTime, "onupdate", "SetAlpha"));
        
        
    }

    public void UpdatePosition(float xPosition) {
        MenuRect.anchoredPosition = new Vector2(xPosition, MenuRect.anchoredPosition.y);
    }
    
    
    public void SetAlpha(float alpha) {
        Tint.enabled = alpha > 0;
        
        Tint.color = new Color(Tint.color.r, Tint.color.g, Tint.color.b, alpha);
    }


    public void SignUpButtonTapped() {
        ModalView.instance.ShowActivity();Quarters.Instance.SignUp(QuartersAuthorizationSuccess, QuartersAuthorizationFailed);
    }

    public void LogoutButtonTapped() {
        Quarters.Instance.Deauthorize();
        
        Storyboard.Start();
    }
    
    

    
    private void QuartersAuthorizationSuccess() {

        Debug.Log("QuartersAuthorizationSuccess");
        ModalView.instance.ShowActivity();
		
        QuartersIAP.Instance.Initialize(Quarters.Instance.CurrencyConfig.IAPProductIds, delegate(Product[] products) {
			
            //products loaded
            Debug.Log("Quarters products loaded: " + products.Length);
			
            //pull user details
            Quarters.Instance.GetUserDetails(delegate(User quartersUser) {
                Quarters.Instance.GetAccounts(delegate(List<User.Account> accounts) {
                    Quarters.Instance.GetAccountBalance(delegate(User.Account.Balance balance) {
					
                        ModalView.instance.HideActivity();
                        

                    }, delegate(string getBalanceError) {
					
                        ModalView.instance.ShowAlert("Quarters get balance error", getBalanceError, new string[]{"Try again"}, null);
                    });

                }, delegate(string getAccountsError) {
				
                    ModalView.instance.ShowAlert("Quarters get user accounts error", getAccountsError, new string[]{"Try again"}, null);
                });

            }, delegate(string getUserDetailsError) {
			
                ModalView.instance.ShowAlert("Quarters user details error", getUserDetailsError, new string[]{"Try again"}, null);
            } );
			
			
			
        }, delegate(InitializationFailureReason reason) {
            ModalView.instance.ShowAlert("Unable to load products", reason.ToString(), new string[]{"Try again"}, null);
        });
        
    }
    
    
    private void QuartersAuthorizationFailed(string error) {

        Debug.LogError("QuartersAuthorizationFailed: " + error);
        ModalView.instance.ShowAlert("Authorization failed", error, new string[]{"Try again"}, null);
    }
    
}
