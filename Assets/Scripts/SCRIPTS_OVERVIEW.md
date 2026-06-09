# mosquitoKiller — Scripts 架構文件

> 本文件自動從原始碼分析產出，涵蓋 `Assets/Scripts/` 下全部 29 個腳本。

---

## 目錄結構

```
Assets/Scripts/
├── Compoment/          # 掛在 GameObject 上的行為元件（注意：資料夾名稱拼字有誤，應為 Component）
│   ├── BaseEnemy.cs
│   ├── ClapEffect.cs
│   ├── ClapScorer.cs
│   ├── EnergyCharger.cs
│   ├── Hands.cs
│   ├── HandStateHandler.cs
│   ├── Mosquito.cs
│   ├── WorldSpaceButton.cs
│   ├── WorldSpaceButtonHover.cs
│   └── WorldSpaceToggleButton.cs
├── Core/               # 純資料型別（Enum）與核心 Singleton 管理器
│   ├── DamageSource.cs
│   ├── GamePhase.cs
│   ├── GamePhaseManager.cs
│   ├── GameState.cs
│   ├── GameStateManager.cs
│   ├── ScoreManager.cs
│   └── Skill.cs
├── Manager/            # 系統層 Singleton 管理器與設定資料
│   ├── ButtonHoverManager.cs
│   ├── EnemySpawnConfig.cs
│   ├── EnergyManager.cs
│   ├── GameManager.cs
│   ├── InputManager.cs
│   ├── MosquitoSpawner.cs
│   └── UIManager.cs
└── UI/                 # UI 面板元件
    ├── BasePanel.cs
    ├── EnergyBar.cs
    ├── GamePanel.cs
    ├── MainMenuPanel.cs
    └── ResultPanel.cs
```

---

## 子系統說明

### 1. 核心狀態管理 (`Core/`)

管理整體遊戲的狀態機與階段進程，是所有其他系統的驅動核心。

| 腳本 | 類型 | 職責 |
|------|------|------|
| `GameState.cs` | Enum | 四個遊戲狀態：`MainMenu` / `Playing` / `Paused` / `Result` |
| `GamePhase.cs` | Enum | 七個遊戲階段：`Phase1`–`Phase5` / `Boss` / `Transition` |
| `GameStateManager.cs` | Singleton MonoBehaviour | 狀態機核心；提供 `Transition(GameState)` 與 `OnStateChanged` 事件 |
| `GamePhaseManager.cs` | Singleton MonoBehaviour | 階段進程控制；提供 `NextPhase()` / `EnterPhase(GamePhase)` 與 `OnPhaseChanged` 事件；進入 Playing 時自動從 Phase1 開始計時 |

**階段流程：** Phase1 → Phase2 → Phase3 → Phase4 → Phase5 → Boss → Transition → Result

每個階段的持續時間由 `GamePhaseManager.PhaseDurations[]` 設定（Inspector 可調）。

---

### 2. 分數與能量系統

處理玩家行動後的分數累積與技能能量（EP）的增減。

| 腳本 | 類型 | 職責 |
|------|------|------|
| `Core/ScoreManager.cs` | Singleton | 持有 `Score`；提供 `Add(int)` 方法；進入 Playing 時自動歸零 |
| `Manager/EnergyManager.cs` | Singleton | 持有 `CurrentEP`；`AddEnergy(int)` 增加 EP；`TryUseSkill(int)` 扣除 EP 並發出 `OnSkillActivated` 事件；進入 Playing 時歸零 |
| `Compoment/ClapScorer.cs` | Component | 訂閱 `GameManager.OnHandClap`，每次拍手加分（預設 67 分/次） |
| `Compoment/EnergyCharger.cs` | Component | 訂閱 `GameManager.OnHandClap`，每次拍手補充 EP（預設 +10 EP/次） |

**技能費用（`EnergyManager.SkillCosts[]`）：**

| Skill 索引 | 技能名稱 | 費用 |
|-----------|---------|------|
| 0 | None | 0 |
| 1 | HadokenLeft | 30 |
| 2 | HadokenRight | 30 |
| 3 | Explosion | 50 |
| 4 | Swatter | 100 |

---

### 3. 輸入與手部追蹤

負責從 Kinect 或滑鼠讀取輸入，並轉換為遊戲內的手部位置與技能觸發。

