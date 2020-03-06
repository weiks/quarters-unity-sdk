using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace CoinforgeSDK.UI {


	public class UIList : MonoBehaviour {

		public GameObject cellPrototype;
		[HideInInspector] public List<GameObject> cells = new List<GameObject>();


		public virtual void Awake() {
			Clear();
		}


		public void Populate(ArrayList info, bool extendList = false) {

			if (extendList == false) {
				Clear();
			}

			for (int i = 0; i < info.Count; i++) {
				GameObject cellCopy = Instantiate(cellPrototype) as GameObject;
				cells.Add(cellCopy);
				cellCopy.transform.SetParent(this.transform);
				cellCopy.transform.localScale = Vector3.one;
				cellCopy.gameObject.SetActive(true);

				IListCell cell = cellCopy.GetComponent(typeof(IListCell)) as IListCell;
				cell.Init(info[i]);
			}
		}


		public void Clear() {
			foreach (GameObject cell in cells) Destroy(cell);
			cells = new List<GameObject>();
			cellPrototype.gameObject.SetActive(false);
		}




	}
}