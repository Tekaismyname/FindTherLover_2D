# 🦇 SHADOW BAT (DƠI QUỶ BÓNG ĐÊM) — TÀI LIỆU QUY CHUẨN SPRITESHEET ANIMATION & AI PROMPTS

> **Dự án:** Find the Lover — 2.5D / HD-2D Survival Adventure (Unity 6 URP)  
> **Chủng loài quái:** Aerial Assassin / Hit & Run (Quái bay sát thủ)  
> **Phong cách đồ họa:** 16-bit / 32-bit Gothic Fantasy Pixel Art, viền nét sạch (Clean Outlines), màu sắc tương phản cao hỗ trợ URP Lit 2D/3D.  
> **Độ phân giải khung hình khuyên dùng:** $64 \times 64\text{px}$ (khuyên dùng) hoặc $48 \times 48\text{px}$.

---

## 🧭 1. KIẾN TRÚC GÓC NHÌN 3 HƯỚNG (3-DIRECTIONAL SYSTEM)

Trong game 2.5D Top-Down / Isometric, chúng ta tối ưu hóa công sức vẽ và bộ nhớ bằng cách **chỉ tạo 3 hướng nhìn chính**. Hướng còn lại (Trái) sẽ được xử lý tự động trong mã nguồn Unity:

```text
                        ▲ [Back / Up] (_bw)
                        │ (Nhìn từ sau lưng)
                        │
 [Left] (Tự động) ◄─────┼─────► [Right / Side] (_r)
 (Lật FlipX từ _r)      │       (Nhìn nghiêng sang phải)
                        │
                        ▼ [Front / Down] (_fw)
                          (Nhìn thẳng mặt / góc máy chính)
```

* **`_fw` (Front / Down):** Quái bay chúc xuống hoặc bay thẳng về phía camera (góc nhìn mặc định ấn tượng nhất).
* **`_bw` (Back / Up):** Quái bay ngược lên phía trên màn hình (lưng quay về phía người chơi).
* **`_r` (Right / Side):** Quái bay ngang từ trái sang phải.
* **Hướng Trái (`Left`):** Không cần tạo sprite riêng! Unity sẽ tự động kích hoạt `SpriteRenderer.flipX = true` khi vận tốc $velocity.x < 0$.

---

## 🎬 2. DANH MỤC 5 ACTIONS CHI TIẾT (FULL ACTION MATRIX)

Bảng tổng hợp toàn bộ 5 hành động của Shadow Bat, số lượng khung hình (Frame Count) và tốc độ hoạt ảnh lý tưởng:

| STT | Tên Animation Clip | Số Frame | Tốc độ (FPS) | Tính chất Loop | Mô tả chuyển động chi tiết |
| :---: | :--- | :---: | :---: | :---: | :--- |
| **1** | **`Bat_Fly`** *(Idle/Move)* | **6 frames** | 10 - 12 FPS | Loop tuần hoàn | Đập cánh nhịp nhàng nâng hạ cơ thể bồng bềnh theo hình sin. Đôi mắt đỏ rực than hồng nhấp nháy, tai nhọn vểnh cao lùng sục mục tiêu. |
| **2** | **`Bat_DiveAttack`** *(Signature)* | **8 frames** | 14 - 16 FPS | Play Once | **Đòn bổ nhào:** Co cánh lấy đà bay vút lên đỉnh $\rightarrow$ Khép cánh lao dốc xé gió theo đường cong parabol $\rightarrow$ Xòe cánh phanh gấp cắn lướt qua mục tiêu $\rightarrow$ Lượn bốc đầu trở lại không trung. |
| **3** | **`Bat_Bite` / `Bat_Screech`** | **5 frames** | 12 FPS | Play Once | **Đớp cắn / Rít sóng âm:** Há toang mồm để lộ cặp răng nanh sắc nhọn, chồm tới cắn ngoạm cự ly gần hoặc phát ra sóng âm làm choáng nhẹ người chơi. |
| **4** | **`Bat_Hurt`** | **3 frames** | 12 FPS | Play Once | **Nhận sát thương:** Thân thể khựng lại giữa không trung, cánh co giật quặp lại vì đau đớn, các vệt máu đen và lông tơ tím bung ra. |
| **5** | **`Bat_Death`** | **6 frames** | 12 FPS | Play Once | **Tử trận:** Mất lực bay, cánh gãy gập rớt cắm đầu xuống đất $\rightarrow$ Cơ thể phát nổ thành làn khói bóng đêm (Shadow Mist) rồi tan biến hoàn toàn. |

---

## 📐 3. BẢNG PHÂN BÃ MÃ TÊN FILE TIÊU CHUẨN (ASSET NAMING CONVENTION)

