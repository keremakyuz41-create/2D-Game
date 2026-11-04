# Arayüz, baştan oynama ve ikinci panel düzeltmesi

- Bitiş ekranının **BAŞTAN OYNA** düğmesi artık `SurvivalSession.RestartGame` üzerinden ilk etkin Build Settings sahnesini yükler; bu projede `13_FinalBolum1` sahnesidir. Önceki sahneler arası aktarım temizlenir. Yeni oyunda tam can, tabanca ve dolu şarjör bulunur.
- Ölüm ekranı ayrı olarak **BÖLÜMÜ YENİDEN DENE** der ve mevcut bölümü tekrar yükler.
- HUD, konuşma geçmişi ve serbest metin girişinde fontlar gerçek ekran piksel boyutunda oluşturulur; metin çizilirken ölçek matrisi kaldırılır ve koordinatlar tam piksellere oturtulur. Böylece küçük font dokusunun büyütülmesi azaltılır. Yardımcı metin en az 11, ana metin en az 13, sohbet/giriş metni en az 14 ekran pikselidir. İkincil metin kontrastı artırıldı; görev ve robot kartları daha büyük yazıya yer açacak şekilde genişletildi.
- İkinci bölümdeki dekoratif **Metal servis dolabi** ve ona ait eski pompa engeli kaldırıldı. Kapıyı gerçekten açan `GateControlPanel` korunur; sahnede bir işlevsel panel vardır. Sonraki tel çit kurulumları boş dolabı tekrar eklemez.

Editor menüleri: **Cernobil → Adim 13 - Bos Paneli Kaldir** ve **Cernobil → Final - Bastan Oyna Testi**.

Kodlar derlendi. Baştan oynama regresyon testi gerçek Unity oynatma modunda **geçti**: son bölümden birinci bölüme dönüş, eski aktarımın temizlenmesi, tam can/tabanca/dolu şarjör/açık kontroller doğrulandı. Sonuç `Bastan-Oyna-Test-Sonuclari.json` dosyasındadır. Test doğrudan bitiş düğmesinin bağlandığı `RestartGame` işlemini çağırır; fareyle bitiş düğmesine tıklama senaryosu bu testte kullanılmaz.

Baştan oynama sonrası HUD büyütülmüş Game görünümünde incelendi: başlık, görev, can, silah ve şarjör yazıları görünür ve okunaklıdır. Konuşma geçmişi/giriş alanlarının yeni piksel çizimi derlendi; serbest metin yazıp gönderme akışı bu değişiklikten sonra tekrar denenmedi.

Boş panel temizliği gerçek editörde çalıştırıldı ve ikinci bölüm kaydedildi; işlevsiz dolap/pompa ve dekorları dahil beş nesne kaldırıldı. Sahnede tek `GateControlPanel` kaldığı doğrulandı. Eski tel çit dokümanındaki dekoratif servis dolabı bu sürümde artık bulunmaz.