| 腳本 | 類型 | 職責 |
|------|------|------|
| `Manager/InputManager.cs` | Singleton | 主要輸入來源；Kinect 優先，不可用時退回滑鼠；識別手勢後發出 `DetectedSkill` 事件；鍵盤 1–4 可模擬技能 |
| `Manager/GameManager.cs` | Singleton | 持有 `leftHand` / `rightHand` 位置；計算雙手距離；檢測拍手並發出 `OnHandClap` 事件；區分遊戲中（大閾值 1.5）與選單（小閾值 0.3）兩種拍手靈敏度 |
| `Compoment/Hands.cs` | Component | 每幀將 GameObject 位置同步至 `GameManager` 的手部座標（區分左右手） |
| `Compoment/HandStateHandler.cs` | Component | 根據 `GameState` 切換手部物件的 Scale：選單時 0.1（游標模式），遊戲中 0.25（玩家模式） |

**Kinect 手勢對應：**

| 手勢 | 技能 | 判斷條件 |
|------|------|---------|
| 雙臂向左交叉 | HadokenLeft | 雙肘角度 < 40° 且向左 |
| 雙臂向右交叉 | HadokenRight | 雙肘角度 < 40° 且向右 |
| 雙臂斜上舉 | Explosion | 肩膀對角線角度 < 25° |
| 左臂彎 90°，右臂上舉 | Swatter | 複合條件 |

---

### 4. 敵人系統

定義敵人的共用框架與具體實作。

| 腳本 | 類型 | 職責 |
|------|------|------|
| `Core/DamageSource.cs` | Enum | 傷害來源：`Explosion` / `HadokenLeft` / `HadokenRight` / `SkillExplotion` / `Swatter` |
| `Core/Skill.cs` | Enum | 技能列表：`None` / `HadokenLeft` / `HadokenRight` / `Explosion` / `Swatter` |
| `Compoment/BaseEnemy.cs` | Abstract MonoBehaviour | 敵人基底類別；管理 HP、死亡、得分、歸還物件池；子類別必須實作 `UpdateMovement()` |
| `Compoment/Mosquito.cs` | BaseEnemy 子類別 | 基本敵人；在螢幕內隨機遊走；HP = 1，單次拍手即死 |

**`BaseEnemy` 生命週期鉤子：**

```
OnSpawn()      ← MosquitoSpawner 生成時呼叫，可覆寫重設狀態
UpdateMovement() ← 每幀呼叫（Playing 中且存活時），子類別實作移動邏輯
OnHit(source)  ← 受傷後呼叫（死亡前），可覆寫加特效
OnDeath(source) ← 預設：加分 + 補 EP + 歸還物件池，可覆寫
KillSilent()   ← 不加分不補 EP，直接移除（過渡/清場用）
OnDespawn()    ← 歸還物件池前呼叫，可覆寫清理效果
```

---

### 5. 生成系統

管理敵人的生成節奏、物件池與生成模式。

| 腳本 | 類型 | 職責 |
|------|------|------|
| `Manager/EnemySpawnConfig.cs` | Serializable Data | 單一敵人類型的生成設定（Prefab、解鎖階段、各階段爆發數量/間隔/模式、HP 覆寫） |
| `Manager/MosquitoSpawner.cs` | Singleton | 核心生成器；管理每種敵人的爆發計時器；提供 `SpawnSingle()` / `ReturnToPool()` / `DespawnAll()`；Boss 階段生成 Boss 並停止一般生成 |

**`EnemySpawnConfig` 關鍵陣列（各索引對應一個 Phase）：**

| 屬性 | 預設值 | 說明 |
|------|--------|------|
| `BurstCountPerPhase[]` | 依設定 | 每次爆發生成幾隻 |
| `BurstIntervalPerPhase[]` | 8, 6, 4, 3, 3, 5 秒 | 每次爆發的間隔（秒） |
| `SpawnModePerPhase[]` | InsideScreen / FromEdge | 各階段生成位置模式 |
| `HPOverride` | 0（使用 Prefab 預設值） | 強制覆寫 HP |

---

### 6. 拍擊效果與世界空間按鈕

處理拍手產生的視覺效果與 UI 互動碰撞。

