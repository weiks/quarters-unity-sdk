using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISafeAreaElement
{
    void SafeAreaChanged(UISafeArea safeArea, RectTransform canvasRectTransform);
}
