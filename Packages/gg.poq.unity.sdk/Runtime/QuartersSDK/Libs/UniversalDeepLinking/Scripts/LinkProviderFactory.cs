using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ImaginationOverflow.UniversalDeepLinking.Providers;

namespace ImaginationOverflow.UniversalDeepLinking
{
    public class LinkProviderFactory
    {
        /// <summary>
        /// Used to register an external executable file.
        /// </summary>
        public static string DeferredExePath;

        public ILinkProvider GetProvider(bool isSteamBuild)
        {
            return new DummyLinkProvider();
        }
    }
}
