using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Quarters {
	public class QuartersInit : MonoBehaviour {

		public static string APP_ID = "jR3GjKHUUtRoI6e8DPs2";
		public static string APP_KEY = "wfecoqrtyqrvne6l85dd07xvdkwtz06";

		private Quarters instance;

		void Awake() {

			DontDestroyOnLoad(this.gameObject);

			instance = new Quarters();


			StartCoroutine(Quarters.Instance.GetRefreshToken("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VySWQiOiJ4WEJTT2RNb0FIZGxDNTI1QXpXc1RZQzRyRnQyIiwicmFuZG9tIjoiMWY3ZDEyMTAtMTFhMC0xMWU4LTk0YjgtOTFhMmJhN2M1OTcyIiwiYXBwSWQiOiJqUjNHaktIVVV0Um9JNmU4RFBzMiIsInRva2VuVHlwZSI6Imp3dDphdXRob3JpemF0aW9uX2NvZGUiLCJpYXQiOjE1MTg2MjQwMDUsImV4cCI6MTUxODYyNDA2NX0.uW1kJ7-yfmFH61CQX83HW32vHTsf74LxIcHWS6_0xM0"));

		}



	}
}
