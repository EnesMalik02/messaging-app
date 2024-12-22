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
        private string reciverID;
        private string otherUserName;
        private string selectedOtherUserId; // Seçilen alıcı kullanıcı ID'si
        private string selectedConversationId;


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
            timer1.Interval = 1000; // Her saniyede bir kontrol
            timer1.Tick += (s, args) => CheckForNewMessages();
            timer1.Start();
        }


        private void DeleteJson(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "DELETE";
            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    // Silme işlemi başarılı
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}");
            }
        }


        private void btnDeleteConversation_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedConversationId))
            {
                MessageBox.Show("Lütfen silmek için bir sohbet seçin.");
                return;
            }

            // Kullanıcıdan onay al
            var result = MessageBox.Show("Bu sohbeti silmek istediğinize emin misiniz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result != DialogResult.Yes)
            {
                return;
            }

            // Sohbeti Firebase'den sil
            string conversationUrl = $"{firebaseUrl}/conversations/{selectedConversationId}.json";
            DeleteJson(conversationUrl);

            // Kullanıcıların sohbet listelerinden kaldır
            string currentUserConvUrl = $"{firebaseUrl}/userConversations/{currentUserId}/{selectedConversationId}.json";
            DeleteJson(currentUserConvUrl);

            string otherUserConvUrl = $"{firebaseUrl}/userConversations/{selectedOtherUserId}/{selectedConversationId}.json";
            DeleteJson(otherUserConvUrl);

            // Arayüzü güncelle
            MessageBox.Show("Sohbet başarıyla silindi.");
            selectedConversationId = null;
            selectedOtherUserId = null;
            LoadConversations(); // Konuşma listesini yeniden yükle
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
            AddChatTile("Yeni Kullanıcı", "Henüz mesaj yok.", newConvId, otherUserId);

            MessageBox.Show("Sohbet başarıyla başlatıldı.");
        }
        private void AddChatTile(string userName, string lastMessage, string conversationId, string otherUserId)
        {
            // Mevcut kutucuk var mı kontrol et
            foreach (TileGroup group in tileControlChats.Groups)
            {
                foreach (TileItem item in group.Items)
                {
                    // Her TileItem'ın Tag özelliği üzerinden kontrol yap
                    var tag = item.Tag as dynamic;
                    if (tag != null && tag.ConversationId == conversationId)
                    {
                        // Kutucuk zaten varsa, güncelle
                        item.Text = $"{userName}\n{lastMessage}";
                        return;
                    }
                }
            }

            // Eğer mevcut kutucuk yoksa, yeni bir tane ekle
            if (tileControlChats.Groups.Count == 0)
            {
                var tileGroup = new TileGroup();
                tileControlChats.Groups.Add(tileGroup);
            }

            var tileGroupChats = tileControlChats.Groups[0];
            var tileItem = new TileItem
            {
                Text = $"{userName}\n{lastMessage}",
                Tag = new { ConversationId = conversationId, OtherUserId = otherUserId },
                ItemSize = TileItemSize.Medium // Boyut küçültme
            };

            // Yazı ve içerik boyutu
            tileItem.AppearanceItem.Normal.Font = new Font("Arial", 8); // Daha küçük yazı boyutu
            tileItem.AppearanceItem.Normal.BackColor = Color.LightBlue;
            tileItem.AppearanceItem.Normal.Options.UseBackColor = true;
            tileItem.AppearanceItem.Normal.Options.UseFont = true;

            // TileItem'a tıklama olayı
            tileItem.ItemClick += (s, e) =>
            {
                var tag = (dynamic)((TileItem)s).Tag;
                selectedConversationId = tag.ConversationId;
                selectedOtherUserId = tag.OtherUserId;

                MessageBox.Show($"Sohbet Yükleniyor:\nSohbet ID: {selectedConversationId}");
                LoadMessages(selectedConversationId);
            };

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
            if (string.IsNullOrEmpty(selectedOtherUserId) || string.IsNullOrEmpty(selectedConversationId))
            {
                MessageBox.Show("Lütfen bir sohbet seçin.");
                return;
            }

            string msg = txtMessage.Text.Trim();
            if (string.IsNullOrEmpty(msg))
            {
                MessageBox.Show("Lütfen bir mesaj yazın.");
                return;
            }

            // 1) Mesajı Firebase'e gönder
            string url = $"{firebaseUrl}/messages/{selectedConversationId}.json";
            var msgObj = new
            {
                sender = currentUserId,
                text = msg,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            string json = JsonConvert.SerializeObject(msgObj);
            PostJson(url, json);

            // 2) Gönderilen mesajı son mesaj olarak conversations içinde güncelle
            var conversationUpdate = new
            {
                lastMessage = msg,
                lastUpdate = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            // Patch veya Put ile güncelleyebilirsiniz, Patch genelde daha uygun
            string convUrl = $"{firebaseUrl}/conversations/{selectedConversationId}.json";
            PatchJson(convUrl, JsonConvert.SerializeObject(conversationUpdate));

            // 3) Mesaj gönderildikten sonra textbox'ı temizle
            txtMessage.Clear();

            // 4) Diğer kullanıcıyı bilgilendirmek için userUpdates'i güncelle
            string triggerUrl = $"{firebaseUrl}/userUpdates/{selectedOtherUserId}.json";
            PutJson(triggerUrl, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());

            // 5) Kendi arayüzümü de güncellemek istersek (opsiyonel)
            LoadConversations();
            LoadMessages(selectedConversationId);
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
            // 1) Eğer başka bir thread’deysek, bu metodu “UI thread” içinde çağır.
            if (lstMessages.InvokeRequired)
            {
                // 2) Invoke ile tekrar kendini UI thread’inde çağır
                lstMessages.Invoke(new Action(() => LoadMessages(conversationId)));
                return;
            }

            // Artık UI thread’inde olduğumuz garanti, güvenle lstMessages’ı güncelleyebiliriz.
            lstMessages.Items.Clear();

            string url = $"{firebaseUrl}/messages/{conversationId}.json";
            string msgData = GetJson(url);

            if (string.IsNullOrEmpty(msgData) || msgData == "null") return;

            var messages = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(msgData);
            if (messages == null) return;

            foreach (var msg in messages)
            {
                string senderId = msg.Value.sender;
                string text = msg.Value.text;

                string senderName = senderId == currentUserId ? "Ben" : GetNameByUserId(senderId);
                lstMessages.Items.Add($"{senderName}: {text}");
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
            if (string.IsNullOrEmpty(convsJson) || convsJson == "null") return;

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

                AddChatTile(userName, lastMessage, conversationId, otherUserId);
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
            string triggerUrl = $"{firebaseUrl}/userUpdates/{currentUserId}.json";
            string triggerData = GetJson(triggerUrl);

            if (!string.IsNullOrEmpty(triggerData) && triggerData != "null")
            {
                long triggerTime = long.Parse(triggerData);

                if (triggerTime > lastCheckedUpdate)
                {
                    lastCheckedUpdate = triggerTime;

                    // 1) Tüm sohbet listesini (Tile'ları) yenile, böylece lastMessage güncellenir
                    LoadConversations();

                    // 2) Şu an seçili bir konuşma varsa o mesaj listesini de yenile
                    if (!string.IsNullOrEmpty(selectedConversationId))
                    {
                        LoadMessages(selectedConversationId);
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
