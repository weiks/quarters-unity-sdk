using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ImaginationOverflow.UniversalDeepLinking;
using UnityEngine;

namespace ImaginationOverflow.UniversalDeepLinking
{
    [Serializable]
    public enum SupportedPlatforms
    {
        Android = 0,
        iOS = 1,
        UWP = 2,
        Windows = 3,
        OSX = 4,
        Linux = 5,
        tvOS = 6
    }

    [Serializable]
    public class PlatformLinkingConfiguration
    {

        [SerializeField]
        private List<LinkInformation> _domainProtocols;
        [SerializeField]
        private List<LinkInformation> _deepLinkingProtocols;
        [SerializeField]
        private bool _initialized;

        public bool IsInitialized { get { return _initialized; } set { _initialized = value; } }

        public List<LinkInformation> DeepLinkingProtocols
        {
            get { return _deepLinkingProtocols; }
            set { _deepLinkingProtocols = value; }
        }

        public List<LinkInformation> DomainProtocols
        {
            get { return _domainProtocols; }
            set { _domainProtocols = value; }
        }
        public PlatformLinkingConfiguration(bool init = false)
        {
            _initialized = init;
            DeepLinkingProtocols = new List<LinkInformation>();
            DomainProtocols = new List<LinkInformation>();
        }
    }


    [Serializable]
    public class AppLinkingConfiguration
    {
        [SerializeField]
        private string _steamId;
        [SerializeField]
        private string _displayName;
        [SerializeField]
        private PlatformLinkingConfiguration _globalConfiguration;

        [SerializeField]
        private PlatformLinkingConfiguration[] _customDeepLinkingProtocols;


        public string SteamId
        {
            get { return _steamId; }
            set { _steamId = value; }
        }

        public List<LinkInformation> DeepLinkingProtocols
        {
            get { return _globalConfiguration.DeepLinkingProtocols; }
            set
            {
                _globalConfiguration.DeepLinkingProtocols = value;
            }
        }



        public List<LinkInformation> DomainProtocols
        {
            get { return _globalConfiguration.DomainProtocols; }
            set
            {
                _globalConfiguration.DomainProtocols = value;

            }
        }

        public PlatformLinkingConfiguration[] CustomDeepLinkingProtocols
        {
            get { return _customDeepLinkingProtocols; }
            set
            {
                _customDeepLinkingProtocols = value;
                _customDeepLinkingProtocols = EnsureAllPlats(value);

            }
        }

        public string DisplayName
        {
            get { return _displayName; }
            set { _displayName = value; }
        }


        

        public AppLinkingConfiguration()
        {
            _globalConfiguration = new PlatformLinkingConfiguration();
            _customDeepLinkingProtocols = new PlatformLinkingConfiguration[Enum.GetValues(typeof(SupportedPlatforms)).Length];
            for (int i = 0; i < _customDeepLinkingProtocols.Length; i++)
            {
                _customDeepLinkingProtocols[i] = new PlatformLinkingConfiguration();
            }
        }

        internal void EnsureAllPlats()
        {
            CustomDeepLinkingProtocols = CustomDeepLinkingProtocols;
        }

        public List<LinkInformation> GetPlatformDeepLinkingProtocols(SupportedPlatforms plat, bool includeDefault = false)
        {
            return GetCustomOrDefault(plat, c => c.DeepLinkingProtocols, includeDefault, DeepLinkingProtocols);

        }

        public List<LinkInformation> GetPlatformDomainProtocols(SupportedPlatforms plat, bool includeDefault = false)
        {
            return GetCustomOrDefault(plat, c => c.DomainProtocols, includeDefault, DomainProtocols);

        }

        private List<LinkInformation> GetCustomOrDefault(SupportedPlatforms plat, Func<PlatformLinkingConfiguration, List<LinkInformation>> func, bool includeDefault, List<LinkInformation> global)
        {
            var idx = (int)plat;

            var platConfig = CustomDeepLinkingProtocols[idx];

            if (platConfig != null && platConfig.IsInitialized)
                return func(platConfig);

            if (includeDefault)
                return global;

            return null;
        }

        public List<LinkInformation> GetCustomDeepLinkingProtocols(SupportedPlatforms value)
        {
            return CustomDeepLinkingProtocols[(int)value].DeepLinkingProtocols;
        }

        public List<LinkInformation> GetCustomDomainAssociation(SupportedPlatforms value)
        {
            return CustomDeepLinkingProtocols[(int)value].DomainProtocols;
        }

        public void ActivatePlatformOverride(SupportedPlatforms value)
        {
            _customDeepLinkingProtocols[(int)value].IsInitialized = true;
        }
        public void DeactivatePlatformOverride(SupportedPlatforms value)
        {
            _customDeepLinkingProtocols[(int)value].IsInitialized = false;
            _customDeepLinkingProtocols[(int)value].DeepLinkingProtocols.Clear();
            _customDeepLinkingProtocols[(int)value].DomainProtocols.Clear();
        }

        private PlatformLinkingConfiguration[] EnsureAllPlats(PlatformLinkingConfiguration[] value)
        {
            var len = Enum.GetValues(typeof(SupportedPlatforms)).Length;

            if (len == value.Length)
                return value;

            var arr = new PlatformLinkingConfiguration[len];

            for (int i = 0; i < arr.Length; i++)
            {
                if (i < value.Length)
                    arr[i] = value[i];
                else
                    arr[i] = new PlatformLinkingConfiguration();
            }

            return arr;
        }
    }
}
