// ======================================================
// SoundManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-02-15
// 更新日時 : 2026-02-15
// 概要     : サウンド管理クラス
// ======================================================

using System;
using UnityEngine;

namespace SoundSystem.Manager
{
    /// <summary>
    /// サウンドを管理するクラス
    /// </summary>
    public sealed class SoundManager : MonoBehaviour
    {
        // ======================================================
        // プライベートクラス
        // ======================================================
        /// <summary>
        /// BGM用オーディオソースとクリップのセット
        /// </summary>
        [Serializable]
        private class BgmSet
        {
            /// <summary>BGM用オーディオソース</summary>
            public AudioSource Source;

            /// <summary>BGMクリップ</summary>
            public AudioClip Clip;
        }

        // ======================================================
        // シングルトン
        // ======================================================

        /// <summary>シングルトンインスタンス</summary>
        public static SoundManager Instance { get; private set; }

        // ======================================================
        // 列挙体
        // ======================================================

        /// <summary>フェードタイプ</summary>
        public enum FadeType
        {
            FadeIn,
            FadeOut
        }

        // ======================================================
        // インスペクタ設定
        // ======================================================

        [Header("BGM 設定")]
        /// <summary>BGM用 オーディオソース</summary>
        [SerializeField] private BgmSet[] _bgmSets;

        /// <summary>BGM フェードにかかる時間（秒）</summary>
        [SerializeField] private float _fadeDuration = 1.5f;

        /// <summary>ローパスフィルター ON 時の目標周波数</summary>
        [SerializeField] private float _lowPassTargetFrequency = 500f;

        /// <summary>ローパスフィルター補間時間（秒）</summary>
        [SerializeField] private float _lowPassTransition = 1.0f;

        [Header("SE 設定")]
        /// <summary>SE 用オーディオソース</summary>
        [SerializeField] private AudioSource _seSource;

        /// <summary>SE クリップリスト</summary>
        [SerializeField] private AudioClip[] _seClips;

        /// <summary>SE再生距離の最大値</summary>
        [SerializeField] private float _seMaxDistance = 100f;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>SE に使用するサウンドリスナー基準 Transform</summary>
        private Transform _listenerTransform;
        
        /// <summary>BGM オーディオソースにアタッチされた AudioLowPassFilter 配列</summary>
        private AudioLowPassFilter[] _lowPassFilters;

        /// <summary>BGMセットごとのフェード中フラグ</summary>
        private bool[] _isFadingArray;

        /// <summary>BGMセットごとのフェード開始音量</summary>
        private float[] _fadeStartVolumeArray;

        /// <summary>BGMセットごとのフェード目標音量</summary>
        private float[] _fadeTargetVolumeArray;

        /// <summary>BGMセットごとのフェード経過時間（秒）</summary>
        private float[] _fadeElapsedArray;

        /// <summary>ローパス補間中フラグ</summary>
        private bool _isLowPassActive = false;

        /// <summary>ローパス補間開始時の周波数</summary>
        private float _lowPassStartFreq;

        /// <summary>ローパス補間目標周波数</summary>
        private float _lowPassTargetFreq;

        /// <summary>ローパス補間経過時間（秒）</summary>
        private float _lowPassElapsed;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>BGM ローパスフィルター OFF 時の最大周波数</summary>
        private const float MAX_LOW_PASS_FREQUENCY = 22000f;

        // ======================================================
        // Unity イベント
        // ======================================================

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // LowPassFilter 配列初期化
            if (_bgmSets != null && _bgmSets.Length > 0)
            {
                _lowPassFilters = new AudioLowPassFilter[_bgmSets.Length];

                for (int i = 0; i < _bgmSets.Length; i++)
                {
                    if (_bgmSets[i].Source == null)
                    {
                        continue;
                    }

                    _lowPassFilters[i] = _bgmSets[i].Source.GetComponent<AudioLowPassFilter>();
                }
            }

            // BGM セット数に応じてフェード管理配列を初期化
            if (_bgmSets != null)
            {
                int count = _bgmSets.Length;
                _isFadingArray = new bool[count];
                _fadeStartVolumeArray = new float[count];
                _fadeTargetVolumeArray = new float[count];
                _fadeElapsedArray = new float[count];
            }
        }

