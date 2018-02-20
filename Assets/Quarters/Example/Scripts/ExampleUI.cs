using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Quarters {
	public class ExampleUI : MonoBehaviour {



		void Start () {
			QuartersInit.Init();
		}




		public void ButtonAuthorizeTapped() {

			Quarters.Instance.Authorize();

		

		}





	}
}
