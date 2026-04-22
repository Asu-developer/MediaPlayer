🎵 MuzikCalar Pro v2.2
----------------------
MuzikCalar Pro, C# ve WPF teknolojilerinin sınırlarını zorlayan, yüksek performanslı ve modern bir masaüstü müzik deneyimidir. Bu sürümle birlikte uygulama, statik bir oynatıcıdan ziyade, müziğin ruhunu dijital bir spektruma dönüştüren akıllı bir ekosisteme evrildi.

👇 Hızlı İndirme
----------------------
[MuzikCalar Pro v2.2 İndir (Not: Linki güncel sürümünüze göre güncellemeyi unutmayın)](https://github.com/Asu-developer/MediaPlayer/releases/download/V2.2/Setup.exe)

🌟 Öne Çıkan Yeni Özellikler
----------------------
🌊 Ritim Duyarlı Dinamik Görselleştirici (v2.2 Güncellemesi)
Gerçek Zamanlı Spektrum Analizi: Artık dalga formu rastgele değil, çalınan müziğin frekans verilerine (FFT) göre anlık olarak şekillenir. Baslar vurduğunda yükselen, melodide süzülen gerçek bir görsel şölen sunar.

Akıcı Render Motoru: CompositionTarget.Rendering optimizasyonu sayesinde, yüksek örnekleme hızlarında bile CPU dostu ve 60+ FPS akıcılığında animasyon sağlar.

Siri-Style Estetiği: Kenar sönümleme (edge damping) ve yumuşak geçiş efektleriyle modern bir görünüm.

🔊 Geliştirilmiş Ses Kontrol Merkezi
Hassas Kaydırıcı (Slider): Ses çubuğu, kullanıcı etkileşimine daha duyarlı hale getirildi. Logaritmik ses geçişleri ile kulak tırmalamayan, pürüzsüz bir ayarlama deneyimi sunar.

Görsel Geri Bildirim: Ses seviyesi değiştikçe dinamik olarak güncellenen ikonlar ve vurgulu renk paleti.

🔍 Akıllı Arama ve Liste Yönetimi
LINQ Tabanlı Filtreleme: Yazmaya başladığınız anda milisaniyeler içinde sonuç veren arama algoritması.

Dinamik Ölçeklendirme: Ctrl + Mouse Wheel kombinasyonu ile tüm listenin font boyutunu anlık olarak değiştirebilme.

🛠️ Teknik Detaylar
----------------------
Mimari: MVVM prensiplerine sadık kalınarak INotifyPropertyChanged ile tam veri bağlama (Data Binding).

Medya Altyapısı: MediaElement üzerine inşa edilmiş, yüksek kararlılığa sahip özel kontrol katmanı.

UI/UX: Tamamen özelleştirilmiş XAML stilleri, modern ince ScrollBar tasarımı ve "Spotify Yeşili" ile zenginleştirilmiş karanlık tema (#121212).

Performans: Görselleştirici için PointCollection ve Polyline optimizasyonları ile düşük kaynak tüketimi.

📦 Kurulum ve Geliştirme
----------------------
Projeyi Klonlayın:

Bash
git clone https://github.com/Asu-developer/MediaPlayer.git
Derleme: Visual Studio 2022+ ile açın ve .NET 6/8+ hedefinde çalıştırın.

Kullanım İpucu:
----------------------

Klasör Seç: MP3 klasörünüzü hedef gösterin, uygulama kütüphanenizi anında indeksler.

Odak Yönetimi: Boş alana tıklayarak seçimi temizleyebilir, arayüzü sadeleştirebilirsiniz.

📅 Yol Haritası (Roadmap)
----------------------
[ ] ID3 Metadata: Şarkıların albüm kapaklarını ve sanatçı bilgilerini otomatik çekme.

[ ] Playlist Kayıt: Filtrelenmiş listeleri .m3u formatında dışa aktarma.

[ ] Tray Mode: Uygulamayı sistem tepsisine küçülterek arka planda çalma desteği.

📄 Lisans
----------------------
Bu proje MIT Lisansı kapsamında sunulmaktadır. Geliştirmelere katkıda bulunmak için fork atabilir veya bir issue oluşturabilirsiniz.

Bu projede yapay zeka araçları kullanılmıştır
