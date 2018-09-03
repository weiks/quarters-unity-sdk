using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UDPEditor;
using UnityEngine.UDP;

public class GameManager : MonoBehaviour
{
    public string Product1;
    public string Product2;

    private static bool m_consumeOnPurchase;
    private static bool _consumeOnQuery;

    private Dropdown _dropdown;
    private List<Dropdown.OptionData> options;
    private static Text _textField;
    private static bool _initialized;

    PurchaseListener purchaseListener;
    InitListener initListener;
    AppInfo appInfo;

    void Start()
    {
        #region Basic Information Initialization

        purchaseListener = new PurchaseListener();
        initListener = new InitListener();
        appInfo = new AppInfo();

        /*
         * GameSettings.asset only supports Unity whose version is higher than 5.6.1 (inlcuded).
         * If developers are using older Unity, they should get these information from the developer portal and fill the AppInfo manually.
         */
        AppStoreSettings appStoreSettings = Resources.Load<AppStoreSettings>("GameSettings");
        appInfo.AppSlug = appStoreSettings.AppSlug;
        appInfo.ClientId = appStoreSettings.UnityClientID;
        appInfo.ClientKey = appStoreSettings.UnityClientKey;
        appInfo.RSAPublicKey = appStoreSettings.UnityClientRSAPublicKey;

        Debug.Log("App Name: " + appStoreSettings.AppName);

        #endregion

        #region Text Field Initialization

        GameObject gameObject = GameObject.Find("Information");
        _textField = gameObject.GetComponent<Text>();
        _textField.text = "Please Click Init to Start";

        #endregion

        #region DropDown Initialization

        gameObject = GameObject.Find("Dropdown");

        _dropdown = gameObject.GetComponent<Dropdown>();
        _dropdown.ClearOptions();
        _dropdown.options.Add(new Dropdown.OptionData(Product1));
        _dropdown.options.Add(new Dropdown.OptionData(Product2));
        _dropdown.RefreshShownValue();

        #endregion

        InitUI();
    }

    private static void Show(string message, bool append = false)
    {
        _textField.text = append ? string.Format("{0}\n{1}", _textField.text, message) : message;
    }

    void InitUI()
    {
        #region Button Initialization

        GetButton("InitButton").onClick.AddListener(() =>
        {
            _initialized = false;
            Debug.Log("Init button is clicked.");
            Show("Initializing");
            StoreService.Initialize(initListener);
        });

        GetButton("BuyButton").onClick.AddListener(() =>
        {
            if (!_initialized)
            {
                Show("Please Initialize first");
                return;
            }

            string prodcutId = _dropdown.options[_dropdown.value].text;
            Debug.Log("Buy button is clicked.");
            Show("Buying Product: " + prodcutId);
            m_consumeOnPurchase = false;
            Debug.Log(_dropdown.options[_dropdown.value].text + " will be bought");
            StoreService.Purchase(prodcutId, null, "{\"AnyKeyYouWant:\" : \"AnyValueYouWant\"}", purchaseListener);
        });

        GetButton("BuyConsumeButton").onClick.AddListener(() =>
        {
            if (!_initialized)
            {
                Show("Please Initialize first");
                return;
            }

            string prodcutId = _dropdown.options[_dropdown.value].text;
            Show("Buying Product: " + prodcutId);
            Debug.Log("Buy&Consume button is clicked.");
            m_consumeOnPurchase = true;
            StoreService.Purchase(prodcutId, null, "buy and consume developer payload", purchaseListener);
        });

        List<string> productIds = new List<string> {Product1, Product2};

        GetButton("QueryButton").onClick.AddListener(() =>
        {
            if (!_initialized)
            {
                Show("Please Initialize first");
                return;
            }

            _consumeOnQuery = false;
            Debug.Log("Query button is clicked.");
            Show("Querying Inventory");
            StoreService.QueryInventory(productIds, purchaseListener);
        });

        GetButton("QueryConsumeButton").onClick.AddListener(() =>
        {
            if (!_initialized)
            {
                Show("Please Initialize first");
                return;
            }

            _consumeOnQuery = true;
            Show("Querying Inventory");
            Debug.Log("QueryConsume button is clicked.");
            StoreService.QueryInventory(productIds, purchaseListener);
        });

        #endregion
    }

    private Button GetButton(string buttonName)
    {
        GameObject obj = GameObject.Find(buttonName);
        if (obj != null)
        {
            return obj.GetComponent<Button>();
        }
        return null;
    }

