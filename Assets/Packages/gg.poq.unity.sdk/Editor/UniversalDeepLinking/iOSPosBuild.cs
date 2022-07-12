using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
#if UDL_DLL_BUILD
#endif
using UnityEditor;
#if (UNITY_EDITOR && (UNITY_IOS || UNITY_TVOS)) || UDL_DLL_BUILD 
using UnityEngine;
#if !UDL_DLL_BUILD && !UNITY_2018_1_OR_NEWER
using UnityEditor.iOS.Xcode;
#else
using ImaginationOverflow.UniversalDeepLinking.Editor.Xcode;
#endif
namespace ImaginationOverflow.UniversalDeepLinking.Editor
{
    /// <summary>
    /// Pos  Build script that configures the info.plist for Deep links and creates the entitlements file for Domain Association
    /// </summary>
    public class iOSBuilder : IPosBuilder
    {

        private const string PListName = "/Info.plist";

        private const string EntitlementsExtension = ".entitlements";

        private const string EntitlementFormat =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
            "<!DOCTYPE plist PUBLIC \"-//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">" +
            "<plist version=\"1.0\">" +
            "<dict></dict></plist>";

        private const string EntitlementsCodeSign = "CODE_SIGN_ENTITLEMENTS";

        private const string AssociatedDomainsKey = "com.apple.developer.associated-domains";

        private const string AppLinksKey = "applinks:";
        private BuildTarget _target;

        public iOSBuilder(BuildTarget target)
        {
            this._target = target;
        }

        public void PostBuildProcess(AppLinkingConfiguration configuration, string pathToBuiltProject)
        {
            ChangeIOSFlagsAndFrameworks(pathToBuiltProject);

            AddDeepLinks(configuration, pathToBuiltProject);

            AddDomainAssociation(pathToBuiltProject, configuration);
        }

        private void AddFrameworks(PBXProject project, string projectTarget)
        {
            //const string flags = "-all_load -ObjC";
            const string flags = "-ObjC";

            project.AddBuildProperty(projectTarget, "OTHER_LDFLAGS", flags);

            try
            {
                var frameworkTarget = project.TargetGuidByName("UnityFramework");

                if (frameworkTarget != null)
                    project.AddBuildProperty(frameworkTarget, "OTHER_LDFLAGS", flags);
            }
            catch (Exception e)
            {
                Debug.Log("UDL iOS Error " + e.ToString());
            }
        }

        private void ChangeIOSFlagsAndFrameworks(string pathToBuiltProject)
        {
            string projectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
            PBXProject project = new PBXProject();
            project.ReadFromString(File.ReadAllText(projectPath));
            string targetName = PBXProject.GetUnityTargetName();
            string projectTarget = project.TargetGuidByName(targetName);

            AddFrameworks(project, projectTarget);


            File.WriteAllText(projectPath, project.WriteToString());
        }

        private void AddDeepLinks(AppLinkingConfiguration configuration, string pathToBuiltProject)
        {
            var plistDocumentPath = pathToBuiltProject + PListName;

            var plistDocument = new PlistDocument();

            plistDocument.ReadFromString(File.ReadAllText(plistDocumentPath));

            var rootElement = plistDocument.root;

            var bundleIdentifier = Application.identifier;

            var urlSchemeArrayElement = rootElement["CFBundleURLTypes"] as PlistElementArray;


            var configuredDeepLinks = new string[0];

            if (urlSchemeArrayElement == null)
                urlSchemeArrayElement = rootElement.CreateArray("CFBundleURLTypes");
            else
            {
                try
                {
                    configuredDeepLinks = urlSchemeArrayElement.values
                        .Select(v => v.AsDict())
                        .Where(d => d.values.ContainsKey("CFBundleURLSchemes"))
                        .Select(d =>
                        {
                            PlistElement elem;
                            d.values.TryGetValue("CFBundleURLSchemes", out elem);
                            return elem != null ? elem.AsString() : string.Empty;
                        }).ToArray();
                }
                catch (Exception e)
                {
                }
            }


            var mtarget = _target == BuildTarget.iOS ? SupportedPlatforms.iOS : SupportedPlatforms.tvOS;
            foreach (var configurationDeepLinkingProtocol in configuration.GetPlatformDeepLinkingProtocols(mtarget, true))
            {

                if (configuredDeepLinks.Contains(configurationDeepLinkingProtocol.Scheme))
                    continue;

                var urlTypeDic = urlSchemeArrayElement.AddDict();

                urlTypeDic.SetString("CFBundleURLName", bundleIdentifier);

                urlTypeDic.SetString("CFBundleTypeRole", "Viewer");

                var link = configurationDeepLinkingProtocol.Scheme;

                urlTypeDic.CreateArray("CFBundleURLSchemes").AddString(link);
            }

            File.WriteAllText(plistDocumentPath, plistDocument.WriteToString());
        }


