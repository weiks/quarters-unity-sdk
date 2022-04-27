using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace QuartersSDK {
	public class QuartersInit : MonoBehaviour {

		public Action OnInitComplete;

        public static QuartersInit Instance;
        [Header("Your Quarters app:")]
        public string APP_ID = "";
        public string APP_KEY = "";
        public string SERVER_API_TOKEN = "";
		public Environment environment = Environment.production;
        [Header("Configuration:")]
        public bool useAutoapproval = false;
        public List<Scope> DefaultScope = new List<Scope>() {
	        Scope.identity,
	        Scope.email,
	        Scope.transactions,
	        Scope.events,
	        Scope.wallet
        };
        
        public CurrencyConfig CurrencyConfig;
        public string FirstScene;
        public bool CustomShopUI = false;

        public static string SDK_VERSION {
	        get { return "1.0.0"; }
        }
        

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


			GameObject coinforge = new GameObject("Quarters");
			coinforge.transform.SetParent(this.transform);
			DontDestroyOnLoad(coinforge.gameObject);

			instance = coinforge.AddComponent<Quarters>();
			instance.Init();


			Debug.Log("QuartersInit complete");
			this.OnInitComplete?.Invoke();

		}

		public void LoadMainScene() {
			SceneManager.LoadScene(FirstScene, LoadSceneMode.Single);
		}


	}
}

