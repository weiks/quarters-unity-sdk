

namespace QuartersSDK {
    public static partial class Constants {


        public static string QUARTERS_PRODUCT_KEY {
            get {
                #if UNITY_IOS
                return "Quarters";

                #elif UNITY_ANDROID
                return "quarters";

                #endif
            }


        }





    }
}
