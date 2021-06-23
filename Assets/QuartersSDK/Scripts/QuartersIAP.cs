#if UNITY_IOS || UNITY_ANDROID
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using Newtonsoft.Json;
using System.IO;
using System.Text.RegularExpressions;

namespace QuartersSDK {
    public class QuartersIAP : MonoBehaviour, IStoreListener  {

        public static QuartersIAP Instance;

        public void Awake() {
            Instance = this;
        }


        /// <summary>
        /// Loaded product lists with definitions and meta data. Can be used to populate UI Shop with ready to purchase products
        /// </summary>
        public List<Product> products = new List<Product>();

        public bool AreProductsLoaded {
            get {
                return products.Count > 0;
            }
        }

        public IStoreController controller;
        private IExtensionProvider extensions;

        public delegate void OnProductsLoadedDelegate (Product[] products);
        public OnProductsLoadedDelegate OnProductsLoaded;

        public delegate void OnInitializeFailedDelegate (InitializationFailureReason reason);
        public OnInitializeFailedDelegate OnInitializeFailedEvent;

        public delegate void PurchaseSucessfull(Product product, string txId);
        public event PurchaseSucessfull PurchaseSucessfullDelegate;


        public delegate void PurchaseFailed(string error);
        public PurchaseFailed PurchaseFailedDelegate;


        

        public bool IsCoinForgeProduct(Product product, CurrencyConfig config) {

            string producId = product.definition.id;

            string[] split = producId.Split('.');

            if (split[split.Length - 2] == config.Code) {

                string quantityString = split[split.Length - 1];

                int quantity = -1;
                if (int.TryParse(quantityString, out quantity)) {

                    return true;
                }
                else {
                    return false;
                }
            }
            else {
                return false;
            }
        }


        public Product GetProduct(string productId) {
            return products.Find(p => p.definition.id == productId);
        }
        

        public int ParseCoinsQuantity(Product product, CurrencyConfig config) {
            
            if (!IsCoinForgeProduct(product, config)) {
                Debug.LogError("Trying to parse non Coinforge product quantity");
            }
            
            string producId = product.definition.id;
            string[] split = producId.Split('.');
            string quantityString = split[split.Length - 1];

            return int.Parse(quantityString);
        }

        
       
        

        public void Initialize(List<string> productIds, OnProductsLoadedDelegate onProductsLoaded, OnInitializeFailedDelegate onInitializeFailed) {

#if UNITY_IOS || UNITY_ANDROID

            Debug.Log("Initialize products: " + JsonConvert.SerializeObject(productIds, Formatting.Indented));

            this.OnProductsLoaded = onProductsLoaded;
            this.OnInitializeFailedEvent = onInitializeFailed;

            StandardPurchasingModule module = StandardPurchasingModule.Instance();
            ConfigurationBuilder builder = ConfigurationBuilder.Instance(module);


            foreach (string productId in productIds) {
                Debug.Log("Initializing product: " + productId);
                builder.AddProduct(productId, ProductType.Consumable);
            }

            UnityPurchasing.Initialize (this, builder);

#else
            Debug.LogError("Purchasing coins through IAP is not supported on this platform");
#endif
            
            
           
        }




        public void OnInitialized (IStoreController controller, IExtensionProvider extensions) {
            this.controller = controller;
            this.extensions = extensions;

            Debug.Log("OnInitialized with products count: " + controller.products.all.Length);

            Debug.Log("Available items:");
            foreach (var item in controller.products.all)
            {
                if (item.availableToPurchase)
                {
                    Debug.Log(string.Join(" - ",
                                          new[]
                    {
                        item.metadata.localizedTitle,
                        item.metadata.localizedDescription,
                        item.metadata.isoCurrencyCode,
                        item.metadata.localizedPrice.ToString(),
                        item.metadata.localizedPriceString,
                        item.transactionID,
                        item.receipt
                    }));
                }
            }

            this.products = new List<Product>(controller.products.all);

            OnProductsLoaded(controller.products.all);
        }



        public void OnInitializeFailed (InitializationFailureReason error) {
            Debug.Log("OnInitializeFailed: " + error.ToString());

            if (OnInitializeFailedEvent != null) OnInitializeFailedEvent(error);
        }





        #region Purchasing


