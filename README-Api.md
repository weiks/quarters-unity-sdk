# API Documentation
## Quarters Initialisation
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