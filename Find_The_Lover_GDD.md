# 📖 GAME DESIGN DOCUMENT (GDD) — FIND THE LOVER
### *Chuẩn Framework: A GDD Template for the Indie Developer (Jason Bakker)*
### *Phiên bản: 3.0 — The Living Survival Ecosystem & Combat Upgrade*

> **Dự án:** Find the Lover  
> **Mục tiêu:** Quà tặng kỷ niệm 1 năm quen nhau (Anniversary Indie Game)  
> **Tác giả:** Teka (Developer)  
> **Engine:** Unity 6 (URP 3D World + 2D Pixel Art Billboard 2.5D)  
> **Nền tảng:** PC (Windows Standalone) & Hỗ trợ điều khiển Mobile (Android)  
> **Tình trạng tài liệu:** Sống (Living Document — Đồng bộ trực tiếp theo tiến độ phát triển)  

---

## PHẦN I: TỔNG QUAN TRÒ CHƠI (GAME OVERVIEW)

### 1.1. Triết Lý Thiết Kế (Philosophy / Core Vision)
* **Khẩu hiệu (Elevator Pitch):** *"Một tựa game 2.5D Survival — Puzzle lãng mạn nhưng thử thách sinh tồn và trí tuệ đỉnh cao. Cô gái bước vào hòn đảo hoang dã và hầm ngục cổ xưa, khai thác tài nguyên, chống lại bầy quái vật bóng đêm, phá giải các đại trận cơ quan liên hoàn và đánh bại Trùm Golem khổng lồ để giải cứu chàng trai của đời mình."*
* **Trụ cột cảm xúc (Core Pillars):**
  1. **Lãng mạn & Cá nhân hóa (Romantic & Personal):** Mọi chi tiết, mật mã chuông đá, giai điệu nền và bức tranh cổ đều gắn liền với câu chuyện tình yêu và kỷ niệm 1 năm của hai bạn.
  2. **Thỏa mãn trí tuệ & Chuỗi phản ứng (Ingenious Multi-layered Puzzles):** Loại bỏ hoàn toàn các câu đố mò mẫm vô lý. Mọi câu đố đều dựa trên quy luật vật lý (tải trọng, quang học lăng kính, dẫn truyền nhiệt lửa) kết hợp tài nguyên sinh tồn.
  3. **Cảm giác đòn đánh & Nhịp độ sảng khoái (Juicy Game Feel & Living World):** Chặt cây nảy nhịp đàn hồi, gỗ rớt văng nảy hút nam châm mượt mà, quái vật chớp đỏ khi trúng đòn, chu kỳ ngày/đêm biến ảo sắc màu.

### 1.2. Thông Tin Trải Nghiệm (Game Spec)
* **Thể loại:** 2.5D Action Survival — Environmental Puzzle — Boss Adventure.
* **Thời lượng chơi:** 35 — 50 phút.
* **Phong cách nghe nhìn (Art & Audio):**
  * **Đồ họa:** HD-2D / 2.5D (Môi trường 3D PBR URP kết hợp Nhân vật & Quái vật Pixel Art 32-bit Billboard sắc nét `Point Filter`).
  * **Âm thanh:** Nhạc nền chuyển biến theo thời gian (Bình yên ban ngày $\rightarrow$ Rùng rợn sương mù ban đêm $\rightarrow$ Hùng tráng rực lửa khi đánh Boss).

---

## PHẦN II: CƠ CHẾ LỐI CHƠI CỐT LÕI (CORE GAMEPLAY & SYSTEMS)

