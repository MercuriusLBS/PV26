# Complete Scene Setup Checklist

## 📋 MAIN SCENE (MainViggo)

### 1. **Player GameObject**
   - **Name:** "Player"
   - **Tag:** "Player" (IMPORTANT!)
   - **Components:**
     - `Transform` (automatic)
     - `SpriteRenderer` - displays player sprite
     - `Character` component - player stats
       - Set Max Health, Attack, Character Name
       - Set all combat stats (evasion, crit chance, etc.)
     - `PlayerMovement` (or your movement script)
     - `Animator` (if using animations)
     - `Rigidbody2D` (if using physics)
     - `Collider2D` (if needed for collisions)
   - **Position:** Anywhere in the scene

---

### 2. **Enemy GameObjects** (for each enemy type)
   - **Name:** "Enemy1", "Enemy2", "Enemy3", etc.
   - **Tag:** Any tag (not "Player")
   - **Components:**
     - `Transform` (automatic)
     - `SpriteRenderer` - displays overworld enemy sprite
       - Assign the overworld sprite here
     - `Collider2D` - MUST be set as **Trigger** (`IsTrigger = true`)
       - Can be BoxCollider2D, CircleCollider2D, etc.
     - `EnemyEncounter` component
       - **Enemy Data:** Assign the corresponding EnemyData ScriptableObject
       - **Enemy ID:** (Optional) Unique identifier
       - **Can Encounter Multiple Times:** false (if enemy disappears after defeat)
       - **Require Player Tag:** true
     - `Animator` (optional, for overworld animations)
   - **Position:** Where enemies should appear in overworld

---

### 3. **EncounterManager GameObject**
   - **Name:** "EncounterManager"
   - **Tag:** Any tag
   - **Components:**
     - `Transform` (automatic)
     - `EncounterManager` component
       - **Battle Scene Name:** "Viggo turnbasedscene"
       - **Overworld Scene Name:** "MainViggo"
       - **Game Over Scene Name:** "GameOverScreen"
   - **Position:** Doesn't matter (will persist across scenes)
   - **Note:** This GameObject will persist across scenes (DontDestroyOnLoad)

---

### 4. **Camera**
   - **Name:** "Main Camera" (or any name)
   - **Components:**
     - `Camera` component
     - `Transform` (automatic)
   - **Settings:** Configure as needed for your game

---

### 5. **EventSystem** (if using UI)
   - **Name:** "EventSystem"
   - **Components:**
     - `EventSystem` component
     - `StandaloneInputModule` component
   - **Note:** Usually auto-created when adding Canvas

---

## 🎮 BATTLE SCENE (Viggo turnbasedscene)

### 1. **Player GameObject**
   - **Name:** "Player"
   - **Tag:** Any tag (not used in battle)
   - **Components:**
     - `Transform` (automatic)
     - `SpriteRenderer` - displays player battle sprite
     - `Character` component - player stats
       - Set Max Health, Attack, Character Name
       - Set all combat stats
     - `Animator` (optional, for battle animations)
   - **Position:** Where player should appear in battle (e.g., left side)
   - **Note:** This is separate from main scene player

---

### 2. **Enemy GameObject**
   - **Name:** "Enemy" (or any name)
   - **Tag:** Any tag
   - **Components:**
     - `Transform` (automatic)
     - `SpriteRenderer` component - **MUST HAVE THIS**
       - Sprite will be set automatically from EnemyData
       - Can start with a placeholder sprite
       - **Enabled:** true
     - `Character` component - **MUST HAVE THIS**
       - Stats will be configured automatically from EnemyData
       - Can leave default values
     - `Animator` component (optional, for battle animations)
       - Animator Controller will be set automatically from EnemyData
       - Can start with a default controller
   - **Position:** Where enemy should appear in battle (e.g., right side)
   - **Active:** Must be checked (enabled)
   - **Note:** This is the enemy that gets configured based on which enemy you collided with

---

### 3. **BattleManager GameObject**
   - **Name:** "BattleManager"
   - **Tag:** Any tag
   - **Components:**
     - `Transform` (automatic)
     - `Battlemanager` component
       - **Player:** Drag the Player GameObject from scene
       - **Enemy:** Drag the Enemy GameObject from scene
       - **Turn Delay:** 1 (seconds between turns)
   - **Position:** Doesn't matter

---

### 4. **BattleUI GameObject**
   - **Name:** "BattleUI"
   - **Tag:** Any tag
   - **Components:**
     - `Transform` (automatic)
     - `BattleUI` component
       - **Player Health Bar:** Slider component
       - **Player Health Text:** TextMeshProUGUI
       - **Player Name Text:** TextMeshProUGUI
       - **Enemy Health Bar:** Slider component
       - **Enemy Health Text:** TextMeshProUGUI
       - **Enemy Name Text:** TextMeshProUGUI
       - **Main Action Menu Panel:** GameObject (Panel)
       - **Abilities Button:** Button component
       - **Abilities Submenu Panel:** GameObject (Panel)
       - **Attack Button:** Button component
       - **Special Attack Button:** Button component
       - **Defense Button:** Button component
       - **Back Button:** Button component
       - **Battle Log Text:** TextMeshProUGUI
       - **Battle Log Display Time:** 2 (seconds)

---

### 5. **Canvas** (for UI)
   - **Name:** "Canvas"
   - **Tag:** Any tag
   - **Components:**
     - `RectTransform` (automatic for Canvas)
     - `Canvas` component
       - **Render Mode:** Screen Space - Overlay (or Screen Space - Camera)
     - `CanvasScaler` component
       - **UI Scale Mode:** Scale With Screen Size
       - **Reference Resolution:** 1920x1080 (or your preferred resolution)
     - `GraphicRaycaster` component
   - **Children:**
     - All UI elements (health bars, buttons, text, panels)

