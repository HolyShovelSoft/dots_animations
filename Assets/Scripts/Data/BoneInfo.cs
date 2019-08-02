using System;
using UnityEngine;

namespace Data
{
    [Serializable]
    public struct BoneInfo
    {
        [SerializeField]
        private Transform _boneTransform;

        [SerializeField]
        private Matrix4x4 _bindMatrix;

        public BoneInfo(Transform boneTransform, Matrix4x4 bindMatrix)
        {
            _boneTransform = boneTransform;
            _bindMatrix = bindMatrix;
        }

        public Transform BoneTransform => _boneTransform;

        public Matrix4x4 BindMatrix => _bindMatrix;
    }
}