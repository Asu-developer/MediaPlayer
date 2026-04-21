using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace MuzikCalar
{
    public partial class MainWindow : Window
    {
        private List<string> playlist = new List<string>();
        private int currentIndex = -1;
        private bool isPlaying = false;
        private bool wasPlayingBeforeDrag = false;

        DispatcherTimer timer = new DispatcherTimer();
        bool isDragging = false;

        public MainWindow()
        {
            InitializeComponent();
            mediaPlayer.UnloadedBehavior = MediaState.Close;
            mediaPlayer.LoadedBehavior = MediaState.Manual;
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;

            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void MediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (playlist.Count == 0) return;
            currentIndex = (currentIndex + 1) % playlist.Count;
            Cal();
        }

        private void MediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                sliderProgress.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                txtTotalTime.Text = mediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
            }
        }

        private void SliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mediaPlayer != null)
                mediaPlayer.Volume = e.NewValue;
        }

        // ──────────────────────────────────────────────
        // YENİ: Slider sürüklenirken anlık zamanı gerçek zamanlı göster
        // ──────────────────────────────────────────────
        private void SliderProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isDragging)
            {
                TimeSpan current = TimeSpan.FromSeconds(e.NewValue);
                txtCurrentTime.Text = current.ToString(@"mm\:ss");
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (mediaPlayer.Source == null) return;

            if (mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                sliderProgress.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                txtTotalTime.Text = mediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
            }

            if (!isDragging)
            {
                sliderProgress.Value = mediaPlayer.Position.TotalSeconds;
                txtCurrentTime.Text = mediaPlayer.Position.ToString(@"mm\:ss");
            }
        }

        private void SliderProgress_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (mediaPlayer.Source == null) return;

            wasPlayingBeforeDrag = isPlaying;
            if (isPlaying)
            {
                mediaPlayer.Pause();
            }
            isDragging = true;
        }

        private void SliderProgress_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (mediaPlayer.Source == null) return;

            mediaPlayer.Position = TimeSpan.FromSeconds(sliderProgress.Value);

            if (wasPlayingBeforeDrag)
            {
                mediaPlayer.Play();
                isPlaying = true;
                btnOynat.Content = "⏸ Durdur";
            }
            else
            {
                isPlaying = false;
                btnOynat.Content = "▶ Oynat";
            }

            isDragging = false;
        }

        private void BtnKlasorSec_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog { Title = "MP3 klasörü seç" };
            if (dialog.ShowDialog() == true)
            {
                txtKlasor.Text = dialog.FolderName;
                Yukle(dialog.FolderName);
            }
        }

        private void Yukle(string path)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) return;

            playlist = Directory.GetFiles(path, "*.mp3").OrderBy(f => f).ToList();

            lstSarkilar.Items.Clear();
            foreach (var file in playlist)
                lstSarkilar.Items.Add(Path.GetFileName(file));

            if (playlist.Count > 0)
                currentIndex = 0;
        }

        private void LstSarkilar_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstSarkilar.SelectedIndex == -1) return;
            currentIndex = lstSarkilar.SelectedIndex;
            Cal();
        }

        private void BtnOynat_Click(object sender, RoutedEventArgs e)
        {
            if (playlist.Count == 0 || currentIndex == -1) return;

            if (isPlaying)
            {
                mediaPlayer.Pause();
                isPlaying = false;
                btnOynat.Content = "▶ Oynat";
            }
            else
            {
                if (mediaPlayer.Source != null && mediaPlayer.Source.LocalPath == playlist[currentIndex])
                {
                    mediaPlayer.Play();
                    isPlaying = true;
                    btnOynat.Content = "⏸ Durdur";
                }
                else
                {
                    Cal();
                }
            }
        }

        private void BtnSonraki_Click(object sender, RoutedEventArgs e)
        {
            if (playlist.Count == 0) return;
            currentIndex = (currentIndex + 1) % playlist.Count;
            Cal();
        }

        private void BtnOnceki_Click(object sender, RoutedEventArgs e)
        {
            if (playlist.Count == 0) return;
            currentIndex = (currentIndex - 1 + playlist.Count) % playlist.Count;
            Cal();
        }

        private void Cal()
        {
            if (currentIndex < 0 || currentIndex >= playlist.Count) return;

            try
            {
                mediaPlayer.Source = new Uri(playlist[currentIndex]);
                mediaPlayer.Play();

                isPlaying = true;
                btnOynat.Content = "⏸ Durdur";

                lstSarkilar.SelectedIndex = currentIndex;
                lstSarkilar.ScrollIntoView(lstSarkilar.SelectedItem);
                this.Title = "Çalıyor: " + Path.GetFileName(playlist[currentIndex]);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            mediaPlayer.Stop();
            mediaPlayer.Close();
            base.OnClosing(e);
        }
    }
}