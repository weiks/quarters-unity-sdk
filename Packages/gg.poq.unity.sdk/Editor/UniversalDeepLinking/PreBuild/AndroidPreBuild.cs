using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using UnityEditor;
using UnityEngine;
using UnityEngine.WSA;
using System.Linq;
using Application = UnityEngine.Application;

namespace ImaginationOverflow.UniversalDeepLinking.Editor.PreBuild
{
    /// <summary>
    /// Pre Process Build script that adds to Android Manifest the Deep Linking and Domain Associations 
    /// </summary>
    public class AndroidPreBuild : IPreBuild
    {
        private const string ACTIVITY_NAME = "com.unity3d.player.UnityPlayerActivity";

        public void OnPreprocessBuild(AppLinkingConfiguration configurations, string path)
        {

            if (configurations.GetPlatformDeepLinkingProtocols(SupportedPlatforms.Android, true).Count == 0 &&
               configurations.GetPlatformDomainProtocols(SupportedPlatforms.Android, true).Count == 0)
                return;

            string androidPluginsPath = Path.Combine(Application.dataPath, "Plugins/Android");
            string adjustManifestPath = Path.Combine(Application.dataPath, EditorHelpers.PluginPath + "/libs/Android/UniversalDeepLinkManifest.xml");
            string appManifestPath = Path.Combine(Application.dataPath, "Plugins/Android/AndroidManifest.xml");

            if (!File.Exists(appManifestPath))
            {
                if (!Directory.Exists(androidPluginsPath))
                {
                    Directory.CreateDirectory(androidPluginsPath);
                }

                File.Copy(adjustManifestPath, appManifestPath);

            }


            XmlDocument manifestFile = new XmlDocument();

            manifestFile.Load(appManifestPath);

            AddDeepLinks(manifestFile, configurations);

            manifestFile.Save(appManifestPath);

            CleanManifestFile(appManifestPath);
        }

        private void AddDeepLinks(XmlDocument manifest, AppLinkingConfiguration configurations)
        {
            var activityNode = SearchUnityActivity(manifest);

            var nodeDeepLink = SearchIntentsBrowsable(activityNode);


            foreach (var xmlNode in nodeDeepLink)
            {
                if (RemoveOldDeepLinks(xmlNode))
                    activityNode.RemoveChild(xmlNode);
            }

            AddConfiguration(manifest, activityNode, configurations.GetPlatformDomainProtocols(SupportedPlatforms.Android, true), true);
            AddConfiguration(manifest, activityNode, configurations.GetPlatformDeepLinkingProtocols(SupportedPlatforms.Android, true));

        }

        public void AddConfiguration(XmlDocument manifest, XmlNode parent, List<LinkInformation> configurations, bool isDomain = false)
        {
            foreach (var linkConfig in configurations)
            {
                if (string.IsNullOrEmpty(linkConfig.Scheme))
                    continue;

                var intent = CreateIntentFilter(manifest, isDomain);

                XmlElement protocol = manifest.CreateElement("data");

                protocol.SetAttribute("android__scheme", linkConfig.Scheme);

                if (!string.IsNullOrEmpty(linkConfig.Host))
                {
                    protocol.SetAttribute("android__host", linkConfig.Host);

                    if (!string.IsNullOrEmpty(linkConfig.Path))
                    {
                        protocol.SetAttribute("android__pathPattern", "/" + linkConfig.Path);
                    }
                    else
                        protocol.SetAttribute("android__pathPattern", ".*");
                }




                intent.AppendChild(protocol);
                parent.AppendChild(intent);
            }
        }

        private static XmlElement CreateIntentFilter(XmlDocument manifest, bool isDomain)
        {
            var element = manifest.CreateElement("intent-filter");

            if (isDomain)
                element.SetAttribute("android__autoVerify", "true");

            var action = manifest.CreateElement("action");
            var catdf = manifest.CreateElement("category");
            var catbro = manifest.CreateElement("category");

            action.SetAttribute("android__name", "android.intent.action.VIEW");
            catdf.SetAttribute("android__name", "android.intent.category.DEFAULT");
            catbro.SetAttribute("android__name", "android.intent.category.BROWSABLE");

            element.AppendChild(action);
            element.AppendChild(catdf);
            element.AppendChild(catbro);

            return element;
        }


