using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace CoinforgeSDK {
	public class CoinforgeInit : MonoBehaviour {

		public Action OnInitComplete;

        public static CoinforgeInit Instance;
        [Header("Your Coinforge app:")]
        public string APP_ID = "";
        public string APP_KEY = "";
        public string SERVER_API_TOKEN = "";
        public string ETHEREUM_ADDRESS = "";
		public Environment environment = Environment.production;
        [Header("Configuration:")]
        public bool useAutoapproval = false;
        
        public CurrencyConfig CurrencyConfig;
        public string FirstScene;
        public bool CustomShopUI = false;

        public static string SDK_VERSION {
	        get { return "1.0.0"; }
        }
        

		private static Coinforge instance;


		void Awake() {
			DontDestroyOnLoad(this.gameObject);
            Instance = this;
		}



		public void Init(Action OnInitComplete) {

			this.OnInitComplete = OnInitComplete;
			
			Debug.Log("Coinforge Init:");

			if (string.IsNullOrEmpty(APP_ID)) Debug.LogError("Coinforge App Id is empty");
			if (string.IsNullOrEmpty(APP_KEY)) Debug.LogError("Coinforge App key is empty");
            if (string.IsNullOrEmpty(SERVER_API_TOKEN)) Debug.LogError("Coinforge Server Token key is empty");


			GameObject coinforge = new GameObject("Coinforge");
			coinforge.transform.SetParent(this.transform);
			DontDestroyOnLoad(coinforge.gameObject);

			instance = coinforge.AddComponent<Coinforge>();
			instance.Init();

			//init currency
			
            GameObject iap = new GameObject("CoinforgeIAP");
            iap.transform.SetParent(this.transform);
            DontDestroyOnLoad(iap.gameObject);
            iap.AddComponent<CoinforgeIAP>();

			
			Debug.Log("CoinforgeInit complete");
			this.OnInitComplete?.Invoke();

		}

		public void LoadMainScene() {
			SceneManager.LoadScene(FirstScene, LoadSceneMode.Single);
		}


	}
}