    /// <summary>
    /// Init Listener
    /// </summary>
    public class InitListener : IInitListener
    {
        public void OnInitialized(UserInfo userInfo)
        {
            Debug.Log("[Game]On Initialized suceeded");
            Show("Initialize succeeded");
            _initialized = true;
        }

        public void OnInitializeFailed(string message)
        {
            Debug.Log("[Game]OnInitializeFailed: " + message);
            Show("Initialize Failed: " + message);
        }
    }

    /// <summary>
    /// Purchase Listener.
    /// </summary>
    public class PurchaseListener : IPurchaseListener
    {
        public void OnPurchase(PurchaseInfo purchaseInfo)
        {
            string message = string.Format(
                "[Game] Purchase Succeeded, productId: {0}, cpOrderId: {1}, developerPayload: {2}, storeJson: {3}",
                purchaseInfo.ProductId, purchaseInfo.GameOrderId, purchaseInfo.DeveloperPayload,
                purchaseInfo.StorePurchaseJsonString);

            Debug.Log(message);
            Show(message);

            if (m_consumeOnPurchase)
            {
                Debug.Log("Consuming");
                StoreService.ConsumePurchase(purchaseInfo, this);
            }
        }

        public void OnPurchaseFailed(string message, PurchaseInfo purchaseInfo)
        {
            Debug.Log("Purchase Failed: " + message);
            Show("Purchase Failed: " + message);
        }

        public void OnPurchaseRepeated(string productCode)
        {
            throw new System.NotImplementedException();
        }

        public void OnPurchaseConsume(PurchaseInfo purchaseInfo)
        {
            Show("Consume success for " + purchaseInfo.ProductId, true);
            Debug.Log("Consume success: " + purchaseInfo.ProductId);
        }

        public void OnMultiPurchaseConsume(List<bool> successful, List<PurchaseInfo> purchaseInfos,
            List<string> messages)
        {
            int len = successful.Count;
            string message;
            for (int i = 0; i < len; i++)
            {
                if (successful[i])
                {
                    message = string.Format("Consuming succeeded for {0}\n", purchaseInfos[i].ProductId);
                    Show(message, true);
                    Debug.Log(message);
                }
                else
                {
                    message = string.Format("Consuming failed for {0}, reason: {1}", purchaseInfos[i].ProductId,
                        messages[i]);
                    Show(message, true);
                    Debug.Log(message);
                }
            }
        }

        public void OnPurchaseConsumeFailed(string message, PurchaseInfo purchaseInfo)
        {
            Debug.Log("Consume Failed: " + message);
            Show("Consume Failed: " + message);
        }

        public void OnQueryInventory(Inventory inventory)
        {
            Debug.Log("OnQueryInventory");
            Debug.Log("[Game] Product List: ");
            string message = "Product List: \n";


            foreach (KeyValuePair<string, ProductInfo> productInfo in inventory.GetProductDictionary())
            {
                Debug.Log("[Game] Returned product: " + productInfo.Key + " " + productInfo.Value.ProductId);
                message += string.Format("{0}:\n" +
                                         "\tTitle: {1}\n" +
                                         "\tDescription: {2}\n" +
                                         "\tConsumable: {3}\n" +
                                         "\tPrice: {4}\n" +
                                         "\tCurrency: {5}\n" +
                                         "\tPriceAmountMicros: {6}\n" + 
                                         "\tItemType: {7}\n",
                    productInfo.Key,
                    productInfo.Value.Title,
                    productInfo.Value.Description,
                    productInfo.Value.Consumable ,
                    productInfo.Value.Price,
                    productInfo.Value.Currency,
                    productInfo.Value.PriceAmountMicros,
                    productInfo.Value.ItemType
                );
            }

            message += "\nPurchase List: \n";

            foreach (KeyValuePair<string, PurchaseInfo> purchaseInfo in inventory.GetPurchaseDictionary())
            {
                Debug.Log("[Game] Returned purchase: " + purchaseInfo.Key);
                message += string.Format("{0}\n", purchaseInfo.Value.ProductId);
            }

            Show(message);

            if (_consumeOnQuery)
            {
                StoreService.ConsumePurchase(inventory.GetPurchaseList(), this);
            }
        }

        public void OnQueryInventoryFailed(string message)
        {
            Debug.Log("OnQueryInventory Failed: " + message);
            Show("QueryInventory Failed: " + message);
        }
    }
}