using System;
using UnityEngine;

namespace UnityGLTF.Extensions {

	public static class GLTFAnimationExtensions {
		public static Vector3 SwitchHandedness(this Vector3 input) {
			return new Vector3(input.x, input.y, -input.z);
		}

		public static Vector4 SwitchHandedness(this Vector4 input) {
			return new Vector4(input.x, input.y, -input.z, -input.w);
		}
	}
}