Tất cả các file sprite sheet nên được xuất dưới dạng **Dải ngang 1 hàng ($1 \times N$)** để đạt độ phân giải và chất lượng sắc nét cao nhất:

```text
Assets/08_Art/Sprites/Enemies/ShadowBat/
├── Bat_Fly_fw.png          (1 hàng x 6 frames = 384 x 64 px)
├── Bat_Fly_bw.png          (1 hàng x 6 frames = 384 x 64 px)
├── Bat_Fly_r.png           (1 hàng x 6 frames = 384 x 64 px)
│
├── Bat_DiveAttack_fw.png   (1 hàng x 8 frames = 512 x 64 px)
├── Bat_DiveAttack_bw.png   (1 hàng x 8 frames = 512 x 64 px)
├── Bat_DiveAttack_r.png    (1 hàng x 8 frames = 512 x 64 px)
│
├── Bat_Bite_fw.png         (1 hàng x 5 frames = 320 x 64 px)
├── Bat_Bite_bw.png         (1 hàng x 5 frames = 320 x 64 px)
├── Bat_Bite_r.png          (1 hàng x 5 frames = 320 x 64 px)
│
├── Bat_Hurt_fw.png         (1 hàng x 3 frames = 192 x 64 px)
├── Bat_Hurt_bw.png         (1 hàng x 3 frames = 192 x 64 px)
├── Bat_Hurt_r.png          (1 hàng x 3 frames = 192 x 64 px)
│
├── Bat_Death_fw.png        (1 hàng x 6 frames = 384 x 64 px)
├── Bat_Death_bw.png        (1 hàng x 6 frames = 384 x 64 px)
└── Bat_Death_r.png         (1 hàng x 6 frames = 384 x 64 px)
```

---

## ⚙️ 4. QUY TRÌNH IMPORT & CẤU HÌNH TRONG UNITY 6 URP

### 4.1. Thông số Inspector của Texture
Sau khi kéo thả ảnh PNG vào Unity, chọn ảnh và thiết lập trong Inspector:
1. **Texture Type:** `Sprite (2D and UI)`
2. **Sprite Mode:** `Multiple`
3. **Pixels Per Unit (PPU):** Cố định **`16`** hoặc **`32`** (phải đồng bộ $100\%$ với Player và Goblin để không bị lệch kích thước).
4. **Filter Mode:** **`Point (no filter)`** *(Bắt buộc đối với Pixel Art để ảnh không bị mờ nhòe)*.
5. **Compression:** **`None`** (đảm bảo độ trong suốt alpha sạch tuyệt đối và màu không bị vỡ hạt nén).

### 4.2. Cấu hình Pivot đặc thù cho Quái Bay (Aerial Unit)
Khác với Goblin đứng trên mặt đất (Pivot đặt ở `Bottom`), Shadow Bat là quái vật **bay lơ lửng trên không trung**:
* **Phương án chuẩn (Khuyên dùng):** Trong **Sprite Editor**, đặt Pivot ở **`Custom` (X = 0.5, Y = 0.2)** hoặc **`Center`**.
* **Bóng đổ mặt đất (Shadow Decal):** Tạo 1 GameObject con tên `Shadow_Decal` mang Sprite một hình elip màu đen bán trong suốt ($Alpha = 40\%$), đặt tại vị trí mặt đất ($Y = 0$). Khi quái bay nhấp nhô lên xuống, bóng dưới đất vẫn giữ cố định trên mặt địa hình 3D, tạo chiều sâu không gian 2.5D cực kỳ chân thật!

---

## 🤖 5. THƯ VIỆN PROMPT AI GENERATION (MIDJOURNEY / DALL-E / SDXL)

### 5.1. Negative Prompt Dùng Chung
Dán đoạn này vào ô **Negative Prompt** để loại bỏ hoàn toàn các lỗi thường gặp:

```text
checkered background, fake transparency grid, scenery, landscape, 3D render, realistic shading, smooth gradients, blurry edges, inconsistent scale, text, watermark, signature, border frames, missing wings, extra limbs, human legs, disjointed sprites, bad anatomy, deformed wings, compression artifacts.
```

---

### 5.2. Prompt Action 1: `Bat_Fly` (Bay lơ lửng — 1 hàng $\times$ 6 Frames)

