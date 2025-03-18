// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;
using Node = UnityEngine.XR.XRNode;

namespace NorthStar
{
    /// <summary>
    /// Bare-bones vr camera rig for testing
    /// </summary>
    public class SimpleOvrCamera : MonoBehaviour
    {
        public bool UseAsw = true;
        public float Framerate = 90;
        public bool DynamicFoveatedRendering = false;
        public OVRPlugin.FoveatedRenderingLevel FoveatedRenderingLevel = OVRPlugin.FoveatedRenderingLevel.Off;
        public bool UpdatePosition = true;

        private void Awake()
        {
            GetComponent<Camera>().depthTextureMode = DepthTextureMode.MotionVectors;

            OVRPlugin.systemDisplayFrequency = Framerate;
            OVRManager.SetSpaceWarp(UseAsw);
            OVRPlugin.useDynamicFoveatedRendering = DynamicFoveatedRendering;
            OVRPlugin.foveatedRenderingLevel = FoveatedRenderingLevel;
        }

        private void Update()
        {
            if (!UpdatePosition)
                return;

            if (OVRNodeStateProperties.GetNodeStatePropertyVector3(Node.CenterEye, NodeStatePropertyType.Position, OVRPlugin.Node.EyeCenter, OVRPlugin.Step.Render, out var centerEyePosition))
                transform.localPosition = centerEyePosition;

            if (OVRNodeStateProperties.GetNodeStatePropertyQuaternion(Node.CenterEye, NodeStatePropertyType.Orientation, OVRPlugin.Node.EyeCenter, OVRPlugin.Step.Render, out var centerEyeRotation))
                transform.localRotation = centerEyeRotation;
        }
    }
}