        public void BuyProduct(Product product, PurchaseSucessfull purchaseSucessfullDelegate, PurchaseFailed purchaseFailedDelegate ) {
            
            if (!IsCoinForgeProduct(product, Quarters.Instance.CurrencyConfig)) {
                purchaseFailedDelegate("Incorrect product id");
            }
            
            if (Application.isEditor) {
                purchaseFailedDelegate("Buying IAP is not supported in Unity Editor, please test on iOS or Android device");
                return;
            }


            if (products.Count == 0) {
                purchaseFailedDelegate("No products loaded. Call CoinforgeIAP.Initialize first!");
                return;
            }

            Debug.Log("Buying Coinforge: " + product.definition.storeSpecificId);

            #if UNITY_IOS || UNITY_ANDROID

                this.PurchaseSucessfullDelegate = purchaseSucessfullDelegate;
                this.PurchaseFailedDelegate = purchaseFailedDelegate;
                controller.InitiatePurchase(product);

            #else
                Debug.LogError("Purchasing coins through IAP is not supported on this platform");
            #endif
        }



        public void OnPurchaseFailed (Product i, PurchaseFailureReason p) {

            Debug.Log("OnPurchaseFailed : " + p.ToString());
            Debug.Log(JsonConvert.SerializeObject(i));

            ProductMetadata metaData = i.metadata;

            if (this.PurchaseFailedDelegate != null) this.PurchaseFailedDelegate(p.ToString());

        }





        public PurchaseProcessingResult ProcessPurchase (PurchaseEventArgs e){
            
            Debug.Log("ProcessPurchase: " + e.purchasedProduct.definition.id);

            StartCoroutine(VerifyPurchase(e));

            return PurchaseProcessingResult.Pending;
        }




        public IEnumerator VerifyPurchase(PurchaseEventArgs e) {

            Debug.Log("Verify purchase: " + JsonConvert.SerializeObject(e));

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Content-Type", "application/json;charset=UTF-8");
            headers.Add("x-api-key", QuartersInit.Instance.SERVER_API_TOKEN);

            Debug.Log("headers: " + JsonConvert.SerializeObject(headers));

            string url = Quarters.Instance.API_URL + "/apps/" + QuartersInit.Instance.APP_ID + "/verifyReceipt/unity";
            Debug.Log("url " + url);


            Dictionary<string, string> receiptData = JsonConvert.DeserializeObject<Dictionary<string, string>>(e.purchasedProduct.receipt);


            Hashtable receipt = new Hashtable();
            receipt.Add("Store", receiptData["Store"]);
            receipt.Add("TransactionID", receiptData["TransactionID"]);
            receipt.Add("Payload", receiptData["Payload"]);

            Debug.Log("CurrentUser: " + Quarters.Instance.CurrentUser);

  
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("receipt", receipt);
            data.Add("user", Quarters.Instance.CurrentUser.userId);
            

            string dataJson = JsonConvert.SerializeObject(data);
            Debug.Log(dataJson);
            byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(dataJson);


            WWW www = new WWW(url, dataBytes, headers);
            Debug.Log(www.url);

            while (!www.isDone) yield return new WaitForEndOfFrame();

            if (!string.IsNullOrEmpty(www.error)) {
                if (!string.IsNullOrEmpty(www.text)) {
                    Debug.Log(www.text);
                }
                Debug.LogError(www.error);
                
                Hashtable errorData = JsonConvert.DeserializeObject<Hashtable>(www.text);

                if (errorData.Contains("processed")) {
                    if (www.text.Contains("Error: Already Processed")) {
                        Debug.Log("Consuming already processed transaction: ");
                        controller.ConfirmPendingPurchase(e.purchasedProduct);
                        if (PurchaseFailedDelegate != null) PurchaseFailedDelegate("Transaction as already processed, please try again.");
                    }
                }
                
                else {
                    if (PurchaseFailedDelegate != null) PurchaseFailedDelegate(www.error);
                }

            }
            else {
                Debug.Log(www.text);
                Hashtable resultData = JsonConvert.DeserializeObject<Hashtable>(www.text);

                //consume product

                if (resultData.ContainsKey("txId")) {
                    string txId = (string)resultData["txId"];
                    Debug.Log("Consume product");
                    controller.ConfirmPendingPurchase(e.purchasedProduct);

                    if (PurchaseSucessfullDelegate != null) PurchaseSucessfullDelegate(e.purchasedProduct, txId);
                }
                else {
                    if (PurchaseFailedDelegate != null) PurchaseFailedDelegate(www.text);
                }

            }

        }



        #endregion
    	
    }
}

#endif