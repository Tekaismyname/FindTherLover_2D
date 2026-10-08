# 📖 SÁCH THIẾT KẾ QUÁI VẬT & ANIMATION BIBLE — "FIND THE LOVER"

> **Dự án:** Find the Lover — 2.5D / HD-2D Survival Adventure  
> **Phong cách Art:** Pixel Art 16-bit / 32-bit kết hợp ánh sáng URP 3D  
> **Mục tiêu:** Cẩm nang chi tiết về ngoại hình, chỉ số toán học, danh mục Animation và câu lệnh Prompt AI để tạo SpriteSheet cho toàn bộ hệ thống quái vật trong game.

---

## 🗺️ TỔNG QUAN HỆ SINH THÁI QUÁI VẬT (ECOSYSTEM ARCHETYPES)

Để tạo nên chiều sâu chiến thuật và nhịp độ sinh tồn kịch tính, dàn quái vật được chia thành 7 chủng loài tương hỗ lẫn nhau:

```text
                               ┌── 1. Magma Golem     (Boss / Siêu Trâu / Đập Đất AOE)
                               ├── 2. Forest Goblin   (Lính Cận Chiến / Tấn Công Bầy Đàn)
                               ├── 3. Skeleton Goblin (Lính Tầm Xa / Ném Xương / Tự Hồi Sinh)
HỆ SINH THÁI QUÁI VẬT HOÀN HẢO ├── 4. Shadow Bat      (Sát Thủ Bay / Bổ Nhào Cắn Trộm)
                               ├── 5. Magma Scarab    (Quả Bom Sống / Cảm Tử Tự Bộc)
                               ├── 6. Spore Shroom    (Ụ Pháo Rừng / Phun Độc Rút Máu DOT)
                               └── 7. Shadow Wraith   (U Linh Khống Chế / Khóa Chân / Xuyên Tường)
```

---

## 1. 🌋 MAGMA GOLEM (LAVA TITAN) — BOSS / TANKER NÚI LỬA

### 📌 Vai trò Gameplay
Quái vật thủ lĩnh khổng lồ, xuất hiện ở các đêm cao điểm (Boss Wave). Giáp cực dày, di chuyển chậm chạp nhưng mỗi cú đập có thể phá hủy hàng rào, công trình phòng thủ và hất tung người chơi.

### 🎨 Mô Tả Ngoại Hình (Visual Concept)
* **Hình khối:** Khối đá magma đen tuyền, gân guốc, cơ bắp cuồn cuộn bằng đá tảng obsidian.
* **Điểm nhấn:** Các đường nứt dung nham đỏ cam rực lửa chảy khắp cơ thể, sáng bừng theo nhịp thở. Đầu có 2 sừng đá cong vút uy nghiêm, mắt đỏ rực than hồng.
* **Kích thước Pixel:** $128 \times 128\text{px}$ (to gấp đôi quái thường).

### 📊 Chỉ Số Cân Bằng (`EnemyDataSO`)
* **Máu tối đa (`maxHealth`):** $300\text{ HP}$
* **Tốc độ chạy (`moveSpeed`):** $1.8\text{m/s}$ (Chậm)
* **Tầm đánh (`attackRange`):** $2.5\text{m}$
* **Sát thương đập (`attackDamage`):** $35\text{ DMG}$ (Kèm hiệu ứng Knockback văng xa $4\text{m}$)
* **Hồi chiêu đòn đánh (`attackCooldown`):** $2.5\text{s}$

