namespace WeaponSystem.Interface
{
    /// <summary>
    /// 弾丸動作を統一するインターフェース
    /// </summary>
    public interface IBullet
    {
        /// <summary>弾丸がアクティブかどうか</summary>
        bool IsEnabled { get; set; }

        /// <summary>弾丸がプールから有効化されたときに呼ばれる</summary>
        void OnEnter();

        /// <summary>弾丸のフレーム更新</summary>
        void OnUpdate(float deltaTime);

        /// <summary>弾丸がプールへ戻るときに呼ばれる</summary>
        void OnExit();
    }
}