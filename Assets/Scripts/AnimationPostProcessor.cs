#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Data;
using UnityEditor;
using UnityEngine;

public class AnimationPostProcessor : AssetPostprocessor
{
    void OnPostprocessModel(GameObject go)
    {
        var skinnedMeshAnimators = go.GetComponentsInChildren<SkinnedMeshRenderer>(true);

        if (!skinnedMeshAnimators.Any())
        {
            Debug.LogError($"There is no SkinnedMeshRenderer on the object: {go.name} ");
            return;
        }

        var transforms = skinnedMeshAnimators.SelectMany(renderer => renderer.bones).Distinct().ToList();
      
        var boneInfoArray = new BoneInfo[transforms.Count];
        var meshRendererArray = new List<MeshRenderer>();

        for (var i = 0; i <= skinnedMeshAnimators.Length - 1; i++)
        {
            var sm = skinnedMeshAnimators[i];
            var smGo = sm.gameObject;
            var mesh = sm.sharedMesh;
            var materials = sm.sharedMaterials;

            var indexMap = sm.bones.Select((tr, idx) => new KeyValuePair<int, int>(idx, transforms.IndexOf(tr))).ToDictionary(pair => pair.Key, pair => pair.Value);
          

            var smBindPoses = mesh.bindposes;
            for (var j = 0; j <= smBindPoses.Length - 1; j++)
            {
                if (indexMap.ContainsKey(j) && boneInfoArray[indexMap[j]].BoneTransform == null)
                {
                    boneInfoArray[indexMap[j]] = new BoneInfo(sm.bones[j], smBindPoses[j]);
                }
            }
           
            var weights = mesh.boneWeights.Select(weight => new Vector4
            {
                x = weight.weight0,
                y = weight.weight1,
                z = weight.weight2,
                w = weight.weight3
            }).ToList();

            var indexes = mesh.boneWeights.Select(weight => new Vector4
            {
                x = indexMap[weight.boneIndex0],
                y = indexMap[weight.boneIndex1],
                z = indexMap[weight.boneIndex2],
                w = indexMap[weight.boneIndex3],
            }).ToList();

          
            mesh.SetUVs(6, indexes);
            mesh.SetUVs(7, weights);

            Object.DestroyImmediate(sm);
            
            smGo.AddComponent<MeshFilter>().sharedMesh = mesh;
            var rend = smGo.AddComponent<MeshRenderer>();
            rend.sharedMaterials = materials;

            meshRendererArray.Add(rend);
        }

        var rootRendererData = go.AddComponent<RootRendererData>();
        rootRendererData.BoneInfoArray = boneInfoArray;
        rootRendererData.MeshRenderers = meshRendererArray;
    }
}
#endif