### 🎬 Danh Mục Animation Chi Tiết
| Tên Animation Clip | Số Frame | Mô tả hành vi chuyển động |
| :--- | :--- | :--- |
| **`Golem_Idle`** | 6 frames | Đứng thở dốc, hai nắm đấm đá hạ thấp, dung nham trên ngực sáng lập lòe. |
| **`Golem_Walk`** | 8 frames | Bước từng bước nặng trịch, vai lắc lư, mỗi bước chân làm rung nhẹ màn hình. |
| **`Golem_Smash`** *(Signature)* | 8 frames | Giơ 2 nắm đấm đá khổng lồ lên cao quá đầu $\rightarrow$ Nện thẳng xuống đất tạo sóng chấn động AOE bán kính $3.5\text{m}$. |
| **`Golem_Roar`** | 6 frames | Ngửa mặt lên trời gầm thét, dung nham bùng cháy (kích hoạt khi máu $< 50\%$, tăng $50\%$ tốc độ chạy). |
| **`Golem_Hurt`** | 3 frames | Khựng người lại, các tia lửa và vụn xỉ than nhỏ tóe ra từ thân đá. |
| **`Golem_Die`** | 8 frames | Quỳ gối xuống, dung nham tắt ngúm chuyển sang màu xám tro, cơ thể vỡ vụn thành đống đá vụn. |

> 🤖 **Prompt AI Gen Ảnh:**  
> `Pixel art sprite sheet, massive magma golem monster, molten lava cracks on black obsidian stone body, giant stone horns, glowing lava eyes, colossal fists, white background, isolated, 16-bit game asset, front view.`

---

## 2. 👺 FOREST GOBLIN — LÍNH CẬN CHIẾN BẦY ĐÀN (SWARM GRUNT)

### 📌 Vai trò Gameplay
Quái vật phổ biến nhất trên đảo, xuất hiện ngay từ đêm đầu tiên. Di chuyển nhanh, chuyên đi thành nhóm $3 - 5$ con quây bắt người chơi khi đi chặt gỗ hoặc nhặt đá.

### 🎨 Mô Tả Ngoại Hình (Visual Concept)
* **Hình khối:** Dáng người nhỏ thó, khom lưng gù, tay dài có móng vuốt đen sắc nhọn.
* **Màu sắc:** Da màu xanh rêu sẫm, mắt đỏ rực hung tợn, tai dài nhọn hoắt đặc trưng của yêu tinh, miệng đầy răng nanh nhọn hoắt. Mặc khố rách màu xám đen.
* **Kích thước Pixel:** $48 \times 48\text{px}$ hoặc $64 \times 64\text{px}$.

### 📊 Chỉ Số Cân Bằng (`EnemyDataSO`)
* **Máu tối đa (`maxHealth`):** $45\text{ HP}$ (Dễ tiêu diệt)
* **Tốc độ chạy (`moveSpeed`):** $4.5\text{m/s}$ (Nhanh, bám sát người chơi)
* **Tầm đánh (`attackRange`):** $1.2\text{m}$
* **Sát thương cào (`attackDamage`):** $10\text{ DMG}$
* **Hồi chiêu đòn đánh (`attackCooldown`):** $1.0\text{s}$

### 🎬 Danh Mục Animation Chi Tiết
| Tên Animation Clip | Số Frame | Mô tả hành vi chuyển động |
| :--- | :--- | :--- |
| **`Goblin_Idle`** | 6 frames | Đứng khom lưng, móng vuốt cựa quậy, đầu ngoẹo qua lại rình rập. |
| **`Goblin_Run`** | 6 frames | Chạy lao chúi người về phía trước, bước chân dồn dập hung hăng. |
| **`Goblin_Scratch`** | 5 frames | Vung vuốt cào chéo 2 nhát liên tiếp (Trái $\rightarrow$ Phải) cự ly gần. |
| **`Goblin_Pounce`** *(Signature)* | 6 frames | Nhún chân nhảy chồm một đoạn $3\text{m}$ vồ lấy người chơi khi đang bỏ chạy. |
| **`Goblin_Laugh`** | 4 frames | Vỗ bụng cười khẩy nhạo báng khi cào trúng người chơi. |
| **`Goblin_Hurt`** | 2 frames | Ngửa mặt bật ngửa ra sau, miệng há hốc đau đớn. |
| **`Goblin_Die`** | 6 frames | Lăn lộn trên đất một vòng rồi nằm sõng soài biến mất. |

