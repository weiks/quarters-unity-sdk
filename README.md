# Quarters Unity SDK

## Before you start
In order to have a full integration with QuartersSDK you will need to follow this steps: 
- Register your APP following [the steps in poq-docs](https://github.com/weiks/poq-docs/blob/main/docs/unity-sdk-integeration.md).
- It's mandatory to verify your app before your app can take Quarters from users. **Verify your app sending us a message to [💡┋api-and-integration](https://discord.com/channels/908772014859378708/910205059403509803) or if you are new on Discord please send us an invite [to request verification](https://discord.com/invite/poq)**.

⚠️ Important notes: ⚠️

Currently you cannot make a transfer between your app and your developper account. To test your app, you need to create a secondary test account on https://www.poq.gg/. For more information about transfers you can [check our API documentation](https://github.com/weiks/poq-docs/blob/main/docs/oauth-api.md#post-apiv1transactions).


## Getting Started
### Installation
1. Open Unity package manager
2. Select **`"+"`** button and then **Add the package from git repository** (or if you have downloaded [QuartersSDK Unity project](https://github.com/weiks/quarters-unity-sdk) you can import it from there selecting `package.json` inside `quarters-unity-sdk-weiks\Packages\gg.poq.unity.sdk`)

![Screenshot 2022-05-31 at 9 07 02 am](https://user-images.githubusercontent.com/41578378/171151345-c5cc06a3-4b30-48aa-b2ea-ae2542c810fe.png)

1. Enter the URL from [poq-unity-package-manager](https://github.com/weiks/poq-unity-package-manager.git) project repository and press **Add** button. Unity Package Manager will pull Quarters Unity SDK and all of its dependencies to the project.
2. Add the following prefab to your first loaded scene `Packages/Quarters Unity SDK/Runtime/QuartersSDK/Prefabs/QuartersInit`

![Screenshot 2022-05-31 at 9 39 47 am](https://user-images.githubusercontent.com/41578378/171151505-6682fe08-0d13-4feb-8428-3f8b6adbc7d8.png)

5. Press **Open App Dashboard** button. The web browser will open [URL to create new Quarters app](https://apps.pocketfulofquarters.com/apps/new). Press **Save** button after populating the form.

![Screenshot 2022-05-31 at 9 44 27 am](https://user-images.githubusercontent.com/41578378/171151698-45a22b3d-134f-4894-8859-c4e125e99392.png)

6. Back in Unity Quarters Init component copy the following values from app dashboard to the Quarters Init Component.
   - `client_id` -> `APP_ID`
   - `client_secret` -> `APP_KEY`
   - `App URL` sub-domain (ex. in this case exampleapp) -> `APP_UNIQUE_IDENTIFIER`

![Screenshot 2022-05-31 at 9 46 25 am](https://user-images.githubusercontent.com/41578378/171151877-db5b23f4-43ff-4b8f-acdb-4e381bdec7a6.png)

### Setup
For the best user experience, Quarters Unity SDK utilises domain association to link users back to the app after purchasing and authorisation. Quarters SDK manages browser to app linking automatically. To set up linking please follow the steps for the chosen platform

#### iOS
1. Open [your PoQ APPS](https://www.poq.gg/apps)
2. Find your app and select iOS under the Auto Manage option
3. Enter any valid string as your PoQ app's unique identifier if empty
4. Enter Apple Team ID. It can be pulled from [Apple developer account](https://developer.apple.com/account/#!/membership/)
5. Enter your app bundle id. Example com.mycompany.mygame
6. Press Submit
7. Copy your PoQ app's unique identifier to the Quarters Init component APP_UNIQUE_IDENTIFIER field in Unity and press Save

#### Android
1. Open [your PoQ APPS](https://www.poq.gg/apps)
2. Find your app and select iOS under the Auto Manage option
3. Enter any valid string as your PoQ app's unique identifier if empty
4. Enter your app package name. Example com.mycompany.mygame
5. You need to get SHA-256 certificate fingerprint, to do that just run the following command on your terminal (note the keytool comes with the Java SDK or included with Unity Android OpenJDK ex. `C:\Program Files\Unity\Hub\Editor\2021.3.2f1\Editor\Data\PlaybackEngines\AndroidPlayer\OpenJDK\jre\bin` in case of Unity 2021.3.2f1 editor)

```
keytool -list -v -keystore mystorekeystore.keystore
```
Running this command should yield something similar to the following image:

![3985997309-keystool](https://user-images.githubusercontent.com/41578378/171152564-6ac8e026-dd86-4c06-9c99-43ced71d7005.png)

6. Press Submit
7. If you haven't already copy your PoQ app's unique identifier to the Quarter's Init component `APP_UNIQUE_IDENTIFIER` field in Unity and press Save

### That's it you are ready to use Quarters Unity SDK

## Sample app
Quarters Unity SDK contains a basic sample app presenting major SDK functionality like
- Sign In with Quarters
- Sign Out
- Spend (*Pay Quarters* menu option): need to register your app manually contacting us on any channel as described in [Before Start section](#before-you-start) 
- Receive (*Receive Quarters* menu option) Quarters
- Buying (*Buy Quarters* menu option) Quarters through the website portal

To import the sample scene select Sample Import from Unity Package Manager


https://user-images.githubusercontent.com/41578378/171196202-3669bee8-29bb-4047-a914-326e6ec9d5dd.mov




## Unity supported versions
Unity 2019.4+

## SDK supported platforms
- iOS
- Android: API 29 and above
- Unity Editor: due to limitations of Unity Editor on Windows external browser is used to authorize user you can check [sign in doc](#please-note-that-due-to-limitations-of-unity-editor-on-windows-external-browser-is-used-to-authorize-user-after-successful-authorisation-please-copy-and-paste-the-browser-url-to-unity-game-view-and-press-authorize) for details. 

## API Reference
### Quarters Initialisation
Before making any of the Quarters SDK calls you must call the following.

```
private void Start() {
    QuartersInit.Instance.Init(OnInitComplete, OnInitError);
}


private void OnInitComplete() {

}

private void OnInitError(string error) {
    Debug.LogError(error);
}
```

### Sign in with Quarters
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
#### Please note that due to limitations of Unity Editor on Windows external browser is used to authorize user. After successful authorisation please copy and paste the browser url to unity game view and press Authorize.
![Capture](https://user-images.githubusercontent.com/41578378/172198600-980454b9-e260-4719-8ad8-621809a2ad14.PNG)


### Making Transactions
You can charge user Quarters as well as reward your user with quarters using unified Transaction API call.
**A negative price takes Quarters from the user's account. Positive price value reward user account with Quarters**

```
public void ButtonTapped() {

   long price = -10;

   Quarters.Instance.Transaction(price, "Example transaction", OnTransferSuccessful, OnTransferFailed );

}

private void OnTransferSuccessful() {
  
}

private void OnTransferFailed(string error) {

}
```

### Buying Quarters
User can also purchase Quarters by using the credit card or other methods.

**Please note that real money transactions are performed outside the application in the browser to adhere to Apple and Google's guidelines.**

```
Quarters.Instance.BuyQuarters();
```

### Sign Out
To sign out current Quarters users call
```
Quarters.Instance.Deauthorize();
```

## Troubleshooting

### Pay or receive Quarters throws *Transaction Error*
When you try to Pay or receive Quarters you get *Transaction Error: Debit and Cretit address cannot be the same. You cannot test your app when you are logged into your developer account* to fix it:
1. Sign Out.
2. Sign In with a registered user that is has not developer rights. 

### Pay or receive Quarters throws *Missing refresh token*
When you try to Pay or receive Quarters you get *Missing refresh token* to fix it:
1. Sign Out.
2. Sign In again with the same account.

### Pay throws *Your app must ve verified to transfer Quarters from users*
When you try to Pay or receive Quarters you get *Your app must ve verified to transfer Quarters from users* to fix it:
1. Verify your app [contacting us](#before-you-start).

