[![Twitter: @plattarglobal](https://img.shields.io/badge/contact-@plattarglobal-blue.svg?style=flat)](https://twitter.com/plattarglobal)
[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg?style=flat)](LICENSE)

_GLTF Exporter_ is a [Unity3D](https://unity3d.com/) Editor Tool that allows exporting Unity3D objects to **glTF 2.0** format.

***

### Before usage

Ensure to remove all converters that depend on the [UnityGLTF](https://github.com/KhronosGroup/UnityGLTF). This is to ensure there are no script clashes or compatibility issues.

### How to use it

Grab the latest Unity3D Package release from the [releases](https://github.com/Plattar/gltf-exporter/releases) section and import it into a new or current project.

Once the _GLTF Exporter_ tool has been imported you will see a new Toolbar in Unity3D.
<h3 align="left">
  <img src="graphics/toolbar.png?raw=true" alt="Unity3D Toolbar" width="600">
</h3>

Clicking on the _Plattar_ Toolbar will present the following editor wizard.
<h3 align="left">
  <img src="graphics/wizard.png?raw=true" alt="Unity3D Wizard" width="400">
</h3>

Either select or drag and drop the **GameObject** you'd like to export and click the export button.
<h3 align="left">
  <img src="graphics/export.png?raw=true" alt="Unity3D Export" width="400">
</h3>

You will be asked for a destination to export the GLTF files. The exporter will automatically zip all files and textures.

Supported Unity objects and features so far:
- Scene objects such as transforms and meshes
- PBR materials (both *Standard* and *Standard (Specular setup)* for metal/smoothness and specular/smoothness respectively). Other materials may also be exported but not with all their channels.
- Solid and skinning animation (note that custom scripts or *humanoid* skeletal animation are not exported yet).

*(Note that animation is still in beta)*

Please note that camera, lights, custom scripts, shaders and post processes are not exported.

### PBR materials

[GLTF 2.0 Core Specification](https://github.com/KhronosGroup/glTF/tree/master/specification/2.0) includes metal/roughness PBR material declaration. Specular/glossiness workflow is also available but kept under an extension for now.

### Important notes

Please note that for now, output glTF files **may not be 100% compliant** with the current state of glTF 2.0.

This plugin is being updated with glTF file format. It's strongly recommended to use the latest version from the [release](https://github.com/Plattar/gltf-exporter/releases) section.

### Acknowledgements

This tool relies on the following open source projects.

- [KhronosGroup UnityGLTF](https://github.com/KhronosGroup/UnityGLTF)
- [Sketchfab UnityGLTF](https://github.com/sketchfab/UnityGLTF)