        private void Update()
        {
            if (_bgmSets == null)
            {
                return;
            }

            // --------------------------------------------------
            // BGM フェード更新
            // 各 BGM セットごとに個別の値を適用
            // --------------------------------------------------
            for (int i = 0; i < _bgmSets.Length; i++)
            {
                BgmSet bgm = _bgmSets[i];
                if (bgm.Source == null)
                {
                    continue;
                }

                // このBGMセットがフェード中か確認
                if (_isFadingArray[i])
                {
                    // 経過時間を加算
                    _fadeElapsedArray[i] += Time.deltaTime;

                    // 0〜1の補間係数に変換
                    float t = Mathf.Clamp01(_fadeElapsedArray[i] / _fadeDuration);

                    // フェード補間
                    bgm.Source.volume = Mathf.Lerp(_fadeStartVolumeArray[i], _fadeTargetVolumeArray[i], t);

                    // フェード完了時にフラグを解除
                    if (t >= 1f)
                    {
                        _isFadingArray[i] = false;
                    }
                }
            }

            // --------------------------------------------------
            // ローパス補間更新
            // 全 BGM に同じローパス値を適用
            // --------------------------------------------------
            if (_isLowPassActive && _lowPassFilters != null)
            {
                // 経過時間を加算
                _lowPassElapsed += Time.deltaTime;

                // 補間係数
                float t = (_lowPassTransition > 0f)
                    ? Mathf.Clamp01(_lowPassElapsed / _lowPassTransition)
                    : 1f;

                // 補間値計算
                float cutoff = Mathf.Lerp(_lowPassStartFreq, _lowPassTargetFreq, t);

                // 全フィルターに適用
                for (int i = 0; i < _lowPassFilters.Length; i++)
                {
                    AudioLowPassFilter filter = _lowPassFilters[i];

                    if (filter == null)
                    {
                        continue;
                    }

                    filter.cutoffFrequency = cutoff;
                }

                // 補間完了
                if (t >= 1f)
                {
                    _isLowPassActive = false;
                }
            }
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        // --------------------------------------------------
        // リスナー
        // --------------------------------------------------
        /// <summary>
        /// サウンドリスナーの基準 Transform を設定
        /// </summary>
        /// <param name="listener">リスナーとなるTransform</param>
        public void SetListenerTransform(in Transform listener)
        {
            _listenerTransform = listener;
        }

        /// <summary>
        /// サウンドリスナーの基準 Transform をリセット
        /// </summary>
        public void ResetListenerTransform()
        {
            _listenerTransform = null;
        }

        // --------------------------------------------------
        // BGM
        // --------------------------------------------------
        /// <summary>
        /// 指定インデックスの BGM を再生
        /// </summary>
        /// <param name="index">BgmSets 配列のインデックス</param>
        public void PlayBGM(in int index)
        {
            if (_bgmSets == null || index < 0 || index >= _bgmSets.Length)
            {
                return;
            }

            BgmSet bgm = _bgmSets[index];

            // Source や Clip が未設定なら再生なし
            if (bgm.Source == null || bgm.Clip == null)
            {
                return;
            }

            // Clip を設定して再生
            bgm.Source.clip = bgm.Clip;
            bgm.Source.Play();
        }

        /// <summary>
        /// BGM 停止
        /// </summary>
        /// <param name="index">停止する BGM インデックス、-1 なら全 BGM 停止</param>
        public void StopBGM(in int index = -1)
        {
            if (_bgmSets == null)
            {
                return;
            }

            // 全 BGM 停止
            if (index == -1)
            {
                foreach (BgmSet bgm in _bgmSets)
                {
                    bgm.Source?.Stop();
                }
            }
            else
            {
                // インデックスチェック
                if (index < 0 || index >= _bgmSets.Length)
                {
                    return;
                }

                // 指定BGM停止
                _bgmSets[index].Source?.Stop();
            }
        }

        /// <summary>
        /// BGM フェード開始
        /// </summary>
        /// <param name="index">フェード対象の BGM インデックス</param>
        /// <param name="fadeType">フェードタイプ（In / Out）</param>
        public void FadeBGM(in int index, in FadeType fadeType)
        {
            if (_bgmSets == null || index < 0 || index >= _bgmSets.Length)
            {
                return;
            }
            BgmSet bgm = _bgmSets[index];

            if (bgm.Source == null)
            {
                return;
            }

            // フェード情報を個別配列に設定
            _fadeStartVolumeArray[index] = bgm.Source.volume;
            _fadeTargetVolumeArray[index] = (fadeType == FadeType.FadeIn) ? 1f : 0f;
            _fadeElapsedArray[index] = 0f;
            _isFadingArray[index] = true;
        }

        /// <summary>
        /// 指定インデックスの BGM ボリュームを設定
        /// </summary>
        /// <param name="index">BgmSets 配列のインデックス</param>
        /// <param name="volume">指定する音量ボリューム</param>
        public void SetBGMVolume(in int index, in float volume)
        {
            if (_bgmSets == null || index < 0 || index >= _bgmSets.Length)
            {
                return;
            }

            BgmSet bgm = _bgmSets[index];

            // Source が未設定なら再生なし
            if (bgm.Source == null)
            {
                return;
            }

            // ボリュームを設定
            bgm.Source.volume = volume;
        }

        /// <summary>
        /// ローパスフィルター ON / OFF
        /// </summary>
        /// <param name="enable">ON なら true、OFF なら false</param>
        public void SetBgmLowPass(in bool enable)
        {
            if (_lowPassFilters == null)
            {
                return;
            }

            // 補間開始値を現在の周波数で設定
            if (_lowPassFilters.Length > 0 && _lowPassFilters[0] != null)
            {
                _lowPassStartFreq = _lowPassFilters[0].cutoffFrequency;
            }

            // 補間目標値を設定
            _lowPassTargetFreq = enable ? _lowPassTargetFrequency : MAX_LOW_PASS_FREQUENCY;

            // 補間開始
            _lowPassElapsed = 0f;
            _isLowPassActive = true;
        }

        // --------------------------------------------------
        // SE
        // --------------------------------------------------
        /// <summary>
        /// SE 再生
        /// </summary>
        /// <param name="index">seClips 配列のインデックス</param>
        /// <param name="position">音発生位置、null ならリスナー位置扱い</param>
        public void PlaySE(in int index, in Vector3? position = null)
        {
            if (_seSource == null || _seClips == null || index < 0 || index >= _seClips.Length)
            {
                return;
            }

            // 音発生位置なしの場合、最大音量で SE 再生
            if (position == null)
            {
                _seSource.PlayOneShot(_seClips[index], 1f);
                return;
            }

            // リスナー位置取得
            Vector3 listenerPos = _listenerTransform != null
                ? _listenerTransform.position
                : Vector3.zero;

            // 距離計算
            float distance = Vector3.Distance(listenerPos, position.Value);

            // 最大距離外は再生なし
            if (distance > _seMaxDistance)
            {
                return;
            }

            // 距離による音量補正
            float volume = 1f - (distance / _seMaxDistance);

            // SE 再生
            _seSource.PlayOneShot(_seClips[index], volume);
        }
    }
}