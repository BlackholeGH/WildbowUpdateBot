using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WildbowUpdateBot
{
    internal class WinFormsNotifMenu
    {
        public System.Windows.Forms.NotifyIcon Icon
        {
            get;
        }
        public WinFormsNotifMenu()
        {
            Icon = new System.Windows.Forms.NotifyIcon();
            Icon.Icon = AppResources.WBIcon;
            Icon.Visible = true;
            Icon.Text = "Wildbow Update Monitor";

            System.Windows.Forms.ContextMenuStrip notifMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            notifMenuStrip.Items.Add("About", null, MenuAbout_Click);
            notifMenuStrip.Items.Add("Settings", null, MenuSettings_Click);
            notifMenuStrip.Items.Add("Exit application", null, MenuExit_Click);

            Icon.ContextMenuStrip = notifMenuStrip;
        }
        void MenuAbout_Click(object sender, EventArgs e)
        {
            if (App.AboutOpen == false)
            {
                About about = new About();
                about.Show();
            }
            else
            {
                About.Current.Focus();
            }
        }

        void MenuSettings_Click(object sender, EventArgs e)
        {
            if (App.SettingsOpen == false)
            {
                App.SettingsOpen = true;
            }
            else
            {
                Settings.Current.Focus();
            }
        }

        void MenuExit_Click(object sender, EventArgs e)
        {
            App.TrueCloseFire();
        }
    }
}
