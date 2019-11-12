#!/bin/sh

# This script copies the updated versions of UnityGLTF Plugins
# onto this repository.
# WARNING - This is a destructive process

# delete old data
rm -rf ./unity-gltf-exporter/Assets/PlattarExporter/UnityGLTF/Plugins
rm -rf ./unity-gltf-exporter/Assets/PlattarExporter/UnityGLTF/Plugins.meta

# Copy the updated sources
cp -R ./../Sketchfab-UnityGLTF/UnityGLTF/Assets/UnityGLTF/Plugins ./unity-gltf-exporter/Assets/PlattarExporter/UnityGLTF