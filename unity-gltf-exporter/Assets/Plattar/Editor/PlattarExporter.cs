using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityGLTF;
using UnityEngine.SceneManagement;
using System.IO;
using System.Text.RegularExpressions;

namespace Plattar {
	public class PlattarExporter : EditorWindow {
		
		AnimBool showSettings;
		GameObject selectedObject;
		static Texture logo;
		
		[MenuItem("Plattar/GLTF Exporter")]
		static void Init() {
			logo = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Plattar/Editor/ExporterHeader.png", typeof(Texture2D));
		
			PlattarExporter window = (PlattarExporter)EditorWindow.GetWindow(typeof(PlattarExporter), false, "Plattar Exporter");
			window.minSize = new Vector2(350, 320);
			window.Show();
		}
		
		void OnEnable() {
			showSettings = new AnimBool(false);
			showSettings.valueChanged.AddListener(Repaint);
		}
		
		void OnGUI() {
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label(logo);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			
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
				EditorGUILayout.HelpBox("You need to select a GameObject to continue", MessageType.Error);
				EditorGUILayout.EndVertical();
			}
			else {
				string selectionName = selectedObject.name;
				
				// ensure the name of our object is valid
				if (string.IsNullOrEmpty(selectionName) || Regex.Matches(selectionName,@"[a-zA-Z]").Count <= 0) {
					EditorGUILayout.BeginVertical();
					EditorGUILayout.HelpBox("Your Selected GameObject does not have a valid name", MessageType.Error);
					EditorGUILayout.EndVertical();

					return;
				}
				
				EditorGUILayout.Separator();
				EditorGUILayout.BeginVertical();

				if (GUILayout.Button("Export " + selectionName + " to GLTF")) {
					var exporter = new GLTFSceneExporter(new Transform[] { selectedObject.transform }, RetrieveTexturePath);

					var path = EditorUtility.OpenFolderPanel("glTF Export Path", "", "");
					if (!string.IsNullOrEmpty(path)) {
						path += "/" + selectionName + "_export_gltf";
						DirectoryInfo info = Directory.CreateDirectory(path);
						
						if (info.Exists) {
							exporter.SaveGLTFandBin(path, selectionName);
							EditorUtility.DisplayDialog("Successful Export", "GLTF Exported Successfully", "OK");
						}
						else {
							EditorUtility.DisplayDialog("Failed Export", "GLTF Could not be exported, could not create export path", "OK");
						}
					}
					else {
						EditorGUILayout.HelpBox("Failed to export since the path is invalid", MessageType.Error);
						EditorUtility.DisplayDialog("Failed Export", "GLTF Could not be exported, invalid export path provided", "OK");
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