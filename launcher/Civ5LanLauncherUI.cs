using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Civ5LanLauncher
{
    /*
     * 1. Get interface & IP address
     * 2(?) Add firewall exception
     * 3. Check UDP pingback
     * 4. Get Civ 5 exe path
     * 5. Extract ForceBindIP from resources to temp dir
     * 6. Run
     */
    public partial class Civ5LanLauncherUI : Form
    {
        /* For VS Designer to work it should not be moved down. */
        public Civ5LanLauncherUI() {
            InitializeComponent();
        }

        private static readonly Byte[] civ5ServerPing = { 0x00, 0x51, 0x45, 0x52, 0x65, 0x72, 0x69, 0x46 };
        private static readonly Byte[] civ5ServerResponse = { 0x59, 0x4C, 0x50, 0x52, 0x65, 0x72, 0x69, 0x46 };

        [DllImport("ntdll.dll")]
        private static extern Int32 RtlCompareMemory(Byte[] b1, Byte[] b2, Int32 count);

        private DialogResult MessageBoxA(String text, String caption = null
            , MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None) {
            return MessageBox.Show(this, text, caption ?? "Civilization V LAN Launcher", buttons, icon);
        }

        /* Fields. */
        private NetworkInterface tapInterface;
        private IPAddress tapAddr;
        private Boolean canRunNet;
        private Boolean canRunExe;

        private void UpdateCanRun() {
            if (canRunNet && canRunExe) {
                toolStripStatusLabel1.Text = "Всё готово для игры";
                button_RunGame.Enabled = true;
            }
            else {
                if (!canRunNet) {
                    toolStripStatusLabel1.Text = "Сетевое подключение не работает";
                }
                else if (!canRunExe) {
                    toolStripStatusLabel1.Text = "Не выбран файл игры";
                }

                button_RunGame.Enabled = false;
            }

            pictureBox1.Invalidate();
            pictureBox2.Invalidate();
        }

        private void UpdateCanRunNet(Boolean can) {
            canRunNet = can;
            this.Invoke((MethodInvoker)UpdateCanRun);
        }

        private void UpdateCanRunExe(Boolean can) {
            canRunExe = can;
            this.Invoke((MethodInvoker)UpdateCanRun);
        }

        /* Methods. */
        private NetworkInterface GetVPNLANInterface(out IPAddress addr) {
            foreach (NetworkInterface netIf in NetworkInterface.GetAllNetworkInterfaces()) {
                foreach (UnicastIPAddressInformation ucastAddr in netIf.GetIPProperties().UnicastAddresses) {
                    if (ucastAddr.Address.AddressFamily != AddressFamily.InterNetwork) {
                        continue;
                    }

                    Byte[] bytes = ucastAddr.Address.GetAddressBytes();
                    if (bytes[0] == 172 && bytes[1] == 16 && bytes[2] == 16) {
                        addr = ucastAddr.Address;
                        return netIf;
                    }
                }
            }

            addr = null;
            return null;
        }

        private void UpdateVisualStatus(String vpnActive = "", String ipAddr = "", String serverMode = ""
            , String latency = "") {
            this.Invoke((MethodInvoker)delegate {
                if (vpnActive != "") label_VPNActive.Text = vpnActive;
                if (ipAddr != "") label_IPAddr.Text = ipAddr;
                if (serverMode != "") label_UDPServerMode.Text = serverMode;
                if (latency != "") label_LatencyValue.Text = latency;
            });
        }

        private void CheckConnectivity() {
            UpdateCanRunNet(false);
            UpdateVisualStatus("НЕТ", "-", "НЕТ", "-1");

            try {
                tapInterface = GetVPNLANInterface(out tapAddr);
                if (tapInterface == null) {
                    this.Invoke((MethodInvoker)delegate {
                        button_TurnOnVPN.Visible = true;
                        checkBox_TurnOnVPN_OverwriteConfig.Visible = true;
                    });
                    return;
                }

                UpdateVisualStatus(vpnActive: "да", ipAddr: tapAddr.ToString());

                //FirewallHelper.Instance.GrantAuthorization(Application.ExecutablePath, "Civ5 LAN Launcher");

                Task.Run(ListRunningServers);

                CheckVPNResponse();
                CheckUDPServerMode();

                UpdateCanRunNet(true);
            }
            catch (Exception e) {
                MessageBoxA($"{e.Message}\n\n{e.StackTrace}", icon: MessageBoxIcon.Error);
            }
        }

        private static String DoHttpApiRequest(String postBody) {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("http://172.16.16.1:85/api.php");
            req.Timeout = 5000;
            req.Method = "POST";
            req.ServicePoint.Expect100Continue = false;
            req.KeepAlive = false;
            req.ContentType = "application/json";
            using (Stream reqs = req.GetRequestStream()) {
                using (StreamWriter sw = new StreamWriter(reqs)) {
                    sw.Write(postBody);
                    sw.Close();
                }

                reqs.Close();
            }

            String responseText;
            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse()) {
                using (Stream resps = resp.GetResponseStream()) {
                    using (StreamReader sr = new StreamReader(resps ?? throw new InvalidOperationException())) {
                        responseText = sr.ReadToEnd();
                        sr.Close();
                    }

                    resps.Close();
                }

                resp.Close();
            }

            req.Abort();

            Console.WriteLine(responseText);
            return responseText;
        }

        private void CheckVPNResponse() {
            String responseText = DoHttpApiRequest(@"{ ""func"": ""ping"" }");

            Match m;
            List<Match> matches = new List<Match>(5);

            matches.Add(m = Regex.Match(responseText, "\"status\":([0-9]+)"));
            if (!m.Success) {
                throw new ApplicationException("Сервер вернул странный ответ (0)");
            }

            matches.Add(m = Regex.Match(responseText, "\"msg\":\"([^\"]+)\""));
            if (!m.Success) {
                throw new ApplicationException("Сервер вернул странный ответ (1)");
            }

            if (matches[0].Groups[1].Value != "0") {
                throw new ApplicationException(matches[1].Groups[1].Value);
            }

            matches.Add(m = Regex.Match(responseText, "\"src_ip\":\"([^\"]+)\""));
            if (!m.Success) {
                throw new ApplicationException("Сервер вернул странный ответ (2)");
            }

            matches.Add(m = Regex.Match(responseText, "\"latency_min\":([0-9-]+)"));
            if (!m.Success) {
                throw new ApplicationException("Сервер вернул странный ответ (3)");
            }

            UpdateVisualStatus(latency: matches[3].Groups[1].Value);
            if (Int32.Parse(matches[3].Groups[1].Value) < 0) {
                throw new ApplicationException(
                    "Сервер не смог пропинговать вас. Отключите Firewall и попробуйте ещё раз");
            }

            if (tapAddr.ToString() != matches[2].Groups[1].Value) {
                throw new ApplicationException(
                    "IP-адрес на интерфейсе оличается от IP-адреса, который видит сервер (как такое возможно?!)");
            }

            UpdateVisualStatus(serverMode: "возможно?");
        }

        private void CheckUDPServerMode() {
            using (Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)) {
                UInt16 port = 62900;
                while (true) {
                    try {
                        s.Bind(new IPEndPoint(tapAddr, port));
                        break;
                    }
                    catch (SocketException e) {
                        if (e.SocketErrorCode == SocketError.AddressAlreadyInUse && port <= 62999) {
                            ++port;
                            continue;
                        }

                        throw;
                    }
                }

                UInt16 size = 8192;
                String parms = $@"""func"": ""udpservertest"", ""port"": {port}, ""size"": {size}";
                DoHttpApiRequest("{" + parms + "}");

                Byte[] buf = new Byte[size];
                EndPoint ep = new IPEndPoint(0, 0);
                s.ReceiveTimeout = 1000;
                s.ReceiveBufferSize = 65535;
                try {
                    Int32 buflen = s.ReceiveFrom(buf, ref ep);
                    if (buflen != size) {
                        throw new ApplicationException(
                            $"Данные с сервера короче ожидаемого - прочитано {buflen} байт вместо {size}");
                    }
                }
                catch (SocketException e) {
                    if (e.SocketErrorCode == SocketError.TimedOut) {
                        UpdateVisualStatus(serverMode: "НЕТ");
                        throw new ApplicationException(
                            "Вы не смогли сымитировать игровой сервер. Отключите Firewall и попробуйте ещё раз");
                    }

                    throw;
                }

                UpdateVisualStatus(serverMode: "да");
            }
        }

        private void ListRunningServers() {
            this.Invoke((MethodInvoker)delegate {
                contextMenuStrip_ServerList.Items.Clear();
                button_ServerList.Text = "Поиск серверов...";
                button_ServerList.Show();
            });
            if (tapInterface == null) {
                return;
            }

            Int32 items = 0;
            using (Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)) {
                s.Bind(new IPEndPoint(tapAddr, 0));
                s.Blocking = false;
                for (UInt16 port = 62900; port <= 62909; ++port) {
                    s.SendTo(civ5ServerPing, new IPEndPoint(IPAddress.Parse("172.16.16.255"), port));
                }

                Byte[] buf = new Byte[65535];
                EndPoint ep = new IPEndPoint(0, 0);
                s.Blocking = true;
                s.ReceiveTimeout = 1000;
                while (true) {
                    try {
                        Int32 buflen = s.ReceiveFrom(buf, ref ep);
                        IPEndPoint iep = ((IPEndPoint)ep);
                        if (RtlCompareMemory(buf, civ5ServerResponse, civ5ServerResponse.Length) == 0 || buflen < 20) {
                            MessageBoxA($"Получены неизвестные данные!\n\n" +
                                        $"Отправитель: {iep.Address}:{iep.Port}\n\n" +
                                        $"Данные:\n\n" +
                                        $"{BitConverter.ToString(buf, buflen)}", icon: MessageBoxIcon.Error);
                            continue;
                        }

                        ++items;
                        UInt32 playersIn = BitConverter.ToUInt32(buf, 8);
                        UInt32 playersMax = BitConverter.ToUInt32(buf, 12);
                        UInt32 namelen = BitConverter.ToUInt32(buf, 16);
                        String name = Encoding.UTF8.GetString(buf, 20, Math.Min((Int32)namelen, buflen - 20));
                        String descr = $"{iep.Address}:{iep.Port}  '{name}': {playersIn}/{playersMax}";
                        Int32 items1 = items;
                        this.Invoke((MethodInvoker)delegate {
                            contextMenuStrip_ServerList.Items.Add(descr).Enabled = false;
                            button_ServerList.Text = $"Сервера: {items1}";
                        });
                    }
                    catch (SocketException e) {
                        if (e.SocketErrorCode != SocketError.TimedOut) {
                            MessageBoxA($"Ошибка {e.SocketErrorCode} ({e.ErrorCode}): {e.Message}"
                                , icon: MessageBoxIcon.Error);
                        }

                        break;
                    }
                }
            }
        }

        private Boolean CheckExe(String path) {
            if (!File.Exists(path)) {
                return false;
            }

            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(path);
            if (versionInfo.FileMajorPart != 1 || versionInfo.FileMinorPart != 0 ||
                versionInfo.FileBuildPart != 3 ||
                versionInfo.FilePrivatePart != 279) {
                MessageBoxA($"Этот исполняемый файл имеет версию {versionInfo.FileVersion}, а нужна 1.0.3.279");
                return false;
            }

            return true;
        }

        private void RunVPN() {
            try {
                String configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                    , "OpenVPN\\config");
                const String vpnCliExe = @"C:\Program Files\OpenVPN\bin\openvpn.exe";
                const String vpnGuiExe = @"C:\Program Files\OpenVPN\bin\openvpn-gui.exe";
                if (!File.Exists(vpnCliExe) || !File.Exists(vpnGuiExe)) {
                    DialogResult dr = MessageBoxA("Похоже, что OpenVPN не установлен. Открыть ссылку скачивания?"
                        , "Автоопределение OpenVPN не удалось", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                    if (dr == DialogResult.No) {
                        return;
                    }

                    Process.Start(new ProcessStartInfo {
                        UseShellExecute = true
                        , FileName = "https://swupdate.openvpn.org/community/releases/OpenVPN-2.6.8-I001-amd64.msi"
                    });
                    return;
                }

                FileVersionInfo vpnCliVer = FileVersionInfo.GetVersionInfo(vpnCliExe);
                if (Version.Parse(vpnCliVer.FileVersion) < Version.Parse("2.6")) {
                    DialogResult dr = MessageBoxA(
                        $"Установлен OpenVPN версии {vpnCliVer.FileMajorPart}.{vpnCliVer.FileMinorPart}.{vpnCliVer.FileBuildPart}, а нужен 2.6 или новее. Открыть ссылку скачивания?"
                        , "OpenVPN старой версии", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                    if (dr == DialogResult.No) {
                        return;
                    }

                    Process.Start(new ProcessStartInfo {
                        UseShellExecute = true
                        , FileName = "https://swupdate.openvpn.org/community/releases/OpenVPN-2.6.8-I001-amd64.msi"
                    });
                    return;
                }

                String configFilePath = Path.Combine(configDir, "nsk1-tap.ovpn");
                if (Properties.Settings.Default.OverwriteConfig || !File.Exists(configFilePath)) {
                    File.WriteAllBytes(configFilePath, Properties.Resources.OpenVPN_ClientConfig);
                }

                using (Process p = Process.Start(vpnGuiExe, "--command disconnect nsk1-tap")) {
                    p?.WaitForExit();
                }

                MessageBoxA("Внимание:\n\n" +
                            "При первом подключении к VPN происходит регистрация - " +
                            "введите своё желаемое имя пользователя и пароль.\n\n" +
                            "При повторных подключениях к VPN нужно будет вводить те же " +
                            "имя пользователя и пароль, что и в первый раз."
                    , icon: MessageBoxIcon.Information);

                using (Process unused = Process.Start(vpnGuiExe
                           , "--connect nsk1-tap --silent_connection 0")) { }

                MessageBoxA(@"Нажмите ""OK"", когда OpenVPN завершит подключение"
                    , icon: MessageBoxIcon.Information);
                button_TurnOnVPN.Visible = false;
                checkBox_TurnOnVPN_OverwriteConfig.Visible = false;
            }
            catch (Exception e) {
                MessageBoxA(e.Message, icon: MessageBoxIcon.Error);
            }
        }

        private void RunGame() {
            try {
                String tmpDir = Path.GetTempPath();
                File.WriteAllBytes($"{tmpDir}\\ForceBindIP.exe", Properties.Resources.ForceBindIP);
                File.WriteAllBytes($"{tmpDir}\\BindIP.dll", Properties.Resources.BindIP);

                //FirewallHelper.Instance.GrantAuthorization(Settings.Default.Civ5ExePath, "Civ5 Executable");

                Console.WriteLine(
                    $"Running {Path.GetTempPath()}\\ForceBindIP.exe {tapInterface.Id} \"{Properties.Settings.Default.Civ5ExePath}\"");
                using (Process p = Process.Start($"{Path.GetTempPath()}\\ForceBindIP.exe"
                           , $"{tapInterface.Id} \"{Properties.Settings.Default.Civ5ExePath}\"")) {
                    p?.WaitForExit();
                }
            }
            catch (Exception e) {
                MessageBoxA(e.Message, icon: MessageBoxIcon.Error);
            }
        }

        private void Form1_Load(Object sender, EventArgs e) {
            UpdateCanRunExe(CheckExe(Properties.Settings.Default.Civ5ExePath));
            Task.Run(CheckConnectivity);
        }

        private void button_CheckConnectivity_Click(Object sender, EventArgs e) {
            CheckConnectivity();
        }

        private void button_ChooseGameExe_Click(Object sender, EventArgs e) {
            openFileDialog_chooseGameExe.ShowDialog();
            Boolean can = CheckExe(openFileDialog_chooseGameExe.FileName);
            if (can) {
                Properties.Settings.Default.Civ5ExePath = openFileDialog_chooseGameExe.FileName;
                Properties.Settings.Default.Save();
                UpdateCanRunExe(true);
            }
        }

        private void button_RunGame_Click(Object sender, EventArgs e) {
            RunGame();
        }

        private void button_TurnOnVPN_Click(Object sender, EventArgs e) {
            RunVPN();
            CheckConnectivity();
        }

        private void button_ShowServers_Click(Object sender, EventArgs e) {
            Task.Run(ListRunningServers);
            button_ServerList_MouseEnter(sender, e);
        }

        private void button_ServerList_MouseEnter(Object sender, EventArgs e) {
            contextMenuStrip_ServerList.Show(((Button)sender), ((Button)sender).Size.Width, 0);
        }

        private void button_ServerList_MouseLeave(Object sender, EventArgs e) {
            contextMenuStrip_ServerList.Hide();
        }

        private void checkBox_TurnOnVPN_OverwriteConfig_CheckedChanged(Object sender, EventArgs e) {
            Properties.Settings.Default.Save();
        }

        private void pictureBox1_Paint(Object sender, PaintEventArgs e) {
            e.Graphics.FillRectangle(canRunNet ? Brushes.LawnGreen : Brushes.OrangeRed, 0, 0
                , pictureBox1.Width, pictureBox1.Height);
        }

        private void pictureBox2_Paint(Object sender, PaintEventArgs e) {
            e.Graphics.FillRectangle(canRunExe ? Brushes.LawnGreen : Brushes.OrangeRed, 0, 0
                , pictureBox2.Width, pictureBox2.Height);
        }
    }
}