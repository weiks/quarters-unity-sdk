using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace QuartersSDK.UI {
	public class MenuView : UIView {

		public UISegue SegueToAuthorizeView;

		

		public override void ViewWillAppear(UIView sourceView = null) {
			base.ViewWillAppear(sourceView);

			Quarters.OnSignOut += OnSignOut;
			
		}

		public override void ViewWillDissappear() {
			base.ViewWillDissappear();

			Quarters.OnSignOut -= OnSignOut;
		}

		
		private void OnSignOut() {
			SegueToAuthorizeView.Perform();
		}


	
	}
}