### 2.1. Vòng Lặp Trò Chơi Toàn Cảnh (Core Game Loop)
```text
  ┌─────────────────────────────────────────────────────────────┐
  │  1. THU THẬP & KHAI THÁC SINH TỒN (HARVEST & SURVIVAL)      │
  │  Đốn cây lấy gỗ, đập đá lấy quặng, tích trữ đuốc & công cụ  │
  └──────────────────────────────┬──────────────────────────────┘
                                 │
                                 ▼
  ┌─────────────────────────────────────────────────────────────┐
  │  2. CHỐNG CHỌI HỆ SINH THÁI QUÁI VẬT (COMBAT & DAY/NIGHT)   │
  │  Ban ngày dọn Goblin, ban đêm đối phó Dơi & U linh xuyên đá │
  └──────────────────────────────┬──────────────────────────────┘
                                 │
                                 ▼
  ┌─────────────────────────────────────────────────────────────┐
  │  3. PHÁ GIẢI ĐẠI TRẬN CƠ QUAN (MULTI-STAGE PUZZLES)         │
  │  Cân bằng tải trọng, dẫn lửa sương độc, tán sắc lăng kính   │
  └──────────────────────────────┬──────────────────────────────┘
                                 │
                                 ▼
  ┌─────────────────────────────────────────────────────────────┐
  │  4. ĐẠI CHIẾN BOSS PUZZLE & GIẢI CỨU (CLIMAX & RESCUE)      │
  │  Phá giáp Magma Warden Golem -> Mở lồng sắt đoàn tụ kỷ niệm │
  └─────────────────────────────────────────────────────────────┘
```

### 2.2. Cơ Chế Điều Khiển (Control Scheme)
* **Di chuyển:** `W`, `A`, `S`, `D` (Điều khiển nhân vật 8 hướng trong không gian 3D).
* **Xoay Camera Orbit:** Di chuyển Chuột (hoặc vuốt cảm ứng trên Mobile) để xoay góc nhìn mượt mà quanh nhân vật.
* **Tấn công / Khai thác:** `Chuột Trái` (Vung tay / Rìu / Kiếm chém trúng vật thể và quái vật).
* **Tương tác / Nhặt đồ / Đẩy khối:** Giữ phím `E`.
* **Mở túi đồ / Chế tạo:** Phím `Tab` hoặc `C`.

### 2.3. Các Hệ Thống Đã Hoàn Thiện (Implemented Systems)
1. **Hệ thống Khai thác Đàn hồi (Elastic Harvesting & Magnetic Drop):**
   * Cây cối và quặng đá có thanh máu riêng (`ResourceDataSO`).
   * Khi bị đánh trúng: Rung nảy biến dạng đàn hồi dạng hàm $\sin$ (`HitBounceRoutine`), phát âm thanh gỗ gãy/đá nứt.
   * Khi cạn máu: Cây đổ, sinh ra các vật phẩm `Item_Wood` văng nảy vật lý trên mặt đất, tự động kích hoạt từ trường nam châm hút gọn vào người chơi khi tiến lại gần.
2. **Hệ thống Túi đồ Ngăn nắp (Slot-based Inventory System):**
   * Quản lý vật phẩm theo ô có giới hạn chồng (`MaxStack = 99`).
   * Thuật toán gom nhặt thông minh (Greedy Fill): Ưu tiên dồn vào ô cùng loại chưa đầy, sau đó mới mở ô trống mới.
   * Bắn sự kiện C# `OnInventoryChanged` cập nhật tức thì lên thanh HUD góc màn hình.
3. **Hệ thống Máu & Kháng Đòn (Comprehensive Health & Combat Feel):**
   * Kế thừa hợp đồng trừu tượng `IDamageable`.
   * Thời gian bất tử chớp nhoáng (I-Frames = `0.2s`) ngăn chặn sát thương dồn tức tử vô lý.
   * Thanh máu nổi World-Space Billboard trên đầu nhân vật/quái vật, co giãn chuẩn xác theo tỷ lệ phần trăm `CurrentHealth / MaxHealth`.
   * Hiệu ứng chớp đỏ rực `DamageFlash` trên SpriteRenderer mỗi khi trúng đòn.
