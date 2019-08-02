using Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Systems
{
    [ExecuteAlways]
    [UpdateInGroup(typeof(AnimationResolveSystemGroup))]
    public class GraphUpdateSystem : ComponentSystem
    {
        private EntityQuery _playableGraphComponentGroup;

        protected override void OnCreate()
        {
            _playableGraphComponentGroup = GetEntityQuery(typeof(PlayableGraphComponent), typeof(RendererRoot));
        }
        protected override void OnUpdate()
        {
            if (_playableGraphComponentGroup.CalculateLength() == 0) return;

            var dt = Time.deltaTime;

            var entityArray = _playableGraphComponentGroup.ToEntityArray(Allocator.TempJob);

            for (int i = 0; i < entityArray.Length; i++)
            {
                EntityManager.GetComponentData<PlayableGraphComponent>(entityArray[i]).PlayableGraph.Evaluate(dt);
            }

            entityArray.Dispose();
        }
    }
}