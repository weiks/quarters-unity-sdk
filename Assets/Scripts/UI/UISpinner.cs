using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISpinner : MonoBehaviour
{

    void Update() {
        this.transform.Rotate(new Vector3(0, 0, -Time.deltaTime * 500f));
    }
    
}
