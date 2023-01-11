# Quarters Unity SDK

## Getting Started

### Installation

1. Install `Unity Hub` and `Unity Editor` (version 2021.3.2f1). The version of Unity Editor is very important to be exactly that one.
2. Also install `Microsoft Visual Studio 2022 - Community`. The installer of the Unity Editor will ask you for installing this also. When the `Visual Studio Installer` runs ensure to tick the following extentions that we will need:
   - ASP.NET and web development
   - Node.js develpment
   - .NET Multi-platform APP UI development
   - Game develpment with Unity
3. Open `Visual Studio` once it's been installed and go to **Tools -> Android -> Android SDK Manager**
       ![image1](https://user-images.githubusercontent.com/36847481/211830751-e76e641a-bf89-4828-9f6f-17efa9d98954.jpeg)
       
4. Select `Android SDKs API Level` 29, 30 and 31. Then, click on **"Apply Changes"**.

### Starting the app locally in Unity

1. Clone [quarters-unity-sdk](https://github.com/weiks/quarters-unity-sdk) repository preferably into `C:\desarrollo\weiks` path.
2. Clone [poq-unity-package-manager](https://github.com/weiks/poq-unity-package-manager) into the same path.
3. Open `Unity Hub`, select **_Open_** and search `quarters-unity-sdk` folder.
4. Once the project appear in the list of Unity Hub, click the project to open it. If a warning of **Editor Version** show up, select the version 2021.3.2f1 thay you should previously installed.
5. Open the [SampleScene.unity](https://github.com/weiks/quarters-unity-sdk/blob/master/Assets/Samples/Quarters%20Unity%20SDK/3.0.2021/Sample/Scenes/SampleScene.unity); File -> Open Scene -> Samples -> Quarters Unity SDK -> 3.2.2021 -> Sample -> Scenes -> **SampleScene.unity**
6. Click on **"Play"** and wait for the app to start.

     ![image2](https://user-images.githubusercontent.com/36847481/211831034-56ad172c-d7f9-4879-8d9d-99a6a34fd417.jpeg)


### How to run it in an Android device locally

1. Open `Unity Hub` and select `quarters-unity-sdk` project.
2. Open `Visual Studio 2022`
3. Click on **"Open a project or solution"** in the right menu
4. Select `quarters-unity-sdk.sln` in your project folder (**_"..\quarters-unity-sdk\quarters-unity-sdk.sln"_**)
5. Go to Tools -> Android -> Android Device Manager...
       ![image3](https://user-images.githubusercontent.com/36847481/211831302-1c9d9e48-fd19-42c9-9795-ba1e659dfe5c.jpeg)

6. Start a device. If you do not have any device in the list, create a new one with the default properties but be sure that `Processor` option has the value **x68_64**
7. Go back to Unity Hub and go to `Build Settings`: File -> Build Settings.
8. Select **Android Platform** and then click on `Switch Platform` at the right bottom of the window
9. Click on Player Settings and update the next properties:
   - In **Other Settings** set **Scripting Backend** with `IL2CPP` value
   - Update **Target Architectures** to only have tick the `x68-64 (Chrome OS)` option
   - In **Publishing Settings**, set a **Custom Keystore** browe an existing one that you can find it in `"..\quarters-unity-sdk\Keystore\user.keystore"`. Set `password` as the password value and then select `user` as an alias below and enter the same password again.
   - Be sure that **Custom Main Manifest** is checked on **Build** section and has `Assets\Plugins\Android\AndroidManifest.xml` value
   - Close the Player Settings window
10. Refresh the aviabile devices and select the one that you have created a moment before.
    ![image4](https://user-images.githubusercontent.com/36847481/211831631-8fea3137-1cd9-4da5-bbfa-f736c8516147.jpeg)

11. `Build and Run`, save the APK wherever you want and the app will start on the Emulator Device that you have running.
