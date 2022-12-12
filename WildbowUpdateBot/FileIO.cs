using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;

namespace WildbowUpdateBot
{
    public static class FileIO
    {
        public static String DecloseOne(char encloser, String target)
        {
            target = target.Remove(0, target.IndexOf(encloser) + 1);
            target = target.Remove(target.IndexOf(encloser));
            return target;
        }
        public static String DecloseTwo(char firstEncloser, char secondEncloser, String target)
        {
            target = target.Remove(0, target.IndexOf(firstEncloser) + 1);
            target = target.Remove(target.IndexOf(secondEncloser));
            return target;
        }
        static public object[] PullPersistentState()
        {
            lock (WriteLock)
            {
                FileInfo PSArchive = new FileInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Blackhole Media Systems\\Wildbow Update Monitor Bot\\log.txt");
                if (PSArchive.Exists)
                {
                    BinaryReader Reader = new BinaryReader(PSArchive.OpenRead());
                    String State = Reader.ReadString();
                    object[] Objects = new object[5];
                    while (State.Contains("<"))
                    {
                        State = State.Remove(0, State.IndexOf("<") + 1);
                        String CurrentAttr = State.Remove(State.IndexOf(">"));
                        State = State.Remove(0, State.IndexOf(">") + 1);
                        String[] Attr = CurrentAttr.Split('=');
                        switch (Attr[0])
                        {
                            case "lastid":
                                Objects[0] = Convert.ToInt64(Attr[1]);
                                break;
                            case "uri":
                                Objects[1] = Attr[1];
                                break;
                            case "interval":
                                Objects[2] = Convert.ToUInt32(Attr[1]);
                                break;
                            case "autostart":
                                Objects[3] = (Attr[1].ToLower() == "true");
                                break;
                            case "volume":
                                Objects[4] = Convert.ToUInt32(Attr[1]);
                                break;
                        }
                    }
                    Reader.Close();
                    return Objects;
                }
            }
            return null;
        }
        static readonly object WriteLock = new object();
        static public void WritePersistentState(long lastPostID, String uri, long interval, Boolean autoStart, uint volume)
        {
            lock (WriteLock)
            {
                String StateRecord = "Blackhole's Wildbow Web Serial Update Monitor Bot - Log\nFile last updated at " + System.DateTime.Now.ToShortTimeString() + " " + System.DateTime.Now.ToShortDateString() + " \n"
                    + "<lastid=" + lastPostID + "> \n"
                    + "<uri=" + uri + "> \n"
                    + "<interval=" + interval + "> \n"
                    + "<autostart=" + autoStart + "> \n"
                    + "<volume=" + volume + "> \n";
                BinaryWriter Writer = new BinaryWriter(new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Blackhole Media Systems\\Wildbow Update Monitor Bot\\log.txt", FileMode.Create));
                Writer.Write(StateRecord);
                Writer.Close();
            }
        }
        static public void InitializeAppFolders()
        {
            String LocalDataStore = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            DirectoryInfo Dir = new DirectoryInfo(Path.Combine(LocalDataStore, "Blackhole Media Systems"));
            if (!Dir.Exists) { Dir.Create(); }
            Dir = new DirectoryInfo(Path.Combine(Dir.FullName, "Wildbow Update Monitor Bot"));
            if (!Dir.Exists) { Dir.Create(); }
        }
    }
}