        private bool RemoveOldDeepLinks(XmlNode browsableNode)
        {
            List<XmlNode> oldDeepLinkNodes = new List<XmlNode>();

            int totalDataElements = 0;
            int totalDeepLinkElements = 0;
            foreach (XmlNode node in browsableNode.ChildNodes)
            {
                if (node.Name == "data" && node.Attributes != null && node.Attributes.Count > 0)
                {
                    totalDataElements++;

                    int elements = 0;
                    foreach (XmlAttribute nodeAttribute in node.Attributes)
                    {
                        if (nodeAttribute.Name == "android:host" || nodeAttribute.Name == "android:scheme" || nodeAttribute.Name == "android:pathPattern")
                        {
                            elements++;
                        }
                    }

                    if (elements == node.Attributes.Count)
                    {
                        totalDeepLinkElements++;
                        oldDeepLinkNodes.Add(node);
                    }
                }
            }

            foreach (var oldDeepLinkNode in oldDeepLinkNodes)
            {
                browsableNode.RemoveChild(oldDeepLinkNode);
            }

            return totalDataElements == totalDeepLinkElements;
        }

        private List<XmlNode> SearchIntentsBrowsable(XmlElement activityNode)
        {
            var browsableNode = new List<XmlNode>();
            foreach (XmlNode node in activityNode.ChildNodes)
            {
                if (node.Name == "intent-filter")
                {
                    bool hasActionView = false;
                    bool hasCategoryDefault = false;
                    bool hasCategoryBrowsable = false;

                    foreach (XmlNode nodeIntent in node.ChildNodes)
                    {
                        if (nodeIntent.Name == "action"
                            && ((XmlElement)nodeIntent).GetAttribute("android:name") == "android.intent.action.VIEW")
                        {
                            hasActionView = true;
                        }
                        else if (nodeIntent.Name == "category"
                                 && ((XmlElement)nodeIntent).GetAttribute("android:name") == "android.intent.category.DEFAULT")
                        {
                            hasCategoryDefault = true;
                        }
                        else if (nodeIntent.Name == "category"
                                 && ((XmlElement)nodeIntent).GetAttribute("android:name") == "android.intent.category.BROWSABLE")
                        {
                            hasCategoryBrowsable = true;
                        }

                        if (hasActionView && hasCategoryDefault && hasCategoryBrowsable)
                        {
                            browsableNode.Add(node);
                            break;
                        }
                    }
                }
            }

            return browsableNode;
        }

        private XmlElement SearchUnityActivity(XmlDocument doc)
        {
            foreach (XmlNode node0 in doc.DocumentElement.ChildNodes)
            {
                if (node0.Name == "application")
                {
                    foreach (XmlNode node1 in node0.ChildNodes)
                    {
                        if (node1.Name == "activity"
                            && ((XmlElement)node1).GetAttribute("android:name") == ACTIVITY_NAME)
                        {
                            return (XmlElement)node1;
                        }
                        if (node1.Name == "activity")
                        {
                            foreach (XmlNode node2 in node1.ChildNodes)
                            {
                                if (node2.Name == "intent-filter")
                                {
                                    bool hasActionMain = false;
                                    bool hasCategoryLauncher = false;
                                    foreach (XmlNode node3 in node2.ChildNodes)
                                    {
                                        if (node3.Name == "action"
                                            && ((XmlElement)node3).GetAttribute("android:name") == "android.intent.action.MAIN")
                                        {
                                            hasActionMain = true;
                                        }
                                        else if (node3.Name == "category"
                                                 && ((XmlElement)node3).GetAttribute("android:name") == "android.intent.category.LAUNCHER")
                                        {
                                            hasCategoryLauncher = true;
                                        }
                                    }
                                    if (hasActionMain && hasCategoryLauncher)
                                    {
                                        return (XmlElement)node1;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        private static void CleanManifestFile(String manifestPath)
        {

            TextReader manifestReader = new StreamReader(manifestPath);
            string manifestContent = manifestReader.ReadToEnd();
            manifestReader.Close();

            Regex regex = new Regex("android__");
            manifestContent = regex.Replace(manifestContent, "android:");

            TextWriter manifestWriter = new StreamWriter(manifestPath);
            manifestWriter.Write(manifestContent);
            manifestWriter.Close();
        }
    }


}
