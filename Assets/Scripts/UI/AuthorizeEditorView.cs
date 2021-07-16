using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace QuartersSDK.UI {
	public class AuthorizeEditorView : UIView {

		public static AuthorizeEditorView Instance;
		
		public InputField CodeInput;


		public override void Awake() {
			base.Awake();
			Instance = this;
		}

		public void Show() {
			ViewWillAppear();
			SetVisible(true);
			ViewAppeared();
		}
		
		
		public void ButtonAuthorizeTapped() {
			Quarters.Instance.AuthorizationCodeReceived(CodeInput.text);
			ViewWillDissappear();
			SetVisible(false);
			ViewDisappeared();
		}
		
		
		
	}
}