> 🤖 **Prompt AI Gen Ảnh:**  
> `Pixel art sprite sheet, green feral goblin monster, creepy sharp teeth, glowing red eyes, pointy ears, black claws, loincloth, T-pose, white background, isolated, 16-bit retro game style.`

---

## 3. 💀 SKELETON GOBLIN — XÁC SỐNG NÉM XƯƠNG & HỒI SINH (UNDEAD SCOUT)

### 📌 Vai trò Gameplay
Biến thể tà ác của Goblin đã chết. Giữ cự ly từ xa để ném xương rỉa máu người chơi, và sở hữu cơ chế đặc biệt: **Tự ghép xương hồi sinh sau khi bị đánh bại** nếu người chơi không kịp đập nát hộp sọ.

### 🎨 Mô Tả Ngoại Hình (Visual Concept)
* **Hình khối:** Bộ xương khô gầy gò, lồng ngực trơ xương sườn, đôi tai nhọn bằng sụn xương kỳ quái.
* **Màu sắc:** Xương màu vàng ngà cổ xưa, hai hốc mắt sâu hoắm rực lên ngọn lửa linh hồn màu đỏ ma quái. Mặc chiếc khố rách tả tơi.
* **Kích thước Pixel:** $48 \times 48\text{px}$ hoặc $64 \times 64\text{px}$.

### 📊 Chỉ Số Cân Bằng (`EnemyDataSO`)
* **Máu tối đa (`maxHealth`):** $35\text{ HP}$ (Máu giấy)
* **Tốc độ chạy (`moveSpeed`):** $2.8\text{m/s}$ (Đi lảo đảo)
* **Tầm đánh ném xương (`attackRange`):** $8.0\text{m}$ (Tầm xa)
* **Sát thương ném (`attackDamage`):** $12\text{ DMG}$
* **Thời gian tự hồi sinh:** $8.0\text{s}$ (sau khi rơi rụng xương)

### 🎬 Danh Mục Animation Chi Tiết
| Tên Animation Clip | Số Frame | Mô tả hành vi chuyển động |
| :--- | :--- | :--- |
| **`Skel_Idle`** | 6 frames | Bộ xương đung đưa nhẹ, các khớp xương kêu lách cách theo nhịp. |
| **`Skel_Walk`** | 6 frames | Bước đi khập khiễng, lảo đảo nhưng âm thầm không phát ra tiếng bước chân. |
| **`Skel_BoneThrow`** *(Signature 1)* | 6 frames | Rút 1 thanh xương sườn trên ngực vung tay ném thẳng về phía người chơi. |
| **`Skel_Bite`** | 4 frames | Cắn ngoạm hàm răng xương sắc nhọn khi người chơi áp sát lại gần. |
| **`Skel_Collapse`** *(Death)* | 6 frames | Toàn bộ khớp xương rụng rời rơi lả tả xuống thành đống xương vụn dưới đất. |
| **`Skel_Reassemble`** *(Signature 2)* | 8 frames | Đống xương dưới đất tự rung lên, xương sườn và đầu lâu tự bay ghép lại thành quái sau $8\text{s}$! |

> 🤖 **Prompt AI Gen Ảnh:**  
> `Pixel art sprite sheet, skeleton goblin undead monster, bone ribcage, pointy skull ears, glowing red eye sockets, sharp bone claws, T-pose, white background, isolated, 16-bit gothic fantasy asset.`

---

## 4. 🦇 SHADOW BAT — DƠI QUỶ BÓNG ĐÊM (SÁT THỦ BẦU TRỜ / HIT & RUN)

### 📌 Vai trò Gameplay
Quái vật bay lơ lửng trên không, xuất hiện khi trời tối mịt. Miễn nhiễm với các chướng ngại vật mặt đất, chuyên lượn vòng trên đầu và bất ngờ bổ nhào cắn trộm người chơi.

