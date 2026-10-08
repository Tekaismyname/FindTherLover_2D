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

### [ ] Ngày 12: Seamless Day/Night Cycle & Atmosphere Transition
- [ ] Script xoay mặt trời $360^\circ$ theo thời gian thực (1 ngày = 10 phút chơi).
- [ ] Chuyển đổi màu ánh sáng bầu trời mượt mà (Bình minh, Trưa, Hoàng hôn, Đêm tối).
- [ ] Kích hoạt sương mù đêm ma quái (`Fog of Night`).

### [ ] Ngày 13: Pooled Wave Manager & Horde Spawner
- [ ] Viết hệ thống `ObjectPooler.cs` tối ưu hiệu năng cho 30-50 quái vật cùng lúc.
- [ ] Quản lý các đợt quái (Wave System): Đêm 1 quái yếu, các đêm sau tăng số lượng & độ khó.

### [ ] Ngày 14: Base Camp Defense System & Game Over Flow
- [ ] Tạo nhà chính / Trại sinh tồn (Base Camp $500\text{HP}$).
- [ ] AI chia tỷ lệ: 50% đuổi theo Player, 50% tấn công đập phá Nhà chính.
- [ ] Vòng lặp Thắng / Thua (Victory / Game Over Loop).

---

## 📙 TUẦN 3 — Chest Augments (Lõi Nâng Cấp TFT) & Rich Content

### [ ] Ngày 15: Chest Augment System (Mở Rương Chọn Lõi)
- [ ] Tạo 3 loại rương: Rương Đồng (25G), Rương Bạc (50G), Rương Vàng (100G).
- [ ] Giao diện UI mở rương: Hiện 3 Thẻ bài Lõi nâng cấp ngẫu nhiên để chọn 1 (chuẩn TFT).

### [ ] Ngày 16: ScriptableObject Augment Data Pool (60 Lõi Nâng Cấp)
- [ ] Tạo 20 Lõi Bậc Đồng: Tăng 10% tốc chạy, cộng 20 HP, tăng 15% tốc độ chặt cây...
- [ ] Tạo 20 Lõi Bậc Bạc: Chém ra tia sét, đòn đánh hút máu, chạy nhanh sau khi né...
- [ ] Tạo 20 Lõi Bậc Vàng: Bất tử 3s khi sắp chết, nhân đôi vàng rơi, triệu hồi đệ tử...

### [ ] Ngày 17: Random Sunset Events System (Sự Kiện Ngẫu Nhiên)
- [ ] Hệ thống sự kiện hoàng hôn: Blood Moon (Trăng máu), Thick Fog (Sương mù mù mịt), Meteor Shower (Mưa thiên thạch), Merchant Caravan (Đoàn buôn bí ẩn).

### [ ] Ngày 18: Epic Boss Fight (Boss Khổng Lồ 3 Giai Đoạn)
- [ ] Thiết kế AI Boss Golem đá hoặc Quái thú rừng sâu.
- [ ] 3 Giai đoạn (Phases): Giậm đất AOE, triệu hồi đệ tử, cuồng nộ (Enrage).
- [ ] Thanh máu Boss khổng lồ trên đỉnh màn hình.

### [ ] Ngày 19: Combo System & Gold Multiplier
- [ ] Bộ đếm chuỗi hạ gục (Kill Streak): x2, x3 Vàng và tài nguyên khi đánh liên tục không bị ngắt.

### [ ] Ngày 20: Merchant NPC & In-Game Shop
- [ ] NPC Thương nhân xuất hiện 60s mỗi hoàng hôn.
- [ ] Giao diện Shop đổi Gỗ/Đá/Vàng lấy Vũ khí hiếm.

### [ ] Ngày 21: Crafting System & Base Building
- [ ] Hệ thống chế tạo: Chế tạo Rìu đá, Kiếm sắt, Đuốc lửa, Tường rào gỗ.
- [ ] Mô hình ảo xem trước vị trí đặt công trình (`Ghost Preview`).

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
