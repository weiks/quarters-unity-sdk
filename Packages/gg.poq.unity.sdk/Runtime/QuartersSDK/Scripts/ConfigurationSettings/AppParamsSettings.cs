using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packages.gg.poq.unity.sdk.Runtime.QuartersSDK.Scripts.ConfigurationSettings
{
    class AppParamsSettings
    {
        public Dictionary<string, string> Settings;
        public AppParamsSettings(string appId, string appKey, string appName, string environment)
        {
            Settings = new Dictionary<string, string>();
            Settings.Add("APP_ID", appId);
            Settings.Add("APP_KEY", appKey);
            Settings.Add("APP_NAME", appName);
            Settings.Add("Environment", environment);
            Settings.Add("BASE_URL_PROD", "https://www.poq.gg");
            Settings.Add("BASE_URL_SANDBOX", "https://s2w-dev-firebase.herokuapp.com");
            Settings.Add("REDIRECT_URL", $"https://{appName}.games.poq.gg");
        }
    }
}
