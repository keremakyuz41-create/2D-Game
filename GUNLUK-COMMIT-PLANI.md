# Çernobil — Son Tahliye: günlük çalışma ve commit mesajları

Bu liste proje dosyalarının tarihleri ve bu sohbetin geliştirme sırasına göre hazırlanmıştır. Dosya tarihleri Git commit geçmişi değildir. Projede bu liste hazırlanırken `.git` klasörü bulunmadığından aşağıdaki başlıklar **hazır mesaj taslaklarıdır**, oluşturulmuş veya GitHub'a gönderilmiş commitler değildir.

Doğrulanabilen çalışma günleri **28.10.2026** ve **30.10.2026**. 29 Ekim için doğrulanmış çalışma kaydı bulunmadığından o güne iş eklenmedi. Bugün oluşturulan commitler bugünün tarihini taşır; aşağıdaki günler işlerin çalışma gününü anlatır.

## 28 Ekim 2026 — Temel oyun ve yardımcı robot

### 1. Proje ve hareket

Başlık:

```text
feat: Unity 2D proje iskeletini ve oyuncu hareketini kur
```

Açıklama:

```text
WASD ve yön tuşlarıyla hareket, çapraz hız sınırı ve Rigidbody2D tabanlı
çarpışma davranışı eklendi. Kamera takibi ve ilk hareket deneme sahnesi kuruldu.
```

İlgili alanlar: `Assets/Scripts/Player`, `CameraFollow`, `FirstStepSetup`, `01_HareketDenemesi`, `Packages`, `ProjectSettings`, `.gitignore`.

### 2. Ateş ve hayatta kalma

Başlık:

```text
feat: silah, zombi takibi ve hayatta kalma mekaniklerini ekle
```

Açıklama:

```text
Fareyle nişan alma, atış, şarjör ve yeniden doldurma eklendi.
Zombilerin yavaş takibi, yakın saldırısı, oyuncu canı ve ölüm sonrası
bölümü yeniden deneme akışı oluşturuldu.
```

İlgili alanlar: `Assets/Scripts/Combat`, `Assets/Scripts/Enemies`, `SurvivalSession`, 2–4. adım kurulumları ve deneme sahneleri.

### 3. Görseller ve karakter sunumu

Başlık:

```text
feat: piksel sokak, koşu animasyonu ve elde silah görünümünü ekle
```

Açıklama:

```text
Çernobil temalı sokak ve karakter görselleri sahneye bağlandı.
Shift ile koşma, hareket animasyonu, elde silah ve namlu parlaması eklendi.
Animasyon kareleri kalıcı Sprite varlıklarına ayrılarak karakterin
aralıklı kaybolmasına yol açan referans sorunu giderildi.
```

İlgili alanlar: `Assets/Art/Street`, `PixelActorPresentation`, 5–6. adım kurulumları ve sahneleri, görsel üretim notları.

### 4. İkinci bölüm ve tüfek

Başlık:

```text
feat: ikinci bölüm, kırılabilir ikmal kutusu ve otomatik tüfek ekle
```

Açıklama:

```text
Sağ çıkıştan ikinci bölüme geçiş ve beş zombili bölüm kuruldu.
Kırılan kutudan çıkan tüfeğin E ile alınması ve seri ateş davranışı eklendi.
Sahne değişiminde can, silah türü ve şarjör mermisi aktarılıyor.
```

İlgili alanlar: `WeaponDefinition`, `WeaponKind`, `LevelTransfer`, `StageProgression`, `LevelExit`, `WeaponCrate`, `WeaponPickup`, `Assets/Data/Weapons`, 7. adım ve sahneleri.

### 5. Robot ve kapı bulmacası

Başlık:

```text
feat: gerçek NavMesh kullanan yardımcı robot ve santral kapısı ekle
```

Açıklama:

