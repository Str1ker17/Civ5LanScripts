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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Civ5LanLauncher.Properties;

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

        private static readonly Byte[] civ5ServerChallengePacket = { 0x00, 0x51, 0x45, 0x52, 0x65, 0x72, 0x69, 0x46 };
        private static readonly Byte[] civ5ServerResponseHeader = { 0x59, 0x4C, 0x50, 0x52, 0x65, 0x72, 0x69, 0x46 };

        [DllImport("ntdll.dll")]
        private static extern Int32 RtlCompareMemory(Byte[] b1, Byte[] b2, Int32 count);

        private DialogResult MessageBoxA(String text, String caption = null
          , MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None
          , MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1) {
            return (DialogResult)Program.mainForm.Invoke((Func<DialogResult>)delegate {
                return MessageBox.Show(this, text, caption ?? "Civilization V LAN Launcher", buttons, icon
                  , defaultButton);
            });
        }

        /* Fields. */
        private Random rnd = new Random();
        private NetworkInterface tapInterface;
        private IPAddress tapAddr;
        private Boolean canRunNet;
        private Boolean canRunExe;

        private void UpdateCanRun() {
            button_TestMTU.Visible = canRunNet;
            button_ServerList.Visible = canRunNet;
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
                    DialogResult dr = MessageBoxA("VPN не подключен. Попытаться включить автоматически?"
                      , buttons: MessageBoxButtons.YesNo, icon: MessageBoxIcon.Error);
                    if (dr == DialogResult.Yes) {
                        RunVPN();
                        CheckConnectivity();
                    }

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
                MessageBoxA($"{e.Message}", icon: MessageBoxIcon.Error);
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

        private void RunMTUTestAsync() {
            using (Mutex m = new Mutex()) {
                Dictionary<UInt16, Tuple<Ping, PingCompletedEventArgs>> hs =
                    new Dictionary<UInt16, Tuple<Ping, PingCompletedEventArgs>>();
                PingOptions po = new PingOptions { DontFragment = true, Ttl = 1 };

                UInt16 l_min = 1300;
                UInt16 l_max = (UInt16)tapInterface.GetIPProperties().GetIPv4Properties().Mtu;
                int net_timeout = 1000;
                int wait_timeout = 100;

                for (UInt16 l = l_min; l <= l_max; ++l) {
                    hs[l] = null;
                }

                foreach (UInt16 l in new List<UInt16>(hs.Keys)) {
                    Ping p = new Ping();
                    p.PingCompleted += ((sender, e) => {
                        if (e.Cancelled) {
                            return;
                        }

                        if (!m.WaitOne(wait_timeout)) {
                            throw new Exception("Failed to acquire mutex");
                        }

                        UInt16 len = (UInt16)e.UserState;
                        hs[len] = new Tuple<Ping, PingCompletedEventArgs>(hs[len].Item1, e);
                        m.ReleaseMutex();
                    });
                    p.SendAsync(IPAddress.Parse("172.16.16.1"), net_timeout, new Byte[l], po, l);
                    hs[l] = new Tuple<Ping, PingCompletedEventArgs>(p, null);
                    Thread.Sleep(1); /* Why? */
                }

                Thread.Sleep(net_timeout);

                if (!m.WaitOne(wait_timeout)) {
                    throw new Exception("Failed to acquire mutex");
                }

                String ret = String.Empty;
                foreach (UInt16 l in hs.Keys) {
                    Tuple<Ping, PingCompletedEventArgs> t = hs[l];
                    t.Item1.SendAsyncCancel();
                    PingCompletedEventArgs ev = t.Item2;
                    if (ev == null) {
                        ret += $"{l} sent and got no response\n";
                    }
                    else if (ev.Cancelled) {
                        ret += $"{l} cancelled\n";
                    }
                    else if (ev.Error != null) {
                        ret += $"{l} exception: {ev.Error}\n";
                    }
                    else if (ev.Reply.Status != IPStatus.Success) {
                        ret += $"{l} status {ev.Reply.Status}\n";
                    }

                    hs[l].Item1.Dispose();
                }

                if (ret != String.Empty) {
                    MessageBoxA(ret, icon: MessageBoxIcon.Warning);
                }
                else {
                    MessageBoxA("Все пинги прошли успешно - должно ли так быть?", icon: MessageBoxIcon.Warning);
                }
            }
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
            if (tapInterface == null) {
                return;
            }

            this.Invoke((MethodInvoker)delegate {
                contextMenuStrip_ServerList.Items.Clear();
                button_ServerList.Text = "Поиск серверов...";
                button_ServerList.Show();
            });

            Int32 items = 0;
            using (Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)) {
                s.Bind(new IPEndPoint(tapAddr, 0));
                s.Blocking = false;
                for (UInt16 port = 62900; port <= 62909; ++port) {
                    s.SendTo(civ5ServerChallengePacket, new IPEndPoint(IPAddress.Parse("172.16.16.255"), port));
                }

                Byte[] buf = new Byte[65535];
                EndPoint ep = new IPEndPoint(0, 0);
                s.Blocking = true;
                s.ReceiveTimeout = 1000;
                while (true) {
                    try {
                        Int32 buflen = s.ReceiveFrom(buf, ref ep);
                        IPEndPoint iep = ((IPEndPoint)ep);
                        if (RtlCompareMemory(buf, civ5ServerResponseHeader, civ5ServerResponseHeader.Length) == 0 ||
                            buflen < 20) {
                            MessageBoxA("Получены неизвестные данные!\n\n" +
                                        $"Отправитель: {iep.Address}:{iep.Port}\n\n" +
                                        "Данные:\n\n" +
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
                            button_ServerList.Text = $"Сервера: {items1} ...";
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

            this.Invoke((MethodInvoker)delegate { button_ServerList.Text = $"Сервера: {items}"; });
        }

        private Boolean CheckExe(String path) {
            String civ5ExeVersion = "1.0.3.279";
            if (!File.Exists(path)) {
                return false;
            }

            if (Path.GetExtension(path) != ".exe") {
                MessageBoxA("Исполняемый файл должен иметь расширение .exe", icon: MessageBoxIcon.Error);
                return false;
            }

            FileVersionInfo civ5ExeFileVer = FileVersionInfo.GetVersionInfo(path);
            Version civ5ExeVer = new Version(civ5ExeFileVer.FileMajorPart, civ5ExeFileVer.FileMinorPart
              , civ5ExeFileVer.FileBuildPart, civ5ExeFileVer.FilePrivatePart);
            if (civ5ExeVer != Version.Parse(civ5ExeVersion)) {
                DialogResult dr = MessageBoxA(
                    $"Этот исполняемый файл имеет версию {civ5ExeVer}, а нужна {civ5ExeVersion}\n\n" +
                    "Вы можете попытаться запустить другую версию Civilization V или даже другую игру, ответив Нет, но это будет " +
                    "ИСКЛЮЧИТЕЛЬНО НА ВАШ СТРАХ И РИСК.\n\n" +
                    "Выбрать другой файл игры?", buttons: MessageBoxButtons.YesNo, icon: MessageBoxIcon.Error);
                if (dr != DialogResult.No) {
                    return false;
                }

                dr = MessageBoxA(
                    "Если вы выберете неподходящую версию игры Civilization V, вы не сможете зайти в сетевую игру, а другие не смогут подключиться к вам.\n\n" +
                    $"Правилами сообщества принято играть на версии {civ5ExeVersion} с DLC Дивный новый мир.\n\n" +
                    "Всё равно, невзирая на все возможные и невозможные трудности, хотите продолжить?"
                  , buttons: MessageBoxButtons.OKCancel, defaultButton: MessageBoxDefaultButton.Button2
                  , icon: MessageBoxIcon.Warning);
                if (dr != DialogResult.OK) {
                    return false;
                }

                int attempts = 2;
                for (int i = 0; i < attempts; ++i) {
                    HashSet<int> hs = new HashSet<Int32>();
                    for (int j = 0; j < 3; ++j) {
                        if (!hs.Add(rnd.Next(10, 100))) {
                            --j;
                        }
                    }

                    List<int> ls = new List<Int32>(hs);
                    int val = ls[rnd.Next(3)];
                    dr = MessageBoxA(
                        $"Чтобы подтвердить серьёзность ваших намерений, нажмите ту кнопку, которая соотнесена с числом {val}\n\n" +
                        $"Да - {ls[0]}\n" +
                        $"Нет - {ls[1]}\n" +
                        $"Отмена - {ls[2]}", caption: $"Проверка серьёзности намерений {i + 1}/{attempts}"
                      , MessageBoxButtons.YesNoCancel);
                    int idx;
                    switch (dr) {
                        case DialogResult.Yes:
                            idx = 0;
                            break;
                        case DialogResult.No:
                            idx = 1;
                            break;
                        case DialogResult.Cancel:
                            idx = 2;
                            break;
                        default: throw new ApplicationException("wtf");
                    }

                    if (val != ls[idx]) {
                        return false;
                    }
                }

                MessageBoxA("Проверки на дурака успешно пройдены. Приятной игры :]", icon: MessageBoxIcon.Information);
            }

            return true;
        }

        private void RunOpenVPNDownload() {
            Process.Start(new ProcessStartInfo {
                UseShellExecute = true
              , FileName = "https://swupdate.openvpn.org/community/releases/OpenVPN-2.6.8-I001-amd64.msi"
            });
        }

        private void RunVPN() {
            try {
                String configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                  , @"OpenVPN\config");
                const String vpnCliExe = @"C:\Program Files\OpenVPN\bin\openvpn.exe";
                const String vpnGuiExe = @"C:\Program Files\OpenVPN\bin\openvpn-gui.exe";
                if (!File.Exists(vpnCliExe) || !File.Exists(vpnGuiExe)) {
                    DialogResult dr = MessageBoxA("Похоже, что OpenVPN не установлен. Открыть ссылку скачивания?"
                      , "Автоопределение OpenVPN не удалось", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                    if (dr == DialogResult.No) {
                        return;
                    }

                    RunOpenVPNDownload();
                    return;
                }

                FileVersionInfo vpnCliVer = FileVersionInfo.GetVersionInfo(vpnCliExe);
                if (Version.Parse(vpnCliVer.FileVersion) < Version.Parse("2.5.0")) {
                    DialogResult dr = MessageBoxA(
                        $"Установлен OpenVPN версии {vpnCliVer.FileMajorPart}.{vpnCliVer.FileMinorPart}.{vpnCliVer.FileBuildPart}, а нужен 2.5.0 или новее. Открыть ссылку скачивания?"
                      , "OpenVPN старой версии", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                    if (dr == DialogResult.No) {
                        return;
                    }

                    RunOpenVPNDownload();
                    return;
                }

                String configFilePath = Path.Combine(configDir, "nsk1-tap.ovpn");
                if (Settings.Default.OverwriteConfig || !File.Exists(configFilePath)) {
                    File.WriteAllBytes(configFilePath, Resources.OpenVPN_ClientConfig);
                }

                using (Process p = Process.Start(vpnGuiExe, "--command disconnect nsk1-tap")) {
                    p?.WaitForExit();
                }

                if (Settings.Default.VPNCredentialsHintLeft > 0) {
                    Settings.Default.VPNCredentialsHintLeft -= 1;
                    Settings.Default.Save();
                    MessageBoxA("Внимание:\n\n" +
                                "При первом подключении к VPN происходит регистрация - " +
                                "введите своё желаемое имя пользователя и пароль.\n\n" +
                                "При повторных подключениях к VPN нужно будет вводить те же " +
                                "имя пользователя и пароль, что и в первый раз.\n\n" +
                                $"Это сообщение будет показано ещё {Settings.Default.VPNCredentialsHintLeft} " +
                                $"раз{(Settings.Default.VPNCredentialsHintLeft >= 2 ? "а" : "")}"
                      , icon: MessageBoxIcon.Information);
                }

                using (Process unused = Process.Start(vpnGuiExe
                         , "--connect nsk1-tap --silent_connection 0")) { }

                MessageBoxA(@"Нажмите ""OK"", когда OpenVPN завершит подключение"
                  , icon: MessageBoxIcon.Information);
                button_TurnOnVPN.Visible = false;
                checkBox_TurnOnVPN_OverwriteConfig.Visible = false;
            }
            catch (Exception e) {
                MessageBoxA($"Произошла ошибка запуска VPN: {e.Message}", icon: MessageBoxIcon.Error);
            }
        }

        private async void RunGame() {
            button_RunGame.Enabled = false;
            try {
                String tmpDir = Path.GetTempPath();
                File.WriteAllBytes($@"{tmpDir}\ForceBindIP.exe", Resources.ForceBindIP);
                File.WriteAllBytes($@"{tmpDir}\BindIP.dll", Resources.BindIP);

                //FirewallHelper.Instance.GrantAuthorization(Settings.Default.Civ5ExePath, "Civ5 Executable");

                String exePath = $@"{Path.GetTempPath()}\ForceBindIP.exe";
                String exeArgs = $@"{tapInterface.Id} ""{Settings.Default.Civ5ExePath}""";
                Console.WriteLine($"Running {exePath} {exeArgs}...");
                using (Process p = Process.Start(new ProcessStartInfo {
                           FileName = exePath, Arguments = exeArgs
                         , WorkingDirectory = Path.GetDirectoryName(Settings.Default.Civ5ExePath) ??
                                              throw new ApplicationException(
                                                  "Не удалось получить рабочую папку для игры")
                       })) {
                    if (p == null) {
                        throw new ApplicationException("Не удалось запустить игру - процесс не создан");
                    }

                    p.WaitForExit(1000);
                }

                await Task.Delay(3000); /* awaitable version of Thread.Sleep() */
            }
            catch (Exception e) {
                MessageBoxA($"Произошла ошибка запуска игры: {e.Message}", icon: MessageBoxIcon.Error);
            }
            finally {
                button_RunGame.Enabled = true;
            }
        }

        private async void Form1_Load(Object sender, EventArgs e) {
            this.Enabled = false;
            await Task.Run(() => {
                UpdateCanRunExe(CheckExe(Settings.Default.Civ5ExePath));
                CheckConnectivity();
            });
            this.Enabled = true;
        }

        private void button_CheckConnectivity_Click(Object sender, EventArgs e) {
            CheckConnectivity();
        }

        private void button_ChooseGameExe_Click(Object sender, EventArgs e) {
            openFileDialog_chooseGameExe.ShowDialog();
            Boolean can = CheckExe(openFileDialog_chooseGameExe.FileName);
            if (can) {
                Settings.Default.Civ5ExePath = openFileDialog_chooseGameExe.FileName;
                Settings.Default.Save();
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

        private async void button_ShowServers_Click(Object sender, EventArgs e) {
            button_ServerList_MouseEnter(sender, e);
            await Task.Run(ListRunningServers);
        }

        private void button_ServerList_MouseEnter(Object sender, EventArgs e) {
            contextMenuStrip_ServerList.Show(((Button)sender), ((Button)sender).Size.Width, 0);
        }

        private void button_ServerList_MouseLeave(Object sender, EventArgs e) {
            contextMenuStrip_ServerList.Hide();
        }

        private void checkBox_TurnOnVPN_OverwriteConfig_CheckedChanged(Object sender, EventArgs e) {
            Settings.Default.Save();
        }

        private async void button_TestMTU_Click(Object sender, EventArgs e) {
            button_TestMTU.Enabled = false;
            this.UseWaitCursor = true;
            await Task.Run(RunMTUTestAsync);
            this.UseWaitCursor = false;
            button_TestMTU.Enabled = true;
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