| 腳本 | 類型 | 職責 |
|------|------|------|
| `Compoment/ClapEffect.cs` | Component | 拍手時在雙手中點產生視覺效果與短暫 Collider；遊戲中大尺寸、選單時小尺寸；Transition 階段強制關閉 |
| `Compoment/WorldSpaceButton.cs` | Component | 被 `ClapEffect` Collider（tag = "Explosion"）觸碰時發出 `OnPressed` UnityEvent |
| `Compoment/WorldSpaceButtonHover.cs` | Component | 手部接近時變色（預設半透明藍），透過 `ButtonHoverManager` 確保互斥 Hover |
| `Compoment/WorldSpaceToggleButton.cs` | Component | 帶 Hover 效果的切換按鈕；維護 `IsOn` 狀態並切換 Sprite；發出 `OnToggleOn` / `OnToggleOff` 事件 |
| `Manager/ButtonHoverManager.cs` | Singleton | 確保同一時間只有一個按鈕顯示 Hover 狀態（互斥控制） |

**`ClapEffect` 時序：**
- 遊戲中：視覺顯示 60 幀（≈1 秒），Collider 啟用 0.1 秒
- 選單中：視覺顯示 18 幀（≈0.3 秒），Collider 啟用 0.1 秒
- 碰撞後立即停用 Collider，防止同一次拍手觸發多個按鈕

---

### 7. UI 面板 (`UI/`)

所有面板繼承 `BasePanel`，根據 `GameState` 自動顯示/隱藏。

| 腳本 | TargetState | 職責 |
|------|-------------|------|
| `UI/BasePanel.cs` | (Abstract) | 訂閱 `GameStateManager.OnStateChanged`；進入/離開 TargetState 時呼叫 `OnShow()` / `OnHide()`；初始為隱藏 |
| `UI/MainMenuPanel.cs` | MainMenu | 主選單；StartButton → 進入 Playing；DebugButton → 切換除錯顯示 |
| `UI/GamePanel.cs` | Playing | 遊戲 HUD；即時顯示分數與當前階段名稱 |
| `UI/ResultPanel.cs` | Result | 結算畫面；顯示最終分數；RetryButton → Playing；MainMenuButton → MainMenu |
| `UI/EnergyBar.cs` | — | 20 格能量條；訂閱 `EnergyManager.OnEnergyChanged` 自動更新填充狀態與數值標籤 |
| `Manager/UIManager.cs` | Singleton | 管理除錯標籤（裝置類型、手部座標、距離、技能 ID）；`SetDebugVisibility(bool)` 控制顯示 |

---

## Singleton 依賴圖

```
GameStateManager  ←────────────────────────────────────────┐
      ↑                                                      │
GamePhaseManager ──────────────────────────────────────────→┤
      ↑                                                      │
ScoreManager ───────────────────────────────────────────────┤
      ↑                                                      │
EnergyManager ──────────────────────────────────────────────┤
      ↑                                                      │
GameManager ──── InputManager ──────────────────────────────┤
      ↑                     ↖                               │
MosquitoSpawner              UIManager ─────────────────────┤
      ↑                                                      │
ButtonHoverManager ─────────────────────────────────────────┘
```

| Singleton | 直接依賴 |
|-----------|---------|
| `GameStateManager` | 無 |
| `GamePhaseManager` | GameStateManager |
| `ScoreManager` | GameStateManager |
| `EnergyManager` | GameStateManager |
| `GameManager` | InputManager, EnergyManager, GameStateManager, GamePhaseManager |
| `InputManager` | GameManager, UIManager |
| `MosquitoSpawner` | GameStateManager, GamePhaseManager, Camera.main |
| `ButtonHoverManager` | 無 |
| `UIManager` | GameStateManager, InputManager, GameManager, EnergyManager |

---

## 資料流與事件流

### 初始化鏈

```
1. GameStateManager.Awake()     → 設定初始狀態 MainMenu
2. GameManager.Awake()          → 初始化手部位置 (-2,0,0) / (2,0,0)
3. InputManager.Start()         → 嘗試初始化 Kinect；失敗則切換滑鼠模式
4. EnergyManager / ScoreManager → 訂閱 OnStateChanged，進入 Playing 時歸零
5. MosquitoSpawner.Start()      → 訂閱 OnStateChanged / OnPhaseChanged
6. BasePanel 子類別             → 訂閱 OnStateChanged；初始為 SetActive(false)
```

