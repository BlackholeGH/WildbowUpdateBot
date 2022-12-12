using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WildbowUpdateBot
{
    /// <summary>
    /// Interaction logic for Notification.xaml
    /// </summary>
    public partial class Notification : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public String NotificationHeader
        {
            get
            {
                String URI = (String)System.Windows.Application.Current.Resources["CurrentSerialURI"];
                URI = URI.Replace('\\', '\0');
                URI = URI.Replace('/', '\0');
                URI = URI.Replace(':', '\0');
                URI = URI.Replace("https", "");
                URI = URI.Replace("http", "");
                URI = URI.Replace("www.", "");
                URI = URI.Remove(URI.IndexOf('.'));
                if (URI.ToLower().Contains("palewebserial")) { URI = "PALE"; }
                return "NEW " + URI.ToUpper() + " UPDATE";
            }
        }
        private String pChapterTitle = "";
        public String ChapterTitle
        {
            get
            {
                return pChapterTitle;
            }
            set
            {
                pChapterTitle = value;
                NotifyPropertyChanged("ChapterTitle");
            }
        }
        private String pChapterDescription = "";
        public String ChapterDescription
        {
            get
            {
                return pChapterDescription;
            }
            set
            {
                pChapterDescription = value;
                NotifyPropertyChanged("ChapterDescription");
            }
        }
        MediaPlayer PlayNotifSound = new MediaPlayer();
        public Notification(String chapterTitle, String chapterDescription, BitmapSource image)
        {
            App.TrueClose += Close;
            ChapterTitle = chapterTitle;
            ChapterDescription = chapterDescription;
            InitializeComponent();
            if (image != null)
            {
                ImgDynamic.Source = image;
            }
            try
            {
                PlayNotifSound.Open(new Uri(AppDomain.CurrentDomain.BaseDirectory + @"/palenotif.wav"));
                PlayNotifSound.Volume = ((uint)System.Windows.Application.Current.Resources["AlertVolume"]) / 100d;
                PlayNotifSound.Play();
            }
            catch { }
            var anim = new DoubleAnimation(0, (Duration)TimeSpan.FromSeconds(1));
            anim.BeginTime = TimeSpan.FromSeconds(15);
            anim.Completed += (s, _) => this.Close();
            this.BeginAnimation(UIElement.OpacityProperty, anim);
        }
        public void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start(new ProcessStartInfo(Monitor.SanitizeURI((String)System.Windows.Application.Current.Resources["CurrentSerialURI"])) { UseShellExecute = true });
            this.Close();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Right - this.Width;
            this.Top = desktopWorkingArea.Bottom - this.Height;
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            ImgDynamic.Source = null;
            ImgDynamic = null;
            BindingOperations.ClearBinding(NotificationHeaderTB, TextBlock.TextProperty);
            BindingOperations.ClearBinding(ChapterTitleTB, TextBlock.TextProperty);
            BindingOperations.ClearBinding(ChapterDescriptionTB, TextBlock.TextProperty);
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            App.TrueClose -= Close;
            System.GC.Collect();
        }
    }
}
