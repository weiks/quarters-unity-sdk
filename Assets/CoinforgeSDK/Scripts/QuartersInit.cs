using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// using QuartersSDK.Currency;

namespace QuartersSDK {
	public class QuartersInit : MonoBehaviour {

		public Action OnInitComplete;

        public static QuartersInit Instance;
        [Header("Your quarters app:")]
        public string APP_ID = "";
        public string APP_KEY = "";
        public string SERVER_API_TOKEN = "";
        public string ETHEREUM_ADDRESS = "";
		public Environment environment = Environment.production;
        [Header("Configuration:")]
        public bool useAutoapproval = false;
        
        public CurrencyConfig CurrencyConfig;
        public string FirstScene;
        

		private static Quarters instance;


		void Awake() {
			DontDestroyOnLoad(this.gameObject);
            Instance = this;
		}



		public void Init(Action OnInitComplete) {

			this.OnInitComplete = OnInitComplete;
			
			Debug.Log("Quarters Init:");

			if (string.IsNullOrEmpty(APP_ID)) Debug.LogError("Quarters App Id is empty");
			if (string.IsNullOrEmpty(APP_KEY)) Debug.LogError("Quarters App key is empty");
            if (string.IsNullOrEmpty(SERVER_API_TOKEN)) Debug.LogError("Quarters Server Token key is empty");


			GameObject quarters = new GameObject("Quarters");
			quarters.transform.SetParent(this.transform);
			DontDestroyOnLoad(quarters.gameObject);

			instance = quarters.AddComponent<Quarters>();
			instance.Init();

			//init currency
			
			
			
            #if QUARTERS_MODULE_IAP
            GameObject quartersIAP = new GameObject("QuartersIAP");
            quartersIAP.transform.SetParent(this.transform);
            DontDestroyOnLoad(quartersIAP.gameObject);
            quartersIAP.AddComponent<QuartersIAP>();
            
            #endif
			
			Debug.Log("QuartersInit complete");
			this.OnInitComplete?.Invoke();

		}

		public void LoadMainScene() {
			SceneManager.LoadScene(FirstScene, LoadSceneMode.Single);
		}


	}
}

