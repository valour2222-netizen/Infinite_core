# Infinite Action-Adventure - Unity 2022 LTS (URP)

Copy `Assets/Scripts` into your project. Requires the **Input System** and **TextMeshPro** packages
(Active Input Handling = new system).

## Assets to create (Create > Game/...)
- Transform Anchor (Player), Float Variable (Difficulty), Difficulty Config, Player Stats
- Events: Void (GameReady, PlayerDied), Chunk (Spawned, Despawned), Enemy Death, Int (ScoreChanged), Damage,
  Projectile Request, Vfx Request
- Vfx Data (hit, death, projectile impact), Projectile Data, Enemy Attacks > Melee / Ranged, Enemy Data, Chunk Definition
- Game > Create Actor Animator Controller (menu item) -> assign your clips to its states

## Scene wiring
1. **Player**: Rigidbody + CapsuleCollider + Health + PlayerController (+ child model with Animator using the generated controller).
   Assign stats, anchor, input actions, PlayerDied event, Camera transform. Set Stats > enemyMask.
2. **Camera**: add `TopDownCameraRig` (anchor, Damage and Enemy Death channels).
3. **Chunk prefabs**: pivot at cell centre, `Chunk` with enemy spawn points and optional obstacle props.
   One ChunkDefinitionSO each; cover every socket combination your set can produce.
4. **Enemy prefabs**: Rigidbody + Collider + Health + EnemyController on an enemy layer.
   EnemyDataSO > attack = MeleeAttackSO (short attackRange) or RangedAttackSO (longer attackRange, set projectile + channel).
5. **Systems** (empty GameObjects): ChunkGenerator, EnemySpawner, ProjectileSpawner, VfxService, DifficultyManager,
   ScoreTracker, GameBootstrapper.
   Drag **ChunkGenerator, EnemySpawner, ProjectileSpawner and VfxService** into GameBootstrapper > Prewarmables.
   Every ProjectileDataSO must be listed in ProjectileSpawner and every VfxDataSO in VfxService (hit/death are added automatically).
6. **UI**: Canvas with HudController (health/stamina Images set to Filled, score and threat TMP texts) and GameOverScreen.
7. Keep Fixed Timestep at 0.02: attack and dash timings are counted in fixed ticks.

## Notes
- Pools are sized at prewarm time. A runtime warning in the editor means a pool grew: raise its prewarm count.
- Enemies return to the pool the instant they die, so rely on the death VFX rather than a death animation for them.
