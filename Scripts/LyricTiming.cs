using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LyricTiming : MonoBehaviour
{
    public bool ViewMode;
    public AnimationClip animClip;
    public AudioSource audioSource;
    public float audioTime;
    public Animator[] animators;
    public GameObject[] Targets;

    
    Transform Lyric;
    bool[] isTargets;
    Dictionary<string, List<Keyframe>> keys;

    GUIStyle style;
    GUIStyleState styleState;

    // 初期化
    void Start()
    {
        // Viewモードじゃなければ
        if (!ViewMode)
        {
            EditorApplication.playModeStateChanged += OnChangedPlayMode;
            keys = new Dictionary<string, List<Keyframe>>();
            foreach (var lyric in Targets)
            {
                keys.Add(lyric.name, new List<Keyframe>());
                keys[lyric.name].Add(new Keyframe(0.0f, 0.0f, float.PositiveInfinity, float.NegativeInfinity));
                lyric.gameObject.SetActive(false);
            }

            // 存在するオブジェクトは対象外とする
            ExistsProparty();

            // GUIスタイル
            style = new GUIStyle();
            styleState = new GUIStyleState();
            styleState.textColor = Color.red;
            style.normal = styleState;
        }
    }

    // Inspector変更時初期化
    void OnValidate()
    {
        Lyric = GetComponent<Transform>();
        audioSource.time = audioTime;

        foreach (var animator in animators)
        {
            var info = animator.GetCurrentAnimatorStateInfo(0);
            animator.Play(info.fullPathHash, 0, audioTime / info.length);
            animator.enabled = true;
        }
    }

    // Update
    void Update()
    {
        audioTime += Time.deltaTime;
        DownKeyCheck();
    }

    // Editor終了イベント
    void OnChangedPlayMode(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode && !ViewMode)
        {
            SaveAnimeClip();
        }
    }

    // Game画面に表示
    void OnGUI()
    {
        GUI.Label(new Rect(10, 0, 250, 30), audioSource.time.ToString() + "s");


        if (Targets.Length > 0 && !ViewMode)
        {
            GUI.Label(new Rect(10, 30, 500, 30), "【AnimationClipに書き込めないオブジェクトは赤色で表示しています】", style);
            for (var i = 0; i < Targets.Length; i++)
            {
                if (isTargets[i])
                {
                    GUI.Label(new Rect(10, 30 * (i + 2), 250, 20), i + 1 + ":" + Targets[i].name);
                }
                else
                {
                    GUI.Label(new Rect(10, 30 * (i + 2), 250, 20), i + 1 + ":" + Targets[i].name, style);
                }
            }
        }
    }

    #region Functions
    // AnimationClip保存
    void SaveAnimeClip()
    {
        // 元AnimationClipバックアップ
        string path = AssetDatabase.GetAssetPath(animClip.GetInstanceID());
        string backup = "-" + DateTime.Now.ToString("hhmmssfff") + ".anim";

        AssetDatabase.CopyAsset(path, path.Replace(".anim", backup));
        Debug.Log("AnimationClip退避: " + path.Replace(".anim", backup));

        for (var i = 0; i < Targets.Length; i++)
        {
            if (isTargets[i])
            {
                // 最終フレーム DisableでKeyFrame追加
                keys[Targets[i].name].Add(new Keyframe(audioSource.clip.length, 0.0f, float.PositiveInfinity, float.NegativeInfinity));

                // Binging
                EditorCurveBinding curveBinding = new EditorCurveBinding();
                curveBinding.path = GethierarchyPath(Targets[i]);
                curveBinding.type = typeof(GameObject);
                curveBinding.propertyName = "m_IsActive";

                // AnimationCurve 登録したKeyFramesで初期化
                AnimationCurve curve = new AnimationCurve(keys[Targets[i].name].ToArray());

                // AnimationCurve保存
                AnimationUtility.SetEditorCurve(animClip, curveBinding, curve);
            }
        }

        Debug.Log("アニメーション保存..!");
    }

    // 入力キー判定
    void DownKeyCheck()
    {
        if (Input.anyKeyDown)
        {
            foreach (KeyCode code in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(code))
                {
                    switch (code)
                    {
                        case KeyCode.Alpha1:
                            SetKeyFrame(0);
                            break;
                        case KeyCode.Alpha2:
                            SetKeyFrame(1);
                            break;
                        case KeyCode.Alpha3:
                            SetKeyFrame(2);
                            break;
                        case KeyCode.Alpha4:
                            SetKeyFrame(3);
                            break;
                        case KeyCode.Alpha5:
                            SetKeyFrame(4);
                            break;
                        case KeyCode.Alpha6:
                            SetKeyFrame(5);
                            break;
                        case KeyCode.Alpha7:
                            SetKeyFrame(6);
                            break;
                        case KeyCode.Alpha8:
                            SetKeyFrame(7);
                            break;
                        case KeyCode.Alpha9:
                            SetKeyFrame(8);
                            break;
                        case KeyCode.Alpha0:
                            SetKeyFrame(9);
                            break;
                    }
                }
            }
        }
    }

    // KeyFrameを格納
    void SetKeyFrame(int i)
    {
        // Start KeyFrame Constant
        if (i < Targets.Length && !ViewMode)
        {
            keys[Targets[i].name].Add(new Keyframe(audioTime, !Targets[i].activeSelf ? 1.0f : 0.0f, float.PositiveInfinity, float.NegativeInfinity));
            Targets[i].SetActive(!Targets[i].activeSelf);
        }
    }


    // 親オブジェクトからのパスを取得
    string GethierarchyPath(GameObject target)
    {
        Transform _target = target.transform.parent;
        Transform _parent = Lyric;
        string path = target.name;

        while (_parent.name != _target.name)
        { 
            path = _target.name + "/" + path;
            _target = _target.parent;
        }

        return path;
    }

    // AnimationPropartyの存在チェック
    void ExistsProparty()
    {
        isTargets = new bool[Targets.Length];
        for (var i = 0; i < Targets.Length; i++)
        {
            isTargets[i] = true;
            // 既存のAnimationCurve検索
            if (animClip)
            {
                foreach (var binding in AnimationUtility.GetCurveBindings(animClip))
                {
                    if (binding.path == GethierarchyPath(Targets[i]))
                    {
                        isTargets[i] = false;
                        Targets[i].SetActive(true);
                        break;
                    }
                }
            }

        }
    }
    #endregion
}
