using UnityEngine;

public static class CustomUrlSchemeAndroid
{
    #region Custom url scheme launcher for Android

    private const string KEY_URL = "UrlStr";
    private const string KEY_SCHEME = "UrlScheme";
    private const string KEY_HOST = "UrlHost";
    private const string KEY_PATH = "UrlPath";
    private const string KEY_QUERY = "UrlQuery";

    private static string GetPlayerPrefString(string key, bool clearDataAfterGet)
    {
        string val = null;

        if (PlayerPrefs.HasKey(key))
        {
            val = PlayerPrefs.GetString(key);

            if (clearDataAfterGet)
            {
                PlayerPrefs.DeleteKey(key);
            }
        }
        return val;
    }

    /// <summary>
    /// Get the URL full string. or null.
    /// </summary>
    public static string GetLaunchedUrl(bool clearDataAfterGet = true)
    {
        return GetPlayerPrefString(KEY_URL, clearDataAfterGet);
    }

    /// <summary>
    /// Get the URL scheme string. or null.
    /// </summary>
    public static string GetLaunchedUrlScheme(bool clearDataAfterGet = true)
    {
        return GetPlayerPrefString(KEY_SCHEME, clearDataAfterGet);
    }

    /// <summary>
    /// Get the URL host string. or null.
    /// </summary>
    public static string GetLaunchedUrlHost(bool clearDataAfterGet = true)
    {
        return GetPlayerPrefString(KEY_HOST, clearDataAfterGet);
    }

    /// <summary>
    /// Get the URL path string. or null.
    /// </summary>
    public static string GetLaunchedUrlPath(bool clearDataAfterGet = true)
    {
        return GetPlayerPrefString(KEY_PATH, clearDataAfterGet);
    }

    /// <summary>
    /// Get the URL query string. or null.
    /// </summary>
    public static string GetLaunchedUrlQuery(bool clearDataAfterGet = true)
    {
        return GetPlayerPrefString(KEY_QUERY, clearDataAfterGet);
    }

    /// <summary>
    /// Clear saved url schemes data.
    /// </summary>
    public static void ClearSavedData()
    {
        PlayerPrefs.DeleteKey(KEY_URL);
        PlayerPrefs.DeleteKey(KEY_SCHEME);
        PlayerPrefs.DeleteKey(KEY_HOST);
        PlayerPrefs.DeleteKey(KEY_PATH);
        PlayerPrefs.DeleteKey(KEY_QUERY);
    }

    #endregion

    #region Package installed checker for Android

#if UNITY_ANDROID

    /// <summary>
    /// It's check the package is installed on Android device.
    /// Please use this as " CustomUrlSchemeAndroid.IsPackageInstalled("your.package.name"); "
    /// </summary>
    /// <returns>
    /// <c>true</c> App packageName is installed in the device.; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsPackageInstalled(string packageName)
    {
        bool result = false;

        // attach thread
        AndroidJNI.AttachCurrentThread();
        AndroidJNI.PushLocalFrame(0);
        try
        {
            // getting UnityPlayerActivity
            using (AndroidJavaClass jcUnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject joCurrentActivity = jcUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            // getting PackageManager
            using (AndroidJavaObject joPackageManager = joCurrentActivity.Call<AndroidJavaObject>("getPackageManager"))
            // getting installed application list
            using (AndroidJavaObject joApplicationInfoList = joPackageManager.Call<AndroidJavaObject>("getInstalledApplications", 0))
            {
                // getting list size
                int listSize = joApplicationInfoList.Call<int>("size");
                // searching package
                for (int i = 0; i < listSize; i++)
                {
                    // getting package name
                    string pName = GetPackageName(joApplicationInfoList, i);
                    //Debug.Log( i + ":" + pName );
                    // comparing package name
                    if (!string.IsNullOrEmpty(pName) && packageName.Equals(pName))
                    {
                        // Installed package name
                        result = true;
                        break;
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
        }
        finally
        {
            AndroidJNI.PopLocalFrame(System.IntPtr.Zero);
        }
        return result;
    }

    /// <summary>
    /// Getting package name from application info list
    /// </summary>
    /// <returns>
    /// The package name.
    /// </returns>
    private static string GetPackageName(AndroidJavaObject joApplicationInfoList, int index)
    {
        string pName = "";

        // attach thread
        AndroidJNI.AttachCurrentThread();
        AndroidJNI.PushLocalFrame(0);
        try
        {
            // getting application info
            using (AndroidJavaObject joApplicationInfo = joApplicationInfoList.Call<AndroidJavaObject>("get", index))
            {
                // getting package name
                pName = joApplicationInfo.Get<string>("packageName");
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
            pName = string.Empty;
        }
        finally
        {
            AndroidJNI.PopLocalFrame(System.IntPtr.Zero);
        }
        return pName;
    }

#endif

    #endregion
}
