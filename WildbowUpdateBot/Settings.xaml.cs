using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WildbowUpdateBot
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window, INotifyPropertyChanged
    {
        public static Settings Current
        {
            get; set;
        }
        private Boolean AllowClose = false;
        public bool ValidInterval
        {
            get { return pValidInterval; }
        }
        private bool pValidInterval = true;
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public Settings()
        {
            App.TrueClose += new Action(() =>
            {
                AllowClose = true;
                this.Close();
            });
            if(App.SettingsOpen)
            {
                this.Close();
                return;
            }
            InitializeComponent();
            Current = this;
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!AllowClose)
            {
                e.Cancel = true;
                App.SettingsOpen = false;
            }
        }
        public void OpenAbout_Click(object sender, RoutedEventArgs e)
        {
            if (!App.AboutOpen)
            {
                About about = new About();
                about.Show();
            }
            else
            {
                About.Current.Focus();
            }
        }
        public void TestNotif_Click(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() => Monitor.MonitorOperation(true));
        }
        public void Minimize_Click(object sender, RoutedEventArgs e)
        {
            App.SettingsOpen = false;
        }
        public void Quit_Click(object sender, RoutedEventArgs e)
        {
            App.TrueCloseFire();
        }

        private void timeInterval_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                UInt32 Extract = Convert.ToUInt32(((System.Windows.Controls.TextBox)sender).Text);
                if(Extract == 0) { throw new Exception(); }
                lock (System.Windows.Application.Current.Resources)
                {
                    System.Windows.Application.Current.Resources["CurrentTimeInterval"] = Extract;
                    Monitor.DoBreak();
                }
                pValidInterval = true;
                NotifyPropertyChanged("ValidInterval");
                FileIO.WritePersistentState((long)System.Windows.Application.Current.Resources["LastSeenPageID"], (String)System.Windows.Application.Current.Resources["CurrentSerialURI"], (uint)System.Windows.Application.Current.Resources["CurrentTimeInterval"], (bool)System.Windows.Application.Current.Resources["AutostartApp"], (uint)System.Windows.Application.Current.Resources["AlertVolume"]);
            }
            catch
            {
                pValidInterval = false;
                NotifyPropertyChanged("ValidInterval");
            }
        }
        private void TimeIntervalSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lock (System.Windows.Application.Current.Resources)
            {
                System.Windows.Application.Current.Resources["CurrentTimeInterval"] = Convert.ToUInt32(((System.Windows.Controls.Slider)sender).Value);
                Monitor.DoBreak();
            }
            pValidInterval = true;
            NotifyPropertyChanged("ValidInterval");
            FileIO.WritePersistentState((long)System.Windows.Application.Current.Resources["LastSeenPageID"], (String)System.Windows.Application.Current.Resources["CurrentSerialURI"], (uint)System.Windows.Application.Current.Resources["CurrentTimeInterval"], (bool)System.Windows.Application.Current.Resources["AutostartApp"], (uint)System.Windows.Application.Current.Resources["AlertVolume"]);
        }

        private void AlertVolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lock (System.Windows.Application.Current.Resources)
            {
                System.Windows.Application.Current.Resources["AlertVolume"] = Convert.ToUInt32(((System.Windows.Controls.Slider)sender).Value);
            }
            FileIO.WritePersistentState((long)System.Windows.Application.Current.Resources["LastSeenPageID"], (String)System.Windows.Application.Current.Resources["CurrentSerialURI"], (uint)System.Windows.Application.Current.Resources["CurrentTimeInterval"], (bool)System.Windows.Application.Current.Resources["AutostartApp"], (uint)System.Windows.Application.Current.Resources["AlertVolume"]);
        }

        private void serialURI_TextChanged(object sender, TextChangedEventArgs e)
        {
            lock (System.Windows.Application.Current.Resources)
            {
                System.Windows.Application.Current.Resources["CurrentSerialURI"] = ((System.Windows.Controls.TextBox)sender).Text;
            }
            FileIO.WritePersistentState((long)System.Windows.Application.Current.Resources["LastSeenPageID"], (String)System.Windows.Application.Current.Resources["CurrentSerialURI"], (uint)System.Windows.Application.Current.Resources["CurrentTimeInterval"], (bool)System.Windows.Application.Current.Resources["AutostartApp"], (uint)System.Windows.Application.Current.Resources["AlertVolume"]);
        }

        private void cbEnableAutorun_CheckedChanged(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            Boolean IsChecked = (bool)((System.Windows.Controls.CheckBox)sender).IsChecked;
            if (IsChecked)
            {
                key.SetValue("Blackhole's Wildbow Serial Update Monitor", System.Windows.Forms.Application.ExecutablePath);
            }
            else
            {
                key.DeleteValue("Blackhole's Wildbow Serial Update Monitor", false);
            }
            lock (System.Windows.Application.Current.Resources)
            {
                System.Windows.Application.Current.Resources["AutostartApp"] = IsChecked;
            }
            FileIO.WritePersistentState((long)System.Windows.Application.Current.Resources["LastSeenPageID"], (String)System.Windows.Application.Current.Resources["CurrentSerialURI"], (uint)System.Windows.Application.Current.Resources["CurrentTimeInterval"], (bool)System.Windows.Application.Current.Resources["AutostartApp"], (uint)System.Windows.Application.Current.Resources["AlertVolume"]);
        }
    }
}