4. **Hệ thống Chu Kỳ Ngày/Đêm & Môi Trường URP:**
   * Ánh sáng mặt trời chuyển màu mượt mà từ vàng ấm ban ngày sang tím sẫm ban đêm.
   * Ban đêm kích hoạt sương mù rùng rợn (`Fog of Night`), gia tăng tần suất xuất hiện của quái vật bóng tối.

---

## PHẦN III: HỆ THỐNG GIẢI ĐỐ NÂNG CAO (ADVANCED PUZZLES)

---

### 🧩 PUZZLE 1: ĐẠI TRẬN CÂN BẰNG CỔ XƯA (ANCIENT WEIGHT & DENSITY BALANCE)
* **Vị trí:** Cổng Rừng Cấm qua khe vực nham thạch.
* **Cơ quan:** Một chiếc cầu treo nối qua vực sâu, được điều khiển bởi một **Bệ Cân Thăng Bằng Cổ Đại** gồm 2 đĩa cân đá lớn:
  * Đĩa A chứa một Tượng Thần Cổ Nặng `10 Đơn vị tải trọng`.
  * Đĩa B đang trống và nối với ròng rọc hạ cầu.
* **Quy luật vật liệu sinh tồn:**
  * `1 Khối Đá Lớn (Pushable Boulder)` có sẵn trong map = `4 Đơn vị`.
  * `1 Khúc Gỗ (Item_Wood)` chặt từ cây = `1 Đơn vị`.
  * `1 Thỏi Quặng Sắt Đặc (Iron Ore)` đập từ vách đá = `2 Đơn vị`.
* **Độ khó & Thử thách:**
  * Nếu đặt **dưới 10 đơn vị**: Cầu chưa đủ nặng để hạ xuống.
  * Nếu đặt **vượt quá 10 đơn vị**: Cầu gãy cáp phụ, kích hoạt bẫy phi tiêu từ hai bên vách đá!
  * **Giải pháp của người chơi:** Đẩy đúng 1 Khối Đá Lớn (`4`), đốn 2 Khúc Gỗ (`2`) và khai thác 2 Thỏi Sắt (`4`) đặt lên đĩa B ($4 + 2 + 4 = 10$). Hai đĩa cân thăng bằng hoàn hảo $\rightarrow$ Cầu gỗ hạ xuống an toàn!

---

### 🧩 PUZZLE 2: HÀNH LANG BÓNG TỐI & DẪN LỬA LIÊN HOÀN (TORCH DECAY & CHAIN FIRE)
* **Vị trí:** Mê Cung Hang Động ngập tràn khí gas độc.
* **Quy luật môi trường:**
  * Không khí trong hang đặc quánh khí độc. Nếu không đứng trong bán kính 2 mét của ngọn lửa bảo vệ, nhân vật sẽ mất máu liên tục theo giây (`5 HP/s`).
  * Đuốc gỗ sinh tồn của nhân vật có thời gian cháy tối đa **40 giây**.
* **Độ khó & Thử thách:**
  * Mê cung có 3 Đài Đuốc Cổ (Ancient Braziers) không bao giờ tắt một khi đã được châm lửa.
  * Cửa ra khỏi hang bị dây gai cổ đại phong ấn, chỉ bị thiêu rụi bởi **Lửa Xanh Huyền Bí (Soul Flame)** ở tận cùng ngách cụt.
  * Người chơi phải tính toán lộ trình:
    1. Châm đuốc thường từ ngoài cửa hang $\rightarrow$ chạy thần tốc đến Đài đuốc số 1 trước khi đuốc tàn $\rightarrow$ thắp sáng trạm 1 an toàn.
    2. Chế tạo thêm đuốc mới từ gỗ nhặt trong hang $\rightarrow$ mồi lửa từ Đài 1 chạy tiếp sang Đài 2 $\rightarrow$ thu thập ngọn Lửa Xanh Huyền Bí.
    3. Mang ngọn Lửa Xanh quay trở lại thiêu rụi rào gai phong ấn để mở lối vào Hầm Ngục!

