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
        private long lastCheckedUpdate = 0; // Unix zaman damgası olarak saklanır


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

        private async void StartListeningToMessages(string conversationId)
        {
            string url = $"{firebaseUrl}/messages/{conversationId}.json";
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "application/json";

            try
            {
                var response = await request.GetResponseAsync();
                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    while (!sr.EndOfStream)
                    {
                        string line = await sr.ReadLineAsync();
                        // Yeni mesajı işle
                        if (!string.IsNullOrEmpty(line))
                        {
                            UpdateUIWithNewMessage(line);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Streaming hatası: {ex.Message}");
            }
        }

        private void UpdateUIWithNewMessage(string messageJson)
        {
            // Gelen JSON'u mesaj objesine dönüştür
            dynamic messageObj = JsonConvert.DeserializeObject<dynamic>(messageJson);

            // Mesajın göndericisi ve metni
            string senderId = messageObj.sender;
            string text = messageObj.text;

            // Hangi konuşmaya ait olduğunu kontrol edin
            string conversationId = ""; // Bu bilgiyi mesaj context'inden çekebilirsiniz (Firebase endpoint'e bağlı)

            // Eğer mesaj, seçili konuşmaya aitse ekranda göster
            if (lstConversations.SelectedItem != null)
            {
                var selectedItem = (ListBoxItem)lstConversations.SelectedItem;
                if (selectedItem.Tag.ToString() == conversationId)
                {
                    string prefix = senderId == currentUserId ? "Ben: " : "Onlar: ";
                    lstMessages.Items.Add(prefix + text);
                }
            }

            // Eğer mesaj yeni bir konuşma ise, konuşma listesine ekleyin
            LoadConversations();
        }


        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
        private string GetUserIdByEmail(string email)
        {
            string firebaseKey = email.Replace(".", ",");
            string url = $"{firebaseUrl}/users/{firebaseKey}.json";
            string userData = GetJson(url);

            if (string.IsNullOrEmpty(userData) || userData == "null")
            {
                MessageBox.Show("Bu e-posta ile kayıtlı bir kullanıcı bulunamadı.");
                return null;
            }

            dynamic user = JsonConvert.DeserializeObject<dynamic>(userData);
            return user.id;
        }


        private void btnNewConversation_Click(object sender, EventArgs e)
        {
            string otherUserEmail = txtOtherUserId.Text.Trim();
            if (string.IsNullOrEmpty(otherUserEmail))
            {
                MessageBox.Show("Lütfen diğer kullanıcının e-posta adresini girin.");
                return;
            }

            // E-posta adresinden kullanıcı ID'sini al
            string otherUserId = GetUserIdByEmail(otherUserEmail);
            if (string.IsNullOrEmpty(otherUserId))
            {
                MessageBox.Show("Bu e-posta adresine sahip bir kullanıcı bulunamadı.");
                return;
            }

            // Mevcut konuşmayı kontrol et
            string existingConversationId = GetExistingConversationId(currentUserId, otherUserId);
            if (!string.IsNullOrEmpty(existingConversationId))
            {
                MessageBox.Show("Bu kişiyle zaten bir konuşmanız var.");
                return;
            }

            // Yeni bir konuşma oluştur
            string newConvId = Guid.NewGuid().ToString();
            var convData = new
            {
                participants = new string[] { currentUserId, otherUserId },
                lastMessage = "",
                lastUpdate = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            // Konuşmayı Firebase'e ekle
            string convJson = JsonConvert.SerializeObject(convData);
            PutJson($"{firebaseUrl}/conversations/{newConvId}.json", convJson);

            // Gönderen ve alıcı için userConversations güncelle
            AddConversationToUser(currentUserId, newConvId);
            AddConversationToUser(otherUserId, newConvId);

            MessageBox.Show("Sohbet başarıyla başlatıldı.");
            LoadConversations();
        }


        private void AddConversationToUser(string userId, string conversationId)
        {
            string url = $"{firebaseUrl}/userConversations/{userId}/{conversationId}.json";
            PutJson(url, "true");
        }




        private string GetExistingConversationId(string userId1, string userId2)
        {
            string url = $"{firebaseUrl}/userConversations/{userId1}.json";
            string userConversationsJson = GetJson(url);
            if (string.IsNullOrEmpty(userConversationsJson) || userConversationsJson == "null")
                return null;

            var conversations = JsonConvert.DeserializeObject<Dictionary<string, bool>>(userConversationsJson);
            if (conversations == null) return null;

            foreach (var conversation in conversations)
            {
                string conversationId = conversation.Key;

                // Konuşmanın katılımcılarını al
                string convUrl = $"{firebaseUrl}/conversations/{conversationId}.json";
                string convData = GetJson(convUrl);
                if (string.IsNullOrEmpty(convData) || convData == "null")
                    continue;

                dynamic convObj = JsonConvert.DeserializeObject<dynamic>(convData);
                var participants = convObj.participants;

                if (participants != null &&
                    (participants[0].ToString() == userId1 && participants[1].ToString() == userId2 ||
                     participants[0].ToString() == userId2 && participants[1].ToString() == userId1))
                {
                    return conversationId;
                }
            }

            return null;
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

            // Mevcut konuşmayı al
            var selectedItem = (ListBoxItem)lstConversations.SelectedItem;
            string conversationId = selectedItem.Tag.ToString();
            string msg = txtMessage.Text.Trim();

            // Eğer mesaj boşsa işlem yapma
            if (string.IsNullOrEmpty(msg))
            {
                MessageBox.Show("Lütfen bir mesaj yazın.");
                return;
            }

            // Mesajı Firebase'e ekle
            string url = $"{firebaseUrl}/messages/{conversationId}.json";
            var msgObj = new
            {
                sender = currentUserId,
                text = msg,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            string json = JsonConvert.SerializeObject(msgObj);
            PostJson(url, json);

            // Konuşmayı Firebase'de güncelle (lastMessage ve lastUpdate)
            string convUrl = $"{firebaseUrl}/conversations/{conversationId}.json";
            var convUpdate = new
            {
                lastMessage = msg,
                lastUpdate = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            string convJson = JsonConvert.SerializeObject(convUpdate);
            PatchJson(convUrl, convJson);

            // Alıcı tarafın userConversations düğümünü güncelle
            UpdateRecipientConversation(conversationId);

            // Mesaj kutusunu temizle
            txtMessage.Clear();

            // Konuşmaları ve mesajları güncelle
            LoadConversations();  // Sohbet listesini yeniler
            LoadMessages(conversationId);  // Mesajları yeniler
        }


        private void RefreshConversationAndMessages(string conversationId)
        {
            // Konuşma listesini güncelle
            LoadConversations();

            // Sohbetin seçili durumda kalmasını sağla
            foreach (ListBoxItem item in lstConversations.Items)
            {
                if (item.Tag.ToString() == conversationId)
                {
                    lstConversations.SelectedItem = item;
                    break;
                }
            }

            // Mesajları güncelle
            LoadMessages(conversationId);
        }


        private void UpdateRecipientConversation(string conversationId)
        {
            // Konuşmadaki diğer kullanıcıyı bul
            string otherUserId = GetOtherUserId(conversationId);
            if (string.IsNullOrEmpty(otherUserId)) return;

            // Diğer kullanıcının userConversations altına bu konuşmayı ekle
            string url = $"{firebaseUrl}/userConversations/{otherUserId}/{conversationId}.json";
            PutJson(url, "true");
        }

        private string GetOtherUserId(string conversationId)
        {
            // Konuşmanın katılımcılarını Firebase'den al
            string url = $"{firebaseUrl}/conversations/{conversationId}.json";
            string convData = GetJson(url);
            if (string.IsNullOrEmpty(convData) || convData == "null") return null;

            dynamic convObj = JsonConvert.DeserializeObject<dynamic>(convData);
            var participants = convObj.participants;

            foreach (var participant in participants)
            {
                string participantId = participant.ToString();
                if (participantId != currentUserId)
                {
                    return participantId;
                }
            }

            return null;
        }
    



        private void textBox1_TextChanged()
        {
            //TExt box
        }

        private void LoadConversations()
        {
            // Firebase'den kullanıcının konuşma listesini al
            string url = $"{firebaseUrl}/userConversations/{currentUserId}.json";
            string convsJson = GetJson(url);
            if (convsJson == null || convsJson == "null") return;

            var conversations = JsonConvert.DeserializeObject<Dictionary<string, bool>>(convsJson);
            if (conversations == null) return;

            bool isUpdated = false; // UI'yi yalnızca gerektiğinde güncellemek için bir kontrol

            foreach (var kvp in conversations)
            {
                string conversationId = kvp.Key;
                string convUrl = $"{firebaseUrl}/conversations/{conversationId}.json";
                string convData = GetJson(convUrl);

                if (convData == null || convData == "null") continue;

                dynamic convObj = JsonConvert.DeserializeObject<dynamic>(convData);

                // Konuşmanın son güncelleme zamanını al
                long lastUpdate = convObj.lastUpdate != null ? (long)convObj.lastUpdate : 0;

                // Eğer konuşma zaten güncelse atla
                if (lastUpdate <= lastCheckedUpdate) continue;

                // Eğer yeni bir güncelleme varsa listeyi yenilemek için işaretle
                isUpdated = true;

                // Sohbetteki diğer kullanıcıyı bul
                var participants = convObj.participants;
                string otherUserId = participants[0].ToString() == currentUserId
                    ? participants[1].ToString()
                    : participants[0].ToString();

                string otherUserEmail = GetEmailByUserId(otherUserId);
                string lastMessage = convObj.lastMessage != null ? (string)convObj.lastMessage : "";

                // lstConversations'a ekle
                string itemText = $"{otherUserEmail} - {lastMessage}";
                lstConversations.Items.Add(new ListBoxItem { Text = itemText, Tag = conversationId });
            }

            // Eğer UI güncellendiyse, son kontrol edilen zamanı güncelle
            if (isUpdated)
            {
                lastCheckedUpdate = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
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


        private void LoadMessages(string conversationId)
        {
            lstMessages.Items.Clear();

            // Mesajları Firebase'den al
            string url = $"{firebaseUrl}/messages/{conversationId}.json";
            string msgData = GetJson(url);
            if (msgData == null || msgData == "null") return;

            var messages = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(msgData);
            if (messages == null) return;

            // Her mesajı lstMessages'a ekle
            foreach (var msg in messages)
            {
                string senderId = msg.Value.sender;
                string text = msg.Value.text;
                string prefix = senderId == currentUserId ? "Ben: " : "Onlar: ";
                lstMessages.Items.Add(prefix + text);
            }
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            // Yeni mesaj veya konuşma güncellemesi kontrolü
            CheckForNewMessages();
        }

        private void CheckForNewMessages()
        {
            string url = $"{firebaseUrl}/userConversations/{currentUserId}.json";
            string convsJson = GetJson(url);

            if (convsJson == null || convsJson == "null") return;

            var conversations = JsonConvert.DeserializeObject<Dictionary<string, bool>>(convsJson);
            if (conversations == null) return;

            foreach (var kvp in conversations)
            {
                string conversationId = kvp.Key;
                string convUrl = $"{firebaseUrl}/conversations/{conversationId}.json";
                string convData = GetJson(convUrl);

                if (convData == null || convData == "null") continue;

                dynamic convObj = JsonConvert.DeserializeObject<dynamic>(convData);

                // Konuşmanın son güncelleme zamanını al
                long lastUpdate = convObj.lastUpdate != null ? (long)convObj.lastUpdate : 0;

                // Eğer yeni bir güncelleme varsa
                if (lastUpdate > lastCheckedUpdate)
                {
                    // lstConversations'ı güncelle
                    LoadConversations();
                    break;
                }
            }
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
