// ======================================================
// TankVersusTankCollisionService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2025-12-17
// 概要     : 全戦車の OBB を参照し、
//            戦車同士の接触判定のみを一元管理するサービス
// ======================================================

using CollisionSystem.Calculator;
using CollisionSystem.Data;
using System;
using System.Collections.Generic;
using TankSystem.Data;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

namespace TankSystem.Service
{
    /// <summary>
    /// 戦車同士の OBB 衝突判定を一元管理するサービス
    /// </summary>
    public class TankVersusTankCollisionService
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>OBB / OBB の距離計算および衝突判定を行う計算器</summary>
        private readonly BoundingBoxCollisionCalculator _boxCollisionCalculator = new BoundingBoxCollisionCalculator();

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>戦車ID一覧キャッシュ配列</summary>
        private int[] _tankIdBuffer;

        /// <summary>現在キャッシュされている戦車数</summary>
        private int _cachedTankCount;
        
        // ======================================================
        // 辞書
        // ======================================================

        /// <summary>戦車IDと衝突判定エントリの対応表</summary>
        private Dictionary<int, TankCollisionEntry> _tankEntries;

        // ======================================================
        // イベント
        // ======================================================

        /// <summary>
        /// 戦車同士が接触した際に通知されるイベント
        /// </summary>
        public event Action<int, int> OnTankVersusTankHit;

        // ======================================================
        // セッター
        // ======================================================

