# 👑 MASTER BOSS DESIGN & NIGHT SCHEDULE SPECIFICATION
### *Tài Liệu Thiết Kế Boss Khu Vực (5 Zones) & Boss Đêm Đại Họa (Ngày 5, 10, 15, 20)*

> **Dự án:** Find The Lover — 2.5D Muck-Like Co-op Survival & Puzzle Adventure (Unity 6 URP)  
> **Tác giả:** Teka (Developer) & Mentor  
> **Mục tiêu:** Cung cấp thông số, cốt truyện, cơ chế chiêu thức (Telegraphs), đòn phối hợp Co-op và bảng rơi đồ (Loot & TFT Augments) cho toàn bộ hệ thống Boss trong hành trình 20 Ngày Sinh Tồn.

---

## 🏛️ TỔNG QUAN HỆ THỐNG BOSS HAI TẦNG (DUAL BOSS SYSTEM)

Trong game có **2 nhóm Boss độc lập** vận hành song song:

```text
                                 ┌── Zone 1: Elder Treant King          (Chúa Tể Cổ Thụ Xanh)
                                 ├── Zone 2: Hydra Spore Wyrm           (Mãng Xà Bào Tử Độc Thần)
1. BOSS KHU VỰC (REGIONAL BOSSES) ┼── Zone 3: Prismatic Light Colossus   (Khổng Lồ Lăng Kính Pha Lê)
(Canh giữ 5 Đài Phong Ấn Đa Đảo)  ├── Zone 4: Siren Harpy Matriarch      (Nữ Chúa U Linh Tiếng Chuông)
                                 └── Zone 5: Magma Warden Golem         (Chúa Tể Dung Nham Vực Thẳm)

                                 ┌── Đêm 5:  Grave Titan Skeleton       (Đại Cốt Tinh Huyết Nguyệt)
2. BOSS ĐÊM ĐẠI HỌA (CALAMITY)   ├── Đêm 10: Night Phantom Wraith King  (Vua U Linh Bóng Tối)
(Tự động trỗi dậy săn lùng lúc   ├── Đêm 15: Venomous Spore Behemoth    (Quái Thú Bào Tử Độc Thần)
 nửa đêm các Ngày 5, 10, 15, 20)  └── Đêm 20: Eternal Abyssal Dragon     (Chúa Tể Vực Thẳm Tận Diệt)
```

---

# PHẦN I: THIẾT KẾ 5 BOSS KHU VỰC CANH GIỮ ĐÀI PHONG ẤN (REGIONAL BOSSES)

---

### 🌲 ZONE 1 BOSS: `Elder Treant King` (Chúa Tể Cổ Thụ Xanh)
* **Vị trí:** Trảng Cỏ Bờ Bắc Đảo 1, án ngữ trước Cầu Cổ Liên Đảo số 1.
* **Cốt truyện (Lore):** Cây cổ thụ nghìn năm tuổi bị rễ ma thuật của quần đảo xâm thực. Nó xem mọi kẻ đốn cây là kẻ thù và dùng thân gỗ ngàn năm cản lối sang Đảo 2.
* **Thông số cân bằng:**
  * **Máu (HP):** $400$
  * **Tốc độ chạy:** $1.8\text{ m/s}$
  * **Sát thương cơ bản:** $20\text{ DMG}$
  * **Điểm yếu cốt tử:** **Hệ Lửa (Fire Vulnerability)** — Nhận thêm $+50\%$ sát thương từ Đuốc và Lửa.

