using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ImaginationOverflow.UniversalDeepLinking
{

    [Serializable]
    public class LinkInformation
    {
        [SerializeField]
        private string _scheme = string.Empty;
        [SerializeField]
        private string _host = string.Empty;
        [SerializeField]
        private string _path = string.Empty;

        public string Scheme
        {
            get { return _scheme; }
            set { _scheme = value; }
        }

        public string Host
        {
            get { return _host; }
            set { _host = value; }
        }

        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }
    }


}


