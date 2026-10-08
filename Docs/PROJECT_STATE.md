# Lootbound Isles — Project State

Last verified update: 2026-10-07 via Unity MCP (playable-slice, AI/spawn, progression, weapon, and local-save verification).

This file records observed project state, not design. `Docs/Lootbound_Isles_GDD.md` remains the source of truth. Editor inspection was performed against the connected `LootboundIsles` project at `C:/DevGame/LootboundIsles`, Unity `6000.4.8f1`; active scene: `Assets/_Game/Scenes/Game.unity`.

## Verification limits and editor snapshot

- One Unity Editor instance was connected; Editor was ready, idle, not compiling, and not in Play Mode. Active scene was clean (not dirty) and was the only enabled scene in Build Settings (index 0).
- Runtime smoke tests were performed in Play Mode using Unity MCP. A synthetic Input System W-key event moved Player about 3.85 m along the camera-relative direction; no physical keyboard/gamepad input was used. Bow firing and an enemy kill were also observed in an earlier controlled Play Mode test.
- Unity Test Framework returned `0` tests in the EditMode run. The Editor lists EditMode and PlayMode test modes/assemblies, but there are no registered tests to execute.
- Scene Validator reports 0 issues, 0 missing scripts and 0 broken prefabs. A live scene scan also found no GameObjects with missing MonoBehaviours. On Play Mode startup, Console previously emitted repeated generic “referenced script (Unknown) is missing” warnings without object/location; these were not reproducible via the validator/scan. The latest Console warning came from the controlled Zone progression diagnostic deliberately detaching LocalSaveSystem; no gameplay/compiler error was reported. MCP also emitted a WebSocket-not-initialised warning earlier in the session.
- The Edit Mode snapshot values `Health.CurrentHealth=0` and `LevelSystem.CurrentLevel=0` were defaults of non-serialized runtime properties, not a startup defect: after entering Play Mode, `Health` initialized to 100/100 and LevelSystem to Level 1 as configured.

## Verified scene and project setup

- Active `Game` scene has seven roots: Main Camera, World, `GameObject` (spawn points), Player, Navigation, GameUI, EventSystem.
- World is a prototype environment made from primitive planes, colliders/obstacles, a Directional Light, Global Volume, and `Zone02AccessGate`. There is no separately identified Safe Zone, Dungeon, Ritual Zone, or authored multi-zone map in the inspected hierarchy.
- Main Camera has Camera, AudioListener, URP additional camera data and `CameraFollow`; no Cinemachine camera/Brain is present.
- `Navigation` has `NavMeshSurface`; `com.unity.ai.navigation` 2.0.12 is installed. Spawned enemy agents were observed with `NavMeshAgent.isOnNavMesh == true`; the serialized bake data itself was not separately inspected.
- `GameUI` is a uGUI Canvas with a configured `LevelUpUI` and three choice buttons/text references. Its scene hierarchy currently contains only the LevelUp panel, not the full HUD, inventory/services, quick slots, or maps.
- EventSystem uses `InputSystemUIInputModule`. `com.unity.inputsystem` 1.19.0 is installed; `PlayerMovement` source creates an Input System `InputAction` for movement. Existing project tags include Player/MainCamera and standard Unity tags; layers include Default and UI.
- Project uses Unity 6.0.4, URP 17.4.0, uGUI, AI Navigation, Cinemachine 3.1.7, Test Framework 1.6.0, and MCP for Unity 10.3.0. `com.unity.pipeline` 0.8.0-exp.1 is also installed. No package changes were made in this audit.

## Verified implemented prototype systems

### Player, combat, and health — partial

