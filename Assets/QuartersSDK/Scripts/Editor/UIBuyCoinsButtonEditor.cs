using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using QuartersSDK;

namespace QuartersSDK.UI {
    [CustomEditor(typeof(UIBuyCoinsButton))]
    public class UIBuyCoinsButtonEditor : Editor {

        private int index;
        
        public override void OnInspectorGUI() {
            
            UIBuyCoinsButton button = (UIBuyCoinsButton) target;
            base.OnInspectorGUI();
            
           
            if (button.CurrencyConfig != null) {
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Coins quantity:");
                
                List<int> quantities = button.CurrencyConfig.IAPProductQuantities;

                List<string> quantitiesString = new List<string>();
                foreach (int quantity in quantities) {
                    quantitiesString.Add(quantity.ToString());
                }
                
                index = button.CurrencyConfig.IAPProductQuantities.IndexOf(button.Quantity);

                int oldIndex = this.index;
                index = EditorGUILayout.Popup(index, quantitiesString.ToArray());
                button.Quantity = button.CurrencyConfig.IAPProductQuantities[index];

                if (oldIndex != index) {
                    button.OnQuantityUpdated();
                    EditorUtility.SetDirty(button);
                }
                

            }  
        }
    }
}
