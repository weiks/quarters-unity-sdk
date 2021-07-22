using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using QuartersSDK;
using QuartersSDK.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Purchasing;


namespace QuartersSDK.UI {

	public class MenuView : UIView {

		[SerializeField] private UISegue segueToShop;



		public void ButtonShopTapped() {

			Session session = new Session();

			CurrencyConfig config = Quarters.Instance.CurrencyConfig;

	
			segueToShop.Perform();
			
		}

		public void LogoutButtonTapped() {
			Quarters.Instance.Deauthorize();

			SceneManager.LoadScene(0);
		}




		private void AuthorizationSuccess() {

			Debug.Log("AuthorizationSuccess");
			ModalView.instance.ShowActivity();

			QuartersIAP.Instance.Initialize(Quarters.Instance.CurrencyConfig.IAPProductIds, delegate(Product[] products) {

				//products loaded
				Debug.Log("Quarters products loaded: " + products.Length);

				//pull user details
				Quarters.Instance.GetUserDetails(delegate(User user) {
					Quarters.Instance.GetAccounts(delegate(List<User.Account> accounts) { Quarters.Instance.GetAccountBalance(delegate(User.Account.Balance balance) {
						ModalView.instance.HideActivity();
					}, delegate(string getBalanceError) {
						ModalView.instance.ShowAlert("Quarters get balance error", getBalanceError, new string[] {"Try again"}, null);
					}); }, delegate(string getAccountsError) {
						ModalView.instance.ShowAlert("Quarters get user accounts error", getAccountsError, new string[] {"Try again"}, null);
					});
				}, delegate(string getUserDetailsError) {
					ModalView.instance.ShowAlert("Quarters user details error", getUserDetailsError, new string[] {"Try again"}, null);
				});

			}, delegate(InitializationFailureReason reason) { ModalView.instance.ShowAlert("Unable to load products", reason.ToString(), new string[] {"Try again"}, null); });

		}


		private void AuthorizationFailed(string error) {

			Debug.LogError("AuthorizationFailed: " + error);
			ModalView.instance.ShowAlert("Authorization failed", error, new string[] {"Try again"}, null);
		}


		public void DebugProceedToGame() {
			ModalView.instance.ShowAlert("Transfer completed", "User has ben charged and its ready to play", new string[] {"OK"}, null);
		}



		public void AwardCoinsExample(int coinsAmount) {

			Quarters.Instance.Award(coinsAmount, delegate(string transactionHash) {
				
				//player successfully received the coins
				Debug.Log("Coins awarded: " + transactionHash);

			}, delegate(string error) {
				
				//error occurred
				ModalView.instance.ShowAlert("Award error", error, new string[] {"OK"}, null);
				
			});

		}






	}

}









