using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ImaginationOverflow.UniversalDeepLinking.Storage;
using UnityEditor;
using UnityEngine;

namespace ImaginationOverflow.UniversalDeepLinking.Editor.Ui
{
    public class PluginWindow : EditorWindow
    {
        private string _displayName;

        private string _steamId;
        private bool _steamSettings;


        private AppLinkingConfiguration _config;
        private Vector2 _scrollPosition;

        private const string DeepLinkLabelFormat = "Deep Link Configuration ({0})";
        private const string DomainAssociationLabelFormat = "Domain Association Configuration ({0})";
        private readonly Dictionary<SupportedPlatforms, bool> _platformsWithCustomData = new Dictionary<SupportedPlatforms, bool>();


        // [MenuItem("Window/ImaginationOverflow/Universal DeepLinking/Configuration", priority = 0)]

        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(PluginWindow), true, "Universal DeepLinking").Show();
        }

        private void OnEnable()
        {
            EnsureConfiguration();
        }

        void OnGUI()
        {

            EnsureConfiguration();
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, false, false, GUILayout.ExpandHeight(false));
            GUILayout.Label("Settings", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;
            _displayName = EditorGUILayout.TextField("Display Name", _displayName);
            EditorGUI.indentLevel--;

            _steamSettings = EditorGUILayout.BeginToggleGroup("Enable Steam Integration", _steamSettings);
            if (_steamSettings == false)
                _steamId = string.Empty;

            EditorGUI.indentLevel++;
            _steamId = EditorGUILayout.TextField("Steam App Id", _steamId);
            EditorGUI.indentLevel--;

            EditorGUILayout.EndToggleGroup();

            DrawLinkInformation(_config.DeepLinkingProtocols, string.Format(DeepLinkLabelFormat, "Global"));
            DrawLinkInformation(_config.DomainProtocols, string.Format(DomainAssociationLabelFormat, "Global"));

            DrawCustomPlatformLinkInformation();

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Save"))
            {
                SaveData();
            }

        }

        private void DrawCustomPlatformLinkInformation()
        {

            GUILayout.Label("Platform Override Configurations", EditorStyles.boldLabel);
            foreach (SupportedPlatforms value in Enum.GetValues(typeof(SupportedPlatforms)))
            {
                _platformsWithCustomData[value] = EditorGUILayout.BeginToggleGroup(value.ToString(), _platformsWithCustomData[value]);

                if (_platformsWithCustomData[value])
                {
                    _config.ActivatePlatformOverride(value);
                    var deepLinking = _config.GetPlatformDeepLinkingProtocols(value);
                    var domainAssociation = _config.GetPlatformDomainProtocols(value);
                    if (deepLinking == null)
                        deepLinking = _config.GetCustomDeepLinkingProtocols(value);

                    if (domainAssociation == null)
                        domainAssociation = _config.GetCustomDomainAssociation(value);


                    DrawLinkInformation(deepLinking, string.Format(DeepLinkLabelFormat, value.ToString()), value != SupportedPlatforms.Linux && value != SupportedPlatforms.Windows);
                    if ((value == SupportedPlatforms.Linux || value == SupportedPlatforms.Windows || value == SupportedPlatforms.OSX) == false)
                        DrawLinkInformation(domainAssociation, string.Format(DomainAssociationLabelFormat, value.ToString()));


                }
                else
                {
                    _config.DeactivatePlatformOverride(value);
                }
                EditorGUILayout.EndToggleGroup();
            }
        }

        private void DrawLinkInformation(List<LinkInformation> lis, string label, bool supportsMultiple = true)
        {
            const int height = 20;
            const int width = 120;
            const int buttonWidth = 50;
            GUILayout.Label(label, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            int idxToDelete = -1;

            GUILayout.BeginHorizontal(GUIStyle.none, GUILayout.Height(height));
            EditorGUILayout.LabelField("Scheme", GUILayout.Width(width));
            EditorGUILayout.LabelField("Host", GUILayout.Width(width));
            EditorGUILayout.LabelField("Path", GUILayout.Width(width));
            EditorGUILayout.LabelField("", GUILayout.Width(buttonWidth));
            EditorGUILayout.LabelField("Preview");
            GUILayout.EndHorizontal();

            for (int i = 0; i < lis.Count; i++)
            {
                GUILayout.BeginHorizontal(GUIStyle.none, GUILayout.Height(height));
                lis[i].Scheme = EditorGUILayout.TextField("", lis[i].Scheme, GUILayout.Width(width)).ToLower();
                lis[i].Host = EditorGUILayout.TextField("", lis[i].Host, GUILayout.Width(width));
                lis[i].Path = EditorGUILayout.TextField("", lis[i].Path, GUILayout.Width(width));

                if (GUILayout.Button("Delete", GUILayout.Width(buttonWidth)))
                {
                    idxToDelete = i;
                }
                GUILayout.Label(GetLinkPreview(lis[i]));
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
            if (supportsMultiple || (lis.Count == 0 && supportsMultiple == false))
                if (GUILayout.Button("Add"))
                {
                    lis.Add(new LinkInformation { });
                }

            if (idxToDelete != -1)
                lis.RemoveAt(idxToDelete);

            EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();

            EditorGUI.indentLevel--;

        }

        private string GetLinkPreview(LinkInformation linkInformation)
        {
            if (string.IsNullOrEmpty(linkInformation.Scheme))
                return "";

            if (string.IsNullOrEmpty(linkInformation.Host))
                return linkInformation.Scheme + "://*";

            if (string.IsNullOrEmpty(linkInformation.Path))
            {
                return string.Format("{0}://{1}/*", linkInformation.Scheme, linkInformation.Host);
            }

            return string.Format("{0}://{1}/{2}", linkInformation.Scheme, linkInformation.Host, linkInformation.Path);
        }

        private void SaveData()
        {
            _config.DisplayName = _displayName;
            _config.SteamId = _steamId;
            ConfigurationStorage.Save(_config);
            _config = null;
        }

        private void EnsureConfiguration()
        {
            if (_config != null)
            {
                if (_platformsWithCustomData.Count == 0)
                    foreach (SupportedPlatforms value in Enum.GetValues(typeof(SupportedPlatforms)))
                    {
                        _platformsWithCustomData.Add(value, _config.GetPlatformDeepLinkingProtocols(value) != null || _config.GetPlatformDomainProtocols(value) != null);
                    }
                return;
            }

            _config = ConfigurationStorage.Load();
            SetupVariables();
        }

        private void SetupVariables()
        {
            _displayName = string.IsNullOrEmpty(_config.DisplayName) ? Application.productName : _config.DisplayName;

            _steamId = _config.SteamId;

            _steamSettings = !string.IsNullOrEmpty(_steamId);

            _platformsWithCustomData.Clear();

            foreach (SupportedPlatforms value in Enum.GetValues(typeof(SupportedPlatforms)))
            {
                _platformsWithCustomData.Add(value, _config.GetPlatformDeepLinkingProtocols(value) != null || _config.GetPlatformDomainProtocols(value) != null);
            }
        }



        private string GetString(string curr, string def)
        {
            return string.IsNullOrEmpty(curr) ? def : curr;
        }

    }
}
