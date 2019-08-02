using System.ComponentModel;
using Unity.Entities;
using UnityEngine.Playables;

namespace Components
{
    public struct PlayableGraphComponent : ISystemStateComponentData
    {
        public PlayableGraph PlayableGraph;
    }
}