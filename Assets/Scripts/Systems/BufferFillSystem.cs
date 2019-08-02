using Components;
using Unity.Entities;

namespace Systems
{
    [UpdateAfter(typeof(BoneMatrixCalculationSystem))]
    [UpdateInGroup(typeof(AnimationSystemGroup))]
    public class BufferFillSystem : ComponentSystem
    {
        private RendererGroup _rendererGroup;

        protected override void OnCreate()
        {
            _rendererGroup = RendererGroup.Instance;
        }

        protected override void OnUpdate()
        {
           
            var groupData = _rendererGroup;
            var buffer = groupData.Buffer;
            var computeBuffer = groupData.ComputeBuffer;
                
            computeBuffer?.SetData(buffer);
        }

        
    }
}