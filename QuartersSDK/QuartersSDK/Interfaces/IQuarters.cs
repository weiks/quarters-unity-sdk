using QuartersSDK.Data;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuartersSDK.Interfaces
{
    public interface IQuarters
    {
        public void Authorize(List<Scope> scopes, Action OnSuccess, Action<string> OnError) { }
        public void Deauthorize() { }

        public ResponseData GetAccessToken();
        public ResponseData GetRefreshToken(string code);
        public User GetUserDetailsCall();
        public long GetAccountBalanceCall();

        public void SignInWithQuarters(Action OnComplete, Action<string> OnError) { }
        public void MakeTransactionCall(long coinsQuantity, string description, Action OnSuccess, Action<string> OnError) { }
    }
}