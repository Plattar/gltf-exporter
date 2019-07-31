using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Plattar {
#if UNITY_EDITOR
	[ExecuteInEditMode]
	public class AlignmentScript : MonoBehaviour {
		void Awake() {
			gameObject.SetActive(true);
		}

		void Update() {
			gameObject.transform.position = new Vector3(0, 0, -4.15f);
			gameObject.transform.localEulerAngles = new Vector3(0, 180.0f, 0);
		}
	}
#else
	public class AlignmentScript : MonoBehaviour {
		void Awake() {
			gameObject.SetActive(false);
		}
	}
#endif
}