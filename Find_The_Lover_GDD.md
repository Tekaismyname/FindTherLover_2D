# 📖 GAME DESIGN DOCUMENT (GDD) — FIND THE LOVER
### *Chuẩn Framework: A GDD Template for the Indie Developer (Jason Bakker)*
### *Phiên bản: 4.0 — The 20-Day Muck-Like Co-op Survival, TFT Augments & Multi-Island Puzzle Evolution*

> **Dự án:** Find The Lover  
> **Mục tiêu:** Quà tặng kỷ niệm 1 năm quen nhau (Anniversary Indie Game)  
> **Tác giả:** Teka (Developer) & Mentor  
> **Engine:** Unity 6 (URP 3D Archipelago World + 2D Pixel Art Billboard 2.5D)  
> **Nền tảng:** PC (Windows Standalone) & Hỗ trợ điều khiển Mobile (Android)  
> **Chế độ chơi:** Singleplayer (Kèm Bạn Đồng Hành AI) & Co-op Multiplayer 2 Người (Quene & Boyfriend qua Unity Netcode / Relay)  
> **Tình trạng tài liệu:** Sống (Living Document — Đồng bộ trực tiếp theo tiến độ phát triển)  

---

## PHẦN I: TỔNG QUAN TRÒ CHƠI (GAME OVERVIEW)

### 1.1. Triết Lý Thiết Kế Mới (The New Vision)
* **Khẩu hiệu (Elevator Pitch):** *"Một tựa game sinh tồn Co-op 2.5D phong cách Muck-like kết hợp giải đố môi trường đa đảo và nâng cấp bùa chú chuẩn Đấu Trường Chân Lý (TFT). Đôi bạn trẻ Quene và Lover dạt vào quần đảo cổ ngữ bí ẩn, cùng nhau khai thác tài nguyên, rèn trang bị, kích hoạt 5 Đài Phong Ấn Nguyên Tố và tử thủ trước các đợt quái đêm hung tàn với mục tiêu tối thượng: **Sinh tồn trọn vẹn 20 Ngày** để mở khóa các kết thúc định mệnh!"*
* **Trụ cột cảm xúc (Core Pillars):**
  1. **Tình Yêu Sát Cánh & Tương Trợ (True Co-op Synergy):** Quene và Lover là đôi bạn đồng hành gắn bó keo sơn. Cơ chế Gục Ngã & Hồi Sinh (`Downed & Revive`), Đuốc bảo vệ (`Torch Aura`) và Bùa Chú Tình Cảm (`Companion Bond`) đòi hỏi hai người chơi luôn che chắn, san sẻ tài nguyên và chiến đấu tựa lưng vào nhau.
  2. **Vòng Lặp Sinh Tồn Muck-like Khắc Nghiệt (Escalating Day/Night Tension):** Ban ngày êm đềm khai khoáng, chặt gỗ, giải đố. Ban đêm lũ quái Vực Thẳm trỗi dậy theo cấp số nhân; các đêm mốc **Ngày 5, 10, 15, 20** là những đêm Đại Họa (`Calamity Boss Nights`) nghẹt thở!
  3. **Cảm Giác Đập Hộp & Nâng Cấp Chuẩn TFT (Juicy 3-Tier Augments):** Mỗi khi diệt Quái Tinh Anh và Boss, người chơi nhận Quả Cầu Bùa Chú để chọn 1 trong 3 Lõi Công Nghệ (Đồng, Bạc, Vàng) mang đến những đột biến sức mạnh không ngờ (Giật sét lan, Vệt lửa lướt né, Hồi sinh bất tử).
  4. **Giải Đố Môi Trường Đa Đảo (Multi-Island Environmental Puzzles):** 5 Hòn đảo độc lập kết nối bằng các Cây Cầu Cổ ProBuilder, mỗi đảo sở hữu một Đại Trận Cơ Quan dựa trên Vật Lý - Quang Học - m Nhạc gắn liền với kỷ niệm 1 năm.

