using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace QuartersSDK.Data
{
    public class AppParams
    {
        [Header("Your Quarters app:")]
        [Header("Copy your App ID and App Key from your Quarters dashboard")]
        public string APP_ID;
        public string APP_KEY { get; }
        public string REDIRECT_URL { get; }

        public string SDK_VERSION { get; }

        public string Environment;
        public CurrencyConfig CurrencyConfig;
        public List<Scope> DefaultScope;

        public AppParams(IConfigurationSection confSection)
        {
            APP_ID = confSection.GetSection("APP_ID").Value;
            APP_KEY = confSection.GetSection("APP_KEY").Value;
            SDK_VERSION = confSection.GetSection("SDK_VERSION").Value;

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
