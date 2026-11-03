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
    - [x] **Tài liệu Báo cáo Kiến trúc Quái vật:** Đã xuất bản MONSTER_SYSTEM_ARCHITECTURE.md tổng hợp sơ đồ lớp UML Mermaid, phân tích 4 Archetype quái vật (Goblin, Skeleton, Scarab, Spore Shroom), máy trạng thái FSM, cơ chế 2.5D Frontal Priority, bảng cân bằng thông số và ma trận test case QA.
- [x] **Task 16.2 (Zone 2 Environment Hazard - Khí Gas Độc):** Lập trình `PoisonGasZone.cs` (rút máu $5\text{ HP/s}$ nếu không có lửa bảo vệ) & Tạo vùng khí độc `Poison_Gas_Zone` trong Scene với hiệu ứng sương độc bán trong suốt.
- [x] **Task 16.3 (Zone 2 Puzzle Gate - Chuỗi Đài Lửa & Lửa Xanh):**
  - Lập trình `PlayerTorchController.cs` (quản lý đuốc cháy $40\text{s}$, Lửa Thường & Lửa Xanh Soul Flame).
  - Lập trình `AncientBrazier.cs` & Dựng model 3D ProBuilder cho Đài Đuốc Cổ (`Ancient_Brazier.prefab`) và Đài Lửa Xanh (`Soul_Flame_Altar.prefab`).
  - Lập trình `ThornBarrierGate.cs` & Dựng model 3D ProBuilder cho Rào Gai Phong Ấn (`Thorn_Barrier_Gate.prefab`) cản đường và tự thiêu rụi khi dùng Soul Flame.

### [ ] Ngày 17: Khu Vực 3 ── ⚔️ Quái Đền Cổ & 🧩 Puzzle 3: Tán Sắc Lăng Kính & Hội Tụ Ánh Sáng Tím Magenta
- [ ] **Task 17.1 (Zone 3 Combat):** Thiết lập bãi quái Khu Vực 3: Shadow Wraith (U linh bay xuyên vật thể, cast chiêu làm chậm và đóng băng bước chạy Quene). Ban đêm Shadow Wraith cuồng nộ mở rộng bán kính truy đuổi.
- [ ] **Task 17.2 (Zone 3 Optics Setup):** Giếng trời rọi chùm sáng trắng trung tâm, Cổng Đền phong ấn bởi Mắt Ngọc Thần đòi hỏi chùm sáng Tím (Magenta).
- [ ] **Task 17.3 (Zone 3 Puzzle Gate):** Lập trình Khối Lăng Kính Pha Lê (`PrismBlock.cs`) tán sắc ánh sáng thành Đỏ - Lam - Lục và 2 Trụ Gương Phản Xạ xoay $360^\circ$ (`MirrorPillar.cs`): Hội tụ tia Đỏ + Lam tạo thành tia Tím Magenta bắn vào Mắt Ngọc Thần mở cánh cổng đá cổ vào Mật Thất Khu Vực 4!

### [ ] Ngày 18: Khu Vực 4 ── ⚔️ Thủ Hộ Mật Thất & 🧩 Puzzle 4: Giao Hưởng Chuông Đá Kỷ Niệm 1 Năm
- [ ] **Task 18.1 (Zone 4 Combat):** Tiêu diệt nhóm Thủ Hộ Mật Thất tinh nhuệ bảo vệ thánh địa trước khi tiếp cận dàn Chuông Đá Cổ.
- [ ] **Task 18.2 (Zone 4 Puzzle Gate):** Lập trình 4 Quả Chuông Đá Cổ (`MelodicBellsPuzzle.cs`) phát ra 4 âm sắc Đồ - Mi - Sol - Đố khi chém kiếm vào. Gõ đúng chuỗi giai điệu điệp khúc bài hát kỷ niệm 1 năm của 2 bạn.
- [ ] **Task 18.3 (Anniversary Reward):** Rương Hoa Hồng mở ra: Chiếc Nhẫn Kỷ Niệm 1 Năm (Lá chắn tình yêu vĩnh viễn giảm $50\%$ sát thương từ Boss), cánh cổng dẫn vào Đấu Trường Final Boss mở toang!

### [ ] Ngày 19: Khu Vực 5 ── 👑 Đấu Trường Dung Nham: Đại Chiến Final Boss (Magma Warden Golem)
- [ ] **Task 19.1 (Boss Combat & Environmental Puzzle Mechanics):** AI Boss Magma Warden Golem 3 giai đoạn kết hợp giải đố môi trường:
  - *Phase 1:* Boss đấm tay càn quét $\rightarrow$ Dụ Boss vào giữa 2 bệ dẫm từ trường kẹp chặt cánh tay $\rightarrow$ Chém vỡ khớp vai!
  - *Phase 2:* Boss phun vòng lửa dung nham $\rightarrow$ Dùng Đuốc thắp sáng 2 Van Cắt Nước ngầm xối nước lạnh làm sốc nhiệt nứt tung giáp đá ngực lộ Trái Tim Pha Lê!
  - *Phase 3:* Boss Cuồng Nộ dậm đất AOE $360^\circ$ $\rightarrow$ Quene nhảy né sóng xung kích, tung đòn quyết định chém vỡ Trái Tim Pha Lê!
