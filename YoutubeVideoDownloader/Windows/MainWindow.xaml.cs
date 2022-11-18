using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
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
using Microsoft.WindowsAPICodePack.Dialogs;

namespace YoutubeVideoDownloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string path = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders", "{374DE290-123F-4565-9164-39C4925E467B}", String.Empty).ToString();

        private string SelectedURL = "";

        public MainWindow()
        {
            InitializeComponent();
            txtFolder.Text = path;
        }

        private ProcessStartInfo ProcInfo(string filename, string parameters)
        {
            ProcessStartInfo procInfo = new ProcessStartInfo();
            procInfo.FileName = filename;
            procInfo.Arguments = parameters;
            procInfo.UseShellExecute = false;
            procInfo.RedirectStandardOutput = true;
            procInfo.CreateNoWindow = true;

            return procInfo;
        }

        private void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            if (cmbFormat.SelectedItem == null) return;

            string selectedFormat = cmbFormat.SelectedItem.ToString();
            string code = selectedFormat.Split(' ').ToArray()[0].ToString();

            string filename = "youtube-dl.exe";
            string parameters = $"-o {path}\\%(title)s.%(ext)s -f {code} {SelectedURL}";

            ProcessStartInfo prcInfo = ProcInfo(filename, parameters);
            using (Process proc = Process.Start(prcInfo))
            {
                proc.WaitForExit();
                MessageBox.Show("Download completed!", "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void txtLink_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                SelectedURL = txtLink.Text;
                string filename = "youtube-dl.exe";
                string parameters = "-F " + SelectedURL;

                if (!CheckForInternet(txtLink.Text))
                {
                    MessageBox.Show("Check your internet connection!", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                ProcessStartInfo procInfo = ProcInfo(filename, parameters);

                using (Process proc = Process.Start(procInfo))
                {
                    cmbFormat.Items.Clear();

                    proc.WaitForExit();

                    byte s = 0;
                    while (!proc.StandardOutput.EndOfStream)
                    {
                        string line = proc.StandardOutput.ReadLine();

                        if (s++ > 2)
                            cmbFormat.Items.Add(line);
                    }
                }
            }
            catch
            {
                //MessageBox.Show("Please copy a youtube link!", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void btnFolderSearch_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = path;
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                path = dialog.FileName;
                txtFolder.Text = path;
            }
        }

        private bool CheckForInternet(string url)
        {
            if (!NetworkInterface.GetIsNetworkAvailable()) return false;

            Uri uri = new Uri(url);
            Ping ping = new Ping();
            PingReply pingRpl = ping.Send(uri.Host);

            if (pingRpl.Status == IPStatus.Success)
                return true;
            else return false;
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            string link = Clipboard.GetText();
            int a = link.IndexOf("youtu");
            
            if (a > 5)
                txtLink.Text = link;
        }
    }
}