#### Kỹ Năng & Báo Hiệu Đòn Đánh (Telegraphs)
1. **Rễ Cây Đâm Đất (Root Spikes):** Boss gầm lên, cắm 2 cánh tay gỗ xuống đất. $1.2\text{s}$ sau, các cọc gỗ nhọn trồi lên từ dưới chân người chơi gây $25\text{ DMG}$. *(Cách né: Nhìn vòng đỏ dưới đất và chạy nước rút né ra ngoài)*.
2. **Quét Cành Cây Ngang (Sweeping Branch):** Vung cánh tay cành lá quét một góc $180^\circ$ phía trước mặt gây $18\text{ DMG}$ và hất văng người chơi $4\text{m}$.
3. **Quả Sồi Nổ Chậm (Acorn Bombs):** Lắc tán lá làm rơi 3 quả sồi nổ xuống đất. Sau $3\text{s}$ quả sồi phát nổ văng dăm gỗ.

#### Cơ Chế Co-op & Giải Đố Môi Trường
* Một người chơi cầm Đuốc gỗ (`Torch`) chém mồi lửa vào vỏ cây Treant $\rightarrow$ Cây bốc cháy trong $6\text{s}$, giảm $40\%$ giáp và bị choáng $2\text{s}$. Người còn lại dùng Rìu hoặc Kiếm lao vào dồn combo sát thương cực đại!

#### Phần Thưởng Khi Tiêu Diệt (Loot Drop)
* $100\%$ Rơi **`Wood Heart Core` (Lõi Cây Thần)** dùng kích hoạt cơ chế Bệ Cân Thăng Bằng mở Cầu Cổ sang Đảo 2.
* $100\%$ Rơi **1 Quả Cầu Nâng Cấp Bậc Bạc (Silver Augment Orb)**.
* $50\text{ Gỗ Sồi}$ + $20\text{ Quả Mọng Hồi Máu}$.

---

### 🍄 ZONE 2 BOSS: `Hydra Spore Wyrm` (Mãng Xà Bào Tử Độc Thần)
* **Vị trí:** Trung tâm Hồ Đầm Lầy Độc Đảo 2 (`Zone2_ToxicSwamp_Water`).
* **Cốt truyện:** Thủy quái mãng xà ngâm mình dưới đáy bùn độc ngàn năm, thân mình mọc đầy những cây nấm phát quang và gai nhọn axit.
* **Thông số cân bằng:**
  * **Máu (HP):** $750$
  * **Tốc độ chạy:** $2.6\text{ m/s}$
  * **Sát thương cơ bản:** $28\text{ DMG} + \text{Trúng Độc } (3\text{ HP/s trong } 4\text{s})$

#### Kỹ Năng & Báo Hiệu Đòn Đánh
1. **Phun Axit Diện Rộng (Toxic Sludge Barrage):** Ngửa cổ kêu rít chói tai, phun 3 bãi axit độc xuống mặt đất tồn tại trong $8\text{s}$. Dẫm vào bị rút máu liên tục.
2. **Quẫy Đuôi Sóng Bùn (Swamp Tail Sweep):** Xoay vòng $360^\circ$ quẫy đuôi tạo làn sóng bùn độc đẩy lùi cả 2 người chơi.
3. **Lặn Bùn Bổ Nhào (Submerge & Ambush):** Lặn xuống nước bùn trong $3\text{s}$, sau đó trồi vọt lên ngay dưới chân người chơi gây $35\text{ DMG}$ và hất tung.

#### Cơ Chế Co-op & Giải Đố Môi Trường
* Đầm lầy phủ sương độc rút máu. Đôi bạn trẻ phải di chuyển sao cho luôn có **ngọn Lửa Xanh (Soul Flame)** bảo hộ bên cạnh.
* Người chơi dẫn dụ Boss tung cú bổ nhào trúng vào **Tảng Đá Hắc Diện Thạch Runic** ven bờ hồ $\rightarrow$ Cú húc làm đá vỡ nứt, chấn động khiến Boss bị gãy nanh độc và choáng váng trong $5\text{s}$!

#### Phần Thưởng Khi Tiêu Diệt
* $100\%$ Rơi **`Soul Venom Crystal`** để thắp sáng vĩnh viễn Đài Lửa Xanh Mật Thất.
* $100\%$ Rơi **1 Quả Cầu Lõi Bạc & 1 Quả Cầu Lõi Vàng**.
* Mở khóa công thức chế tạo **Bộ Giáp Hắc Diện Thạch Kháng Độc**.