- [ ] **Task 19.2 (Boss Victory):** Boss sụp đổ thành tro tàn, làm rơi chiếc **Chìa Khóa Vàng Cổ Đại** để giải cứu người yêu!

### [ ] Ngày 20: 💖 The Grand Finale: Đập Xích Lồng Sắt, Giải Cứu Chàng Trai & Màn Đoàn Tụ Kỷ Niệm
- [ ] **Task 20.1 (Rescue Interaction):** Quene cầm Chìa Khóa Vàng tiến tới chiếc lồng sắt cổ treo giữa đấu trường, tương tác phím `E` chặt đứt xiềng xích giải cứu chàng trai.
- [ ] **Task 20.2 (Romantic Ending Cutscene):** Chàng trai bước ra, hai nhân vật ôm chầm lấy nhau trong hoạt cảnh Pixel Art ấm áp, Camera Orbit quay $360^\circ$ điện ảnh lung linh.
- [ ] **Task 20.3 (Anniversary Surprise):** Hiện bức thư tình kỷ niệm 1 năm lãng mạn, phát bản nhạc tình yêu trọn vẹn và mở nút xem Album Ảnh Kỷ Niệm Ngoài Đời Thật!

### [ ] Ngày 21: Crafting Expansion & Player Power Progression
- [ ] Hoàn thiện hệ thống chế tạo công cụ & vũ khí (Rìu, Kiếm Sắt Bén, Đuốc Sinh Tồn Bền Bỉ, Giáp Da Hộ Mệnh) giúp Quene tự tin càn quét các khu vực.

---

## 📕 TUẦN 4 — Audio Polish, Optimization, Build & Release

### [ ] Ngày 22: Complete Audio Design & Sound Manager
- [ ] AudioManager dạng Singleton: BGM Ngày, BGM Đêm, BGM Đánh Boss.
- [ ] Tiếng bước chân thay đổi theo bề mặt (Cỏ, Cát, Nước, Đá).
- [ ] SFX chiến đấu, tiếng chặt cây, tiếng mở rương.

### [ ] Ngày 23: Dynamic UI/UX Polish
- [ ] Số sát thương nảy lên trên đầu quái (`Damage Numbers Floating`).
- [ ] Hiệu ứng hoạt họa cho Panel UI (DoTween / LeanTween / Animator UI).

### [ ] Ngày 24: Performance Optimization & Profiling (Tối Ưu Android)
- [ ] Dùng Unity Profiler kiểm tra FPS, CPU, RAM.
- [ ] Tối ưu bộ nhớ rác (GC Alloc = 0 trong vòng lặp chính).
- [ ] Cấu hình Occlusion Culling và LOD cho cây cối.

### [ ] Ngày 25: Stress Test & Bug Fixing
- [ ] Kiểm thử nhiều người chơi thực tế, xử lý lỗi mất đồng bộ vị trí.
- [ ] Kiểm tra toàn bộ góc cạnh trên địa hình không bị kẹt nhân vật.

### [ ] Ngày 26: Game Balance & Tuning
- [ ] Cân bằng chỉ số: Máu quái, Sát thương vũ khí, Giá mở rương.
- [ ] Tinh chỉnh cảm giác đánh (Game Feel).

### [ ] Ngày 27: Standalone Build Setup & Relay Testing
- [ ] Đóng gói bản cài đặt Android (`.apk`) và Windows (`.exe`).
- [ ] Kiểm thử kết nối Unity Relay chơi qua 2 mạng Wifi khác nhau.

### [ ] Ngày 28: Release Game On itch.io
- [ ] Đăng tải bản build lên nền tảng itch.io.
- [ ] Chuẩn bị banner, mô tả game và GIF gameplay hấp dẫn.

### [ ] Ngày 29: Portfolio & GitHub Showcase
- [ ] Viết file `README.md` kho lưu trữ GitHub chuyên nghiệp với sơ đồ kiến trúc SOLID.
- [ ] Cập nhật dự án vào CV / Hồ sơ năng lực Game Developer.

### [ ] Ngày 30: Final Project Review & Architecture Refactoring
- [ ] Tổng kết dự án 30 ngày, tối ưu hóa cấu trúc mã nguồn chuẩn doanh nghiệp.
