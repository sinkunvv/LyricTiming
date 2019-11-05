/*
 * ParticleSystemのPrefab
 * 選択したFBXのメッシュ分設定
 * Derayを等間隔で設定
 * 
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Fbx2Particle : EditorWindow
{
    [SerializeField] GameObject fbx;
    [SerializeField] GameObject particle;
    [SerializeField] string prefix = "0-";
    [SerializeField] int count;
    [SerializeField] float delay;

    List<Mesh> meshes;
    bool isDisable = true;
    [MenuItem("SWORKS/Fbx2Particle")]
    private static void Create()
    {
        Fbx2Particle window = GetWindow<Fbx2Particle>("Fbx2Particle");
        window.minSize = new Vector2(320, 120);
        window.maxSize = new Vector2(320, 120);
    }

    private void OnGUI()
    {
        fbx = (GameObject)EditorGUILayout.ObjectField("歌詞FBX:", fbx, typeof(GameObject), false);
        particle = (GameObject)EditorGUILayout.ObjectField("パーティクルPrefab:", particle, typeof(GameObject), false);
        prefix = EditorGUILayout.TextField("除外Prefix:", prefix);
        count = EditorGUILayout.IntField("歌詞番号:", count);
        delay = EditorGUILayout.FloatField("Particle Delay:", delay);

        // 必須項目チェック
        if (fbx && particle)
        {
            isDisable = false;
        }
        else
        {
            isDisable = true;
        }

        EditorGUI.BeginDisabledGroup(isDisable);
        if (GUILayout.Button("生成"))
        {
            CreateParticle();
        }
        EditorGUI.EndDisabledGroup();
    }

    private void CreateParticle()
    {
        // 親オブジェクト名
        if(prefix.Length > 1)
        {
            name = fbx.name.Replace(prefix, "");
        }
        else
        {
            name = fbx.name;
        }

        // 親オブジェクト生成
        GameObject LyricObject = new GameObject(name);

        // Fbx Mesh取得
        meshes = new List<Mesh>();
        GetMesh(name);

        // Particle作成
        var cnt = 0;
        foreach (var mesh in meshes)
        {
            var _particle = (GameObject)PrefabUtility.InstantiatePrefab(particle);
            var ps = _particle.GetComponent<ParticleSystem>();
            var psr = _particle.GetComponent<ParticleSystemRenderer>();
            var main = ps.main;

            // Delay
            main.startDelay = delay * cnt;

            // MeshRender
            psr.renderMode = ParticleSystemRenderMode.Mesh;
            psr.mesh = mesh;

            // ParticleSystem Object
            _particle.name = count.ToString() + " " + mesh.name;
            _particle.transform.parent = LyricObject.transform;
            count++;
            cnt++;
        }
    }

    private void GetMesh(string name)
    {
        var cnt = fbx.transform.childCount;
        if (cnt > 0)
        {
            foreach (Transform ct in fbx.transform)
            {
                meshes.Add(ct.GetComponent<MeshFilter>().sharedMesh);
            }

            // Sort
            List<Mesh> temp = new List<Mesh>();
            foreach (char c in name)
            {
                foreach (var mesh in meshes)
                {
                    if (c.ToString().Equals(mesh.name.Replace(".001", "")))
                    {
                        temp.Add(mesh);
                        meshes.Remove(mesh);
                        Debug.Log(mesh.name);
                        break;
                    }
                }
            }
            meshes = temp;
        }
        else
        {
            meshes.Add(fbx.GetComponent<MeshFilter>().sharedMesh);
        }
    }

    private List<Mesh> SortMesh(string name, List<Mesh> meshes)
    {
        List<Mesh> temp = new List<Mesh>();
       foreach(char c in name)
        {
           foreach(var mesh in meshes)
            {
                if (c.Equals(mesh.name))
                {
                    temp.Add(mesh);
                    break;
                }
            }
        }

        return temp;
    }
}
