# Üçüncü bölüm: Son Tahliye

Adım 12: İkinci bölüm kapısının üstündeki/altındaki yeşil duvar şeritleri şeffaf paslı tel çitle değiştirildi. Robot bakım açıklığı metal çerçeveli hâle getirildi, yeşil pompa kutusunun yerini metal servis dolabı aldı. Fiziksel engeller ve dar geçit boyutu korunur. Kaynak ve prompt: `Art-Prompts-Step12.md`.

Adım 11 görsel düzenlemesi uygulandı: ikinci bölümün santral kapısı yeni paslı çelik panel, sabit ray/kasa ve kırmızı-sarı-yeşil durum lambası kullanır; panel 1,15 saniyede kasanın içine çekilir. Üçüncü bölüm görevlisinin görünür yüksekliği oyuncuyla aynı 2,49 birime getirildi. Görsel kaynaklar ve doğrulamanın kapsamı: `Art-Prompts-Step11.md`.

Yeni oyun sırası:

1. `13_FinalBolum1`: İlk sokağın sağ çıkışı ikinci bölüme götürür.
2. `14_FinalBolum2`: Beş zombi, kırılabilir kutu, otomatik tüfek ve gerçek NavMesh robot kapısı korunur. Robot kapıyı açınca en sağdaki yeşil çıkış çizgileri üçüncü bölümü yükler. Kapı kapalıyken geçiş kod tarafından da engellenir. Bu yeni kopyada teknisyen ikinci bölümden çıkarıldı.
3. `15_SonTahliye`: Ayrı sığınak avlusu, kilitli metal kapı, kapıyı koruyan görevli ve robot bulunur. Oyuncunun canı, silahı ve kalan şarjör mermisi sahneler arasında aktarılır. Üçüncü sahneyi doğrudan açınca varsayılan tabanca ve tam canla başlar.

Sahneler `Cernobil → Adim 10 - Ucuncu Bolum Siginak ve Serbest Konusma` ile oluşturuldu ve Build Settings'e eklendi. Eski 11/12 NPC sahneleri korunur. Kurulum menüsü yeniden çalıştırılınca yeni sahnelerin üzerine yazmaz.

## Görevliyle konuşma

Yaklaş ve **E**'ye bas. Alt taraftaki portreli pencereye istediğin cümleyi yaz; **Enter / GÖNDER** ile ilet. Hazır cevap seçenekleri veya sabit parola yoktur. Geçmişi kaydırarak okuyabilirsin. **ESC / X** kapatır; **İptal** bekleyen isteği keser. Ollama açık ve `qwen2.5:3b` yerel modeli indirilmiş olmalı.

Görevli ihtiyacını sorabilir; sığınma ihtiyacı, yaralanma veya yardım teklifi gibi makul bir sebep üzerine giriş izni verebilir. Örneğin: “Yaralandım, sığınacak bir yere ihtiyacım var. Size yardım edebilirim.” Bu bir örnektir, zorunlu cümle değildir.

Yanıtta `grant_access / shelter` işlemi önerilirse oyun yanıtın yapısını, izin verilen işlem/hedef çiftini, oyuncunun yakınlığını, görevli kimliğini, oyuncunun hayatta olmasını ve iznin daha önce verilmediğini kontrol eder. Yalnızca doğrulamadan geçen izin gerçek kapı animasyonunu başlatır. Modelin serbest konuşma metni tek başına kapıyı açmaz. Model cephane, can veya başka kapıları değiştiremez.

Ollama isteği bir JSON Schema gönderir: üç alan zorunludur ve işlem/hedef değerleri izin verilen seçeneklerle sınırlıdır. Rolü ve konuşma/işlem tutarlılığını anlatan örnekler model bağlamına eklenir; bunlar oyuncunun seçeceği hazır cevaplar değildir. İlk canlı denemede modelin metinde izin verip `none` işlemi döndürmesi yakalandı; yapılandırılmış yanıt ve örnek bağlam bu sorun üzerine eklendi. Oyun, metinden anahtar kelime arayarak izin üretmez.

İzin alınca konuşmayı kapat ve **üstteki kapıdan yukarı ilerle**. Kapı animasyonu bitmeden çarpışma kalkmaz. Kapının arkasındaki çıkış tetikleyicisi “SIĞINAĞA ULAŞTIN” bitiş ekranını açar. İzin alınmadan çıkış tetiklenemez.

## Teknik ayrım

- İkinci bölümün santral kapısını yalnız robot ve kontrol paneli açar. Sığınak görevlisi bu kapıyı kontrol etmez.
- Üçüncü bölüm robotu da gerçek NavMeshAgent ile takip eder; görünür XY hareketi mevcut XZ NavMesh uyarlayıcısından gelir.
- `NpcDialogue` teknisyen ve sığınak görevlisi bağlamlarını ayırır; `ShelterAccessPolicy` giriş izni için bağımsız izin listesidir.
- `ShelterGate` kilit/animasyon/bitiş durumlarını, `ShelterExit` gerçek fizik tetikleyicisini yönetir.
- Yeni görsellerin kaynak ve promptları `Art-Prompts-Step10.md` içindedir. Kapı için mevcut görselden kalıcı bir Unity Sprite kesiti kullanılır.

## Doğrulama

Çalışma zamanı ve Editor kodları derlendi. Üretim kaynaklarıyla **22 işlem politikası + 6 bellek içi HTTP kontrolü** geçti (`NPC-Standalone-Test-Sonuclari.json`). Bunlar 28 gerçek model diyaloğu değildir.

`Cernobil → Final - Canli Siginak Testi` menüsü gerçek yerel modele örnek bir serbest metin yollar; kilitli kapının çıkışı reddetmesini, yetkisiz aktörün reddini, gerçek model iznini, kapının açılmasını ve fiziksel çıkış tetikleyicisini dener. Sonuç `Siginak-Canli-Test-Sonuclari.json` dosyasına yazılır. Test oyuncuyu belirli konumlara yerleştirir; ikinci bölümden normal yürüyüşle geçişin uçtan uca oynanması ayrıca yapılmalıdır. Tek konuşma testi modelin bütün cümlelerde aynı kararı vereceğini göstermez.

Son canlı deneme **geçti**: gerçek yerel model “İçeri geç, burada güvendesin. Kapıyı açıyorum.” yanıtı ve `grant_access` işlemini üretti. Kapı animasyonu ve fiziksel çıkış tetikleyicisi tamamlandı; bitiş ekranı açıldı. Kilitli kapıda çıkış ve yetkisiz aktör de reddedildi. Toplam test süresi **9,64 saniye**; bu değer yalnız model üretim süresi değildir, testin hazırlık/kapı/çıkış beklemelerini de içerir.

Ödev için en az beş farklı gerçek NPC konuşması ve önceki dört robot başlangıç testinin dönüş dahil tamamlanması hâlâ ayrıca belgelenmeli.