---

### 💎 ZONE 3 BOSS: `Prismatic Light Colossus` (Khổng Lồ Lăng Kính Pha Lê)
* **Vị trí:** Điện Thờ Giếng Trời Đảo 3.
* **Cốt truyện:** Vệ thần bằng đá thạch anh nguyên khối bảo vệ bí mật quang học của nền văn minh cổ xưa.
* **Thông số cân bằng:**
  * **Máu (HP):** $1200$
  * **Tốc độ chạy:** $1.6\text{ m/s}$
  * **Sát thương cơ bản:** $35\text{ DMG}$

#### Kỹ Năng & Báo Hiệu Đòn Đánh
1. **Chùm Laser Tán Sắc (Prism Ray):** Ngực rọi chùm tia sáng quét ngang sàn đấu. Trúng phải bị thiêu đốt $40\text{ DMG}$.
2. **Bão Mảnh Thạch Anh (Crystal Shard Volley):** Đập mạnh 2 tay vào ngực, bắn ra một loạt mảnh thạch anh sắc nhọn hình cánh quạt.
3. **Khiên Gương Toàn Năng (Reflective Barrier):** Bật lớp khiên pha lê bao quanh người. Mọi đòn đánh vật lý đánh vào đều bị **phản ngược $100\%$ sát thương** lại người chơi!

#### Cơ Chế Co-op & Giải Đố Môi Trường
* Khi Boss bật Khiên Gương, người chơi không thể đánh cận chiến.
* Quene và Lover phải phối hợp: Một người kéo dụ Boss đứng quay lưng về phía Mắt Ngọc, người kia xoay 2 Trụ Gương trong phòng để **chiếu chùm sáng Tím Magenta vào viên ngọc sau lưng Boss** $\rightarrow$ Khiên Gương nổ tung, Boss quỳ xuống bất động $6\text{s}$!

#### Phần Thưởng Khi Tiêu Diệt
* $100\%$ Rơi **`Prismatic Heart Core`** mở cánh cổng sang Đảo 4.
* $100\%$ Rơi **1 Quả Cầu Lõi Vàng (Gold Augment Orb)**.
* Mở khóa chế tạo **Kiếm Tinh Thể Ánh Sáng (Crit Rate $+25\%$)**.

---

### 🔔 ZONE 4 BOSS: `Siren Harpy Matriarch` (Nữ Chúa U Linh Tiếng Chuông)
* **Vị trí:** Đỉnh Núi Rừng Ngân Hạnh Đảo 4.
* **Cốt truyện:** Sinh vật nửa người nửa chim u linh canh giữ cây chuông đá cổ. Tiếng hát của ả có thể bẻ cong tâm trí và gây ảo giác.
* **Thông số cân bằng:**
  * **Máu (HP):** $1800$
  * **Tốc độ bay:** $4.2\text{ m/s}$ (Bay lượn trên không trung)
  * **Sát thương cơ bản:** $30\text{ DMG}$

#### Kỹ Năng & Báo Hiệu Đòn Đánh
1. **Khúc Hát Đảo Nghịch (Dissonant Screech):** Cất tiếng hát ma quái làm **đảo ngược toàn bộ phím điều khiển** (`W` đi lùi, `A` sang phải...) của người chơi trong $5\text{s}$!
2. **Bổ Nhào Lốc Xoáy (Gale Dive):** Lao vút từ trên không xuống sàn đấu tạo lốc xoáy cuốn người chơi lên trời.
3. **Phi Vũ Lông Bạc (Razor Feathers):** Quạt cánh phóng hàng chục lông vũ cắm phập xuống sàn đấu.

