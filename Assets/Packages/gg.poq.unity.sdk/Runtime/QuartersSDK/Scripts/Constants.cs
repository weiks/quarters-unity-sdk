using Newtonsoft.Json;

namespace QuartersSDK {
    public enum Environment {
        sandbox,
        production
    }

    public static class Constants {
        public const string REFRESH_TOKEN_KEY = "QuartersRefreshToken";
        public const string BUY_QUARTERS_BUTTON = "Buy Quarters";
        public const string QUARTERS_NOT_ENOUGH = "The address to debit doesn\'t have enough Quarters";
        public const string VSP_POQ_COMPANY_NAME = "Pocketful of Quarters Inc";
    }

    public enum Scope {
        identity,
        email,
        transactions,
        events,
        wallet
    }

    public enum LinkType {
        WebView,
        External,
        EditorExternal
    }
}