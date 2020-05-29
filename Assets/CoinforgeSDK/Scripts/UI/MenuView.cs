using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using CoinforgeSDK;
using CoinforgeSDK.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Purchasing;


namespace CoinforgeSDK.UI {

	public class MenuView : UIView {

		[SerializeField] private UISegue segueToShop;



		public void ButtonShopTapped() {

			Session session = new Session();

			CurrencyConfig config = Coinforge.Instance.CurrencyConfig;

			if (session.IsGuestSession) {

				string[] buttonNames = new string[] {"Cancel", "Sign up"};

				ModalView.instance.ShowAlert($"Sign up to purchase {config.DisplayNamePlural}",
					$"You need to sign up or login to purchase {config.DisplayNamePlural}",
					buttonNames, delegate(string buttonTapped) {

						if (buttonTapped == buttonNames[1]) {
							SignUpButtonTapped();
						}
					});
			}
			else {
				segueToShop.Perform();
			}




		}



		public void SignUpButtonTapped() {
			ModalView.instance.ShowActivity();
			Coinforge.Instance.SignUp(AuthorizationSuccess, AuthorizationFailed);
		}

		public void LogoutButtonTapped() {
			Coinforge.Instance.Deauthorize();

			SceneManager.LoadScene(0);
		}




		private void AuthorizationSuccess() {

			Debug.Log("AuthorizationSuccess");
			ModalView.instance.ShowActivity();

			CoinforgeIAP.Instance.Initialize(Coinforge.Instance.CurrencyConfig.IAPProductIds, delegate(Product[] products) {

				//products loaded
				Debug.Log("Coinforge products loaded: " + products.Length);

				//pull user details
				Coinforge.Instance.GetUserDetails(delegate(User user) {
					Coinforge.Instance.GetAccounts(delegate(List<User.Account> accounts) { Coinforge.Instance.GetAccountBalance(delegate(User.Account.Balance balance) { ModalView.instance.HideActivity(); }, delegate(string getBalanceError) { ModalView.instance.ShowAlert("Coinforge get balance error", getBalanceError, new string[] {"Try again"}, null); }); }, delegate(string getAccountsError) { ModalView.instance.ShowAlert("Coinforge get user accounts error", getAccountsError, new string[] {"Try again"}, null); });
				}, delegate(string getUserDetailsError) {
					ModalView.instance.ShowAlert("Coinforge user details error", getUserDetailsError, new string[] {"Try again"}, null);
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

			Coinforge.Instance.Award(coinsAmount, delegate(string transactionHash) {
				
				//player successfully received the coins
				Debug.Log("Coins awarded: " + transactionHash);

			}, delegate(string error) {
				
				//error occurred
				ModalView.instance.ShowAlert("Award error", error, new string[] {"OK"}, null);
				
			});

		}






	}

}









