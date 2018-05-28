using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuartersSDK {
    public class QuartersSession {


        public string RefreshToken {
            get {
                return PlayerPrefs.GetString(Constants.REFRESH_TOKEN_KEY, "");
            }
            set {
                this.PersistSession(value);
            }
        }


        private string accessToken = "";
        public string AccessToken {
            get {
                return accessToken;
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
                return DoesHaveRefreshToken;
            }
        }



        private void PersistSession(string token) {
            PlayerPrefs.SetString(Constants.REFRESH_TOKEN_KEY, token);
        }

        public void Invalidate() {
            PlayerPrefs.DeleteKey(Constants.REFRESH_TOKEN_KEY);
        }


    }
}
