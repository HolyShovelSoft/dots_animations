using Components;
using Unity.Collections;
using Unity.Entities;

namespace Systems
{
    [UpdateBefore(typeof(CleanRendererSystem))]
    [UpdateInGroup(typeof(AnimationSystemGroup))]
    public class CleanPlayableGraphSystem : ComponentSystem
    {
        private EntityQuery _removePlayableGraphGroupRunTime;
        private EntityQuery _removePlayableGraphGroupOnDestroy;

        protected override void OnCreate()
        {
            _removePlayableGraphGroupRunTime = GetEntityQuery(typeof(PlayableGraphComponent), ComponentType.Exclude(typeof(RemoveTagPlayableGraphComponent)));
            _removePlayableGraphGroupOnDestroy = GetEntityQuery(typeof(PlayableGraphComponent));
        }

        protected override void OnUpdate()
        {
            var playableGraphEntityArray = _removePlayableGraphGroupRunTime.ToEntityArray(Allocator.TempJob);

           
            for (int i = 0; i < playableGraphEntityArray.Length; i++)
            {

                var graphEntity = playableGraphEntityArray[i];

                var data = EntityManager.GetComponentData<PlayableGraphComponent>(graphEntity);
                data.PlayableGraph.Destroy();

                PostUpdateCommands.RemoveComponent(graphEntity, typeof(PlayableGraphComponent));

            }

            playableGraphEntityArray.Dispose();
        }

        protected override void OnDestroy()
        {
            var playableGraphEntityArray = _removePlayableGraphGroupOnDestroy.ToEntityArray(Allocator.TempJob);
            
            for (int i = 0; i < playableGraphEntityArray.Length; i++)
            {
                var graphEntity = playableGraphEntityArray[i];

                var data = EntityManager.GetComponentData<PlayableGraphComponent>(graphEntity);
                data.PlayableGraph.Destroy();
            }

            playableGraphEntityArray.Dispose();
        }
    }
}