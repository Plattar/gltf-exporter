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
	public class PlattarExporter : EditorWindow {

		GameObject selectedObject;
		static Texture logo;

		[MenuItem("Plattar/GLTF Exporter")]
		static void Init() {
			logo = (Texture2D) AssetDatabase.LoadAssetAtPath("Assets/PlattarExporter/Plattar/Editor/ExporterHeader.png", typeof(Texture2D));

			PlattarExporter window = (PlattarExporter) EditorWindow.GetWindow(typeof(PlattarExporter), false, "Plattar Exporter");
			window.minSize = new Vector2(350, 340);
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
			PlattarExporterOptions.ExportAnimations = EditorGUILayout.Toggle("Export animations", PlattarExporterOptions.ExportAnimations);

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
			EditorGUILayout.HelpBox("Select a GameObject from the scene to export into GLTF", MessageType.Info);
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
					EditorGUILayout.HelpBox("Your selected GameObject or it's children contains an Alignment Grid", MessageType.Error);
					EditorGUILayout.EndVertical();

					return;
				}

				var meshes = selectedObject.GetComponentsInChildren<MeshFilter>();
				var skinnedMeshes = selectedObject.GetComponentsInChildren<SkinnedMeshRenderer>();

				if ((meshes == null || meshes.Length <= 0) && (skinnedMeshes == null || skinnedMeshes.Length <= 0)) {
					EditorGUILayout.BeginVertical();
					EditorGUILayout.HelpBox("Your selected GameObject or it's children have no Geometry", MessageType.Error);
					EditorGUILayout.EndVertical();

					return;
				}

				EditorGUILayout.Separator();
				EditorGUILayout.BeginVertical();

				if (GUILayout.Button("Export " + selectionName + " to GLTF")) {
					if (GenerateGLTFZipped(selectedObject) != null) {
						EditorUtility.DisplayDialog("Successful Export", "GLTF Exported and Zipped Successfully", "OK");
					}
				}

				EditorGUILayout.EndVertical();
			}
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
					} else {
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
	}
}