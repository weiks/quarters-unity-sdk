using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ImaginationOverflow.UniversalDeepLinking.Storage;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace ImaginationOverflow.UniversalDeepLinking.Providers
{
#if UNITY_STANDALONE_LINUX
    public class LinuxLinkProvider : ILinkProvider
    {
        private readonly bool _steamBuild;
        private string _scheme;

        const string LocalAppDir = "/.local/share/applications/";
        const string DesktopFileFormat = @"[Desktop Entry]
Version=1.0
Type=Application
Exec={0} %u
Icon={1} 
StartupNotify=true
Terminal=false
Categories=Game;
MimeType=x-scheme-handler/{2}
Name={2}
Comment=Launch {2}
";

        public LinuxLinkProvider(bool steamBuild)
        {
            _steamBuild = steamBuild;
        }

        public bool Initialize()
        {
            try
            {
                var config = ConfigurationStorage.Load();

                var protocol = config.GetPlatformDeepLinkingProtocols(SupportedPlatforms.Linux,true).FirstOrDefault();
                if (protocol == null)
                    return false;

                _scheme = protocol.Scheme;
                var activationProtocol = protocol.Scheme;
                var fromSteam = _steamBuild;

                var steamAppId = fromSteam ? config.SteamId : string.Empty;

                var home = Environment.GetEnvironmentVariable("HOME");

                const string mimeTypePrefix = "x-scheme-handler/";



                var mimeType = mimeTypePrefix + activationProtocol;
                var desktopFilename = activationProtocol + ".desktop";
                var desktopFile = home + LocalAppDir + desktopFilename;
                var mimeapps = home + LocalAppDir + "mimeapps.list";

                HandleMimefile(mimeType, desktopFilename, mimeapps);

                HandleDesktopFile(activationProtocol, desktopFile, steamAppId);

                SetupMimeType(activationProtocol);
                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("Linux Provider " + e);
                return false;
            }
        }

        private void SetupMimeType(string protocol)
        {
            Process foo = new Process();
            foo.StartInfo.FileName = "xdg-mime";
            foo.StartInfo.Arguments = string.Format("default {0}.desktop x-scheme-handler/{0}", protocol);
            foo.Start();

        }

        

        private static void HandleDesktopFile(string activationProtocol, string desktopFile, string steamAppId)
        {
            using (var f = File.Open(desktopFile, FileMode.OpenOrCreate))
            {
                using (var writter = new StreamWriter(f))
                {

                    var filename = ProviderHelpers.GetExecutingPath();

                    if (filename.Contains(" "))
                        filename = "\"" + filename + "\"";

                    var icon = filename;

                    if (string.IsNullOrEmpty(LinkProviderFactory.DeferredExePath) == false)
                    {
                        filename = LinkProviderFactory.DeferredExePath;
                    }

                    if (string.IsNullOrEmpty(steamAppId) == false)
                        filename = "steam -applaunch " + steamAppId;

                   
                    writter.Write(DesktopFileFormat, filename, icon, activationProtocol);
                }
            }
        }

      
        private void HandleMimefile(string mimeType, string desktopFilename, string mimeapps)
        {


            string allContent = "";

            if (File.Exists(mimeapps))
            {
                var stream = File.Open(mimeapps, FileMode.OpenOrCreate);
                allContent = new StreamReader(stream).ReadToEnd();
                allContent = RemoveMimeFromContent(mimeType, allContent);
                stream.Dispose();
                File.Delete(mimeapps);
            }

            bool hasContent = allContent.Length == 0;
            if (hasContent)
            {
                allContent += "[Added Associations]" + Environment.NewLine;
            }


            allContent += string.Format("{2}{0}={1}", mimeType, desktopFilename,
                hasContent ? string.Empty : Environment.NewLine);

            using (var f = File.Open(mimeapps, FileMode.OpenOrCreate))
            {
                using (var writter = new StreamWriter(f))
                {
                    writter.Write(allContent.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine));
                }
            }

        }

        private string RemoveMimeFromContent(string mime, string str)
        {
            var idx = str.IndexOf(mime, StringComparison.InvariantCultureIgnoreCase);
            if (idx == -1)
                return str;

            var newLineIdx = str.IndexOf("\n", idx, StringComparison.InvariantCultureIgnoreCase);

            if (newLineIdx == -1)
                newLineIdx = str.Length - 1;
            var stringToRemove = str.Substring(idx, newLineIdx - idx + 1);
            return str.Replace(stringToRemove, String.Empty);
        }


        private event Action<string> _linkReceived;
        public event Action<string> LinkReceived
        {
            add
            {
                _linkReceived += value;
                CheckArguments();
            }
            remove { _linkReceived -= value; }
        }

        private void CheckArguments()
        {
            if (string.IsNullOrEmpty(_scheme))
                return;

            var arg = Environment.GetCommandLineArgs().FirstOrDefault(a => a.StartsWith(_scheme));

            if (string.IsNullOrEmpty(arg) == false)
                OnLinkReceived(arg);
        }

        public void PollInfoAfterPause()
        {

        }

        protected virtual void OnLinkReceived(string obj)
        {
            var handler = _linkReceived;
            if (handler != null) handler(obj);
        }

    }
#endif

}