### 🎨 Mô Tả Ngoại Hình (Visual Concept)
* **Hình khối:** Thân hình dơi quỷ lai tiểu yêu, cánh màng da sải rộng gấp 3 lần cơ thể.
* **Màu sắc:** Lông tím than sẫm, màng cánh có gân đỏ phát sáng lờ mờ trong đêm tối, hai mắt đỏ rực không con ngươi.
* **Kích thước Pixel:** $48 \times 48\text{px}$.

### 📊 Chỉ Số Cân Bằng (`EnemyDataSO`)
* **Máu tối đa (`maxHealth`):** $30\text{ HP}$ (Rất ít máu)
* **Tốc độ bay (`moveSpeed`):** $6.0\text{m/s}$ (Nhanh nhất đảo!)
* **Tầm bay lượn (`orbitRadius`):** $4.0\text{m}$
* **Sát thương bổ nhào (`attackDamage`):** $14\text{ DMG}$

### 🎬 Danh Mục Animation Chi Tiết
| Tên Animation Clip | Số Frame | Mô tả hành vi chuyển động |
| :--- | :--- | :--- |
| **`Bat_Fly`** | 6 frames | Đập cánh dập dờn bồng bềnh trên không, đầu ngó nghiêng tìm mồi. |
| **`Bat_DiveAttack`** *(Signature)* | 8 frames | Xếp cánh lao vút xuống theo đường cong parabol cắn xoẹt qua người chơi rồi vút bay lên lại. |
| **`Bat_Bite` / `Bat_Screech`** | 5 frames | Đớp ngoạm cự ly gần hoặc phát sóng âm rung rinh làm choáng người chơi. |
| **`Bat_Hurt`** | 3 frames | Khựng lại giữa không trung, cánh co giật dữ dội khi bị đấm trúng. |
| **`Bat_Die`** | 6 frames | Cánh gãy gập, rớt cắm đầu xuống đất và tan biến thành làn khói đen bóng đêm. |

