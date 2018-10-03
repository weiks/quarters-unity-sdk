using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QuartersSDK {
	public class User {
    
		public string userId = "";
		public string displayName = "";
		public string email = "";
		public bool emailVerified = false;

        public List<Account> accounts = new List<Account>();


        public class Account {
            public string id = "";
            public string address = "";
            public DateTime created = DateTime.MinValue;
            public string userId = "";
            public Balance balance = null;
            public Reward reward = null;

            public long AvailableQuarters {
                get {
                    long result = 0;
                    if (balance != null) result += balance.quarters;
                    if (reward != null) result += reward.rewardAmount;

                    return result;
                }
            }


            public class Balance {
                public long quarters = 0;
                public string formattedQuarters = "";
                public long ethers = 0;
                public string formattedEthers = "";
            }

            public class Reward {
                public long rewardAmount = 0;
                public string rewardAmountFormatted = "";
            }

        }

    }




}
