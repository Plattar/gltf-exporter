using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GLTF.Schema;
using UnityEngine;
using UnityEngine.Rendering;
using UnityGLTF.Extensions;
using CameraType = GLTF.Schema.CameraType;
using WrapMode = GLTF.Schema.WrapMode;
using UnityEditor;

namespace UnityGLTF {

	public class GLTFAnimationExporter {

		private const int BACKING_FRAMERATE = 30;

		public enum ROTATION_TYPE {
			UNKNOWN,
			QUATERNION,
			EULER
		};

		private struct TargetCurveSet {
			public AnimationCurve[] translationCurves;
			public AnimationCurve[] rotationCurves;

			//Additional curve types
			public AnimationCurve[] localEulerAnglesRaw;
			public AnimationCurve[] m_LocalEuler;
			public AnimationCurve[] scaleCurves;
			public ROTATION_TYPE rotationType;

			public void Init() {
				translationCurves = new AnimationCurve[3];
				rotationCurves = new AnimationCurve[4];
				scaleCurves = new AnimationCurve[3];
			}
		}

		private readonly List<Transform> _animationNodes = new List<Transform>();
		private readonly Dictionary<int, int> _exportedTransforms = new Dictionary<int, int>();

		private GLTFRoot _root;
		private GLTFSceneExporter _exporter;

		public int Length { get { return _animationNodes.Count; } }

		public void AddAnimationIfAny(Transform node, int count) {
			if (node == null) {
				return;
			}

			if (node.GetComponent<UnityEngine.Animation>() || node.GetComponent<UnityEngine.Animator>()) {
				_animationNodes.Add(node);
			}

			_exportedTransforms.Add(node.GetInstanceID(), count);
		}

		public void ExportAnimations(GLTFRoot _rootNode, GLTFSceneExporter _exporter) {
			// nothing to export
			if (Length <= 0) {
				return;
			}

			this._root = _rootNode;
			this._exporter = _exporter;

			GLTFAnimation anim = new GLTFAnimation();
			anim.Samplers = new List<AnimationSampler>();
			anim.Channels = new List<AnimationChannel>();
			anim.Name = "Animation Root";

			for (int i = 0; i < _animationNodes.Count; ++i) {
				Transform animationNode = _animationNodes[i];
				ExportAnimationFromNode(ref animationNode, ref anim);
			}

			if (anim.Channels.Count > 0 && anim.Samplers.Count > 0) {
				_rootNode.Animations.Add(anim);
			}
		}

		private void ExportAnimationFromNode(ref Transform transform, ref GLTFAnimation anim) {
			Animator animator = transform.GetComponent<Animator>();

			if (animator != null) {
				AnimationClip[] clips = AnimationUtility.GetAnimationClips(transform.gameObject);

				for (int i = 0; i < clips.Length; i++) {
					//FIXME It seems not good to generate one animation per animator.
					ConvertClipToGLTFAnimation(ref clips[i], ref transform, ref anim);
				}
			}

			UnityEngine.Animation animation = transform.GetComponent<UnityEngine.Animation>();

			if (animation != null) {
				AnimationClip[] clips = AnimationUtility.GetAnimationClips(transform.gameObject);
				for (int i = 0; i < clips.Length; i++) {
					//FIXME It seems not good to generate one animation per animator.
					ConvertClipToGLTFAnimation(ref clips[i], ref transform, ref anim);
				}
			}
		}

