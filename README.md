🎵 MuzikCalar Pro v2.1
------------------------
MuzikCalar Pro, C# ve WPF teknolojilerinin sınırlarını zorlayan, yüksek performanslı ve modern bir masaüstü müzik deneyimidir. Bu sürümle birlikte uygulama, sadece müzik çalmakla kalmayıp, görsel bir şölen sunan ve kullanıcı etkileşimini en üst düzeye çıkaran akıllı özelliklerle donatılmıştır.

Hızlı indirme linki
------------------------
https://github.com/Asu-developer/MediaPlayer/releases/download/V2.1/MediaPlayer.zip

🌟 Öne Çıkan Yeni Özellikler
------------------------
🌊 Siri-Style Dinamik Görselleştirici (Visualizer)
Yüksek FPS Animasyon: CompositionTarget.Rendering motoru kullanılarak saniyede 60+ kare hızında akıcı bir dalga formu sunar.

Ritim Simülasyonu: Müzik çalarken rastgele genlik değişimleri ve kenar sönümleme (edge damping) ile gerçekçi bir "Siri" dalga efekti oluşturur.

Dingin Mod: Müzik durduğunda dalga formu otomatik olarak düz bir çizgiye dönüşür.

🔍 Akıllı Arama ve Filtreleme
Anlık Filtreleme: Yazmaya başladığınız anda listeniz daralır.

Relevans Sıralaması: LINQ tabanlı arama algoritması, aranan kelimenin şarkı ismindeki konumuna göre en alakalı sonuçları en üste taşır.

🖱️ Gelişmiş Etkileşim ve UI Kontrolleri
Dinamik Ölçeklendirme: Ctrl + Fare Tekerleği kombinasyonu ile şarkı listesinin boyutunu ve yazı tipini (FontSize) anlık olarak büyütebilir veya küçültebilirsiniz.

Akıllı Odak Yönetimi: Boş bir alana tıklandığında seçili şarkı otomatik olarak bırakılır, arayüz kalabalığı önlenir.

Şarkı Takibi: O an çalan şarkı, listede özel bir stil (Tag="Playing") ile vurgulanır.

🎨 Premium XAML Stilleri
Özel ScrollBar: Standart Windows görünümünden kurtulmuş, modern ve ince tasarımlı kaydırma çubukları.

Modern Shuffle Toggle: Aktif olduğunda Spotify yeşiline dönen, etkileşimli "Karıştır" butonu.

Karanlık Tema v2: #121212 tabanlı, göz yormayan profesyonel renk paleti.

🛠️ Teknik Detaylar
------------------------
Mimari: INotifyPropertyChanged ile MVVM prensiplerine uygun veri bağlama (Data Binding).

Medya Motoru: MediaElement üzerinden manuel kontrol (Play/Pause/Stop/Position).

Performans: Görselleştirici için PointCollection ve Polyline optimizasyonu.

Hata Yönetimi: Dosya okuma ve oynatma süreçlerinde try-catch blokları ile kararlı çalışma.

📦 Kurulum ve Kullanım
------------------------
Projeyi Klonlayın:

Bash
git clone https://github.com/kullaniciadi/MuzikCalarPro.git
Derleme: Visual Studio 2022+ ile projeyi açın ve .NET 6/8+ hedefinde derleyin.

Kullanım:

Klasör Seç: MP3 klasörünüzü seçin, liste otomatik dolacaktır.

Arama: Sağ üstteki büyüteç simgeli alandan şarkılarınızı anında bulun.

Yakınlaştırma: Liste üzerindeyken Ctrl tuşuna basılı tutarak fare tekerleğini çevirin.

🔧 Geliştirilmiş Kullanım Klavuzu
------------------------
Karıştırma Modu: Sol alt paneldeki 🔀 Karıştır butonu aktifse, şarkı bittiğinde veya "Sonraki" denildiğinde rastgele bir parçaya geçilir.

İlerleme Çubuğu: Slider üzerine tıkladığınızda müzik duraklar, sürüklemeyi bıraktığınızda kaldığı yerden (yeni pozisyonunda) devam eder.

Ses Ayarı: Sağdaki dikey bar üzerinden hassas ses kontrolü sağlayabilirsiniz.

📅 Roadmap (Gelecek Planları)
------------------------
[ ] ID3 Metadata: Şarkıların albüm kapaklarını ve sanatçı bilgilerini görüntüleme.

[ ] Playlist Kayıt: Oluşturulan filtreli listeleri .m3u olarak kaydetme.

[ ] Sistem Tepsisi (Tray): Uygulama kapandığında arka planda çalmaya devam etme.

📄 Lisans
------------------------
Bu proje MIT Lisansı kapsamında sunulmaktadır. Geliştirmelere katkıda bulunmak için fork atabilir veya issue açabilirsiniz.

Bu proje, modern C# tekniklerini ve görsel tasarım yeteneklerini birleştirmek amacıyla yapay zeka desteğiyle optimize edilerek geliştirilmiştir.
