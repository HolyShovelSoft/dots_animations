using Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Systems
{
    [ExecuteAlways]
    [UpdateAfter(typeof(VisibleSystem))]
    [UpdateInGroup(typeof(AnimationSystemGroup))]
    public class BoneMatrixCalculationSystem : JobComponentSystem
    {
        private EntityQuery _customRendererGroup;
        private JobHandle _previous;
        private RendererGroup _rendererGroup;

        protected override void OnCreate()
        {
            _rendererGroup = RendererGroup.Instance;
            _customRendererGroup = GetEntityQuery(typeof(RendererRoot),ComponentType.Exclude(typeof(NotCalculate)));
        }
        
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var customRendererEntityArray = _customRendererGroup.ToEntityArray(Allocator.TempJob);
            var length = customRendererEntityArray.Length;

            var current = new CalculateMatrix()
            {
                CustomRendererEntityArray = customRendererEntityArray,
                CustomRenderer = GetComponentDataFromEntity<RendererRoot>(),
                CustomBone = GetComponentDataFromEntity<CustomBone>(),
                LocalToWorld = GetComponentDataFromEntity<LocalToWorld>(),
                CustomBoneDynamicBuffer = GetBufferFromEntity<BoneEntityDynamicBufferElement>(),
                MatrixArray = _rendererGroup.Buffer,
                BonesCount = Constants.BoneSize,

            }.Schedule(length,4,inputDeps);

            current.Complete();
            customRendererEntityArray.Dispose();

           return current;
        }

        [BurstCompile]
        private struct CalculateMatrix : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<Entity> CustomRendererEntityArray;

            [NativeDisableParallelForRestriction]
            public NativeArray<float4x4> MatrixArray;

            [ReadOnly]
            public BufferFromEntity<BoneEntityDynamicBufferElement> CustomBoneDynamicBuffer;

            [ReadOnly]
            public ComponentDataFromEntity<LocalToWorld> LocalToWorld;

            [ReadOnly]
            public ComponentDataFromEntity<CustomBone> CustomBone;

            [ReadOnly]
            public ComponentDataFromEntity<RendererRoot> CustomRenderer;

            [ReadOnly]
            public int BonesCount;

           

            public void Execute(int index)
            {
                var customRendererEntity = CustomRendererEntityArray[index];

                var boneDynamicBuffer = CustomBoneDynamicBuffer[customRendererEntity];
                var instanceCount = boneDynamicBuffer.Length;

                for (int i = 0; i < instanceCount; i++)
                {
                    var boneEntity = boneDynamicBuffer[i].Value;

                    var localToWorld = LocalToWorld.Exists(boneEntity)? LocalToWorld[boneEntity] : new LocalToWorld();
                    var bindMatrix   = CustomBone.Exists(boneEntity)? CustomBone[boneEntity].BindMatrix:float4x4.identity;

                    var renderEntityListId = CustomRenderer[customRendererEntity].RenderEntityListId;
                    var result = math.mul(localToWorld.Value, bindMatrix);

                    MatrixArray[renderEntityListId * BonesCount + i] = result;
                }
            }
        }
    }
}