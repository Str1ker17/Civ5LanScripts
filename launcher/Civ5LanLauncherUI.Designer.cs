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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Civ5LanLauncherUI));
            this.button1 = new System.Windows.Forms.Button();
            this.button_RunGame = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label_VPNActive = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label_UDPPingback = new System.Windows.Forms.Label();
            this.label_IPAddr = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label_LatencyValue = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.button_TurnOnVPN = new System.Windows.Forms.Button();
            this.textBox_Civ5ExePath = new System.Windows.Forms.TextBox();
            this.chooseYourFighter = new System.Windows.Forms.OpenFileDialog();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            resources.ApplyResources(this.button1, "button1");
            this.button1.Name = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button_RunGame
            // 
            resources.ApplyResources(this.button_RunGame, "button_RunGame");
            this.button_RunGame.Name = "button_RunGame";
            this.button_RunGame.UseVisualStyleBackColor = true;
            this.button_RunGame.Click += new System.EventHandler(this.button2_Click);
            // 
            // pictureBox1
            // 
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
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
            // 
            // label_UDPPingback
            // 
            resources.ApplyResources(this.label_UDPPingback, "label_UDPPingback");
            this.label_UDPPingback.Name = "label_UDPPingback";
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
            // button3
            // 
            resources.ApplyResources(this.button3, "button3");
            this.button3.Name = "button3";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button_TurnOnVPN
            // 
            resources.ApplyResources(this.button_TurnOnVPN, "button_TurnOnVPN");
            this.button_TurnOnVPN.Name = "button_TurnOnVPN";
            this.button_TurnOnVPN.UseVisualStyleBackColor = true;
            this.button_TurnOnVPN.Click += new System.EventHandler(this.button_TurnOnVPN_Click);
            // 
            // textBox_Civ5ExePath
            // 
            this.textBox_Civ5ExePath.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::Civ5LanLauncher.Properties.Settings.Default, "Civ5ExePath", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            resources.ApplyResources(this.textBox_Civ5ExePath, "textBox_Civ5ExePath");
            this.textBox_Civ5ExePath.Name = "textBox_Civ5ExePath";
            this.textBox_Civ5ExePath.ReadOnly = true;
            this.textBox_Civ5ExePath.Text = global::Civ5LanLauncher.Properties.Settings.Default.Civ5ExePath;
            // 
            // chooseYourFighter
            // 
            this.chooseYourFighter.AddExtension = false;
            this.chooseYourFighter.FileName = global::Civ5LanLauncher.Properties.Settings.Default.Civ5ExePath;
            resources.ApplyResources(this.chooseYourFighter, "chooseYourFighter");
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            resources.ApplyResources(this.statusStrip1, "statusStrip1");
            this.statusStrip1.Name = "statusStrip1";
            // 
            // pictureBox2
            // 
            resources.ApplyResources(this.pictureBox2, "pictureBox2");
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.TabStop = false;
            this.pictureBox2.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox2_Paint);
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            resources.ApplyResources(this.toolStripStatusLabel1, "toolStripStatusLabel1");
            // 
            // Civ5LanLauncherUI
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.button_TurnOnVPN);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.textBox_Civ5ExePath);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label_LatencyValue);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label_IPAddr);
            this.Controls.Add(this.label_UDPPingback);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label_VPNActive);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.button_RunGame);
            this.Controls.Add(this.button1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Civ5LanLauncherUI";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog chooseYourFighter;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button_RunGame;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label_VPNActive;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label_UDPPingback;
        private System.Windows.Forms.Label label_IPAddr;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label_LatencyValue;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox textBox_Civ5ExePath;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button_TurnOnVPN;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
    }
}