---

### 🧩 PUZZLE 3: TÁN SẮC QUANG HỌC & PHA MÀU NGUYÊN TỐ (COLOR PRISM REFLECTION)
* **Vị trí:** Cổng đền Hầm Ngục Trung Tâm.
* **Cơ quan:**
  * Một luồng **Ánh Sáng Trắng** cực mạnh chiếu thẳng từ giếng trời trên trần đền xuống giữa phòng.
  * Cánh cổng đá phong ấn có một Mắt Ngọc Thần Ma Thuật đòi hỏi chùm tia sáng **MÀU TÍM (MAGENTA)** chiếu chuẩn xác vào tâm mắt.
* **Đạo cụ trong phòng:**
  * 1 Khối Lăng Kính Pha Lê (Prism Block) có thể đẩy được. Khi ánh sáng trắng đi qua, lăng kính tán sắc tách thành 3 tia: **ĐỎ (RED)**, **LAM (BLUE)**, và **LỤC (GREEN)** theo 3 góc 90 độ.
  * 2 Trụ Gương Phản Xạ (Reflection Mirrors) có thể xoay 360 độ bằng phím `E`.
* **Độ khó & Thử thách:**
  * Nếu chiếu nhầm tia Lục vào Mắt Ngọc: Cơ quan báo động, triệu hồi 2 con Skeleton Goblin bảo vệ xuất hiện.
  * Người chơi phải:
    1. Đẩy Khối Lăng Kính vào chính giữa luồng sáng trắng từ trần nhà.
    2. Xoay Trụ Gương A để đón lấy tia sáng **ĐỎ**.
    3. Xoay Trụ Gương B để đón lấy tia sáng **LAM**.
    4. Căn chỉnh góc phản xạ sao cho chùm tia Đỏ và tia Lam **cùng hội tụ giao thoa tại một điểm** ngay trên Mắt Ngọc $\rightarrow$ Đỏ kết hợp Lam tạo thành luồng sáng **TÍM MAGENTA** lộng lẫy $\rightarrow$ Cánh cổng hầm ngục rền vang mở ra!

---

### 🧩 PUZZLE 4: GIAO HƯỞNG CHUÔNG ĐÁ KỶ NIỆM (THE MELODIC VAULT - EASTER EGG)
* **Vị trí:** Mật Thất trước phòng Trùm Cuối.
* **Cơ quan:** 4 Chiếc Chuông Đá Cổ (Musical Stone Bells) được đánh dấu ký tự $1, 2, 3, 4$, mỗi chiếc phát ra một cao độ thánh thót riêng biệt khi vung kiếm gõ vào:
  * Chuông 1: Nốt Đồ ($C$)
  * Chuông 2: Nốt Mi ($E$)
  * Chuông 3: Nốt Sol ($G$)
  * Chuông 4: Nốt Đố ($C_2$)
* **Gợi ý trên tường:** Bức tranh cổ vẽ cảnh 2 người nắm tay dưới bầu trời sao kèm bản nhạc ngắn. Giai điệu này được phối theo **đoạn điệp khúc bài hát kỷ niệm của 2 bạn**.
* **Độ khó:** Người chơi lắng nghe và suy luận nốt nhạc để gõ đúng chuỗi 6 nhịp (Ví dụ: `1 - 2 - 3 - 2 - 3 - 4`).
* **Phần thưởng:** Một chiếc Rương Hoa Hồng mở ra: Chiếc Nhẫn Kỷ Niệm 1 Năm (tạo lá chắn vĩnh viễn giảm 50% sát thương từ Boss)!

---

## PHẦN IV: HỆ SINH THÁI QUÁI VẬT & MA TRẬN TÁC CHIẾN (THE MONSTER ECOSYSTEM)

