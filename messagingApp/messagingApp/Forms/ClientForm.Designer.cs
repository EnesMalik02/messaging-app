using System.Drawing;
using System.Windows.Forms;

namespace messagingApp.Forms
{
    partial class ClientForm
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
            this.lstConversations = new System.Windows.Forms.ListBox();
            this.lstMessages = new System.Windows.Forms.ListBox();
            this.txtOtherUserId = new System.Windows.Forms.TextBox();
            this.btnNewConversation = new System.Windows.Forms.Button();
            this.btnSend = new System.Windows.Forms.Button();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.selfName = new DevExpress.XtraEditors.LabelControl();
            this.selfID = new DevExpress.XtraEditors.LabelControl();
            this.simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
            this.btnLogout_Click = new System.Windows.Forms.Button();
            this.tileControlChats = new DevExpress.XtraEditors.TileControl();
            this.btnDeleteConversation = new DevExpress.XtraEditors.SimpleButton();
            this.SuspendLayout();
            // 
            // lstConversations
            // 
            this.lstConversations.FormattingEnabled = true;
            this.lstConversations.Location = new System.Drawing.Point(1059, 102);
            this.lstConversations.Name = "lstConversations";
            this.lstConversations.Size = new System.Drawing.Size(120, 381);
            this.lstConversations.TabIndex = 0;
            //this.lstConversations.SelectedIndexChanged += new System.EventHandler(this.lstConversations_SelectedIndexChanged);
            // 
            // lstMessages
            // 
            this.lstMessages.FormattingEnabled = true;
            this.lstMessages.Location = new System.Drawing.Point(183, 52);
            this.lstMessages.Name = "lstMessages";
            this.lstMessages.Size = new System.Drawing.Size(412, 355);
            this.lstMessages.TabIndex = 1;
            this.lstMessages.SelectedIndexChanged += new System.EventHandler(this.lstMessages_SelectedIndexChanged);
            // 
            // txtOtherUserId
            // 
            this.txtOtherUserId.Location = new System.Drawing.Point(12, 12);
            this.txtOtherUserId.Name = "txtOtherUserId";
            this.txtOtherUserId.Size = new System.Drawing.Size(121, 20);
            this.txtOtherUserId.TabIndex = 2;
            this.txtOtherUserId.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // btnNewConversation
            // 
            this.btnNewConversation.Location = new System.Drawing.Point(13, 38);
            this.btnNewConversation.Name = "btnNewConversation";
            this.btnNewConversation.Size = new System.Drawing.Size(120, 23);
            this.btnNewConversation.TabIndex = 3;
            this.btnNewConversation.Text = "Sohbet Başlat";
            this.btnNewConversation.UseVisualStyleBackColor = true;
            this.btnNewConversation.Click += new System.EventHandler(this.btnNewConversation_Click);
            // 
            // btnSend
            // 
            this.btnSend.Location = new System.Drawing.Point(512, 416);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(83, 23);
            this.btnSend.TabIndex = 4;
            this.btnSend.Text = "Mesaj Gönder";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // txtMessage
            // 
            this.txtMessage.Location = new System.Drawing.Point(183, 418);
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.Size = new System.Drawing.Size(323, 20);
            this.txtMessage.TabIndex = 5;
            // 
            // timer1
            // 
            this.timer1.Interval = 2000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // selfName
            // 
            this.selfName.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.selfName.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.selfName.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.selfName.LineLocation = DevExpress.XtraEditors.LineLocation.Center;
            this.selfName.LineOrientation = DevExpress.XtraEditors.LabelLineOrientation.Vertical;
            this.selfName.LineStyle = System.Drawing.Drawing2D.DashStyle.DashDotDot;
            this.selfName.Location = new System.Drawing.Point(1085, 45);
            this.selfName.Name = "selfName";
            this.selfName.Size = new System.Drawing.Size(94, 33);
            this.selfName.TabIndex = 21;
            this.selfName.Text = "labelControl1";
            this.selfName.Click += new System.EventHandler(this.selfName_Click);
            // 
            // selfID
            // 
            this.selfID.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.selfID.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.selfID.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.selfID.LineLocation = DevExpress.XtraEditors.LineLocation.Center;
            this.selfID.LineStyle = System.Drawing.Drawing2D.DashStyle.DashDotDot;
            this.selfID.Location = new System.Drawing.Point(987, 84);
            this.selfID.Name = "selfID";
            this.selfID.Size = new System.Drawing.Size(224, 33);
            this.selfID.TabIndex = 22;
            this.selfID.Text = "labelControl1";
            // 
            // simpleButton1
            // 
            this.simpleButton1.Location = new System.Drawing.Point(671, 91);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Size = new System.Drawing.Size(95, 30);
            this.simpleButton1.TabIndex = 23;
            this.simpleButton1.Text = "Kopyala ID";
            this.simpleButton1.Click += new System.EventHandler(this.simpleButton1_Click);
            // 
            // btnLogout_Click
            // 
            this.btnLogout_Click.Location = new System.Drawing.Point(671, 412);
            this.btnLogout_Click.Name = "btnLogout_Click";
            this.btnLogout_Click.Size = new System.Drawing.Size(122, 35);
            this.btnLogout_Click.TabIndex = 25;
            this.btnLogout_Click.Text = "ÇIKIŞ YAP";
            this.btnLogout_Click.UseVisualStyleBackColor = true;
            this.btnLogout_Click.Click += new System.EventHandler(this.btnLogout_Click_Click);
            // 
            // tileControlChats
            // 
            this.tileControlChats.Location = new System.Drawing.Point(12, 67);
            this.tileControlChats.Name = "tileControlChats";
            this.tileControlChats.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.tileControlChats.Size = new System.Drawing.Size(165, 435);
            this.tileControlChats.TabIndex = 26;
            this.tileControlChats.Text = "tileControl1";
            // 
            // btnDeleteConversation
            // 
            this.btnDeleteConversation.Location = new System.Drawing.Point(474, 22);
            this.btnDeleteConversation.Name = "btnDeleteConversation";
            this.btnDeleteConversation.Size = new System.Drawing.Size(121, 24);
            this.btnDeleteConversation.TabIndex = 27;
            this.btnDeleteConversation.Text = "simpleButton2";
            this.btnDeleteConversation.Click += new System.EventHandler(this.btnDeleteConversation_Click);
            // 
            // ClientForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1223, 514);
            this.Controls.Add(this.btnDeleteConversation);
            this.Controls.Add(this.tileControlChats);
            this.Controls.Add(this.btnLogout_Click);
            this.Controls.Add(this.simpleButton1);
            this.Controls.Add(this.selfID);
            this.Controls.Add(this.selfName);
            this.Controls.Add(this.txtMessage);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.btnNewConversation);
            this.Controls.Add(this.txtOtherUserId);
            this.Controls.Add(this.lstMessages);
            this.Controls.Add(this.lstConversations);
            this.Name = "ClientForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ClientForm";
            this.Load += new System.EventHandler(this.ClientForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lstConversations;
        private System.Windows.Forms.ListBox lstMessages;
        private System.Windows.Forms.TextBox txtOtherUserId;
        private System.Windows.Forms.Button btnNewConversation;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.Timer timer1;
        private DevExpress.XtraEditors.LabelControl selfName;
        private DevExpress.XtraEditors.LabelControl selfID;
        private DevExpress.XtraEditors.SimpleButton simpleButton1;
        private System.Windows.Forms.Button btnLogout_Click;
        private DevExpress.XtraEditors.TileControl tileControlChats;
        private DevExpress.XtraEditors.SimpleButton btnDeleteConversation;
    }
}