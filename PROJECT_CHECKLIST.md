# 📋 BẢNG CHECKLIST CHI TIẾT TOÀN BỘ DỰ ÁN 30 NGÀY — "FIND THE LOVER"

> **Quy ước trạng thái:**  
> - `[x]` : Đã hoàn thành và kiểm thử thành công (Done).  
> - `[/]` : Đang thực hiện dở dang (In Progress).  
> - `[ ]` : Chưa bắt đầu (Pending).  
> 
> *Tài liệu này được cập nhật tự động sau mỗi tính năng hoàn thành. Mỗi buổi làm việc mới, Mentor và Học viên chỉ cần mở file này là biết chính xác 100% việc cần làm tiếp theo mà không cần quét lại toàn bộ mã nguồn!*

---

## 📗 TUẦN 1 — Core Architecture, Advanced Locomotion & World Setup

### [x] Ngày 1: Setup Môi Trường & Cấu Hình Dự Án
- [x] Tạo dự án Unity 6 URP (6000.4.0f1), cấu hình Render Pipeline.
- [x] Cài đặt các gói cốt lõi: New Input System, Cinemachine v3, Netcode for GameObjects.
- [x] Cấu hình Color Space Linear, Android Build Settings.

### [x] Ngày 2: Network Infrastructure & Connection UI
- [x] Cấu hình `NetworkManager` & `UnityTransport`.
- [x] Tạo `Player.prefab` cơ bản gắn `NetworkObject`.
- [x] Viết giao diện test Host / Server / Client.

### [x] Ngày 3: Character Locomotion & Camera System (SOLID Architecture)
- [x] Viết `PlayerLocomotion.cs` (CharacterController, New Input System, trượt dốc).
- [x] Tạo hợp đồng trừu tượng `ILookInputProvider.cs` (DIP).
- [x] Viết `InputSystemLookProvider.cs` đọc `<Pointer>/delta` (hỗ trợ cả Chuột PC lẫn Touch Android).
- [x] Viết `CameraSettingSO.cs` (Data Layer - Độ nhạy, Pitch Clamping, Invert Y).
- [x] Viết `CameraLookController.cs` (Decoupled Yaw/Pitch, SmoothDamp).
- [x] Setup Rig Camera: Gắn Main Camera làm con của Player để tự động Follow & Orbit.

### [x] Ngày 4: 2D Sprite Character Animation & State Machine
- [x] Cắt SpriteSheet nhân vật Quene (`Quene_Idle`, `Quene_Walk`, `Quene_Run`, `Quene_Attack`).
- [x] Tạo các Animation Clip `.anim` (8 hướng / 4 hướng cơ bản).
- [x] Cấu hình Animator BlendTree `Player_Locomotions.controller` (Tham số `PosX`, `PosY`, `Speed`).

### [x] Ngày 5: High-Quality Island Map & Environment Setup
- [x] Tạo Terrain 3D PBR $500\text{m} \times 500\text{m}$ (4 TerrainLayers: Cỏ, Đường đất, Đá, Cát).
- [x] Tạo Shader Nước URP biển & hồ (`RealisticWaterURP.shader`, `RealisticWater_Mat.mat`).
- [x] Viết công cụ rải cỏ tự nhiên `TerrainGrassAutoSpawner.cs` (20 loại thực vật).
- [x] Viết công cụ chuyển đổi `TerrainTreesToGameObjects.cs` (Chuyển 291 cây Terrain thành GameObject).
- [x] Viết `DynamicBillboard.cs` xoay mặt cây về phía Camera.
- [x] Khắc phục lỗi tàng hình mặt sau cây (Chuyển vật liệu cây/đá sang Two-Sided `_Cull = 0`).
- [x] Gắn `CapsuleCollider` cản đường cho thân cây.

### [x] Ngày 6: Lighting & Post-Processing Atmosphere
- [x] Cấu hình Directional Light (Sun) góc xiên tạo bóng đổ đẹp mắt.
- [x] Cấu hình Global Volume Post-Processing: Bloom, Tonemapping (ACES), Color Adjustments.
- [x] Cấu hình sương mù (Fog) và đổ bóng URP.

