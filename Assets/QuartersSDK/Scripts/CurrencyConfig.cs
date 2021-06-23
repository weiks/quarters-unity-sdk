using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;


namespace QuartersSDK {
    [CreateAssetMenu(fileName = "DefaultCurrency", menuName = "Quarters/Currency", order = 1)]
    public class CurrencyConfig : ScriptableObject {

        public string APIBaseUrl;
        public string Code;
        public string DisplayNameSingular;
        public string DisplayNamePlural;

        [Header("Branding")] public Sprite CurrencyIcon;
        public Sprite CurrencyLogo;
        public Color BackgroundsColor;
        public Color BrandColor;


        [HideInInspector] [SerializeField] public List<int> IAPProductQuantities;


        public List<string> IAPProductIds {
            get {
                List<string> result = new List<string>();

                List<int> sortedQuantities = IAPProductQuantities;
                sortedQuantities.Sort();

                foreach (int productQuantity in sortedQuantities) {
                    result.Add(GetProductId(productQuantity));
                }

                return result;
            }
        }

        public string GetProductId(int quantity) {
            return $"{Application.identifier}.{Code}.{quantity.ToString()}";
        }

        public int GetQuantity(string productId) {

            string[] split = productId.Split('.');

            if (split[split.Length - 2] == this.Code) {

                string quantityString = split[split.Length - 1];

                int quantity = -1;
                if (int.TryParse(quantityString, out quantity)) {

                    return quantity;
                }
                else {
                    return -1;
                }
            }
            else {
                return -1;
            }


        }
    }
}


