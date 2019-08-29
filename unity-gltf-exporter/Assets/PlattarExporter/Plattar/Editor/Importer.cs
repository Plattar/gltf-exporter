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
	public class Importer : EditorWindow {

		static Texture logo;
		static GLTFEditorImporter importer;

		[MenuItem("Plattar/GLTF Importer")]
		static void Init() {
			RefreshLogo();

			Type inspectorType = Type.GetType("UnityEditor.InspectorWindow,UnityEditor.dll");
			EditorWindow window = EditorWindow.GetWindow<Importer>(new Type[] {inspectorType});
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

			EditorGUILayout.EndVertical();
			EditorGUILayout.Separator();

			if (Importer.importer == null) {
				EditorGUILayout.BeginVertical();
				EditorGUILayout.HelpBox("Select a GLTF file to import into the project", MessageType.Info);

				if (GUILayout.Button("Import GLTF")) {
					string gltfPath = SelectGLTF();

					if (gltfPath == null) {
						EditorUtility.DisplayDialog("Import Failed", "GLTF failed to import, file is invalid", "OK");
					}
					else {
						string name = Path.GetFileNameWithoutExtension(gltfPath);
						string importPath = Application.dataPath + "/GLTFImports/" + name;

						Importer.importer = new GLTFEditorImporter((task, start, end) => {
							float progress = start / end;
							EditorUtility.DisplayProgressBar("Importing GLTF", "Importing " + name + " model, please wait...", progress);
						},() => {
							// if this gets called, we are done!
							Importer.importer = null;
							EditorUtility.ClearProgressBar();
						});

						Importer.importer.setupForPath(gltfPath, importPath, name, true);
						Importer.importer.Load();
					}
				}

				EditorGUILayout.EndVertical();
			}
		}

		/**
		 * Prompt the User for selecting a .GLTF file to import
		 */
		private string SelectGLTF() {
			string fullpath = EditorUtility.OpenFilePanel("Select GLTF", PlattarExporterOptions.LastEditorPath, "gltf");

			try {
				PlattarExporterOptions.LastEditorPath = Path.GetDirectoryName(fullpath);
			}
			catch {
				return null;
			}

			return fullpath;
		}

		void Update() {
			if (Importer.importer != null) {
				Importer.importer.Update();
			}
		}
	}
}