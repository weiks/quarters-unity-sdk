using QuartersSDK.Data;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuartersSDK.Interfaces
{
    public interface IQuarters
    {
        public void Authorize(List<Scope> scopes, Action OnSuccess, Action<string> OnError) { }
        public void SignInWithQuarters(Action OnComplete, Action<string> OnError) { }
        public void Deauthorize() { }
        public string GetAuthorizeUrl();
        public string GetBuyQuartersUrl();
        public ResponseData GetAccessToken();
        public ResponseData GetRefreshToken(string code);
        public User GetUserDetailsCall();
        public long GetAccountBalanceCall();
        public ResponseData MakeTransaction(long coinsQuantity, string description);
        public void BuyQuarters() { }
    }
}