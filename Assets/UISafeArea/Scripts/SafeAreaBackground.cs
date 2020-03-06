using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SafeAreaBackground : MonoBehaviour, ISafeAreaElement {

	public void SafeAreaChanged(UISafeArea safeArea, RectTransform canvasRectTransform)
	{
		RectTransform rectTransform = (RectTransform) this.transform;
		
		rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, safeArea.topMarginRectSize, canvasRectTransform.rect.height);
		rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, safeArea.rightMarginRectSize, canvasRectTransform.rect.width);
		rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, -safeArea.leftMarginRectSize, canvasRectTransform.rect.width);
		rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, -safeArea.bottomMarginRectSize, canvasRectTransform.rect.height);
	}
}
