using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuartersSDK {
	public class QuartersInit : MonoBehaviour {

        public static QuartersInit Instance;
        [Header("Your quarters app:")]
        public string APP_ID = "";
        public string APP_KEY = "";
        public string SERVER_API_TOKEN = "";
		public Environment environment = Environment.production;
        [Header("Configuration:")]
        public bool useAutoapproval = false;



		private static Quarters instance;


		void Awake() {
			DontDestroyOnLoad(this.gameObject);
            Instance = this;
			Init();
		}



		public void Init() {

			if (string.IsNullOrEmpty(APP_ID)) Debug.LogError("Quarters App Id is empty");
			if (string.IsNullOrEmpty(APP_KEY)) Debug.LogError("Quarters App key is empty");
            if (string.IsNullOrEmpty(SERVER_API_TOKEN)) Debug.LogError("Quarters Server Token key is empty");


			GameObject quarters = new GameObject("Quarters");
			quarters.transform.SetParent(this.transform);
			DontDestroyOnLoad(quarters.gameObject);

			instance = quarters.AddComponent<Quarters>();
			instance.Init();

            #if QUARTERS_MODULE_PLAYFAB
            GameObject quartersIAP = new GameObject("QuartersIAP");
            quartersIAP.transform.SetParent(this.transform);
            DontDestroyOnLoad(quartersIAP.gameObject);
            quartersIAP.AddComponent<QuartersIAP>();
            #endif

		}





	}
}

