using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MuzikCalar
{
    public partial class MainWindow : Window
    {
        private List<string> playlist = new List<string>(); // tam dosya yolları
        private int currentIndex = -1;

        public MainWindow()
        {
            InitializeComponent();

            mediaPlayer.UnloadedBehavior = MediaState.Close;
            mediaPlayer.LoadedBehavior = MediaState.Manual;

            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
        }
        private void MediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (playlist.Count == 0) return;

            currentIndex = (currentIndex + 1) % playlist.Count;
            Cal();
        }

        // Klasör seç butonu
        private void BtnKlasorSec_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = "MP3 dosyalarının olduğu klasörü seçin"
            };

            if (dialog.ShowDialog() == true)
            {
                txtKlasor.Text = dialog.FolderName;
            }
        }

        // Listeyi yükle butonu
        private void BtnYukle_Click(object sender, RoutedEventArgs e)
        {
            string path = txtKlasor.Text.Trim();
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                MessageBox.Show("Geçerli bir klasör yolu girin!", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Sadece .mp3 dosyalarını al
            playlist = Directory.GetFiles(path, "*.mp3", SearchOption.TopDirectoryOnly).ToList();

            lstSarkilar.Items.Clear();

            if (playlist.Count == 0)
            {
                MessageBox.Show("Bu klasörde MP3 dosyası bulunamadı!", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            foreach (string file in playlist)
            {
                lstSarkilar.Items.Add(Path.GetFileName(file)); // sadece dosya adı göster
            }

            currentIndex = 0;
            MessageBox.Show($"{playlist.Count} adet MP3 yüklendi.", "Başarılı");
        }

        // Çift tıklayınca çal
        private void LstSarkilar_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstSarkilar.SelectedIndex == -1) return;
            currentIndex = lstSarkilar.SelectedIndex;
            Cal();
        }

        // Oynat butonu
        private void BtnOynat_Click(object sender, RoutedEventArgs e)
        {
            if (playlist.Count == 0 || currentIndex == -1)
                return;

            // Eğer zaten aynı şarkı yüklüyse sadece devam ettir
            if (mediaPlayer.Source != null &&
                mediaPlayer.Source.LocalPath == playlist[currentIndex])
            {
                mediaPlayer.Play(); // kaldığı yerden devam
            }
            else
            {
                Cal(); // yeni şarkı başlat
            }
        }

        // Durdur butonu
        private void BtnDurdur_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Pause();
        }

        // Sonraki
        private void BtnSonraki_Click(object sender, RoutedEventArgs e)
        {
            if (playlist.Count == 0) return;
            currentIndex = (currentIndex + 1) % playlist.Count;
            Cal();
        }

        // Önceki
        private void BtnOnceki_Click(object sender, RoutedEventArgs e)
        {
            if (playlist.Count == 0) return;
            currentIndex = (currentIndex - 1 + playlist.Count) % playlist.Count;
            Cal();
        }

        // Şarkıyı çalan ana fonksiyon
        private void Cal()
        {
            if (currentIndex < 0 || currentIndex >= playlist.Count) return;

            try
            {
                mediaPlayer.Source = new Uri(playlist[currentIndex]);
                mediaPlayer.Play();

                // Listede seçili olanı vurgula
                lstSarkilar.SelectedIndex = currentIndex;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Şarkı çalınırken hata oluştu: " + ex.Message);
            }
        }
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (mediaPlayer != null)
            {
                mediaPlayer.Stop();
                mediaPlayer.Close();           // UnloadedBehavior=Close ile aynı etki
            }
            base.OnClosing(e);
        }
    }
}