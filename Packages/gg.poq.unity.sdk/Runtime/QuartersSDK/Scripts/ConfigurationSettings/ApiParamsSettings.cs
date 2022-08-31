using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packages.gg.poq.unity.sdk.Runtime.QuartersSDK.Scripts.ConfigurationSettings
{
    class ApiParamsSettings
    {
        public Dictionary<string, string> Settings;
        public ApiParamsSettings()
        {
            Settings = new Dictionary<string, string>();
            Settings.Add("DASHBOARD_URL", "https://sandbox.pocketfulofquarters.com/apps/");
            Settings.Add("BASE_URL", "https://www.poq.gg");
            Settings.Add("AVATAR_URL", "https://www.poq.gg/images/");
            Settings.Add("API_ENDPOINT", "/api/v1");
            Settings.Add("BUY_ENDPOINT", "/buy");
            Settings.Add("AUTHORIZE_ENDPOINT", "/oauth2/authorize");
            Settings.Add("USER_DETAILS_ENDPOINT", "/users/me");
            Settings.Add("BALANCE_ENDPOINT", "/wallets/@me");
            Settings.Add("TRANSACTIONS_ENDPOINT", "/transactions");
        }
    }
}
