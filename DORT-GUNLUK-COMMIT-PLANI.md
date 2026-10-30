# 28–31 Ekim: dört günlük geliştirme planı

Bu belge bir çalışma planıdır; geçmişte bu sırayla commit atıldığı iddiası değildir. Mevcut dosya kayıtlarında temel oyun ve robot 28 Ekim'de, NPC/final/düzeltmeler 30 Ekim'de görünür. 29 Ekim aşağıda önerilen iş dağılımıdır; 31 Ekim henüz yapılacak test/teslim günüdür. Bugün oluşturulan commitlere geçmiş tarih verilmez.

Kodlar anlaşılması için kısaltılmış örneklerdir; Unity projesinin tamamının veya bağımlı sınıfların yerine geçmez.

## 1. gün — 28 Ekim 2026: temel oyun

İşler: Unity 2D proje yapısı, oyuncu hareketi ve kamera, ilk atış/zombi/can prototipi.

Commit 1:

```text
feat: Unity 2D projesini ve oyuncu hareketini oluştur
```

```text
WASD hareketi, çapraz hız sınırı, fiziksel çarpışmalar ve kamera takibi eklendi.
İlk hareket deneme sahnesi hazırlandı.
```

Commit 2:

```text
feat: ateş etme, zombi takibi ve can sistemini ekle
```

```text
Fareyle nişan, şarjör, yeniden doldurma ve yavaş zombi takibi eklendi.
Oyuncunun hasar alması ve ölünce mevcut bölümü yeniden denemesi sağlandı.
```

Kısa kod — çapraz giderken hızın artmasını önleyen yön okuma:

```csharp
Vector2 direction = new Vector2(
    Input.GetAxisRaw("Horizontal"),
    Input.GetAxisRaw("Vertical"));
direction = Vector2.ClampMagnitude(direction, 1f);
```

İlgili kaynaklar: `Assets/Scripts/Player/KeyboardMovementInput.cs`, `PlayerMovement.cs`, `Assets/Scripts/Combat`, `Assets/Scripts/Enemies`, `CameraFollow.cs`.

Gün sonu kontrolü: hareket, engellerde durma, ateş, R ile doldurma ve zombiden hasar alma.

## 2. gün — 29 Ekim 2026: önerilen bölüm ve robot iş paketi

Bu gün, dört günlük plan içindeki önerilen dağılımdır. Bu işlerin mevcut dosyaları gerçekte 28 Ekim tarihli kayıtlar içerir; 29 Ekim'de yapılmış gibi sunulmamalı.

İşler: ikinci bölüm, beş zombi, kutudan tüfek, sahneler arasında oyuncu durumu, robotun servis yolundan kapıyı açması, koşu ve silah görselleri.

Commit 1:

```text
feat: ikinci bölüm ve kutudan alınan otomatik tüfeği ekle
```

```text
Sağ çıkıştan ikinci bölüme geçiş ve beş zombili bölüm kuruldu.
Kutu kırılınca tüfek ortaya çıkıyor, E ile alınıyor ve seri ateş ediyor.
Can, silah ve şarjör bilgileri sonraki bölüme aktarılıyor.
```

Commit 2:

```text
feat: NavMesh tabanlı robot ve kilitli kapı bulmacasını ekle
```

```text
Robotun E emriyle gerçek NavMesh üzerinden dar servis geçidini kullanması
ve kontrol panelini çalıştırması eklendi. Oyuncu dar geçitten geçemiyor.
```

Kısa kod — gerçek NavMeshAgent için hedef belirleme:

```csharp
if (!agent.SetDestination(target.position))
{
    Fail("Robot hedef için yol isteği başlatamadı.");
    return false;
}
```

Bu parça mevcut yol kontrolü yönteminin içindendir; `agent` gerçek `NavMeshAgent` nesnesidir. Hedefe doğrudan görsel konum atayarak hareket ettirme değildir.

İlgili kaynaklar: `StageProgression.cs`, `LevelTransfer.cs`, `WeaponCrate.cs`, `WeaponPickup.cs`, `Assets/Scripts/Companion`.

Gün sonu kontrolü: sağ çıkış, kutu, tüfek, dar geçidin oyuncuyu engellemesi ve robotun panelden kapıyı açması. Dönüş testi tamamlanmadan başarı diye kaydedilmemeli.

## 3. gün — 30 Ekim 2026: yerel NPC ve sığınak finali

İşler: yerel Ollama bağlantısı, serbest metinli sohbet, JSON doğrulama, üçüncü bölüm, görevli izni, kapı ve tel çit görselleri, yazı/yeniden oynama düzeltmeleri.

Commit 1:

```text
feat: Ollama ile serbest metinli yerel NPC konuşmasını ekle
```

```text
Yerel modele asenkron istek, konuşma geçmişi ve serbest metin giriş alanı eklendi.
JSON işlemleri oyun durumuna göre doğrulanıyor; hata ve iptal durumları yönetiliyor.
```

