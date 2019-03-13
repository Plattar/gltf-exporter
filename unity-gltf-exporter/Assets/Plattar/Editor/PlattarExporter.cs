using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityGLTF;
using UnityEngine.SceneManagement;

namespace Plattar {
	public class PlattarExporter : EditorWindow {
		
		AnimBool showSettings;
		GameObject selectedObject;
		
		[MenuItem("Plattar/Exporter")]
		static void Init() {
			PlattarExporter window = (PlattarExporter)EditorWindow.GetWindow(typeof(PlattarExporter), false, "Plattar Exporter");
			window.Show();
		}
		
		void OnEnable() {
			showSettings = new AnimBool(false);
			showSettings.valueChanged.AddListener(Repaint);
		}
		
		void OnGUI() {
			showSettings.target = EditorGUILayout.ToggleLeft("Exporter Options", showSettings.target);
			
			//Extra block that can be toggled on and off.
			if (EditorGUILayout.BeginFadeGroup(showSettings.faded)) {
				EditorGUI.indentLevel++;
				
				EditorGUILayout.BeginVertical();
				GLTFSceneExporter.ExportFullPath = EditorGUILayout.Toggle("Export using original path", GLTFSceneExporter.ExportFullPath);
				GLTFSceneExporter.ExportNames = EditorGUILayout.Toggle("Export names of nodes", GLTFSceneExporter.ExportNames);
				GLTFSceneExporter.RequireExtensions= EditorGUILayout.Toggle("Require extensions", GLTFSceneExporter.RequireExtensions);
				EditorGUILayout.EndVertical();
				EditorGUI.indentLevel--;
			}

			EditorGUILayout.EndFadeGroup();
			EditorGUILayout.Separator();
			
			EditorGUILayout.BeginVertical();
			EditorGUILayout.HelpBox("Select a GameObject from the scene to export into GLTF", MessageType.Info);
			selectedObject = (GameObject) EditorGUILayout.ObjectField("Export Object", selectedObject, typeof(GameObject), true);
			EditorGUILayout.EndVertical();
			
			if (selectedObject == null) {
				EditorGUILayout.BeginVertical();
				EditorGUILayout.HelpBox("You need to select an object to continue", MessageType.Error);
				EditorGUILayout.EndVertical();
			}
			else {
				EditorGUILayout.Separator();
				EditorGUILayout.BeginVertical();
				string selectionName = selectedObject.name;
				
				if (GUILayout.Button("Export " + selectionName + " to GLTF")) {
					var exporter = new GLTFSceneExporter(new Transform[] { selectedObject.transform }, RetrieveTexturePath);

					var path = EditorUtility.OpenFolderPanel("glTF Export Path", "", "");
					
					if (!string.IsNullOrEmpty(path)) {
						exporter.SaveGLTFandBin(path, selectionName);
					}
					else {
						EditorGUILayout.HelpBox("Failed to export since the path is invalid", MessageType.Error);
					}
				}
				EditorGUILayout.EndVertical();
			}
		}
		
		public static string RetrieveTexturePath(UnityEngine.Texture texture) {
			return AssetDatabase.GetAssetPath(texture);
		}
	}
}