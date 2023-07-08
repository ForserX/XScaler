
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Threading.Tasks;
using System.Configuration;

namespace XScaler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Configuration ConfigFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        List<string> Files = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ListBox_DragEnter(object sender, DragEventArgs e)
        {
        }

        private void ClearClick(object sender, MouseButtonEventArgs e)
        {
            Files.Clear();
            FilesUI.Items.Clear();
        }

        private void AddClick(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog Dlg = new OpenFileDialog();
            Dlg.Multiselect = true;
            bool? result = Dlg.ShowDialog();
            
            if (result != null && result.Value)
            {
                foreach (string file in Dlg.FileNames)
                {
                    FilesUI.Items.Add(System.IO.Path.GetFileName(file));
                    Files.Add(file);
                }
            }
        }

        private void DelClick(object sender, MouseButtonEventArgs e)
        {
            if (FilesUI.Items.Count > 0)
            {
                if (FilesUI.SelectedIndex < 0)
                    FilesUI.SelectedIndex = 0;

                int OldSel = FilesUI.SelectedIndex;

                Files.Remove(Files[OldSel]);
                FilesUI.Items.Clear();

                foreach (string file in Files)
                {
                    FilesUI.Items.Add(System.IO.Path.GetFileName(file));
                }

                if (OldSel > 0)
                {
                    FilesUI.SelectedIndex = OldSel - 1;
                }
                else
                {
                    FilesUI.SelectedIndex = 0;
                }
            }
        }

        private void FilesUI_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
          // try
          // {
          //     if (FilesUI.SelectedIndex >= 0)
          //         ImgView.Source = new BitmapImage(new Uri(Files[FilesUI.SelectedIndex]));
          //     else
          //         ImgView.Source = null;
          // }
          // catch
          // {
          //     ImgView.Source = null;
          // } 
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog Dlg = new CommonOpenFileDialog();
            Dlg.Multiselect = false;
            Dlg.IsFolderPicker = true;

            var result = Dlg.ShowDialog();

            if (result == CommonFileDialogResult.Ok)
            {
                tbOutput.Text = Dlg.FileName;
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (slScale != null && lbScale != null)
                lbScale.Content = $"x{(int)slScale.Value}";

        }

        async Task RunCmd(int ProcessIt, string OutDir, string Path, string ModelCmd, int ScaleVal)
        {
            foreach (var file in Files)
            {
                string fname = System.IO.Path.GetFileNameWithoutExtension(file);
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.CreateNoWindow = true;
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = Path;
                startInfo.WorkingDirectory = System.IO.Directory.GetCurrentDirectory() + "\\upscaler\\";
                startInfo.Arguments = $"-i \"{file}\" -o \"{OutDir}\\{fname}_x{ScaleVal}.png\" -s {ScaleVal}";
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();

                this.Dispatcher.Invoke(() => pbRun.Value += ProcessIt);
            }
            this.Dispatcher.Invoke(() => pbRun.Value = 100);
        }
        
        async Task RunCmd3D(int ProcessIt, string OutDir, string Path, int ScaleVal)
        {
            foreach (var file in Files)
            {
                string fname = System.IO.Path.GetFileNameWithoutExtension(file);
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.CreateNoWindow = true;
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = Path;
                startInfo.Arguments = $"-i \"{file}\" -o \"{OutDir}\\{fname}_x{ScaleVal}.png\" -s {ScaleVal} -m \"{System.IO.Directory.GetCurrentDirectory() + "\\upscaler\\"}models-upconv_7_photo";
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();

                this.Dispatcher.Invoke(() => pbRun.Value += ProcessIt);
            }

            this.Dispatcher.Invoke(() => pbRun.Value = 100);
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (Files.Count == 0)
                return;

            pbRun.Value = 0;

            int ProcessIt = 100 / Files.Count;
            string OutDir = tbOutput.Text;
            int ScaleVal = (int)slScale.Value;
            
            if (cbModel.Text == "RealESERGAN (2D)")
            {
                string ModelCmd = "-n realesrgan-animevideov3";

                string Path = System.IO.Directory.GetCurrentDirectory() + "\\upscaler\\realesrgan-ncnn-vulkan.exe";
                Task.Run(() => RunCmd(ProcessIt, OutDir, Path, ModelCmd, ScaleVal));
            }
            else
            {
                string Path = System.IO.Directory.GetCurrentDirectory() + "\\upscaler\\waifu2x-ncnn-vulkan.exe";
                Task.Run(() => RunCmd3D(ProcessIt, OutDir, Path, ScaleVal));
            }
        }

        private void Grid_DragEnter(object sender, DragEventArgs e)
        {
            string[] dropedFile = ((string[])e.Data.GetData(DataFormats.FileDrop));

            foreach (string file in dropedFile)
            {
                FilesUI.Items.Add(System.IO.Path.GetFileName(file));
                Files.Add(file);
            }
        }

        private void OnClose(object sender, EventArgs e)
        {
            if (ConfigFile.AppSettings.Settings["outdir"] != null)
                ConfigFile.AppSettings.Settings["outdir"].Value = tbOutput.Text;
            else
                ConfigFile.AppSettings.Settings.Add("outdir", tbOutput.Text);

            ConfigFile.Save(ConfigurationSaveMode.Full, true);
            ConfigurationManager.RefreshSection("appSettings");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (ConfigFile.AppSettings.Settings["outdir"] != null)
                tbOutput.Text = ConfigFile.AppSettings.Settings["outdir"].Value;
        }
    }
}
