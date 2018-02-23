using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quarters {
	public class QuartersInit : MonoBehaviour {

        public static QuartersInit Instance;
        public string APP_ID = "";
        public string APP_KEY = "";


		private static Quarters instance;


		void Awake() {
			DontDestroyOnLoad(this.gameObject);
            Instance = this;
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
