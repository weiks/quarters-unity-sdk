using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using Newtonsoft.Json;

namespace QuartersSDK {
    public partial class Quarters : MonoBehaviour {


        public delegate void OnAwardQuartersSuccessDelegate();
        public delegate void OnAwardQuartersFailedDelegate(string error);


        public void AwardQuarters(int expectedAmount, OnAwardQuartersSuccessDelegate OnSuccessDelegate, OnAwardQuartersFailedDelegate OnFailedDelegate) {
            StartCoroutine(AwardQuartersCall(expectedAmount, OnSuccessDelegate, OnFailedDelegate));
        }
  

        private IEnumerator AwardQuartersCall(int expectedAward, OnAwardQuartersSuccessDelegate OnSucess, OnAwardQuartersFailedDelegate OnFailed) {

            //pull user details if dont exist
            if (CurrentUser == null) {
                bool isUserDetailsDone = false;
                string getUserDetailsError = "";

                StartCoroutine(GetUserDetailsCall(delegate (User user) {
                    //user details loaded
                    isUserDetailsDone = true;

                }, delegate (string userDetailsError) {
                    OnFailed("Getting user details failed: " + userDetailsError);
                    isUserDetailsDone = true;
                    getUserDetailsError = userDetailsError;
                }));

                while (!isUserDetailsDone) yield return new WaitForEndOfFrame();

                //error occured, break out of coroutine
                if (!string.IsNullOrEmpty(getUserDetailsError)) yield break;
            }



            
            ExecuteCloudScriptRequest request = new ExecuteCloudScriptRequest();
            request.FunctionName = "AwardQuarters";
            request.GeneratePlayStreamEvent = true;
            request.FunctionParameter = JsonConvert.SerializeObject(new {
                amount = expectedAward,
                userId = Quarters.Instance.CurrentUser.id
            });


            PlayFabClientAPI.ExecuteCloudScript(request, delegate(ExecuteCloudScriptResult result) {
            
                Debug.Log(JsonConvert.SerializeObject(result.FunctionResult));

                //TODO check for error

                OnSucess();

            }, delegate(PlayFabError error) {
                Debug.LogError(JsonConvert.SerializeObject(error.ErrorMessage));
            });

        }


    }
}