### 1.2. Mục Tiêu Tối Thượng Của Người Chơi (Win & Survival Conditions)
1. **Mục tiêu Sinh tồn 20 Ngày (20-Day Survival Marathon):** Mỗi ngày trong game kéo dài $5\text{ phút}$ ($3\text{ phút}$ Ngày, $2\text{ phút}$ Đêm). Người chơi phải sống sót qua 20 chu kỳ ngày/đêm trước bầy quái vật ngày càng đông và hung hãn.
2. **Mục tiêu Giải mã 5 Đài Phong Ấn (5 Island Shrines):** Hoàn thành các puzzle tại 5 khu vực để mở cánh cổng thời không trở về vương quốc.
3. **Điều kiện Thua cuộc (Defeat Condition):** Cả hai nhân vật cùng bị gục ngã (`Downed`) trong lúc đêm tối mà không ai còn khả năng hồi sinh $\rightarrow$ Màn đêm nuốt chửng ngọn lửa trại!

---

## PHẦN II: VÒNG LẶP LỐI CHƠI CỐT LÕI (CORE GAME LOOP & SYSTEMS)

```text
        ┌─────────────────────────────────────────────────────────────────┐
        │                        BAN NGÀY (DAYTIME - 3 PHÚT)               │
        │  • Chặt cây lấy gỗ, đập quặng lấy sắt/obsidian bằng Rìu/Cuốc     │
        │  • Chế tạo vũ khí & áo giáp tại Bàn Rèn (Crafting Forge)        │
        │  • Giải các Puzzle môi trường mở đường sang các Đảo kế tiếp     │
        │  • Mở Rương Cổ Tộc, thu gom thảo dược hồi phục & chuẩn bị đuốc │
        └───────────────────────────────┬─────────────────────────────────┘
                                        │ (Hoàng hôn - Chuông báo động vang lên)
                                        ▼
        ┌─────────────────────────────────────────────────────────────────┐
        │                        BAN ĐÊM (NIGHTTIME - 2 PHÚT)              │
        │  • Màn đêm buông xuống, sương mù che khuất tầm nhìn (Night Fog) │
        │  • Quái vật đêm (Goblin, Skeleton, Bat, Shroom) đổ bộ từng đợt  │
        │  • Càng về những ngày sau, ngân sách quái (SpawnBudget) tăng cao│
        │  • ĐÊM ĐẶC BIỆT (NGÀY 5, 10, 15, 20): ĐẠI BOSS ĐÊM XUẤT HIỆN!  │
        └───────────────────────────────┬─────────────────────────────────┘
                                        │ (Tiêu diệt Boss & Quái Tinh Anh)
                                        ▼
        ┌─────────────────────────────────────────────────────────────────┐
        │            PHẦN THƯỞNG: LÕI NÂNG CẤP BẬC ĐỒNG / BẠC / VÀNG     │
        │                   (TFT-STYLE AUGMENTS & POWER-UPS)              │
        │  • Rơi Quả Cầu Nâng Cấp (Augment Orb) $\rightarrow$ Modal 3 Thẻ Bài chọn 1 │
        │    🥉 BẬC ĐỒNG: Tăng chỉ số căn bản (+Tốc chạy, +Máu, Hút máu)  │
        │    🥈 BẬC BẠC: Biến đổi cơ chế (Giật sét lan, Vệt lửa, Giáp ảo)│
        │    🥇 BẬC VÀNG: Siêu nội tại bá đạo (Hồi sinh, Sấm sét toàn map)│
        └───────────────────────────────┬─────────────────────────────────┘
                                        │ (Tăng tiến sức mạnh vượt bậc)
                                        └───► [TIẾP TỤC ĐẾN NGÀY 20 ĐỂ ĐẠT KẾT THÚC]
```

### 2.1. Cơ Chế Co-op Sống Còn: Gục Ngã & Hồi Sinh (Downed & Revive System)
* **Trạng thái Gục ngã (`Downed`):** Khi máu về $0$, nhân vật không chết tức thì mà quỳ gối hấp hối trong $15\text{ giây}$.
* **Cứu sống (`Revive`):** Đồng đội chạy lại gần giữ phím `E` trong $3\text{ giây}$ để truyền sinh lực, hồi sinh nhân vật với $35\%$ máu.
* **Xác suất Thua:** Nếu người còn lại cũng bị hạ gục trong khi đồng đội đang gục ngã $\rightarrow$ Game Over ngay lập tức!

