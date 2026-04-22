using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using NAudio.Wave;

namespace MuzikCalar
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private List<string> playlist = new List<string>();
        private List<string> filteredPlaylist = new List<string>();
        private int currentIndex = -1;
        private bool isPlaying = false;
        private bool isDragging = false;
        private bool wasPlayingBeforeDrag = false;
        private double phase = 0;
        private double[] beatAmplitudes = new double[5] { 30, 30, 30, 30, 30 }; // Başlangıç değerleri
        private Random rnd = new Random();
        private DispatcherTimer uiTimer = new DispatcherTimer();
        
        // NAudio sadece ses analizi için - BASİT VE ETKİLİ
        private AudioFileReader? audioAnalyzer;
        private double currentVolume = 0;
        private double smoothedVolume = 30;
        private Queue<double> volumeHistory = new Queue<double>();
        private DispatcherTimer audioAnalysisTimer = new DispatcherTimer();
        private DateTime lastBeatTime = DateTime.Now;

        private double _itemHeight = 38;
        public double ItemHeight
        {
            get => _itemHeight;
            set
            {
                if (_itemHeight != value)
                {
                    _itemHeight = value;
                    OnPropertyChanged(nameof(ItemHeight));
                }
            }
        }

        private double _itemFontSize = 14;
        public double ItemFontSize
        {
            get => _itemFontSize;
            set
            {
                if (_itemFontSize != value)
                {
                    _itemFontSize = value;
                    OnPropertyChanged(nameof(ItemFontSize));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            
            // Pencereye tıklanınca listbox seçimini kaldır
            this.PreviewMouseDown += (s, e) =>
            {
                var element = e.OriginalSource as DependencyObject;
                
                // ListBox veya içindeki bir element mi kontrol et
                while (element != null)
                {
                    if (element == lstSarkilar)
                        return; // ListBox içindeyse hiçbir şey yapma
                    element = VisualTreeHelper.GetParent(element);
                }
                
                // ListBox dışına tıklandı, seçimi kaldır
                lstSarkilar.UnselectAll();
            };
            
            mediaPlayer.MediaEnded += (s, e) => {
                if (chkShuffle.IsChecked == true) currentIndex = rnd.Next(0, filteredPlaylist.Count);
                else currentIndex = (currentIndex + 1) % filteredPlaylist.Count;
                Cal();
            };

            uiTimer.Interval = TimeSpan.FromMilliseconds(200);
            uiTimer.Tick += (s, e) => {
                if (!isDragging && mediaPlayer.NaturalDuration.HasTimeSpan)
                {
                    sliderProgress.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                    sliderProgress.Value = mediaPlayer.Position.TotalSeconds;
                    txtCurrentTime.Text = mediaPlayer.Position.ToString(@"mm\:ss");
                    txtTotalTime.Text = mediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
                }
            };
            uiTimer.Start();

            // Ses analizi timer - Her bass vuruşunu yakala
            audioAnalysisTimer.Interval = TimeSpan.FromMilliseconds(20); // Çok hızlı
            audioAnalysisTimer.Tick += (s, e) => AnalyzeAudio();
            audioAnalysisTimer.Start();

            // Yüksek FPS Animasyon Döngüsü
            CompositionTarget.Rendering += (s, e) => {
                DrawWave();
            };
        }

        private void AnalyzeAudio()
        {
            if (!isPlaying || audioAnalyzer == null)
            {
                // Müzik çalmıyorsa yavaşça azalt
                for (int i = 0; i < beatAmplitudes.Length; i++)
                {
                    beatAmplitudes[i] = Math.Max(20, beatAmplitudes[i] * 0.92);
                }
                return;
            }

            try
            {
                // MediaElement pozisyonunu al
                var currentPos = mediaPlayer.Position;
                
                // AudioAnalyzer'ı aynı pozisyona getir (senkronizasyon)
                if (Math.Abs((audioAnalyzer.CurrentTime - currentPos).TotalSeconds) > 0.25)
                {
                    audioAnalyzer.CurrentTime = currentPos;
                }

                // BASS ALGILAMA İÇİN OPTİMAL BUFFER
                float[] samples = new float[2048];
                int samplesRead = audioAnalyzer.Read(samples, 0, samples.Length);

                if (samplesRead > 0)
                {
                    // BASS ENERJİSİ HESAPLAMA - Düşük frekanslara odaklan
                    double bassEnergy = 0;
                    double midEnergy = 0;
                    
                    // İlk 1/3'ü bass (düşük frekans)
                    int bassRange = samplesRead / 3;
                    
                    for (int i = 0; i < samplesRead; i++)
                    {
                        double sampleValue = samples[i] * samples[i]; // Karesi - daha hassas
                        
                        if (i < bassRange)
                        {
                            bassEnergy += sampleValue * 3; // Bass'a çok daha fazla ağırlık
                        }
                        else
                        {
                            midEnergy += sampleValue;
                        }
                    }
                    
                    // Normalize - BASS'a odaklan
                    bassEnergy = Math.Sqrt(bassEnergy / bassRange) * 1200; // Yüksek hassasiyet
                    
                    currentVolume = bassEnergy;
                    
                    // ÇOK HAFİF SMOOTHING - Her vuruşu yakala
                    smoothedVolume = smoothedVolume * 0.35 + currentVolume * 0.65;

                    // Kısa geçmiş - daha reaktif
                    volumeHistory.Enqueue(smoothedVolume);
                    if (volumeHistory.Count > 5)
                        volumeHistory.Dequeue();

                    double avgVolume = volumeHistory.Count > 0 ? volumeHistory.Average() : 30;

                    // BASS BEAT DETECTION - DAHA HASSAS
                    double timeSinceLastBeat = (DateTime.Now - lastBeatTime).TotalMilliseconds;
                    
                    // Daha düşük threshold - her ritmi yakala
                    bool bassBeat = smoothedVolume > avgVolume * 1.25 && 
                                   smoothedVolume > 8 && 
                                   timeSinceLastBeat > 75; // Daha kısa cooldown
                    
                    if (bassBeat)
                    {
                        // BASS BEAT TESPİT EDİLDİ!
                        lastBeatTime = DateTime.Now;
                        
                        // CANVAS YÜKSEKLİĞİNE GÖRE MAKSİMUM DALGA
                        double canvasHeight = canvasSiri.ActualHeight > 0 ? canvasSiri.ActualHeight : 100;
                        double maxAmplitude = canvasHeight * 0.22; // Biraz daha küçük maksimum
                        
                        // Bass seviyesine göre orantılı dalga
                        double rawStrength = smoothedVolume * 2.8; // Daha küçük çarpan
                        double beatStrength = Math.Min(rawStrength, maxAmplitude);
                        
                        // SMOOTH GEÇİŞ - Mevcut değerden yavaşça artır
                        for (int i = 0; i < beatAmplitudes.Length; i++)
                        {
                            double targetValue = beatStrength * (0.85 + rnd.NextDouble() * 0.3);
                            // Ani sıçrama yerine smooth geçiş
                            beatAmplitudes[i] = beatAmplitudes[i] * 0.3 + targetValue * 0.7;
                        }
                    }
                    else
                    {
                        // Beat yok - DAHA YAVAŞ AZALMA (düzleşmeyi önle)
                        for (int i = 0; i < beatAmplitudes.Length; i++)
                        {
                            double minLevel = Math.Max(20, smoothedVolume * 0.5); // Daha yüksek minimum
                            beatAmplitudes[i] = Math.Max(minLevel, beatAmplitudes[i] * 0.90); // Çok yavaş azalma
                        }
                    }
                }
            }
            catch
            {
                // Hata olursa minimum seviyeyi koru
                for (int i = 0; i < beatAmplitudes.Length; i++)
                {
                    beatAmplitudes[i] = Math.Max(20, beatAmplitudes[i] * 0.90);
                }
            }
        }

        private void DrawWave()
        {
            double width = canvasSiri.ActualWidth;
            double height = canvasSiri.ActualHeight;
            if (width <= 0 || height <= 0) return;

            PointCollection points = new PointCollection();
            double midY = height / 2;

            if (isPlaying)
            {
                // Sabit hız
                phase += 0.12;

                for (int x = 0; x <= width; x += 2)
                {
                    double xNormalized = (double)x / width;
                    double edgeDamping = Math.Sin(Math.PI * xNormalized);
                    
                    // PULSE SİSTEMİ - Sağdan sola giderken küçülür
                    double pulseEffect = xNormalized;
                    
                    // 7 DALGA - Daha fazla dalga görünür
                    double wave = 0;
                    wave += Math.Sin(x * 0.030 + phase) * beatAmplitudes[0] * 0.17 * pulseEffect;
                    wave += Math.Sin(x * 0.045 + phase * 1.2) * beatAmplitudes[1] * 0.14 * pulseEffect;
                    wave += Math.Sin(x * 0.060 + phase * 1.5) * beatAmplitudes[2] * 0.12 * pulseEffect;
                    wave += Math.Sin(x * 0.075 + phase * 1.8) * beatAmplitudes[3] * 0.10 * pulseEffect;
                    wave += Math.Sin(x * 0.090 + phase * 2.1) * beatAmplitudes[4] * 0.08 * pulseEffect;
                    wave += Math.Sin(x * 0.105 + phase * 2.4) * beatAmplitudes[0] * 0.06 * pulseEffect;
                    wave += Math.Sin(x * 0.120 + phase * 2.7) * beatAmplitudes[1] * 0.04 * pulseEffect;
                    
                    double y = midY + wave * edgeDamping;
                    points.Add(new Point(x, y));
                }
            }
            else
            {
                // Müzik durunca düz çizgi
                points.Add(new Point(0, midY));
                points.Add(new Point(width, midY));
            }
            
            waveLine.Points = points;
        }

        private void Cal()
        {
            if (currentIndex < 0 || currentIndex >= filteredPlaylist.Count) return;

            try
            {
                // 1. Önce kaynağı durdur ve temizle
                mediaPlayer.Stop();
                audioAnalyzer?.Dispose();

                string dosyaYolu = filteredPlaylist[currentIndex];

                // 2. MediaElement için kaynağı ata
                mediaPlayer.Source = new Uri(dosyaYolu, UriKind.Absolute);
                mediaPlayer.Play();

                // 3. NAudio analyzer aç (sadece analiz için)
                audioAnalyzer = new AudioFileReader(dosyaYolu);

                // 4. UI Güncellemeleri
                isPlaying = true;
                btnOynat.Content = "⏸";
                this.Title = "Çalıyor: " + System.IO.Path.GetFileName(dosyaYolu);

                // Çalan şarkıyı beyaz yap
                UpdatePlayingItemStyle();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Oynatma başlatılamadı: " + ex.Message);
            }
        }

        private void UpdatePlayingItemStyle()
        {
            // Tüm itemlerin Tag'ini temizle
            for (int i = 0; i < lstSarkilar.Items.Count; i++)
            {
                var container = lstSarkilar.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
                if (container != null)
                {
                    container.Tag = null;
                }
            }

            // Çalan şarkının Tag'ini "Playing" yap
            if (currentIndex >= 0 && currentIndex < lstSarkilar.Items.Count)
            {
                var playingContainer = lstSarkilar.ItemContainerGenerator.ContainerFromIndex(currentIndex) as ListBoxItem;
                if (playingContainer != null)
                {
                    playingContainer.Tag = "Playing";
                }
            }
        }

        // Buton ve Slider Eventleri
        private void BtnSonraki_Click(object sender, RoutedEventArgs e) { if (filteredPlaylist.Count > 0) { currentIndex = (chkShuffle.IsChecked == true) ? rnd.Next(0, filteredPlaylist.Count) : (currentIndex + 1) % filteredPlaylist.Count; Cal(); } }
        private void BtnOnceki_Click(object sender, RoutedEventArgs e) { if (filteredPlaylist.Count > 0) { currentIndex = (currentIndex - 1 + filteredPlaylist.Count) % filteredPlaylist.Count; Cal(); } }
        private void BtnKlasorSec_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog();
            if (dialog.ShowDialog() == true)
            {
                // Dosya yolunu textbox'a yaz
                txtKlasor.Text = dialog.FolderName;
                
                playlist = Directory.GetFiles(dialog.FolderName, "*.mp3").ToList();
                filteredPlaylist = new List<string>(playlist);
                UpdateListBox();
                
                if (filteredPlaylist.Count > 0) 
                {
                    currentIndex = 0;
                    // ItemContainerGenerator hazır olduğunda style'ı güncelle
                    lstSarkilar.ItemContainerGenerator.StatusChanged += (s, args) =>
                    {
                        if (lstSarkilar.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                        {
                            UpdatePlayingItemStyle();
                        }
                    };
                }
            }
        }

        private void UpdateListBox()
        {
            lstSarkilar.Items.Clear();
            foreach (var f in filteredPlaylist) 
                lstSarkilar.Items.Add(Path.GetFileName(f));
        }

        private void TxtArama_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = txtArama.Text.ToLower();
            
            if (string.IsNullOrWhiteSpace(searchText))
            {
                filteredPlaylist = new List<string>(playlist);
            }
            else
            {
                filteredPlaylist = playlist
                    .Where(f => Path.GetFileName(f).ToLower().Contains(searchText))
                    .OrderBy(f => Path.GetFileName(f).ToLower().IndexOf(searchText))
                    .ToList();
            }
            
            UpdateListBox();
            
            // Eğer çalan şarkı filtrede yoksa, index'i sıfırla
            if (currentIndex >= 0 && currentIndex < playlist.Count)
            {
                string currentSong = playlist[currentIndex];
                int newIndex = filteredPlaylist.IndexOf(currentSong);
                if (newIndex >= 0)
                {
                    currentIndex = newIndex;
                }
                else
                {
                    currentIndex = filteredPlaylist.Count > 0 ? 0 : -1;
                }
            }
            
            UpdatePlayingItemStyle();
        }

        private void LstSarkilar_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                e.Handled = true;
                
                if (e.Delta > 0)
                {
                    ItemHeight = Math.Min(ItemHeight + 2, 100);
                    ItemFontSize = Math.Min(ItemFontSize + 0.5, 24);
                }
                else
                {
                    ItemHeight = Math.Max(ItemHeight - 2, 20);
                    ItemFontSize = Math.Max(ItemFontSize - 0.5, 10);
                }
            }
        }
        
        private void BtnOynat_Click(object sender, RoutedEventArgs e)
        {
            if (filteredPlaylist.Count == 0) return;
            
            if (isPlaying) 
            { 
                mediaPlayer.Pause(); 
                isPlaying = false; 
                btnOynat.Content = "▶"; 
            }
            else 
            { 
                // Eğer hiç şarkı çalmamışsa ilk şarkıyı başlat
                if (mediaPlayer.Source == null && currentIndex >= 0)
                {
                    Cal();
                }
                else
                {
                    mediaPlayer.Play(); 
                    isPlaying = true; 
                    btnOynat.Content = "⏸";
                }
            }
        }
        private void LstSarkilar_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) 
        { 
            if (lstSarkilar.SelectedIndex >= 0)
            {
                currentIndex = lstSarkilar.SelectedIndex; 
                Cal();
            }
        }

        private void LstSarkilar_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Tıklanan elementi bul
            var element = e.OriginalSource as DependencyObject;
            
            // ListBoxItem'a kadar çık
            while (element != null && !(element is ListBoxItem))
            {
                element = VisualTreeHelper.GetParent(element);
            }

            // Eğer ListBoxItem bulunamadıysa (boş alana tıklandı), seçimi kaldır
            if (element == null)
            {
                lstSarkilar.UnselectAll();
                e.Handled = true;
            }
        }

        private void LstSarkilar_LostFocus(object sender, RoutedEventArgs e)
        {
            // Focus kaybedildiğinde seçimi kaldır
            lstSarkilar.UnselectAll();
        }
        private void SliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) { if (mediaPlayer != null) mediaPlayer.Volume = sliderVolume.Value; }
        private void SliderProgress_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) 
        { 
            isDragging = true;
            wasPlayingBeforeDrag = isPlaying;
            if (isPlaying)
            {
                mediaPlayer.Pause();
                isPlaying = false;
            }
        }
        
        private void SliderProgress_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e) 
        { 
            isDragging = false; 
            mediaPlayer.Position = TimeSpan.FromSeconds(sliderProgress.Value);
            if (wasPlayingBeforeDrag)
            {
                mediaPlayer.Play();
                isPlaying = true;
            }
        }
        
        private void SliderProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) 
        { 
            if (isDragging) 
                txtCurrentTime.Text = TimeSpan.FromSeconds(sliderProgress.Value).ToString(@"mm\:ss"); 
        }
    }
}
