# Quarters Unity SDK

  - [Before you start](#before-you-start)
  - [Getting Started](#getting-started)
    - [Installation](#installation)
    - [Setup](#setup)
      - [iOS](#ios)
      - [Android](#android)
  - [Sample app](#sample-app)
  - [Unity supported versions](#unity-supported-versions)
  - [SDK supported platforms](#sdk-supported-platforms)
  - [SDK calls to API Reference](#sdk-calls-to-api-reference)
    - [Sign in with Quarters](#sign-in-with-quarters)
    - [Making Transactions](#making-transactions)
    - [Buying Quarters](#buying-quarters)
    - [Sign Out](#sign-out)
  - [Troubleshooting](#troubleshooting)
    - [Pay or receive Quarters throws *Transaction Error*](#pay-or-receive-quarters-throws-transaction-error)
    - [Pay or receive Quarters throws *Missing refresh token*](#pay-or-receive-quarters-throws-missing-refresh-token)
    - [Pay throws *Your app must be verified to transfer Quarters from users*](#pay-throws-your-app-must-be-verified-to-transfer-quarters-from-users)
    - [Pressing `Authorize` in Android mobile doesn't redirect to the app.](#pressing-authorize-in-android-mobile-doesnt-redirect-to-the-app)
    - [Pressing `Authorize` in iOS mobile doesn't redirect to the app.](#pressing-authorize-in-ios-mobile-doesnt-redirect-to-the-app)
    - [Making a transaction (pay or receive) shows `Transaction error: Debit and credit address cannot be the same.`](#making-a-transaction-pay-or-receive-shows-transaction-error-debit-and-credit-address-cannot-be-the-same)

## Before you start
In order to have a full integration with QuartersSDK you must follow this steps: 
- Register your APP following [the steps in poq-docs](https://github.com/weiks/poq-docs/blob/main/docs/unity-sdk-integration.md).
- It's mandatory to verify your app before your app can take Quarters from users. Please follow these steps: 
   1. Join our [`PoQ Game Devs` discord server](https://discord.gg/yQxYgRx3n8) 
   2. Send us a message to our public channel [💡┋api-and-integration](https://discord.com/channels/908772014859378708/910205059403509803) where we are going to handle your request. 

Also if you want you can join [`Pocketful Of Quarters` main discord server](https://discord.com/invite/poq) to be updated with the latest news and meet the rest of the community.

## Getting Started
### Installation
1. Open Unity package manager
2. Select **`"+"`** button and then **Add the package from git repository** (or if you have downloaded [QuartersSDK Unity project](https://github.com/weiks/quarters-unity-sdk) you can import it from there selecting `package.json` inside `quarters-unity-sdk-weiks\Packages\gg.poq.unity.sdk`)

![Screenshot 2022-05-31 at 9 07 02 am](https://user-images.githubusercontent.com/41578378/171151345-c5cc06a3-4b30-48aa-b2ea-ae2542c810fe.png)

3. Enter the URL from [poq-unity-package-manager](https://github.com/weiks/poq-unity-package-manager.git) project repository and press **Add** button. Unity Package Manager will pull Quarters Unity SDK and all of its dependencies to the project.
4. Add the following prefab to your first loaded scene `Packages/Quarters Unity SDK/Runtime/QuartersSDK/Prefabs/QuartersInit`

![Screenshot 2022-05-31 at 9 39 47 am](https://user-images.githubusercontent.com/41578378/171151505-6682fe08-0d13-4feb-8428-3f8b6adbc7d8.png)

5. Press **Open App Dashboard** button. The web browser will open [URL to create new Quarters app](https://apps.pocketfulofquarters.com/apps/new). Press **Save** button after populating the form.

![Screenshot 2022-05-31 at 9 44 27 am](https://user-images.githubusercontent.com/41578378/171151698-45a22b3d-134f-4894-8859-c4e125e99392.png)

6. Back in Unity `QuartersInit` component copy the values from app dashboard to the inpector tab of `QuartersInit` Component:
   - `APP_ID` -> `client_id` 
   - `APP_KEY` -> `client_secret`
   - `APP_UNIQUE_IDENTIFIER` -> It needs to be `App URL` sub-domain (ex. in this case exampleapp as in the image below)  
   Please check the addiotional values you can set in the Unity inspector tab:
   - `Environment` -> select `Production` if your app was registered in [production apps environment](https://apps.pocketfulofquarters.com/) otherwise select `Sandbox`. 
   - `Default scope` -> select the 5 scopes (`Identity`, `Email`, `Transactions`, `Events` and `Wallet`).
   - `Currency config` -> set it in `QuartersCurrency` 

![Screenshot 2022-05-31 at 9 46 25 am](https://user-images.githubusercontent.com/41578378/171151877-db5b23f4-43ff-4b8f-acdb-4e381bdec7a6.png)

### Setup
For the best user experience, Quarters Unity SDK utilises domain association to link users back to the app after purchasing and authorisation. Quarters SDK manages browser to app linking automatically. To set up linking please follow the steps for the chosen platform

#### iOS
1. Open [your PoQ APPS](https://www.poq.gg/apps)
2. Find your app and select iOS under the Auto Manage option
3. Enter any valid string as your PoQ app's unique identifier if empty
4. Enter Apple Team ID. It can be pulled from [Apple 
account](https://developer.apple.com/account/#!/membership/)
5. Enter your app bundle id. Example com.mycompany.mygame
6. Press Submit
7. Copy your PoQ app's unique identifier to the `QuartersInit` component `APP_UNIQUE_IDENTIFIER` field in Unity and press Save

#### Android
1. Open [your PoQ APPS](https://www.poq.gg/apps)
2. Find your app and select Android under the Auto Manage option
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

**That's it you are ready to use Quarters Unity SDK**

## Sample app
Quarters Unity SDK contains a basic sample app presenting major SDK functionality like
- Sign In with Quarters
- Sign Out
- Spend (*Pay Quarters* menu option): need to register your app manually contacting us on any channel as described in [Before Start section](#before-you-start). For this transaction you won't be able to use the same account that you used as a developer to register your app. 
- Receive (*Receive Quarters* menu option) Quarters. For this transaction you won't be able to use the same account that you used as a developer to register your app.
- Buying (*Buy Quarters* menu option) Quarters through the website portal

To import the sample scene select Sample Import from Unity Package Manager


https://user-images.githubusercontent.com/41578378/171196202-3669bee8-29bb-4047-a914-326e6ec9d5dd.mov

## Unity supported versions
Unity 2019.4+

## SDK supported platforms
- iOS
- Android: API 29 and above
- Unity Editor: due to limitations of Unity Editor on Windows external browser is used to authorize user you can check [sign-in-with-quarters](#please-note-that-due-to-limitations-of-unity-editor-on-windows-external-browser-is-used-to-authorize-user-after-successful-authorisation-please-copy-and-paste-the-browser-url-to-unity-game-view-and-press-authorize) for details. 

## SDK calls to API Reference
In order to make all the operations/transactions with Quarters you will be able to interact with the SDK calls that consumes an API. See that calls [fully documented with methods and examples in this README-Api file](/README-Api.md). Here we will list a quick review of the API operations: 

### Sign in with Quarters
Once `QuartersInit` is completed configured you will be able to sign in.

You will find the method to use and an example in [fully documented SDK calls documentation ](/README-Api.md#sign-in-with-quarters).

**Please note that due to limitations of Unity Editor on Windows external browser is used to authorize user. After successful authorisation please copy and paste the browser url to unity game view and press Authorize.**

![Capture](https://user-images.githubusercontent.com/41578378/172198600-980454b9-e260-4719-8ad8-621809a2ad14.PNG)


### Making Transactions
You can charge user Quarters as well as reward your user with quarters using unified Transaction API call.
**A negative price takes Quarters from the user's account. Positive price value reward user account with Quarters**

Please go to the [SDK calls to API documentation](/README-Api.md#making-transactions) for method description and a code example.

### Buying Quarters
User can also purchase Quarters by using the credit card or other methods.

**Please note that real money transactions are performed outside the application in the browser to adhere to Apple and Google's guidelines.**

Please go to the [SDK calls to API documentation](/README-Api.md#buying-quarters) for method description and a code example.

### Sign Out
To sign out current Quarters users call.

Please go to the [SDK calls to API documentation](/README-Api.md#sign-out) for method description and a code example.

## Troubleshooting

⚠️ Important notes: ⚠️

Currently you cannot make a transfer between your app and your developer account. To test your app, you need to create a secondary test account on https://www.poq.gg/. For more information about transfers you can [check our API documentation](https://github.com/weiks/poq-docs/blob/main/docs/oauth-api.md#post-apiv1transactions).

### Pay or receive Quarters throws *Transaction Error*
When you try to Pay or receive Quarters you get *Transaction Error: Debit and Cretit address cannot be the same. You cannot test your app when you are logged into your developer account* to fix it:
1. Sign Out.
2. Sign In with a registered user that is has not developer rights. 

### Pay or receive Quarters throws *Missing refresh token*
When you try to Pay or receive Quarters you get *Missing refresh token* to fix it:
1. Sign Out.
2. Sign In again with the same account.

### Pay throws *Your app must be verified to transfer Quarters from users*
When you try to Pay or receive Quarters you get *Your app must be verified to transfer Quarters from users* to fix it:
1. Verify your app [contacting us](#before-you-start).

### Pressing `Authorize` in Android mobile doesn't redirect to the app.
When you `Sign In` with the sample app you are not redirected to the app after pressing `Authorize` button in your browser.

You need to:
#### A) Set your Android `manifest.xml` settings to open your app url
Open your Android manifest file and add these lines to enable the link redirection option in your app:
```
   <intent-filter android:autoVerify="true">
        <action android:name="android.intent.action.VIEW" />
        <category android:name="android.intent.category.DEFAULT" />
        <category android:name="android.intent.category.BROWSABLE" />
        <data android:scheme="https" android:host="#YOUR_APP_LINK.games.poq.gg" android:pathPattern=".*" />
      </intent-filter>
```
If your manifest already had these lines, skip to [step B](set-your-android-default-browser-to-chrome) if that was not the case, save your manifest and rebuild your app (important: just to be sure please delete your build output folder before rebuild)  to generate the new installer that you will use for steps B and C. 

#### B) Set your Android default browser to Chrome
Some mobile internet browsers (like Samsung default browser) don't redirect to the app inmediatly. In case you want to be redirected inmediatly to the app change your default internet mobile browser:
1. On your Android phone, tap `Settings`.
2. Then tap `Apps & Notifications` (or in old Android versions `App Management`) and finally `Default Apps`
3. There on the option `Browser App` you will be able to change your default mobile browser
![image](https://user-images.githubusercontent.com/3865131/173102296-e0a56135-b621-491f-9326-4aafdcbdd983.png)

#### C) Enable support to web addresses in your app
1. On your Android phone, tap `Settings`.
2. Then tap `Apps & Notifications` (or in old Android versions `App Management`) `Default Apps/Openning links`
3. Choose your app (ex `Quarters SDK`) there you will be able to enable `Open supported links`.
4. Open `Supported web links` to check that the link of your app is in that list.

You can follow the steps in this video:



https://user-images.githubusercontent.com/3865131/188503301-94ddc74a-5dbb-45ae-9dc3-dedbd1fe6785.mp4






If after performing steps A, B and C the problem persists, please read the following troubleshooting case [Still not redirecting to the app after pressing the `Authorize` button](#still-not-redirecting-to-the-app-after-pressing-the-authorize-button)

### Pressing `Authorize` in iOS mobile doesn't redirect to the app.
#### In the project target, “Signing & Capability” tab check that 'Associated Domains' is enabled with the right domain.
<img width="690" alt="Screen Shot 2022-09-16 at 17 31 36" src="https://user-images.githubusercontent.com/3978399/190738163-3b14952a-9461-48e0-93bd-bec4d9bbdb70.png">

#### If you don't have 'Associated Domains' in "Signing & Capability".

##### A) Configure your app to register approved domains
Enter to your [Apple Developer Certificates, Identifiers & Profiles](https://developer.apple.com/account/resources/identifiers/list)
Find your identifier and enable  `Associated Domains` 

<img width="1273" alt="Screen Shot 2022-09-16 at 17 27 27" src="https://user-images.githubusercontent.com/3978399/190743949-a5467b13-cbce-4859-af2e-7eba1b9b9def.png">

##### B) Enable `Associated Domain` in your Xcode project
<img width="690" alt="Screen Shot 2022-09-16 at 17 29 29" src="https://user-images.githubusercontent.com/3978399/190745229-528e972c-49a8-46ea-93ec-b555391335af.png">
<img width="731" alt="Screen Shot 2022-09-16 at 17 30 11" src="https://user-images.githubusercontent.com/3978399/190745716-0d362f10-66fd-4f0e-b49e-ce11454f2288.png">

Add your app domain

<img width="711" alt="Screen Shot 2022-09-16 at 17 30 34" src="https://user-images.githubusercontent.com/3978399/190745961-f3b22b66-5f87-484e-8bdd-5f356df65812.png">


### Still not redirecting to the app after pressing the `Authorize` button.
Besides trying the steps in "[Pressing `Authorize` in Android mobile doesn't redirect to the app](#pressing-authorize-in-android-mobile-doesnt-redirect-to-the-app)" the issue still happens please contact us.

PoQ dev team decided not to create a possible solution because it would apply to an end user and all the alternatives could expose them to phishing.
For example lets suppose this case where you set up a game called `Original Authorized game`: 
1. You verified it and has access to user's wallets.
2. Published it to `App Store` and `Google Play`.
3. Another app realizes that anyone can just get the troubleshooting link with validation code (generated by us for this issue) and paste it into any app, so they reuse the same link as your game (because they easily can, it's a public link).
4. Then when the user is redirected back and since your real `Original Authorized game` is not installed nothing opens and it shows the link with the code, whether encrypted or not.
5. User then goes to the fake app and pastes the code
6. The fake app calls our API with the code and gets an access token on behalf of `Original Authorized game`.
7. The fake app can do whatever `Original Authorized game` can do without any restrictions, and as PoQ we can't stop them.

### Making a transaction (pay or receive) shows `Transaction error: Debit and credit address cannot be the same.` 
When you create an app with your developer account supposse `X@domain.com` you login to the integration app using that developer account if you try to make a transaction (as long as it's pay or receive) you will get this message error:
![image](https://user-images.githubusercontent.com/3865131/185242146-fecfca01-ffcb-43bb-80e3-8353d867a90b.png)

To fix it: 
1. Log out from the integration app.
2. Login to the integration app with a different account than the one you used to create the app (`X@domain.com`).  