### 遊戲主迴圈（Playing 狀態）

```
InputManager.Update()
  ├─ UpdateKinect() 或 UpdateMouse()（每幀）
  ├─ 偵測到手勢 → DetectedSkill 事件
  │    └─ GameManager.OnSkillDetected()
  │         └─ EnergyManager.TryUseSkill(skillId)
  │              └─ OnSkillActivated 事件 → UIManager 更新除錯標籤
  └─ GameManager.UpdateHand()
       ├─ 更新 leftHand / rightHand 位置
       ├─ 計算雙手距離
       └─ 距離 < PlayingClapDistance → OnHandClap 事件
            ├─ ClapEffect.OnHandClap()   → 視覺效果 + Collider
            ├─ ClapScorer.OnHandClap()   → ScoreManager.Add(67)
            └─ EnergyCharger.OnHandClap() → EnergyManager.AddEnergy(10)
                                                └─ OnEnergyChanged → EnergyBar.Refresh()

GamePhaseManager.Update()
  └─ 計時器到期 → NextPhase()
       ├─ Phase1-5: EnterPhase(next) → OnPhaseChanged → MosquitoSpawner 更新爆發設定
       ├─ Boss: 生成 Boss，停止一般生成
       └─ Transition: DespawnAll()，2 秒後 → GameStateManager.Transition(Result)

MosquitoSpawner.Update()
  └─ 每種 EnemySpawnConfig 的爆發計時器
       └─ 到期 → SpawnBurst()
            └─ 逐隻 SpawnSingle() → BaseEnemy.OnSpawn()

BaseEnemy.Update()（每幀，Playing 中且存活）
  └─ UpdateMovement()（子類別實作）
       └─ Mosquito: 移向隨機 Waypoint；抵達後重選 Waypoint

WorldSpaceButtonHover.Update()（每幀）
  └─ IsWithinRange() → RequestHover() → ButtonHoverManager（互斥）
```

### 狀態轉換路徑

```
MainMenu ──[StartButton 拍擊]──→ Playing
Playing  ──[Transition 結束]──→ Result
Result   ──[RetryButton 拍擊]──→ Playing
Result   ──[MainMenuButton]───→ MainMenu
```

---

## 設計參數一覽

| 參數 | 預設值 | 所在腳本 | 說明 |
|------|--------|---------|------|
| `MaxEP` | 100 | EnergyManager | 最大能量上限 |
| `InitialEP` | 0 | EnergyManager | 進入 Playing 時的初始 EP |
| `SkillCosts[]` | [0, 30, 30, 50, 100] | EnergyManager | 各技能費用 |
| `PhaseDurations[]` | [5, 5, 5, …] | GamePhaseManager | 各階段持續秒數（Inspector 設定） |
| `PlayingClapDistance` | 1.5 | GameManager | 遊戲中拍手觸發距離 |
| `PlayingResetDistance` | 2.0 | GameManager | 遊戲中拍手重置距離 |
| `CursorClapDistance` | 0.3 | GameManager | 選單中拍手觸發距離 |
| `CursorResetDistance` | 0.6 | GameManager | 選單中拍手重置距離 |
| `ClapCooldown` | 0.125 秒 | GameManager | 拍手事件最短間隔（防連發） |
| `PointsPerClap` | 67 | ClapScorer | 每次拍手加分 |
| `EnergyPerClap` | 10 | EnergyCharger | 每次拍手補充 EP |
| `EdgeSpawnMargin` | 1.0 | MosquitoSpawner | FromEdge 模式生成點距螢幕外距離 |
| `InsideSpawnMargin` | 1.0 | MosquitoSpawner | InsideScreen 模式距邊緣安全距離 |
| `BossSpawnPosition` | (0, 6, 0) | MosquitoSpawner | Boss 生成位置 |
| `HoverRadius` | 1.0 | WorldSpaceButtonHover | 手部接近按鈕的 Hover 觸發距離 |
| `ColliderDuration` | 0.1 秒 | ClapEffect | ClapEffect Collider 保持啟用時長 |
| `MoveSpeed` (Mosquito) | 2.0 | Mosquito | 蚊子移動速度 |
| `EdgeMargin` (Mosquito) | 0.5 | Mosquito | 蚊子 Waypoint 距螢幕邊緣安全距離 |