#### Cơ Chế Co-op & Giải Đố Môi Trường
* Khi Boss cất tiếng hát ma quái, người chơi nào đứng gần **Chuông Đá Cổ Nốt Đố ($C_2$)** phải lập tức vung vũ khí gõ chuông $\rightarrow$ m thanh trong trẻo của chuông đá lập tức phá tan ảo giác, làm rụng lông cánh của Harpy khiến ả rơi phịch xuống đất trong $6\text{s}$!

#### Phần Thưởng Khi Tiêu Diệt
* $100\%$ Rơi **`Feather of Harmony`** và **Bùa Vàng: Nhẫn Tình Yêu Kỷ Niệm (Giảm $40\%$ sát thương Boss)**.
* $100\%$ Rơi **1 Quả Cầu Lõi Vàng**.

---

### 🌋 ZONE 5 BOSS: `Magma Warden Golem` (Chúa Tể Dung Nham Vực Thẳm)
* **Vị trí:** Miệng Núi Lửa Đảo 5 (Caldera Arena).
* **Cốt truyện:** Khổng lồ dung nham cổ xưa, kẻ cai trị tối cao của Quần đảo Cổ Ngữ.
* **Thông số cân bằng:**
  * **Máu (HP):** $3000$ (Chia 3 Giai đoạn phá giáp)
  * **Tốc độ di chuyển:** $1.8\text{ m/s} \to 3.0\text{ m/s (Cuồng Nộ)}$
  * **Sát thương:** $45\text{ DMG} \to 70\text{ DMG (Dậm đất AOE)}$

#### Cơ Chế 3 Giai Đoạn Đỉnh Cao
* **Giai đoạn 1 (Phá Giáp Tay):** Dụ Golem vào giữa 2 Bệ Dẫm Từ Trường $\rightarrow$ Giật điện khóa chặt 2 cánh tay đá trong $6\text{s}$ $\rightarrow$ Quene & Lover vòng ra sau chém đứt khớp vai!
* **Giai đoạn 2 (Sốc Nhiệt Nứt Giáp Ngực):** Golem phun vòng lửa dung nham hộ thể $\rightarrow$ Dùng Đuốc thắp sáng 2 Van Cắt Nước ngầm trên trần $\rightarrow$ Thác nước lạnh trút xuống làm co ngót nhiệt độ đột ngột, nứt vỡ toang giáp đá ngực lộ ra **Trái Tim Pha Lê**!
* **Giai đoạn 3 (Cuồng Nộ & Kết Liễu):** Golem dậm chân liên tục tạo sóng xung kích $360^\circ$ lan khắp sàn. Hai người chơi phải canh nhịp nhảy qua sóng chấn động và tung nhát kiếm quyết định chém vỡ Trái Tim Pha Lê!

---

# PHẦN II: THIẾT KẾ 4 BOSS ĐÊM ĐẠI HỌA TRỖI DẬY (NGÀY 5, 10, 15, 20)

Vào đúng **00:00 Nửa Đêm** của các ngày này, bầu trời sẽ chuyển sang màu đỏ máu hoặc tím đen, chuông báo tử vang rền và Đại Boss Đêm sẽ xuất hiện tấn công trực diện vào vị trí của người chơi!

---

```text
┌──────────────────────────────────────────────────────────────────────────────────┐
│ 💀 ĐÊM 5: HUYẾT NGUYỆT KHỞI SINH — GRAVE TITAN SKELETON                          │
├──────────────────────────────────────────────────────────────────────────────────┤
│ • Bối cảnh: Trăng máu đỏ rực, mặt đất rung chuyển dữ dội.                        │
│ • Ngoại hình: Bộ xương khổng lồ cao 6m cầm cây búa đại đao làm từ xương khủng long.│
│ • Chỉ số: 800 HP, 2.2 m/s, 30 DMG.                                               │
│ • Kỹ năng:                                                                       │
│   1. Ném Cự Thạch Xương (Bone Boulder): Bốc tảng xương ném xa 15m gây nổ diện rộng.│
│   2. Dậm Đất Triệu Hồi (Skeleton Swarm): Dậm chân gọi 6 Skeleton Goblin phụ chiến.│
│ • Cơ Chế Tử Thần: Khi bị đánh cạn máu lần 1, thân thể rã thành 4 Khối Đầu Lâu.   │
│   Trong 8s, Quene & Lover phải chia nhau đập nát 4 đầu lâu, nếu không Boss sẽ    │
│   hồi sinh lại với 50% máu!                                                      │
│ • Phần thưởng: Đảm bảo 1 Quả Cầu Lõi Bạc + 30 Quặng Sắt + Rương Kho Báu Đêm.   │
└──────────────────────────────────────────────────────────────────────────────────┘
```