#### Góc Trước (Front View — `Bat_Fly_fw`):
```text
A professional 2D sprite strip of a shadow vampire bat monster, hovering and flying forward facing directly toward the camera. Clean 32-bit pixel art, dark gothic fantasy aesthetic.

LAYOUT:
- Exactly 1 single horizontal row of 6 sequential frames (1x6 horizontal sprite sheet).
- Viewpoint: Orthographic front view (facing forward).

CHARACTER DESIGN:
- Menacing small demonic bat body with charcoal purple velvet fur.
- Large leathery bat wings with glowing crimson veins stretched wide.
- Pointed ears, glowing neon-red eyes with no pupils, sharp white fangs slightly exposed.

ANIMATION CYCLE (SEAMLESS 6-FRAME HOVER LOOP):
- Frame 1: Wings at highest flap peak, body slightly dipping.
- Frame 2: Wings forcefully pushing downwards, body lifting.
- Frame 3: Wings fully extended at lowest position, maximum thrust.
- Frame 4: Wings begin gliding upward curve, body floating at apex height.
- Frame 5: Wings recovering upward, body descending slightly.
- Frame 6: Wings returning to top position, seamlessly connecting back to Frame 1.

BACKGROUND:
- Pure solid white background (#FFFFFF) or transparent PNG with background completely removed.
- 6 isolated, non-touching sprites aligned along the same horizontal centerline, no floor shadow.
```

#### Góc Sau (Back View — `Bat_Fly_bw`):
```text
A professional 2D sprite strip of a shadow vampire bat monster, hovering and flying forward with back facing the camera. Clean 32-bit pixel art, dark gothic fantasy aesthetic.

LAYOUT:
- Exactly 1 single horizontal row of 6 sequential frames (1x6 horizontal sprite sheet).
- Viewpoint: Orthographic back view (facing away from viewer).

CHARACTER DESIGN:
- Back of dark purple furry body, pointed demon ears seen from behind.
- Broad wingspan showing dark leathery wing texture and subtle red pulses.

ANIMATION CYCLE:
- Flapping cycle identical to front view: Up-stroke to down-stroke seamless 6-frame loop.

BACKGROUND:
- Pure solid white background (#FFFFFF) or transparent PNG, 6 isolated sprites.
```

#### Góc Nghiêng Phải (Right Side View — `Bat_Fly_r`):
```text
A professional 2D sprite strip of a shadow vampire bat monster, flying horizontally towards the right. Clean 32-bit pixel art, dark gothic fantasy aesthetic.

LAYOUT:
- Exactly 1 single horizontal row of 6 sequential frames (1x6 horizontal sprite sheet).
- Viewpoint: Side profile view facing right.

CHARACTER DESIGN:
- Streamlined aerodynamic body tilted slightly forward, fanged snout pointed right.
- Near wing flapping fully in foreground, far wing visible in background offset.

ANIMATION CYCLE:
- Dynamic horizontal wing stroke cycle pushing air backwards, seamless 6-frame loop.

BACKGROUND:
- Pure solid white background (#FFFFFF) or transparent PNG, 6 isolated sprites.
```

---

### 5.3. Prompt Action 2: `Bat_DiveAttack` (Bổ nhào tấn công — 1 hàng $\times$ 8 Frames)

#### Góc Trước (Front View — `Bat_DiveAttack_fw`):
```text
A professional 2D sprite strip of a shadow vampire bat performing an aggressive dive-bomb attack straight toward the viewer. Clean 32-bit pixel art, dynamic dark fantasy action.

LAYOUT:
- Exactly 1 single horizontal row of 8 sequential frames (1x8 horizontal sprite sheet).
- Viewpoint: Front view facing directly forward.

ANIMATION PHASES (8 FRAMES PLAY ONCE):
- Frame 1-2 (Anticipation): Wings pull backward high above head, jaws open wide with glowing red eyes charging dark energy.
- Frame 3-5 (Dive & Strike): Folds wings tightly like a dart, plunging downward at violent speed, claws out reaching forward, mouth gaping with fangs ready to bite.
- Frame 6-7 (Follow-through): Wings snap wide open catching air to brake momentum, slashing motion across target.
- Frame 8 (Recovery): Scoops upward with powerful wing push, leveling back into hovering posture.

BACKGROUND:
- Pure solid white background (#FFFFFF) or transparent PNG, 8 isolated sprites horizontally aligned.
```

#### Góc Nghiêng Phải (Right Side View — `Bat_DiveAttack_r`):
```text
A professional 2D sprite strip of a shadow vampire bat executing a steep parabolic swooping dive attack to the right. Clean 32-bit pixel art.

LAYOUT:
- Exactly 1 single horizontal row of 8 sequential frames (1x8 horizontal sprite sheet).
- Viewpoint: Side view facing right.

ANIMATION PHASES:
- Frame 1-2: Coils body back in air.
- Frame 3-5: Streams downward diagonally at 45 degrees, wings tucked tightly in bullet shape.
- Frame 6-7: Swoops at bottom of curve, raking claws horizontally across target.
- Frame 8: Arcs upward regaining cruising altitude.

BACKGROUND:
- Pure solid white background (#FFFFFF) or transparent PNG, 8 isolated sprites.
```

---

