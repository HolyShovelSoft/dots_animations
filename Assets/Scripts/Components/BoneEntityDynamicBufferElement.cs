using Systems;
using Unity.Entities;

namespace Components
{
    [InternalBufferCapacity(Constants.BoneSize)]
    public struct BoneEntityDynamicBufferElement : IBufferElementData
    {
        public Entity Value;
    }
}