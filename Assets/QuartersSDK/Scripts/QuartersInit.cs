using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace QuartersSDK {
	public class QuartersInit : MonoBehaviour {

		public Action OnInitComplete;

        public static QuartersInit Instance;
        
        //TODO move those to dedicated class
        [Header("Your Quarters app:")]
        public string APP_ID = "";
        public string APP_KEY = "";
        public string SERVER_API_TOKEN = "";
        public string REDIRECT_URL = "";
		public Environment environment = Environment.production;

        public List<Scope> DefaultScope = new List<Scope>() {
	        Scope.identity,
	        Scope.email,
	        Scope.transactions,
	        Scope.events,
	        Scope.wallet
        };
        
        public CurrencyConfig CurrencyConfig;
        public string FirstScene;

        public static string SDK_VERSION {
	        get { return "2.0.0"; }
        }
        

	

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

			Quarters quartersComponent = quarters.AddComponent<Quarters>();
			quartersComponent.Init();
			
			GameObject quartersWebView = new GameObject("QuartersWebView");
			quarters.transform.SetParent(this.transform);
			QuartersWebView webViewComponent = quarters.AddComponent<QuartersWebView>();
			quartersComponent.QuartersWebView = webViewComponent;
			webViewComponent.Init();
			DontDestroyOnLoad(quartersWebView.gameObject);
			

			Debug.Log("QuartersInit complete");
			this.OnInitComplete?.Invoke();

		}

		public void LoadMainScene() {
			SceneManager.LoadScene(FirstScene, LoadSceneMode.Single);
		}


	}
}

