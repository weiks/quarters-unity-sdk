using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quarters {
	public class QuartersInit : MonoBehaviour {

		public static string APP_ID = "jR3GjKHUUtRoI6e8DPs2";
		public static string APP_KEY = "wfecoqrtyqrvne6l85dd07xvdkwtz06";

		private static Quarters instance;


		void Awake() {
			DontDestroyOnLoad(this.gameObject);
			Init();
		}



		public void Init() {

			GameObject quarters = new GameObject("Quarters");
			quarters.transform.SetParent(this.transform);
			DontDestroyOnLoad(quarters.gameObject);

			instance = quarters.AddComponent<Quarters>();
			instance.Init();
		}





	}
}
