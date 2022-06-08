using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISafeArea : MonoBehaviour
{
	public delegate void OnSafeAreaChangedDelegate();

	public event OnSafeAreaChangedDelegate OnSafeAreaChanged;
	private ScreenOrientation currentOrientation = ScreenOrientation.Unknown;


	private CanvasScaler CanvasScaler
	{
		get
		{
			CanvasScaler canvasScaler = this.transform.parent.parent.GetComponent<CanvasScaler>();

			if (canvasScaler == null)
			{
				Debug.LogError(new Exception("UI Safe area must be a child of the CanvasScaler"));
				return null;
			}
			else
			{
				return canvasScaler;
			}
		}

	}

	private Rect currentSafeArea;

	[SerializeField]
	private bool ignoreTopMargin = false;
	public bool IgnoreTopMargin
	{
		get
		{
			return ignoreTopMargin;
		}
		set
		{
			if (ignoreTopMargin != value)
			{
				ignoreTopMargin = value;
				UpdateSafeArea();
			}
		}
	}
	
	[SerializeField]
	private bool ignoreLeftMargin = true;
	public bool IgnoreLeftMargin
	{
		get
		{
			return ignoreLeftMargin;
		}
		set
		{
			if (ignoreLeftMargin != value)
			{
				ignoreLeftMargin = value;
				UpdateSafeArea();
			}
		}
	}
	
	[SerializeField]
	private bool ignoreRightMargin = true;
	public bool IgnoreRightMargin
	{
		get
		{
			return ignoreRightMargin;
		}
		set
		{
			if (ignoreRightMargin != value)
			{
				ignoreRightMargin = value;
				UpdateSafeArea();
			}
		}
	}
	
	[SerializeField]
	private bool ignoreBottomMargin = true;
	public bool IgnoreBottomMargin
	{
		get
		{
			return ignoreBottomMargin;
		}
		set
		{
			if (ignoreBottomMargin != value)
			{
				ignoreBottomMargin = value;
				UpdateSafeArea();
			}
		}
	}
	
	[SerializeField]
	private bool ignoreOpositeNotchMargin = false;
	public bool IgnoreOpositeNotchMargin
	{
		get
		{
			return ignoreOpositeNotchMargin;
		}
		set
		{
			if (ignoreOpositeNotchMargin != value)
			{
				ignoreOpositeNotchMargin = value;
				UpdateSafeArea();
			}
		}
	}
	


	[NonSerialized]
	private bool simulateiPhoneX = false;
	public bool SimulateiPhoneX
	{
		get {
//			return true;
			return simulateiPhoneX && Application.isEditor;
		}
		set
		{
			if (simulateiPhoneX != null)
			{
				simulateiPhoneX = value;
				UpdateSafeArea();
			}
		}
	}

	public RectTransform RectTransform
	{
		get
		{
			return (RectTransform) this.transform;
		}	
	}


	private float ScaleRatio
	{
		get
		{
			return (float)ScreenSize.x / (float)CanvasScaler.referenceResolution.x;
		}
	}

	//forcing screen size is required as Unity Editor does not report screen size correctly when play mode is disabled
	private Vector2 ScreenSize
	{
		get
		{
			if (SimulateiPhoneX)
			{
				RectTransform canvasRect = (RectTransform) CanvasScaler.transform;
				if (canvasRect.rect.width > canvasRect.rect.height)
				{
					return new Vector2(2436f, 1125f);
				}
				else
				{
					return new Vector2(1125f, 2436f);
				}
			}
			else
			{
				return new Vector2((float)Screen.width, (float)Screen.height);
			} 
		}
	}

	private Rect SimulatedSafeArea
	{
		get
		{
			if (ScreenSize.x > ScreenSize.y)
			{
				return new Rect(132f, 63f, 2172f, 1062f);
			}
			else
			{
				return new Rect(0, 102f, 1125f, 2202f);	
			}
		}
	}
	
	public float leftMarginRectSize = 0;
	public float rightMarginRectSize = 0;
	public float topMarginRectSize = 0;
	public float bottomMarginRectSize = 0;

	public List<ISafeAreaElement> elements = new List<ISafeAreaElement>();
	

	public void LoadSafeAreaElements()
	{
		elements = new List<ISafeAreaElement>(CanvasScaler.transform.GetComponentsInChildren<ISafeAreaElement>(includeInactive: true));
	}



	private void Update()
	{
		Rect pixelSafeArea = SimulateiPhoneX ? SimulatedSafeArea : Screen.safeArea;
		if (currentSafeArea != pixelSafeArea || currentOrientation != Screen.orientation)
		{
			currentOrientation = Screen.orientation;
			UpdateSafeArea();
		}
		
	}

	public void UpdateSafeArea()
	{
		Rect pixelSafeArea = SimulateiPhoneX ? SimulatedSafeArea : Screen.safeArea;
		currentSafeArea = pixelSafeArea;

		//margin in pixels
		float leftMargin = pixelSafeArea.x;
		float rightMargin = -(ScreenSize.x - pixelSafeArea.width - pixelSafeArea.x);
		float topMargin = -pixelSafeArea.y;
		float bottomMargin = ScreenSize.y  - pixelSafeArea.height - pixelSafeArea.y;

		//margins in rect space
		leftMarginRectSize = leftMargin / ScaleRatio;
		rightMarginRectSize = rightMargin / ScaleRatio;
		topMarginRectSize = topMargin / ScaleRatio;
		bottomMarginRectSize = bottomMargin / ScaleRatio;
		
		if (ignoreTopMargin) topMarginRectSize = 0;
		if (ignoreLeftMargin) leftMarginRectSize = 0;
		if (ignoreBottomMargin) bottomMarginRectSize = 0;
		if (ignoreRightMargin) rightMarginRectSize = 0;


		if (ignoreOpositeNotchMargin)
		{
			if (Screen.orientation == ScreenOrientation.LandscapeRight)
			{
				leftMarginRectSize = 0;
			}
			else if (Screen.orientation == ScreenOrientation.LandscapeLeft)
			{
				rightMarginRectSize = 0;
			}
			else if (Screen.orientation == ScreenOrientation.Portrait)
			{
				bottomMarginRectSize = 0;
			}
			else if (Screen.orientation == ScreenOrientation.PortraitUpsideDown)
			{
				topMarginRectSize = 0;
			}
		}

		

		RectTransform.offsetMin = new Vector2(leftMarginRectSize, bottomMarginRectSize);
		RectTransform.offsetMax = new Vector2(rightMarginRectSize, topMarginRectSize);
		
		if (OnSafeAreaChanged != null) OnSafeAreaChanged();
		
		LoadSafeAreaElements();
		foreach (ISafeAreaElement element in elements)
		{
			element.SafeAreaChanged(this, (RectTransform) CanvasScaler.transform);
		}
	}

}
