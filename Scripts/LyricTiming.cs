using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LyricTiming : MonoBehaviour
{
    public bool ViewMode;
    public Animator animator;
    public AnimationClip animClip;
    public AudioSource audioSource;
    public float audioTime;
    public GameObject lyricParent;
    public GameObject[] lyrics;

    GameObject[] ExistLyrics;
    Dictionary<string, List<Keyframe>> keys;


    void Start()
    {
        EditorApplication.playModeStateChanged += OnChangedPlayMode;
        foreach (var lyric in lyrics)
        {
            keys.Add(lyric.name, new List<Keyframe>());
            keys[lyric.name].Add(new Keyframe(0.0f, 0.0f, float.PositiveInfinity, float.NegativeInfinity));
            //keys[lyric.name].Add(new Keyframe(0.1f, 0.0f));
        }
    }

    // Inspector変更時初期化
    void OnValidate()
    {
        audioSource.time = audioTime;

        var info = animator.GetCurrentAnimatorStateInfo(0);
        animator.Play(info.fullPathHash, 0, audioTime / info.length);
        animator.enabled = true;


        keys = new Dictionary<string, List<Keyframe>>();
        if (lyricParent)
        {
            var child = lyricParent.transform.childCount;
            lyrics = new GameObject[child];
            ExistLyrics = new GameObject[child];
            var cnt = 0;
            foreach (Transform lyric in lyricParent.transform)
            {
                lyrics[cnt] = lyric.gameObject;
                lyric.gameObject.SetActive(false);
                cnt++;
            }

            // 既存アニメーションチェック
            ExistsCurve();
        }
    }

    void OnGUI()
    {
        if (lyrics.Length > 0 && !ViewMode)
        {
            var x = 0;
            var y = 0;
            for (var i = 0; i < lyrics.Length; i++)
            {
                // 既存のアニメーションが存在しないオブジェクトだけボタン配置
                if (lyrics[i] != ExistLyrics[i])
                {
                    if (GUI.Button(new Rect(130 * x, 40 * y, 120, 30), (!lyrics[i].activeSelf ? "Enable:" : "Disable:") + lyrics[i].name))
                    {
                        // Start:0 Stop:1
                        // Start KeyFrame Constant
                        keys[lyrics[i].name].Add(new Keyframe(audioTime, !lyrics[i].activeSelf ? 1.0f : 0.0f, float.PositiveInfinity, float.NegativeInfinity));

                        lyrics[i].SetActive(!lyrics[i].activeSelf);
                    }
                    x++;

                    // ボタン改行
                    if (x > 5)
                    {
                        x = 1;
                        y++;
                    }
                }
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        audioTime += Time.deltaTime;

    }

    void SaveAnimeClip()
    {
        // 元AnimationClipバックアップ
        string path = AssetDatabase.GetAssetPath(animClip.GetInstanceID());
        string backup = "-" + DateTime.Now.ToString("hhmmssfff") + ".anim";

        AssetDatabase.CopyAsset(path, path.Replace(".anim", backup));
        Debug.Log("AnimationClip退避: " + path.Replace(".anim", backup));

        foreach (var lyric in lyrics)
        {
            // 最終フレーム DisableでKeyFrame追加
            keys[lyric.name].Add(new Keyframe(audioSource.clip.length, 0.0f, float.PositiveInfinity, float.NegativeInfinity));

            // Binging
            EditorCurveBinding curveBinding = new EditorCurveBinding();
            curveBinding.path = lyric.name;
            curveBinding.type = typeof(GameObject);
            curveBinding.propertyName = "m_IsActive";

            // AnimationCurve 登録したKeyFramesで初期化
            AnimationCurve curve = new AnimationCurve(keys[lyric.name].ToArray());

            // 既存のAnimationCurve検索
            if (animClip)
            {
                foreach (var binding in AnimationUtility.GetCurveBindings(animClip))
                {
                    // 既存のAnimationCurveが存在すれば上書き
                    if (binding.path == lyric.name)
                    {
                        curve = AnimationUtility.GetEditorCurve(animClip, binding);

                        // 登録したKeyFrameをAnimationCurveに追加
                        foreach (var key in keys[lyric.name])
                        {
                            curve.AddKey(key);
                        }
                        break;
                    }
                }
            }
            // AnimationCurve保存
            AnimationUtility.SetEditorCurve(animClip, curveBinding, curve);
        }

        Debug.Log("アニメーション保存..!");
    }

    void OnChangedPlayMode(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode && !ViewMode)
        {
            SaveAnimeClip();
        }
    }

    void ExistsCurve()
    {
        for (var i = 0; i < lyrics.Length; i++)
        {
            // 既存のAnimationCurve検索
            if (animClip)
            {
                foreach (var binding in AnimationUtility.GetCurveBindings(animClip))
                {
                    if (binding.path == lyrics[i].name)
                    {
                        ExistLyrics[i] = lyrics[i];
                    }
                }
            }
        }
    }
}