using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Components
{
    public class RendererGroup
    {
        private static RendererGroup _instance;

        public static RendererGroup Instance => _instance ?? (_instance = new RendererGroup());
        
        public List<Entity> RenderEntityList;
        public Queue<int> FreeIndexes;
        
        public NativeArray<float4x4> Buffer;
        public ComputeBuffer ComputeBuffer;

        private RendererGroup()
        {
            RenderEntityList = new List<Entity>();
            FreeIndexes = new Queue<int>();
        }

        public int CurrentCount => RenderEntityList.Count - FreeIndexes.Count;
    }
}