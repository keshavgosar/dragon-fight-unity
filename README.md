# Dragon Fight

A small 2.5D top-down battle between two dragons, in the style of TFT / Dota Underlords. You control one dragon with **WASD** and three abilities; the other is controlled by an AI that chases you, picks abilities by distance, and obeys the same cooldowns. Built in Unity for the Dexhigh Services Junior Unity Developer technical assessment.

| | |
|---|---|
| **Gameplay video** | `https://youtu.be/HlRTWHrr8dQ` |
| **Windows build** | `https://drive.google.com/file/d/12NhwHD64GAuYK2C0VAa78CxRa_Q6l6cP/view?usp=sharing` |
| **Unity version** | **Unity 6.4 (6000.4.1f1)** |

---

## Features at a glance

- Enclosed arena with physical walls plus a code-level clamp, so neither dragon can leave the play area
- Angled top-down camera that smoothly follows and zooms to keep both dragons in view
- Three distinct abilities (Fire Breath, Tail Whip, Sky Strike) with damage, cooldown, animation, VFX and sound
- State-machine enemy AI (Idle, Chase, Attack) that fights back and uses the same cooldowns as the player
- Health bars with a trailing damage bar, ability icons with radial cooldown and countdown
- Hit feedback: colour flash, floating damage numbers, hit particles, camera shake, hit sound, knockback
- Winner Screen naming the victor, with Restart button (and `R` key)
- Background music, win and lose jingles, and a UI click sound

---

## Controls

| Input | Action |
|---|---|
| **W A S D** | Move (WASD was chosen over click-to-move) |
| **1** | Fire Breath: ranged cone of fire in front of the dragon |
| **2** | Tail Whip: close-range spinning tail attack with knockback |
| **3** | Sky Strike: take off, glide over the enemy, land with an area slam |
| **R** / Restart button | Restart after the Winner Screen |

Abilities automatically face the opponent when cast (light aim assist), so no mouse aiming is needed. Key presses are buffered for 0.3 s, so pressing an ability during another's recovery still fires the moment the dragon is free. That keeps the controls responsive.

---

## Abilities

Values below are the defaults and are all editable in the Inspector (`DragonCombat`).

| Ability | Key | Damage | Cooldown | Range | Behaviour |
|---|---|---|---|---|---|
| **Fire Breath** | 1 | 36 total (6 ticks) | 5 s | 8 | 50-degree cone in front, fire particles that follow the head animation |
| **Tail Whip** | 2 | 22 | 2.5 s | 3.5 | Body spins 360 degrees so the tail sweeps a full circle, with knockback |
| **Sky Strike** | 3 | 40 | 9 s | 14 | Takes off, glides over the enemy, lands with a radius-4 slam, knockback and camera shake |

Each dragon has 200 health. The player and the AI share the **same** `DragonCombat` component, so damage values and cooldowns are identical for both.

---

## Enemy AI

A deliberately simple, readable state machine in `EnemyAI.cs`:

- **Idle**: waits a short delay at the start of the fight.
- **Chase**: runs toward the player until inside fire range.
- **Attack**: every 0.3 s picks an ability by distance:
  - close range: **Tail Whip**
  - mid range: **Fire Breath**
  - far but within range: **Sky Strike** to close the gap

  If nothing is off cooldown it strafes around the player and holds a preferred distance, so it never stands still. If the player moves too far away it goes back to Chase.

The AI calls the same `TryUse(index)` method as the player, so it respects the same cooldowns.

---

## Project structure

```
Assets/
  Scripts/
    Core/     Health, GameManager (win / restart), GameAudio, CameraRig, ArenaBounds, Sfx
    Combat/   Ability (data), DragonCombat, DragonMotor, PlayerController, FollowBone
    AI/       EnemyAI
    UI/       HealthBarUI, AbilityIconUI, DamagePopup, HitFeedback, UIClickSound
  Animation/  DragonCTRL (animator controller) + override controller for the second dragon
  Prefabs/    DamagePopup, HitVFX
  Audio/      music, win / lose jingles, sound effects
  Scenes/     SampleScene (main battle scene)
```

### Design decisions

