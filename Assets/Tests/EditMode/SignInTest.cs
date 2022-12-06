using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class SignInTest
{
    bool isErrorOnSignIn = false;
    private void OnSignInComplete()
    {
        Debug.LogAssertion("login successful");
    }

    private void OnSignInError(string signInError)
    {
        isErrorOnSignIn = true;
        Debug.Log(signInError);
    }

    // Test if QuartersInit and QuartersController are instantiated and not authorized while not instantiated 
    [Test]
    public void SignInTestInstantiates()
    {
        GameObject quarters = new GameObject("Quarters");
        QuartersSDK.QuartersInit.Instance  = new QuartersSDK.QuartersInit();

        QuartersSDK.QuartersController quartersComponent = quarters.AddComponent<QuartersSDK.QuartersController>();
        quartersComponent.Init();
        Assert.IsFalse(QuartersSDK.QuartersController.Instance._session.IsAuthorized);
    }

    // Test if QuartersInit and QuartersController are instantiated and not authorized while not instantiated 
    [Test]
    public void SignInTestLogin()
    {
        GameObject quarters = new GameObject("Quarters");
        QuartersSDK.QuartersInit.Instance = new QuartersSDK.QuartersInit();
        QuartersSDK.QuartersInit.Instance.APP_ID = "3yh0HFnykT9cL1iwDHhA";
        QuartersSDK.QuartersInit.Instance.APP_UNIQUE_IDENTIFIER = "exampleapp";
        QuartersSDK.QuartersController quartersComponent = quarters.AddComponent<QuartersSDK.QuartersController>();
        quartersComponent.Init();
        quartersComponent.QuartersWebView = new QuartersSDK.QuartersWebView();
        
        quartersComponent.SignInWithQuarters(OnSignInComplete, OnSignInError);
        Assert.IsFalse(isErrorOnSignIn);
    }

}
