using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImaginationOverflow.UniversalDeepLinking.Providers
{
    public class EditorLinkProvider : ILinkProvider
    {
        private static EditorLinkProvider _instance;

        public EditorLinkProvider()
        {
            _instance = this;
        }

        public bool Initialize()
        {
            return true;
        }

        public event Action<string> LinkReceived;
        public void PollInfoAfterPause()
        {

        }

        public static void SimulateLink(string link)
        {
            if(_instance == null)
                return;

            var evt = _instance.LinkReceived;
            if (evt != null)
                evt(link);
        }
    }
}