### 2.2. Hệ Thống Cấp Bậc Trang Bị & Bàn Rèn (Tiered Progression Forge)
Tài nguyên được phân bổ từ Đảo 1 đến Đảo 5, khuyến khích người chơi tiến lên các đảo mới:
1. **Bậc Gỗ (Wood):** Đốn Cây Sồi Đảo 1 $\rightarrow$ Rìu Gỗ, Kiếm Gỗ, Đuốc Sinh Tồn.
2. **Bậc Đá (Stone):** Đập Đá Cuội Đảo 1 $\rightarrow$ Kiếm Đá, Rìu Đá.
3. **Bậc Sắt (Iron):** Khai thác Mỏ Sắt Vách Núi Đảo 2 $\rightarrow$ Kiếm Sắt Bén, Giáp Sắt Kiên Cố.
4. **Bậc Hắc Diện Thạch (Obsidian):** Khai thác Đá Hắc Diện Thạch Đảo 2 & Đảo 3 $\rightarrow$ Trang Bị Hắc Diện Thạch Kháng Độc & Bỏng.
5. **Bậc Tinh Thể Thánh (Adamantite):** Khai thác Đá Thạch Anh Đền Cổ Đảo 4 & Đảo 5 $\rightarrow$ Vũ Khí Tinh Thể Tối Thượng sẵn sàng diệt Final Boss Ngày 20!

---

## PHẦN III: HỆ THỐNG GIẢI ĐỐ 5 KHU VỰC ĐA ĐẢO (5-ISLAND PUZZLE SANCTUARY)

### 🧩 ĐẢO 1: BỜ BIỂN & ĐẠI TRẬN CÂN BẰNG TẢI TRỌNG CỔ XƯA
* **Vị trí:** Vực sâu dẫn ra Cầu Cổ Liên Đảo số 1.
* **Cơ quan:** Bệ Cân Thăng Bằng Cổ Đại gồm Đĩa A (Tượng Thần nặng $10\text{ đơn vị}$) và Đĩa B (nối cáp hạ cầu).
* **Quy luật:** `1 Tảng Đá Đẩy = 4`, `1 Thỏi Sắt = 2`, `1 Khúc Gỗ = 1`. Đặt đúng tổng trọng lượng bằng 10 để hạ cầu an toàn sang Đảo 2.

### 🧩 ĐẢO 2: ĐẦM LẦY ĐỘC & MẠNG LƯỚI LỬA XANH (SOUL FLAME RUN)
* **Vị trí:** Vùng trũng sương độc Đảo 2 (`Poison_Gas_Zone`).
* **Cơ quan:** Rào Gai Cổ Đại (`Thorn_Barrier_Gate`) phong ấn hẻm núi, chỉ bị thiêu rụi bởi ngọn Lửa Xanh Huyền Bí (`Soul Flame`) tại Mật Thất 6 Cột Đá.
* **Quy luật:** Đuốc gỗ cháy $40\text{s}$. Người chơi phải chia nhau châm sáng 2 Đài Đuốc Trung Gian (`Brazier_01` & `Brazier_02`), dẫn ngọn Lửa Xanh quay lại thiêu cháy rào gai mở lối sang Đảo 3.

### 🧩 ĐẢO 3: ĐỀN CỔ THẠCH ANH & TÁN SẮC LĂNG KÍNH TÍM MAGENTA
* **Vị trí:** Giếng trời trung tâm Đảo 3.
* **Cơ quan:** Luồng Ánh Sáng Trắng chiếu từ trần đền, Mắt Ngọc Thần đòi hỏi chùm sáng **TÍM MAGENTA** để mở cánh cổng đền.
* **Quy luật:** Đẩy Khối Lăng Kính Pha Lê (`PrismBlock`) tách ánh sáng trắng thành Đỏ, Lam, Lục. Xoay 2 Trụ Gương Phản Xạ $360^\circ$ (`MirrorPillar`) để hội tụ tia Đỏ + tia Lam tại Mắt Ngọc Thần kích hoạt Đài Phong Ấn Số 3.

