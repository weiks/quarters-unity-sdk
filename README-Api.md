# API Documentation
## Quarters Initialisation
Before making any of the Quarters SDK calls you must call the following.

```
private void Start() {
    // Runs the Init function of the Quarters class. Allows further Quarters functionality if completed
    QuartersInit.Instance.Init(OnInitComplete, OnInitError);
}

//Empty function for now. Gets filled in the next step
//Runs after the Init function has successfully finished
private void OnInitComplete() {

}

//Runs if there was an error during the Init process. Returns an error string.
private void OnInitError(string error) {
    Debug.LogError(error);
}
```

## Sign in with Quarters
Once Quarters Init is completed successfully you need to sign in your user

```
private void OnInitComplete() {
    Quarters.Instance.SignInWithQuarters(OnSignInComplete, OnSignInError);
}

private void OnSignInComplete() {

}

private void OnSignInError(string signInError) {
    Debug.Log(signInError);
}
```

## Authorization Screen
When the  SignInWithQuarters function is called, the player will be taken to the Quarters web page on their default browser. 
 
After the player has signed into their Quarters account (if they have not already done so) they will be prompted to allow the game access to the player’s PoQ account information. 

![AuthImage](https://github.com/BRoerish/TestRepo/blob/master/TestAuth.png)

⚠️ Important notes: ⚠️

The player must click the **Authorize** button to allow Quarters transactions. Any attempt to exchange Quarters, without being authorized first, will result in an error during the transaction process.


![AuthConsoleLog](https://github.com/BRoerish/TestRepo/blob/master/ConsoleLogEx.png)

Debug message for if a player clicks *Cancel* and attempts to exchange Quarters


## Player Information
After a player has authorized your game to have access to their account information, you can use that data to keep track of their transactions during game play. These scripts can be called after the player has signed into their Quarters account.

### GetUserDetails
```
//function used as an example for grabbing user information.
private void Example()
{
	//Calls the GetUserDetails function in the Quarters script
	Quarters.Instance.GetUserDetails(delegate (User user)
        {
	        //Posts the user’s GamerTag in the console log. user.GamerTag can be referenced as a string
            Debug.Log(user.GamerTag);

            //In case GetUserDetails runs into any errors, delegate can be used to to disclose what went wrong
        }, delegate (string error)
        {
            Debug.LogError(error);
        });

}

```

### GetAccountBalanceCall

```
//function used as an example for grabbing user’s Quarters balance
private void Example()
{

//Runs the GetAccountBalanceCall function in the Quarters script. Returns an error note in the console log, if there were any issues in the function.
Quarters.Instance.GetAccountBalanceCall(OnSuccess, delegate (string error) { Debug.LogError(error); });

}

//Runs if GetAccountBalanceCall was successful. Balance will be filled in with the number of Quarters the player has
Private void OnSuccess(long balance)
{
	//Posts the player’s account balance in the console log
	Debug.Log(balance);
}
```
