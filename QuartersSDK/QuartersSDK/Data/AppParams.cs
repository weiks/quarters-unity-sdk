using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace QuartersSDK.Data
{
    public class AppParams
    {
        public string APP_ID;
        public string APP_NAME { get; }
        public string REDIRECT_URL { get; }
        public string SCHEMA_URL { get; }

        public string Environment;
        public List<Scope> DefaultScope;

        public AppParams(IConfigurationSection confSection)
        {
            APP_ID = confSection.GetSection("APP_ID").Value;
            APP_NAME = confSection.GetSection("APP_NAME").Value;
            REDIRECT_URL = confSection.GetSection("REDIRECT_URL").Value.Replace("#REPLACE_WITH_APP_NAME", APP_NAME);

            this.DefaultScope = new List<Scope>() {
                Scope.identity,
                Scope.email,
                Scope.transactions,
                Scope.events,
                Scope.wallet
            };
        }

        public AppParams(Dictionary<string, string> settings)
        {
            APP_ID = settings["APP_ID"];
            APP_NAME = settings["APP_NAME"];
            REDIRECT_URL = settings["REDIRECT_URL"];

            this.DefaultScope = new List<Scope>() {
                Scope.identity,
                Scope.email,
                Scope.transactions,
                Scope.events,
                Scope.wallet
            };
        }
    }
}