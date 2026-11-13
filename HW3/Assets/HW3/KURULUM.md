# SWE 585 HW3 – Maze Escape: Hızlı Kurulum (≈10 dk)

## 1. Mevcut HW3 projeni aç
- Unity Hub → `SWE585-Fall2026/HW3` (Unity 6000.3.25f1, URP).
- Zip'teki `HW3/Assets/HW3` klasörünü Finder'da **senin projendeki `HW3/Assets/` içine** sürükle (Assets/Materials, Assets/Scenes'in yanına). Unity derlemeyi kendisi yapar.
- Zip'teki `.gitignore`'a gerek yok, reponda zaten var.

## 2. Sahneyi tek tıkla kur
1. `Assets/Scenes/MazeScene` sahneni aç.
2. Menü: **Tools → SWE585 → Build Maze Escape Scene**
   - Senin `WallMat` ve `GroundMat` materyallerin kullanılır.
   - Eski `Ground` ve `Maze` objelerin **silinmez, sadece kapatılır** (Hierarchy'de gri görünür, istersen sonra sil).
3. **Cmd+S** ile kaydet. (Builder sahneyi Build Settings'e de ekler.)
4. **Play**.

> Hata alırsan: Console'da `InvalidOperationException ... Input System` varsa →
> `Edit → Project Settings → Player → Active Input Handling = Both`. (Kod iki sistemi de destekliyor, normalde gerekmez.)

## 3. Müzik
GameManager'daki AudioSource'a **kendi müzik dosyanı** (mp3/ogg) sürükle. Boş bırakırsan kod kendi küçük melodisini üretir — her hâlükârda çalışır. **M** tuşu veya sol üstteki buton ile aç/kapa.

## 4. Kontroller
WASD hareket · E anahtarı al/bırak · Shift koş · Space zıpla · Mouse kamera · M müzik · Esc imleci serbest bırak · (oyun bitince) R / “Play Again”

## Haritadaki yollar (ince duvarlı, 9×7 oda)
```
#D#################   D = Çıkış kapısı (sol üst)
#.....#.#.....#..K#   K = Anahtar (sağ üst)
#.#####.#####.#.#.#
#.....#.........#.#
###.#.#####.#.###.#
#...#...#...#.#...#
#.#########.#####.#
#...#A.........B#T#   A↔B = Gardiyan koridoru, T = Diken tuzağı
#.#######.#######.#
#.....#.#.....#...#
#.#.###.#.#######.#
#.#...#.#.#.#...#.#
###.###.#.#.#.###.#
#........S........#   S = Başlangıç
###################
```
- **Sol yol** (kıvrımlı) → kapı. **Orta yol** → gardiyan koridorunu karşıdan karşıya geç → anahtar. **Sağ yol** → diken tuzağı → anahtar. Bol çıkmaz sokak var.
- Gardiyan: **değersen** ölürsün; ayrıca tam önünde ~2.5 m'ye girersen yakalanırsın (ödevdeki "önünden geçip çok yaklaşırsa" kuralı). Sadece değince ölsün istersen Guard objesinde **Touch Only** kutusunu işaretle.

Haritayı değiştirmek istersen `Editor/MazeSceneBuilder.cs` içindeki `Layout` dizisini düzenle ve menüden tekrar çalıştır.

## Notlandırma ↔ kod eşleşmesi
| Kriter | Nerede |
|---|---|
| Labirent, ≥2 yol (20%) | `MazeSceneBuilder` – 3 yol |
| Oyuncu hareketi (5%) | `PlayerController` (Rigidbody, kamera-göreli) |
| Anahtarı itme (10%) | `KeyObject` – Rigidbody küp (kuvvet, çarpışma, yerçekimi); ayrıca E ile alınıp taşınabilir |
| Anahtar+kapı = oyun biter (10%) | `Door.OnCollisionEnter` |
| Tuzak periyodik (10%) | `SpikeTrap` – **Coroutine** (güvenli → uyarı → diken → geri) |
| Tuzak & gardiyan öldürür (10%) | `TrapKillZone`, `Guard.CanSeePlayer` (görüş konisi + duvar kontrolü + çok yakınlık) |
| Müzik aç/kapa (10%) | `GameManager.ToggleMusic` |
| Gardiyan periyodik hareket (10%) | `Guard.Patrol` – **Coroutine** A↔B |
| Akıcılık/estetik (15%) | Kamera çarpışması, ışıklar, uyarı yanıp sönmesi, süre sayacı, ipuçları |
| **Bonus +10%** tekrar oyna | Kazanınca/ölünce “Play Again / Quit” paneli |
| **Bonus +10%** Asset Store | Aşağıya bak |

## Asset Store bonusu (zaman kalırsa, ~20 dk)
Ücretsiz paketler önerisi: duvar/zemin için bir “dungeon/maze” paketi, karakter için Unity'nin “Starter Assets” veya herhangi bir ücretsiz karakter. Kolay yol: modeli ilgili objenin **çocuğu** yap, objenin kendi `MeshRenderer`'ını kapat — collider ve scriptler aynen çalışır (Player, Guard, Key, Door, Walls).

## Video (< 3 dk) senaryosu
1. 0:00 Haritanın üstten görünüşü (Scene view) – 3 yolu göster.
2. 0:15 Hareket + kamera, müzik M ile kapat/aç.
3. 0:30 Kapıya anahtarsız git → “Locked” uyarısı.
4. 0:45 Diken tuzağına bilerek gir → ölüm → “Play Again”.
5. 1:10 Gardiyanın önüne çık → yakalanma → “Play Again”.
6. 1:35 Gardiyanın arkasından süzül / tuzağı zamanlayarak geç → anahtarı it.
7. 2:20 Anahtarı kapıya çarptır → kapı açılır → “YOU ESCAPED” → Play Again.

## GitHub
```bash
git add HW3
git commit -m "HW3 Maze Escape"
git push
```
(Repo'da klasör adı **HW3** olmalı; `Library/` push edilmez, .gitignore bunu halleder. `ProjectSettings/` ve `Packages/` push edilmeli.)
