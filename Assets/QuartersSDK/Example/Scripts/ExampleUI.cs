using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using QuartersSDK;
#if QUARTERS_MODULE_PLAYFAB
using PlayFab;
using PlayFab.ClientModels;
#endif


public class ExampleUI : MonoBehaviour {

    public List<CanvasGroup> authorizedOnlyUI = new List<CanvasGroup>();
    public List<CanvasGroup> unAuthorizedOnlyUI = new List<CanvasGroup>();
    public Text debugConsole;

    public InputField tokensInput;
    public InputField descriptionInput;


	void Start() {

        debugConsole.text = "Quarters SDK example";
        debugConsole.text += "\nUnauthorized";

        RefreshUI();
	}





	private void RefreshUI() {
		authorizedOnlyUI.ForEach(b => b.interactable = Quarters.Instance.IsAuthorized);
        unAuthorizedOnlyUI.ForEach(b => b.interactable = !Quarters.Instance.IsAuthorized);


        if (Quarters.Instance.IsAuthorized) {
            authorizedOnlyUI.ForEach(b => b.alpha = 1f);
            unAuthorizedOnlyUI.ForEach(b => b.alpha = 0.4f);
        }
        else {
            authorizedOnlyUI.ForEach(b => b.alpha = 0.4f);
            unAuthorizedOnlyUI.ForEach(b => b.alpha = 1f);
        }

	}





	public void ButtonAuthorizeTapped() {
		Quarters.Instance.Authorize(OnAuthorizationSuccess, OnAuthorizationFailed);
	}

    public void ButtonDeauthorizeTapped() {
        Quarters.Instance.Deauthorize();
        RefreshUI();
    }

	public void ButtonGetUserDetailsTapped() {
		Quarters.Instance.GetUserDetails(delegate(User user) {
			Debug.Log("User loaded");

            debugConsole.text += "\n";
            debugConsole.text += "\nUser loaded: ";
            debugConsole.text += JsonConvert.SerializeObject(user, Formatting.Indented);

		}, delegate (string error) {
			Debug.LogError("Cannot load the user details: " + error);
            debugConsole.text += "\n";
            debugConsole.text += "\nCannot load the user details:: " + error;
		});
	}





	public void OnAuthorizationSuccess() {
		Debug.Log("OnAuthorizationSuccess");

        debugConsole.text += "\n";
        debugConsole.text += "\nOnAuthorizationSuccess";

		RefreshUI();
	}


	public void OnAuthorizationFailed(string error) {
		Debug.LogError("OnAuthorizationFailed: " + error);

        debugConsole.text += "\n";
        debugConsole.text += "\nOnAuthorizationFailed: " + error;

		RefreshUI();
	}




    public void ButtonGetAccountsTapped() {
        
        Quarters.Instance.GetAccounts(delegate (List<User.Account> accounts) {

            debugConsole.text += "\n";
            debugConsole.text += "\nOnGetAccountsSuccess";
            debugConsole.text += JsonConvert.SerializeObject(accounts, Formatting.Indented);

            RefreshUI();
            
        }, delegate (string error) {

            debugConsole.text += "\n";
            debugConsole.text += "\nOnGetAccountsFailed: " + error;

            RefreshUI();

        });
    }




    public void ButtonGetFirstAccountBalanceTapped() {

        Quarters.Instance.GetAccountBalance(delegate (User.Account.Balance balance) {

            debugConsole.text += "\n";
            debugConsole.text += "\nOnGetAccountBalanceSuccess";
            debugConsole.text += JsonConvert.SerializeObject(balance, Formatting.Indented);

            RefreshUI();

        }, delegate (string error) {

            debugConsole.text += "\n";
            debugConsole.text += "\nOnGetAccountBalanceFailed: " + error;

            RefreshUI();

        });
    }




    public void ButtonGetAccountRewardTapped() {

        #if QUARTERS_MODULE_PLAYFAB

        //login user to playfab title using device id
        LoginWithCustomIDRequest loginRequest = new LoginWithCustomIDRequest();
        loginRequest.CustomId = SystemInfo.deviceUniqueIdentifier;
        loginRequest.CreateAccount = true;


        PlayFabClientAPI.LoginWithCustomID(loginRequest, delegate(LoginResult result) {

            Debug.Log("Playfab user logged in: " + result.PlayFabId);

            //Request 2 quarters from Playfab Cloud build
            Quarters.Instance.AwardQuarters(2, delegate(string transactionHash) {

                Debug.Log("Quarters awarded: " + transactionHash);

            }, delegate (string error) {

                debugConsole.text += "\n";
                debugConsole.text += "\nOnAwardQuartersFailed: " + error;

                RefreshUI();

            });

        }, delegate (PlayFabError error){
            Debug.LogError(error.ErrorMessage);
        });



        #else
        Debug.LogError("Quarters module: Playfab, is not enabled. Add QUARTERS_MODULE_PLAYFAB scripting define in Player settings");
        #endif

    }










    public void ButtonTransferTapped() {

        TransferAPIRequest request = new TransferAPIRequest(int.Parse(tokensInput.text), descriptionInput.text, delegate (string transactionHash) {
        
            debugConsole.text += "\n";
            debugConsole.text += "\nTransfer successful, transactionHash: " + transactionHash;
            Debug.Log("Console: " + debugConsole.text);

        }, delegate (string error) {
        
            debugConsole.text += "\n";
            debugConsole.text += "\nOnTransactionFailed: " + error;
            Debug.Log("Console: " + debugConsole.text);
        });



        Quarters.Instance.CreateTransfer(request);


    }



}

