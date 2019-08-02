using Components;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


namespace Systems
{
    [UpdateAfter(typeof(CleanRendererSystem))]
    [UpdateInGroup(typeof(AnimationSystemGroup))]
    public class BufferExtenderSystem : ComponentSystem
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

            var rootCount = groupData.RenderEntityList.Count;

            var totalBufferSize = Constants.BoneSize * rootCount;

            if (buffer.Length != totalBufferSize)
            {
                if (buffer.IsCreated)
                {
                    buffer.Dispose();
                }
                computeBuffer?.Dispose();

                buffer = new NativeArray<float4x4>(totalBufferSize, Allocator.Persistent);
                computeBuffer = new ComputeBuffer(totalBufferSize, sizeof(float) * Constants.RenderSizeConst, ComputeBufferType.Default);


                groupData.Buffer = buffer;
                groupData.ComputeBuffer = computeBuffer;

                SetComputeBuffer(groupData.RenderEntityList, computeBuffer);
            }
        }

        private void SetComputeBuffer(List<Entity> renderEntityList, ComputeBuffer buffer)
        {
            for (int i = 0; i < renderEntityList.Count; i++)
            {
                var entity = renderEntityList[i];

                if (entity != Entity.Null)
                {
                    var rootRendererData = EntityManager.GetComponentObject<RootRendererData>(entity);

                    rootRendererData.SetBuffer(buffer);
                }
            }
        }

        protected override void OnDestroy()
        {
            if (_rendererGroup.Buffer.IsCreated)
            {
                _rendererGroup.Buffer.Dispose();
            }
            _rendererGroup.ComputeBuffer?.Dispose();
        }
    }
}