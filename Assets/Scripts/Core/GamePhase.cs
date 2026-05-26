public enum GamePhase
{
    Phase1,     // 普通蚊子登場
    Phase2,     // 第二種敵人解鎖
    Phase3,     // 第三種敵人解鎖
    Phase4,     // 第四種敵人解鎖
    Phase5,     // 第五種敵人解鎖
    Boss,       // Boss 登場，舊敵人仍持續出現
    Transition  // Boss 後的轉場，約 2 秒，禁止拍手與生成，顯示結果畫面後切換至 Result
}
