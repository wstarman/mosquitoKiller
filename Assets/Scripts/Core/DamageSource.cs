// 傷害來源，供 BaseEnemy.OnHit / OnDeath 判斷，組員可依此實作不同死亡行為
public enum DamageSource
{
    Explosion,      // 拍手觸發的 Explotion 碰撞（徒手）
    HadokenLeft,    // 技能：往左波動拳
    HadokenRight,   // 技能：往右波動拳
    SkillExplotion, // 技能：雙臂斜舉大爆炸
    Swatter         // 技能：電蚊拍
}
