using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityGLTF;

namespace Plattar {
	public class Exporter : EditorWindow {

		GameObject selectedObject;
		static Texture logo;
		static readonly List<int> pivotCheck = new List<int>();

		[MenuItem("Plattar/GLTF Exporter")]
		static void Init() {
			RefreshLogo();

			Type inspectorType = Type.GetType("UnityEditor.InspectorWindow,UnityEditor.dll");
			EditorWindow window = EditorWindow.GetWindow<Exporter>(new Type[] {inspectorType});
			window.Show();
		}

		static void RefreshLogo() {
			if (logo == null) {
				logo = (Texture2D) AssetDatabase.LoadAssetAtPath("Assets/PlattarExporter/Plattar/Editor/ExporterHeader.png", typeof(Texture2D));
			}
		}

		void OnEnable() { }

		void OnGUI() {
			RefreshLogo();
			
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label(logo, GUILayout.Width(150), GUILayout.Height(150));
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			EditorGUILayout.BeginVertical();

			var foundGrids = GameObject.FindObjectsOfType<AlignmentScript>();

			if (foundGrids != null && foundGrids.Length > 0) {
				if (GUILayout.Button("Hide Alignment Grid")) {
					for (int i = 0; i < foundGrids.Length; i++) {
						if (foundGrids[i] != null && foundGrids[i].gameObject != null) {
							GameObject.DestroyImmediate(foundGrids[i].gameObject);
						}
					}
				}
			} else {
				if (GUILayout.Button("Show Alignment Grid")) {
					var grid = (GameObject) AssetDatabase.LoadAssetAtPath("Assets/PlattarExporter/Plattar/Alignment/AlignmentPlane.prefab", typeof(GameObject));
					var obj = GameObject.Instantiate(grid);

					obj.name = "Plattar Alignment Grid";
				}
			}

			if (GUILayout.Button("Reset Exporter Defaults")) {
				GLTFTextureUtils.textureOption = GLTFTextureUtils.TextureExportOption.None;
				GLTFTextureUtils.JPGQuality = 75;
				GLTFUtils.boundsExportOption = GLTFUtils.BoundsExportOption.Default;
				PlattarExporterOptions.ExportAnimations = true;
			}

			EditorGUILayout.EndVertical();
			EditorGUILayout.Separator();

			EditorGUILayout.BeginVertical();
			selectedObject = (GameObject) EditorGUILayout.ObjectField("Export Object", selectedObject, typeof(GameObject), true);
			EditorGUILayout.EndVertical();

			if (selectedObject == null) {
				EditorGUILayout.BeginVertical();
				EditorGUILayout.HelpBox("You need to select a GameObject to continue", MessageType.Error);
				EditorGUILayout.EndVertical();
			} else {
				string selectionName = selectedObject.name;

				// ensure the name of our object is valid
				if (string.IsNullOrEmpty(selectionName) || Regex.Matches(selectionName, @"[a-zA-Z]").Count <= 0) {
					EditorGUILayout.BeginVertical();
					EditorGUILayout.HelpBox("Your Selected GameObject does not have a valid name", MessageType.Error);
					EditorGUILayout.EndVertical();

					return;
				}

				var checkScripts = selectedObject.GetComponentsInChildren<AlignmentScript>();

				if (checkScripts != null && checkScripts.Length > 0) {
					EditorGUILayout.BeginVertical();
					EditorGUILayout.HelpBox("Your selected GameObject or its children contains an Alignment Grid", MessageType.Error);
					EditorGUILayout.EndVertical();

					return;
				}

				var meshes = selectedObject.GetComponentsInChildren<MeshFilter>();
				var skinnedMeshes = selectedObject.GetComponentsInChildren<SkinnedMeshRenderer>();

				if ((meshes == null || meshes.Length <= 0) && (skinnedMeshes == null || skinnedMeshes.Length <= 0)) {
					EditorGUILayout.BeginVertical();
					EditorGUILayout.HelpBox("Your selected GameObject or its children have no Geometry", MessageType.Error);
					EditorGUILayout.EndVertical();

					return;
				}

				EditorGUILayout.Separator();

				ShowAnimationModifiersUI();
				EditorGUILayout.Separator();

				ShowMeshModifiersUI(selectedObject);
				EditorGUILayout.Separator();

				ShowTextureModifiersUI();
				EditorGUILayout.Separator();
				
				EditorGUILayout.BeginVertical();

				if (GUILayout.Button($"Export {selectionName} to GLTF")) {
					if (GenerateGLTFZipped(selectedObject) != null) {
						EditorUtility.DisplayDialog("Successful Export", "GLTF Exported and Zipped Successfully", "OK");
					}
				}

				EditorGUILayout.EndVertical();
			}
		}