### 🧩 ĐẢO 4: THÁNH ĐỊA RỪNG NGÂN HẠNH & CHUÔNG ĐÁ GIAO HƯỞNG CO-OP
* **Vị trí:** Rừng Ngân Hạnh thiêng liêng Đảo 4.
* **Cơ quan:** Dàn 4 Chuông Đá Cổ phát ra 4 âm sắc Đồ - Mi - Sol - Đố ($C - E - G - C_2$).
* **Quy luật:** Bức tranh cổ gợi ý đoạn điệp khúc bài hát kỷ niệm 1 năm. Quene và Lover phải phối hợp nhịp nhàng, chia nhau vị trí gõ chuẩn xác 6 nhịp điệu trong vòng $10\text{s}$ để kích hoạt Đài Phong Ấn Số 4 và nhận **Bùa Vàng: Nhẫn Tình Yêu Cổ Đại**!

### 🧩 ĐẢO 5: MIỆNG NÚI LỬA DUNG NHAM & PHÁ GIÁP TRÙM CUỐI
* **Vị trí:** Đấu Trường Vực Thẳm Đảo 5.
* **Cơ quan:** Đại chiến Boss Ngày 20 kết hợp 2 Bệ Dẫm Từ Trường (Kẹp chặt cánh tay) và Van Xối Nước Ngầm (Sốc nhiệt nứt vỡ lõi giáp) mở đường kích hoạt Đài Phong Ấn Tối Thượng!

---

## PHẦN IV: HỆ THỐNG LÕI NÂNG CẤP CHUẨN TFT (3-TIER AUGMENT SYSTEM)

Khi tiêu diệt Quái Tinh Anh và các Boss Đêm Ngày 5, 10, 15, 20, một **Quả Cầu Nâng Cấp (Augment Orb)** sẽ rơi ra. Khi mở sẽ kích hoạt giao diện **Chọn 1 trong 3 Thẻ Bài**:

```text
┌──────────────────────────────────────────────────────────────────────────────────┐
│ 🥉 BẬC ĐỒNG (BRONZE AUGMENTS - TĂNG CHỈ SỐ CƠ BẢN)                               │
│ 1. Giày Gió Lướt (Wind Sneakers): +20% Tốc độ di chuyển.                         │
│ 2. Nồi Hầm Thịt Thao (Hearty Stew): +40 Máu tối đa (Max HP).                     │
│ 3. Nanh Ma Cà Rồng (Vampiric Fang): Hồi phục 5% sát thương đòn đánh thành máu.   │
│ 4. Bản Năng Khai Khoáng (Miner's Surge): Thu hoạch gỗ và quặng đá x2 sản lượng.  │
│ 5. Đuốc Bền Bỉ (Eternal Wick): Đuốc gỗ cháy lâu hơn 100% thời gian (80s).        │
│ 6. Khiên Gỗ Gia Cố (Reinforced Guard): Giảm 15% sát thương nhận vào từ sau lưng.  │
├──────────────────────────────────────────────────────────────────────────────────┤
│ 🥈 BẬC BẠC (SILVER AUGMENTS - BIẾN ĐỔI CƠ CHẾ CHIẾN ĐẤU)                         │
│ 1. Sét Lan Chuỗi (Chain Lightning): Đòn đánh chém trúng giật sét lan sang 3 quái.│
│ 2. Lướt Lửa Thiêu Đốt (Blazing Dash): Khi chạy nước rút để lại vệt lửa 3s.       │
│ 3. Giáp Hộ Mệnh Cổ (Guardian Shell): Nhận lớp giáp ảo 30 HP tự hồi mỗi 20s.      │
│ 4. Hơi Thở Bào Tử (Toxic Rebound): Khi bị đánh, phát nổ vòng khí độc đẩy lùi quái.│
│ 5. Đôi Bạn Sát Cánh (Partner Radiance): Đứng cạnh nhau trong 4m tăng 25% công.   │
│ 6. Lưỡi Dao Chém Gió (Wind Slash Wave): Nhát kiếm thứ 3 phóng ra sóng khí 6m.    │
├──────────────────────────────────────────────────────────────────────────────────┤
│ 🥇 BẬC VÀNG (GOLD AUGMENTS - SIÊU NỘI TẠI THẦN THOẠI)                            │
│ 1. Khế Ước Bất Diệt (Immortal Lover Bond): Khi 1 người gục ngã, người kia nhận    │
│    100% giáp và miễn nhiễm sát thương trong 10s để lật kèo hồi sinh bạn đời!    │
│ 2. Nộ Lôi Tinh Tú (Celestial Thunderstorm): Cứ mỗi 6s giáng một tia sét 120 DMG  │
│    xuống con quái vật có lượng máu cao nhất trên màn hình.                       │
│ 3. Bàn Tay Vàng Midas (Golden Touch): Diệt quái nổ tung ra mưa tiền vàng & đá quý.│
│ 4. Nhẫn Tình Yêu Vĩnh Hằng (Eternal Ring of Protection): Cả hai chia sẻ thanh máu│
│    chung và giảm vĩnh viễn 40% toàn bộ sát thương từ Boss!                       │
└──────────────────────────────────────────────────────────────────────────────────┘
```

