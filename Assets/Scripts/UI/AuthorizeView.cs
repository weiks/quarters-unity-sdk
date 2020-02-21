using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using QuartersSDK.UI;
using QuartersSDK;

public class AuthorizeView : UIView {
	
	public override void ViewAppeared() {
		base.ViewAppeared();
		
		//show loading
		ModalView.instance.ShowActivity();
		
		
		QuartersSession session = new QuartersSession();

		//quarters
		if (!session.IsAuthorized) {
			//first session
			Quarters.Instance.AuthorizeGuest(QuartersAuthorizationSuccess, QuartersAuthorizationFailed);
		}
		else {
			//not first session
			if (session.IsGuestSession) {
				//following session with guest mode display dialog
				Quarters.Instance.AuthorizeGuest(QuartersAuthorizationSuccess, QuartersAuthorizationFailed);
			}
			else {
				//email user
				Quarters.Instance.Authorize(QuartersAuthorizationSuccess, QuartersAuthorizationFailed);
			}
		}
	}
	
	
	//TODO add retrying coroutine to all error UX
	private void QuartersAuthorizationFailed(string error) {

		Debug.LogError("QuartersAuthorizationFailed: " + error);
		ModalView.instance.ShowAlert("Quarters get balance error", error, new string[]{"Try again"}, null);
	}
	
	
	private void QuartersAuthorizationSuccess() {

		Debug.Log("QuartersAuthorizationSuccess");
		
	
		//pull user details
		Quarters.Instance.GetUserDetails(delegate(User quartersUser) {
			
			Quarters.Instance.GetAccounts(delegate(List<User.Account> accounts) {
				
				Quarters.Instance.GetAccountBalance(delegate(User.Account.Balance balance) {
					
					ModalView.instance.HideActivity();
					Debug.Log("All user data loaded");
					
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



}