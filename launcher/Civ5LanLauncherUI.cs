using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Civ5LanLauncher
{
    public partial class Civ5LanLauncherUI : Form
    {
        /* For VS Designer to work it should not be moved down. */
        public Civ5LanLauncherUI() {
            InitializeComponent();
        }

        /* Fields. */
        private NetworkInterface tapInterface;
        private bool canRunNet;
        private bool canRunExe;

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

        private void UpdateCanRunNet(bool can) {
            canRunNet = can;
            UpdateCanRun();
        }

        private void UpdateCanRunExe(bool can) {
            if (!File.Exists(Properties.Settings.Default.Civ5ExePath)) {
                can = false;
            }
            else {
                FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Properties.Settings.Default.Civ5ExePath);
                if (versionInfo.FileMajorPart != 1 || versionInfo.FileMinorPart != 0 ||
                    versionInfo.FileBuildPart != 3 ||
                    versionInfo.FilePrivatePart != 279) {
                    MessageBox.Show(
                        $"Этот исполняемый файл имеет версию {versionInfo.FileVersion}, а нужна 1.0.3.279");
                    can = false;
                }
            }

            canRunExe = can;
            UpdateCanRun();
        }

        /* Methods. */
        private NetworkInterface GetVPNLANInterface(out IPAddress addr) {
            foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces()) {
                foreach (UnicastIPAddressInformation ucastAddr in networkInterface.GetIPProperties().UnicastAddresses) {
                    if (ucastAddr.Address.AddressFamily != AddressFamily.InterNetwork) {
                        continue;
                    }

                    Byte[] bytes = ucastAddr.Address.GetAddressBytes();
                    if (bytes[0] == 172 && bytes[1] == 16 && bytes[2] == 16) {
                        addr = ucastAddr.Address;
                        return networkInterface;
                    }
                }
            }

            addr = null;
            return null;
        }

        private void CheckConnectivity() {
            UpdateCanRunNet(false);
            try {
                tapInterface = GetVPNLANInterface(out IPAddress addr);
                if (tapInterface == null) {
                    label_VPNActive.Text = "НЕТ";
                    label_IPAddr.Text = "-";
                    button_TurnOnVPN.Visible = true;

                    DialogResult dr = MessageBox.Show("VPN не подключен. Попытаться включить автоматически?"
                        , "VPN не подключен!", MessageBoxButtons.YesNo);
                    if (dr == DialogResult.No) {
                        return;
                    }

                    RunVPN();
                    return;
                }

                label_VPNActive.Text = "да";
                label_IPAddr.Text = addr.ToString();

                //FirewallHelper.Instance.GrantAuthorization(Application.ExecutablePath, "Civ5 LAN Launcher");

                HttpWebRequest req = (HttpWebRequest)WebRequest.Create("http://172.16.16.1:85/api.php");
                req.Method = "POST";
                using (StreamWriter sw = new StreamWriter(req.GetRequestStream())) {
                    sw.Write("{ \"func\": \"ping\" }");
                    sw.Close();
                }

                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                String responseText;
                using (StreamReader sr = new StreamReader(resp.GetResponseStream())) {
                    responseText = sr.ReadToEnd();
                }

                Console.WriteLine(responseText);

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

                label_LatencyValue.Text = matches[3].Groups[1].Value;
                if (Int32.Parse(matches[3].Groups[1].Value) < 0) {
                    label_UDPPingback.Text = "НЕТ";
                    throw new ApplicationException(
                        "Сервер не смог пропинговать вас. Отключите Firewall и попробуйте ещё раз");
                }

                if (addr.ToString() != matches[2].Groups[1].Value) {
                    throw new ApplicationException(
                        "IP-адрес на интерфейсе оличается от IP-адреса, который видит сервер (как такое возможно?!)");
                }

                /* TODO */
                label_UDPPingback.Text = "вероятно да";

                UpdateCanRunNet(true);
            }
            catch (Exception e) {
                MessageBox.Show(e.Message);
            }
        }

        private void RunVPN() {
            try {
                String configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                    , "OpenVPN\\config");
                if (!File.Exists(@"C:\Program Files\OpenVPN\bin\openvpn-gui.exe") || !Directory.Exists(configDir)) {
                    DialogResult dr = MessageBox.Show("Похоже, что OpenVPN не установлен. Открыть ссылку скачивания?"
                        , "Автоопределение OpenVPN не удалось", MessageBoxButtons.YesNo);
                    if (dr == DialogResult.No) {
                        return;
                    }

                    Process.Start(new ProcessStartInfo {
                        UseShellExecute = true
                        , FileName = "https://swupdate.openvpn.org/community/releases/OpenVPN-2.6.8-I001-amd64.msi"
                    });
                    return;
                }

                if (File.Exists(Path.Combine(configDir, "nsk1-tap.ovpn"))) {
                    DialogResult dr = MessageBox.Show("Файл конфигурации уже существует. Перезаписать?"
                        , "Файл конфигурации уже существует", MessageBoxButtons.YesNo);
                    if (dr == DialogResult.Yes) {
                        File.WriteAllBytes(Path.Combine(configDir, "nsk1-tap.ovpn")
                            , Properties.Resources.OpenVPN_ClientConfig);
                    }
                }

                using (Process p = Process.Start(@"C:\Program Files\OpenVPN\bin\openvpn-gui.exe"
                           , "--command exit")) {
                    p.WaitForExit();
                }

                MessageBox.Show("Внимание:\n\n" +
                                "При первом подключении к VPN происходит регистрация - " +
                                "введите своё желаемое имя пользователя и пароль.\n\n" +
                                "При повторных подключениях к VPN нужно будет вводить те же имя пользователя и пароль, что и в первый раз.");

                using (Process p = Process.Start(@"C:\Program Files\OpenVPN\bin\openvpn-gui.exe"
                           , "--connect nsk1-tap")) { }

                MessageBox.Show(@"Нажмите OK и ""Перепроверить связь"", когда OpenVPN завершит подключение");
                button_TurnOnVPN.Visible = false;
            }
            catch (Exception e) {
                MessageBox.Show(e.Message);
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
                    p.WaitForExit();
                }
            }
            catch (Exception e) {
                MessageBox.Show(e.Message);
            }
        }

        private void Form1_Load(object sender, EventArgs e) {
            /*
             * 1. Get interface & IP address
             * 2(?) Add firewall exception
             * 3. Check UDP pingback
             * 4. Get Civ 5 exe path
             * 5. Extract ForceBindIP from resources to temp dir
             * 6. Run
             */

            textBox_Civ5ExePath.Text = Properties.Settings.Default.Civ5ExePath;
            UpdateCanRunExe(File.Exists(Properties.Settings.Default.Civ5ExePath));
            CheckConnectivity();
        }

        private void button1_Click(object sender, EventArgs e) {
            chooseYourFighter.ShowDialog();
            if (chooseYourFighter.CheckFileExists) {
                Properties.Settings.Default.Civ5ExePath = chooseYourFighter.FileName;
                Properties.Settings.Default.Save();
                UpdateCanRunExe(true);
            }
            else {
                UpdateCanRunExe(false);
            }
        }

        private void button2_Click(object sender, EventArgs e) {
            RunGame();
        }

        private void button3_Click(object sender, EventArgs e) {
            CheckConnectivity();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e) {
            e.Graphics.FillRectangle(canRunNet ? Brushes.LawnGreen : Brushes.OrangeRed, 0, 0
                , pictureBox1.Width, pictureBox1.Height);
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e) {
            e.Graphics.FillRectangle(canRunExe ? Brushes.LawnGreen : Brushes.OrangeRed, 0, 0
                , pictureBox1.Width, pictureBox1.Height);
        }

        private void button_TurnOnVPN_Click(object sender, EventArgs e) {
            RunVPN();
        }
    }
}