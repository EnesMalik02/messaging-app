using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Newtonsoft.Json;

namespace messagingApp
{
    public partial class ClientForm : Form
    {
        private string currentUserEmail;
        private string currentUserId;
        private long lastCheckedUpdate = 0; // Unix zaman damgası olarak saklanır
        private string otherUserName;

        // Kendi Firebase URL'nizi buraya yazın
        private string firebaseUrl = "https://messaging-app-11f5f-default-rtdb.europe-west1.firebasedatabase.app";

        public ClientForm(string email, string userId, string userName)
        {
            InitializeComponent();
            currentUserEmail = email;
            currentUserId = userId;
            selfID.Text = $"ID : {userId}";
            selfName.Text = userName;
        }

        private void ClientForm_Load(object sender, EventArgs e)
        {
            LoadConversations();
            // Timer ayarları
            timer1.Interval = 2000; // 2 saniyede bir yenile
            timer1.Tick += timer1_Tick;
            timer1.Start();
        }


        private async void StartListeningToMessages(string conversationId)
        {
            string url = $"{firebaseUrl}/messages/{conversationId}.json";
            var request = WebRequest.Create(url);
            request.Method = "GET";
            request.Headers.Add("Accept", "text/event-stream");

            try
            {
                using (var response = await request.GetResponseAsync())
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = await reader.ReadLineAsync();
                        if (!string.IsNullOrEmpty(line) && line.StartsWith("data:"))
                        {
                            string jsonData = line.Substring(5).Trim();
                            // conversationId bilgisini buradan UpdateUIWithNewMessage'a geçirin
                            UpdateUIWithNewMessage(jsonData, conversationId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Streaming hatası: {ex.Message}");
            }
        }

        private void UpdateUIWithNewMessage(string messageJson, string conversationId)
        {
            dynamic messageObj = JsonConvert.DeserializeObject<dynamic>(messageJson);
            string senderId = messageObj.sender;
            string text = messageObj.text;

            // Eğer güncellenmesi gereken konuşma şu an seçiliyse:
            if (lstConversations.SelectedItem != null)
            {
                var selectedItem = (MessageList)lstConversations.SelectedItem;
                if (selectedItem.Tag.ToString() == conversationId)
                {
                    // lstMessages'ı yenile
                    LoadMessages(conversationId);
                }
            }

            // Dilerseniz konuşmaları da güncelleyebilirsiniz
            // LoadConversations();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // Text değiştiğinde yapılacak işlemler (gerekirse)
        }

        /// <summary>
        /// Yeni yapıya uygun hale getirildi.
        /// Bu metot artık tüm kullanıcıları "/users.json" dan çekecek
        /// ve email alanı ile eşleştirme yaparak doğru userId'yi döndürecek.
        /// </summary>
        private string GetUserIdByEmail(string Email)
        {
            string emailKey = Email.Replace(".", ","); // Noktaları virgülle değiştir.
            string url = $"{firebaseUrl}/users.json";
            string json = GetJson(url);

            if (string.IsNullOrEmpty(json) || json == "null") return null;

            var users = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(json);

            foreach (var user in users)
            {
                // Veritabanındaki email alanıyla eşleştiriyoruz
                string userEmail = (string)user.Value.email;
                if (userEmail == emailKey)
                {
                    return user.Key;
                }
            }
            return null;
        }

        private void btnNewConversation_Click(object sender, EventArgs e)
        {
            string otherUserEmail = txtOtherUserId.Text.Trim();
            if (string.IsNullOrEmpty(otherUserEmail))
            {
                MessageBox.Show("Lütfen diğer kullanıcının e-posta adresini girin.");
                return;
            }

            // Kullanıcı ID'sini al
            string otherUserId = GetUserIdByEmail(otherUserEmail);
            if (string.IsNullOrEmpty(otherUserId))
            {
                MessageBox.Show("Bu e-posta adresine sahip bir kullanıcı bulunamadı.");
                return;
            }

            // Sohbet kontrolü
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

            string convJson = JsonConvert.SerializeObject(convData);
            PutJson($"{firebaseUrl}/conversations/{newConvId}.json", convJson);

            // Kullanıcı için sohbeti güncelle
            AddConversationToUser(currentUserId, newConvId);
            AddConversationToUser(otherUserId, newConvId);

            // Yeni buton oluştur
            AddChatTile("Yeni Kullanıcı", "Henüz mesaj yok.", newConvId);

            MessageBox.Show("Sohbet başarıyla başlatıldı.");
        }

        private void AddChatTile(string userName, string lastMessage, string conversationId)
        {
            // TileGroup oluştur
            if (tileControlChats.Groups.Count == 0)
            {
                var tileGroup = new DevExpress.XtraEditors.TileGroup();
                tileControlChats.Groups.Add(tileGroup);
            }

            // İlk TileGroup'u al
            var tileGroupChats = tileControlChats.Groups[0];

            // Yeni TileItem oluştur
            var tileItem = new DevExpress.XtraEditors.TileItem
            {
                Text = $"{userName}\n{lastMessage}",
                Tag = conversationId // Sohbet ID'sini Tag olarak ekle
            };

            // Görsel ve metin düzeni
            tileItem.AppearanceItem.Normal.Font = new Font("Arial", 10);
            tileItem.AppearanceItem.Normal.BackColor = Color.LightBlue;
            tileItem.AppearanceItem.Normal.Options.UseBackColor = true;
            tileItem.AppearanceItem.Normal.Options.UseFont = true;

            // TileItem boyutu
            tileItem.ItemSize = DevExpress.XtraEditors.TileItemSize.Wide;

            // TileItem tıklama olayı
            tileItem.ItemClick += (s, e) =>
            {
                string selectedConversationId = (string)((TileItem)s).Tag;
                MessageBox.Show($"Sohbet Yükleniyor: {userName}\nSohbet ID: {selectedConversationId}");

                // Mesajları yükleme metodu çağır
                LoadMessages(selectedConversationId);
            };

            // TileItem'ı TileGroup'a ekle
            tileGroupChats.Items.Add(tileItem);
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

                string convUrl = $"{firebaseUrl}/conversations/{conversationId}.json";
                string convData = GetJson(convUrl);
                if (string.IsNullOrEmpty(convData) || convData == "null")
                    continue;

                dynamic convObj = JsonConvert.DeserializeObject<dynamic>(convData);
                var participants = convObj.participants;

                if (participants != null &&
                    ((participants[0].ToString() == userId1 && participants[1].ToString() == userId2) ||
                     (participants[0].ToString() == userId2 && participants[1].ToString() == userId1)))
                {
                    return conversationId;
                }
            }

            return null;
        }


        private void btnSend_Click(object sender, EventArgs e)
        {
            if (lstConversations.SelectedItem == null)
            {
                MessageBox.Show("Önce bir konuşma seçin.");
                return;
            }

            var selectedItem = (MessageList)lstConversations.SelectedItem;
            string conversationId = selectedItem.Tag.ToString();
            string msg = txtMessage.Text.Trim();

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

            // Konuşmayı güncelle
            string convUrl = $"{firebaseUrl}/conversations/{conversationId}.json";
            var convUpdate = new
            {
                lastMessage = msg,
                lastUpdate = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            string convJson = JsonConvert.SerializeObject(convUpdate);
            PatchJson(convUrl, convJson);

            // Alıcının userConversations'ını güncelle
            UpdateRecipientConversation(conversationId);

            txtMessage.Clear();
            LoadConversations();
            LoadMessages(conversationId);
        }

        private void lstConversations_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstConversations.SelectedItem == null) return;

            // Eğer GridControl kullanıyorsanız:
            var selectedItem = (MessageList)lstConversations.SelectedItem;

            // Eğer ListBox kullanıyorsanız:
            string conversationId = selectedItem.Tag.ToString();

            // Mesajları yükle
            LoadMessages(conversationId);
        }

