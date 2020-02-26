using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QuartersSDK {
	public class User {

        public Action OnAccountsLoaded;

        public string userId = "";
		public string displayName = "";
		public string email = "";
		public bool emailVerified = false;

        public bool IsGuestUser {
            get {
                return email.Contains("@guest");
            }
        }

        public List<Account> accounts = new List<Account>();

        public Account MainAccount {
            get {
                if (accounts.Count > 0) {
                    return accounts[0];
                }
                else return null;
            }
        }


        public class Account {
            
            public Action<long> OnAvailableCoinsUpdated;
            
            public string id = "";
            public string address = "";
            public DateTime created = DateTime.MinValue;
            public string userId = "";

            private Balance balance = null;

            public Balance CurrentBalance {
                get { return balance; }
                set {
                    balance = value;
                    OnAvailableCoinsUpdated?.Invoke(AvailableCoins);
                }
            }

            private Reward currentReward;
            public Reward CurrentReward {
                get { return currentReward; }
                set {
                    currentReward = value;
                    OnAvailableCoinsUpdated?.Invoke(AvailableCoins);
                }
            }

            
            public long AvailableCoins {
                get {
                    long result = 0;
                    if (CurrentBalance != null) result += CurrentBalance.quarters;
                    if (CurrentReward != null) result += CurrentReward.rewardAmount;

                    return result;
                }
            }


            public class Balance {
                public long quarters = 0;
                public string formattedQuarters = "";
                public long ethers = 0;
                public string formattedEthers = "";
                
                //add delegate for easy refreshing?
            }

            public class Reward {
                public long rewardAmount = 0;
                public string rewardAmountFormatted = "";
            }

        }

    }




}
