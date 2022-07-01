using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ImaginationOverflow.UniversalDeepLinking;
using UnityEngine;

namespace ImaginationOverflow.UniversalDeepLinking.Providers
{
    public class DummyLinkProvider : ILinkProvider
    {
        public bool Initialize()
        {
            Debug.Log("Dummy init");
            return false;
        }

        public event Action<string> LinkReceived;
        public void PollInfoAfterPause()
        {

        }

        protected virtual void OnLinkReceived(string obj)
        {
            if (LinkReceived != null)
                LinkReceived(obj);
        }
    }
}
