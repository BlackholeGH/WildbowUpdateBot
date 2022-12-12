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
using System.Windows.Shapes;

namespace WildbowUpdateBot
{
    /// <summary>
    /// Interaction logic for NotFoundError.xaml
    /// </summary>
    public partial class NotFoundError : Window
    {
        public NotFoundError()
        {
            OnOpen("");
        }
        public NotFoundError(String err)
        {
            OnOpen(err);
        }
        private void OnOpen(String err)
        {
            App.TrueClose += new Action(() => { this.Close(); });
            if (App.ErrorOpen)
            {
                this.Close();
                return;
            }
            App.ErrorOpen = true;
            InitializeComponent();
            Resources["ErrorDetails"] = err;
        }
        public void Window_Closed(object sender, EventArgs e)
        {
            App.ErrorOpen = false;
        }
        public void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            if (!App.SettingsOpen) { App.SettingsOpen = true; }
            this.Close();
        }
        public void CloseWarning_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
