using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing;
using System.Threading;

namespace WildbowUpdateBot
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Boolean pSettingsOpen = false;
        public static Boolean SettingsOpen
        {
            get
            {
                return pSettingsOpen;
            }
            set
            {
                Boolean NewVal = value;
                if(pSettingsOpen && !NewVal)
                {
                    settings.Hide();
                    settings.ShowInTaskbar = false;
                }
                else if(!pSettingsOpen && NewVal)
                {
                    settings.Show();
                    settings.ShowInTaskbar = true;
                }
                pSettingsOpen = NewVal;
            }
        }
        private static Settings settings = null;
        public static Boolean AboutOpen
        {
            get; set;
        }
        public static Boolean ErrorOpen
        {
            get; set;
        }
        public static void TrueCloseFire()
        {   
            TrueClose();
        }
        public static event Action TrueClose;
        private static Boolean First = false;
        private void Application_Startup(object sender, StartupEventArgs e)
		{
            FileIO.InitializeAppFolders();
            Object State = FileIO.PullPersistentState();
            if (State == null)
            {
                First = true;
            }
            else
            {
                lock (Current.Resources)
                {
                    if (((object[])State).Length > 0 && ((object[])State)[0] != null) { Current.Resources["LastSeenPageID"] = (long)((object[])State)[0]; }
                    if (((object[])State).Length > 1 && ((object[])State)[1] != null) { Current.Resources["CurrentSerialURI"] = (string)((object[])State)[1]; }
                    if (((object[])State).Length > 2 && ((object[])State)[2] != null) { Current.Resources["CurrentTimeInterval"] = (uint)((object[])State)[2]; }
                    if (((object[])State).Length > 3 && ((object[])State)[3] != null) { Current.Resources["AutostartApp"] = (bool)((object[])State)[3]; }
                    if (((object[])State).Length > 4 && ((object[])State)[4] != null) { Current.Resources["AlertVolume"] = (uint)((object[])State)[4]; }
                }
            }
            AboutOpen = false;
            ErrorOpen = false;
            settings = new Settings();
            if (First)
            {
                First_Startup();
                Monitor.DoMonitor(true);
                SettingsOpen = true;
            }
            else
            {
                Monitor.DoMonitor(false);
            }
		}
		private void First_Startup()
		{
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            key.SetValue("Blackhole's Wildbow Serial Update Monitor", System.Windows.Forms.Application.ExecutablePath);
            Task SetPersistence = Task.Factory.StartNew(() =>
            {
                try
                {
                    String TrueUri = Monitor.SanitizeURI((String)System.Windows.Application.Current.Resources["CurrentSerialURI"]);
                    String ThisRead = Monitor.HttpRead(TrueUri).Result;
                    object[] WordPressData = Monitor.GetFromWordpress(ThisRead);
                    if (WordPressData != null && WordPressData[0] != null)
                    {
                        Current.Resources["LastSeenPageID"] = WordPressData[0];
                    }
                    else
                    {
                        if (!App.ErrorOpen)
                        {
                            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                NotFoundError NotFound = new NotFoundError();
                                NotFound.Show();
                            }));
                        }
                    }
                }
                catch
                {
                    if (!App.ErrorOpen)
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            NotFoundError NotFound = new NotFoundError();
                            NotFound.Show();
                        }));
                    }
                }
                FileIO.WritePersistentState((long)Current.Resources["LastSeenPageID"], (String)Current.Resources["CurrentSerialURI"], (uint)Current.Resources["CurrentTimeInterval"], (bool)Current.Resources["AutostartApp"], (uint)Current.Resources["AlertVolume"]);
            });
        }
	}
}
