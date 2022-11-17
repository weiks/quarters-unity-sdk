using QuartersSDK.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuartersSDK
{
    public class QuartersInit : MonoBehaviour
    {
        public static QuartersInit Instance;
        public static Action OnInitComplete;

        [Header("Your Quarters app:")]
        [Header("Copy your App ID from your Quarters dashboard")]
        public string APP_ID = "";

        public string APP_UNIQUE_IDENTIFIER = "";
        public static string SDK_VERSION => "3.0.2021";

        public CurrencyConfig CurrencyConfig;

        public List<Scope> DefaultScope = new List<Scope> {
            Scope.identity,
            Scope.email,
            Scope.transactions,
            Scope.events,
            Scope.wallet
        };

        public Environment Environment = Environment.production;
        public LoggingType ConsoleLogging = LoggingType.None;
        public enum LoggingType
        {
            None,
            Verbose
        }


        public string DASHBOARD_URL
        {
            get
            {
                string suffix = string.IsNullOrEmpty(APP_ID) ? "new" : APP_ID;

                if (Environment == Environment.production) return $"https://apps.pocketfulofquarters.com/apps/{suffix}";
                if (Environment == Environment.sandbox) return $"https://sandbox.pocketfulofquarters.com/apps/{suffix}";
                return null;
            }
        }

        public string POQ_APPS_URL
        {
            get
            {
                if (Environment == Environment.production) return "https://www.poq.gg/apps";
                if (Environment == Environment.sandbox) return "https://s2w-dev-firebase.herokuapp.com/apps";
                return null;
            }
        }


        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }


        public void Init(Action OnInitComplete, Action<string> OnInitError)
        {
            Log("Quarters Init:");
            try
            {
                if (string.IsNullOrEmpty(APP_ID)) LogError("Quarters App Id is empty");


                GameObject quarters = new GameObject("Quarters");
                quarters.transform.SetParent(transform);
                DontDestroyOnLoad(quarters.gameObject);

                QuartersController quartersComponent = quarters.AddComponent<QuartersController>();
                quartersComponent.Init();

                GameObject quartersWebView = new GameObject("QuartersWebView");
                quarters.transform.SetParent(transform);
                QuartersWebView webViewComponent = quarters.AddComponent<QuartersWebView>();
                quartersComponent.QuartersWebView = webViewComponent;
                webViewComponent.Init();
                DontDestroyOnLoad(quartersWebView.gameObject);


                Log("QuartersInit complete");
                QuartersInit.OnInitComplete?.Invoke();
                OnInitComplete?.Invoke();

                VspAttribution.VspAttribution.SendAttributionEvent("SDKInit", $"{Constants.VSP_POQ_COMPANY_NAME} {SDK_VERSION}", $"{QuartersInit.Instance.APP_UNIQUE_IDENTIFIER} | {QuartersInit.Instance.APP_ID}");
            }
            catch (Exception ex)
            {
                ModalView.instance.ShowAlert("QuartersInit|Init|Error", $"{ex.Message} \n {ex.StackTrace} ", new string[] { "OK" }, null);
                // throw ex;
            }
        }

        private void Log(string message)
        {
            if (ConsoleLogging == LoggingType.Verbose)
            {
                Debug.Log(message);
            }
        }

        private void LogError(string message)
        {
            if (ConsoleLogging == LoggingType.Verbose)
            {
                Debug.LogError(message);
            }
        }
    }
}