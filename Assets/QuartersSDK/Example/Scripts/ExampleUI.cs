using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using QuartersSDK;


public class ExampleUI : MonoBehaviour {

    public List<CanvasGroup> authorizedOnlyUI = new List<CanvasGroup>();
    public Text debugConsole;

    public InputField tokensInput;
    public InputField descriptionInput;


	void Awake() {
		authorizedOnlyUI.ForEach(b => b.interactable = false);

        debugConsole.text = "Quarters SDK example";
        debugConsole.text += "\nUnauthorized";
	}





	private void RefreshUI() {
		authorizedOnlyUI.ForEach(b => b.interactable = Quarters.Instance.IsAuthorized);
        if (Quarters.Instance.IsAuthorized) {
            authorizedOnlyUI.ForEach(b => b.alpha = 1f);
        }
	}





	public void ButtonAuthorizeTapped() {
		Quarters.Instance.Authorize(OnAuthorizationSuccess, OnAuthorizationFailed);
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