---

## PHẦN V: HỆ THỐNG ĐA KẾT THÚC DỰA TRÊN MỐC 20 NGÀY (BRANCHING MULTI-ENDINGS)

Cuộc hành trình sinh tồn của Quene & Lover được chấm điểm dựa trên **Số ngày sinh tồn (tối đa 20 Ngày)** và **Số lượng Đài Phong Ấn được kích hoạt (tối đa 5 Đảo)**:

| Ending ID | Tên Kết Thúc | Điều Kiện Đạt Được | Ý Nghĩa Điện Ảnh Của Cutscene |
| :--- | :--- | :--- | :--- |
| **Ending 1** | **Chôn Vùi Dưới Màn Đêm (Lost to the Abyss)** | Cả hai cùng gục ngã trước Ngày 10, giải được $\le 2$ đài phong ấn. | Màn đêm nuốt chửng ngọn lửa trại. Hai nhân vật ôm chặt lấy nhau hóa thành tượng đá vĩnh hằng giữa quần đảo. Lời nhắc nhở về sự tàn khốc của màn đêm. |
| **Ending 2** | **Lời Thề Rút Lui (Survivor's Oath)** | Sống sót từ Ngày $10\text{--}19$, giải được $3\text{--}4$ đài phong ấn, nhưng gục ngã trước đêm 20. | Cả hai dùng bè gỗ trốn thoát khỏi quần đảo lúc rạng đông. Dù chưa tiêu diệt được Boss tối thượng, họ vẫn hạnh phúc vì còn sống bên nhau và thề sẽ quay lại. |
| **Ending 3** | **Ngọn Hải Đăng Hy Vọng (Beacon of Hope)** | Sống sót trọn vẹn **20 Ngày**, giải đủ 5 đài phong ấn, nhưng một trong hai người từng bị gục ngã $\ge 3$ lần. | Ánh sáng từ 5 đài phong ấn kích hoạt Cánh Cổng Thời Không. Quene và Lover nắm tay nhau bước qua cổng về vương quốc trong tiếng reo hò của muôn dân! |
| **Ending 4** | **Tình Yêu Vĩnh Cửu (Eternal True Lovers — True Ending)** | Sống sót trọn vẹn **20 Ngày**, giải $100\%$ Puzzle, hạ gục Final Boss Ngày 20 và **không một ai bị gục ngã quá 1 lần**! | Bức thư tình kỷ niệm 1 năm xúc động hiện lên giữa bầu trời sao đêm không còn bóng tối. Hoạt cảnh 2 người khiêu vũ lãng mạn $360^\circ$ và **Mở Khóa Album Ảnh Kỷ Niệm 1 Năm Ngoài Đời Thật**! |

---

*GDD Version 4.0 — Tài liệu thiết kế chuẩn mực tích hợp trọn vẹn Cơ Chế Sinh Tồn Muck-like 20 Ngày, Hệ Thống Lõi Nâng Cấp TFT 3 Bậc và Giải Đố Đa Đảo Co-op.*
