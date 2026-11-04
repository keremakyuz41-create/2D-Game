# Tel çit görseli — Adım 12

Built-in imagegen ile şeffaf tel çit görseli üretildi. Kaynak: `exec-895027d0-5f95-49d6-a390-006a41aab5a5.png`.

Projedeki dosya: `Assets/Art/Companion/SantralWireFence.png`, 769x2045 RGBA, alfa 0..255. Kalıcı Unity sprite kesiti: `Assets/Art/Companion/SantralWireFenceTile.asset`. Görseldeki tel gözleri şeffaftır; arkasındaki asfalt görünür.

Prompt:
Use case: stylized-concept. Asset type: transparent PNG vertical chain-link security fence segment for a gritty Chernobyl 2D overhead pixel-art game. A single slender tall chain-link wire fence strip running NORTH-SOUTH vertically in the image, overall rectangle width to height ratio 1:4. Diamond woven dark galvanized wire with rust and metallic highlights, two reinforced narrow side tension rails, a little bent razor wire along one edge. Viewed from a steep overhead orthographic three-quarter perspective suitable for a fence line on the XY ground of a top-down game; no background ground or wall. Crisp gritty realistic pixel art, muted dark gray steel, brown corrosion, subtle moss only at tiny fastenings. Very important: the DIAMOND HOLES ARE TRULY TRANSPARENT ALPHA so asphalt behind the fence is visible. The whole exterior background is transparent too. No opaque fill between wires, no green flat panels, no yellow bars, no blue neon. No doorway, no people, no signage, no text or UI. Complete sprite with little margin, no scene, no shadow backdrop. Repeatable straight fence section, visually readable at small game scale.

Menü: `Cernobil → Adim 12 - Santral Tel Cit ve Servis Gecidi`. Mevcut `14_FinalBolum2` içindeki yeşil alt/orta/üst duvar görselleri tel çitle değiştirilir; eski yeşil pompa görseli mevcut metal panel/dolap sanatıyla değiştirilir. Robotun turkuaz zemin işareti yerine koyu metal çerçeveli bakım açıklığı çizilir. Eski collider ve NavMesh engelleri aktif kalır; düzenleme yalnız SpriteRenderer nesnelerini etkiler.

Editor kodu derlendi. Menü gerçek Unity editöründe çalıştırıldı ve `14_FinalBolum2` kaydedildi. İkinci bölüm Game görünümünde kontrol edildi: yeşil çubuklar ve turkuaz servis işaretleri görünmüyor; kapının iki yanında şeffaf tel örgü var. Düzenleme öncesi/sonrası 20 çarpışma nesnesi bulundu; yeni collider eklenmedi. Robotun uçtan uca görev testi bu görsel düzenleme sonrasında yeniden çalıştırılmadı.

