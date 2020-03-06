using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using QuartersSDK.UI;
using QuartersSDK;
using UnityEngine.Purchasing;

public class AuthorizeView : UIView {

	
	public override void ViewAppeared() {
		base.ViewAppeared();
		
		QuartersInit.Instance.Init(delegate {
			
			//show loading
			ModalView.instance.ShowActivity();
		
		
			QuartersSession session = new QuartersSession();

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
					Quarters.Instance.Authorize(QuartersAuthorizationSuccess, QuartersAuthorizationFailed);
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
			Quarters.Instance.GetUserDetails(delegate(User quartersUser) {
				Quarters.Instance.GetAccounts(delegate(List<User.Account> accounts) {
					Quarters.Instance.GetAccountBalance(delegate(User.Account.Balance balance) {
						
						QuartersIAP.Instance.Initialize(Quarters.Instance.CurrencyConfig.IAPProductIds, delegate(Product[] products) {
						
							QuartersInit.Instance.LoadMainScene();
								
						}, delegate(InitializationFailureReason reason) {
							ModalView.instance.ShowAlert("Unable to load products", reason.ToString(), new string[]{"Try again"}, null);
						});

					}, delegate(string getBalanceError) {
					
						ModalView.instance.ShowAlert("Quarters get balance error", getBalanceError, new string[]{"Try again"}, null);
					});

				}, delegate(string getAccountsError) {
				
					ModalView.instance.ShowAlert("Quarters get user accounts error", getAccountsError, new string[]{"Try again"}, null);
				});

			}, delegate(string getUserDetailsError) {
			
				ModalView.instance.ShowAlert("Quarters user details error", getUserDetailsError, new string[]{"Try again"}, null);
			} );
			
			
	
		
		
		
	}


	public void ButtonPlayAsGuestTapped() {
		ModalView.instance.ShowActivity();
		Quarters.Instance.AuthorizeGuest(QuartersAuthorizationSuccess, QuartersAuthorizationFailed);
	}
	

	public void ButtonSignUpTapped() {
		
		QuartersSession session = new QuartersSession();

		ModalView.instance.ShowActivity();
		
		Quarters.Instance.AuthorizeGuest(delegate {
			Quarters.Instance.SignUp(QuartersAuthorizationSuccess, QuartersAuthorizationFailed);
		}, QuartersAuthorizationFailed);
		
		
	}

	
	public void ButtonLoginTapped() {
		Quarters.Instance.Authorize(QuartersAuthorizationSuccess, QuartersAuthorizationFailed);
	}

}