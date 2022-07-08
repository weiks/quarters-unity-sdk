# QuartersSDK .net library
The project unifies in a simple way the requests to `Quarters API` all the logic and concerns to use the API will be here.

## Prerequirements

* .net standard 2.1
* .net Core 3.1

## Structure
The main structure has 2 folders:
* `QuartersSDK`: contains `QuartersSDK` the development project 
* `QuartersSDKTest`: contains the test project for the development

### QuartersSDK folder
Here you will find the main development. It's a `.net standard 2.1` library (the main reason to use this framework is Unity in a future we will be able just to use .NET6).
Having all the concerns to `Quarters API` here will give us the possibility to add new features (every new feature must have a test case in test project) and gather
all the concerns to the services here.
The development has 3 main folders:
* `Services`: business logic layer
* `Data`: entities and data layer
* `Interfaces`: layouts for what a class is going to implement (in a future we will use them to easily interchange one component for another)

### QuartersSDKTes folder
Here you will find the unit tests. It's a `.net Core 3.1` test project that uses `NUnit` (because it's easy to aim it with Unity).
You will find 2 main folders:
* `Services`: business logic layer
* `Data`: entities and data layer

## Setup & Configure
You will find in both folders `appsettings.json` file where you will be able to configure all the parameters that need the SDK to run.
Also you can create an instance of QuartersSDK sending the configuration parameters without `appsettings.json` file this constructor is used for Unity ( to be able to run it in mobile devices).

## Deploy 
After you build `QuartersSDK` project it will generate `QuartersSDK.dll` file inside `bin/Debug/netstandard2.1` (Debug can be Release folder if you selected the option of release build).
This project it's a library and you will be able to include it to Unity or other project as a dependency (including the development project to you solution).
In case of Unity you just need to copy the `QuartersSDK.dll` file (generated after build inside bin folder) and paste it in `quarters-unity-sdk-weiks\Assets\Resources\QuartersSDK`.