		private void ConvertClipToGLTFAnimation(ref AnimationClip clip, ref Transform transform, ref GLTFAnimation animation) {
			if (animation == null) {
				Debug.Log("animation null");
				return;
			}
			// Generate GLTF.Schema.AnimationChannel and GLTF.Schema.AnimationSampler
			// 1 channel per node T/R/S, one sampler per node T/R/S
			// Need to keep a list of nodes to convert to indexes

			// 1. browse clip, collect all curves and create a TargetCurveSet for each target
			Dictionary<string, TargetCurveSet> targetCurvesBinding = new Dictionary<string, TargetCurveSet>();
			CollectClipCurves(clip, ref targetCurvesBinding);

			// Baking needs all properties, fill missing curves with transform data in 2 keyframes (start, endTime)
			// where endTime is clip duration
			// Note: we should avoid creating curves for a property if none of it's components is animated
			GenerateMissingCurves(clip.length, ref transform, ref targetCurvesBinding);

			// Bake animation for all animated nodes
			foreach (string target in targetCurvesBinding.Keys) {
				Transform targetTr = target.Length > 0 ? transform.Find(target) : transform;

				if (targetTr == null || targetTr.GetComponent<SkinnedMeshRenderer>()) {
					continue;
				}

				// Initialize data
				// Bake and populate animation data
				float[] times = null;
				Vector3[] positions = null;
				Vector3[] scales = null;
				Vector4[] rotations = null;

				BakeCurveSet(targetCurvesBinding[target], clip.length, BACKING_FRAMERATE, ref times, ref positions, ref rotations, ref scales);

				int channelTargetId = GetTargetIdFromTransform(ref targetTr);
				AccessorId timeAccessor = _exporter.ExportAccessor(times);

				// Create channel
				AnimationChannel Tchannel = new AnimationChannel();
				AnimationChannelTarget TchannelTarget = new AnimationChannelTarget();
				TchannelTarget.Path = GLTFAnimationChannelPath.translation;

				TchannelTarget.Node = new NodeId {
					Id = channelTargetId,
					Root = _root
				};

				Tchannel.Target = TchannelTarget;

				AnimationSampler Tsampler = new AnimationSampler();
				Tsampler.Input = timeAccessor;
				Tsampler.Output = _exporter.ExportAccessor(positions, true); // Vec3 for translation
				Tchannel.Sampler = new SamplerId {
					Id = animation.Samplers.Count,
					Root = _root
				};

				animation.Samplers.Add(Tsampler);
				animation.Channels.Add(Tchannel);

				// Rotation
				AnimationChannel Rchannel = new AnimationChannel();
				AnimationChannelTarget RchannelTarget = new AnimationChannelTarget();
				RchannelTarget.Path = GLTFAnimationChannelPath.rotation;
				RchannelTarget.Node = new NodeId {
					Id = channelTargetId,
					Root = _root
				};

				Rchannel.Target = RchannelTarget;

				AnimationSampler Rsampler = new AnimationSampler();
				Rsampler.Input = timeAccessor; // Float, for time
				Rsampler.Output = _exporter.ExportAccessor(rotations, true); // Vec4 for
				Rchannel.Sampler = new SamplerId {
					Id = animation.Samplers.Count,
					Root = _root
				};

				animation.Samplers.Add(Rsampler);
				animation.Channels.Add(Rchannel);

				// Scale
				AnimationChannel Schannel = new AnimationChannel();
				AnimationChannelTarget SchannelTarget = new AnimationChannelTarget();
				SchannelTarget.Path = GLTFAnimationChannelPath.scale;
				SchannelTarget.Node = new NodeId {
					Id = channelTargetId,
					Root = _root
				};

				Schannel.Target = SchannelTarget;

				AnimationSampler Ssampler = new AnimationSampler();
				Ssampler.Input = timeAccessor; // Float, for time
				Ssampler.Output = _exporter.ExportAccessor(scales); // Vec3 for scale
				Schannel.Sampler = new SamplerId {
					Id = animation.Samplers.Count,
					Root = _root
				};

				animation.Samplers.Add(Ssampler);
				animation.Channels.Add(Schannel);
			}
		}

		private void CollectClipCurves(AnimationClip clip, ref Dictionary<string, TargetCurveSet> targetCurves) {
			foreach (var binding in UnityEditor.AnimationUtility.GetCurveBindings(clip)) {
				AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);

				if (!targetCurves.ContainsKey(binding.path)) {
					TargetCurveSet curveSet = new TargetCurveSet();
					curveSet.Init();
					targetCurves.Add(binding.path, curveSet);
				}

				TargetCurveSet current = targetCurves[binding.path];
				if (binding.propertyName.Contains("m_LocalPosition")) {
					if (binding.propertyName.Contains(".x"))
						current.translationCurves[0] = curve;
					else if (binding.propertyName.Contains(".y"))
						current.translationCurves[1] = curve;
					else if (binding.propertyName.Contains(".z"))
						current.translationCurves[2] = curve;
				}
				else if (binding.propertyName.Contains("m_LocalScale")) {
					if (binding.propertyName.Contains(".x"))
						current.scaleCurves[0] = curve;
					else if (binding.propertyName.Contains(".y"))
						current.scaleCurves[1] = curve;
					else if (binding.propertyName.Contains(".z"))
						current.scaleCurves[2] = curve;
				}
				else if (binding.propertyName.ToLower().Contains("localrotation")) {
					current.rotationType = ROTATION_TYPE.QUATERNION;
					if (binding.propertyName.Contains(".x"))
						current.rotationCurves[0] = curve;
					else if (binding.propertyName.Contains(".y"))
						current.rotationCurves[1] = curve;
					else if (binding.propertyName.Contains(".z"))
						current.rotationCurves[2] = curve;
					else if (binding.propertyName.Contains(".w"))
						current.rotationCurves[3] = curve;
				}
				else if (binding.propertyName.ToLower().Contains("localeuler")) {
					current.rotationType = ROTATION_TYPE.EULER;
					if (binding.propertyName.Contains(".x"))
						current.rotationCurves[0] = curve;
					else if (binding.propertyName.Contains(".y"))
						current.rotationCurves[1] = curve;
					else if (binding.propertyName.Contains(".z"))
						current.rotationCurves[2] = curve;
				}

				targetCurves[binding.path] = current;
			}
		}

