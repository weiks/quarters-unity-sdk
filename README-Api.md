# API Documentation
## Quarters Initialization
Before making any of the Quarters SDK calls you must call the following:
 ```
 QuartersInit.Instance.Init(OnInitComplete, OnInitError);
 ```

Where:
| Parameter        | Required | Description                                                        |
| -----------------| -------- | ----------------------------------------------------------------   |
| `OnInitComplete` | yes      |  Runs this function if the Init function was successful            |
| `OnInitError`    | yes      |  Runs this function if there was an error with the Init function   |

For example:
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
Once Quarters Init is completed successfully you need to sign in your user calling:
```
Quarters.Instance.SignInWithQuarters(OnSignInComplete, OnSignInError);
```
Where:

| Parameter          | Required | Description                                                |
| ------------------ | -------- | ---------------------------------------------------------- |
| `OnSignInComplete` | yes      |  What happens when signing in is successful                |
| `OnSignInError`    | yes      |  Checks to see if there was an error in signing in         |

Example:
```
private void OnInitComplete() {
        
        /*Runs the SignInWithQuarters function of the Quarters class.
    Loads the player's web browser. If they are not logged into their PoQ account, the user will be prompted to sign in.
    Once the user is signed in, they will be redirected to the Authorization screen.
	The player must authorize access to their PoQ wallet to allow Quarters transactions.
    */

    Quarters.Instance.SignInWithQuarters(OnSignInComplete, OnSignInError);
}

//Runs if the sign in function was successful. Players can make Quarters transactions at this point.
private void OnSignInComplete() {

}

//Runs if there was an error during the Sign in process. Returns an error string
private void OnSignInError(string signInError) {
    Debug.Log(signInError);
}
```

## Authorization Screen
When the  `SignInWithQuarters` function is called, the player will be taken to the Quarters web page on their default browser. 
 
After the player has signed into their Quarters account (if they have not already done so) they will be prompted to allow the game access to the player’s PoQ account information. 

![AuthImage](https://github.com/BRoerish/TestRepo/blob/master/TestAuth.png)

⚠️ Important notes: ⚠️

The player must click the **Authorize** button to allow Quarters transactions. Any attempt to exchange Quarters, without being authorized first, will result in an error during the transaction process.


![AuthConsoleLog](https://github.com/BRoerish/TestRepo/blob/master/ConsoleLogEx.png)

Debug message for if a player clicks *Cancel* and attempts to exchange Quarters

#### Please note that due to limitations of Unity Editor on Windows external browser is used to authorize user. Additional steps must be taken to authorize a player in the Unity Editor

1. After clicking the **Authorize** button in the browser, the web browser will load a white page. 
2. Have the player copy this new URL
3. When the player returns from the web browser to the game, they will see this prompt in the Unity Editor:

![Capture](https://user-images.githubusercontent.com/41578378/172198600-980454b9-e260-4719-8ad8-621809a2ad14.PNG)

4. Have the player either click the **Paste and Authorize** button or have them paste the URL into the text box and have them click **Authorize**

The game will then finish the authorization process.

## Player Information
After a player has authorized your game to have access to their account information, you can use that data to keep track of their transactions during game play. These methods can be called after the player has signed into their Quarters account. 

### GetUserDetails
Grabs the display name of the player, after they have signed in to their Quarters account.

| Parameter        | Required | Description                                                        |
| -----------------| -------- | ----------------------------------------------------------------   |
| `user`           | yes      |  Collects the data from the user's account                         |
| `user.GamerTag`  | no       |  A string that conatains the player's Quarters display name        |

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
Grabs the Quarters balance of the player signed in.

| Parameter        | Required | Description                                                        |
| -----------------| -------- | ----------------------------------------------------------------   |
| `OnSuccess`      | yes      |  Runs if the GetAccountBalanceCall function was successful         |
| `balance`        | yes      |  The number of Quarters the player has in their account            |

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
