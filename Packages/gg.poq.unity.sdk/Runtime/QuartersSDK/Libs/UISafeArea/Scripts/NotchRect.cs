using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotchRect : MonoBehaviour, ISafeAreaElement {

	public void SafeAreaChanged(UISafeArea safeArea, RectTransform canvasRectTransform)
	{
		StopAllCoroutines();
		StartCoroutine(Adjust(safeArea, canvasRectTransform));
	}

	
	private IEnumerator Adjust(UISafeArea safeArea, RectTransform canvasRectTransform)
	{
		//workaround unity issue when rotation is reported before the safe area adjust
		yield return new WaitForEndOfFrame();
		RectTransform rectTransform = (RectTransform) this.transform;

	
		if (Screen.orientation == ScreenOrientation.LandscapeRight)
		{
			//notch on right
			rectTransform.offsetMin = new Vector2(canvasRectTransform.rect.width + safeArea.rightMarginRectSize, 0);
			rectTransform.offsetMax = Vector2.zero;
		}
		else if (Screen.orientation == ScreenOrientation.LandscapeLeft)
		{
			//notch on left
			rectTransform.offsetMin = Vector2.zero;
			rectTransform.offsetMax = new Vector2(-canvasRectTransform.rect.width + safeArea.leftMarginRectSize, 0);
		}
		else if (Screen.orientation == ScreenOrientation.Portrait)
		{
			rectTransform.offsetMin = new Vector2(0, safeArea.RectTransform.rect.height + safeArea.bottomMarginRectSize);
			rectTransform.offsetMax = new Vector2(safeArea.rightMarginRectSize, 0);
		}
		else if (Screen.orientation == ScreenOrientation.PortraitUpsideDown)
		{
			rectTransform.offsetMin = new Vector2(0, 0);
			rectTransform.offsetMax = new Vector2(0, -canvasRectTransform.rect.height + safeArea.bottomMarginRectSize);
		}
	}
}
