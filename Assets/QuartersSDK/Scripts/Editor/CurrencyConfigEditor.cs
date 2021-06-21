using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace QuartersSDK {
    [CustomEditor(typeof(CurrencyConfig))]
    public class CurrencyConfigEditor : Editor {

        private string quantityString;

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            CurrencyConfig config = (CurrencyConfig) target;

            GUIStyle errorStyle = new GUIStyle(EditorStyles.textField);
            errorStyle.normal.textColor = Color.red;

            EditorGUILayout.Space();
            foreach (char c in config.Code.ToCharArray()) {
                if (Char.IsUpper(c)) {
                    EditorGUILayout.LabelField("Invalid currency code. Currency code must be lowercase", errorStyle);
                }
            }

            EditorGUILayout.Space();

            foreach (string productId in config.IAPProductIds) {
                foreach (char c in productId.ToCharArray()) {
                    if (Char.IsUpper(c)) {
                        EditorGUILayout.LabelField("Invalid product ID. Product Ids must be lowercase", errorStyle);
                    }
                }
            }


            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Add Shop Products");
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            quantityString = EditorGUILayout.TextField("Quantity", quantityString);


            if (IsCorrectQuantity(quantityString)) {

                if (GUILayout.Button("Add Product")) {

                    int quantity = int.Parse(quantityString);
                    if (!config.IAPProductQuantities.Contains(quantity)) {
                        config.IAPProductQuantities.Add(quantity);

                        EditorUtility.SetDirty(target);
                        AssetDatabase.SaveAssets();
                    }
                }
            }
            else {
                //incorrect format
                EditorGUILayout.LabelField("Enter correct quantity");
            }


            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Shop product Ids");
            DrawProductIds(config);


        }



        private void DrawProductIds(CurrencyConfig config) {
            
            foreach (string productId in config.IAPProductIds) {

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Copy")) {
                    EditorGUIUtility.systemCopyBuffer = productId;
                }

                EditorGUILayout.LabelField((productId));

                if (GUILayout.Button("Remove")) {
                    int quantity = config.GetQuantity(productId);

                    if (quantity != -1) {
                        config.IAPProductQuantities.Remove(quantity);
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

        }

        private bool IsCorrectQuantity(string quantityString) {

            int quantity = -1;
            if (int.TryParse(quantityString, out quantity)) {
                if (quantity > 0) {
                    return true;
                }
                else return false;
            }
            else {
                return false;
            }

        }

    }
}
