namespace Civ5LanLauncher
{
    partial class Civ5LanLauncherUI
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Civ5LanLauncherUI));
            this.button_ChooseGameExe = new System.Windows.Forms.Button();
            this.button_RunGame = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label_VPNActive = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label_UDPServerMode = new System.Windows.Forms.Label();
            this.label_IPAddr = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label_LatencyValue = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.button_CheckConnectivity = new System.Windows.Forms.Button();
            this.button_TurnOnVPN = new System.Windows.Forms.Button();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.checkBox_TurnOnVPN_OverwriteConfig = new System.Windows.Forms.CheckBox();
            this.textBox_Civ5ExePath = new System.Windows.Forms.TextBox();
            this.openFileDialog_chooseGameExe = new System.Windows.Forms.OpenFileDialog();
            this.hoverHints = new System.Windows.Forms.ToolTip(this.components);
            this.contextMenuStrip_ServerList = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.button_ServerList = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.statusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // button_ChooseGameExe
            // 
            resources.ApplyResources(this.button_ChooseGameExe, "button_ChooseGameExe");
            this.button_ChooseGameExe.Name = "button_ChooseGameExe";
            this.button_ChooseGameExe.UseVisualStyleBackColor = true;
            this.button_ChooseGameExe.Click += new System.EventHandler(this.button_ChooseGameExe_Click);
            // 
            // button_RunGame
            // 
            resources.ApplyResources(this.button_RunGame, "button_RunGame");
            this.button_RunGame.Name = "button_RunGame";
            this.button_RunGame.UseVisualStyleBackColor = true;
            this.button_RunGame.Click += new System.EventHandler(this.button_RunGame_Click);
            // 
            // pictureBox1
            // 
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            this.hoverHints.SetToolTip(this.pictureBox1, resources.GetString("pictureBox1.ToolTip"));
            this.pictureBox1.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox1_Paint);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // label_VPNActive
            // 
            resources.ApplyResources(this.label_VPNActive, "label_VPNActive");
            this.label_VPNActive.Name = "label_VPNActive";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            this.hoverHints.SetToolTip(this.label4, resources.GetString("label4.ToolTip"));
            // 
            // label_UDPServerMode
            // 
            resources.ApplyResources(this.label_UDPServerMode, "label_UDPServerMode");
            this.label_UDPServerMode.Name = "label_UDPServerMode";
            // 
            // label_IPAddr
            // 
            resources.ApplyResources(this.label_IPAddr, "label_IPAddr");
            this.label_IPAddr.Name = "label_IPAddr";
            // 
            // label7
            // 
            resources.ApplyResources(this.label7, "label7");
            this.label7.Name = "label7";
            // 
            // label_LatencyValue
            // 
            resources.ApplyResources(this.label_LatencyValue, "label_LatencyValue");
            this.label_LatencyValue.Name = "label_LatencyValue";
            // 
            // label9
            // 
            resources.ApplyResources(this.label9, "label9");
            this.label9.Name = "label9";
            // 
            // button_CheckConnectivity
            // 
            resources.ApplyResources(this.button_CheckConnectivity, "button_CheckConnectivity");
            this.button_CheckConnectivity.Name = "button_CheckConnectivity";
            this.button_CheckConnectivity.UseVisualStyleBackColor = true;
            this.button_CheckConnectivity.Click += new System.EventHandler(this.button_CheckConnectivity_Click);
            // 
            // button_TurnOnVPN
            // 
            resources.ApplyResources(this.button_TurnOnVPN, "button_TurnOnVPN");
            this.button_TurnOnVPN.Name = "button_TurnOnVPN";
            this.button_TurnOnVPN.UseVisualStyleBackColor = true;
            this.button_TurnOnVPN.Click += new System.EventHandler(this.button_TurnOnVPN_Click);
            // 
            // statusStrip
            // 
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            resources.ApplyResources(this.statusStrip, "statusStrip");
            this.statusStrip.Name = "statusStrip";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            resources.ApplyResources(this.toolStripStatusLabel1, "toolStripStatusLabel1");
            // 
            // pictureBox2
            // 
            resources.ApplyResources(this.pictureBox2, "pictureBox2");
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.TabStop = false;
            this.hoverHints.SetToolTip(this.pictureBox2, resources.GetString("pictureBox2.ToolTip"));
            this.pictureBox2.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox2_Paint);
            // 
            // checkBox_TurnOnVPN_OverwriteConfig
            // 
            resources.ApplyResources(this.checkBox_TurnOnVPN_OverwriteConfig, "checkBox_TurnOnVPN_OverwriteConfig");
            this.checkBox_TurnOnVPN_OverwriteConfig.Checked = global::Civ5LanLauncher.Properties.Settings.Default.OverwriteConfig;
            this.checkBox_TurnOnVPN_OverwriteConfig.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_TurnOnVPN_OverwriteConfig.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::Civ5LanLauncher.Properties.Settings.Default, "OverwriteConfig", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.checkBox_TurnOnVPN_OverwriteConfig.Name = "checkBox_TurnOnVPN_OverwriteConfig";
            this.hoverHints.SetToolTip(this.checkBox_TurnOnVPN_OverwriteConfig, resources.GetString("checkBox_TurnOnVPN_OverwriteConfig.ToolTip"));
            this.checkBox_TurnOnVPN_OverwriteConfig.UseVisualStyleBackColor = true;
            this.checkBox_TurnOnVPN_OverwriteConfig.CheckedChanged += new System.EventHandler(this.checkBox_TurnOnVPN_OverwriteConfig_CheckedChanged);
            // 
            // textBox_Civ5ExePath
            // 
            this.textBox_Civ5ExePath.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::Civ5LanLauncher.Properties.Settings.Default, "Civ5ExePath", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            resources.ApplyResources(this.textBox_Civ5ExePath, "textBox_Civ5ExePath");
            this.textBox_Civ5ExePath.Name = "textBox_Civ5ExePath";
            this.textBox_Civ5ExePath.ReadOnly = true;
            this.textBox_Civ5ExePath.Text = global::Civ5LanLauncher.Properties.Settings.Default.Civ5ExePath;
            // 
            // openFileDialog_chooseGameExe
            // 
            this.openFileDialog_chooseGameExe.AddExtension = false;
            this.openFileDialog_chooseGameExe.FileName = global::Civ5LanLauncher.Properties.Settings.Default.Civ5ExePath;
            resources.ApplyResources(this.openFileDialog_chooseGameExe, "openFileDialog_chooseGameExe");
            // 
            // hoverHints
            // 
            this.hoverHints.AutomaticDelay = 50;
            this.hoverHints.AutoPopDelay = 0;
            this.hoverHints.InitialDelay = 50;
            this.hoverHints.ReshowDelay = 10;
            this.hoverHints.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            // 
            // contextMenuStrip_ServerList
            // 
            this.contextMenuStrip_ServerList.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip_ServerList.Name = "contextMenuStrip1";
            resources.ApplyResources(this.contextMenuStrip_ServerList, "contextMenuStrip_ServerList");
            this.hoverHints.SetToolTip(this.contextMenuStrip_ServerList, resources.GetString("contextMenuStrip_ServerList.ToolTip"));
            // 
            // button_ServerList
            // 
            resources.ApplyResources(this.button_ServerList, "button_ServerList");
            this.button_ServerList.Name = "button_ServerList";
            this.button_ServerList.UseVisualStyleBackColor = true;
            this.button_ServerList.Click += new System.EventHandler(this.button_ShowServers_Click);
            this.button_ServerList.MouseEnter += new System.EventHandler(this.button_ServerList_MouseEnter);
            this.button_ServerList.MouseLeave += new System.EventHandler(this.button_ServerList_MouseLeave);
            // 
            // Civ5LanLauncherUI
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.button_ServerList);
            this.Controls.Add(this.checkBox_TurnOnVPN_OverwriteConfig);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.button_TurnOnVPN);
            this.Controls.Add(this.button_CheckConnectivity);
            this.Controls.Add(this.textBox_Civ5ExePath);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label_LatencyValue);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label_IPAddr);
            this.Controls.Add(this.label_UDPServerMode);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label_VPNActive);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.button_RunGame);
            this.Controls.Add(this.button_ChooseGameExe);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Civ5LanLauncherUI";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialog_chooseGameExe;
        private System.Windows.Forms.Button button_ChooseGameExe;
        private System.Windows.Forms.Button button_RunGame;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label_VPNActive;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label_UDPServerMode;
        private System.Windows.Forms.Label label_IPAddr;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label_LatencyValue;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox textBox_Civ5ExePath;
        private System.Windows.Forms.Button button_CheckConnectivity;
        private System.Windows.Forms.Button button_TurnOnVPN;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.CheckBox checkBox_TurnOnVPN_OverwriteConfig;
        private System.Windows.Forms.ToolTip hoverHints;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip_ServerList;
        private System.Windows.Forms.Button button_ServerList;
    }
}