        /// <summary>
        /// 戦車衝突判定に使用するエントリ辞書を設定し、内部で使用する戦車ID配列キャッシュを構築する
        /// </summary>
        /// <param name="tankEntries">戦車固有IDをキー、TankCollisionEntry を値として保持する辞書</param>
        public void SetTankEntries(
            in Dictionary<int, TankCollisionEntry> tankEntries
        )
        {
            // 参照を保持
            _tankEntries = tankEntries;

            // 戦車数を取得
            int tankCount = _tankEntries.Count;

            // 数が変わった場合のみ再確保
            if (_tankIdBuffer == null || _cachedTankCount != tankCount)
            {
                // 戦車ID配列を再生成
                _tankIdBuffer = new int[tankCount];

                // キャッシュ数を更新
                _cachedTankCount = tankCount;
            }

            // ID を配列にコピー
            int index = 0;
            foreach (int tankId in _tankEntries.Keys)
            {
                _tankIdBuffer[index] = tankId;
                index++;
            }
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 全戦車同士の接触判定を行い、
        /// 接触している組み合わせをイベント通知する
        /// </summary>
        public void UpdateCollisionChecks()
        {
            // 判定対象が 2 台未満なら処理しない
            if (_tankEntries.Count < 2)
            {
                return;
            }

            // --------------------------
            // 総当たり判定
            // --------------------------
            for (int i = 0; i < _cachedTankCount - 1; i++)
            {
                // 判定対象戦車AのIDを取得
                int tankIdA = _tankIdBuffer[i];

                // 戦車Aの OBB を更新
                _tankEntries[tankIdA].OBB.Update();

                for (int j = i + 1; j < _cachedTankCount; j++)
                {
                    // 判定対象戦車BのIDを取得
                    int tankIdB = _tankIdBuffer[j];

                    // 戦車Bの OBB を更新
                    _tankEntries[tankIdB].OBB.Update();

                    if (_boxCollisionCalculator.IsCollidingHorizontal(
                            _tankEntries[tankIdA].OBB,
                            _tankEntries[tankIdB].OBB))
                    {
                        OnTankVersusTankHit?.Invoke(
                            tankIdA,
                            tankIdB
                        );
                    }
                }
            }
        }

        /// <summary>
        /// 衝突している 2 台の戦車に対して MTV を用いた押し戻し量を計算する
        /// 軸ロックを最優先に考慮し、ロック軸分を相手に逆向きで付与する
        /// </summary>
        public void CalculateTankVersusTankResolveInfo(
            in TankCollisionEntry entryA,
            in TankCollisionEntry entryB,
            in float deltaForwardA,
            in float deltaForwardB,
            out CollisionResolveInfo resolveInfoA,
            out CollisionResolveInfo resolveInfoB
        )
        {
            // 初期化
            resolveInfoA = default;
            resolveInfoB = default;

            // フレーム中の軸制限を取得
            MovementLockAxis lockAxisA = entryA.TankRootManager.CurrentFrameLockAxis;
            MovementLockAxis lockAxisB = entryB.TankRootManager.CurrentFrameLockAxis;

            // OBB を最新化
            entryA.OBB.Update();
            entryB.OBB.Update();

            // MTV 算出
            if (!_boxCollisionCalculator.TryCalculateHorizontalMTV(
                entryA.OBB,
                entryB.OBB,
                out Vector3 resolveAxis,
                out float resolveDistance
            ))
            {
                return;
            }

            // 押し戻し方向補正（中心差）
            Vector3 centerDelta = entryA.OBB.Center - entryB.OBB.Center;
            centerDelta.y = 0f;
            if (Vector3.Dot(resolveAxis, centerDelta) < 0f)
            {
                resolveAxis = -resolveAxis;
            }

            // 最終押し戻し量
            Vector3 finalResolveA = Vector3.zero;
            Vector3 finalResolveB = Vector3.zero;

            // --------------------------
            // X 軸押し戻し
            // --------------------------
            if ((lockAxisA & MovementLockAxis.X) != 0)
            {
                // A がロック中なら B に押し返し
                finalResolveB.x = -resolveAxis.x * resolveDistance;
            }
            else if ((lockAxisB & MovementLockAxis.X) != 0)
            {
                // B がロック中なら A に押し返し
                finalResolveA.x = resolveAxis.x * resolveDistance;
            }
            else
            {
                // 両方動ける場合は DeltaForward による振り分け
                if (!Mathf.Approximately(deltaForwardA, 0f) && !Mathf.Approximately(deltaForwardB, 0f))
                {
                    if (Mathf.Abs(deltaForwardA) <= Mathf.Abs(deltaForwardB))
                    {
                        finalResolveA.x = resolveAxis.x * resolveDistance;
                        finalResolveB.x = 0f;
                    }
                    else
                    {
                        finalResolveA.x = 0f;
                        finalResolveB.x = -resolveAxis.x * resolveDistance;
                    }
                }
                else if (!Mathf.Approximately(deltaForwardA, 0f))
                {
                    finalResolveB.x = -resolveAxis.x * resolveDistance;
                }
                else if (!Mathf.Approximately(deltaForwardB, 0f))
                {
                    finalResolveA.x = resolveAxis.x * resolveDistance;
                }
            }

            // --------------------------
            // Z 軸押し戻し
            // --------------------------
            if ((lockAxisA & MovementLockAxis.Z) != 0)
            {
                // A が Z 軸ロック → B に押し返す
                finalResolveA.z = 0f;
                finalResolveB.z = -resolveAxis.z * resolveDistance;
            }
            else if ((lockAxisB & MovementLockAxis.Z) != 0)
            {
                // B が Z 軸ロック → A に押し返す
                finalResolveA.z = resolveAxis.z * resolveDistance;
                finalResolveB.z = 0f;
            }
            else
            {
                // 両方動ける場合は DeltaForward による振り分け
                if (!Mathf.Approximately(deltaForwardA, 0f) && !Mathf.Approximately(deltaForwardB, 0f))
                {
                    if (Mathf.Abs(deltaForwardA) <= Mathf.Abs(deltaForwardB))
                    {
                        finalResolveA.z = resolveAxis.z * resolveDistance;
                        finalResolveB.z = 0f;
                    }
                    else
                    {
                        finalResolveA.z = 0f;
                        finalResolveB.z = -resolveAxis.z * resolveDistance;
                    }
                }
                else if (!Mathf.Approximately(deltaForwardA, 0f))
                {
                    finalResolveB.z = -resolveAxis.z * resolveDistance;
                }
                else if (!Mathf.Approximately(deltaForwardB, 0f))
                {
                    finalResolveA.z = resolveAxis.z * resolveDistance;
                }
            }

            // 押し戻し量を CollisionResolveInfo に格納
            resolveInfoA = new CollisionResolveInfo
            {
                ResolveDirection = finalResolveA.normalized,
                ResolveDistance = finalResolveA.magnitude,
                IsValid = true
            };

            resolveInfoB = new CollisionResolveInfo
            {
                ResolveDirection = finalResolveB.normalized,
                ResolveDistance = finalResolveB.magnitude,
                IsValid = true
            };
        }
    }
}