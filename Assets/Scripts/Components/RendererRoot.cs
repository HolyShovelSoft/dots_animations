using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Components
{
    public struct RendererRoot : IComponentData
    {
        public int RenderEntityListId;
    }
}