```text
┌──────────────────────────────────────────────────────────────────────────────────┐
│ 👻 ĐÊM 10: NHẬT THỰC HƯ KHÔNG — NIGHT PHANTOM WRAITH KING                        │
├──────────────────────────────────────────────────────────────────────────────────┤
│ • Bối cảnh: Bầu trời tối đen như mực, sương mù dày đặc buốt giá.                 │
│ • Ngoại hình: Vua U Linh khoác áo choàng đen rách rưới, bay lơ lửng, mắt đỏ rực.│
│ • Chỉ số: 1500 HP, 3.5 m/s, 38 DMG. Bay xuyên toàn bộ vách đá và cây cối.        │
│ • Kỹ năng:                                                                       │
│   1. Bão Tuyết Vực Thẳm (Soul Freeze): Đóng băng tốc độ di chuyển trong 2s.      │
│   2. Tia Laser Hư Không 360° (Void Laser): Xoay tròn bắn 4 tia laser quét sàn đấu.│
│   3. Tàng Hình Ám Sát (Shadow Cloak): Biến mất vào màn sương, bất ngờ hiện sau lưng.│
│ • Cơ Chế Khắc Chế: Đuốc lửa là khắc tinh duy nhất! Người cầm đuốc soi vào Boss  │
│   sẽ thiêu rụi lớp tàng hình và khiến Boss nhận thêm +40% sát thương!            │
│ • Phần thưởng: Đảm bảo 1 Quả Cầu Lõi Bạc + 1 Quả Cầu Lõi Vàng + 50 Vàng.        │
└──────────────────────────────────────────────────────────────────────────────────┘
```

```text
┌──────────────────────────────────────────────────────────────────────────────────┐
│ ☣️ ĐÊM 15: ĐẠI HỌA ĐẦM LẦY — VENOMOUS SPORE BEHEMOTH                             │
├──────────────────────────────────────────────────────────────────────────────────┤
│ • Bối cảnh: Cơn bão bào tử màu lục tím bao trùm toàn bộ các hòn đảo.             │
│ • Ngoại hình: Quái thú khổng lồ lai giữa bọ hung và nấm độc gai góc ngàn mắt.   │
│ • Chỉ số: 2400 HP, 2.5 m/s, 48 DMG.                                              │
│ • Kỹ năng:                                                                       │
│   1. Mưa Bào Tử Hủy Diệt: Bắn hàng trăm quả nang nấm độc rơi như mưa khắp bản đồ. │
│   2. Húc Càn Quét (Juggernaut Charge): Lao thẳng về phía trước ủi bay mọi vật cản.│
│   3. Màn Sương Độc Ăn Mòn: Rút máu toàn bộ người chơi 3 HP/s nếu rời xa ngọn lửa! │
│ • Cơ Chế Co-op: Một người phải liên tục châm đuốc duy trì vùng an toàn, người kia │
│   lướt né cú húc để dụ Boss đâm đầu vào vách đá tự làm choáng!                   │
│ • Phần thưởng: Đảm bảo 100% 1 Lõi Vàng Tối Thượng (Immortal Bond / Celestial).   │
└──────────────────────────────────────────────────────────────────────────────────┘
```

