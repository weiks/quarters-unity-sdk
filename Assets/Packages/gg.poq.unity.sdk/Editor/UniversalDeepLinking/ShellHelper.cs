using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace ImaginationOverflow.UniversalDeepLinking.Editor
{
    public class ShellHelper
    {

        public class ShellRequest
        {
            public event Action<int, string> OnLog;
            public event Action OnError;
            public event Action OnDone;

            public void Log(int type, string log)
            {
                if (OnLog != null)
                {
                    OnLog(type, log);
                }

                if (type == 1)
                {
                    UnityEngine.Debug.LogError(log);
                }
            }

            public void NotifyDone()
            {
                if (OnDone != null)
                {
                    OnDone();
                }
            }

            public void Error()
            {
                if (OnError != null)
                {
                    OnError();
                }
            }
        }


        private static string _shellApp
        {
            get
            {
                if (IsMac)
                    return "bash";
                else
                    return "cmd.exe";
            }
        }

        private static bool IsMac
        {
            get
            {
                return (Application.platform == RuntimePlatform.OSXEditor ||
                        SystemInfo.operatingSystem.ToLower().Contains("mac"));
            }
        }


        private static List<System.Action> _queue = new List<System.Action>();


        static ShellHelper()
        {
            _queue = new List<System.Action>();
            EditorApplication.update += OnUpdate;
        }

        private static void OnUpdate()
        {
            for (int i = 0; i < _queue.Count; i++)
            {
                try
                {
                    var action = _queue[i];
                    if (action != null)
                    {
                        action();
                    }
                }
                catch (System.Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                }
            }

            _queue.Clear();
        }

        public static void Step()
        {
            OnUpdate();
        }

        public static ShellRequest ProcessFileCommand(string fileName, string parameters, string workDirectory = null)
        {
            ShellRequest req = new ShellRequest();

            Process p = null;
            try
            {
                ProcessStartInfo start = new ProcessStartInfo();

                start.FileName = fileName;
                start.Arguments = parameters;
                start.CreateNoWindow = true;
                start.ErrorDialog = true;
                start.UseShellExecute = false;

                if (workDirectory != null)
                    start.WorkingDirectory = workDirectory;

                if (start.UseShellExecute)
                {
                    start.RedirectStandardOutput = false;
                    start.RedirectStandardError = false;
                    start.RedirectStandardInput = false;
                }
                else
                {
                    start.RedirectStandardOutput = true;
                    start.RedirectStandardError = true;
                    start.RedirectStandardInput = true;
                    start.StandardOutputEncoding = System.Text.UTF8Encoding.UTF8;
                    start.StandardErrorEncoding = System.Text.UTF8Encoding.UTF8;
                }
                p = Process.Start(start);
                p.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs e) { UnityEngine.Debug.LogError(e.Data); };
                p.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e) { UnityEngine.Debug.LogError(e.Data); };
                p.Exited += delegate (object sender, System.EventArgs e) { UnityEngine.Debug.LogError(e.ToString()); };
                bool hasError = false;
                do
                {
                    string line = p.StandardOutput.ReadLine();
                    if (line == null)
                    {
                        break;
                    }
                    line = line.Replace("\\", "/");
                    _queue.Add(delegate () { req.Log(0, line); });
                } while (true);
                while (true)
                {
                    string error = p.StandardError.ReadLine();
                    if (string.IsNullOrEmpty(error))
                    {
                        break;
                    }
                    hasError = true;
                    _queue.Add(delegate () { req.Log(1, error); });
                }
                p.Close();
                if (hasError)
                {
                    _queue.Add(delegate () { req.Error(); });
                }
                else
                {
                    _queue.Add(delegate () { req.NotifyDone(); });
                }


            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogException(e);
                if (p != null)
                {
                    p.Close();
                }
            }
            return req;
        }

        public static ShellRequest ProcessCommand(string cmd, string workDirectory, List<string> environmentVars = null)
        {
            ShellRequest req = new ShellRequest();
            Process p = null;
            try
            {
                ProcessStartInfo start = new ProcessStartInfo(_shellApp);

                var splitChar = ":";
                if (IsMac)
                {
                    splitChar = ":";
                    start.Arguments = "-c ";
                }

                else
                {
                    splitChar = ";";
                    start.Arguments = "/c";
                }


                if (environmentVars != null)
                {
                    foreach (string var in environmentVars)
                    {
                        start.EnvironmentVariables["PATH"] += (splitChar + var);
                    }
                }
                start.Arguments += (" " + cmd + " "); //(" \"" + cmd + " \"");
                start.CreateNoWindow = true;
                start.ErrorDialog = true;
                start.UseShellExecute = false;
                start.WorkingDirectory = workDirectory;
                if (start.UseShellExecute)
                {
                    start.RedirectStandardOutput = false;
                    start.RedirectStandardError = false;
                    start.RedirectStandardInput = false;
                }
                else
                {
                    start.RedirectStandardOutput = true;
                    start.RedirectStandardError = true;
                    start.RedirectStandardInput = true;
                    start.StandardOutputEncoding = System.Text.UTF8Encoding.UTF8;
                    start.StandardErrorEncoding = System.Text.UTF8Encoding.UTF8;
                }
                p = Process.Start(start);
                p.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs e) { UnityEngine.Debug.LogError(e.Data); };
                p.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e) { UnityEngine.Debug.LogError(e.Data); };
                p.Exited += delegate (object sender, System.EventArgs e) { UnityEngine.Debug.LogError(e.ToString()); };
                bool hasError = false;
                do
                {
                    string line = p.StandardOutput.ReadLine();
                    if (line == null)
                    {
                        break;
                    }
                    line = line.Replace("\\", "/");
                    _queue.Add(delegate () { req.Log(0, line); });
                } while (true);
                while (true)
                {
                    string error = p.StandardError.ReadLine();
                    if (string.IsNullOrEmpty(error))
                    {
                        break;
                    }
                    hasError = true;
                    _queue.Add(delegate () { req.Log(1, error); });
                }
                p.Close();
                if (hasError)
                {
                    _queue.Add(delegate () { req.Error(); });
                }
                else
                {
                    _queue.Add(delegate () { req.NotifyDone(); });
                }


            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogException(e);
                if (p != null)
                {
                    p.Close();
                }
            }
            return req;
        }


        private List<string> _enviroumentVars = new List<string>();

        public void AddEnvironmentVars(params string[] vars)
        {
            for (int i = 0; i < vars.Length; i++)
            {
                if (vars[i] == null)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(vars[i].Trim()))
                {
                    continue;
                }

                _enviroumentVars.Add(vars[i]);
            }
        }

        public ShellRequest ProcessCMD(string cmd, string workDir)
        {
            return ShellHelper.ProcessCommand(cmd, workDir, _enviroumentVars);
        }
    }
}