- Player has CharacterController, PlayerMovement, PlayerTargeting, WeaponController, Health, PlayerStats, PlayerDeath, CombatState, HealthRegeneration, and CombatResolver; references on inspected Player/Camera components are assigned where observed.
- PlayerMovement is configured with speed 5, rotation speed 12, gravity -20, and camera/visual references. Movement uses the Input System. Its DEBUG field is enabled in the scene and produces the current unused-field warning.
- `PlayerStats` includes Damage/Attack Speed/Max HP/Movement Speed/Crit/Attack Range multipliers and Accuracy/Evasion/Defense values. Scene snapshot values: Accuracy 100, Evasion 100, Defense 0.
- `CombatResolver` references `DefaultCombatRules.asset` and reports itself configured. `CombatState` has a 5-second exit delay. Health regeneration is attached at 1 HP/sec.
- CombatResolver numerical contract was smoke-tested in Edit Mode through the live Player component: equal Accuracy/Evasion reports 90%; the lower cap is 20%; a 2.5 post-defense result rounds to 3; critical damage doubles; and a heavily defended successful hit still deals the configured minimum 1. A 20,000-roll equal-stat sample produced 17,992 hits (89.96%). This was an in-editor diagnostic, not a registered Unity test.
- CombatState/HealthRegeneration were verified with controlled in-Editor component diagnostics in Play Mode. Combat remained active with an event age of 4.9 seconds and exited at 5.1 seconds. Regeneration accumulated a 0.6 fractional remainder outside Combat, held it at 0.6 while In Combat, then resumed to heal one HP (90→91) and retained 0.1. Private Update methods were invoked directly to keep the check deterministic; this does not replace a full-frame attack-event integration test.
- `PlayerDeath` references `PlayerSpawnPoint`, delay 2 seconds, and respawn health fraction 0.1. A controlled Play Mode check confirmed death hides LevelUpUI, leaves `Time.timeScale=1`, resets Combat State, and respawns at PlayerSpawnPoint with exactly 10/100 HP when regeneration is disabled for measurement. Projectile cleanup on Player death remains unverified.

### Enemy and spawning — partial

- Four standalone enemy prefabs exist: Normal, Elite, MiniBoss, Boss. Each inspected prefab has NavMeshAgent, EnemyAI, Health, EnemyStats, EnemyCombat, Enemy, and EnemyDeath. No enemy GameObjects are currently instantiated in the active scene outside the prefabs.
- Four `EnemyDefinition` assets exist (one per type). Enemy system includes EnemyType, stats, AI, combat, death and definition scripts. `EnemyAI` checks leash distance from Home, suppresses chase while Returning Home, and restores full HP at arrival. Its anti-stuck progress check now measures reduction in distance to Home, rather than arbitrary movement; a controlled Play Mode regression verified progress keeps return active and movement away after timeout leads to Warp/Home reset and full HP. Natural pathing, all leash transitions, Player Death return, and special attacks remain unverified.
- Six scene spawn points use `EnemySpawnPoint` and reference data assets/prefabs: three normal cycles, elite, miniboss, and Boss. Spawn-cycle definitions exist for Normal01–03, EliteTest, MiniBossTest, and Boss01.
- Scene spawners are prototype-configured, with short test cooldowns (Boss 0.5 minutes) rather than the GDD’s approximate 60-minute Main Boss baseline. Cooldown storage currently uses PlayerPrefs and is not integrated with the general save system.
- A PlayerArrowProjectile prefab exists and is assigned by the current Bow weapon reference. Source inspection confirms it stores fire-time damage/accuracy/crit, stays bound to its original target, aborts if that target is invalid/dead, and uses the target's current evasion/defense when resolving impact. A limited runtime test confirmed automatic firing, projectile travel, hit/miss resolution and damage application; snapshot mutation and invalid-target cases remain untested.

### Weapons, EXP, Level Up — partial

- Weapon definitions exist for Sword, GreatSword, and Bow; Player currently references BowDefinition, with projectile origin and PlayerArrowProjectile assigned. Play Mode confirmed Sword damage; GreatSword selected exactly the three nearest front-sector targets, shared guaranteed crit damage, left a fourth/behind target untouched and set its configured cooldown; Bow preserved its fire-time damage/accuracy/crit snapshot through impact after Player damage stats changed, and cancelled when its bound target became inactive. Weapon swap retained a controlled 1.234-second cooldown. Physical/manual input and repeated statistical combat balance remain unverified.
- ExperienceSystem, LevelSystem and UpgradeSystem are attached. Six upgrade assets cover Damage, Attack Speed, Max HP, Movement Speed, Critical Chance and Attack Range; UpgradeSystem is configured for three choices and queues pending choices.
- `LevelUpUI` has valid references to UpgradeSystem, a panel, three buttons and three TMP labels. A controlled Play Mode scenario verified multiple queued choices, button selection, movement lock/unlock, hide-on-death and reopen-after-respawn without changing `Time.timeScale`. UI priority against future screens remains unverified.
- LevelSystem has max level 100 and configured start level 1. EXP curve/configuration and current editor snapshot require validation in Play Mode.

