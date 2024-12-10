using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace messagingApp
{
    public partial class ClientForm : Form
    {
        private string currentUserEmail;
        private string currentUserId;

        // Kendi Firebase URL'nizi buraya yazın
        private string firebaseUrl = "https://messaging-app-11f5f-default-rtdb.europe-west1.firebasedatabase.app";

        public ClientForm(string email, string userId)
        {
            InitializeComponent();
            currentUserEmail = email;
            currentUserId = userId;
        }

        private void ClientForm_Load(object sender, EventArgs e)
        {
            // Form yüklendiğinde konuşmaları listele
            LoadConversations();

            // Timer ayarları
            timer1.Interval = 2000; // 2 saniyede bir yenile
            timer1.Tick += timer1_Tick; 
            timer1.Start();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }


        private void btnNewConversation_Click(object sender, EventArgs e)
        {
            string otherUserId = txtOtherUserId.Text.Trim();
            if (string.IsNullOrEmpty(otherUserId))
            {
                MessageBox.Show("Lütfen diğer kullanıcının ID'sini girin.");
                return;
            }

            string newConvId = Guid.NewGuid().ToString();

            var convData = new
            {
                participants = new string[] { currentUserId, otherUserId },
                lastMessage = "",
                lastUpdate = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            string convJson = JsonConvert.SerializeObject(convData);
            PutJson($"{firebaseUrl}/conversations/{newConvId}.json", convJson);

            // userConversations güncelle
            PutJson($"{firebaseUrl}/userConversations/{currentUserId}/{newConvId}.json", "true");
            PutJson($"{firebaseUrl}/userConversations/{otherUserId}/{newConvId}.json", "true");

            // Listeyi yenile
            LoadConversations();
        }

        private void lstConversations_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstConversations.SelectedItem == null) return;
            var selectedItem = (ListBoxItem)lstConversations.SelectedItem;
            string conversationId = selectedItem.Tag.ToString();
            LoadMessages(conversationId);
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (lstConversations.SelectedItem == null)
            {
                MessageBox.Show("Önce bir konuşma seçin.");
                return;
            }

            var selectedItem = (ListBoxItem)lstConversations.SelectedItem;
            string conversationId = selectedItem.Tag.ToString();
            string msg = txtMessage.Text.Trim();
            if (string.IsNullOrEmpty(msg)) return;

            // Mesajı ekle
            string url = $"{firebaseUrl}/messages/{conversationId}.json";
            var msgObj = new
            {
                sender = currentUserId,
                text = msg,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            string json = JsonConvert.SerializeObject(msgObj);
            PostJson(url, json);

            // conversation güncelle
            string convUrl = $"{firebaseUrl}/conversations/{conversationId}.json";
            var convUpdate = new
            {
                lastMessage = msg,
                lastUpdate = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            string convJson = JsonConvert.SerializeObject(convUpdate);
            PatchJson(convUrl, convJson);

            txtMessage.Clear();
            LoadMessages(conversationId);
            LoadConversations();
        }

        private void textBox1_TextChanged()
        {
            //TExt box
        }
        private void LoadConversations()
        {
            lstConversations.Items.Clear();
            string url = $"{firebaseUrl}/userConversations/{currentUserId}.json";
            string convsJson = GetJson(url);
            if (convsJson == null || convsJson == "null") return;

            var dict = JsonConvert.DeserializeObject<Dictionary<string, bool>>(convsJson);
            if (dict == null) return;

            foreach (var kvp in dict)
            {
                string conversationId = kvp.Key;
                string convUrl = $"{firebaseUrl}/conversations/{conversationId}.json";
                string convData = GetJson(convUrl);

                if (convData != null && convData != "null")
                {
                    dynamic convObj = JsonConvert.DeserializeObject<dynamic>(convData);
                    var participants = convObj.participants;
                    string otherUserId = null;

                    foreach (var p in participants)
                    {
                        string pid = p.ToString();
                        if (pid != currentUserId)
                        {
                            otherUserId = pid;
                            break;
                        }
                    }

                    string otherUserEmail = GetEmailByUserId(otherUserId);
                    string lastMessage = convObj.lastMessage != null ? (string)convObj.lastMessage : "";

                    string itemText = $"{otherUserEmail} - {lastMessage}";
                    lstConversations.Items.Add(new ListBoxItem { Text = itemText, Tag = conversationId });
                }
            }
        }

        private void LoadMessages(string conversationId)
        {
            lstMessages.Items.Clear();
            string url = $"{firebaseUrl}/messages/{conversationId}.json";
            string msgData = GetJson(url);
            if (msgData == null || msgData == "null") return;

            var msgs = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(msgData);
            if (msgs == null) return;

            foreach (var m in msgs)
            {
                string senderId = m.Value.sender;
                string text = m.Value.text;
                string prefix = senderId == currentUserId ? "Ben: " : "Onlar: ";
                lstMessages.Items.Add(prefix + text);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (lstConversations.SelectedItem != null)
            {
                var selectedItem = (ListBoxItem)lstConversations.SelectedItem;
                string conversationId = selectedItem.Tag.ToString();
                LoadMessages(conversationId); // Yeni mesaj gelmişse buradan görebilirsiniz.
            }
        }


        private string GetEmailByUserId(string userId)
        {
            string url = $"{firebaseUrl}/userIds/{userId}.json";
            string json = GetJson(url);
            if (json == null || json == "null") return "Bilinmeyen";

            dynamic obj = JsonConvert.DeserializeObject<dynamic>(json);
            string email = obj.email;
            return email;
        }

        private string GetJson(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "application/json";
            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    return sr.ReadToEnd();
                }
            }
            catch
            {
                return null;
            }
        }

        private void PutJson(string url, string json)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "PUT";
            request.ContentType = "application/json";
            using (var sw = new StreamWriter(request.GetRequestStream()))
            {
                sw.Write(json);
            }
            var response = (HttpWebResponse)request.GetResponse();
            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                string result = sr.ReadToEnd();
            }
        }

        private void PostJson(string url, string json)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json";
            using (var sw = new StreamWriter(request.GetRequestStream()))
            {
                sw.Write(json);
            }
            var response = (HttpWebResponse)request.GetResponse();
            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                string result = sr.ReadToEnd();
            }
        }

        private void PatchJson(string url, string json)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "PATCH";
            request.ContentType = "application/json";
            using (var sw = new StreamWriter(request.GetRequestStream()))
            {
                sw.Write(json);
            }
            var response = (HttpWebResponse)request.GetResponse();
            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                string result = sr.ReadToEnd();
            }
        }
    }


    // ListBox'a özel class
    public class ListBoxItem
    {
        public string Text { get; set; }
        public object Tag { get; set; }
        public override string ToString()
        {
            return Text;
        }
    }
}