> 📄 **Tài liệu chi tiết & Prompt AI 3 góc:** Xem chi tiết tại [SHADOW_BAT_SPRITESHEET_SPEC.md](file:///C:/Users/hokha/Dropbox/PC/Downloads/Unity/FindTherLover/SHADOW_BAT_SPRITESHEET_SPEC.md)

---

## 5. 💣 MAGMA SCARAB — BỌ DUNG NHAM CẢM TỬ (QUẢ BOM SỐNG)

### 📌 Vai trò Gameplay
Con bọ cảm tử bò nhanh đến người chơi hoặc bờ rào gỗ. Khi đến gần nó dừng lại, phồng to bọc dung nham trên lưng và phát nổ tự sát, tạo áp lực buộc người chơi phải ưu tiên bắn tỉa từ xa.

### 🎨 Mô Tả Ngoại Hình (Visual Concept)
* **Hình khối:** Bọ cánh cứng 6 chân ngắn thoăn thoắt, giáp đầu bằng đá núi lửa đen bóng.
* **Điểm nhấn:** Trên lưng cõng một **bọc dung nham khổng lồ trong suốt** sôi sùng sục những bọt khí nham thạch đỏ cam phập phồng.
* **Kích thước Pixel:** $48 \times 48\text{px}$.

### 📊 Chỉ Số Cân Bằng (`EnemyDataSO`)
* **Máu tối đa (`maxHealth`):** $25\text{ HP}$
* **Tốc độ bò (`moveSpeed`):** $3.5\text{m/s}$ (Tăng lên $5.5\text{m/s}$ khi cách người chơi $< 4\text{m}$)
* **Thời gian gồng nổ (`fuseTime`):** $1.0\text{s}$
* **Bán kính nổ AOE:** $3.5\text{m}$
* **Sát thương nổ (`explosionDamage`):** $40\text{ DMG}$ (Cực khủng!)

### 🎬 Danh Mục Animation Chi Tiết
| Tên Animation Clip | Số Frame | Mô tả hành vi chuyển động |
| :--- | :--- | :--- |
| **`Scarab_Run`** | 6 frames | 6 chân đá chạy thoăn thoắt, bọc dung nham trên lưng nảy tưng tưng. |
| **`Scarab_Ignite`** *(Signature)* | 4 frames | Cắm chân xuống đất, bọc dung nham phồng to chớp đỏ liên tục kèm khói xì ra. |
| **`Scarab_Explode`** | 8 frames | Nổ tung thành quả cầu lửa nham thạch tóe ra 8 hướng, để lại vết muội than dưới đất. |
| **`Scarab_Hurt`** | 2 frames | Bị giật lùi lại một đoạn nhỏ khi trúng đạn. |

> 🤖 **Prompt AI Gen Ảnh:**  
> `Pixel art sprite, magma beetle scarab, glowing molten lava sack on its back, volcanic obsidian rock armor, 6 insect legs, glowing cracks, white background, isolated, 16-bit retro game style.`

---

## 6. 🍄 SPORE SHROOM — NẤM ĐỘC MA QUÁI (PHUN ĐỘC TẦM XA / POISON DOT)

### 📌 Vai trò Gameplay
Cây nấm độc phục kích trong các lùm cỏ rậm. Không di chuyển nhiều mà đóng vai trò như một "Ụ súng sinh học", bắn ra các bọc axit độc làm người chơi bị trừ máu theo thời gian (Poison DOT).

### 🎨 Mô Tả Ngoại Hình (Visual Concept)
* **Hình khối:** Cây nấm ma thuật đứng bằng 2 chùm rễ cây quấn lại thành đôi chân nhỏ.
* **Điểm nhấn:** Mũ nấm màu tím lục giác có đốm xanh rêu, trên đỉnh mũ có các lỗ khí xì khói độc. Dưới vành mũ là đôi mắt vàng ma quái và miệng tròn vo như nòng súng.
* **Kích thước Pixel:** $48 \times 48\text{px}$.

### 📊 Chỉ Số Cân Bằng (`EnemyDataSO`)
* **Máu tối đa (`maxHealth`):** $50\text{ HP}$
* **Tốc độ đi (`moveSpeed`):** $1.5\text{m/s}$ (Rất chậm)
* **Tầm bắn đạn độc (`attackRange`):** $8.0\text{m}$
* **Sát thương trực tiếp:** $10\text{ DMG}$
* **Hiệu ứng Độc Dược (Poison DOT):** Trừ tiếp $2\text{ HP/giây}$ trong $5\text{s}$ (Tổng mất $20\text{ HP}$)

### 🎬 Danh Mục Animation Chi Tiết
| Tên Animation Clip | Số Frame | Mô tả hành vi chuyển động |
| :--- | :--- | :--- |
| **`Shroom_Idle`** | 4 frames | Thân nấm nhún nhảy đàn hồi như thạch, mũ nấm phập phồng xì khói độc nhẹ. |
| **`Shroom_Walk`** | 6 frames | Hai cái rễ cây bước đi lạch bạch hài hước nhưng ma mị. |
| **`Shroom_Spit`** *(Signature)* | 6 frames | Thân nấm co thắt hút một hơi căng phồng $\rightarrow$ Bắn phụt viên đạn bào tử độc ra xa. |
| **`Shroom_Hurt`** | 2 frames | Thân nấm méo mó bẹp dúm lại rồi bật đàn hồi về vị trí cũ. |
| **`Shroom_Die`** | 5 frames | Mũ nấm xì hết hơi, xẹp lép rơi phịch xuống đất như bánh xẹp. |

> 🤖 **Prompt AI Gen Ảnh:**  
> `Pixel art sprite, evil toxic mushroom monster, purple and green mushroom cap with glowing spores, creepy yellow eyes beneath cap, small root feet, shooting poison bubbles, white background, 16-bit RPG.`

---

## 7. 👻 SHADOW WRAITH — U LINH KHÓA CHÂN (LƯỚT XUYÊN VẬT CẢN)

### 📌 Vai trò Gameplay
Bóng ma xuất hiện từ đêm thứ 3 trở đi. Khả năng đáng sợ nhất: **Đi xuyên qua cây cối và vách đá**, dùng phép thuật trói chân người chơi để bầy Goblin lao vào tiêu diệt.

### 🎨 Mô Tả Ngoại Hình (Visual Concept)
* **Hình khối:** Bóng ma bay lơ lửng không chân, khoác áo choàng tơi tả rách rưới màu xanh lam ma trĩu nặng.
* **Điểm nhấn:** Hai bàn tay xương xẩu dài ngoằng lơ lửng, mặt khuất trong bóng tối chỉ lộ 2 đốm lửa linh hồn xanh lạnh lẽo (Soul Fire). Thân thể nửa trong suốt (Alpha = 70%).
* **Kích thước Pixel:** $64 \times 64\text{px}$.

### 📊 Chỉ Số Cân Bằng (`EnemyDataSO`)
* **Máu tối đa (`maxHealth`):** $60\text{ HP}$
* **Tốc độ lướt (`moveSpeed`):** $3.0\text{m/s}$ (Bỏ qua va chạm cây cối)
* **Tầm thi triển phép (`castRange`):** $4.0\text{m}$
* **Kỹ năng Khóa Chân (`Soul Freeze`):** Đóng băng tốc độ chạy của người chơi ($moveSpeed = 0$) trong $1.5\text{s}$!

### 🎬 Danh Mục Animation Chi Tiết
| Tên Animation Clip | Số Frame | Mô tả hành vi chuyển động |
| :--- | :--- | :--- |
| **`Wraith_Hover`** | 6 frames | Áo choàng bay phấp phới bồng bềnh trong gió lạnh, đốm lửa trong mắt nhấp nháy. |
| **`Wraith_Cast`** *(Signature)* | 6 frames | Giơ 2 bàn tay xương lên cao, triệu hồi bàn tay ma quỷ từ dưới đất trói chân Player. |
| **`Wraith_Slash`** | 4 frames | Vung móng vuốt ma lạnh buốt chém ngang người gây $15\text{ DMG}$. |
| **`Wraith_Hurt`** | 2 frames | Thân thể nhạt nhòa chập chờn như bóng đèn sắp tắt. |
| **`Wraith_Disperse`** | 6 frames | Tan biến thành một làn sương mù xanh lam tan loãng vào không khí. |

> 🤖 **Prompt AI Gen Ảnh:**  
> `Pixel art sprite, floating ghost wraith monster, ragged cyan hood and cloak, glowing spectral blue eyes, skeletal claw hands, transparent ethereal body, spooky soul reaper, white background, isolated, 16-bit.`

---

## ⚙️ QUY CHUẨN KỸ THUẬT IMPORT SPRITESHEET VÀO UNITY 6

Để toàn bộ SpriteSheet của 7 con quái vật hiển thị **sắc nét $100\%$, không bị nhòe và tiếp đất chuẩn 3D**, bắt buộc cài đặt trong Inspector của Unity:

1. **Texture Type:** `Sprite (2D and UI)`
2. **Sprite Mode:** `Multiple`
3. **Pixels Per Unit (PPU):** Cố định **`16`** hoặc **`32`** (phải trùng với PPU của nhân vật Quene).
4. **Filter Mode:** **`Point (no filter)`** *(BẮT BUỘC để giữ góc cạnh pixel sắc sảo)*.
5. **Compression:** **`None`** (để giữ màu sắc tương phản rực rỡ nhất).
6. **Pivot Alignment:** Trong Sprite Editor, luôn chỉnh Pivot về **`Bottom`** (ngay giữa hai gót chân quái vật) để chân quái luôn chạm chuẩn mặt đất 3D!
