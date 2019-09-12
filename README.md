[![Twitter: @plattarglobal](https://img.shields.io/badge/contact-@plattarglobal-blue.svg?style=flat)](https://twitter.com/plattarglobal)
[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg?style=flat)](LICENSE)

_GLTF Exporter_ is a [Unity3D](https://unity3d.com/) Editor Tool that allows exporting Unity3D objects to **glTF 2.0** format.

***

### ***Before Usage*** 

Ensure to remove all converters that depend on the [UnityGLTF](https://github.com/KhronosGroup/UnityGLTF). This is to ensure there are no script clashes or compatibility issues.

***This tool has been tested using the latest Unity3D 2019.x release.***

### ***Exporter Usage***

Grab the latest Unity3D Package release from the [releases](https://github.com/Plattar/gltf-exporter/releases) section and import it into a new or current project.

Once the _GLTF Exporter_ tool has been imported you will see a new Toolbar in Unity3D.
<h3 align="left">
  <img src="graphics/toolbar.png?raw=true" alt="Unity3D Toolbar" width="400">
</h3>

Clicking on ***Plattar->GLTF Exporter*** Toolbar will present the following editor wizard.

<h3 align="left">
  <img src="graphics/wizard.png?raw=true" alt="Unity3D Wizard" width="400">
</h3>

Either select or drag and drop a **GameObject** into the ***Export Object*** field.

<h3 align="left">
  <img src="graphics/export.png?raw=true" alt="Unity3D Export" width="400">
</h3>

Click on ***Export GLTF*** button.

You will be asked for a destination to export the GLTF files. The exporter will automatically zip all files and textures.

### ***Texture Options***

This section provides some flexibility regarding texture exports.

* Select _***None***_ for default functionality
* Select _***JPG***_ to force all textures to be exported in the JPG format
* Select _***PNG***_ to force all textures to be exported in the PNG format

The _Texture Quality_ slider can be used to control the quality of the output textures. Higher value will increase the quality but have larger file sizes. This setting only applies to JPG textures.

### ***Bounds Options***

This section provides some flexibility regarding bounds exports.

* Selecting _***None***_ will skip writing the min/max fields in GLTF. Some renderers will be forced to re-calculate these fields during runtime, others will break. Use at own risk.
* Selecting _***Local***_ is the default functionality and will export min/max fields computed according to the current mesh pivot.
* Selecting _***World***_ will export min/max fields and force a center pivot for the local mesh.

### ***Other Notes***

Supported Unity objects and features so far:

* Scene objects such as transforms and meshes
* PBR materials (both *Standard* and *Standard (Specular setup)* for metal/smoothness and specular/smoothness respectively). Other materials may also be exported but not with all their channels.
* Solid and skinning animation (note that custom scripts or *humanoid* skeletal animation are not exported yet).

*(Note that animation is still in beta)*

Please note that camera, lights, custom scripts, shaders and post processes are not exported.

### ***PBR Materials***

[GLTF 2.0 Core Specification](https://github.com/KhronosGroup/glTF/tree/master/specification/2.0) includes metal/roughness PBR material declaration. Specular/glossiness workflow is also available but kept under an extension for now.

### ***Important Notes***

Please note that for now, output glTF files **may not be 100% compliant** with the current state of glTF 2.0.

This plugin is being updated with glTF file format. It's strongly recommended to use the latest version from the [release](https://github.com/Plattar/gltf-exporter/releases) section.

### ***Acknowledgements***

This tool relies on the following open source projects.

* [KhronosGroup UnityGLTF](https://github.com/KhronosGroup/UnityGLTF)
* [Sketchfab UnityGLTF](https://github.com/sketchfab/UnityGLTF)

