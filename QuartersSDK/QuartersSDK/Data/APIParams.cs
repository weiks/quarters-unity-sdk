using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace QuartersSDK.Data
{
    public class APIParams
    {
        public string BASE_URL { get; private set; }
        public string DASHBOARD_URL { get; private set; }
        public string AVATAR_URL { get; private set; }
        public string API_ENDPOINT { get; private set; }
        public string BUY_ENDPOINT { get; private set; }
        public string USER_DETAILS_ENDPOINT { get; private set; }
        public string BALANCE_ENDPOINT { get; private set; }
        public string TRANSACTIONS_ENDPOINT { get; private set; }

        public string ApiURL { get { return $"{BASE_URL}{API_ENDPOINT}"; } }
        public string BuyURL { get { return $"{BASE_URL}{BUY_ENDPOINT}"; } }
        public string UserDetailsURL { get { return $"{ApiURL}{USER_DETAILS_ENDPOINT}"; } }
        public string BalanceURL { get { return $"{ApiURL}{BALANCE_ENDPOINT}"; } }
        public string TransactionsURL { get { return $"{ApiURL}{TRANSACTIONS_ENDPOINT}"; } }
        public string ApiTokenURL { get { return $"{BASE_URL}/api/oauth2/token"; } }
        public string ApiAuthorizeURL { get { return $"{BASE_URL}/oauth2/authorize"; } }


        public string AvatarURL(User u){ 
            return $"{AVATAR_URL}/{u.Id}/{u.AvatarUrl}"; 
        } 
        
        public APIParams(IConfigurationSection confSection)
        {
            DASHBOARD_URL = confSection.GetSection("DASHBOARD_URL").Value;
            BASE_URL = confSection.GetSection("BASE_URL").Value;
            API_ENDPOINT = confSection.GetSection("API_ENDPOINT").Value;
            BUY_ENDPOINT = confSection.GetSection("BUY_ENDPOINT").Value;
            USER_DETAILS_ENDPOINT = confSection.GetSection("USER_DETAILS_ENDPOINT").Value;
            BALANCE_ENDPOINT = confSection.GetSection("BALANCE_ENDPOINT").Value;
            TRANSACTIONS_ENDPOINT = confSection.GetSection("TRANSACTIONS_ENDPOINT").Value;
        }

        public APIParams(Dictionary<string,string> settings)
        {
            DASHBOARD_URL = settings["DASHBOARD_URL"];
            BASE_URL = settings["BASE_URL"];
            API_ENDPOINT = settings["API_ENDPOINT"];
            BUY_ENDPOINT = settings["BUY_ENDPOINT"];
            USER_DETAILS_ENDPOINT = settings["USER_DETAILS_ENDPOINT"];
            BALANCE_ENDPOINT = settings["BALANCE_ENDPOINT"];
            TRANSACTIONS_ENDPOINT = settings["TRANSACTIONS_ENDPOINT"];
        }

    }
}