```text
┌──────────────────────────────────────────────────────────────────────────────────┐
│ 🐉 ĐÊM 20: KỶ NGUYÊN TẬN DIỆT — ETERNAL ABYSSAL DRAGON (THE APOCALYPSE)          │
├──────────────────────────────────────────────────────────────────────────────────┤
│ • Bối cảnh: Đêm cuối cùng của thử thách 20 Ngày! Bầu trời nứt toác rực lửa tím.  │
│ • Ngoại hình: Rồng hắc ám cổ đại với sừng dung nham và đôi cánh bóng đêm rực lửa.│
│ • Chỉ số: 3800 HP, 4.0 m/s, 60 DMG. Trùm cuối của Chế độ Sinh Tồn Muck-like!    │
│ • Kỹ năng:                                                                       │
│   1. Hơi Thở Vực Thẳm (Abyssal Breath): Phun dòng lửa tím thiêu rụi nửa màn hình. │
│   2. Giam Cầm Linh Hồn (Soul Cocoon): Phóng kén phong ấn một người chơi vào tảng │
│      đá đen. Người còn lại phải chém vỡ tảng đá trong 6s để cứu đồng đội!        │
│   3. Mưa Thiên Thạch Hư Không: Thiên thạch rực lửa dội xuống khắp quần đảo.       │
│ • Ý nghĩa sống còn: Tiêu diệt thành công Boss Đêm 20 trước lúc bình minh $\to$    │
│   Hoàn thành xuất sắc thử thách 20 Ngày, mở khóa TRỌN VẸN TRUE ENDING!           │
│ • Phần thưởng: Chiến thắng vẻ vang, Mở khóa Bức Thư Tình & Album Kỷ Niệm 1 Năm!  │
└──────────────────────────────────────────────────────────────────────────────────┘
```

---

## 📊 BẢNG TỔNG HỢP LỊCH TRÌNH BOSS & TỶ LỆ RƠI LÕI TFT

| Thời Điểm / Vị Trí | Tên Boss | Cấp Độ Nguy Hiểm | Máu (HP) | Phần Thưởng Lõi TFT Đảm Bảo |
| :--- | :--- | :---: | :---: | :--- |
| **Zone 1 (Đảo 1)** | `Elder Treant King` | ⭐⭐ | $400$ | 1 Lõi Bạc (Silver Augment) |
| **Zone 2 (Đảo 2)** | `Hydra Spore Wyrm` | ⭐⭐⭐ | $750$ | 1 Lõi Bạc + Tỷ lệ $30\%$ Lõi Vàng |
| **Zone 3 (Đảo 3)** | `Prismatic Light Colossus` | ⭐⭐⭐⭐ | $1200$ | 1 Lõi Vàng (Gold Augment) |
| **Zone 4 (Đảo 4)** | `Siren Harpy Matriarch` | ⭐⭐⭐⭐ | $1800$ | 1 Lõi Vàng + Nhẫn Kỷ Niệm |
| **Zone 5 (Đảo 5)** | `Magma Warden Golem` | ⭐⭐⭐⭐⭐ | $3000$ | 2 Lõi Vàng + Chìa Khóa Cổ |
| **ĐÊM 5 (Night 5)** | `Grave Titan Skeleton` | 💀💀 | $800$ | 1 Lõi Bạc + 30 Quặng Sắt |
| **ĐÊM 10 (Night 10)**| `Night Phantom Wraith King`| 💀💀💀 | $1500$ | 1 Lõi Bạc + 1 Lõi Vàng |
| **ĐÊM 15 (Night 15)**| `Venomous Spore Behemoth`| 💀💀💀💀 | $2400$ | 1 Lõi Vàng Tối Thượng |
| **ĐÊM 20 (Night 20)**| `Eternal Abyssal Dragon` | 👑👑👑👑👑 | $3800$ | **PHÁ ĐẢO GAME — TRUE ENDING!** |

---

*Tài liệu Boss Design v1.0 — Chuẩn mực thiết kế quái vật & nhịp độ sinh tồn kịch tính cho dự án Find The Lover.*
