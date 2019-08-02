using Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace Systems
{

    [ExecuteAlways]
    [UpdateAfter(typeof(RendererInitializeSystem))]
    [UpdateInGroup(typeof(AnimationSystemGroup))]
    public class CleanRendererSystem : ComponentSystem
    {
        private EntityQuery _removeRendererGroup;
        private RendererGroup _rendererGroup;


        protected override void OnCreate()
        {
            _rendererGroup = RendererGroup.Instance;
            _removeRendererGroup = GetEntityQuery(typeof(RemoveRenderer),ComponentType.Exclude(typeof(RendererRoot)));
        }

        protected override void OnUpdate()
        {
            var renderEntityArray = _removeRendererGroup.ToEntityArray(Allocator.TempJob);
            
            var removeRendererGroupEntityArray = _removeRendererGroup.ToEntityArray(Allocator.TempJob);

            for (int i = 0; i < removeRendererGroupEntityArray.Length; i++)
            {
                var removeRendererArchetypeChunk = GetArchetypeChunkSharedComponentType<RemoveRenderer>();
                var removeRenderer = EntityManager.GetChunk(removeRendererGroupEntityArray[i]).GetSharedComponentData(removeRendererArchetypeChunk, EntityManager); 

             
                var index = removeRenderer.CustomRendererIndex;
                _rendererGroup.RenderEntityList[index] = Entity.Null;
                _rendererGroup.FreeIndexes.Enqueue(index);

                var renderEntity = renderEntityArray[i];
               
                PostUpdateCommands.RemoveComponent(renderEntity, typeof(RemoveRenderer));

            }

            renderEntityArray.Dispose();
            removeRendererGroupEntityArray.Dispose();
        }
    }
}