### Stabilization smoke test (2026-10-07)

- Entered Play Mode: Player initialized alive at 100/100 HP and Level 1. Normal/Elite/MiniBoss/Boss and additional Normal wave spawners ran; 11 enemy clones were observed with expected AI/Health/Combat components. The Game View showed the primitive prototype arena and spawned enemies.
- Injected 30 EXP through the existing ExperienceSystem as a controlled runtime check: Level became 2, one choice was pending, LevelUpPanel opened, PlayerMovement was disabled and `Time.timeScale` remained 1. This was a scripted component-level smoke test, not a keyboard/UI click test.
- Applied lethal damage through Health: Player entered dead state; movement and WeaponController disabled while `Time.timeScale` remained 1. After waiting, the Player was alive again and the queued Level Up panel was open. Exact post-respawn HP could not be confirmed because active enemy waves were attacking during the observation window; verify respawn percentage in a controlled safe test before treating it as runtime-confirmed.
- Play Mode was exited afterward. No scripts or scene configuration were changed in this stage. The EditMode test job succeeded but discovered and ran 0 tests.

### Combat state and regeneration verification (2026-10-07)

- Confirmed by source inspection that Sword, GreatSword, Bow fire/impact, and Enemy attack paths call `RegisterCombatEvent`; invalid/dead-target Bow projectiles are destroyed before registering an impact event. Enemy hit and miss both begin from the same attack event registration.
- In a controlled Play Mode diagnostic, temporarily disabled the six scene spawners and six spawned `EnemyCombat` components (runtime only), then verified the 5-second CombatState threshold and fractional regeneration pause/resume described above.
- Restored full HP and reset CombatState after the diagnostic. Exited Play Mode; active scene is clean (`isDirty=false`). No gameplay scripts, scene settings or prefabs were changed. No new Console errors; the only warning in the final query was an MCP WebSocket warning.

### Zone progression, wallet, and saving — partial

- Two `ZoneDefinition` assets exist and the scene has a `Zone02AccessGate` referencing ZoneProgression, Zone02 and blocking/locked visual colliders.
- ZoneProgression source handles first Main Boss defeat, records defeated Boss IDs, unlocks its configured Zone and saves progression. A controlled in-memory Play Mode check confirmed Zone 2 was initially locked, first Boss registration unlocked it and its gate collider, and a repeated registration was rejected. The save-system reference was deliberately detached for this check, so persistence was not part of this verification. The component falls back to same-object LocalSaveSystem at runtime.
- `PlayerWallet` is attached and loads 0 Gold / 0 Piastres from the current save. Runtime checks confirmed insufficient spends fail and non-positive additions are ignored. Successful balance-change persistence/backup behavior was not exercised in this pass.
- LocalSaveSystem implements current/backup JSON rotation, timestamp selection/validation and a save version. At the latest check the current local JSON exists with start-state progression and the backup JSON is absent. Actual payload currently covers unlocked zones, defeated Main Boss IDs, Gold and Piastres only. Yandex Cloud, all-player-state persistence, lifecycle/periodic autosave, and demonstrated migrations are not implemented/verified.

## GDD comparison

Status labels: **present** = observed and aligned for the inspected scope; **partial** = prototype exists but coverage or runtime proof is incomplete; **missing** = no corresponding system/assets were found in the project inventory; **prototype conflict** = current configured value/behaviour differs from GDD.

