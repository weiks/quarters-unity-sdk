using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace QuartersSDK {
	public class QuartersEditor : MonoBehaviour {

		public InputField codeInput;

		public void CancelTapped() {
			Destroy(this.gameObject);
		}




		public void AuthorizeTapped() {

            Quarters.Instance.RefreshTokenReceived(codeInput.text);
			Destroy(this.gameObject);

		}

	}
}