```text
Robotun oyuncuyu takip etmesi ve E emriyle dar servis yolundan kontrol
paneline gitmesi eklendi. Gerçek NavMeshAgent, engel sensörü ve dinamik
NavMesh güncellemesi kullanılıyor. Kapı yalnız robotun yetkili panel
etkileşimiyle açılıyor; oyuncu dar servis geçidinden geçemiyor.
```

İlgili alanlar: `Assets/Scripts/Companion`, `EighthStepSetup`, `CompanionNavMeshValidation`, `CompanionPlayValidation`, 9–10. sahneler, `Robot-Tasarimi.md`.

Doğrulama notu: 13 NavMesh/fizik kontrolü geçti. Eski canlı robot raporu başarısız bir denemeyi içerir; dönüş düzeltmesi sonrasında dört başlangıçlı canlı test yeniden tamamlanıp belgelenmeden bu test için başarı iddiası yazılmamalı.

## 30 Ekim 2026 — Yerel yapay zekâ, üçüncü bölüm ve düzeltmeler

### 6. Yerel NPC

Başlık:

```text
feat: Ollama ile yerel ve serbest metinli NPC konuşmasını ekle
```

Açıklama:

```text
Yerel qwen2.5:3b modeline asenkron bağlantı ve serbest metinli konuşma
penceresi eklendi. JSON işlem doğrulaması, konuşma geçmişi, iptal ve
zaman aşımı yönetimi kuruldu. Sunucu kapalı, model eksik ve geçersiz
yanıt durumları oyunu kilitlemeden kullanıcıya bildiriliyor.
```

İlgili alanlar: `Assets/Scripts/Dialogue`, `NinthStepSetup`, `DialogueValidation`, `Tests/DialogueChecks.cs`, 11–12. sahneler, yerel NPC kurulum notları, JSON modülü.

### 7. Üçüncü bölüm ve bitiş

Başlık:

```text
feat: sığınak bölümü ve görevli izniyle tamamlanan tahliye finali ekle
```

Açıklama:

```text
İkinci bölümde santral kapısı açıldıktan sonra sağ çıkış üçüncü bölüme
bağlandı. Sığınak görevlisiyle serbest konuşma ve doğrulanan giriş izniyle
açılan kapı eklendi. Kapıdan geçince tahliye bitiş ekranı gösteriliyor.
NPC yanıtları JSON Schema ile sınırlandırıldı ve konuşma örnekleriyle
metin/işlem tutarlılığı iyileştirildi.
```

İlgili alanlar: `ShelterGate`, `ShelterExit`, `ShelterAccessPolicy`, `ShelterRobotFollower`, `TenthStepSetup`, `ShelterPlayValidation`, 13–15. sahneler, `Assets/Art/Shelter`, `Siginak-Finali.md`.

Doğrulama: gerçek yerel modelin giriş izni, kapının açılması ve fiziksel çıkış tetikleyicisiyle bitiş akışı geçti. Tek testin toplam süresi yaklaşık 9,64 saniye; bu yalnız model üretim süresi değildir.

### 8. Kapı, çit ve karakter boyutu

Başlık:

```text
feat: santral kapısını ve tel çitleri yenile, görevli boyutunu ayarla
```

Açıklama:

```text
Santral kapısına paslı çelik panel, sabit kasa, sürgü rayı ve durum lambası
eklendi. Yeşil geçici duvar görselleri tel örgüyle değiştirildi, robot
servis açıklığı metal çerçeveyle gösterildi. Sığınak görevlisinin görünür
boyu oyuncuyla aynı 2,49 birime getirildi.
```

İlgili alanlar: `EleventhVisualPolish`, `TwelfthFencePolish`, santral kapı/çit varlıkları, 14–15. sahneler, `Art-Prompts-Step11.md`, `Art-Prompts-Step12.md`.

### 9. Arayüz, baştan oynama ve boş panel

Başlık:

```text
fix: baştan oynamayı ilk bölüme bağla, yazıları netleştir ve boş paneli kaldır
```