		private static void ShowMeshModifiersUI(GameObject selectedObject) {
			EditorGUILayout.BeginVertical("HelpBox");

			EditorGUILayout.LabelField("Mesh Options");

			bool hasAnim = SelectionHasAnimation(selectedObject);

			if (!hasAnim) {
				EditorGUILayout.HelpBox("Use to fix google scene-viewer hovering issues", MessageType.Info);
			}
			else {
				EditorGUILayout.HelpBox("Pivot realignment is unavailable for animated objects", MessageType.Error);
			}

			EditorGUI.BeginDisabledGroup(hasAnim);

			if (GUILayout.Button("Realign Pivot Center")) {
				if (EditorUtility.DisplayDialog("Re-Align Mesh Pivots?", "Are you sure you want to re-align the pivot center? This operation will modify mesh data and cannot be reversed, continue?", "Yes", "Cancel")) {
					CenterMesh(selectedObject);
				}
			}

			EditorGUI.EndDisabledGroup();

			EditorGUI.BeginDisabledGroup(!pivotCheck.Contains(selectedObject.GetInstanceID()));

			// only support center-pivot objects for now
			if (GUILayout.Button($"Pin to Alignment Grid")) {
				PinToGrid(selectedObject);
			}

			EditorGUI.EndDisabledGroup();

			GLTFUtils.boundsExportOption = (GLTFUtils.BoundsExportOption)EditorGUILayout.EnumPopup("Bounding Box Calculation", GLTFUtils.boundsExportOption);

			EditorGUILayout.EndVertical();
		}

		private static void ShowTextureModifiersUI() {
			EditorGUILayout.BeginVertical("HelpBox");

			EditorGUILayout.LabelField("Texture Options");

			GLTFTextureUtils.textureOption = (GLTFTextureUtils.TextureExportOption)EditorGUILayout.EnumPopup("Force Texture Conversion", GLTFTextureUtils.textureOption);

			EditorGUI.BeginDisabledGroup(GLTFTextureUtils.textureOption == GLTFTextureUtils.TextureExportOption.PNG);

			GLTFTextureUtils.JPGQuality = EditorGUILayout.IntSlider("JPG Texture Quality", GLTFTextureUtils.JPGQuality, 0, 100);

			EditorGUI.EndDisabledGroup();

			GLTFEditorExporter.materialExportOption = (GLTFEditorExporter.MaterialDoubleSidedExport)EditorGUILayout.EnumPopup("Material Side", GLTFEditorExporter.materialExportOption);

			EditorGUILayout.EndVertical();
		}

		private static void ShowAnimationModifiersUI() {
			EditorGUILayout.BeginVertical("HelpBox");

			EditorGUILayout.LabelField("Animation Options");

			PlattarExporterOptions.ExportAnimations = EditorGUILayout.Toggle("Export Animations", PlattarExporterOptions.ExportAnimations);

			EditorGUILayout.EndVertical();
		}

		/**
		 * Generate the GLTF file and zip it all up
		 */
		public static Tuple<string, string, string> GenerateGLTFZipped(GameObject selectedObject) {
			var path = GenerateGLTF(selectedObject);

			if (path == null) {
				return path;
			}

			// otherwise, we need to zip up the entire directory
			// and delete the original
			PlattarExporterOptions.CompressDirectory(path.Item2, path.Item1 + "/" + path.Item3);
			PlattarExporterOptions.DeleteDirectory(path.Item2);

			return path;
		}

