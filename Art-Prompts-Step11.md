# Kapı ve görevli boyutu — Adım 11

Built-in imagegen ile yeni santral çelik kapı görseli üretildi. Kaynak: `exec-4b7b310e-90bf-4720-a990-3d92df20320a.png`.

Projedeki görsel: `Assets/Art/Companion/SantralSteelGate.png`, 809x1942 RGBA, alfa 0..255. Yerleşimde şeffaf kenarlar kalıcı Unity sprite kesitiyle çıkarılır: `Assets/Art/Companion/SantralSteelPanel.asset`. Kaynak PNG değiştirilmez. Sığınak kapısı kendi görselini kullanmaya devam eder.

Prompt:
Use case: stylized-concept. Asset type: transparent PNG sprite of ONE realistic reinforced steel sliding security gate PANEL for a gritty Chernobyl 2D top-down pixel-art game. Whole isolated object, no environment, no people, no text or UI. The panel stands along a north-south fence line on the ground: its LONG axis is VERTICAL in the image, ratio WIDTH 1 to HEIGHT 3.2. Perspective: orthographic viewed steeply from above with subtle side thickness, compatible with a top-down game ground plane, NOT an eye-level doorway, NOT a front facade. An aged heavy industrial sliding barrier with dark weathered gray-green steel plates, real bolts and hinges, rust streaks, scratched paint, central reinforcement rails, small yellow-black caution strips at both vertical ends, chunky welded construction. Restrained detailed gritty pixel art with realistic materials and clear silhouette, warm overhead light. No outer fixed doorway frame (the engine adds that), no attached wall, no ground, no cast shadow/background, no pad around the panel; fill the canvas tightly with the complete single tall slender panel. Genuine transparent alpha outside the object.

`Cernobil → Adim 11 - Gercekci Kapi ve Gorevli Boyutu` mevcut `14_FinalBolum2` ve `15_SonTahliye` sahnelerini düzenler. İkinci bölümde sabit kasa, panelin çekildiği maskeli açıklık, motor muhafazası ve durum lambası eklenir. Açılma 1,15 saniye sürer; çarpışma animasyon sonunda kalkar ve gerçek NavMesh güncellemesi istenir. Robot emir/panel yetkisi ve fiziksel geçit ölçüsü korunur.

Üçüncü bölüm görevlisinin yüksekliği kaynak görsellerin alfa alanları ölçülerek oyuncunun gerçek görünür yüksekliğine eşitlenir. Oran korunur; yalnız tek eksende esnetilmez. Konuşma portresinin boyutu değiştirilmez.

Çalışma zamanı ve Editor kodları derlendi. Menü gerçek Unity editöründe çalıştırıldı ve iki sahne kaydedildi. Kaynak alfa alanına göre görevlinin görünür boyu 2,25 → 2,49 birim olarak ölçüldü; oyuncu da 2,49 birim. İkinci bölümde kapının kapalı görünümü editörün Game penceresinde kontrol edildi. Yeni kapının açılma animasyonu bu düzenleme sonrasında canlı oynatılarak tekrar test edilmedi.