### [x] Ngày 7: Resource Node System & Harvesting Interaction (HOÀN THÀNH)
- [x] **Task 7.1:** Tạo Interface hợp đồng `IDamageable.cs` (`TakeDamage`, `CurrentHealth`, `IsDead`).
- [x] **Task 7.2:** Tạo Blueprint dữ liệu `ResourceDataSO.cs` (Máu, âm thanh, độ rung nảy, loot rớt).
- [x] **Task 7.3:** Tạo asset `Tree_Oak_Data.asset` ($60\text{HP}$, nảy $0.2$, rớt $2-4$ gỗ).
- [x] **Task 7.4:** Viết `ResourceNode.cs` (Triển khai `IDamageable`, thuật toán rung nảy `HitBounceRoutine`, âm thanh).
- [x] **Task 7.5:** Gắn `ResourceNode` vào Prefab cây `Tree_01_Quad`, `Tree_02_Quad`.
- [x] **Task 7.6:** Cập nhật `PlayerCombat.cs` bổ sung Hitbox đòn đánh (`Physics.OverlapSphere` + `TryGetComponent<IDamageable>`).
- [x] **Task 7.7:** Tạo Interface `IHarvestable.cs` (Phân định công cụ: Rìu, Cuốc, Tay không).
- [x] **Task 7.8:** Viết `PlayerHarvestInteraction.cs` (Hệ thống quét tương tác nhặt đồ).
- [x] **Task 7.9:** Viết `ItemDataSO.cs` (Data Layer - Quản lý tên khúc gỗ, icon, max stack).
- [x] **Task 7.9b (Items & Relics Master Spec):** Xây dựng cẩm nang toàn diện [`ITEMS_MASTER_SPRITESHEET_SPEC.md`](file:///c:/Users/hokha/Downloads/CVs/Items/ITEMS_MASTER_SPRITESHEET_SPEC.md) (25 Vật Phẩm, Trang Bị, Đạo Cụ Giải Đố, Kỷ Vật Kỷ Niệm 1 Năm & Bộ Prompts AI Icon 32x32 + Animated World Drop $1 \times N$ nền trong suốt).
- [x] **Task 7.10:** Viết `LootItem.cs` (Vật lý rơi nảy tưng tưng + lực hút nam châm Magnetic Pickup vào người chơi).
- [x] **Task 7.11:** Tạo Prefab `Item_Wood.prefab` và gán vào ô `Drop Prefab` của `Tree_Oak_Data`.
- [x] **Task 7.12:** Test toàn diện vòng lặp: Đấm 6 phát -> Cây đổ -> 3 Khúc gỗ nảy ra -> Tự hút vào người!

---

## 📘 TUẦN 2 — Networked Gameplay Systems & Combat Juice

### [x] Ngày 8: Shared Team Inventory & Full HUD UI (HOÀN THÀNH)
- [x] **Task 8.1:** Viết cấu trúc dữ liệu túi đồ `InventorySystem.cs` (Slots, Stacking, Add/Remove Item, OnInventoryChanged).
- [x] **Task 8.2:** Kết nối `LootItem.cs` với `InventorySystem` (Chạm ngực -> Tự nhét vào túi đồ).
- [x] **Task 8.3:** Viết View hiển thị `HUDInventoryView.cs` (Presentation Layer: Event-Driven cập nhật UI).
- [x] **Task 8.4:** Thiết kế Canvas HUD UI trên Scene với TextMeshPro (Icon Gỗ + Text đếm số lượng).
- [x] **Task 8.5:** Test thực chiến: Chặt cây -> Gỗ hút vào người -> Số lượng trên màn hình nhảy số tức thì!

### [x] Ngày 9: Comprehensive Health & Damage System (HOÀN THÀNH)
- [x] **Task 9.1:** Viết `HealthSystem.cs` (Data/Logic: Máu tối đa, Máu hiện tại, Sự kiện `OnHealthChanged`, `OnDamaged`, `OnDeath`, I-Frames, triển khai `IDamageable`).
- [x] **Task 9.2:** Tạo thanh máu World-Space Canvas nổi trên đầu nhân vật (`HealthBarView.cs`).
- [x] **Task 9.3:** Lập trình hiệu ứng chớp đỏ trúng đòn (`Red Flash Hit`) trên SpriteRenderer (`DamageFlash.cs`).
- [x] **Task 9.4:** Test thực chiến: Nhận sát thương -> Máu tụt, thanh máu co lại, nhân vật chớp đỏ rực!

### [x] Ngày 10: Advanced Enemy AI & NavMesh Pathfinding (HOÀN THÀNH)
- [x] **Task 10.0 (Design & Architecture):** Xây dựng tài liệu thiết kế quái vật (`MONSTER_DESIGN_BIBLE.md`) & Nâng cấp GDD ([`Find_The_Lover_GDD.md`](file:///c:/Users/hokha/Downloads/CVs/Find_The_Lover_GDD.md) v3.0) tích hợp trọn vẹn 7 chủng quái vật, ma trận xuất hiện, chỉ số toán học và cơ chế Boss.
- [x] **Task 10.0b (Water Slime Expansion):** Bổ sung quái sơ cấp Water Slime (`slime_water` - Khu 1 Bãi Biển & Đồng Cỏ) vào [`MONSTER_DESIGN_BIBLE.md`](file:///c:/Users/hokha/Downloads/CVs/Monster/MONSTER_DESIGN_BIBLE.md) và tạo đặc tả mỹ thuật hoàn chỉnh [`WATER_SLIME_SPRITESHEET_SPEC.md`](file:///c:/Users/hokha/Downloads/CVs/Monster/WATER_SLIME_SPRITESHEET_SPEC.md) (6 Actions, 18 Prompts AI 3 góc nhìn `_fw`, `_r`, `_bw` + Spritesheet Đạn Nước `Water_Bullet_FX` độc lập).
- [x] **Task 10.1:** Cài đặt package `com.unity.ai.navigation` & Bake NavMesh 3D trên địa hình đảo.
- [x] **Task 10.2:** Viết Blueprint dữ liệu quái vật `EnemyDataSO.cs` tại `Assets/02_Scripts/Enemies/Data/`.
- [x] **Task 10.3:** Viết cỗ máy AI `EnemyAI.cs` tại `Assets/02_Scripts/Enemies/` (FSM 4 trạng thái, tích hợp Animator 4 hướng và tự động bơm `maxHealth` vào `HealthSystem`).
- [x] **Task 10.4:** Tạo Prefab quái mẫu `Enemy_Goblin.prefab` (NavMeshAgent, HealthSystem, DamageFlash, HealthBarView, Visual 4 hướng).
- [x] **Task 10.5:** Test thực chiến: Quái đuổi theo Player vòng qua các gốc cây và cắn tụt máu!

### [x] Ngày 11: Melee Combat System & Combat Juice (HOÀN THÀNH)
- [x] **Task 11.1:** Viết `CameraShake.cs` (Presentation: Thuật toán rung suy giảm chấn động bậc 2).
- [x] **Task 11.2:** Viết `HitStopManager.cs` (Logic: Khựng hình freeze-frame với `WaitForSecondsRealtime`).
- [x] **Task 11.3:** Tạo `IKnockbackable.cs` & nâng cấp `EnemyAI.cs` đẩy lùi quái vật trên NavMesh (`_navAgent.Move`).
- [x] **Task 11.4:** Nâng cấp `PlayerCombat.cs` kích hoạt chuỗi phản ứng Combat Juice (Damage + Knockback + VFX + Shake + HitStop).
- [x] **Task 11.5:** Tạo Prefab hạt tia lửa `HitSpark_VFX.prefab` (URP Particle Burst, Auto Destroy) & liên kết vào Player.

### [x] Ngày 12: Seamless Day/Night Cycle & Atmosphere Transition (HOÀN THÀNH)
- [x] **Task 12.1:** Viết Blueprint dữ liệu `DayNightDataSO.cs` (Data Layer: chu kỳ 24h, ngưỡng ngày/đêm, gradient mặt trời & sương mù).
- [x] **Task 12.2:** Tạo asset cấu hình `DayNight_Config.asset`, pha dải màu hoàng hôn, nắng trưa và đồ thị cường độ/sương mù.
- [x] **Task 12.3:** Viết `DayNightCycleManager.cs` (Logic Layer: xoay đèn mặt trời $360^\circ$, cập nhật RenderSettings toàn cục, phát sự kiện `OnDayStarted`/`OnNightStarted`).
- [x] **Task 12.4:** Setup GameObject `DayNight_Manager` trên Scene, gán Directional Light và test Play Mode thành công mượt mà.
- [x] **Task 12.5 (QA):** Xây dựng bộ ma trận Test Case cho chu kỳ ngày/đêm và thiết lập module kiểm thử tự động `EnvironmentTests.asmdef`.

### [x] Ngày 13: Night Monster Spawner & Zero-Allocation Object Pooling (HOÀN THÀNH)
- [x] **Task 13.1:** Viết Blueprint dữ liệu `NightSpawnConfigSO.cs` (Data Layer: nhịp sinh quái, giới hạn sức chứa, bán kính Donut $15\text{m} \rightarrow 25\text{m}$).
- [x] **Task 13.2:** Viết hệ thống hồ chứa tái chế `EnemyPoolManager.cs` (Logic Layer: Prewarm pool 15 quái, Queue FIFO, Get/Return Enemy không tốn RAM).
- [x] **Task 13.3:** Nâng cấp `EnemyAI.cs`: Thêm hàm tái sinh `SpawnAt(Vector3)` (bật collider, warp NavMesh, hồi máu 100%) và tự thu hồi về Pool sau 1.5s khi chết.
- [x] **Task 13.4:** Viết cỗ máy `NightHordeSpawner.cs` (Event-Driven: tự động bắt đầu sinh quái khi đêm xuống `OnNightStarted`, dừng lại khi trời sáng `OnDayStarted`, quét `NavMesh.SamplePosition`).
- [x] **Task 13.5:** Setup GameObject `Night_Spawner` trên Scene, tạo asset `NightSpawn_Config.asset`, test Play Mode thành công mượt mà không giật lag.

### [x] Ngày 14: Night Frenzy Monster Buff & Zone 1 Campfire Sanctuary (HOÀN THÀNH)
- [x] **Task 14.1:** Cập nhật `EnemyAI.cs`: Hệ thống **Night Frenzy Buff (Logic Chỉ Số Quái Đêm)**:
  - Ban đêm tự động nhân hệ số: $+50\%$ Máu tối đa ($1.5\times$), $+30\%$ Tốc độ chạy ($1.3\times$), $+50\%$ Sát thương ($1.5\times$), tầm phát hiện mở rộng $10\text{m} \rightarrow 16\text{m}$.
- [x] **Task 14.2:** Thiết lập **Night Frenzy Visual Feedback & Enrage VFX (Dấu Hiệu Nhận Biết Quái Cuồng Nộ)**:
  - 👁️ **Mắt Đỏ Phát Sáng (Red Eye Glow / Point Light):** Hai đốm sáng đỏ rực xuyên màn đêm trên mặt quái vật.
  - 💨 **Hào Quang Bốc Khói Đỏ (Enrage Aura Particle):** Khói lửa đỏ/đen bốc lên dưới chân quái vật khi cuồng nộ.
  - 📈 **Phình To Cơ Bắp ($1.2\times$ Scale):** Kích thước quái to hơn bình thường $20\%$ (`transform.localScale`), nhịp chạy Animator tăng tốc $1.3\times$ (`animator.speed`).
  - 💢 **Biểu Tượng Nộ (Enrage Icon):** Biểu tượng tia sét/đầu lâu đỏ nổi cạnh thanh máu World Space.
- [x] **Task 14.3:** Tạo Lửa Trại An Toàn (Campfire Sanctuary / Safe Zone):
  - Ánh lửa bán kính $5\text{m}$ xua đuổi quái vật (quái vật không thể bước vào vùng ánh sáng).
  - Quene đứng trong vùng lửa trại được hồi phục $4\text{ HP/s}$ và an toàn tuyệt đối qua đêm.
- [x] **Task 14.4:** Vòng lặp Game Over & Hồi sinh (Respawn Loop): Khi cạn máu sẽ mờ màn hình (Fade out) và thức dậy an toàn cạnh Lửa Trại để tiếp tục hành trình.

---

## 📙 TUẦN 3 — Zone-Based Progression, Monster Combat & Puzzle Gates (Hành Trình Vượt Ải & Giải Cứu)

### [x] Ngày 15: Core Mechanics First — Equipment System, Hazard Traps & Puzzle Triggers (HOÀN THÀNH)
- [x] **Task 15.1 (Hệ Thống Cầm Nắm Trang Bị - Generic Player Equipment):**
  - Lập trình `PlayerEquipment.cs` gắn vào Quene quản lý ổ cắm `Belt_Socket` (gài đèn/đuốc thắt lưng) & phân tách vũ khí chiến đấu.
  - Tách bỏ hoàn toàn biến đuốc bị viết lẫn trong `PlayerLocomotion.cs` sang hệ thống mới chuẩn SOLID.
  - Trang bị Đuốc/Đèn Dắt Thắt Lưng (`Belt_Socket`) soi sáng ban đêm; triệt tiêu 100% lỗi lật vung tay của hoạt ảnh 2D.
- [x] **Task 15.2 (Cơ Chế Bẫy Môi Trường - Spike Trap Hazard):**
  - Lập trình Bẫy Chông Đất (`SpikeTrap.cs`): Chu kỳ $3$ pha (Ẩn $\rightarrow$ Cảnh báo rung $\rightarrow$ Phóng chông), trừ máu qua `IDamageable` cho cả Quene và Goblin.
  - Xây dựng Prefab 3D `Spike_Trap.prefab` (Base Plate đá + cụm chông nhọn 4 góc) tại `Assets/04_Prefabs/Environment/Spike_Trap.prefab` và tích hợp vào Scene.
- [x] **Task 15.3 (Cơ Chế Nền Tảng Cho Câu Đố - Puzzle Triggers & Interactables):**
  - Lập trình Bệ Dẫm Áp Lực (`PressurePlate.cs`): Bộ đếm thực thể `occupantCount`, nhún lò xo mượt mà, phát sự kiện `OnPressed / OnReleased` mở cửa/tắt bẫy.
  - Lập trình Cần Gạt Cơ Quan (`PuzzleLever.cs`): Tương tác phím `E` đổi trạng thái cần gạt, truyền tín hiệu kích hoạt cơ quan.
  - Tạo bộ Prefab 3D `Pressure_Plate.prefab` và `Puzzle_Lever.prefab` tại `Assets/04_Prefabs/Environment/` và đưa vào Scene.
- [x] **Task 15.4 (Đánh Bóng Cử Động & Cảm Giác Điều Khiển - Locomotion Juice):**
  - Tinh chỉnh tốc độ Chạy Nhanh Sprint, lực phanh chống trượt (`deceleration = 80`), gia tốc xuất phát nhạy bén (`acceleration = 25`).
  - Tạo hiệu ứng hạt bụi đất nảy dưới chân khi chạy nước rút (`FootstepDust`) gắn trực tiếp vào Quene_Player.

- [x] **Ngày 16: Khu Vực 2 ── ⚔️ Quái Hang Độc & 🧩 Puzzle 2: Dẫn Lửa Đuốc & Lửa Xanh (Phá Rào Gai Phong Ấn) (HOÀN THÀNH)**
  - [x] **Task 16.1 (Zone 2 Combat):** Thiết lập trọn vẹn bãi quái Khu Vực 2:
    - [x] **Skeleton Goblin AI & Prefabs:** Đã viết `BoneProjectile.cs`, `SkeletonEnemyAI.cs`, tạo `Skeleton_Enemy_Data.asset`, `Bone_Projectile.prefab` và `Enemy_Skeleton.prefab` (Cơ chế ném xương + vỡ đống xương 1 HP tự hồi sinh sau 4s).
    - [x] **Magma Scarab AI, VFX & Prefabs:** Đã chuẩn hóa 31 sheet, viết `MagmaScarabEnemyAI.cs`, tạo `MagmaScarab_Enemy_Data.asset`, `Magma_Explosion_VFX.prefab` và `Enemy_MagmaScarab.prefab` (Cơ chế đội đất chui lên `Spawn`, nạp nhiệt phình to tự bộc 45 AoE DMG).
    - [x] **Spore Shroom AI & Prefabs:** Đã chuẩn hóa 16 sheet, tạo 22 clips `.anim`, `SporeShroomController.controller`, `SporeBulletController.controller`, viết `PoisonStatusEffect.cs`, `SporeProjectile.cs`, `SporeShroomEnemyAI.cs`, tạo `SporeShroom_Enemy_Data.asset`, `Spore_Projectile.prefab` và `Enemy_SporeShroom.prefab` (Ụ súng nấm phun bào tử độc 8m gây DoT rút máu 2 HP/s trong 5s).
    - [x] **Skeleton Walking Animation Remaster & BlendTree Scaling:** Da can chinh PPU cua 3 spritesheet buoc di (Skel_walk_fw, w, 
) tu 100 ve 42, dong bo kich thuoc the gioi hoan hao voi Idle (~4.6m), toi uu Walk_Tree va Idle_Tree sang che do SimpleDirectional2D (type 1) 4 huong chong crab-walking.
    - [x] **Shadow Bat Slicing, Scaling, Animation & Prefabs:** Da cat chuan xac 26 spritesheet cua Shadow Bat (Fly, Bite, Stun, DiveAttack, Alert, Sonic Wave, Hurt, Death, Sonic Projectile/Impact), can chinh PPU theo ty le (80 cho 1x va 160 cho 2x/shockwave), tao 30 clips .anim, xay dung ShadowBatController.controller (7 BlendTrees), viet ShadowBat_Enemy_Data.asset, lap trinh ShadowBatEnemyAI.cs (bay luon tren khong, luon vong stalk player o ban kinh 4m, bo nhao Dive Swoop sat mat dat tan cong roi bay vut len, phong song am Sonic Wave, roi tu do khi chet), lap trinh SonicProjectile.cs, tao Sonic_Projectile.prefab va Enemy_ShadowBat.prefab.
    - [x] **Bugfix AI & 2.5D Directional Presentation (Shadow Bat & Spore Shroom):**
      - Khắc phục lỗi Shadow Bat không đuổi theo Player: Chuyển `EnemyAI.Update()` sang `protected virtual`, loại bỏ hàm ẩn trong `ShadowBatEnemyAI`, chuyển hiệu ứng bay dập dìu sang `LateUpdate()` và đồng bộ di chuyển NavMesh liên tục bám sát Player.
      - Khắc phục lỗi Spore Shroom bị lệch hướng (toàn quay `r` và `bw`): Cập nhật Cung Ưu Tiên Chính Diện $140^\circ$ theo góc nhìn Player ($Z \le 0.25$), khóa cứng `spriteRenderer.flipX = false` nhằm triệt tiêu xung đột lật ảnh với Blend Tree 4 hướng.
    - [x] **Tài liệu Báo cáo Kiến trúc Quái vật:** Đã xuất bản MONSTER_SYSTEM_ARCHITECTURE.md tổng hợp sơ đồ lớp UML Mermaid, phân tích 4 Archetype quái vật (Goblin, Skeleton, Scarab, Spore Shroom), máy trạng thái FSM, cơ chế 2.5D Frontal Priority, bảng cân bằng thông số và ma trận test case QA.
  - [x] **Task 16.2 (Zone 2 Environment Hazard - Khí Gas Độc):** Lập trình `PoisonGasZone.cs` (rút máu $5\text{ HP/s}$ nếu không có lửa bảo vệ) & Tạo vùng khí độc `Poison_Gas_Zone` trong Scene với hiệu ứng sương độc bán trong suốt.
  - [x] **Task 16.3 (Zone 2 Puzzle Gate - Chuỗi Đài Lửa & Lửa Xanh):**
    - Lập trình `PlayerTorchController.cs` (quản lý đuốc cháy $40\text{s}$, Lửa Thường & Lửa Xanh Soul Flame).
    - Lập trình `AncientBrazier.cs` & Dựng model 3D ProBuilder cho Đài Đuốc Cổ (`Ancient_Brazier.prefab`) và Đài Lửa Xanh (`Soul_Flame_Altar.prefab`).
    - Lập trình `ThornBarrierGate.cs` & Dựng model 3D ProBuilder cho Rào Gai Phong Ấn (`Thorn_Barrier_Gate.prefab`) cản đường và tự thiêu rụi khi dùng Soul Flame.
  - [x] **Task 16.4 (Zone 2 Level Construction & Multi-Island Archipelago Assembly — HOÀN THÀNH):**
    - [x] Khởi tạo Đảo Độc Lập Khu Vực 2 (`Terrain_Zone2_ToxicIsland`, kích thước $400 \times 80 \times 400\text{m}$ tại $X \in [250, 650], Z \in [-200, 200]$) lưu tại `Assets/01_Data/Terrain/Zone2_ToxicIsland_TerrainData.asset`, điêu khắc bờ biển cát vàng, vành đai núi đá tự nhiên, hồ đầm lầy độc (`Zone2_ToxicSwamp_Water`) và hẻm núi Mật thất Lửa Xanh.
    - [x] Xây dựng Cầu Cổ Liên Đảo (`Bridge_Zone1_To_Zone2` dài $60\text{m}$ từ $X=222 \to 282$) bằng ProBuilder (trụ đá rêu phong, ván gỗ sồi, lan can sắt rèn, đèn lồng cổ ngữ Cyan) bắc qua eo biển nối liền Đảo 1 và Đảo 2.
    - [x] Sơn tự động 4 Biome (`Layer_Grass`, `Layer_Rock`, `Layer_DirtPath`, `Layer_Sand`) đúng chuẩn thứ tự kênh màu trên cả Đảo 1 (`Terrain`) và Đảo 2 (`Terrain_Zone2_ToxicIsland`), đồng thời vẽ lại hệ thống đường mòn uốn lượn tự nhiên dạng đường cong chữ S (S-Curve Winding Pathways + Perlin Noise Wobble) nối liền Bãi Biển Nam $\rightarrow$ Lửa Trại $\rightarrow$ Hồ Ngọc Lục Bảo $\rightarrow$ Cầu Cổ Liên Đảo $\rightarrow$ 2 Nhánh Đường Vòng Quanh Hồ Đầm Lầy Độc $\rightarrow$ Mật Thất Lửa Xanh & Cổng Gai Phong Ấn.
    - [x] Thiết kế & Tách nền tự động bộ 6 Prefab 2.5D Billboard đặc trưng cho Khu Vực 2 (`Zone2_WeepingWillow_Quad` Liễu Rủ Đầm Lầy, `Zone2_DeadRuneTree_Quad` Cổ Thụ Khô Khắc Rune, `Zone2_GiantSporeShrooms_Quad` Cụm Nấm Bào Tử Phát Quang, `Zone2_RunicObsidianRock_Quad` Đá Hắc Diện Thạch Cổ Ngữ, `Rock_Crystal_Quad` Đá Tinh Thể Hổ Phách, `Rock_Ore_Quad` Đá Quặng Cổ) kết hợp thảm thực vật `CoverageMode` $1024 \times 1024$.
    - [x] Bố trí Lộ trình Dẫn Lửa 3 Trạm & Kiến trúc Cổ đại ProBuilder: Cổng Vòm Đá Cổ (`Zone2_Entrance_RuinArchway`), `Brazier_Checkpoint_01` ($X=342, Z=0$), Quần thể 6 Cột Đá Cổ Mật Thất & `Soul_Flame_Altar` ($X=435, Z=-68$), `Brazier_Checkpoint_02` ($X=485, Z=14$), và Hẻm núi `Thorn_Barrier_Gate` ($X=535, Z=0$).
    - [x] Phủ sương độc 3 tầng Particle VFX (`Poison_Gas_Zone`), bố trí 4 loài quái (`SporeShroom`, `Skeleton`, `MagmaScarab`, `ShadowBat`) và Re-bake `NavMeshSurface` xuyên suốt liên đảo.

### [x] Ngày 17: Muck-Like Night Raid Spawner & Đạo Diễn Độ Khó Theo Ngày (Night Raid Director)
- [x] **Task 17.1 (Night Raid Director):** Lập trình `NightRaidConfigSO.cs` & `NightRaidDirector.cs` quản lý ngân sách sinh quái ban đêm (`SpawnBudget = Base + (Day - 1) * Multiplier`), nhân hệ số Đêm Đại Họa (Ngày 5, 10, 15, 20), tăng tiến HP/DMG theo ngày, âm thanh báo động hoàng hôn và co-located unit tests `NightRaidDirectorTests.cs`.
- [x] **Task 17.2 (Wave Spawning Engine):** Lập trình thuật toán sinh quái đa chủng loại theo vành đai vòng tròn ngoài camera ($25\text{--}35\text{m}$ quanh người chơi/bãi trại) kết hợp Multi-Archetype Object Pooling cho 5 loài quái (`Goblin`, `Skeleton`, `MagmaScarab`, `SporeShroom`, `ShadowBat`) và kiểm tra `NavMesh.SamplePosition`.
- [x] **Task 17.3 (Survival HUD):** Lập trình `SurvivalHUDView.cs` hiển thị `DAY X / 20`, đồng hồ đếm ngược tới Hoàng Hôn/Bình Minh, thanh tiến trình pha, bộ đếm Lõi đang sở hữu và banner đỏ nhấp nháy cảnh báo Night Raid / Calamity Night.

### [x] Ngày 18: Hệ Thống Lõi Nâng Cấp 3 Bậc TFT (Đồng / Bạc / Vàng) & Rơi Quả Cầu Boss
- [x] **Task 18.1 (Data-Driven Augments):** Tạo `AugmentTier.cs`, `AugmentDataSO.cs`, `IAugmentReceiver.cs`, `PlayerAugmentStats.cs` và Editor Builder `SurvivalAndAugmentSetupBuilder.cs` khởi tạo trọn bộ 18 Lõi Công Nghệ (6 Đồng, 6 Bạc, 6 Vàng) tích hợp vào di chuyển, máu/khiên, hút máu, giật sét lan, vệt lửa, hồi sinh bất tử, sấm sét thiên thạch và nhân sản lượng khai thác.
- [x] **Task 18.2 (Augment Modal UI):** Lập trình `AugmentCardUI.cs` & `AugmentSelectionModalUI.cs` mở cửa sổ modal chọn 1 trong 3 thẻ bài viền sáng Đồng/Bạc/Vàng chuẩn TFT (hỗ trợ click chuột, phím tắt `1/2/3`, phím `K` test nhanh).
- [x] **Task 18.3 (Augment Manager & Drop Logic):** Lập trình `AugmentManager.cs`, `AugmentOrb.cs`, `AugmentDropController.cs` (lắng nghe `EnemyAI.OnAnyEnemyDefeated` rơi Quả Cầu Nâng Cấp hút nam châm) và co-located unit tests `AugmentManagerTests.cs`. *(Lưu ý: chạy menu `Tools > Find The Lover > Setup Survival Night Raid & TFT Augments` trong Unity Editor để sinh 18 file `.asset`, `AugmentOrb.prefab` & gắn UI vào Scene nếu MCP bridge chưa bật).*

### [ ] Ngày 19: Khu Vực 3 (Đảo 3) ── ⚔️ Quái Bay U Linh & 🧩 Puzzle 3: Tán Sắc Lăng Kính Tím Magenta
- [ ] **Task 19.1 (Zone 3 Archipelago):** Tạo Đảo Thạch Anh Đền Cổ (`Terrain_Zone3_CrystalTemple`) kết nối qua Cầu Cổ số 2.
- [ ] **Task 19.2 (Shadow Wraith AI):** Quái u linh bay xuyên địa hình, thả bùa làm chậm và cuồng nộ vào ban đêm.
- [ ] **Task 19.3 (Optics Puzzle):** Lập trình `PrismBlock.cs` và `MirrorPillar.cs` xoay trụ gương hội tụ tia Đỏ + Lam tạo tia Tím Magenta kích hoạt Đài Phong Ấn Số 3.

### [ ] Ngày 20: Khu Vực 4 (Đảo 4) ── ⚔️ Thủ Hộ Mật Thất & 🧩 Puzzle 4: Giao Hưởng Chuông Đá Co-op
- [ ] **Task 20.1 (Zone 4 Archipelago):** Tạo Đảo Rừng Ngân Hạnh (`Terrain_Zone4_GinkgoShrine`) và Quần thể Chuông Đá.
- [ ] **Task 20.2 (Melody Bells Puzzle):** Lập trình `MelodicBellsPuzzle.cs` 4 nốt Đồ - Mi - Sol - Đố, Quene và Lover phối hợp gõ đúng giai điệu điệp khúc bài hát kỷ niệm.
- [ ] **Task 20.3 (Co-op Reward):** Kích hoạt Đài Phong Ấn Số 4 và nhận Bùa Vàng: Nhẫn Tình Yêu Cổ Đại (Giảm $40\%$ sát thương khi đi cùng nhau).

### [ ] Ngày 21: Bàn Rèn Chế Tác Nâng Cao (Muck-Style Anvil & Power Forge)
- [ ] **Task 21.1 (Crafting Forge):** Lập trình `CraftingForge.cs` chế tạo vũ khí & giáp cấp cao (Sắt, Obsidian, Adamantite).
- [ ] **Task 21.2 (Tiered Tools & Weapons):** Hệ thống rìu, cuốc và kiếm chia cấp bậc, mở rộng sát thương và tốc độ khai khoáng.
- [ ] **Task 21.3 (Anvil Audio & VFX):** Hiệu ứng tóe lửa và âm thanh đe búa leng keng khi hoàn thành rèn trang bị.

### [ ] Ngày 22: Quái Tinh Anh & Boss Đêm Tấn Công (Night Boss Waves)
- [ ] **Task 22.1 (Night Boss AI):** Boss Đêm 3 (`SporeTitan`) và Boss Đêm 6 (`NightPhantomGolem`) xuất hiện tấn công căn cứ.
- [ ] **Task 22.2 (Gold Augment Chest):** Boss đêm bị tiêu diệt rơi Rương Lõi Vàng đảm bảo nhận Lõi Nâng Cấp Bậc Vàng tối thượng.

### [ ] Ngày 23: Khu Vực 5 (Đảo Núi Lửa) ── 👑 Đại Chiến Final Boss & Động Cơ Tính Đa Kết Thúc
- [ ] **Task 23.1 (Magma Warden Boss Fight):** AI Boss Dung Nham 3 giai đoạn kết hợp giải đố môi trường phá giáp.
- [ ] **Task 23.2 (Branching Multi-Endings):** Lập trình `SurvivalEndingManager.cs` tính toán 4 Ending (Lost to the Abyss, Survivor's Oath, Beacon of Hope, Eternal True Lovers).
- [ ] **Task 23.3 (Cutscene Engine):** Hoạt cảnh điện ảnh chuyển tiếp cho từng Ending tương ứng.

---

## 📕 TUẦN 4 — Audio Polish, Optimization, Build & Release

### [ ] Ngày 24: Thiết Kế Âm Thanh Toàn Diện (Day Chill Survival vs Night Horde Panic)
- [ ] **Task 24.1:** BGM ngày Acoustic Guitar êm dịu vs BGM đêm Horde Battle dồn dập kịch tính.
- [ ] **Task 24.2:** SFX chọn Lõi TFT "Ding!" rực rỡ, tiếng bước chân bề mặt và âm thanh chặt quặng giòn giã.

### [ ] Ngày 25: Đánh Bóng Giao Diện (Floating Damage Numbers & TFT Card Modal)
- [ ] **Task 25.1:** Số sát thương bay bốc lên đầu quái (`DamageNumberSpawner` - Trắng/Đỏ/Tím).
- [ ] **Task 25.2:** Hoạt họa lật thẻ bài 3D mượt mà cho cửa sổ chọn Lõi TFT.

### [ ] Ngày 26: Tối Ưu Hóa Hiệu Năng & Object Pooling (Tối Ưu Android & PC)
- [ ] **Task 26.1:** Triển khai `ObjectPoolManager.cs` cho Quái đêm, Đạn bào tử và VFX hạt ($0\text{ GC Alloc}$).
- [ ] **Task 26.2:** Đo đạc Unity Profiler duy trì $60\text{ FPS}$ ổn định khi có hàng chục quái vật tràn vào ban đêm.

### [ ] Ngày 27: Game Balance & Kiểm Thử Relay Co-op
- [ ] **Task 27.1:** Cân bằng đường cong sức mạnh người chơi vs tỷ lệ máu/sát thương quái đêm.
- [ ] **Task 27.2:** Kiểm thử kết nối Unity Relay chơi Co-op 2 người qua mạng.

### [ ] Ngày 28: Đóng Gói Standalone & Xuất Bản Lên itch.io
- [ ] **Task 28.1:** Đóng gói bản cài đặt Windows (`.exe`) và Android (`.apk`).
- [ ] **Task 28.2:** Xuất bản game lên itch.io kèm poster, gif và mô tả gameplay sinh tồn lãng mạn.

### [ ] Ngày 29: Portfolio & GitHub Showcase
- [ ] **Task 29.1:** Viết `README.md` kho lưu trữ GitHub chuyên nghiệp với sơ đồ kiến trúc SOLID & Muck Loop.
- [ ] **Task 29.2:** Cập nhật dự án vào CV / Portfolio thực tập Unity Game Tester / Developer.

### [ ] Ngày 30: Final Project Review & Architecture Refactoring
- [ ] **Task 30.1:** Rà soát toàn bộ mã nguồn, tối ưu hóa cấu trúc doanh nghiệp và đóng gói bản kỷ niệm tình yêu 1 năm trọn vẹn!
