using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace QuartersSDK {
	public class QuartersInit : MonoBehaviour {
		
        public static QuartersInit Instance;
        public static Action OnInitComplete;
        
        [Header("Your Quarters app:")]
        [Header("Copy your App ID and App Key from your Quarters dashboard")]
        public string APP_ID = "";
        public string APP_KEY = "";
        public string APP_UNIQUE_IDENTIFIER = "";
		public Environment Environment = Environment.production;

		public CurrencyConfig CurrencyConfig;

        public static string SDK_VERSION {
	        get { return "2.0.0"; }
        }
        
        public List<Scope> DefaultScope = new List<Scope>() {
	        Scope.identity,
	        Scope.email,
	        Scope.transactions, 
	        Scope.events,
	        Scope.wallet
        };
        
        public string DASHBOARD_URL {
	        get {
		        string suffix = string.IsNullOrEmpty(APP_ID) ? "new" : APP_ID;
		        
		        if (Environment == Environment.production) return $"https://apps.pocketfulofquarters.com/apps/{suffix}";
		        else if (Environment == Environment.sandbox) return $"https://sandbox.pocketfulofquarters.com/apps/{suffix}";
		        return null;
	        }
        }



	

		void Awake() {
			DontDestroyOnLoad(this.gameObject);
            Instance = this;
		}



		public void Init(Action OnInitComplete, Action<string> OnInitError) {
			
			Debug.Log("Quarters Init:");

			string error = "";
			
			if (string.IsNullOrEmpty(APP_ID)) Debug.LogError("Quarters App Id is empty");
			if (string.IsNullOrEmpty(APP_KEY)) Debug.LogError("Quarters App key is empty");


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
			QuartersInit.OnInitComplete?.Invoke();
			OnInitComplete?.Invoke();

		}

	
		

	}
}

