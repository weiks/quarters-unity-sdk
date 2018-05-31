using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Xml;

public class AndroidManifestGenerator  {


	static string path = Application.dataPath + "/Plugins/Android/AndroidManifest.xml";

		

	[MenuItem("Quarters/Android/Generate Android Manifest")]
	private static void GenerateManifest() {
		Debug.Log("Generating Quaters Android Manifest at path: " + path);
		SaveManifest(Application.identifier.ToLower(), path);
		UnityEditor.AssetDatabase.Refresh();
	}


	public static void SaveManifest (string _packageName, string _path) {
		SaveManifest(_packageName, "1", "1.0", _path);   
	}
	
	public static void SaveManifest (string _packageName, string _versionCode, string _versionName, string _path)
	{

		// Settings
		XmlWriterSettings _settings 	= new XmlWriterSettings();
		_settings.Encoding 				= new System.Text.UTF8Encoding(true);
		_settings.ConformanceLevel 		= ConformanceLevel.Document;
		_settings.Indent 				= true;
		_settings.NewLineHandling = NewLineHandling.Replace;
		
		// Generate and write manifest
		using (XmlWriter _xmlWriter = XmlWriter.Create(_path, _settings))
		{
			_xmlWriter.WriteStartDocument();
			{
				//********************
				// Manifest
				//********************
				_xmlWriter.WriteComment("AUTO GENERATED MANIFEST FILE FROM AndroidManifestGenerator. DONT MODIFY HERE.");
				
				_xmlWriter.WriteStartElement("manifest");					
				_xmlWriter.WriteAttributeString("xmlns:android", 		"http://schemas.android.com/apk/res/android");
				_xmlWriter.WriteAttributeString("package", 				_packageName);
				_xmlWriter.WriteAttributeString("android:versionCode", 	_versionCode);
                _xmlWriter.WriteAttributeString("android:hardwareAccelerated", 	"true");
				{
					//********************
					// Application
					//********************
					_xmlWriter.WriteStartElement("application");
					{
						WriteApplicationProperties(_xmlWriter);
					}


					//main launcher activity
					_xmlWriter.WriteStartElement("activity");
					{
						_xmlWriter.WriteAttributeString("android:name", "com.unity3d.player.UnityPlayerNativeActivity");
                        _xmlWriter.WriteAttributeString("android:label",  "@string/app_name");


						_xmlWriter.WriteStartElement("meta-data");
						_xmlWriter.WriteAttributeString("android:name", "android.app.lib_name");
						_xmlWriter.WriteAttributeString("android:value", "unity");
						_xmlWriter.WriteEndElement();

						_xmlWriter.WriteStartElement("meta-data");
						_xmlWriter.WriteAttributeString("android:name", "unityplayer.ForwardNativeEventsToDalvik");
						_xmlWriter.WriteAttributeString("android:value", "true");
						_xmlWriter.WriteEndElement();

						//intent filter
						_xmlWriter.WriteStartElement("intent-filter");
						{

							_xmlWriter.WriteStartElement("action");
							_xmlWriter.WriteAttributeString("android:name", "android.intent.action.MAIN");
							_xmlWriter.WriteEndElement();

							_xmlWriter.WriteStartElement("category");
							_xmlWriter.WriteAttributeString("android:name", "android.intent.category.LAUNCHER");
							_xmlWriter.WriteEndElement();


						}
						_xmlWriter.WriteEndElement();



					}
					_xmlWriter.WriteEndElement();
					


						//Quarters deep linking activity
						_xmlWriter.WriteStartElement("activity");
						{
							_xmlWriter.WriteAttributeString("android:name", "org.westhill.customurlschemelauncher.CustomUrlSchemeLauncherActivity");

								_xmlWriter.WriteStartElement("intent-filter");
								{


								_xmlWriter.WriteStartElement("data");
								_xmlWriter.WriteAttributeString("android:scheme", Application.identifier.ToLower());
								_xmlWriter.WriteEndElement();

								_xmlWriter.WriteStartElement("category");
								_xmlWriter.WriteAttributeString("android:name", "android.intent.category.DEFAULT");
								_xmlWriter.WriteEndElement();

								_xmlWriter.WriteStartElement("category");
								_xmlWriter.WriteAttributeString("android:name", "android.intent.category.BROWSABLE");
								_xmlWriter.WriteEndElement();

								_xmlWriter.WriteStartElement("action");
								_xmlWriter.WriteAttributeString("android:name", "android.intent.action.VIEW");
								_xmlWriter.WriteEndElement();



								}

								_xmlWriter.WriteEndElement();
				

						}
						_xmlWriter.WriteEndElement();



					_xmlWriter.WriteEndElement();
				}

                //permissions
                _xmlWriter.WriteStartElement("uses-permission");
                {
                    _xmlWriter.WriteAttributeString("android:name", "android.permission.INTERNET");

                }
                _xmlWriter.WriteEndElement();





				_xmlWriter.WriteEndElement();
			}


			_xmlWriter.WriteEndDocument();
		}

	}








	
	protected static void WriteApplicationProperties (XmlWriter _xmlWriter)
	{}
	