Açıklama:

```text
Bitiş ekranındaki BAŞTAN OYNA ilk bölümü temiz başlangıç durumuyla yüklüyor.
Ölüm ekranındaki mevcut bölümü yeniden deneme davranışı ayrı tutuldu.
HUD ve sohbet yazıları gerçek ekran piksel boyutunda çiziliyor; minimum
yazı boyutları, kontrast ve kart yerleşimleri iyileştirildi.
İkinci bölümdeki işlevsiz panel ve ona ait engel kaldırıldı.
```

İlgili alanlar: `HudTheme`, `PrototypeOverlay`, `NpcDialogue`, `SurvivalSession`, `ShelterGate`, `ThirteenthHudFix`, `RestartPlayValidation`, 14. sahne, arayüz notları.

Doğrulama: gerçek oynatma modunda üçüncü bölümden birinci bölüme dönüş, eski aktarımın temizlenmesi, tam can, tabanca, dolu şarjör ve açık kontroller doğrulandı. Kullanıcı bu düzeltmeleri oynayarak da onayladı.

### 10. Test kayıtları ve dokümantasyon

Başlık:

```text
docs: kurulum, geliştirme adımları ve doğrulanmış test sonuçlarını belgele
```

Açıklama:

```text
Yerel model kurulumu, bölüm sırası, robot tasarımı, görsel kaynaklar ve
oyun kontrolleri belgelendi. 22 işlem politikası ve 6 bellek içi HTTP
kontrolünün sonuçları, canlı sığınak testi ve baştan oynama raporu eklendi.
Henüz tamamlanmamış testler başarı olarak sunulmadan ayrıca belirtildi.
```

İlgili alanlar: `README.md`, proje kökündeki kurulum/tasarım/görsel notları, `Tests`, ilgili JSON test raporları. Bu 28 otomatik kontrol, 28 gerçek NPC diyaloğu anlamına gelmez.

## Mevcut projeyi ilk kez yüklerken

Bugünkü dosyalar son sürümdür; örneğin hareket ve silah dosyaları sonraki NPC/arayüz değişikliklerini de içerir. Bu nedenle yukarıdaki dosya gruplarını rastgele `git add` ile bölmek, geçmişteki çalışır sürümleri yeniden oluşturmaz. Başlıklar günlük çalışma kaydı ve değişiklik açıklaması olarak kullanılabilir; ayrı geçmiş sürümlerin yerine geçmez.

İlk yükleme için mevcut son sürümü tek, dürüst başlangıç commitiyle almak pratik olur:

```text
feat: Çernobil Son Tahliye oyununun üç bölümlü mevcut sürümünü ekle
```

```text
28 ve 30 Ekim'de geliştirilen hareket, çatışma, robot kapısı, yerel NPC
ve sığınak finalini ilk kez sürüm kontrolüne al. Günlük geliştirme özeti
GUNLUK-COMMIT-PLANI.md dosyasında, test kapsamları ilgili raporlardadır.
```

Depoya `Assets` ve içindeki `.meta` dosyaları, `Packages`, `ProjectSettings`, `.gitignore`, kaynak testler ve proje dokümanları eklenir. `Library`, `Temp`, `Logs`, `UserSettings`, `.vs`, derleme çıktıları ve indirilen Ollama model dosyaları eklenmez. Projedeki `.gitignore` geçici Unity klasörlerini zaten dışarıda bırakır.

Sonraki çalışmalarda her tamamlanan gerçek değişikliği o gün ayrı commit yap. İki kişilik projede her üye kendi yaptığı değişikliği kendi Git kimliğiyle kaydetmeli; kimin hangi işleri yaptığı bu sohbetten doğrulanamadığı için burada kişilere dağıtım yapılmadı. Öğretmenin istediği düzenli geçmiş, bundan sonraki gerçek commitlerle oluşur.