        private void LoadMessages(string conversationId)
        {
            // Mesajları temizle (GridControl veya ListBox)
            lstMessages.Items.Clear(); // Eğer GridControl kullanıyorsanız, farklı bağlama yapabilirsiniz.

            // Mesajları Firebase'den al
            string url = $"{firebaseUrl}/messages/{conversationId}.json";
            string msgData = GetJson(url);
            if (string.IsNullOrEmpty(msgData) || msgData == "null") return;

            var messages = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(msgData);
            if (messages == null) return;

            // Mesajları bir listeye ekleyin
            var messageList = new List<MessageItem>();

            foreach (var msg in messages)
            {
                string senderId = msg.Value.sender;
                string text = msg.Value.text;
                long timestamp = msg.Value.timestamp;

                messageList.Add(new MessageItem
                {
                    SenderName = senderId == currentUserId ? "Ben" : GetNameByUserId(senderId),
                    Text = text,
                    Timestamp = DateTimeOffset.FromUnixTimeSeconds(timestamp).ToString("g")
                });
            }


            // Eğer ListBox kullanıyorsanız:
            foreach (var message in messageList)
            {
                lstMessages.Items.Add($"{message.SenderName}: {message.Text} ({message.Timestamp})");
            }
        }


        private void RefreshConversationAndMessages(string conversationId)
        {
            LoadConversations();

            foreach (MessageList item in lstConversations.Items)
            {
                if (item.Tag.ToString() == conversationId)
                {
                    lstConversations.SelectedItem = item;
                    break;
                }
            }

            LoadMessages(conversationId);
        }

