using System;
using System.Windows.Forms;

namespace main
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.splitContainerControl1 = new DevExpress.XtraEditors.SplitContainerControl();
            this.btnNewConversation = new DevExpress.XtraEditors.SimpleButton();
            this.txtOtherUserId = new System.Windows.Forms.TextBox();
            this.tileControlChats = new DevExpress.XtraEditors.TileControl();
            this.chatSettings = new DevExpress.XtraEditors.SimpleButton();
            this.panelControl2 = new DevExpress.XtraEditors.PanelControl();
            this.simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
            this.selfID = new System.Windows.Forms.Label();
            this.selfName = new System.Windows.Forms.Label();
            this.btnLogout_Click = new DevExpress.XtraEditors.SimpleButton();
            this.btnSend = new DevExpress.XtraEditors.SimpleButton();
            this.txtMessage = new DevExpress.XtraEditors.TextEdit();
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            this.lstMessages = new System.Windows.Forms.ListBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.behaviorManager1 = new DevExpress.Utils.Behaviors.BehaviorManager(this.components);
            this.tileGroup1 = new DevExpress.XtraEditors.TileGroup();
            this.updateBtn = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1)).BeginInit();
            this.splitContainerControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl2)).BeginInit();
            this.panelControl2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtMessage.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.behaviorManager1)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainerControl1
            // 
            this.splitContainerControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerControl1.Location = new System.Drawing.Point(0, 0);
            this.splitContainerControl1.Name = "splitContainerControl1";
            this.splitContainerControl1.Panel1.Controls.Add(this.btnNewConversation);
            this.splitContainerControl1.Panel1.Controls.Add(this.txtOtherUserId);
            this.splitContainerControl1.Panel1.Controls.Add(this.tileControlChats);
            this.splitContainerControl1.Panel1.Text = "Panel1";
            this.splitContainerControl1.Panel2.Controls.Add(this.updateBtn);
            this.splitContainerControl1.Panel2.Controls.Add(this.chatSettings);
            this.splitContainerControl1.Panel2.Controls.Add(this.panelControl2);
            this.splitContainerControl1.Panel2.Controls.Add(this.btnLogout_Click);
            this.splitContainerControl1.Panel2.Controls.Add(this.btnSend);
            this.splitContainerControl1.Panel2.Controls.Add(this.txtMessage);
            this.splitContainerControl1.Panel2.Controls.Add(this.panelControl1);
            this.splitContainerControl1.Panel2.Text = "Panel2";
            this.splitContainerControl1.Size = new System.Drawing.Size(1089, 625);
            this.splitContainerControl1.SplitterPosition = 180;
            this.splitContainerControl1.TabIndex = 0;
            this.splitContainerControl1.Text = "splitContainerControl1";
            // 
            // btnNewConversation
            // 
            this.btnNewConversation.Location = new System.Drawing.Point(3, 41);
            this.btnNewConversation.Name = "btnNewConversation";
            this.btnNewConversation.Size = new System.Drawing.Size(149, 23);
            this.btnNewConversation.TabIndex = 2;
            this.btnNewConversation.Text = "Sohbet Başlat";
            this.btnNewConversation.Click += new System.EventHandler(this.btnNewConversation_Click);
            // 
            // txtOtherUserId
            // 
            this.txtOtherUserId.Location = new System.Drawing.Point(3, 12);
            this.txtOtherUserId.Name = "txtOtherUserId";
            this.txtOtherUserId.Size = new System.Drawing.Size(149, 21);
            this.txtOtherUserId.TabIndex = 1;
            // 
            // tileControlChats
            // 
            this.tileControlChats.AccessibleRole = System.Windows.Forms.AccessibleRole.PageTabList;
            this.tileControlChats.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Flat;
            this.tileControlChats.CausesValidation = false;
            this.tileControlChats.HorizontalContentAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.tileControlChats.IndentBetweenItems = 5;
            this.tileControlChats.ItemBorderVisibility = DevExpress.XtraEditors.TileItemBorderVisibility.Always;
            this.tileControlChats.Location = new System.Drawing.Point(3, 70);
            this.tileControlChats.MaxId = 23;
            this.tileControlChats.Name = "tileControlChats";
            this.tileControlChats.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.tileControlChats.RowCount = 1;
            this.tileControlChats.SelectionColor = System.Drawing.Color.Red;
            this.tileControlChats.ShowGroupText = true;
            this.tileControlChats.Size = new System.Drawing.Size(163, 543);
            this.tileControlChats.TabIndex = 0;
            this.tileControlChats.Text = "tileControl1";
            this.tileControlChats.Click += new System.EventHandler(this.tileControlChats_Click);
            // 
            // chatSettings
            // 
            this.chatSettings.Location = new System.Drawing.Point(1, 12);
            this.chatSettings.Name = "chatSettings";
            this.chatSettings.Size = new System.Drawing.Size(614, 47);
            this.chatSettings.TabIndex = 8;
            this.chatSettings.Text = "Sohbet Ayarları";
            this.chatSettings.Click += new System.EventHandler(this.chatSettings_Click);
            // 
            // panelControl2
            // 
            this.panelControl2.Controls.Add(this.simpleButton1);
            this.panelControl2.Controls.Add(this.selfID);
            this.panelControl2.Controls.Add(this.selfName);
            this.panelControl2.Location = new System.Drawing.Point(621, 41);
            this.panelControl2.Name = "panelControl2";
            this.panelControl2.Size = new System.Drawing.Size(280, 107);
            this.panelControl2.TabIndex = 7;
            // 
            // simpleButton1
            // 
            this.simpleButton1.Location = new System.Drawing.Point(93, 77);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Size = new System.Drawing.Size(107, 25);
            this.simpleButton1.TabIndex = 6;
            this.simpleButton1.Text = "ID Kopyala";
            this.simpleButton1.Click += new System.EventHandler(this.simpleButton1_Click_1);
            // 
            // selfID
            // 
            this.selfID.AutoSize = true;
            this.selfID.Location = new System.Drawing.Point(35, 34);
            this.selfID.Name = "selfID";
            this.selfID.Size = new System.Drawing.Size(226, 13);
            this.selfID.TabIndex = 3;
            this.selfID.Text = "ID : 1757ac79-d72c-4afd-a5fa-fad66bef1289";
            this.selfID.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.selfID.Click += new System.EventHandler(this.selfID_Click);
            // 
            // selfName
            // 
            this.selfName.AutoSize = true;
            this.selfName.Location = new System.Drawing.Point(115, 8);
            this.selfName.Name = "selfName";
            this.selfName.Size = new System.Drawing.Size(51, 13);
            this.selfName.TabIndex = 4;
            this.selfName.Text = "selfName";
            this.selfName.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.selfName.Click += new System.EventHandler(this.selfName_Click_1);
            // 
            // btnLogout_Click
            // 
            this.btnLogout_Click.Location = new System.Drawing.Point(817, 593);
            this.btnLogout_Click.Name = "btnLogout_Click";
            this.btnLogout_Click.Size = new System.Drawing.Size(75, 23);
            this.btnLogout_Click.TabIndex = 5;
            this.btnLogout_Click.Text = "Çıkış Yap";
            this.btnLogout_Click.Click += new System.EventHandler(this.btnLogout_Click_Click);
            // 
            // btnSend
            // 
            this.btnSend.Location = new System.Drawing.Point(510, 593);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(105, 23);
            this.btnSend.TabIndex = 2;
            this.btnSend.Text = "Gönder";
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // txtMessage
            // 
            this.txtMessage.Location = new System.Drawing.Point(1, 595);
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.Size = new System.Drawing.Size(503, 20);
            this.txtMessage.TabIndex = 1;
            // 
            // panelControl1
            // 
            this.panelControl1.Controls.Add(this.lstMessages);
            this.panelControl1.Location = new System.Drawing.Point(1, 70);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Size = new System.Drawing.Size(614, 511);
            this.panelControl1.TabIndex = 0;
            this.panelControl1.Paint += new System.Windows.Forms.PaintEventHandler(this.panelControl1_Paint);
            // 
            // lstMessages
            // 
            this.lstMessages.FormattingEnabled = true;
            this.lstMessages.Location = new System.Drawing.Point(0, 0);
            this.lstMessages.Name = "lstMessages";
            this.lstMessages.Size = new System.Drawing.Size(614, 511);
            this.lstMessages.TabIndex = 0;
            // 
            // timer1
            // 
            this.timer1.Interval = 2000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // tileGroup1
            // 
            this.tileGroup1.Name = "tileGroup1";
            this.tileGroup1.Text = "Sohbetler";
            // 
            // updateBtn
            // 
            this.updateBtn.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.updateBtn.ImageOptions.Image = ((System.Drawing.Image)(resources.GetObject("simpleButton2.ImageOptions.Image")));
            this.updateBtn.Location = new System.Drawing.Point(860, 3);
            this.updateBtn.Name = "updateBtn";
            this.updateBtn.Size = new System.Drawing.Size(32, 30);
            this.updateBtn.TabIndex = 9;
            this.updateBtn.Click += new System.EventHandler(this.updateBtn_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1089, 625);
            this.Controls.Add(this.splitContainerControl1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1)).EndInit();
            this.splitContainerControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.panelControl2)).EndInit();
            this.panelControl2.ResumeLayout(false);
            this.panelControl2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtMessage.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.behaviorManager1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.SplitContainerControl splitContainerControl1;
        private DevExpress.XtraEditors.SimpleButton btnSend;
        private DevExpress.XtraEditors.TextEdit txtMessage;
        private DevExpress.XtraEditors.PanelControl panelControl1;
        private DevExpress.XtraEditors.SimpleButton btnNewConversation;
        private System.Windows.Forms.TextBox txtOtherUserId;
        private System.Windows.Forms.Timer timer1;
        private ListBox lstMessages;
        private DevExpress.XtraEditors.SimpleButton btnLogout_Click;
        private DevExpress.XtraEditors.TileControl tileControlChats;
        private DevExpress.Utils.Behaviors.BehaviorManager behaviorManager1;
        private DevExpress.XtraEditors.TileGroup tileGroup1;
        private DevExpress.XtraEditors.PanelControl panelControl2;
        private DevExpress.XtraEditors.SimpleButton simpleButton1;
        private Label selfID;
        private Label selfName;
        private DevExpress.XtraEditors.SimpleButton chatSettings;
        private DevExpress.XtraEditors.SimpleButton updateBtn;
    }
}

