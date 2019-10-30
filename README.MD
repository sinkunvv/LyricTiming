# LyricTiming

UnityEditorでAudioSourceをを再生しながら
音ゲーちっくにisActiveアニメーションを生成できる

## 使い方
* LyricTiming.csをダウンロードしてUnityにインポート
* 以下のような構成で親オブジェクトにダウンロードしたスクリプトをアタッチ

```
親オブジェクト
    └子オブジェクト_1  
        └子オブジェクト_1
    └子オブジェクト_2
    └子オブジェクト_3
    └子オブジェクト_4
    └子オブジェクト_5
```
* 親オブジェクトにAnimationClipとAnimatorをアタッチする
    * AnimationClipの中は空でもよい

* スクリプトのInspectorに必要な要素をアタッチする
    * Animator     (親オブジェクトに付随するもの)
    * Anim Clip    (Animatorに紐付いているもの)
    * Audio Source (再生する音源)
    * Lyric Parent (親オブジェクト)

* AudioTimeで再生開始の秒数を指定可能
* UnityEditorの再生ボタンを押下する
* 再生された音源にあわせGameタブに表示されているボタンを押す
* 任意のタイミングでUnityEditorの停止ボタンを押下する
* 再生が停止されたタイミングでAnimationClipが更新される
    * 同時に元のAnimationClipのバックアップも生成される

* アニメーションを生成せず、現在のアニメーションを確認したい場合はViewModeにチェックを入れて再生する

## Tips
* ボタンが表示されるのはAnimationClipに存在しない子オブジェクトのみ
* 再生と同時に子オブジェクトは非アクティブとなる
* 生成されるAnimationClipには必ず0フレーム目と音源の最終フレームに非アクティブとなるキーが追加される
* バックアップで生成されるAnimationClipは元のAnimationClipと同じディレクトリに現在時刻が付与された名前で生成される
* 

## 更新予定
* ボタン式からキーボード式に変更したい
* 特定角度で回転するアニメーション生成に対応したい    
