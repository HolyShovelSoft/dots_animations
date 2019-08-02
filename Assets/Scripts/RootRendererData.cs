using System.Collections.Generic;
using Systems;
using Components;
using Data;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Tools;

[ExecuteAlways]
public class RootRendererData : MonoBehaviour
{
    [SerializeField]
    public BoneInfo[] BoneInfoArray;

    [SerializeField]
    public List<MeshRenderer> MeshRenderers;

    [SerializeField]
    private RuntimeAnimatorController _runtimeAnimatorController;
       
    public bool IsVisible
    {
        get {
            for (int i = 0; i < MeshRenderers.Count; i++)
            {
                var meshRenderer = MeshRenderers[i];
                if (meshRenderer && meshRenderer.isVisible) return true;
            }
            return false;
        }
    }
       
    public bool AlwaysCalculate;


    public void SetBuffer(ComputeBuffer buffer)
    {
        Shader.SetGlobalBuffer("matrixBuffer", buffer);
    }

    public void SetIndex(int index)
    {
        for (int i = 0; i < MeshRenderers.Count; i++)
        {
            var mpb = Constants.MaterialPropertyBlock;
            mpb.Clear();
            mpb.SetInt("_RendererIndex", index);
            MeshRenderers[i].SetPropertyBlock(mpb);
        }
    }

    

    private void Start()
    {
        if (!Application.IsPlaying(this)) return;
        
        var playableGraph = PlayableGraph.Create("RootRenderPlayableGraph");
        playableGraph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
        var animatorComtrollerPlayable = AnimatorControllerPlayable.Create(playableGraph, _runtimeAnimatorController);
        var output = AnimationPlayableOutput.Create(playableGraph, "RootRenderPlayableGraphOutPut", GetComponent<Animator>());
        output.SetSourcePlayable(animatorComtrollerPlayable);

        var dataEntity = gameObject.AddComponent<GameObjectEntity>();

        dataEntity.EntityManager.AddComponent(dataEntity.Entity, ComponentType.ReadWrite<RendererRoot>());
        dataEntity.EntityManager.AddBuffer<BoneEntityDynamicBufferElement>(dataEntity.Entity);

        dataEntity.EntityManager.AddComponentData(dataEntity.Entity, new PlayableGraphComponent
        {
            PlayableGraph = playableGraph
        });

        dataEntity.EntityManager.AddComponentData(dataEntity.Entity, new RemoveTagPlayableGraphComponent());


        playableGraph.Play();

#if UNITY_EDITOR
        dataEntity.EntityManager.SetName(dataEntity.Entity, "RendererRoot " + dataEntity.Entity.Index);
        gameObject.name += " " + dataEntity.Entity.Index;
#endif   

        for (int i = 0; i < BoneInfoArray.Length; i++)
        {
            var boneGameObject = BoneInfoArray[i].BoneTransform.gameObject;
            var entityBone = boneGameObject.GetComponent<GameObjectEntity>();

            if (entityBone != null) continue;

            entityBone = boneGameObject.AddComponent<GameObjectEntity>();


#if UNITY_EDITOR
            entityBone.EntityManager.SetName(entityBone.Entity, boneGameObject.name+ " " + entityBone.Entity.Index);
#endif

            entityBone.EntityManager.SafeSetComponentData(entityBone.Entity, new CopyTransformFromGameObject());
            entityBone.EntityManager.SafeSetComponentData(entityBone.Entity, new Translation());
            entityBone.EntityManager.SafeSetComponentData(entityBone.Entity, new LocalToWorld ());

            var customBone = new CustomBone
            {
                BindMatrix = BoneInfoArray[i].BindMatrix,
            };

         
            entityBone.EntityManager.SafeSetComponentData(entityBone.Entity, customBone);


            var boneEntityDynamicBufferElement = new BoneEntityDynamicBufferElement { Value = entityBone.Entity };

            dataEntity.EntityManager.GetBuffer<BoneEntityDynamicBufferElement>(dataEntity.Entity).Add(boneEntityDynamicBufferElement);


        }
    }

#if UNITY_EDITOR

    private NativeArray<float4x4> _matrixArray;
    private ComputeBuffer _computeBuffer;


    private void OnEnable()
    {
        if (!Application.IsPlaying(this) && BoneInfoArray != null && BoneInfoArray.Length > 0)
        {
            _matrixArray = new NativeArray<float4x4>(BoneInfoArray.Length, Allocator.Persistent);
            _computeBuffer = new ComputeBuffer(Constants.BoneSize, sizeof(float) * Constants.RenderSizeConst, ComputeBufferType.Default);

            for (int i = 0; i < MeshRenderers.Count; i++)
            {
                var mpb = Constants.MaterialPropertyBlock;
                mpb.Clear();
                mpb.SetInt("_RendererIndex", 0);
                mpb.SetBuffer("matrixBuffer", _computeBuffer);
                MeshRenderers[i].SetPropertyBlock(mpb);
            }
        }
    }


    private void OnDisable()
    {
        if (!Application.IsPlaying(this))
        {
            _computeBuffer?.Dispose();

            if (_matrixArray.Length > 0)
                _matrixArray.Dispose();
        }
    }


    private void LateUpdate()
    {
        if (Application.IsPlaying(this)) return;

        for (int i = 0; i < BoneInfoArray.Length; i++)
        {
            var bone = BoneInfoArray[i];

            var localToWorld = bone.BoneTransform.localToWorldMatrix;
            var bindMatrix = bone.BindMatrix;
            var result = math.mul(localToWorld, bindMatrix);

            _matrixArray[i] = result;
        }

        _computeBuffer.SetData(_matrixArray);
    }
#endif

}