        private void UpdateRecipientConversation(string conversationId)
        {
            string otherUserId = GetOtherUserId(conversationId);
            if (string.IsNullOrEmpty(otherUserId)) return;

            string url = $"{firebaseUrl}/userConversations/{otherUserId}/{conversationId}.json";
            PutJson(url, "true");
        }

        private string GetOtherUserId(string conversationId)
        {
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

        /// <summary>
        /// Kullanıcı ID'sine göre email alır.
        /// Artık "/users/{userId}.json" yapısı kullanılıyor.
        /// Kayıt sırasında '.' yerine ',' kullanıldığından geri çevirmeyi unutmayın.
        /// </summary>
        private string GetNameByUserId(string userId)
        {
            string url = $"{firebaseUrl}/users/{userId}.json";
            string json = GetJson(url);
            if (string.IsNullOrEmpty(json) || json == "null") return "Bilinmeyen";

            dynamic userObj = JsonConvert.DeserializeObject<dynamic>(json);
            string name = (string)userObj.nickname;
            otherUserName = name;
            return name;
        }

        private void AddToListBoxSafely(ListBox listBox, MessageList item)
        {
            if (listBox.InvokeRequired)
            {
                listBox.Invoke(new Action(() => listBox.Items.Add(item)));
            }
            else
            {
                listBox.Items.Add(item);
            }
        }

        private void LoadConversations()
        {
            string url = $"{firebaseUrl}/userConversations/{currentUserId}.json";
            string convsJson = GetJson(url);
            if (convsJson == null || convsJson == "null") return;

            var conversations = JsonConvert.DeserializeObject<Dictionary<string, bool>>(convsJson);

            foreach (var kvp in conversations)
            {
                string conversationId = kvp.Key;
                string convUrl = $"{firebaseUrl}/conversations/{conversationId}.json";
                string convData = GetJson(convUrl);

                if (string.IsNullOrEmpty(convData) || convData == "null") continue;

                dynamic convObj = JsonConvert.DeserializeObject<dynamic>(convData);
                string otherUserId = convObj.participants[0] == currentUserId
                    ? convObj.participants[1].ToString()
                    : convObj.participants[0].ToString();

                string userName = GetNameByUserId(otherUserId);
                string lastMessage = convObj.lastMessage != null ? (string)convObj.lastMessage : "Henüz mesaj yok.";

                AddChatTile(userName, lastMessage, conversationId);
            }
        }


        private void LoadMessagesEski(string conversationId)
        {
            lstMessages.Items.Clear();

            string url = $"{firebaseUrl}/messages/{conversationId}.json";
            string msgData = GetJson(url);
            if (msgData == null || msgData == "null") return;

            var messages = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(msgData);
            if (messages == null) return;

            foreach (var msg in messages)
            {
                string senderId = msg.Value.sender;
                string text = msg.Value.text;
                string prefix = senderId == currentUserId ? "Ben: " : $"{otherUserName}: ";
                lstMessages.Items.Add(prefix + text);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Task.Run(() => CheckForNewMessages());
        }

        private async Task CheckForNewMessagesAsync()
        {
            string url = $"{firebaseUrl}/userConversations/{currentUserId}.json";

            try
            {
                string convsJson = await GetJsonAsync(url);

                if (string.IsNullOrEmpty(convsJson) || convsJson == "null") return;

                var conversations = JsonConvert.DeserializeObject<Dictionary<string, bool>>(convsJson);
                if (conversations == null) return;

                foreach (var kvp in conversations)
                {
                    string conversationId = kvp.Key;
                    string convUrl = $"{firebaseUrl}/conversations/{conversationId}.json";
                    string convData = await GetJsonAsync(convUrl);

                    if (string.IsNullOrEmpty(convData) || convData == "null") continue;

                    dynamic convObj = JsonConvert.DeserializeObject<dynamic>(convData);

                    long lastUpdate = convObj.lastUpdate != null ? (long)convObj.lastUpdate : 0;

                    if (lastUpdate > lastCheckedUpdate)
                    {
                        lastCheckedUpdate = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                        LoadConversations();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}");
            }
        }

        private async Task<string> GetJsonAsync(string url)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync();
                }
                catch
                {
                    return null;
                }
            }
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
                long lastUpdate = convObj.lastUpdate != null ? (long)convObj.lastUpdate : 0;

                if (lastUpdate > lastCheckedUpdate)
                {
                    lastCheckedUpdate = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                    // Konuşmaları yenile
                    LoadConversations();

                    // Eğer yeni mesaj gelen konuşma zaten seçiliyse, lstMessages'ı da yenile
                    if (lstConversations.InvokeRequired)
                    {
                        lstConversations.Invoke(new Action(() =>
                        {
                            if (lstConversations.SelectedItem != null &&
                                ((MessageList)lstConversations.SelectedItem).Tag.ToString() == conversationId)
                            {
                                LoadMessages(conversationId);
                            }
                        }));
                    }
                    else
                    {
                        if (lstConversations.SelectedItem != null &&
                            ((MessageList)lstConversations.SelectedItem).Tag.ToString() == conversationId)
                        {
                            LoadMessages(conversationId);
                        }
                    }

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

        private void label1_Click(object sender, EventArgs e)
        {
            //
        }

        private void selfName_Click(object sender, EventArgs e)
        {
            //
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(currentUserId))
            {
                Clipboard.SetText(currentUserId);
                MessageBox.Show("ID kopyalandı.");
            }
            else
            {
                MessageBox.Show("Kopyalanacak metin yok!");
            }
        }

        private void lstMessages_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnLogout_Click_Click(object sender, EventArgs e)
        {
            // LoginForm'a geri dön
            this.Hide(); // Mevcut formu gizle
            LoginForm loginForm = new LoginForm(); // LoginForm örneğini oluştur
            loginForm.Show(); // LoginForm'u göster
            this.Close(); // ClientForm'u tamamen kapat
        }
    }

    public class MessageList
    {
        public string Text { get; set; }
        public object Tag { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }

    public class ChatItem
    {
        public string Username { get; set; }
        public string LastMessage { get; set; }
        public string ProfilePhoto { get; set; }
    }
    public class MessageItem
    {
        public string SenderName { get; set; }
        public string Text { get; set; }
        public string Timestamp { get; set; }
    }


}
