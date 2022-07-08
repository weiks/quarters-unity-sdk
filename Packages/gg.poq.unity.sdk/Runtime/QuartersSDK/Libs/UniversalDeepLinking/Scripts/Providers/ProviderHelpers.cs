using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ImaginationOverflow.UniversalDeepLinking
{


    public static class ProviderHelpers
    {
        public static string GetExecutingPath()
        {
            try
            {
                return System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            }
#pragma warning disable 168
            catch (Exception e)
#pragma warning restore 168
            {
                // ignored
            }

            var exe = Environment.GetCommandLineArgs()[0];

            var parentDataDir = Directory.GetParent(Application.dataPath).FullName;

            if (exe.Contains("/"))
                parentDataDir = parentDataDir.Replace('\\', Path.PathSeparator);
            else
                parentDataDir = parentDataDir.Replace('/', Path.PathSeparator);

            if (exe.StartsWith(parentDataDir))
                return exe;

            return Path.Combine(parentDataDir, Path.GetFileName(exe));

        }

    }
}