| GDD area | Audit status | Evidence / note |
|---|---|---|
| Single-player real-time movement and automatic targeting | Core route runtime-verified | Synthetic `Player/Move` W input moved Player camera-relative; automatic Bow targeting was observed; GreatSword front-sector target selection was checked. Physical input/manual UX remains unverified. |
| Shared combat pipeline, Accuracy/Evasion/Defense, combat state, regen | Core runtime-verified | Resolver math, 5-second state, regen pause/resume, Player and Enemy attack events, hit/miss and health changes were observed in controlled Play Mode checks; extended balance testing remains. |
| Sword / GreatSword / Bow | Core attack rules runtime-verified | Controlled Play Mode checks covered Sword, GreatSword nearest-front-sector/max-target/shared-crit behavior, Bow snapshot/invalid-target cancellation, hit impact, and cooldown preservation across weapon swap. Manual physical play and long-run statistical balance remain unverified. |
| Normal/Elite/MiniBoss/Boss and data-driven cycles | Core AI/waves runtime-verified; special attacks/projectile gap | Natural chase→leash→return, Player-death return, home HP reset, NavMesh placement, no individual wave replenishment, inter-wave delay and next-wave spawn were observed. Final cooldown persistence was not exercised. No Enemy projectile or special-attack implementation was found in the inspected scripts/prefabs. |
| Main Boss cooldown | Prototype conflict | Current Boss test spawner uses 0.5 minutes, not the ~60-minute design baseline (configurable value, not a design-level code contradiction). |
| Level cap and mandatory 1-of-3 upgrades | Core cap/queue runtime-verified | Two queued choices behaved correctly through button selection and death/respawn; a controlled Level 100 check discarded excess EXP. Upgrade balance/progression persistence remains outside the current implementation. |
| Death / respawn | Core runtime-verified | PlayerDeath closed LevelUpUI, did not pause, returned Player to spawn at exactly 10% HP in an isolated measurement, restored control, and triggered engaged Enemy return. Enemy projectile cleanup is not implemented/verified. |
| Real-time Level Up UI | Core queue/death behavior runtime-verified | Two level-up choices, button selection, death/respawn reopen and movement gating were checked in Play Mode; priority versus future UI remains unverified. |
| Authored Zone progression and Boss first-kill unlock | First-kill logic/gate/save runtime-verified | First Boss registration unlocked Zone 2, opened its gate, saved `zone_02` and the Boss ID; duplicate registration was rejected. Test save was restored byte-for-byte afterward. Reload across a fresh application session remains unverified. |
| Inventory, equipment instances/8 slots, gems, loot, crafting, vendors, consumables | Missing | No corresponding scripts or data assets found in project inventory. |
| Dungeon, Rebirth/Ritual/Quest, offline idle rewards | Missing | No corresponding systems/assets found. |
| Full HUD, Quick Slots, minimap/world map/fog of war | Missing | Scene UI contains LevelUp screen only. |
| Local + Backup + Yandex Cloud Save | Partial | Local current/backup and versioned data exist for limited progression/currency only; cloud and full save scope absent. |
| Persistent safe-zone/service-world structure and final presentation | Missing / prototype | Current scene is primitive test world; no distinct Safe Zone/services or final content presentation was found. |

No obsolete infinite WorldLevel system was found among the inspected script inventory. Existing Player/Enemy/Weapon/Spawn/Level-up and zone prototype architecture can be retained and extended; a wholesale rewrite is not indicated by this audit.

## Recommended development roadmap

The existing playable-slice checks (movement/input route, core combat/weapons, Enemy chase/leash/death response, spawn waves, Level Up/death flow, and first-Boss Zone save) are now complete for this audit; no concrete gameplay code defect requiring a change was reproduced. Remaining caveats are listed below. Per GDD §33, proceed to the first missing gameplay foundation only after accepting those validation limits.

