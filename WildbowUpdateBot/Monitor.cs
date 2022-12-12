using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WildbowUpdateBot
{
    internal static class Monitor
    {

        private static CancellationTokenSource StopToken = new CancellationTokenSource();
        private static Task RealMonitor = null;
        private static readonly object StopLock = new object();
        public static void DoStop()
        {
            lock (StopLock)
            {
                FullStop = true;
                StopToken.Cancel();
            }
        }
        private static Boolean FullStop = false;
        public static void DoBreak()
        {
            lock (StopLock)
            {
                StopToken.Cancel();
            }
        }
        public static void DoMonitor(Boolean first)
        {
            WinFormsNotifMenu NotifMenu = new WinFormsNotifMenu();
            if(first) { NotifMenu.Icon.ShowBalloonTip(5000, "Wildbow Update Monitor is now running", "You will be notified when the current Wildbow web serial updates.", System.Windows.Forms.ToolTipIcon.Info); }
            App.TrueClose += DoStop;
            Action MonitorAction = new Action(() =>
            {
                Boolean InnerStop = false;
                CancellationToken StopTokenToken;
                while (!InnerStop)
                {
                    lock (StopLock)
                    {
                        StopToken = new CancellationTokenSource();
                        StopTokenToken = StopToken.Token;
                    }
                    MonitorOperation(false);
                    StopTokenToken.WaitHandle.WaitOne(Convert.ToInt32(System.Windows.Application.Current.Resources["CurrentTimeInterval"]) * 60000);
                    lock (StopLock)
                    {
                        InnerStop = FullStop;
                    }
                }
            });
            RealMonitor = new Task(() => MonitorAction());
            RealMonitor.Start();
        }
        public static void MonitorOperation(Boolean force)
        {
            try
            {
                String TrueUri = SanitizeURI((String)System.Windows.Application.Current.Resources["CurrentSerialURI"]);
                String ThisRead = HttpRead(TrueUri).Result;
                object[] WordPressData = GetFromWordpress(ThisRead);
                if (WordPressData != null && WordPressData[0] != null)
                {
                    if ((long)WordPressData[0] != (long)System.Windows.Application.Current.Resources["LastSeenPageID"] || force)
                    {
                        Boolean ShowNotif = true;
                        if ((long)System.Windows.Application.Current.Resources["LastSeenPageID"] == -1) { ShowNotif = false; }
                        lock (System.Windows.Application.Current.Resources)
                        {
                            System.Windows.Application.Current.Resources["LastSeenPageID"] = WordPressData[0];
                        }
                        FileIO.WritePersistentState((long)System.Windows.Application.Current.Resources["LastSeenPageID"], (String)System.Windows.Application.Current.Resources["CurrentSerialURI"], (uint)System.Windows.Application.Current.Resources["CurrentTimeInterval"], (bool)System.Windows.Application.Current.Resources["AutostartApp"], (uint)System.Windows.Application.Current.Resources["AlertVolume"]);
                        if (ShowNotif || force)
                        {
                            TransformedBitmap ResizedImage = null;
                            BitmapImage Image = Monitor.HttpReadImage((String)WordPressData[3]).Result;
                            if (Image != null)
                            {
                                double Scale = 300d / Image.PixelWidth;
                                ResizedImage = new TransformedBitmap(Image, new ScaleTransform(Scale, Scale));
                                ResizedImage.Freeze();
                            }
                            Image = null;
                            //Show notif!
                            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                Notification Notif = new Notification((String)WordPressData[1], (String)WordPressData[2], ResizedImage);
                                Notif.Show();
                            }));
                        }
                    }
                }
                else
                {
                    if (!App.ErrorOpen)
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            NotFoundError NotFound = new NotFoundError("Null return from the WordPress data extractor.");
                            NotFound.Show();
                        }));
                    }
                }
                ThisRead = "";
            }
            catch (Exception e)
            {
                if (!App.ErrorOpen)
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        NotFoundError NotFound = new NotFoundError(e.ToString());
                        NotFound.Show();
                    }));
                }
            }
        }
        public static String SanitizeURI(String uri)
        {
            uri = uri.ToLower();
            if(!uri.StartsWith("https://") && !uri.StartsWith("https://"))
            {
                if(uri.IndexOf('/') >= 0 && uri.IndexOf('/') < uri.Length - 1)
                {
                    uri = uri.Remove(0, uri.IndexOf('/'));
                }
                uri = "https://" + uri;
            }
            return uri;
        }
        public static async Task<String> HttpRead(String url)
        {
            HttpClient Client = new HttpClient();
            String Response = await Client.GetStringAsync(url);
            Client.Dispose();
            return Response;
        }
        public static async Task<BitmapImage?> HttpReadImage(String url)
        {
            try
            {
                HttpClient Client = new HttpClient();
                BitmapImage bitmapImage = null;
                using (var response = await Client.GetAsync(url))
                {
                    response.EnsureSuccessStatusCode();
                    using (Stream inputStream = await response.Content.ReadAsStreamAsync())
                    {
                        BitmapImage innerImage = new BitmapImage();
                        innerImage.BeginInit();
                        innerImage.StreamSource = inputStream;
                        innerImage.CacheOption = BitmapCacheOption.OnLoad;
                        innerImage.EndInit();
                        innerImage.Freeze();
                        bitmapImage = innerImage;
                        //innerImage = null;
                    }
                }
                Client.Dispose();
                //bitmapImage.StreamSource = null;
                //bitmapImage.Freeze();
                return bitmapImage;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static int FindHTMLTagClose(String str, String endParticle)
        {
            int InStep = 0;
            int Index = 0;
            Boolean LookingForTagOpenEnd = false;
            Boolean LookingForTagCloseEnd = false;
            Boolean FoundViable = false;
            while (Index < str.Length)
            {
                char Cur = str[Index];
                if(!LookingForTagOpenEnd && !LookingForTagCloseEnd)
                {
                    if(Index < str.Length - 1)
                    {
                        if (Cur == '<' && str[Index+1] != '/')
                        {
                            LookingForTagOpenEnd = true;
                        }
                        else if(Cur == '<')
                        {
                            LookingForTagCloseEnd = true;
                        }
                    }
                    else
                    {
                        return -1;
                    }
                }
                else
                {
                    if(Cur == '>')
                    {
                        if(LookingForTagOpenEnd)
                        {
                            if (str[Index - 1] != '/')
                            {
                                InStep++;
                            }
                            LookingForTagOpenEnd = false;
                        }
                        else if(LookingForTagCloseEnd)
                        {
                            InStep--;
                            LookingForTagCloseEnd = false;
                            if(FoundViable && InStep == 0)
                            {
                                return Index + 1;
                            }
                        }
                    }
                }
                if (LookingForTagCloseEnd && InStep <= 1 && InStep >= 0)
                {
                    if (str.Length - Index < endParticle.Length) { return -1; }
                    if (str.Substring(Index, endParticle.Length) == endParticle) { FoundViable = true; }
                }
                Index++;
            }
            return -1;
        }
        public static String PullDeepest(String str)
        {
            int InStep = 0;
            int Index = 0;
            Boolean LookingForTagOpenEnd = false;
            Boolean LookingForTagCloseEnd = false;
            int GreatestStep = 0;
            int GreatestStepStart = -1;
            int GreatestStepEnd = -1;
            int LastCommence = -1;
            while (Index < str.Length)
            {
                char Cur = str[Index];
                if (!LookingForTagOpenEnd && !LookingForTagCloseEnd)
                {
                    if (Index < str.Length - 1)
                    {
                        if (Cur == '<' && str[Index + 1] != '/')
                        {
                            LookingForTagOpenEnd = true;
                            LastCommence = Index;
                        }
                        else if (Cur == '<')
                        {
                            LookingForTagCloseEnd = true;
                            LastCommence = Index;
                        }
                    }
                }
                else
                {
                    if (Cur == '>')
                    {
                        if (LookingForTagOpenEnd)
                        {
                            if (str[Index - 1] != '/')
                            {
                                InStep++;
                            }
                            LookingForTagOpenEnd = false;
                            if(InStep > GreatestStep)
                            {
                                GreatestStep = InStep;
                                GreatestStepStart = Index + 1;
                            }
                        }
                        else if (LookingForTagCloseEnd)
                        {
                            InStep--;
                            LookingForTagCloseEnd = false;
                            if(InStep == GreatestStep - 1)
                            {
                                GreatestStepEnd = LastCommence;
                            }
                        }
                    }
                }
                Index++;
            }
            if(GreatestStepEnd < GreatestStepStart) { GreatestStepEnd = Index; }
            return str.Substring(GreatestStepStart, GreatestStepEnd - GreatestStepStart);
        }
        public static String RemoveEnclosedInclusive(String str, char open, char close)
        {
            while (str.Contains(open) && str.Contains(close) && str.IndexOf(open) < str.IndexOf(close))
            {
                int StartRemove = str.IndexOf(open);
                for (int i = StartRemove; i < str.Length; i++)
                {
                    if (str[i] == close)
                    {
                        str = str.Remove(StartRemove, i + 1 - StartRemove);
                        break;
                    }
                }
            }
            return str;
        }
        public static object[] GetFromWordpress(String wordpress)
        {
            long PostID = -1;
            String EntryTitle = null;
            String FirstContentLine = null;
            String ImageURL = null;
            if(wordpress.IndexOf("post-") == -1) { return null; }
            wordpress = wordpress.Remove(0, wordpress.IndexOf("post-"));
            String StrPostID = wordpress.Remove(0, 5);
            StrPostID = StrPostID.Remove(StrPostID.IndexOf('"'));
            PostID = Convert.ToInt64(StrPostID);

            String HLevel = "h1";
            if((wordpress.IndexOf("<h1 class=\"entry-title\">") == -1 && wordpress.IndexOf("<h2 class=\"entry-title\">") != -1) || (wordpress.IndexOf("<h2 class=\"entry-title\">") != -1 && wordpress.IndexOf("<h2 class=\"entry-title\">") < wordpress.IndexOf("<h1 class=\"entry-title\">")))
            {
                HLevel = "h2";
            }

            if (wordpress.IndexOf("<" + HLevel + " class=\"entry-title\">") == -1) { return new object[] { PostID, null, null, null }; }
            wordpress = wordpress.Remove(0, wordpress.IndexOf("<" + HLevel + " class=\"entry-title\">"));
            int TagEndpoint = FindHTMLTagClose(wordpress, "/" + HLevel);
            if (TagEndpoint == -1) { return new object[] { PostID, null, null, null }; }
            EntryTitle = HttpUtility.HtmlDecode(PullDeepest(wordpress.Remove(TagEndpoint)));

            if (wordpress.IndexOf("<p>") == -1) { return new object[] { PostID, EntryTitle, null, null }; }
            wordpress = wordpress.Remove(0, wordpress.IndexOf("<p>"));
            TagEndpoint = FindHTMLTagClose(wordpress, "/p");
            if (TagEndpoint == -1) { return new object[] { PostID, EntryTitle, null, null }; }
            FirstContentLine = RemoveEnclosedInclusive(PullDeepest(wordpress.Remove(TagEndpoint)), '<', '>');
            if(FirstContentLine.Length > 50) { FirstContentLine = FirstContentLine.Substring(0, 50); }

            if (wordpress.IndexOf("<img") == -1) { return new object[] { PostID, EntryTitle, FirstContentLine, null }; }
            wordpress = wordpress.Remove(0, wordpress.IndexOf("<img"));
            int SrcStart = wordpress.IndexOf("src=\"");
            if (SrcStart == -1) { return new object[] { PostID, EntryTitle, FirstContentLine, null }; }
            wordpress = wordpress.Remove(0, SrcStart + 5);
            ImageURL = wordpress.Remove(wordpress.IndexOfAny(new char[] { '?', '"' }));

            return new object[] { PostID, EntryTitle, FirstContentLine, ImageURL };
        }
    }
}
