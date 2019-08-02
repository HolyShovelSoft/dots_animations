using Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Systems
{
    [ExecuteAlways]
    [UpdateInGroup(typeof(AnimationSystemGroup))]
    public class RendererInitializeSystem : ComponentSystem
    {
        private EntityQuery _customRendererGroup;
        private RendererGroup _rendererGroup;
      
        protected override void OnCreate()
        {
            _rendererGroup = RendererGroup.Instance;
            _customRendererGroup = GetEntityQuery(typeof(RendererRoot), ComponentType.Exclude(typeof(RemoveRenderer)));
        }

        protected override void OnUpdate()
        {
            var customRendererEntityArray = _customRendererGroup.ToEntityArray(Allocator.TempJob);

            for (int i = 0; i < customRendererEntityArray.Length; i++)
            {
                var customRendererEntity = customRendererEntityArray[i];

                var rootRendererData = EntityManager.GetComponentObject<RootRendererData>(customRendererEntity);
               

                var groupData = RegisterRendererInGroup(customRendererEntity);
                var rootRenderId = groupData.RootRenderId;

                rootRendererData.SetIndex(rootRenderId);


                PostUpdateCommands.AddSharedComponent(customRendererEntity, new RemoveRenderer
                {
                    CustomRendererIndex = rootRenderId,

                });

                PostUpdateCommands.SetComponent(customRendererEntity, new RendererRoot()
                {
                    RenderEntityListId = rootRenderId,
                });
            }

            customRendererEntityArray.Dispose();
        }

        private RendererGroupData RegisterRendererInGroup(Entity customRendererEntity)
        {
            
            if (_rendererGroup.FreeIndexes.Count == 0)
            {
                _rendererGroup.RenderEntityList.Add(customRendererEntity);
                return new RendererGroupData(_rendererGroup.RenderEntityList.Count - 1);
            }
            
            var idInList = _rendererGroup.FreeIndexes.Dequeue();
            _rendererGroup.RenderEntityList[idInList] = customRendererEntity;
            return new RendererGroupData(idInList);
        }
        

        private struct RendererGroupData
        {
            public int RootRenderId;
            public RendererGroupData(int rootRenderId)
            {
                RootRenderId = rootRenderId;
            }
        }
    }
}