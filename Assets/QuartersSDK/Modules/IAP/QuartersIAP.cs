using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.ClientModels;
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



        public bool IsQuartersProduct(Product product) {
            return product.definition.id.Contains(Constants.QUARTERS_PRODUCT_KEY);
        }



        public int ParseQuartersQuantity(Product product) {
            if (!IsQuartersProduct(product)) {
                Debug.LogError("Trying to parse non Quarters product quantity");
            }

            return int.Parse(product.definition.id.Replace(Constants.QUARTERS_PRODUCT_KEY, ""));
        }



        public void Initialize(List<string> productIds, OnProductsLoadedDelegate onProductsLoaded, OnInitializeFailedDelegate onInitializeFailed) {

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
            if (!IsQuartersProduct(product)) return;


            if (products.Count == 0) {
                Debug.LogError("No products loaded. Call QuartersIAP.Initialize first!");
                return;
            }

            Debug.Log("Buying Quarters: " + product.definition.storeSpecificId);

            #if UNITY_IOS || UNITY_ANDROID

                this.PurchaseSucessfullDelegate = purchaseSucessfullDelegate;
                this.PurchaseFailedDelegate = purchaseFailedDelegate;
                controller.InitiatePurchase(product);

            #else
                Debug.LogError("Purchasing quarters through IAP is not supported on this platform
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




        public IEnumerator VerifyPurchase(PurchaseEventArgs e = null) {

            Debug.Log("Verify purchase");

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Content-Type", "application/json;charset=UTF-8");
            headers.Add("x-api-key", QuartersInit.Instance.SERVER_API_TOKEN);


            string url = Quarters.API_URL + "/apps/" + QuartersInit.Instance.APP_ID + "/verifyReceipt/unity";


            Dictionary<string, string> receiptData = JsonConvert.DeserializeObject<Dictionary<string, string>>(e.purchasedProduct.receipt);


            Hashtable receipt = new Hashtable();
            receipt.Add("Store", receiptData["Store"]);
            receipt.Add("TransactionID", receiptData["TransactionID"]);
            receipt.Add("Payload", receiptData["Payload"]);


  
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("receipt", receipt);
            data.Add("user", Quarters.Instance.CurrentUser.id);



            string fileName = Application.persistentDataPath + "/" + Random.Range(0, 100000).ToString() + ".json";
            Debug.Log(fileName);

            if (File.Exists(fileName)) {
                Debug.Log(fileName + " already exists.");

            }
            var sr = File.CreateText(fileName);


            sr.WriteLine (JsonConvert.SerializeObject(data));
            sr.Close();



            string dataJson = JsonConvert.SerializeObject(data);
            Debug.Log(dataJson);
            byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(dataJson);


            WWW www = new WWW(url, dataBytes, headers);
            Debug.Log(www.url);

            while (!www.isDone) yield return new WaitForEndOfFrame();

            if (!string.IsNullOrEmpty(www.error)) {
                Debug.LogError(www.error);

                if (PurchaseFailedDelegate != null) PurchaseFailedDelegate(www.error);
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