- **Event-driven health.** `Health` raises `Changed`, `Damaged` and `Died`. The UI, hit feedback, audio and GameManager just subscribe, so nothing polls health every frame.
- **One combat class for both dragons.** Player and AI only decide *when* to call `DragonCombat.TryUse()`. This guarantees identical rules.
- **Abilities are data.** The `Ability` class holds damage, cooldown, range, timing, VFX, sound and icon, so tuning needs no code changes.
- **Gameplay logic does not depend on animation clips.** Damage is applied from timed coroutines, not animation events. A missing or poor clip can never break an ability (see the tail attack story below).
- **Movement uses velocity plus decaying knockback** in `DragonMotor`, so hits push the dragon physically and the arena walls still stop it.

---

## How to run

1. Open the project in **Unity 6.4 (6000.4.1f1)**.
2. Project Settings, Player, **Active Input Handling = Both** (the scripts use the classic `Input` class).
3. Open `Assets/Scenes/SampleScene.unity` and press Play.

To make a Windows build: File, Build Profiles (Build Settings), Windows, Build.

---

## Asset sources

Only free assets were used. No paid or ripped assets.

| Used for | Asset | Source |
|---|---|---|
| Arena / environment | Low Poly Gladiators Arena | https://assetstore.unity.com/packages/3d/environments/fantasy/low-poly-gladiators-arena-167116 |
| Fire and particle effects | Free StylizedVFX Fire Pack | https://assetstore.unity.com/packages/vfx/particles/fire-explosions/free-stylizedvfx-fire-pack-321082 |
| Dragon models and animations | Dragon for Boss Monster PBR (two of the four dragons used) | https://assetstore.unity.com/packages/3d/characters/creatures/dragon-for-boss-monster-pbr-78923 |
| Sound effects, background music, win / lose jingles | Pixabay free sound effects (Pixabay Content License) and sounds created with the help of Claude | https://pixabay.com/sound-effects/ |

---

# AI Usage Note

## Which AI tools I used
**Claude (Anthropic)** was the main tool, used through the chat interface. Some sound effects were also made with Claude's help (see asset sources).

## What I used it for

- **Planning:** breaking the brief into scripts, choosing an event-driven health design and a shared combat class for player and AI.
- **Coding:** first drafts of all gameplay and UI scripts (health, motor, combat, AI, camera, health bars, ability cooldown icons, damage popups, hit feedback, game manager, audio).
- **Unity setup guidance:** step-by-step help with the animator controller, UI canvas, particle effects and audio import settings, based on screenshots of my project.
- **Debugging:** diagnosing problems from screenshots and Console messages (listed below).
- **Research and documentation:** camera and UI conventions from TFT / Dota Underlords, and this README.

I wired everything up in the editor myself, tuned the values, tested the game, and made the final decisions about what to keep.

## Where the AI got something wrong, and how I fixed it

### 1. The Tail Attack animation (the AI misunderstood my setup)

**What went wrong.** The AI planned the animator around the clips listed in the asset pack's demo controller, including a *Tail Attack* state, and assumed that every dragon in the pack shared the same set of clips. In reality the Tail Attack clip belongs to a **different dragon** in the pack, not the two dragons I had chosen because they looked best. On my dragons the Tail Attack state had no usable animation, so pressing **2** looked like it did nothing and the ability seemed broken. The AI's first suggestion treated it as a setup problem, but the real cause was a wrong assumption about which clips exist.

**How I diagnosed it.** I realised the damage code never depended on the animation (it is applied from a coroutine in `DragonCombat`), so the ability itself was fine and only the visual was missing. I then checked the other dragon's clip on my rig and it did not play correctly, because the two rigs do not match.

**How I fixed it.**
1. In the animator controller, I replaced the empty state's clip with a clip my dragons do have (their own Claw / Basic Attack) and renamed the state *Tail Whip*.
2. I changed `TailRoutine` in `DragonCombat.cs` to a **procedural tail whip**: the dragon's visual model spins 360 degrees over about 0.5 s, so the tail sweeps around, and damage plus knockback is applied in a full circle halfway through the spin.
3. I stored the model's original rotation and restored it after the spin and on death so a dragon is never left turned the wrong way.
4. I kept a `spinForTail` toggle, so if a proper tail clip is ever available the old animation-based version can be used instead.

