using Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Systems
{
    [ExecuteAlways]
    [UpdateAfter(typeof(CleanRendererSystem))]
    [UpdateInGroup(typeof(AnimationSystemGroup))]
    public class VisibleSystem : ComponentSystem
    {
        private EntityQuery _rendererRootGroup;
        private CullingGroup _cullingGroup;
        private BoundingSphere[] _spheres;
        private int[] resultIndices;
        
        private bool isevent;
        protected override void OnCreate()
        {
            _cullingGroup = new CullingGroup();
           
            _rendererRootGroup = GetEntityQuery(typeof(RendererRoot));

        }

        protected override void OnUpdate()
        {
            var rootEntityArray = _rendererRootGroup.ToEntityArray(Allocator.TempJob);

            if (_spheres == null || _spheres.Length != rootEntityArray.Length)
            {
                _spheres = new BoundingSphere[rootEntityArray.Length];
            }


            for (int i = 0; i < rootEntityArray.Length; i++)
            {
                var rootEntity = rootEntityArray[i];

                var rootRendererData = EntityManager.GetComponentObject<RootRendererData>(rootEntity);

                _spheres[i] = new BoundingSphere(rootRendererData.transform.position, 4f);
               
               
                if (!rootRendererData.AlwaysCalculate && !rootRendererData.IsVisible && EntityManager.HasComponent(rootEntity, typeof(NotCalculate)))
                {
                    EntityManager.RemoveComponent(rootEntity, typeof(NotCalculate));
                }

                if (rootRendererData.AlwaysCalculate && rootRendererData.IsVisible && !EntityManager.HasComponent(rootEntity, typeof(NotCalculate)))
                {
                    EntityManager.SetComponentData(rootEntity, new NotCalculate());
                }

            }

            if (!isevent)
            {
                isevent = true;
                _cullingGroup.targetCamera = Camera.main;
            }

            _cullingGroup.SetBoundingSpheres(_spheres);
            rootEntityArray.Dispose();
        }

        protected override void OnDestroy()
        {
            _cullingGroup.Dispose();
            _cullingGroup = null;
        }
    }
}