Để tạo nên nhịp độ sinh tồn kịch tính, thế giới trong game được canh giữ bởi 7 chủng quái vật với các vai trò chiến thuật tương hỗ:

```text
                               ┌── 1. Magma Golem     (Boss / Siêu Trâu / Đập Đất AOE)
                               ├── 2. Forest Goblin   (Lính Cận Chiến / Tấn Công Bầy Đàn)
                               ├── 3. Skeleton Goblin (Lính Tầm Xa / Ném Xương / Tự Hồi Sinh)
HỆ SINH THÁI QUÁI VẬT ─────────┼── 4. Shadow Bat      (Sát Thủ Bay Đêm / Bổ Nhào Cắn Trộm)
                               ├── 5. Magma Scarab    (Quả Bom Sống / Cảm Tử Tự Bộc)
                               ├── 6. Spore Shroom    (Ụ Pháo Rừng / Phun Độc Rút Máu DOT)
                               └── 7. Shadow Wraith   (U Linh Khống Chế / Khóa Chân / Xuyên Tường)
```

### 4.1. Bảng Chỉ Số Cân Bằng Chi Tiết (`EnemyDataSO`)

| STT | Tên Quái Vật | Vai Trò Gameplay | Máu (HP) | Tốc Chạy (m/s) | Sát Thương (DMG) | Tầm Đánh (m) | Đòn Đánh / Kỹ Năng Đặc Trưng |
| :---: | :--- | :--- | :---: | :---: | :---: | :---: | :--- |
| **1** | **Magma Golem** | Boss / Siêu chống chịu | $300$ | $1.8$ | $35$ | $2.5$ | **Earth Smash:** Đập đất rung chuyển AOE bán kính $3.5\text{m}$, hất tung người chơi. |
| **2** | **Forest Goblin** | Cận chiến / Bầy đàn | $40$ | $3.8$ | $8$ | $1.2$ | **Pounce Bite:** Nhảy xổ vồ cắn chớp nhoáng khi áp sát mục tiêu $< 2\text{m}$. |
| **3** | **Skeleton Goblin** | Tầm xa / Quấy rối | $35$ | $2.5$ | $12$ | $6.0$ | **Reassembly:** Bị đánh chết vỡ thành đống xương, sau 4s tự ráp lại hồi $50\%$ máu nếu không bị đập nát! |
| **4** | **Shadow Bat** | Sát thủ không trung | $25$ | $5.0$ | $10$ | $1.5$ | **Dive Hit & Run:** Lượn vòng trên không $\rightarrow$ Bổ nhào cắn bất ngờ $\rightarrow$ Bay vút lên cao thoát thân. |
| **5** | **Magma Scarab** | Cảm tử / Quả bom sống | $20$ | $4.5$ | $45$ (Nổ) | $1.0$ | **Self-Destruct:** Phát sáng đỏ rực trong 1s rồi tự kích nổ thiêu rụi toàn bộ vùng $3\text{m}$. |
| **6** | **Spore Shroom** | Ụ súng độc / Rút máu | $50$ | $1.5$ | $10 + \text{DOT}$ | $8.0$ | **Poison Spit:** Bắn đạn bào tử gây độc trừ tiếp $2\text{ HP/s}$ trong $5\text{s}$ liên tục. |
| **7** | **Shadow Wraith** | Khống chế / Xuyên thấu | $60$ | $3.0$ | $15$ | $4.0$ | **Soul Freeze:** Bay xuyên cây/đá, cast phép đóng băng tốc độ chạy ($moveSpeed = 0$) trong $1.5\text{s}$. |

### 4.2. Phân Bổ Quái Vật Theo Khu Vực & Thời Gian (Spawn Matrix)
* **Khu 1: Bìa Rừng Khởi Đầu (Island Coast & Grasslands):**
  * *Ban ngày:* Forest Goblin đi lẻ tẻ quấy rối khi người chơi đốn cây.
  * *Ban đêm:* Shadow Bat rình rập trên tán cây, lao xuống tấn công bất ngờ.
