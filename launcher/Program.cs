using System;
using System.Windows.Forms;

namespace Civ5LanLauncher
{
    internal static class Program
    {
        internal static Civ5LanLauncherUI mainForm;

        [STAThread]
        private static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            mainForm = new Civ5LanLauncherUI();

            Application.Run(mainForm);
        }
    }
}