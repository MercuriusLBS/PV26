# Popup Manager – Step-by-Step Setup Tutorial

This guide walks you through adding the popup UI and wiring it to the **PopupManager** so that:

1. **First defeat popup** – After you defeat an enemy for the first time and return to the overworld, a text bubble explains that enemies drop Whiskers.
2. **10 Whiskers popup** – When you reach 10 Whiskers (from battle rewards), a text bubble congratulates you.

---

## Prerequisites

- Your project has **EncounterManager** (e.g. in the overworld scene) and **BattleLootDropper** (on the same GameObject as Battlemanager in the battle scene).
- **PopupManager.cs** is in the project and the scripts **EncounterManager** and **BattleLootDropper** are already wired to call it (no extra code needed).

---

## Step 1: Choose where to put the Popup UI

The popup must be visible in **both** the overworld and the battle scene. You have two options:

- **Option A (recommended):** Put the PopupManager (and its UI) on a **persistent** GameObject that exists in your **first-loaded scene** (e.g. overworld or a bootstrap scene). The script uses `DontDestroyOnLoad`, so it will persist when you load the battle scene.
- **Option B:** Create a dedicated **Popup** canvas in the overworld scene and add the PopupManager to the same GameObject as **EncounterManager**. Because EncounterManager already uses `DontDestroyOnLoad`, that GameObject (and its children) will persist — so add the popup UI as a **child** of that GameObject.

In this tutorial we assume **Option B**: you already have an EncounterManager GameObject. We will add a Canvas and popup UI as siblings or under a child, then add PopupManager to the same GameObject as EncounterManager (or to a child that stays with it).

---

## Step 2: Create the Popup Canvas (if you don’t have one)

1. In the **Hierarchy**, right-click → **UI** → **Canvas**.
2. Name it e.g. **PopupCanvas**.
3. Set the Canvas **Render Mode** to **Screen Space - Overlay** (default).
4. Optional: Add a **Canvas Scaler** component and set **UI Scale Mode** to **Scale With Screen Size**, **Reference Resolution** e.g. 1920×1080, **Match** 0.5.
5. To keep this canvas when loading other scenes, make it a **child of the GameObject that has EncounterManager** (so it stays with the DontDestroyOnLoad object).  
   - If EncounterManager is on e.g. **GameManager**: drag **PopupCanvas** onto **GameManager** in the Hierarchy.

---

## Step 3: Create the Popup Panel

1. Right-click **PopupCanvas** → **UI** → **Panel**. Name it **PopupPanel**.
2. Select **PopupPanel** and in the **RectTransform**:
   - Set **Anchor** to middle (e.g. center-middle).
   - Set **Pos X, Pos Y** to 0, **Width** e.g. 600, **Height** e.g. 220.
   - Or use **Anchor Presets** → hold Alt and click the “center” preset to center it, then set width/height.
3. In the **Image** component of the panel, set **Color** to a semi-transparent dark (e.g. black with Alpha 230) so text is readable.
4. Optional: Add a **Outline** or **Shadow** for a “bubble” look.

---

## Step 4: Add the Message Text (TextMeshPro)

1. Right-click **PopupPanel** → **UI** → **Text - TextMeshPro** (if asked to import TMP essentials, click **Import**).
2. Name the new object **MessageText**.
3. Select **MessageText**:
   - **RectTransform**: Stretch to fill the panel with a small margin (e.g. Left 20, Right 20, Top 20, Bottom 50 so there’s room for a button).
   - **TextMeshPro - Text**:
     - **Font Size** e.g. 24–28.
     - **Alignment** Horizontal and Vertical: Center (or Left + Top if you prefer).
     - **Wrapping** enabled (e.g. **Auto**).
     - **Color** white (or your preferred text color).
   - Set the default **Text** in the Inspector to something like “Message” (it will be replaced by script).

---

## Step 5: Add the Continue Button (optional but recommended)

1. Right-click **PopupPanel** → **UI** → **Button - TextMeshPro** (or **Button** and add a Text child).
2. Name it **ContinueButton**.
3. **RectTransform**: Anchor to bottom-center of the panel, e.g. **Width** 160, **Height** 36, **Pos Y** 25.
4. Set the button’s **Text** child to e.g. “Continue” or “OK”.
5. When the button is assigned in PopupManager, the popup will close on click. If you don’t assign a button, the popup will auto-close after the **Display Duration** set in PopupManager.

---

## Step 6: Add the PopupManager script and assign references

1. Select the **GameObject that has EncounterManager** (the one that persists across scenes, e.g. **GameManager**).
2. **Add Component** → search for **Popup Manager** → add it.
3. In the **Popup Manager** component:
   - **Popup Panel**: Drag **PopupPanel** from the Hierarchy (the Panel you created).
   - **Message Text**: Drag **MessageText** (the TextMeshPro under the panel).
   - **Continue Button**: Drag **ContinueButton** (leave empty for auto-close only).
   - **Display Duration**: e.g. 4 (seconds). Only used if **Continue Button** is empty.
   - **First Defeat Whisker Message**: Edit the text that appears after the first victory (default explains that enemies drop Whiskers).
   - **Ten Whiskers Message**: Edit the text that appears when the player reaches 10 Whiskers.

4. **Important:** The GameObject that has both **EncounterManager** and **PopupManager** must be in the **first scene you run** (e.g. overworld). That way it (and the popup canvas) will persist into the battle scene.

---

## Step 7: Make sure the Popup starts hidden

- **PopupManager** turns off **PopupPanel** in `Awake()`, so the panel should start **active** in the scene; the script will hide it on start.  
- If you prefer the panel to start disabled: select **PopupPanel** and **uncheck** the checkbox at the top of the Inspector so it’s inactive. Then in **PopupManager.Awake()** we only call `SetActive(false)` when `popupPanel != null`, so it will still work. Either way is fine.

---

## Step 8: Test the popups

1. **First defeat popup**
   - Start the game in the overworld (where EncounterManager + PopupManager live).
   - Defeat an enemy in battle and return to the overworld.
   - The first time you do this, the whisker info popup should appear (after position/health restore).
2. **10 Whiskers popup**
   - Win enough battles so your total Whiskers reach 10 (e.g. 5 battles with 2 whiskers each, or adjust **Whisker Quantity** in BattleLootDropper for testing).
   - The 10-whiskers message should appear once, right after the battle where you cross 10.

If you don’t see the popup:
- Confirm **Popup Panel** and **Message Text** are assigned on PopupManager.
- Confirm the EncounterManager/PopupManager GameObject is in the scene that loads first and uses **DontDestroyOnLoad** (so it exists in both overworld and battle).

---

## Summary checklist

- [ ] Canvas (e.g. PopupCanvas) exists and is under the persistent GameObject (e.g. with EncounterManager).
- [ ] PopupPanel (Panel with background) exists under the canvas.
- [ ] MessageText (TextMeshPro) is a child of PopupPanel and has a visible font/size/color.
- [ ] Optional: ContinueButton is a child of PopupPanel.
- [ ] PopupManager component is on the same persistent GameObject as EncounterManager.
- [ ] Popup Manager’s **Popup Panel**, **Message Text**, and optionally **Continue Button** are assigned.
- [ ] Game is run from the scene that contains the EncounterManager/PopupManager GameObject.

After this, the two popups will show automatically at the right moments.