		private void GenerateMissingCurves(float endTime, ref Transform tr, ref Dictionary<string, TargetCurveSet> targetCurvesBinding) {
			foreach (string target in targetCurvesBinding.Keys) {

				Transform targetTr = target.Length > 0 ? tr.Find(target) : tr;

				if (targetTr == null) {
					continue;
				}

				TargetCurveSet current = targetCurvesBinding[target];

				if (current.translationCurves[0] == null) {
					current.translationCurves[0] = CreateConstantCurve(targetTr.localPosition.x, endTime);
					current.translationCurves[1] = CreateConstantCurve(targetTr.localPosition.y, endTime);
					current.translationCurves[2] = CreateConstantCurve(targetTr.localPosition.z, endTime);
				}

				if (current.scaleCurves[0] == null) {
					current.scaleCurves[0] = CreateConstantCurve(targetTr.localScale.x, endTime);
					current.scaleCurves[1] = CreateConstantCurve(targetTr.localScale.y, endTime);
					current.scaleCurves[2] = CreateConstantCurve(targetTr.localScale.z, endTime);
				}

				if (current.rotationCurves[0] == null) {
					current.rotationCurves[0] = CreateConstantCurve(targetTr.localRotation.x, endTime);
					current.rotationCurves[1] = CreateConstantCurve(targetTr.localRotation.y, endTime);
					current.rotationCurves[2] = CreateConstantCurve(targetTr.localRotation.z, endTime);
					current.rotationCurves[3] = CreateConstantCurve(targetTr.localRotation.w, endTime);
				}
			}
		}

		private AnimationCurve CreateConstantCurve(float value, float endTime) {
			AnimationCurve curve = new AnimationCurve();

			curve.AddKey(0, value);
			curve.AddKey(endTime, value);

			return curve;
		}

		private void BakeCurveSet(TargetCurveSet curveSet, float length, int bakingFramerate, ref float[] times, ref Vector3[] positions, ref Vector4[] rotations, ref Vector3[] scales) {
			int nbSamples = (int)(length * 30);
			float deltaTime = length / nbSamples;

			// Initialize Arrays
			times = new float[nbSamples];
			positions = new Vector3[nbSamples];
			scales = new Vector3[nbSamples];
			rotations = new Vector4[nbSamples];

			// Assuming all the curves exist now
			for (int i = 0; i < nbSamples; ++i) {
				float currentTime = i * deltaTime;
				times[i] = currentTime;
				positions[i] = new Vector3(curveSet.translationCurves[0].Evaluate(currentTime), curveSet.translationCurves[1].Evaluate(currentTime), curveSet.translationCurves[2].Evaluate(currentTime));
				scales[i] = new Vector3(curveSet.scaleCurves[0].Evaluate(currentTime), curveSet.scaleCurves[1].Evaluate(currentTime), curveSet.scaleCurves[2].Evaluate(currentTime));

				if (curveSet.rotationType == ROTATION_TYPE.EULER) {
					Quaternion eulerToQuat = Quaternion.Euler(curveSet.rotationCurves[0].Evaluate(currentTime), curveSet.rotationCurves[1].Evaluate(currentTime), curveSet.rotationCurves[2].Evaluate(currentTime));
					rotations[i] = new Vector4(eulerToQuat.x, eulerToQuat.y, eulerToQuat.z, eulerToQuat.w);
				}
				else {
					rotations[i] = new Vector4(curveSet.rotationCurves[0].Evaluate(currentTime), curveSet.rotationCurves[1].Evaluate(currentTime), curveSet.rotationCurves[2].Evaluate(currentTime), curveSet.rotationCurves[3].Evaluate(currentTime));
				}
			}
		}

		private int GetTargetIdFromTransform(ref Transform transform) {
			if (_exportedTransforms.ContainsKey(transform.GetInstanceID())) {
				return _exportedTransforms[transform.GetInstanceID()];
			}
			else {
				Debug.Log(transform.name + " " + transform.GetInstanceID());
				return 0;
			}
		}
	}
}