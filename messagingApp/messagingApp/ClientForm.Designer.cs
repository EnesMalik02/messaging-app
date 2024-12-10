namespace messagingApp
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
            this.SuspendLayout();
            // 
            // lstConversations
            // 
            this.lstConversations.FormattingEnabled = true;
            this.lstConversations.Location = new System.Drawing.Point(13, 66);
            this.lstConversations.Name = "lstConversations";
            this.lstConversations.Size = new System.Drawing.Size(120, 381);
            this.lstConversations.TabIndex = 0;
            this.lstConversations.SelectedIndexChanged += new System.EventHandler(this.lstConversations_SelectedIndexChanged);
            // 
            // lstMessages
            // 
            this.lstMessages.FormattingEnabled = true;
            this.lstMessages.Location = new System.Drawing.Point(147, 13);
            this.lstMessages.Name = "lstMessages";
            this.lstMessages.Size = new System.Drawing.Size(641, 394);
            this.lstMessages.TabIndex = 1;
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
            this.btnSend.Location = new System.Drawing.Point(701, 418);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(87, 23);
            this.btnSend.TabIndex = 4;
            this.btnSend.Text = "Mesaj Gönder";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // txtMessage
            // 
            this.txtMessage.Location = new System.Drawing.Point(147, 418);
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.Size = new System.Drawing.Size(548, 20);
            this.txtMessage.TabIndex = 5;
            // 
            // timer1
            // 
            this.timer1.Interval = 2000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // ClientForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.txtMessage);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.btnNewConversation);
            this.Controls.Add(this.txtOtherUserId);
            this.Controls.Add(this.lstMessages);
            this.Controls.Add(this.lstConversations);
            this.Name = "ClientForm";
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
    }
}