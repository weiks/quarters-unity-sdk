using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImaginationOverflow.UniversalDeepLinking
{
    public delegate void LinkActivationHandler(LinkActivation s);
    public delegate void UniversalLinkCallback(string link);


    public interface ILinkProvider
    {
        bool Initialize();
        event Action<string> LinkReceived;
        void PollInfoAfterPause();
    }
}