        private void AddDomainAssociation(string pathToBuiltProject, AppLinkingConfiguration configuration)
        {
            string entitlementFilePath = "";

            var mtarget = _target == BuildTarget.iOS ? SupportedPlatforms.iOS : SupportedPlatforms.tvOS;

            if (!configuration.GetPlatformDomainProtocols(mtarget, true).Select(d => d.Host).Any())
                return;

            var entitlementsFiles = new DirectoryInfo(pathToBuiltProject).GetFiles("*.entitlements", SearchOption.AllDirectories);

            if (entitlementsFiles.Length == 0)
            {
                entitlementFilePath = CreateEntitlementsFile(pathToBuiltProject);

            }
            else
            {
                //
                //  Try to find the default entitlements file if any
                //
                var file = entitlementsFiles.FirstOrDefault(d => d.Name.ToLower().Contains("unity"));

                if (file == null)
                    file = entitlementsFiles.First();


                if (entitlementsFiles.Length > 1)
                    Debug.LogWarning("Attention, it seems that you have multiple entitlements file. Only one will be added the Project : " + file.Name);

                entitlementFilePath = file.FullName;
            }


            ChangeEntitlementAssociatedDomains(entitlementFilePath, configuration);



        }

        private static string CreateEntitlementsFile(string pathToBuiltProject)
        {
            string entitlementFilePath = "";


            //var productName = Application.productName;
            var productName = "UnityUdl";

            var entitlementFileName = productName + EntitlementsExtension;

            var projPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);

            var proj = new PBXProject();

            proj.ReadFromString(File.ReadAllText(projPath));

            var targetName = PBXProject.GetUnityTargetName();

            var targetGuid = proj.TargetGuidByName(targetName);

            var dst = CombinePaths(pathToBuiltProject, targetName, entitlementFileName);

            File.WriteAllText(dst, EntitlementFormat);

            proj.AddFile(CombinePaths(targetName, entitlementFileName), entitlementFileName, PBXSourceTree.Source);

            var entitlementFile = CombinePaths(targetName, entitlementFileName);

            proj.AddBuildProperty(targetGuid, EntitlementsCodeSign, entitlementFile.Replace("\\", "/"));

            proj.WriteToFile(projPath);

            entitlementFilePath = dst;

            return entitlementFilePath;
        }

        private void ChangeEntitlementAssociatedDomains(string entitlementFilePath, AppLinkingConfiguration configuration)
        {
            PlistDocument entitlementFile = new PlistDocument();

            entitlementFile.ReadFromString(File.ReadAllText(entitlementFilePath));

            PlistElementDict rootElement = entitlementFile.root;


            var mtarget = _target == BuildTarget.iOS ? SupportedPlatforms.iOS : SupportedPlatforms.tvOS;

            var configs = configuration.GetPlatformDomainProtocols(mtarget, true).Select(d => d.Host)
                .Distinct().ToArray();

            if (configs.Length != 0)
            {

                var associatedDomainsArrayElement = rootElement[AssociatedDomainsKey] as PlistElementArray;

                var alreadySetupDomains = new string[0];
                if (associatedDomainsArrayElement == null)
                    associatedDomainsArrayElement = rootElement.CreateArray(AssociatedDomainsKey);
                else
                {
                    try
                    {
                        alreadySetupDomains = associatedDomainsArrayElement.values.Select(a => a.AsString()).ToArray();
                    }
                    catch (Exception e)
                    {
                    }
                }



                //
                //  The except removes any config when the Append is used in the generation
                //
                foreach (var domainProtocol in configs.Where(c => alreadySetupDomains.Any(d => d.Contains(c)) == false))
                {
                    associatedDomainsArrayElement.AddString(AppLinksKey + domainProtocol);
                }
            }
            else
            {
                if (rootElement.values.ContainsKey(AssociatedDomainsKey))
                    rootElement.values.Remove(AssociatedDomainsKey);
            }


            File.WriteAllText(entitlementFilePath, entitlementFile.WriteToString());
        }

        public static string CombinePaths(params string[] paths)
        {
            if (paths == null)
            {
                throw new ArgumentNullException("paths");
            }
            return paths.Aggregate(Path.Combine);
        }

    }
}

#endif