		/**
		 * Generate a non-zipped GLTF file with all folders etc
		 */
		public static Tuple<string, string, string> GenerateGLTF(GameObject selectedObject) {
			string selectionName = selectedObject.name;

			string fullpath = EditorUtility.SaveFilePanel("glTF Export Path", PlattarExporterOptions.LastEditorPath, selectionName, "zip");
			PlattarExporterOptions.LastEditorPath = Path.GetDirectoryName(fullpath);

			string selectedName = Path.GetFileNameWithoutExtension(fullpath);

			var path = PlattarExporterOptions.LastEditorPath;
			if (!string.IsNullOrEmpty(path)) {
				var newpath = path + "/" + selectionName + "_export_gltf";
				DirectoryInfo info = Directory.CreateDirectory(newpath);

				if (info.Exists) {
					if (PlattarExporterOptions.ExportAnimations == true) {
						var exporter = new GLTFEditorExporter(new Transform[] { selectedObject.transform });
						exporter.SaveGLTFandBin(newpath, selectionName);
					} 
					else {
						var exporter = new GLTFSceneExporter(new Transform[] { selectedObject.transform });
						exporter.SaveGLTFandBin(newpath, selectionName);
					}

					return new Tuple<string, string, string>(path, newpath, selectedName);
				}

				EditorUtility.DisplayDialog("Failed Export", "GLTF Could not be exported, could not create export path", "OK");
			} else {
				EditorGUILayout.HelpBox("Failed to export since the path is invalid", MessageType.Error);
				EditorUtility.DisplayDialog("Failed Export", "GLTF Could not be exported, invalid export path provided", "OK");
			}

			return null;
		}

		public static void PinToGrid(GameObject root) {
			MeshFilter[] filters = root.GetComponentsInChildren<MeshFilter>();

			Bounds totalBounds = new Bounds();
			
			int count = filters.Length;

			// find the center pivot of ALL meshes
			for (int i = 0; i < count; i++) {
				if (filters[i] != null) {
					Mesh mesh = filters[i].sharedMesh;

					if (mesh != null) {
						Vector3[] positions = mesh.vertices;
						int pCount = positions.Length;

						for (int j = 0; j < pCount; j++) {
							// ensure we are using the world position of the Transform
							totalBounds.Encapsulate(filters[i].gameObject.transform.TransformPoint(positions[j]));
						}
					}
				}
			}

			Vector3 position = root.gameObject.transform.position;
			
			if (totalBounds.min.y < 0.0f) {
				position.y = Math.Abs(totalBounds.min.y - position.y);
			}
			else {
				position.y = totalBounds.max.y - position.y;
			}

			root.gameObject.transform.position = position;
		}

		private static bool SelectionHasAnimation(GameObject selectedObject) {
			if (selectedObject == null) {
				return false;
			}

			Animation[] filters = selectedObject.GetComponentsInChildren<Animation>();

			if (filters == null || filters.Length == 0) {
				return false;
			}

			return true;
		}

		/**
		 * O(2n) operation
		 */
		public static void CenterMesh(GameObject root) {
			MeshFilter[] filters = root.GetComponentsInChildren<MeshFilter>();

			Bounds totalBounds = new Bounds();
			
			int count = filters.Length;

			// find the center pivot of ALL meshes
			for (int i = 0; i < count; i++) {
				if (filters[i] != null) {
					Mesh mesh = filters[i].sharedMesh;

					if (mesh != null) {
						Vector3[] positions = mesh.vertices;
						int pCount = positions.Length;

						for (int j = 0; j < pCount; j++) {
							totalBounds.Encapsulate(positions[j]);
						}
					}
				}
			}

			float pivotX = (totalBounds.min.x + totalBounds.max.x) / 2.0f;
			float pivotY = (totalBounds.min.y + totalBounds.max.y) / 2.0f;
			float pivotZ = (totalBounds.min.z + totalBounds.max.z) / 2.0f;

			Vector3 center = new Vector3(pivotX, pivotY, pivotZ);

			// we calculate the min and max, we require this data to center
			// everything properly

			// now we need to displace all vertices, so loop again
			for (int i = 0; i < count; i++) {
				if (filters[i] != null) {
					Mesh mesh = Mesh.Instantiate(filters[i].sharedMesh);

					if (mesh != null) {
						Vector3[] positions = mesh.vertices;
						int pCount = positions.Length;

						for (int j = 0; j < pCount; j++) {
							positions[j] = positions[j] - center;
						}

						mesh.vertices = positions;
					}

					filters[i].sharedMesh = mesh;
				}
			}

			pivotCheck.Add(root.GetInstanceID());
		}
	}
}