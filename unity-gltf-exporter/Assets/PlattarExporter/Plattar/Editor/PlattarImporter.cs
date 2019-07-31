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
	public class PlattarImporter : EditorWindow {

		static Texture logo;

		[MenuItem("Plattar/GLTF Importer")]
		static void Init() {
			logo = (Texture2D) AssetDatabase.LoadAssetAtPath("Assets/PlattarExporter/Plattar/Editor/ExporterHeader.png", typeof(Texture2D));

			Type inspectorType = Type.GetType("UnityEditor.InspectorWindow,UnityEditor.dll");
			EditorWindow window = EditorWindow.GetWindow<PlattarImporter>(new Type[] {inspectorType});
			window.Show();
		}

		void OnEnable() { }

		void OnGUI() {
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label(logo);
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

			EditorGUILayout.BeginVertical();
			EditorGUILayout.HelpBox("Select a GLTF file to import into the project", MessageType.Info);
			if (GUILayout.Button("Import GLTF")) {

			}
			EditorGUILayout.EndVertical();
		}
	}
}