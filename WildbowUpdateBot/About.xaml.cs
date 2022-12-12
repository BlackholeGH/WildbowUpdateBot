using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;

namespace WildbowUpdateBot
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        public static About Current
        {
            get; set;
        }
        public About()
        {
            App.TrueClose += Close;
            if (App.AboutOpen)
            {
                this.Close();
                return;
            }
            App.AboutOpen = true;
            InitializeComponent();
            Current = this;
        }
        public void Window_Closed(object sender, EventArgs e)
        {
            App.AboutOpen = false;
            App.TrueClose -= Close;
        }
        public void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            if (!App.SettingsOpen) { App.SettingsOpen = true; }
            else
            {
                Settings.Current.Focus();
            }
        }
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        }
    }
}
