using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using Microsoft.Win32;
using AltoHttp;
using System.IO;
using System.IO.Compression;
using Ionic.Zip;
using System.Diagnostics;
using System.Threading;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Reflection;

namespace CDsharck_Launcher
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public class LauncherKeeper
        {
            public String game_puth;
        }

        LauncherKeeper keeper = new LauncherKeeper();
        private HttpDownloader _httpLoader = null;
        SaveFileDialog save_file = new SaveFileDialog()
        {
            FileName = "HeatCars",
            Filter = ".zip(*.zip) | .zip"
        };

        private void MainWind_Loaded(object sender, RoutedEventArgs e)
        {
            DownloadAttributes(false);
            CreateNews();

            MediaElement_video.Play();

            if (File.Exists("DATA.json"))
            {
                string json = System.IO.File.ReadAllText("DATA.json");
                keeper = JsonConvert.DeserializeObject<LauncherKeeper>(json);

                ReadyPlay(true);
            }
            else
                ReadyPlay(false);
        }

        private void ButtonClick_Download(object sender, RoutedEventArgs e)
        {
            if (save_file.FileName == "HeatCars")
            {
                MessageBox.Show("File not selected. Specify the path to download", "Attention!", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DownloadAttributes(true);

            const String url = "https://github.com/FacePunch1337/HeatCars_PreAlphaBuild/archive/refs/heads/main.zip";

            _httpLoader = new HttpDownloader(url, save_file.FileName);
            _httpLoader.DownloadCompleted += _httpLoader_DownloadCompleted;
            _httpLoader.ProgressChanged += _httpLoader_ProgressChanged;
            _httpLoader.Start();

            Button_Puth.IsEnabled = false;
            Label_Path.IsEnabled = false;
            Button_Download.IsEnabled = false;
            MainWind.Cursor = Cursors.Wait;
        }

        private void _httpLoader_DownloadCompleted(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(new Action(delegate
            {
                Unboxing();

                Label_DownloadedProc.Content = "100%";
                Button_Puth.IsEnabled = false;
                Button_Download.IsEnabled = false;
                MainWind.Cursor = Cursors.Arrow;

                MessageBox.Show("Download completed successfully", "Download message", MessageBoxButton.OK, MessageBoxImage.Asterisk);

                DownloadAttributes(false);

                using (StreamWriter write = File.CreateText("DATA.json"))
                {
                    string json_text = JsonConvert.SerializeObject(keeper);

                    write.Write(json_text);
                }

                ReadyPlay(true);
            }));
        }

        private void _httpLoader_ProgressChanged(object sender, AltoHttp.ProgressChangedEventArgs e)
        {
            this.ProgressBar_ProgressDownload.Value = (double)e.Progress;
            Label_DownloadedProc.Content = $"{Convert.ToInt32(e.Progress).ToString("0")}%";
            Label_Speed.Content = String.Format("{0} MB/s", (e.SpeedInBytes / 1024d / 1024d).ToString("0.00"));
            Label_MB.Content = String.Format("{0} MB", (_httpLoader.TotalBytesReceived / 1024d / 1024d).ToString("0.00"));
        }

        private void Unboxing()
        {
            keeper.game_puth = save_file.FileName;
            keeper.game_puth = keeper.game_puth.Replace(".zip", "");

            Directory.CreateDirectory(keeper.game_puth);

            using (var zip = Ionic.Zip.ZipFile.Read(save_file.FileName))
            {
                zip.ExtractAll(keeper.game_puth, ExtractExistingFileAction.OverwriteSilently);
            }

            File.Delete(save_file.FileName);
        }

        private void Button_Puth_Click(object sender, RoutedEventArgs e)
        {
            if (save_file.ShowDialog() == true)
                Label_Path.Content = save_file.FileName;
            else
                return;
        }

        private void Button_Play_Click(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo info = new ProcessStartInfo() { FileName = keeper.game_puth + "\\HeatCars_PreAlphaBuild-main\\HeatCars.exe" };

            if (AvailabilityFile(info.FileName))
            {
                Process.Start(info.FileName);

                MediaElement_video.Volume = 0;
            }
            else
            {
                MessageBox.Show("The file was lost or corrupted", "ERROR!", MessageBoxButton.OK, MessageBoxImage.Error);
                File.Delete("DATA.json");

                MainWind_Loaded(sender, e);
            }
        }

        private void Button_Remove_Click(object sender, RoutedEventArgs e)
        {
            if (AvailabilityFile(keeper.game_puth))
            {
                Directory.Delete(keeper.game_puth, true);
                File.Delete("DATA.json");
                save_file.FileName = "HeatCars";
                IsRemuve(true);

                MessageBox.Show("Deleted  ;(", ":(", MessageBoxButton.OK, MessageBoxImage.Information);

                IsRemuve(false);
                ReadyPlay(false);
            }
            else
            {
                MessageBox.Show("The file was lost or corrupted", "ERROR!", MessageBoxButton.OK, MessageBoxImage.Error);
                File.Delete("DATA.json");

                MainWind_Loaded(sender, e);
            }
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (MediaElement_video.Volume > 0)
            {
                MediaElement_video.Volume = 0;
                Image_Volume.Source = new BitmapImage(new Uri("Img/Volume/VolumeFalse.png", UriKind.Relative));
            }
            else
            {
                MediaElement_video.Volume = 50;
                Image_Volume.Source = new BitmapImage(new Uri("Img/Volume/VolumeTrue.png", UriKind.Relative));
            }
        }

        private void Rectangle_MouseEnter(object sender, MouseEventArgs e) => Image_Volume.Visibility = Visibility.Visible;
        private void Rectangle_MouseLeave(object sender, MouseEventArgs e) => Image_Volume.Visibility = Visibility.Hidden;
        private void Image_Volume_MouseMove(object sender, MouseEventArgs e) => Image_Volume.Visibility = Visibility.Visible;


        private void CreateNews()
        {
            for (int i = 0; i < 7; i++)
            {
                TextBlock text = new TextBlock()
                {
                    Text = $"NEW UPDATE 00.0{i+1} added bus \n Selecting a car in the main menu added launcher fixed machine material when choosing color\t\t\t",
                    TextWrapping = TextWrapping.Wrap
                };
                listRound.Items.Add(text);
            }
        }

        private void DownloadAttributes(bool isit)
        {
            if (isit)
            {
                MaskBorder.Visibility = Visibility.Visible;
                ProgressBar_ProgressDownload.Visibility = Visibility.Visible;
                Label_isDownload.Visibility = Visibility.Visible;
                Label_DownloadedProc.Visibility = Visibility.Visible;
                Label_MB.Visibility = Visibility.Visible;
                Label_MB_.Visibility = Visibility.Visible;
                Label_Speed_.Visibility = Visibility.Visible;
                Label_Speed.Visibility = Visibility.Visible;
            }
            else
            {
                MaskBorder.Visibility = Visibility.Hidden;
                ProgressBar_ProgressDownload.Visibility = Visibility.Hidden;
                Label_isDownload.Visibility = Visibility.Hidden;
                Label_DownloadedProc.Visibility = Visibility.Hidden;
                Label_MB.Visibility = Visibility.Hidden;
                Label_MB_.Visibility = Visibility.Hidden;
                Label_Speed_.Visibility = Visibility.Hidden;
                Label_Speed.Visibility = Visibility.Hidden;
            }
        }

        private void ReadyPlay(bool isit)
        {
            if (isit)
            {
                Button_Download.Visibility = Visibility.Hidden;
                Button_Download.IsEnabled = false;
                Button_Puth.Visibility = Visibility.Hidden;
                Button_Puth.IsEnabled = false;
                Label_Path.Visibility = Visibility.Hidden;
                Label_Path.IsEnabled = false;
                Button_Play.Visibility = Visibility.Visible;
                Button_Play.IsEnabled = true;
                Button_Remove.Visibility = Visibility.Visible;
                Button_Remove.IsEnabled = true;
            }
            else
            {
                Button_Download.Visibility = Visibility.Visible;
                Button_Download.IsEnabled = true;
                Button_Puth.Visibility = Visibility.Visible;
                Button_Puth.IsEnabled = true;
                Label_Path.Visibility = Visibility.Visible;
                Label_Path.IsEnabled = true;
                Label_Path.Content = "...";
                Button_Play.Visibility = Visibility.Hidden;
                Button_Play.IsEnabled = false;
                Button_Remove.Visibility = Visibility.Hidden;
                Button_Remove.IsEnabled = false;
            }
        }

        private void IsRemuve(bool isit)
        {
            if (isit)
            {
                var solorBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fc1937"));
                MaskBorder.Visibility = Visibility.Visible;
                MaskBorder.BorderBrush = solorBrush;
                ProgressBar_ProgressDownload.Visibility = Visibility.Visible;
                ProgressBar_ProgressDownload.BorderBrush = solorBrush;
                ProgressBar_ProgressDownload.Foreground = solorBrush;
                ProgressBar_ProgressDownload.Value = 100;
                ProgressBar_ProgressDownload_shadow.Color = Color.FromRgb(252, 25, 55);
                Label_isDownload.Visibility = Visibility.Visible;
                Label_isDownload.Foreground = solorBrush;
                Label_isDownload.Content = "Removal...";
                Label_DownloadedProc.Visibility = Visibility.Visible;
                Label_DownloadedProc.Foreground = solorBrush;
                Label_DownloadedProc.Content = "100%";
            }
            else
            {
                var solorBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFAFFC41"));
                MaskBorder.Visibility = Visibility.Hidden;
                MaskBorder.BorderBrush = solorBrush;
                ProgressBar_ProgressDownload.Visibility = Visibility.Hidden;
                ProgressBar_ProgressDownload.BorderBrush = solorBrush;
                ProgressBar_ProgressDownload.Foreground = solorBrush;
                ProgressBar_ProgressDownload.Value = 0;
                ProgressBar_ProgressDownload_shadow.Color = Color.FromRgb(175, 252, 65); ;
                Label_isDownload.Visibility = Visibility.Hidden;
                Label_isDownload.Foreground = solorBrush;
                Label_isDownload.Content = "Download...";
                Label_DownloadedProc.Visibility = Visibility.Hidden;
                Label_DownloadedProc.Foreground = solorBrush;
                Label_DownloadedProc.Content = "0%";
            }
        }

        private bool AvailabilityFile(string puth)
        {
            if (File.Exists(puth) || Directory.Exists(puth))
                return true;
            return false;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Environment.Exit(0);
            base.OnClosing(e);
        }
    }
}