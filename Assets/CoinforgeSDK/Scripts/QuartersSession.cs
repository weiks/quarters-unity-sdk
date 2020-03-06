using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuartersSDK {
    public class QuartersSession {

        public bool IsGuestSession {
            get {
                return !string.IsNullOrEmpty(GuestToken);
            }
        }


        public string GuestToken {
            get {
                return PlayerPrefs.GetString(Constants.GUEST_TOKEN_KEY, "");
            }
            set {
                PlayerPrefs.SetString(Constants.GUEST_TOKEN_KEY, value);
            }
        }

        public string GuestFirebaseToken {
            get {
                return PlayerPrefs.GetString(Constants.GUEST_FIREBASE_TOKEN, "");
            }
            set {
                PlayerPrefs.SetString(Constants.GUEST_FIREBASE_TOKEN, value);
            }
        }


        public string RefreshToken {
            get {
                return PlayerPrefs.GetString(Constants.REFRESH_TOKEN_KEY, "");
            }
            set {
                PlayerPrefs.SetString(Constants.REFRESH_TOKEN_KEY, value);
            }
        }


        private string accessToken = "";
        public string AccessToken {
            get {
                if (IsGuestSession) return GuestToken;
                else {
                    return accessToken;
                }
            }
            set {
                accessToken = value;
            }
        }


        public bool DoesHaveRefreshToken {
            get {
                return !string.IsNullOrEmpty(RefreshToken);
            }
        }

        public bool DoesHaveAccessToken {
            get {
                return !string.IsNullOrEmpty(accessToken);
            }
        }


        public bool IsAuthorized {
            get {
                if (IsGuestSession) return true;
                else {
                    return DoesHaveRefreshToken;
                }
            }
        }


        public void InvalidateGuestSession() {
            PlayerPrefs.DeleteKey(Constants.GUEST_TOKEN_KEY);
            PlayerPrefs.DeleteKey(Constants.GUEST_FIREBASE_TOKEN);
        }


        public static void Invalidate() {
            PlayerPrefs.DeleteKey(Constants.REFRESH_TOKEN_KEY);
            PlayerPrefs.DeleteKey(Constants.GUEST_TOKEN_KEY);
            PlayerPrefs.DeleteKey(Constants.GUEST_FIREBASE_TOKEN);
        }


    }
}