	protected static void WritePermissions (XmlWriter _xmlWriter)
	{}
	
	protected static void WriteActivity (XmlWriter _xmlWriter, string _name, string _theme = null, string _orientation = null, string _configChanges = null, string _exported = null, string _comment = null)
	{
		if (_comment != null)
			_xmlWriter.WriteComment(_comment);
		
		_xmlWriter.WriteStartElement("activity");
		{
			_xmlWriter.WriteAttributeString("android:name", 					_name);
			
			if (_theme != null)
				_xmlWriter.WriteAttributeString("android:theme", 				_theme);
			
			if (_orientation != null)
				_xmlWriter.WriteAttributeString("android:screenOrientation", 	_orientation);
			
			if (_configChanges != null)
				_xmlWriter.WriteAttributeString("android:configChanges", 		_configChanges);

			if (_exported != null)
				_xmlWriter.WriteAttributeString("android:exported", 			_exported);

		}
		_xmlWriter.WriteEndElement();
	}
	
	protected static void WriteAction (XmlWriter _xmlWriter, string _name, string _permission = null, string _comment = null)
	{
		if (_comment != null)
			_xmlWriter.WriteComment(_comment);
		
		_xmlWriter.WriteStartElement("action");
		{
			_xmlWriter.WriteAttributeString("android:name", 	_name);
			
			if (_permission != null)
				_xmlWriter.WriteAttributeString("android:permission", _permission);
		}
		_xmlWriter.WriteEndElement();
	}
	
	protected static void WriteCategory (XmlWriter _xmlWriter, string _name, string _comment = null)
	{
		if (_comment != null)
			_xmlWriter.WriteComment(_comment);
		
		_xmlWriter.WriteStartElement("category");
		{
			_xmlWriter.WriteAttributeString("android:name", 	_name);
		}
		_xmlWriter.WriteEndElement();
	}
	
	protected static void WriteService (XmlWriter _xmlWriter, string _name, string _comment = null)
	{
		if (_comment != null)
			_xmlWriter.WriteComment(_comment);
		
		_xmlWriter.WriteStartElement("service");
		{
			_xmlWriter.WriteAttributeString("android:name", 	_name);
		}
		_xmlWriter.WriteEndElement();
	}
	
    protected static void WritePermission (XmlWriter _xmlWriter, string _name, string _protectionLevel = null, string _comment = null)
	{
		if (_comment != null)
			_xmlWriter.WriteComment(_comment);
		
		_xmlWriter.WriteStartElement("permission");
		{
			_xmlWriter.WriteAttributeString("android:name", 			_name);
            if (_protectionLevel != null ) {
			    _xmlWriter.WriteAttributeString("android:protectionLevel", 	_protectionLevel);
            }
		}
		_xmlWriter.WriteEndElement();
	}
	


}