Commit 2:

```text
feat: görevli izniyle açılan sığınak kapısını ve tahliye finalini ekle
```

```text
İkinci bölümün sağ çıkışı üçüncü bölüme bağlandı. Görevlinin giriş izni
doğrulandıktan sonra sığınak kapısı açılıyor; içeri geçince oyun tamamlanıyor.
Kapı, tel çit ve görevli boyutları düzenlendi.
```

Commit 3:

```text
fix: baştan oynama, okunaklı yazılar ve gereksiz panel sorunlarını düzelt
```

```text
Bitiş düğmesi ilk bölüme temiz başlangıçla dönüyor. HUD ve sohbet yazıları
gerçek piksel boyutunda çiziliyor. İkinci bölümdeki işlevsiz panel kaldırıldı.
```

Kısa kod — bulut yerine bilgisayarın kendi model sunucusuna istek:

```csharp
using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
using (var response = await http.PostAsync(
    "http://127.0.0.1:11434/api/chat", content, cancellation))
{
    return await response.Content.ReadAsStringAsync();
}
```

Bu kısa örnek hata kontrollerini göstermez; üretim `OllamaClient` sınıfı model bulunamadı, HTTP hatası, iptal ve zaman aşımını ayrıca ele alır.

İlgili kaynaklar: `Assets/Scripts/Dialogue`, `ShelterGate.cs`, `ShelterExit.cs`, `HudTheme.cs`, `PrototypeOverlay.cs`, `SurvivalSession.cs`, 13–15. sahneler.

Doğrulanmış sonuçlar: 22 işlem politikası + 6 taklit HTTP kontrolü; gerçek yerel modelle sığınağa giriş/bitiş testi; gerçek sahne yüklemeli baştan oynama testi geçti. 28 otomatik kontrol, 28 gerçek model diyaloğu değildir.

## 4. gün — 31 Ekim 2026: planlanan test ve teslim hazırlığı

Bu gün henüz gelmedi. Aşağıdaki commitler, işler gerçekten tamamlandıktan sonra kullanılmalı.

Yapılacaklar:

- Oyunu ilk bölümden sonuna kadar normal oynayarak dene; bölüm geçişlerini kaydet.
- Robotun dört farklı başlangıç konumundan panele gidip dönmesini tekrar ölç.
- En az beş farklı gerçek NPC diyaloğunu soru, cevap, JSON işlemi ve sonuçla kaydet.
- Ollama kapalı, model eksik, iptal, geçersiz yanıt ve tekrar görev/izin durumlarını dene.
- Küçük Game penceresinde ve tam ekranda HUD, sohbet, kaydırma ve metin girişini kontrol et.
- README'nin eski adım notlarıyla güncel durumu ayır; son ekran görüntülerini ve Mermaid diyagramlarını ekle.
- Windows oyun çıktısını al ve Unity editörü kapalıyken baştan sona dene.

Planlanan commit 1:

```text
test: bölüm akışını, robot görevini ve yerel NPC senaryolarını doğrula
```

```text
Tam oyun akışı, farklı robot başlangıçları ve gerçek NPC konuşmaları test edildi.
Ölçülen sonuçlar, başarısız senaryolar ve yeniden denemeler test raporuna işlendi.
```

Planlanan commit 2:

```text
docs: kurulum, test sonuçları ve teslim belgelerini tamamla
```

```text
Güncel kurulum, yerel model gereksinimleri, oyun kontrolleri ve sınıf/akış
diyagramları belgelendi. Windows çıktısının doğrulama sonucu rapora eklendi.
```

Kısa inceleme örneği — baştan oynarken eski durumu temizleyip ilk bölümü yükleme:

```csharp
LevelTransfer.Clear();
SceneManager.LoadScene(0);
```

Bu davranış mevcut projede 30 Ekim'de düzeltildi. 31 Ekim iş paketi, bunu yeniden eklemek değil, oyunu bitirdikten sonra düğme üzerinden uçtan uca kontrol etmektir.

## İlk GitHub yüklemesi

Mevcut kaynaklar son sürümü içerir; bu plan geçmiş günlerin ayrı, çalışır kaynak görüntülerini üretmez. Henüz Git deposu olmayan bu klasör için ilk commit örneği:

```text
feat: Çernobil Son Tahliye oyununun mevcut üç bölümlü sürümünü ekle
```

Sonraki gerçek değişiklikler yukarıdaki kapsamlarla ayrı ayrı commitlenebilir. Tarihleri ve yazarları değiştirmeden, her ekip üyesi kendi yaptığı işi kendi Git kimliğiyle kaydetmeli. Unity `.meta` dosyaları depoya dahil; `Library`, `Temp`, `Logs`, `UserSettings` ve Ollama model dosyaları hariç tutulmalı.