1. **Inventory and item foundations (recommended first implementation stage):** add data-driven item definitions/instances, inventory categories/stacks, item locking and the GDD's eight equipment slots with stat recalculation. Keep UI, persistence and loot delivery as subsequent small stages rather than one coupled implementation.
2. **Direct loot, resources and economy:** per-enemy loot tables, instant inventory/currency awards, Gold/Piastres, material inventory and notifications.
3. **Equipment progression:** item levels, rarity fragments, exchange, selling/dismantling and lock protections.
4. **Gems and blacksmith operations:** sockets, gem definitions/upgrade, risky extraction, efficiency and reforge.
5. **Crafting and service loop:** recipes/blueprints, Merchant/Buyer, Jeweler, Healer/Blessings, consumables and three Quick Slots.
6. **Dungeon content model:** authored dungeon area, ordinary cycles, real-time Boss cooldown and associated loot/blueprints.
7. **Rebirth loop:** Level 100 quest activation, materials/Ritual Stone, Ritual Zone/waves/protection, Rebirth I–III and reset/persistent bonuses.
8. **Offline rewards:** entitlement zone, elapsed-time rules, caps and queued mandatory Level Up choices.
9. **Complete persistence:** local+backup validation/migrations, all gameplay state, lifecycle/autosave and Yandex Cloud conflict resolution.
10. **Maps and discovery:** minimap, world map and fog of war.
11. **Presentation pass:** replace gameplay primitives and finish visual/audio/UI polish once underlying gameplay loop is stable.

### Weapon behavior review (2026-10-07)

- Verified Unity MCP routing identifies the open `LootboundIsles` project at `C:/DevGame/LootboundIsles` (Unity 6000.4.8f1); earlier live inspection in this session confirmed Player's BowDefinition/projectile-origin references and the PlayerArrowProjectile prefab. The current Editor resource snapshot had become stale during this check, so no new scene mutation or gameplay run was attempted.
- Compared `WeaponController`, `PlayerTargeting`, `PlayerProjectile`, and `WeaponDefinition` with GDD §§7.1–7.2. The implementation matches target selection, shared GreatSword crit, per-target hit, Bow snapshot/target binding, and cooldown-preservation contracts at source level; no concrete discrepancy requiring code changes was found.
- Console query returned only two MCP integration warnings (WebSocket receive not initialized; null TransformHandle serialization), not gameplay/compiler errors. No scripts, scene, prefabs, or settings changed in this stage.

### Bow runtime verification (2026-10-07)

- In Play Mode, the existing Bow auto-targeted a temporarily repositioned Boss instance. Console recorded nine Bow shots: six 50-damage hits and three misses, then Boss death. This confirms the basic live fire → projectile travel → impact hit/miss → damage route, but not the full Bow snapshot/target-invalid edge cases. No Player death occurred.
- Six scene EnemySpawnPoint components and twelve EnemyAI/EnemyCombat components were disabled only for this runtime diagnostic; all changes were reverted by exiting Play Mode. The Boss diagnostic kill invoked progression saving. The current local save was then restored to the start-state data (`zone_01`, no defeated Main Boss IDs; 0 Gold/Piastres) so the test does not grant persistent Zone 2 progress.
- Spawned enemies were observed with `NavMeshAgent.isOnNavMesh == true`. Play Mode was stopped; active scene `Game.unity` reports `isDirty=false`. No new gameplay/compiler errors were found; the final Console query contained only the MCP WebSocket warning.

### Enemy return progress fix (2026-10-07)

- Corrected `EnemyAI` anti-stuck progress tracking: only a decrease in horizontal distance to Home by `ReturnProgressDistance` refreshes the progress timer. Movement sideways/away no longer hides a stuck return; `ReturnStuckTimeout` and existing NavMesh Warp/reset behavior are unchanged.
- Unity compiled the script successfully with no Console errors. In a controlled Play Mode regression on a live Normal enemy with `NavMeshAgent.isOnNavMesh == true`, 0.2m improvement kept `IsReturningHome` active; after simulated away movement and timeout, the agent returned exactly to Home, exited return state, and recovered HP from 29/30 to 30/30. The private update method/timer were driven deterministically; this does not replace an unassisted natural-path test.
- Diagnostic-only spawner/enemy state was runtime-local. Play Mode was stopped; `Game.unity` remains clean (`isDirty=false`), and the local save still contains only default Zone 1 progression. No registered project tests were available for this change.

### Input, death/respawn, and Level Up queue verification (2026-10-07)

