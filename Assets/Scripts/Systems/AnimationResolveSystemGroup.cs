using Unity.Entities;
using Unity.Transforms;

namespace Systems
{
    [UpdateInGroup(typeof(TransformSystemGroup))]
    [UpdateAfter(typeof(CopyTransformFromGameObjectSystem))]
    public class AnimationResolveSystemGroup : ComponentSystemGroup
    {
        
    }
}