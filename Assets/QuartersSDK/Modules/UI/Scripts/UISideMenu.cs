using System;
using System.Collections;
using System.Collections.Generic;
using QuartersSDK;
using UnityEngine;
using UnityEngine.UI;


public class UISideMenu : MonoBehaviour {

    public RectTransform MenuRect;
    public Image Tint;
    private float tweenTime = 0.15f;
    public Text ShopButtonText;
    

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
    }


    public void HideMenu(bool isAnimated) {
        SetMenuAppearance(false, isAnimated);
    }

    private void Start() {
        ShopButtonText.text = $"Buy " + Quarters.Instance.CurrencyConfig.DisplayNamePlural;
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

}
