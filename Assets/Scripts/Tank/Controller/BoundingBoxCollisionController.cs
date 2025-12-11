// ======================================================
// BoundingBoxCollisionController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-11
// 更新日時 : 2025-12-11
// 概要     : AABB / OBB 間の衝突判定と距離計算を担当するコントローラクラス
// ======================================================

using UnityEngine;
using TankSystem.Data;

namespace TankSystem.Controller
{
    /// <summary>
    /// AABB / OBB の距離計算および衝突判定を行うクラス
    /// </summary>
    public class BoundingBoxCollisionController
    {
        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 2つのAABBが衝突しているか判定する
        /// </summary>
        /// <param name="a">AABB A</param>
        /// <param name="b">AABB B</param>
        /// <returns>衝突していれば true、していなければ false</returns>
        public bool IsColliding(in AABBData a, in AABBData b)
        {
            // X方向の重なり判定
            bool hitX = Mathf.Abs(a.Center.x - b.Center.x) <= (a.HalfSize.x + b.HalfSize.x);

            // Y方向の重なり判定
            bool hitY = Mathf.Abs(a.Center.y - b.Center.y) <= (a.HalfSize.y + b.HalfSize.y);

            // Z方向の重なり判定
            bool hitZ = Mathf.Abs(a.Center.z - b.Center.z) <= (a.HalfSize.z + b.HalfSize.z);

            // 全方向で重なりがあれば衝突
            return hitX && hitY && hitZ;
        }

        /// <summary>
        /// 2つのOBBが衝突しているか判定する
        /// </summary>
        /// <param name="a">OBB A</param>
        /// <param name="b">OBB B</param>
        /// <returns>衝突していれば true、していなければ false</returns>
        public bool IsColliding(in OBBData a, in OBBData b)
        {
            // OBBのローカル軸をワールド回転込みで取得
            Vector3[] axesA = { a.Rotation * Vector3.right, a.Rotation * Vector3.up, a.Rotation * Vector3.forward };
            Vector3[] axesB = { b.Rotation * Vector3.right, b.Rotation * Vector3.up, b.Rotation * Vector3.forward };

            // 衝突判定用の全軸
            Vector3[] testAxes = new Vector3[15];

            // AOBBとBOBBの軸を追加
            for (int i = 0; i < 3; i++)
            {
                testAxes[i] = axesA[i];
                testAxes[i + 3] = axesB[i];
            }

            // 外積による分離軸追加
            int idx = 6;
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                {
                    Vector3 cross = Vector3.Cross(axesA[i], axesB[j]);
                    // 軸の長さが0に近くない場合のみ追加
                    if (cross.sqrMagnitude > 1e-6f)
                        testAxes[idx++] = cross.normalized;
                }

            // 重なり判定
            for (int i = 0; i < idx; i++)
            {
                // 一つでも分離軸があれば衝突なし
                if (!OverlapOnAxis(a, b, testAxes[i]))
                    return false;
            }

            // 全ての軸で重なりがあれば衝突
            return true;
        }

        /// <summary>
        /// OBBとAABBが衝突しているか判定する
        /// </summary>
        /// <param name="obb">OBB</param>
        /// <param name="aabb">AABB</param>
        /// <returns>衝突していれば true、していなければ false</returns>
        public bool IsColliding(in OBBData obb, in AABBData aabb)
        {
            // AABBを回転なしOBBとして扱うことでOBB判定に統一
            OBBData aAsOBB = new OBBData(aabb.Center, aabb.HalfSize, Quaternion.identity);

            return IsColliding(aAsOBB, obb);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 指定軸に投影したときの2つのOBBの重なりを判定
        /// </summary>
        /// <param name="a">OBB A</param>
        /// <param name="b">OBB B</param>
        /// <param name="axis">判定軸</param>
        /// <returns>重なっていれば true、していなければ false</returns>
        private bool OverlapOnAxis(in OBBData a, in OBBData b, in Vector3 axis)
        {
            // 各OBBを軸に投影した半長さを取得
            float aProj = ProjectOBB(a, axis);
            float bProj = ProjectOBB(b, axis);

            // OBB中心間の距離を軸方向に投影
            float distance = Mathf.Abs(Vector3.Dot(b.Center - a.Center, axis));

            // 投影長さの合計より距離が小さければ重なりあり
            return distance <= (aProj + bProj);
        }

        /// <summary>
        /// OBBを指定軸に投影した半長さを計算する
        /// </summary>
        /// <param name="obb">OBB</param>
        /// <param name="axis">投影軸</param>
        /// <returns>投影半長さ</returns>
        private float ProjectOBB(in OBBData obb, in Vector3 axis)
        {
            // OBBのローカル軸を回転・半サイズを考慮してワールド軸ベクトルに変換
            Vector3 right = obb.Rotation * Vector3.right * obb.HalfSize.x;
            Vector3 up = obb.Rotation * Vector3.up * obb.HalfSize.y;
            Vector3 forward = obb.Rotation * Vector3.forward * obb.HalfSize.z;

            // 指定軸に投影して絶対値を合計
            return Mathf.Abs(Vector3.Dot(right, axis))
                 + Mathf.Abs(Vector3.Dot(up, axis))
                 + Mathf.Abs(Vector3.Dot(forward, axis));
        }
    }
}