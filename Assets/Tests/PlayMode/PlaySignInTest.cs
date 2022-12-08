using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using QuartersSDK;
using QuartersSDK.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class PlaySignInTest
{
    private GameObject game;
    private QuartersWebView quartersView;
    public UISegue SegueToMainMenu;
    private bool isClicked = false;
    private bool isSceneLoaded = false;
    private bool hasReferencesSetup = false;
    private object hasSomeComponentReference;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isSceneLoaded = true;
    }

    void SetupReferences()
    {
        if (hasReferencesSetup)
        {
            return;
        }

        Transform[] objects = Resources.FindObjectsOfTypeAll<Transform>();
        foreach (Transform t in objects)
        {
            if (t.name == "Quarters")
            {
                hasSomeComponentReference = true;
            }
        }

        hasReferencesSetup = true;
    }



    [UnityTest]
    public IEnumerator PlaySignInTestAsPlayerPasses()
    {
        yield return new WaitWhile(() => isSceneLoaded == false);
        SetupReferences();
        Assert.IsNotNull(hasSomeComponentReference);

        var setupButtonGameObject = GameObject.Find("ButtonLogin");
        var setupButton = setupButtonGameObject.GetComponent<Button>();
        Assert.NotNull(setupButton);
        
        setupButton.onClick.AddListener(Clicked);
        setupButton.onClick.Invoke();
        ExecuteEvents.Execute(setupButton.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
        Assert.True(isClicked);
        yield return null;
        yield return new WaitForSeconds(1);

        yield return null;
    }

    private void Clicked()
    {
        isClicked = true;
    }

    private void OnInitComplete()
    {
        try
        {
            if (QuartersController.Instance.IsAuthorized())
            {
                QuartersController.Instance.SignInWithQuarters(OnSignInComplete, OnSignInError);
            }
        }
        catch (Exception ex)
        {
            ModalView.instance.ShowAlert("Error", $"{ex.Message} \n {ex.InnerException}", new string[] { "OK" }, null);
        }
    }

    private void OnSignInComplete()
    {
        try
        {
            SegueToMainMenu.Perform();
        }
        catch (Exception ex)
        {
            ModalView.instance.ShowAlert("Error", $"{ex.Message} \n {ex.InnerException}", new string[] { "OK" }, null);
        }
    }


    private void OnInitError(string error)
    {
        Debug.LogError(error);
    }


    private void OnSignInError(string signInError)
    {
        ModalView.instance.ShowAlert("Error", $"{signInError} ", new string[] { "OK" }, null);
        Debug.Log(signInError);
    }
}
