[![Twitter: @plattarglobal](https://img.shields.io/badge/contact-@plattarglobal-blue.svg?style=flat)](https://twitter.com/plattarglobal)
[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg?style=flat)](LICENSE)

_GLTF Exporter_ is a [Unity3D](https://unity3d.com/) Editor Tool that allows exporting Unity3D objects to **glTF 2.0** format.

***

### How to use it

It is recommended you download the latest release from https://github.com/Plattar/Unity-glTF-Exporter/releases and open it in unity as a project. This will give you access to the environment maps.

Once the project is open, a new item should appear in the *Tools* menu. You can access the exporter by going through **Tools/Export to Plattar** as shown in the following screenshot:

Select the objects you want to export and click export. You will be prompted to save the zip file somewhere.

Supported Unity objects and features so far:
- Scene objects such as transforms and meshes
- PBR materials (both *Standard* and *Standard (Specular setup)* for metal/smoothness and specular/smoothness respectively). Other materials may also be exported but not with all their channels.
- Solid and skinning animation (note that custom scripts or *humanoid* skeletal animation are not exported yet).

*(Note that animation is still in beta)*

Please note that camera, lights, custom scripts, shaders and post processes are not exported.


### PBR materials

glTF 2.0 core specification includes metal/roughness PBR material declaration. Specular/glossiness workflow is also available but kept under an extension for now.

Link to the glTF 2.0 specification: https://github.com/KhronosGroup/glTF/tree/2.0/specification/2.0

### Important notes

Please note that for now, output glTF files **may not be 100% compliant** with the current state of glTF 2.0.

This plugin is being updated with glTF file format. It's strongly recommended to use the latest version from the [release](https://github.com/Plattar/gltf-exporter/releases) section.

### Acknowledgements

This tool relies on the following open source projects.

- [KhronosGroup UnityGLTF](https://github.com/KhronosGroup/UnityGLTF)
