﻿namespace Hexpoint.Blox
{
    partial class Launcher
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Launcher));
            this.ddlServerIp = new System.Windows.Forms.ComboBox();
            this.cbVSync = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtUserName = new System.Windows.Forms.TextBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.txtSeed = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cbMipmapping = new System.Windows.Forms.CheckBox();
            this.gbVideo = new System.Windows.Forms.GroupBox();
            this.cbSmoothLighting = new System.Windows.Forms.CheckBox();
            this.cbWindowed = new System.Windows.Forms.CheckBox();
            this.cbLinearMagnificationFilter = new System.Windows.Forms.CheckBox();
            this.cbFog = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.ddlViewDistance = new System.Windows.Forms.ComboBox();
            this.cbSoundEnabled = new System.Windows.Forms.CheckBox();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.lblPort = new System.Windows.Forms.Label();
            this.pbProgress = new System.Windows.Forms.ProgressBar();
            this.txtProgress = new System.Windows.Forms.TextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuExit = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuVisitWebSite = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.btnUpgradeAvailable = new System.Windows.Forms.Button();
            this.cbMusic = new System.Windows.Forms.CheckBox();
            this.gbVideo.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ddlServerIp
            // 
            this.ddlServerIp.BackColor = System.Drawing.SystemColors.Control;
            this.ddlServerIp.FormattingEnabled = true;
            this.ddlServerIp.Location = new System.Drawing.Point(274, 38);
            this.ddlServerIp.MaxLength = 70;
            this.ddlServerIp.Name = "ddlServerIp";
            this.ddlServerIp.Size = new System.Drawing.Size(190, 21);
            this.ddlServerIp.TabIndex = 3;
            // 
            // cbVSync
            // 
            this.cbVSync.AutoSize = true;
            this.cbVSync.Checked = true;
            this.cbVSync.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbVSync.Location = new System.Drawing.Point(8, 17);
            this.cbVSync.Name = "cbVSync";
            this.cbVSync.Size = new System.Drawing.Size(88, 17);
            this.cbVSync.TabIndex = 50;
            this.cbVSync.Text = "Vertical Sync";
            this.cbVSync.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Location = new System.Drawing.Point(19, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "UserName";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtUserName
            // 
            this.txtUserName.BackColor = System.Drawing.SystemColors.Control;
            this.txtUserName.Location = new System.Drawing.Point(82, 38);
            this.txtUserName.MaxLength = 16;
            this.txtUserName.Name = "txtUserName";
            this.txtUserName.Size = new System.Drawing.Size(177, 20);
            this.txtUserName.TabIndex = 6;
            // 
            // btnStart
            // 
            this.btnStart.BackColor = System.Drawing.Color.Transparent;
            this.btnStart.FlatAppearance.BorderColor = System.Drawing.Color.DimGray;
            this.btnStart.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkGray;
            this.btnStart.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightGray;
            this.btnStart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStart.ForeColor = System.Drawing.Color.DarkGreen;
            this.btnStart.Location = new System.Drawing.Point(352, 219);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(188, 35);
            this.btnStart.TabIndex = 15;
            this.btnStart.Text = "Start Game";
            this.btnStart.UseVisualStyleBackColor = false;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.BackColor = System.Drawing.Color.Transparent;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(144, 76);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(44, 13);
            this.label7.TabIndex = 25;
            this.label7.Text = "optional";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtSeed
            // 
            this.txtSeed.BackColor = System.Drawing.SystemColors.Control;
            this.txtSeed.Location = new System.Drawing.Point(50, 73);
            this.txtSeed.MaxLength = 12;
            this.txtSeed.Name = "txtSeed";
            this.txtSeed.Size = new System.Drawing.Size(88, 20);
            this.txtSeed.TabIndex = 24;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.Transparent;
            this.label5.Location = new System.Drawing.Point(12, 76);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(32, 13);
            this.label5.TabIndex = 23;
            this.label5.Text = "Seed";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(141, 49);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(27, 13);
            this.label3.TabIndex = 24;
            this.label3.Text = "Size";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(13, 49);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(31, 13);
            this.label6.TabIndex = 22;
            this.label6.Text = "Type";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 21;
            this.label1.Text = "Name";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cbMipmapping
            // 
            this.cbMipmapping.AutoSize = true;
            this.cbMipmapping.Checked = true;
            this.cbMipmapping.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbMipmapping.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbMipmapping.Location = new System.Drawing.Point(102, 17);
            this.cbMipmapping.Name = "cbMipmapping";
            this.cbMipmapping.Size = new System.Drawing.Size(83, 17);
            this.cbMipmapping.TabIndex = 51;
            this.cbMipmapping.Text = "Mipmapping";
            this.cbMipmapping.UseVisualStyleBackColor = true;
            // 
            // gbVideo
            // 
            this.gbVideo.BackColor = System.Drawing.Color.Transparent;
            this.gbVideo.Controls.Add(this.cbSmoothLighting);
            this.gbVideo.Controls.Add(this.cbWindowed);
            this.gbVideo.Controls.Add(this.cbLinearMagnificationFilter);
            this.gbVideo.Controls.Add(this.cbFog);
            this.gbVideo.Controls.Add(this.label4);
            this.gbVideo.Controls.Add(this.ddlViewDistance);
            this.gbVideo.Controls.Add(this.cbMipmapping);
            this.gbVideo.Controls.Add(this.cbVSync);
            this.gbVideo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.gbVideo.Location = new System.Drawing.Point(265, 92);
            this.gbVideo.Name = "gbVideo";
            this.gbVideo.Size = new System.Drawing.Size(275, 121);
            this.gbVideo.TabIndex = 22;
            this.gbVideo.TabStop = false;
            this.gbVideo.Text = "Video Settings";
            // 
            // cbSmoothLighting
            // 
            this.cbSmoothLighting.AutoSize = true;
            this.cbSmoothLighting.Checked = true;
            this.cbSmoothLighting.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbSmoothLighting.Location = new System.Drawing.Point(102, 63);
            this.cbSmoothLighting.Name = "cbSmoothLighting";
            this.cbSmoothLighting.Size = new System.Drawing.Size(102, 17);
            this.cbSmoothLighting.TabIndex = 56;
            this.cbSmoothLighting.Text = "Smooth Lighting";
            this.cbSmoothLighting.UseVisualStyleBackColor = true;
            // 
            // cbWindowed
            // 
            this.cbWindowed.AutoSize = true;
            this.cbWindowed.Checked = true;
            this.cbWindowed.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbWindowed.Location = new System.Drawing.Point(8, 63);
            this.cbWindowed.Name = "cbWindowed";
            this.cbWindowed.Size = new System.Drawing.Size(77, 17);
            this.cbWindowed.TabIndex = 55;
            this.cbWindowed.Text = "Windowed";
            this.cbWindowed.UseVisualStyleBackColor = true;
            // 
            // cbLinearMagnificationFilter
            // 
            this.cbLinearMagnificationFilter.AutoSize = true;
            this.cbLinearMagnificationFilter.Checked = true;
            this.cbLinearMagnificationFilter.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbLinearMagnificationFilter.Location = new System.Drawing.Point(102, 40);
            this.cbLinearMagnificationFilter.Name = "cbLinearMagnificationFilter";
            this.cbLinearMagnificationFilter.Size = new System.Drawing.Size(131, 17);
            this.cbLinearMagnificationFilter.TabIndex = 54;
            this.cbLinearMagnificationFilter.Text = "Linear Mag Filter (Blur)";
            this.cbLinearMagnificationFilter.UseVisualStyleBackColor = true;
            // 
            // cbFog
            // 
            this.cbFog.AutoSize = true;
            this.cbFog.Checked = true;
            this.cbFog.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbFog.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbFog.Location = new System.Drawing.Point(8, 40);
            this.cbFog.Name = "cbFog";
            this.cbFog.Size = new System.Drawing.Size(66, 17);
            this.cbFog.TabIndex = 53;
            this.cbFog.Text = "Use Fog";
            this.cbFog.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 89);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(75, 13);
            this.label4.TabIndex = 27;
            this.label4.Text = "View Distance";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ddlViewDistance
            // 
            this.ddlViewDistance.BackColor = System.Drawing.SystemColors.Control;
            this.ddlViewDistance.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddlViewDistance.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ddlViewDistance.FormattingEnabled = true;
            this.ddlViewDistance.Location = new System.Drawing.Point(87, 86);
            this.ddlViewDistance.MaxLength = 24;
            this.ddlViewDistance.Name = "ddlViewDistance";
            this.ddlViewDistance.Size = new System.Drawing.Size(70, 21);
            this.ddlViewDistance.TabIndex = 26;
            // 
            // cbSoundEnabled
            // 
            this.cbSoundEnabled.AutoSize = true;
            this.cbSoundEnabled.BackColor = System.Drawing.Color.Transparent;
            this.cbSoundEnabled.Checked = true;
            this.cbSoundEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbSoundEnabled.Location = new System.Drawing.Point(273, 69);
            this.cbSoundEnabled.Name = "cbSoundEnabled";
            this.cbSoundEnabled.Size = new System.Drawing.Size(99, 17);
            this.cbSoundEnabled.TabIndex = 24;
            this.cbSoundEnabled.Text = "Sound Enabled";
            this.cbSoundEnabled.UseVisualStyleBackColor = false;
            // 
            // txtPort
            // 
            this.txtPort.BackColor = System.Drawing.SystemColors.Control;
            this.txtPort.Location = new System.Drawing.Point(496, 38);
            this.txtPort.MaxLength = 5;
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(44, 20);
            this.txtPort.TabIndex = 58;
            // 
            // lblPort
            // 
            this.lblPort.AutoSize = true;
            this.lblPort.BackColor = System.Drawing.Color.Transparent;
            this.lblPort.Location = new System.Drawing.Point(470, 41);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(26, 13);
            this.lblPort.TabIndex = 57;
            this.lblPort.Text = "Port";
            this.lblPort.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pbProgress
            // 
            this.pbProgress.Location = new System.Drawing.Point(12, 219);
            this.pbProgress.Name = "pbProgress";
            this.pbProgress.Size = new System.Drawing.Size(334, 12);
            this.pbProgress.TabIndex = 59;
            this.pbProgress.Visible = false;
            // 
            // txtProgress
            // 
            this.txtProgress.Location = new System.Drawing.Point(12, 233);
            this.txtProgress.Name = "txtProgress";
            this.txtProgress.Size = new System.Drawing.Size(334, 20);
            this.txtProgress.TabIndex = 60;
            this.txtProgress.Visible = false;
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.Color.Transparent;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(552, 24);
            this.menuStrip1.TabIndex = 61;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem2,
            this.mnuExit});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(145, 6);
            // 
            // mnuExit
            // 
            this.mnuExit.Name = "mnuExit";
            this.mnuExit.Size = new System.Drawing.Size(148, 22);
            this.mnuExit.Text = "Exit";
            this.mnuExit.Click += new System.EventHandler(this.mnuExit_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.mnuVisitWebSite,
            this.mnuAbout});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(168, 6);
            // 
            // mnuVisitWebSite
            // 
            this.mnuVisitWebSite.Name = "mnuVisitWebSite";
            this.mnuVisitWebSite.Size = new System.Drawing.Size(171, 22);
            this.mnuVisitWebSite.Text = "Visit Web Site";
            this.mnuVisitWebSite.Click += new System.EventHandler(this.mnuVisitWebSite_Click);
            // 
            // mnuAbout
            // 
            this.mnuAbout.Name = "mnuAbout";
            this.mnuAbout.Size = new System.Drawing.Size(171, 22);
            this.mnuAbout.Text = "About";
            this.mnuAbout.Click += new System.EventHandler(this.mnuAbout_Click);
            // 
            // btnUpgradeAvailable
            // 
            this.btnUpgradeAvailable.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnUpgradeAvailable.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUpgradeAvailable.Font = new System.Drawing.Font("Arial", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnUpgradeAvailable.Location = new System.Drawing.Point(432, 4);
            this.btnUpgradeAvailable.Name = "btnUpgradeAvailable";
            this.btnUpgradeAvailable.Size = new System.Drawing.Size(108, 21);
            this.btnUpgradeAvailable.TabIndex = 62;
            this.btnUpgradeAvailable.Text = "Upgrade Available";
            this.btnUpgradeAvailable.UseVisualStyleBackColor = false;
            this.btnUpgradeAvailable.Visible = false;
            // 
            // cbMusic
            // 
            this.cbMusic.AutoSize = true;
            this.cbMusic.BackColor = System.Drawing.Color.Transparent;
            this.cbMusic.Checked = true;
            this.cbMusic.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbMusic.Location = new System.Drawing.Point(378, 69);
            this.cbMusic.Name = "cbMusic";
            this.cbMusic.Size = new System.Drawing.Size(54, 17);
            this.cbMusic.TabIndex = 63;
            this.cbMusic.Text = "Music";
            this.cbMusic.UseVisualStyleBackColor = false;
            // 
            // Launcher
            // 
            this.AcceptButton = this.btnStart;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(210)))), ((int)(((byte)(192)))));
            this.BackgroundImage = global::Hexpoint.Blox.Properties.Resources.LauncherBackground2;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(552, 260);
            this.Controls.Add(this.cbMusic);
            this.Controls.Add(this.btnUpgradeAvailable);
            this.Controls.Add(this.txtProgress);
            this.Controls.Add(this.pbProgress);
            this.Controls.Add(this.txtPort);
            this.Controls.Add(this.lblPort);
            this.Controls.Add(this.cbSoundEnabled);
            this.Controls.Add(this.gbVideo);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.txtUserName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ddlServerIp);
            this.Controls.Add(this.menuStrip1);
            this.DoubleBuffered = true;
            this.ForeColor = System.Drawing.SystemColors.WindowText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.Name = "Launcher";
            this.Load += new System.EventHandler(this.Launcher_Load);
            this.gbVideo.ResumeLayout(false);
            this.gbVideo.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox ddlServerIp;
        private System.Windows.Forms.CheckBox cbVSync;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtUserName;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox cbMipmapping;
        private System.Windows.Forms.GroupBox gbVideo;
        private System.Windows.Forms.TextBox txtSeed;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox ddlViewDistance;
        private System.Windows.Forms.CheckBox cbFog;
        private System.Windows.Forms.CheckBox cbLinearMagnificationFilter;
        private System.Windows.Forms.CheckBox cbWindowed;
        private System.Windows.Forms.CheckBox cbSoundEnabled;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.ProgressBar pbProgress;
        private System.Windows.Forms.TextBox txtProgress;
        private System.Windows.Forms.CheckBox cbSmoothLighting;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mnuExit;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem mnuVisitWebSite;
        private System.Windows.Forms.ToolStripMenuItem mnuAbout;
        private System.Windows.Forms.Button btnUpgradeAvailable;
        private System.Windows.Forms.CheckBox cbMusic;
    }
}