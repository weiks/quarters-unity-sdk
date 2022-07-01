using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace ImaginationOverflow.UniversalDeepLinking.Editor
{
    public class UwpBuilder : IPosBuilder
    {

 

        private const string DomainAssociationNamespacePrefix = "uap3";
        private const string CategoryAttr = "Category";
        private const string WinProtocol = "windows.protocol";
        private const string UriProtocol = "windows.appUriHandler";

        public void PostBuildProcess(AppLinkingConfiguration configuration, string pathToBuiltProject)
        {
            UpdateApp(pathToBuiltProject);
            UpdateManifest(configuration, pathToBuiltProject);
        }

        private void UpdateApp(string pathToBuiltProject)
        {
            var projLocation = Path.Combine(pathToBuiltProject, Application.productName);

            if (PlayerSettings.GetScriptingBackend(BuildTargetGroup.WSA) == ScriptingImplementation.IL2CPP)
            {
                if (EditorUserBuildSettings.wsaUWPBuildType == WSAUWPBuildType.D3D)
                    HandleCppD3d(projLocation, "App.cpp");
                else
                    HandleCppXaml(projLocation, "App.xaml.cpp");

                return;
            }

            if (EditorUserBuildSettings.wsaUWPBuildType == WSAUWPBuildType.D3D)
                HandleCodeInsertion(projLocation, "App.cs");
            else
                HandleCodeInsertion(projLocation, "App.xaml.cs");
        }

        public void HandleCppXaml(string projLocation, string app)
        {
            var appFile = Path.Combine(projLocation, app);
            var appcs = File.ReadAllText(appFile);

            if (appcs.Contains("static int c = 0;"))
                return;


            var idx = appcs.IndexOf("void App::OnActivated(IActivatedEventArgs^ args)", StringComparison.InvariantCultureIgnoreCase);

            if (idx == -1)
                return;

            int ob = 0; //open brackets
            int cb = 0; //close brackets
            int functionEndIdx = -1;
            for (int i = idx; i < appcs.Length; i++)
            {
                if (ob == cb && ob != 0)
                {
                    functionEndIdx = i;
                    break;
                }

                if (appcs[i] == '{')
                    ob++;
                else if (appcs[i] == '}')
                    cb++;
            }

            appcs = appcs.Replace(appcs.Substring(idx, functionEndIdx - idx), ToAddCppXaml);
            File.WriteAllText(appFile, appcs);

        }

        private void HandleCppD3d(string projLocation, string app)
        {
            var appFile = Path.Combine(projLocation, app);
            var appcs = File.ReadAllText(appFile);

            if (appcs.Contains("ActivationKind::Protocol"))
                return;

            appcs = appcs.Replace("m_CoreWindow->Activate();", ToAddCppD3d);

            File.WriteAllText(appFile, appcs);

        }

        private static void HandleCodeInsertion(string projLocation, string fileName)
        {
            var appFile = Path.Combine(projLocation, fileName);
            var appcs = File.ReadAllText(appFile);
            const string activatedArgMethodToInsert = "IActivatedEventArgs args)";

            /*
               if (args is ProtocolActivatedEventArgs) 
                    ImaginationOverflow.UniversalDeepLinking.Providers.UwpLinkProvider.ProcessLink((args as ProtocolActivatedEventArgs).Uri.AbsoluteUri);
             */

            if (appcs.Contains("ImaginationOverflow.UniversalDeepLinking.Providers.UwpLinkProvider.ProcessLink"))
                return;

            var idx = appcs.IndexOf(activatedArgMethodToInsert);

            if (idx == -1)
            {
                throw new Exception("Could not find Activated event handler");
            }

            var methodStart = appcs.IndexOf("{", idx);

            appcs = appcs.Insert(methodStart + 1, toAddStr);

            File.WriteAllText(appFile, appcs);


        }
        private static void UpdateManifest(AppLinkingConfiguration configuration, string pathToBuiltProject)
        {
            var projPath = Path.Combine(pathToBuiltProject, Application.productName);

            var manifest = Path.Combine(projPath, "Package.appxmanifest");

            var elem = XElement.Load(manifest);

            //new XAttribute("xmlns:uap3", "http://schemas.microsoft.com/appx/manifest/uap/windows10/3")

            var domain = configuration.GetPlatformDomainProtocols(SupportedPlatforms.UWP, true);
            if (domain.Count != 0)
            {
                HandleNamespaceRegistration(elem);
            }

            var apps = GetElement(elem, "Applications");
            var app = GetElement(apps, "Application");

            var extensions = GetElement(app, "Extensions");

            if (extensions == null)
            {
                app.Add(extensions = CreateChild(app, "Extensions"));
            }

            RemoveOldRegistrations(extensions);

            AddDeepLinkConfiguration(configuration, elem, extensions);
            AddDomainAssociationConfiguration(configuration, elem, extensions);

            elem.Save(manifest);
        }

        private static void AddDomainAssociationConfiguration(AppLinkingConfiguration configuration, XElement elem, XElement extensions)
        {
            /*
        <uap3:Extension Category="windows.appUriHandler">
          <uap3:AppUriHandler>
            <uap3:Host Name="sudokuzenkai.imaginationoverflow.com" />
            <uap3:Host Name="www.sudokuzenkai.imaginationoverflow.com" />
          </uap3:AppUriHandler>
        </uap3:Extension>
             
             */
            var config = configuration.GetPlatformDomainProtocols(SupportedPlatforms.UWP, true);
            if (config.Count == 0)
                return;

            var uap3Prefix = elem.GetNamespaceOfPrefix(DomainAssociationNamespacePrefix);

            var extension = new XElement(uap3Prefix + "Extension", new XAttribute(CategoryAttr, UriProtocol));
            extensions.Add(extension);

            var uriHandler = new XElement(uap3Prefix + "AppUriHandler");
            extension.Add(uriHandler);

            foreach (var domain in config.Select(d => d.Host).Distinct())
            {
                /*
                  <uap3:Host Name="sudokuzenkai.imaginationoverflow.com" 
                   */

                var host = new XElement(uap3Prefix + "Host", new XAttribute("Name", domain));

                uriHandler.Add(host);
            }
        }

        private static void AddDeepLinkConfiguration(AppLinkingConfiguration configuration, XElement elem, XElement extensions)
        {
            var config = configuration.GetPlatformDeepLinkingProtocols(SupportedPlatforms.UWP, true);
            if (config.Count == 0)
                return;

            var uapPrefix = elem.GetNamespaceOfPrefix("uap");
            foreach (var deepLink in config)
            {
                var extension = new XElement(uapPrefix + "Extension", new XAttribute(CategoryAttr, WinProtocol));
                extensions.Add(extension);

                var protocol = new XElement(uapPrefix + "Protocol", new XAttribute("Name", deepLink.Scheme));
                extension.Add(protocol);

                var logo = new XElement(uapPrefix + "Logo") { Value = @"Assets\StoreLogo.png" };
                var displayName = new XElement(uapPrefix + "DisplayName")
                {
                    Value = string.IsNullOrEmpty(configuration.DisplayName)
                        ? Application.productName
                        : configuration.DisplayName
                };

                protocol.Add(logo);
                protocol.Add(displayName);
            }
        }

        private static void RemoveOldRegistrations(XElement extensions)
        {
            foreach (var protocol in GetElements(extensions, "Extension"))
            {
                var cat = protocol.Attribute(CategoryAttr);
                if (cat != null && (cat.Value == WinProtocol || cat.Value == UriProtocol))
                    protocol.Remove();
            }
        }

        private static void HandleNamespaceRegistration(XElement elem)
        {
            if (string.IsNullOrEmpty(elem.GetNamespaceOfPrefix(DomainAssociationNamespacePrefix).NamespaceName) == false)
                return;

            var attrName = XNamespace.Xmlns + DomainAssociationNamespacePrefix;
            XNamespace uap3 = "http://schemas.microsoft.com/appx/manifest/uap/windows10/3";
            elem.Add(new XAttribute(attrName, uap3));

            var attr = elem.Attributes().FirstOrDefault(f => f.Name == "IgnorableNamespaces");

            if (attr == null)
                return;

            attr.Value = attr.Value.Replace(DomainAssociationNamespacePrefix, string.Empty) + " " +
                         DomainAssociationNamespacePrefix;

        }

        public static XElement CreateChild(XElement parent, string childName)
        {
            return new XElement(parent.Name.Namespace + childName);
        }
        public static IEnumerable<XElement> GetElements(XElement elem, string name)
        {
            return elem.Elements().Where(e => e.Name.LocalName == name);
        }

        public static XElement GetElement(XElement elem, string name)
        {
            return GetElements(elem, name).FirstOrDefault();
        }


        private const string toAddStr = @"
            //
            //  ImaginationOverflow Universal Deeplinking Plugin Auto-Gen
            //
            //
            if (args is ProtocolActivatedEventArgs) 
				ImaginationOverflow.UniversalDeepLinking.Providers.UwpLinkProvider.ProcessLink((args as ProtocolActivatedEventArgs).Uri.AbsoluteUri);
";

        private const string ToAddCppD3d = @"
    //
    //  ImaginationOverflow Universal Deeplinking Plugin Auto-Gen
    //
    //
	if (args->Kind == ActivationKind::Protocol)
	{
        static int c = 0;
		String^ appArgs = """";
		auto eventArgs = safe_cast<ProtocolActivatedEventArgs^>(args);
		appArgs += String::Concat(String::Concat(""Uri="", (c=((c+1) % 2)).ToString()), eventArgs->Uri->AbsoluteUri);
        m_AppCallbacks->SetAppArguments(appArgs);
    }

    m_CoreWindow->Activate();
";
        private const string ToAddCppXaml = @"
void App::OnActivated(IActivatedEventArgs^ args)
{
    //
    //  ImaginationOverflow Universal Deeplinking Plugin Auto-Gen
    //
    //
	String^ appArgs = """";    

	if (args->Kind == ActivationKind::Protocol)
	{
        static int c = 0;
		auto eventArgs = safe_cast<ProtocolActivatedEventArgs^>(args);
        m_SplashScreen = eventArgs->SplashScreen;
		appArgs += String::Concat(String::Concat(""Uri="", (c=((c+1) % 2)).ToString()), eventArgs->Uri->AbsoluteUri);
    }

	InitializeUnity(appArgs);
}
";
    }
}