- In Play Mode, a synthetic W keyboard state event was delivered through the configured `Player/Move` Input Action. Player moved about 3.85 m camera-relative; PlayerMovement was enabled and the Player remained alive. This verifies the action-to-movement route but is not a physical-input/manual playtest.
- In a controlled Play Mode scenario, adding 70 EXP advanced Level 1 to Level 3 and queued two choices. Selecting one choice left one pending, kept the panel open and movement disabled. Death hid the panel without pausing; after respawn the queued choice reopened. With HealthRegeneration temporarily disabled to remove measurement drift, respawn was exactly 10/100 HP at PlayerSpawnPoint and Combat State was inactive. Selecting the final queued choice cleared the queue, hid the panel and re-enabled movement. All runtime-only enemy/spawner/regeneration changes were discarded by stopping Play Mode.
- The Player's local save remained unchanged after the scenario: `zone_01`, no defeated Main Boss IDs, 0 Gold and 0 Piastres. Active scene `Game.unity` is not dirty. The Unity package query returned 51 installed packages, including Input System 1.19.0, AI Navigation 2.0.12, URP 17.4.0, uGUI 2.0.0 and Test Framework 1.6.0. Scene Validator reported 0 issues; Build Settings contain only enabled `Game.unity` at index 0. EditMode Test Framework run succeeded with 0 tests. Final Unity Console had no compile/gameplay errors; only the MCP integration warning `[WebSocket] Unexpected receive error: WebSocket is not initialised` was present.
- Synthetic input and scripted `Button.onClick` were used; physical/manual UX and long-run balance were not tested. The latest project EditMode run succeeded with 0 discovered tests.

### Weapon, Enemy AI, spawn, and persistence verification (2026-10-07)

- Controlled Play Mode checks confirmed GreatSword selected the three nearest targets in its configured front sector: all three took the same 200 critical damage, while the fourth front target and a target behind Player were untouched; cooldown was 2.00s. Sword dealt the expected 200 critical damage. Bow created a projectile with fire-time snapshot `50 damage / 100 accuracy / 100% crit`; doubling Player damage after firing did not alter impact damage (100 rather than 200). Deactivating the original Bow target caused the projectile to be destroyed without impact. Swapping weapons preserved an injected 1.234s remaining cooldown.
- With `Application.runInBackground` enabled for the MCP session, a real spawner-created MiniBoss naturally chased Player, crossed its 12m leash during a staged lure, returned home, and recovered to 250/250 HP. Killing Player while the MiniBoss was chasing immediately switched it to Returning Home; after respawn it was back within the horizontal home radius, full HP, and not chasing the distant Player.
- A live Normal01 two-wave cycle was checked: one of two spawned enemies dying did not replenish the wave; clearing the remaining enemy started the configured 2s inter-wave delay, then Wave 2/2 spawned its configured two enemies. The real-time final cooldown and PlayerPrefs persistence were not triggered to avoid leaving an active persistent timer.
- `LevelSystem` at Level 100 discarded 500 excess EXP. Wallet `AddGold(1)`/`TrySpendGold(1)` successfully wrote current JSON and rotated the prior current into backup. First Main Boss registration unlocked Zone 2/gate and wrote both `zone_02` and `main_boss_zone_01`; duplicate registration was rejected. Those save tests used a byte-for-byte file snapshot and the test-created backup was removed afterward. The original local save hash was restored exactly (Zone 1 only, no defeated Bosses, 0 Gold/Piastres); no backup file existed before or after. One initial wallet call in a separate just-transitioned test session encountered an uninitialized component reference; this was not reproduced after a fresh Play Mode entry, and both wallet write/spend passed without source changes.
- After the checks, Play Mode was stopped, active `Game.unity` remained clean, Scene Validator reported 0 issues, no compilation was in progress, and the only latest Console warning was the Unity MCP WebSocket-not-initialized integration warning. No gameplay code or scene/prefab settings were changed in this verification pass.
- Remaining prototype limits: no Enemy special-attack/telegraph or Enemy projectile implementation exists in the inspected scripts/prefabs; final spawn cooldown persistence, malformed-save backup recovery, fresh-process progression reload, physical/manual UX, and broader balance/performance playtesting remain. These are not regressions discovered by this audit. The next implementation stage proposed by GDD §33 is Inventory + Equipment foundations.

Each roadmap stage should remain separate and follow the project’s Unity MCP compile/configure/save/verify checklist. Do not start a later major stage until its predecessor is verified.
