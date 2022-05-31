# Quarters Unity SDK

## Getting Started
### Installation
1. Open Unity package manager
2. Select + button and Add the package from git URL

![Screenshot 2022-05-31 at 9 07 02 am](https://user-images.githubusercontent.com/41578378/171151345-c5cc06a3-4b30-48aa-b2ea-ae2542c810fe.png)

3. Enter https://github.com/weiks/poq-unity-package-manager.git and press Add. Unity Package Manager will pull Quarters Unity SDK and all of its dependencies to the project
4. Add the following prefab to your first loaded scene Packages/Quarters Unity SDK/Runtime/QuartersSDK/Prefabs/QuartersInit

![Screenshot 2022-05-31 at 9 39 47 am](https://user-images.githubusercontent.com/41578378/171151505-6682fe08-0d13-4feb-8428-3f8b6adbc7d8.png)

5. Press Open App Dashboard. The web browser will open https://apps.pocketfulofquarters.com/apps/new to create new Quarters app. Press Save after populating the form.

![Screenshot 2022-05-31 at 9 44 27 am](https://user-images.githubusercontent.com/41578378/171151698-45a22b3d-134f-4894-8859-c4e125e99392.png)

6. Back in Unity Quarters Init component copy the following values from app dashboard to the Quarters Init Component.

client_id -> APP_ID

client_secret -> APP_KEY

![Screenshot 2022-05-31 at 9 46 25 am](https://user-images.githubusercontent.com/41578378/171151877-db5b23f4-43ff-4b8f-acdb-4e381bdec7a6.png)

### Setup
For the best user experience, Quarters Unity SDK utilises domain association to link users back to the app after purchasing and authorisation. Quarters SDK manages browser to app linking automatically. To set up linking please follow the steps for the chosen platform

#### iOS
1. Open https://www.poq.gg/apps
2. Find your app and select iOS under the Auto Manage option
3. Enter any valid string as your PoQ app's unique identifier if empty
4. Enter Apple Team ID. It can be pulled from https://developer.apple.com/account/#!/membership/
5. Enter your app bundle id. Example com.mycompany.mygame
6. Press Submit
7. Copy your PoQ app's unique identifier to the Quarters Init component APP_UNIQUE_IDENTIFIER field in Unity and press Save
