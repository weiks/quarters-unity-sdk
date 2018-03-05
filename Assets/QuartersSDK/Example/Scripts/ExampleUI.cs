using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

namespace Quarters {
	public class ExampleUI : MonoBehaviour {

		public List<Button> authorizedOnlyButtons = new List<Button>();
        public Text debugConsole;


		void Awake() {
			authorizedOnlyButtons.ForEach(b => b.interactable = false);

            debugConsole.text = "Quarters SDK example";
            debugConsole.text += "\nUnauthorized";
		}


		private void RefreshButtons() {
			authorizedOnlyButtons.ForEach(b => b.interactable = Quarters.Instance.IsAuthorized);
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

			RefreshButtons();
		}


		public void OnAuthorizationFailed(string error) {
			Debug.LogError("OnAuthorizationFailed: " + error);

            debugConsole.text += "\n";
            debugConsole.text += "\nOnAuthorizationFailed: " + error;

			RefreshButtons();
		}




        public void ButtonGetAccountsTapped() {
            
            Quarters.Instance.GetAccounts(delegate (List<User.Account> accounts) {

                debugConsole.text += "\n";
                debugConsole.text += "\nOnGetAccountsSuccess";
                debugConsole.text += JsonConvert.SerializeObject(accounts, Formatting.Indented);

                RefreshButtons();
                
            }, delegate (string error) {

                debugConsole.text += "\n";
                debugConsole.text += "\nOnGetAccountsFailed: " + error;

                RefreshButtons();

            });
        }




        public void ButtonGetFirstAccountBalanceTapped() {

            Quarters.Instance.GetAccountBalance(delegate (User.Account.Balance balance) {

                debugConsole.text += "\n";
                debugConsole.text += "\nOnGetAccountBalanceSuccess";
                debugConsole.text += JsonConvert.SerializeObject(balance, Formatting.Indented);

                RefreshButtons();

            }, delegate (string error) {

                debugConsole.text += "\n";
                debugConsole.text += "\nOnGetAccountBalanceFailed: " + error;

                RefreshButtons();

            });
        }


	}
}