### 5.4. Prompt Action 3: `Bat_Bite` / `Bat_Screech` (Đớp cắn / Sóng âm — 1 hàng $\times$ 5 Frames)

```text
A professional 2D sprite strip of a shadow vampire bat monster performing a fierce bite and ultrasonic screech attack. Clean 32-bit pixel art, front view.

LAYOUT:
- Exactly 1 single horizontal row of 5 sequential frames (1x5 horizontal sprite sheet).

ANIMATION SEQUENCE:
- Frame 1: Pulls head back, jaw starting to unhinge.
- Frame 2: Mouth opens fully into a terrifying screech, sound wave ripples or dark sonic energy pulses emanate.
- Frame 3: Lunges head forward snapping razor-sharp fangs shut with bite impact.
- Frame 4: Chewing/snarl follow-through.
- Frame 5: Returns to normal flying posture.

BACKGROUND:
- Pure solid white background (#FFFFFF) or transparent PNG, 5 isolated sprites.
```

---

### 5.5. Prompt Action 4: `Bat_Hurt` (Bị thương — 1 hàng $\times$ 3 Frames)

```text
A professional 2D sprite strip of a shadow vampire bat taking damage, reeling backward in mid-air. Clean 32-bit pixel art.

LAYOUT:
- Exactly 1 single horizontal row of 3 sequential frames (1x3 horizontal sprite sheet).

ANIMATION SEQUENCE:
- Frame 1: Severe hit reaction, head knocked back, wings violently buckling inward, eyes flashing white.
- Frame 2: Body recoils backward in air, dark shadow droplets splatter.
- Frame 3: Stabilizing wings, regaining flight equilibrium.

BACKGROUND:
- Pure solid white background (#FFFFFF) or transparent PNG, 3 isolated sprites.
```

---

### 5.6. Prompt Action 5: `Bat_Death` (Tử trận & Khói bóng đêm — 1 hàng $\times$ 6 Frames)

```text
A professional 2D sprite strip of a shadow vampire bat monster dying and disintegrating into dark shadow mist. Clean 32-bit pixel art.

LAYOUT:
- Exactly 1 single horizontal row of 6 sequential frames (1x6 horizontal sprite sheet).

ANIMATION SEQUENCE:
- Frame 1: Lethal hit, wings go limp and fold unnaturally, eyes lose their red glow.
- Frame 2: Plummeting downward head-first through the air.
- Frame 3: Impacts the ground with wings sprawled out.
- Frame 4: Body cracks with dark purple energy, bursting into black shadow particles.
- Frame 5: Disintegrating into a dissipating cloud of dark smoke and fading embers.
- Frame 6: Only a vanishing wisp of smoke remains, completely dissolving into air.

BACKGROUND:
- Pure solid white background (#FFFFFF) or transparent PNG, 6 isolated sprites.
```

---

## 🎮 6. TÍCH HỢP VÀO UNITY ANIMATOR CONTROLLER (`Bat_Animator`)

Khi đưa vào Unity, bạn tạo `Bat_Animator.controller` có cấu trúc tương tự `Goblin_Animator`:

```text
               ┌─────────────┐
               │    Entry    │
               └──────┬──────┘
                      │
                      ▼
               ┌─────────────┐   Trigger "Attack_Dive"   ┌─────────────────┐
               │  Fly State  ├──────────────────────────►│ DiveAttack State│
               │ (BlendTree) │◄──────────────────────────┤   (BlendTree)   │
               └──────┬──────┘       Has Exit Time       └─────────────────┘
                      │
                      ├───────── Trigger "Hurt" ────────►┌─────────────────┐
                      │                                  │   Hurt State    │
                      ├───────── Bool "IsDead" ─────────►├─────────────────┘
                      ▼                                  │
               ┌─────────────┐                           ▼
               │ Death State │                    ┌─────────────────┐
               └─────────────┘                    │   Dead State    │
                                                  └─────────────────┘
```

* **Fly State (Blend Tree 2D Simple Directional):**
  * Tham số: `MoveX` (Float), `MoveY` (Float).
  * `(0, -1)` $\rightarrow$ `Bat_Fly_fw`
  * `(0, 1)` $\rightarrow$ `Bat_Fly_bw`
  * `(1, 0)` $\rightarrow$ `Bat_Fly_r`
  * *(Code tự lật `flipX` khi `MoveX < 0`)*.
* **DiveAttack State (Blend Tree 2D Simple Directional):**
  * Tương tự gán `Bat_DiveAttack_fw`, `Bat_DiveAttack_bw`, `Bat_DiveAttack_r`.
* **Trạng thái Tử Trận (`IsDead == true`):**
  * Bật State `Death`, tắt `Collider` và tắt `NavMeshAgent` (hoặc logic bay) để quái rớt xuống và biến mất mượt mà.