**Lesson.** Keep gameplay logic independent of animation assets, and verify the assets in your own project instead of trusting an assumed clip list.

### 2. Animator state machine bugs

**What went wrong.** The animator controller that came with the dragon pack is a demo: it has no parameters, and its states are chained together with exit-time transitions. The AI's first scripts drive the animator with parameters (`Speed`, `Fire`, `Tail`, `Fly`, `Land`, `Die`) that this controller did not have, so nothing responded to gameplay. There were three related bugs:

1. **Unusable demo controller.** Nothing could trigger the right animation at the right moment.
2. **No way out of the flying animation.** The first design only had a `Fly` trigger. The looping *Fly Forward* state never exited, so the dragon stayed in its air pose after landing.
3. **Skeleton and mesh were siblings.** The mesh and the bone hierarchy were separate children of the dragon, so moving only the mesh up for the take-off would not lift the visible dragon (a skinned mesh follows its bones).

**How I fixed it.**
1. I duplicated the pack's controller into my own `DragonCTRL` and kept only the states I needed: Idle, Walk, Flame Attack, Tail (Whip), Take Off, Fly Forward, Land and Die. I added the parameters `Speed` (float) and the triggers `Fire`, `Tail`, `Fly`, `Land` and `Die`.
2. I built the transitions: Any State to each attack / Die on its trigger, Idle and Walk driven by `Speed`, and attack / Land states returning to Idle on exit time. I unticked *Can Transition To Self* on every Any State transition so attacks cannot restart themselves, and turned on *Loop Time* only for the locomotion and fly-loop clips.
3. I added a **`Land` trigger** that `DragonCombat` fires at the moment of landing, and a *Fly Forward to Land* transition, so the animation exits exactly when the slam happens.
4. I put the mesh and bones under a single `Visual` object, moved the Animator there, and made that object the `Model` that gets lifted during Sky Strike.
5. The second dragon uses an **Animator Override Controller** pointing at the same graph with its own clips, so one state machine drives both dragons.
6. I matched the script timings (windup, active time, recovery) to the clips by scrubbing each clip and reading off the moment the flames start or the tail hits.

### Other issues found while debugging

- **Wrong references.** On review of a screenshot, one dragon's `DragonCombat` pointed at the other dragon's model and Animator. I re-assigned each dragon's own objects.
- **Fire VFX did nothing.** I had dragged the VFX *prefab asset* into the ability's slot; it must be an *instance in the scene*. I fixed it by instancing the effect under a `FirePoint` per dragon. I also added a Console warning in `DragonCombat` that detects this mistake.
- **Fire did not follow the head animation.** The fire point was parented to the dragon root, which does not move with the animation. I added `FollowBone.cs`, which keeps the fire point on the head bone while keeping the dragon's facing, so the flames follow the head and still shoot the way the damage cone points.

## How the tools made my work faster and better

- The AI produced complete first drafts of the scripts quickly, so I could spend most of my time on **game feel**: cooldown and damage tuning, knockback, camera shake, VFX timing and audio.
- It explained the Unity editor steps from screenshots of my own project, which sped up animator, UI and VFX setup.
- Reading errors and screenshots with it shortened debugging, especially the animator problems above.
- It helped me keep the code structured (shared combat class, events, data-driven abilities).

## Understanding the code
I did not just paste code. I can explain how `DragonCombat` runs abilities as coroutines with cone and radius hit checks, how `DragonMotor` combines velocity movement with decaying knockback, how `EnemyAI` chooses abilities from distance, how the `Health` events drive the UI, audio and win screen, and how the tail whip works without a tail animation.

---

## Known limitations and possible improvements

- The Tail Whip uses a procedural spin rather than a dedicated tail animation.
- The AI is intentionally simple (three states); it could dodge, retreat at low health or vary its tactics.
- Single arena and a single duel; no menu or difficulty options.
- Hit stun and a "get hit" animation are not implemented.

## Credits
Dragons, arena and fire effects: see the asset links above. Sound effects from Pixabay and created with Claude. Code written with AI assistance, as described in the AI Usage Note.
