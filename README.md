# PoQ Unity SDK

This project was made on `Unity v.2020.3.34f1`. 
## Install
1. Download [Unity](https://store.unity.com/es#plans-individual) and [Unity Hub](https://unity3d.com/es/get-unity/download)
2. Clone [quarters-unity-sdk](https://github.com/weiks/quarters-unity-sdk)
3. Open `Unity Hub` click on `Open/Add project from disk` and open the cloned repository. 
![image](https://user-images.githubusercontent.com/3865131/170773335-6ce1d317-fea5-41d8-ab18-cd4040ae2069.png)
4. Once the project loads click it and will open `Unity` environment
![image](https://user-images.githubusercontent.com/3865131/170773949-dd49d805-b299-4463-8bd1-811154a534da.png)

## Run
The SDK can be consume using:
- Unity Editor
- External Android/iOS app

### Unity Editor 
Once you load the project to test the SDK: 
- Go inside `Assest/Samples/Quarters SDK/2.0.0/Sample/Scene` in Unity Project view. There you will find an scene file called `SampleScene` (.unity extension).

![image](https://user-images.githubusercontent.com/3865131/171059181-69a3eb84-12a6-44f0-9377-17315f52740a.png)
- Load the scene (clicking on it) and select the `QuartersInitInit` component in `Hierarchy` view

![image](https://user-images.githubusercontent.com/3865131/171059753-49600e84-0a1d-4ec5-bcf4-ef353951d35a.png)
- Open Unity inspector view and fill the APP_ID (equivalent to `client_id``) , APP_KEY (equivalent to `client_secret`), APP_UNIQUE_IDENTIFIER (equivalent to `Server API token`) with the correct values from your APP (the one rigistered in https://sandbox.pocketfulofquarters.com/apps).

- Press play and you will be able to use the SDK
![image](https://user-images.githubusercontent.com/3865131/171061038-bb0a3d74-1b8e-49a9-a0eb-9d2638d55eb2.png)



## Architecture 
The project has 5 Assemblies (C# code libraries)
- Assembly-CSharp
- Tests
- CSharp-Editor
- UniWebView-CSharp
- UniWebView-CSharp.Editor

## Tests
To create Tests in Unity you should use `Test Runner` view.
![image](https://user-images.githubusercontent.com/3865131/169372443-8e43b0ab-4fc3-4737-bfd1-8b26abcd702e.png)

You can choose between:  
- Edit Mode: are the Editor tests only run in the Unity Editor and have access to the Editor code
- Play Mode: are standalone in a Player or inside the Editor.

Check the option `EditMode` and several Test will appear. 

### EditMode Tests

We want to test `QuartersSDK` in `Edit Mode` for that we create a Test assembly in: `Assets/QuartersSDK/Scripts/Editor/`

Inside `Tests` folder you will find 3 files:
- Tests: the assembly that is used for tests. It has all the references to the assemblies you want to test QuartersSDK (that is inside Assembly-CSharp.dll). 
To include that assembly you should paste the `Assembly-CSharp.dll`(you can find it in `quarters-unity-sdk-develop\Library\ScriptAssemblies`) inside `quarters-unity-sdk\Assets\QuartersSDK\Scripts\Editor\Tests`
Once you have done this you will be able to add it as an `Assembly Reference` in Unity inspector.
![image](https://user-images.githubusercontent.com/3865131/169376773-758337df-0989-4487-a51a-a5aad035bc7c.png)

- QuartersSDKTest: here you will be able to write the code of the tests. To write the code of the tests you will need a code editor for example `VS` or `VS Code`. 
