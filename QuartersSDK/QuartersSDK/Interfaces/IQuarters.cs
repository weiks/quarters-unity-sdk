using QuartersSDK.Data;
using System;
using System.Collections.Generic;

namespace QuartersSDK.Interfaces
{
    public interface IQuarters
    {
        string GetAuthorizeUrl();
        string GetBuyQuartersUrl();
        ResponseData GetAccessToken();
        ResponseData GetRefreshToken(string code);
        User GetUserDetailsCall();
        long GetAccountBalanceCall();
        ResponseData MakeTransaction(long coinsQuantity, string description);
        
    }
}