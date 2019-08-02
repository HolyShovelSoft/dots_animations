using Unity.Collections;
using Unity.Entities;

namespace Components
{
    public struct RemoveRenderer : ISystemStateSharedComponentData
    {
        public int CustomRendererIndex;
    }
}