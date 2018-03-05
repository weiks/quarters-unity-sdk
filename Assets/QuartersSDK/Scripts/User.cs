using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Quarters {
	public class User {

		public string id = "";
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

            public class Balance {
                public long quarters = 0;
                public string formattedQuarters = "";
                public long ethers = 0;
                public string formattedEthers = "";
            }

        }

    }




}
