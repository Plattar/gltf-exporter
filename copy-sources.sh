#!/bin/sh

# This script copies the updated versions of UnityGLTF
# onto this repository.
# WARNING - This is a destructive process

# delete old data
rm -rf ./unity-gltf-exporter/Assets/PlattarExporter/UnityGLTF
rm -rf ./unity-gltf-exporter/Assets/PlattarExporter/UnityGLTF.meta
rm -rf ./unity-gltf-exporter/Assets/PlattarExporter/Resources
rm -rf ./unity-gltf-exporter/Assets/PlattarExporter/Resources.meta


# create a fresh directory to copy new sources into
mkdir -p ./unity-gltf-exporter/Assets/PlattarExporter/UnityGLTF

# Copy the updated sources
cp -R ./../UnityGLTF/UnityGLTF/Assets/UnityGLTF/Scripts ./unity-gltf-exporter/Assets/PlattarExporter/UnityGLTF
cp -R ./../UnityGLTF/UnityGLTF/Assets/UnityGLTF/Shaders ./unity-gltf-exporter/Assets/PlattarExporter/UnityGLTF
cp -R ./../UnityGLTF/UnityGLTF/Assets/UnityGLTF/Plugins ./unity-gltf-exporter/Assets/PlattarExporter/UnityGLTF

cp -R ./../UnityGLTF/UnityGLTF/Assets/Resources ./unity-gltf-exporter/Assets/PlattarExporter

# delete the editor folder
rm -rf ./unity-gltf-exporter/Assets/PlattarExporter/UnityGLTF/Scripts/Editor
rm -rf ./unity-gltf-exporter/Assets/PlattarExporter/UnityGLTF/Scripts/Editor.meta