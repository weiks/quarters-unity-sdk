using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using CoinforgeSDK.UI;
using CoinforgeSDK;
using UnityEngine.Purchasing;

public class AuthorizeView : UIView {

	
	public override void ViewAppeared() {
		base.ViewAppeared();
		
		CoinforgeInit.Instance.Init(delegate {
			
			//show loading
			ModalView.instance.ShowActivity();
		
		
			Session session = new Session();

			//quarters
			if (!session.IsAuthorized) {
				//first session
				ModalView.instance.HideActivity();
			}
			else {
				//not first session
				if (session.IsGuestSession) {
					//following session with guest mode display dialog
					ModalView.instance.HideActivity();
				}
				else {
					//email user
					Coinforge.Instance.Authorize(QuartersAuthorizationSuccess, QuartersAuthorizationFailed);
				}
			}
		});

	}
	
	
	
	
	
	//TODO add retrying coroutine to all error UX
	private void QuartersAuthorizationFailed(string error) {

		Debug.LogError("QuartersAuthorizationFailed: " + error);
		ModalView.instance.ShowAlert("Authorization failed", error, new string[]{"Try again"}, null);
	}
	
	
	//todo move this outside the view
	private void QuartersAuthorizationSuccess() {

		Debug.Log("QuartersAuthorizationSuccess");
		ModalView.instance.ShowActivity();
		
			
			//pull user details
			Coinforge.Instance.GetUserDetails(delegate(User quartersUser) {
				Coinforge.Instance.GetAccounts(delegate(List<User.Account> accounts) {
					Coinforge.Instance.GetAccountBalance(delegate(User.Account.Balance balance) {
						
						CoinforgeIAP.Instance.Initialize(Coinforge.Instance.CurrencyConfig.IAPProductIds, delegate(Product[] products) {
						
							CoinforgeInit.Instance.LoadMainScene();
								
						}, delegate(InitializationFailureReason reason) {
							ModalView.instance.ShowAlert("Unable to load products", reason.ToString(), new string[]{"Try again"}, null);
						});

					}, delegate(string getBalanceError) {
					
						ModalView.instance.ShowAlert("Coinforge get balance error", getBalanceError, new string[]{"Try again"}, null);
					});

				}, delegate(string getAccountsError) {
				
					ModalView.instance.ShowAlert("Coinforge get user accounts error", getAccountsError, new string[]{"Try again"}, null);
				});

			}, delegate(string getUserDetailsError) {
			
				ModalView.instance.ShowAlert("Coinforge user details error", getUserDetailsError, new string[]{"Try again"}, null);
			} );
			
			
	
		
		
		
	}


	public void ButtonPlayAsGuestTapped() {
		ModalView.instance.ShowActivity();
		Coinforge.Instance.AuthorizeGuest(QuartersAuthorizationSuccess, QuartersAuthorizationFailed);
	}
	

	public void ButtonSignUpTapped() {
		
		Session session = new Session();

		ModalView.instance.ShowActivity();
		
		Coinforge.Instance.AuthorizeGuest(delegate {
			Coinforge.Instance.SignUp(QuartersAuthorizationSuccess, QuartersAuthorizationFailed);
		}, QuartersAuthorizationFailed);
		
		
	}

	
	public void ButtonLoginTapped() {
		Coinforge.Instance.Authorize(QuartersAuthorizationSuccess, QuartersAuthorizationFailed);
	}

}