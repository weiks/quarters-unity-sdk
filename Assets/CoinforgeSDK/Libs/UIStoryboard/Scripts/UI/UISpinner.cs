using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace CoinforgeSDK.UI {
	[RequireComponent(typeof(Image))]
	public class UISpinner : MonoBehaviour {

		private Image image {
			get {
				return this.gameObject.GetComponent<Image>();
			}
		}
		public bool isActive {
			get {
				return image.enabled;
			}
		}

		private Animator animator {
			get {
				return this.gameObject.GetComponent<Animator>();
			}
		}

		public void Show() {
			image.enabled = true;
			animator.SetBool("spinning", true);
		}


		public void Hide() {
			image.enabled = false;
			animator.SetBool("spinning", false);
		}



	}
}
