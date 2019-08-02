using Unity.Entities;
using Unity.Mathematics;

namespace Components
{
    public struct CustomBone : IComponentData
    {       
        public float4x4 BindMatrix;
    }
}