* **Khu 2: Vực Đá Nham Thạch & Hang Khí Độc:**
  * *Vách đá:* Skeleton Goblin đứng trên cao ném xương cản trở giải đố Bệ Cân Thăng Bằng.
  * *Lối vào hang:* Magma Scarab bò nhanh ra từ các khe nứt núi lửa, đe dọa nổ tan xác nếu người chơi mất cảnh giác.
  * *Trong hang sương độc:* Spore Shroom ngụy trang thành cây nấm ven đường, phun axit độc tầm xa.
* **Khu 3: Hầm Ngục Cổ Đại (Ancient Temple):**
  * *Hành lang lăng kính:* Skeleton Goblin phục kích nếu người chơi chiếu sai tia sáng quang học.
  * *Mật thất chuông đá:* Shadow Wraith bay lơ lửng xuyên qua các trụ đá, làm chậm và khóa chân người chơi.
* **Khu 4: Buồng Ngục Trùm Cuối (The Warden's Sanctum):**
  * Đại chiến Trùm Magma Warden Golem cùng các đợt đệ tử Scarab hỗ trợ.

### 4.3. Tương Tác Khắc Chế & Chiến Thuật (Tactical Counterplay)
* **Ánh lửa xua đuổi bóng tối:** Cầm Đuốc trên tay làm giảm $50\%$ tốc độ lao của Shadow Bat và ngăn Shadow Wraith áp sát gần.
* **Cơ chế Triệt Tiêu Xương Cốt:** Khi đánh gục Skeleton Goblin, người chơi phải tung thêm 1 đòn đập nát đống xương rơi trên đất, nếu không nó sẽ tự động ráp lại sau 4 giây!
* **Dụ Scarab nổ liên hoàn:** Người chơi khéo léo dụ Magma Scarab lao vào bầy Goblin đông đúc rồi né ra $\rightarrow$ Cú tự phát nổ của Scarab sẽ tiêu diệt luôn cả đám quái xung quanh!

---

## PHẦN V: CHIẾN ĐẤU GIẢI ĐỐ VỚI TRÙM (PUZZLE-BOSS COMBAT)

Boss **Magma Warden Golem** canh giữ lồng sắt giam người yêu không thể bị tiêu diệt bằng cách đánh trực diện. Toàn bộ trận đánh là sự kết hợp nhuần nhuyễn giữa kỹ năng né tránh và cơ chế giải đố môi trường:

```text
  ┌─────────────────────────────────────────────────────────────┐
  │  GIAI ĐOẠN 1: BẪY NAM CHÂM PHÁ GIÁP TAY                      │
  │  Dụ Golem vào giữa 2 bệ dẫm -> Giật điện hút 2 cánh tay đá   │
  │  -> Vòng ra sau lưng chém nát 2 khớp vai                    │
  └──────────────────────────────┬──────────────────────────────┘
                                 │
                                 ▼
  ┌─────────────────────────────────────────────────────────────┐
  │  GIAI ĐOẠN 2: XỐI NƯỚC CO NGÓT LÀM NỨT LÕI DUNG NHAM        │
  │  Golem phun vòng lửa hộ thể -> Dùng Đuốc thắp sáng Van Nước │
  │  -> Nước đổ xuống sốc nhiệt làm vỡ nứt lớp giáp ngực đá     │
  └──────────────────────────────┬──────────────────────────────┘
                                 │
                                 ▼
  ┌─────────────────────────────────────────────────────────────┐
  │  GIAI ĐOẠN 3: CUỒNG NỘ CHẤN ĐỘNG & ĐÒN KẾT LIỄU             │
  │  Lõi Pha Lê lộ diện rực sáng -> Boss dậm đất AOE liên tục   │
  │  -> Canh nhịp nhảy né chấn động -> Tung nhát chém quyết định│
  └─────────────────────────────────────────────────────────────┘
```

### 5.1. Giai Đoạn 1: Bẫy Nam Châm Phá Giáp Tay
* Golem vung hai nắm đấm đá khổng lồ gây sát thương lớn.
* Trong phòng có 2 Bệ Dẫm Từ Trường ở hai góc đối diện.
* **Cách hóa giải:** Người chơi dụ Golem đứng vào giữa đường nối 2 bệ dẫm $\rightarrow$ Chém kích hoạt cả 2 bệ $\rightarrow$ Lồng từ trường cực mạnh phóng ra ghim chặt 2 cánh tay đá của Golem vào tường trong $6\text{s}$ $\rightarrow$ Người chơi vòng ra sau lưng chém vỡ khớp vai của Boss!

### 5.2. Giai Đoạn 2: Xối Nước Dung Nham Làm Nứt Lõi Đá
* Khi mất 2 tay, Golem quỳ xuống gầm thét, dung nham từ trong lồng ngực phun trào tạo thành màng chắn lửa thiêu rụi mọi thứ lại gần.
* **Cách hóa giải:** Người chơi dùng Đuốc thắp sáng 2 Van Cắt Nước ngầm trên cao $\rightarrow$ Hồ nước lạnh trên trần trút xuống ào ạt $\rightarrow$ Hiện tượng co ngót nhiệt độ đột ngột làm lớp giáp đá vỡ toang, để lộ **Trái Tim Pha Lê** rực sáng bên trong ngực Boss!

### 5.3. Giai Đoạn 3: Cuồng Nộ Chấn Động & Đòn Kết Liễu
* Mất lớp giáp bảo vệ, Golem bước vào trạng thái Cuồng Nộ (`Enrage`): Tốc độ tăng vọt, liên tục dậm chân tạo sóng chấn động $360^\circ$ lan tỏa khắp sàn đấu.
* **Cách kết liễu:** Người chơi căn nhịp nhảy né sóng chấn động $\rightarrow$ Áp sát vung nhát kiếm quyết định chém vỡ Trái Tim Pha Lê!
* Golem sụp đổ thành đống xỉ than vụn vỡ, làm rơi ra chiếc **Chìa Khóa Vàng Cổ Đại**.

---

## PHẦN VI: ĐOẠN KẾT GIẢI CỨU NGƯỜI YÊU (THE CLIMAX & CELEBRATION)

1. Cô gái nhặt Chiếc Chìa Khóa Vàng, bước từng bước vững vàng tiến tới chiếc lồng sắt cổ treo giữa căn phòng ngục.
2. **Tương tác phím `E`:** Tiếng xích sắt lách cách rơi xuống, cửa lồng bật mở.
3. Chàng trai bước ra khỏi lồng sắt. Hai nhân vật ôm chầm lấy nhau (hoạt cảnh Pixel Art ôm nhau ấm áp, ánh mắt xúc động).
4. Camera 3D Orbit quay chậm một vòng điện ảnh $360^\circ$ quanh đôi bạn trẻ dưới ánh sáng pha lê lung linh và những hạt bụi sáng rơi lấp lánh.
5. Màn hình tối dần và hiện lên thông điệp kỷ niệm đặc biệt:
   > *"Chúc mừng 1 năm bên nhau của chúng ta! Cảm ơn em vì đã luôn là người hùng tuyệt vời nhất trong cuộc đời anh."*
6. Nhạc nền kỷ niệm của hai bạn cất lên trọn vẹn, trên màn hình xuất hiện nút bấm: **"Mở Album Ảnh Kỷ Niệm 1 Năm"** để xem lại toàn bộ hình ảnh hành trình tình yêu ngoài đời thật!

---
*GDD Version 3.0 — Bản nâng cấp hoàn thiện tích hợp Hệ Sinh Thái Quái Vật & Cơ Chế Sinh Tồn Chiến Đấu Đỉnh Cao.*
