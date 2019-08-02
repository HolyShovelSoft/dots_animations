using Unity.Entities;

namespace Tools
{
    public static class EcsExtension
    {
        public static void SafeSetComponentData<T>(this EntityManager entityManager, Entity entity, T componentData) where T : struct, IComponentData
        {
            if (entityManager.HasComponent<T>(entity))
            {
                entityManager.SetComponentData(entity, componentData);
            }
            else
            {
                entityManager.AddComponentData(entity, componentData);
            }
        }

        public static void SafeSetSharedComponentData<T>(this EntityManager entityManager, Entity entity, T componentData) where T : struct, ISharedComponentData
        {
            if (entityManager.HasComponent<T>(entity))
            {
                entityManager.SetSharedComponentData(entity, componentData);
            }
            else
            {
                entityManager.AddSharedComponentData(entity, componentData);
            }
        }
    }
}