---

### 6. **UI Elements Structure** (under Canvas)

#### **Player UI Section:**
   - **Player Health Bar** (Slider)
     - Min Value: 0
     - Max Value: 1
     - Value: 1 (full health)
   - **Player Health Text** (TextMeshProUGUI)
     - Text: "100 / 100" (placeholder)
   - **Player Name Text** (TextMeshProUGUI)
     - Text: "Player" (placeholder)

#### **Enemy UI Section:**
   - **Enemy Health Bar** (Slider)
     - Min Value: 0
     - Max Value: 1
     - Value: 1 (full health)
   - **Enemy Health Text** (TextMeshProUGUI)
     - Text: "100 / 100" (placeholder)
   - **Enemy Name Text** (TextMeshProUGUI)
     - Text: "Enemy" (placeholder)

#### **Main Action Menu Panel:**
   - **Panel** (GameObject)
     - **Abilities Button** (Button)
       - Text: "Abilities"

#### **Abilities Submenu Panel:**
   - **Panel** (GameObject)
     - Initially inactive (will be shown when Abilities is clicked)
     - **Attack Button** (Button)
       - Text: "Attack"
     - **Special Attack Button** (Button)
       - Text: "Special Attack"
     - **Defense Button** (Button)
       - Text: "Defense"
     - **Back Button** (Button)
       - Text: "Back"

#### **Battle Log:**
   - **Battle Log Text** (TextMeshProUGUI)
     - Text: "" (empty initially)

---

### 7. **EventSystem** (for UI)
   - **Name:** "EventSystem"
   - **Components:**
     - `EventSystem` component
     - `StandaloneInputModule` component
   - **Note:** Usually auto-created with Canvas

---

### 8. **Camera**
   - **Name:** "Main Camera" (or any name)
   - **Components:**
     - `Camera` component
     - `Transform` (automatic)
   - **Settings:** Configure to see player and enemy in battle

---

## 📦 ENEMYDATA SCRIPTABLEOBJECTS

### For Each Enemy Type (Enemy1Data, Enemy2Data, Enemy3Data, etc.):

1. **Create Asset:**
   - Right-click in Project → Create → Scriptable Objects → EnemyData
   - Name it (e.g., "SlimeData", "GoblinData")

2. **Basic Info:**
   - **Enemy Name:** "Slime" (or enemy name)

3. **Visuals:**
   - **Enemy Sprite:** Assign the BATTLE sprite (different from overworld)
   - **Enemy Animator Controller:** Assign the BATTLE animator controller (optional)

4. **Stats:**
   - **Max Health:** 100 (or desired value)
   - **Attack:** 10 (or desired value)

5. **Combat Stats:**
   - **Evasion Chance:** 20 (0-100)
   - **Critical Hit Chance:** 15 (0-100)
   - **Critical Hit Multiplier:** 2.0
   - **Damage Variance:** 0.2 (20% variance)

6. **Special Attack:**
   - **Special Attack Multiplier:** 2.0
   - **Special Attack Accuracy:** 50 (0-100)

7. **Defense:**
   - **Guard Damage Reduction:** 0.85 (85% reduction, 0-1)

---

## ⚙️ BUILD SETTINGS

### Scenes in Build Settings:
1. **MainViggo** (overworld scene)
2. **Viggo turnbasedscene** (battle scene)
3. **GameOverScreen** (game over scene)

**To add:**
- File → Build Settings
- Drag scenes from Project to "Scenes In Build"
- Order: MainViggo (index 0), then others

---

## 🔗 CONNECTION CHECKLIST

### Main Scene:
- ✅ Player has Tag "Player"
- ✅ EnemyEncounter components have EnemyData assigned
- ✅ Enemy colliders are set to IsTrigger = true
- ✅ EncounterManager has correct scene names

### Battle Scene:
- ✅ BattleManager has Player GameObject assigned
- ✅ BattleManager has Enemy GameObject assigned
- ✅ Enemy GameObject has SpriteRenderer component
- ✅ Enemy GameObject has Character component
- ✅ BattleUI has all UI elements assigned
- ✅ All buttons are children of their respective panels

### EnemyData Assets:
- ✅ Each EnemyData has a battle sprite assigned
- ✅ Each EnemyData has stats configured
- ✅ Overworld enemies have correct EnemyData assigned

---

## 🎯 QUICK VERIFICATION

**Test this flow:**
1. Start game in MainViggo scene
2. Walk into Enemy1 → Should load battle scene
3. Battle scene should show:
   - Enemy with Enemy1's sprite
   - Enemy with Enemy1's stats
   - UI showing health bars
   - Action menu with Abilities button
4. Click Abilities → Should show submenu
5. Click Attack → Should deal damage
6. Win battle → Should return to MainViggo
7. Enemy1 should be gone (if canEncounterMultipleTimes = false)

---

## 🐛 COMMON ISSUES

1. **Enemy not visible in battle:**
   - Check Enemy GameObject has SpriteRenderer
   - Check EnemyData has sprite assigned
   - Check enemy GameObject is active
   - Check enemy position is visible to camera

2. **Collision not working:**
   - Check Player has Tag "Player"
   - Check enemy Collider2D IsTrigger = true
   - Check EnemyEncounter has EnemyData assigned

3. **Wrong enemy in battle:**
   - Check EnemyData assets have correct sprites
   - Check overworld enemies have correct EnemyData assigned

4. **UI not working:**
   - Check EventSystem exists in scene
   - Check all UI elements are assigned in BattleUI component
   - Check Canvas is set up correctly
