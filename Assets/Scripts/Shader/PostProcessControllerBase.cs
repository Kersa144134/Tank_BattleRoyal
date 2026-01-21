using UnityEngine;
using UnityEngine.Rendering.Universal;

// ======================================================
// Render Feature 制御基底クラス
// ======================================================

/// <summary>
/// Full Screen Pass Render Feature を制御するための共通基底クラス
/// </summary>
public abstract class PostProcessControllerBase
{
    // ======================================================
    // コンポーネント参照
    // ======================================================

    /// <summary>
    /// 制御対象となる Full Screen Pass Render Feature
    /// </summary>
    protected readonly ScriptableRendererFeature _fullScreenPassFeature;

    /// <summary>
    /// Full Screen Pass で使用される Effect Material
    /// </summary>
    protected readonly Material _effectMaterial;

    // ======================================================
    // コンストラクタ
    // ======================================================

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="fullScreenPassFeature">外部から注入される制御対象の Render Feature</param>
    protected PostProcessControllerBase(
        ScriptableRendererFeature fullScreenPassFeature,
        Material effectMaterial)
    {
        _fullScreenPassFeature = fullScreenPassFeature;
        _effectMaterial = effectMaterial;
    }

    // ======================================================
    // PostProcessControllerBase イベント
    // ======================================================

    /// <summary>
    /// Full Screen Pass Render Feature を有効化する
    /// </summary>
    public void EnableFullScreenPass()
    {
        if (_fullScreenPassFeature == null)
        {
            return;
        }

        _fullScreenPassFeature.SetActive(true);
    }

    /// <summary>
    /// Full Screen Pass Render Feature を無効化する
    /// </summary>
    public void DisableFullScreenPass()
    {
        if (_fullScreenPassFeature == null)
        {
            return;
        }

        _fullScreenPassFeature.SetActive(false);
    }

    /// <summary>
    /// Full Screen Pass Render Feature の有効状態を反転する
    /// </summary>
    public void ToggleFullScreenPass()
    {
        if (_fullScreenPassFeature == null)
        {
            return;
        }

        _fullScreenPassFeature.SetActive(
            !_fullScreenPassFeature.isActive);
    }

    // ======================================================
    // 抽象メソッド
    // ======================================================

    /// <summary>
    /// 派生クラスごとのシェーダーパラメータを
    /// Material に書き込むための抽象メソッド
    /// </summary>
    /// <param name="material">
    /// 書き込み対象となる Effect Material
    /// </param>
    protected abstract void ApplyPropertiesToMaterial(
        Material material);

    // ======================================================
    // プライベートメソッド
    // ======================================================

    /// <summary>
    /// 現在保持している内部状態を
    /// Effect Material に反映する
    /// </summary>
    protected void ApplyToMaterial()
    {
        // Material が未設定の場合は反映を行わない
        if (_effectMaterial == null)
        {
            return;
        }

        // 派生クラス側で定義された
        // 個別のパラメータ反映処理を呼び出す
        ApplyPropertiesToMaterial(_effectMaterial);
    }
}