using DevExpress.XtraEditors;
using main.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace main
{
    public partial class Form1 : DevExpress.XtraEditors.XtraForm
    {
        private string currentUserEmail;
        private string currentUserId;
        private string otherUserName;
        private string selectedOtherUserId;
        private string selectedConversationId;
        private long lastCheckedUpdate = 0;
        private string firebaseUrl = "https://messaging-app-11f5f-default-rtdb.europe-west1.firebasedatabase.app";


        // Bu projede tek bir URL var diyorsanız:
        private readonly FirebaseManager _firebaseManager;
        private readonly ConversationService _conversationService;

        public Form1(string email, string userId, string userName)
        {

            InitializeComponent();

            currentUserEmail = email;
            currentUserId = userId;
            selfID.Text = $"ID : {userId}";
            selfName.Text = userName;

            // FirebaseManager örneği
            _firebaseManager = new FirebaseManager(firebaseUrl);

            // ConversationService örneği
            _conversationService = new ConversationService(_firebaseManager);
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            LoadConversations();

            timer1.Interval = 1000; // 1 saniyelik aralıklarla kontrol
            timer1.Tick += Timer1_Tick; // Timer olayını bağla
            timer1.Start();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            // Güncelleme mesajını kontrol et
           // CheckForAnnouncement();
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            CheckForNewMessages();
        }



        private void panelControl1_Paint(object sender, PaintEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void btnDeleteConversation_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedConversationId))
            {
                MessageBox.Show("Lütfen silmek için bir sohbet seçin.");
                return;
            }

            var result = MessageBox.Show("Bu sohbeti silmek istediğinize emin misiniz?",
                                         "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result != DialogResult.Yes) return;

            // Sil
            _conversationService.DeleteConversation(selectedConversationId, currentUserId, selectedOtherUserId);

            MessageBox.Show("Sohbet başarıyla silindi.");
            selectedConversationId = null;
            selectedOtherUserId = null;
            LoadConversations();
        }

        private void btnNewConversation_Click(object sender, EventArgs e)
        {
            string emailList = txtOtherUserId.Text.Trim();
            if (string.IsNullOrEmpty(emailList))
            {
                MessageBox.Show("Lütfen diğer kullanıcıların e-posta adreslerini girin (virgülle ayrılmış).");
                return;
            }

            // 1) E-posta listesini parçalayarak kullanıcı ID'lerini al
            string[] emails = emailList.Split(',');
            var userIds = new List<string>();

            foreach (string email in emails)
            {
                string trimmedEmail = email.Trim();
                string userId = GetUserIdByEmail(trimmedEmail);

                if (string.IsNullOrEmpty(userId))
                {
                    MessageBox.Show($"Bu e-posta adresine sahip kullanıcı bulunamadı: {trimmedEmail}");
                    return;
                }

                // Aynı kullanıcı birden fazla kez eklenmesin
                if (!userIds.Contains(userId))
                {
                    userIds.Add(userId);
                }
            }

            // 2) Kullanıcının kendi ID'sini ekle (çift kontrol)
            if (!userIds.Contains(currentUserId))
            {
                userIds.Add(currentUserId);
            }

            // 3) Katılımcı sayısına göre birebir veya grup sohbeti
            string newConversationId = Guid.NewGuid().ToString();
            string conversationTitle = userIds.Count > 2
                ? "Yeni Grup Sohbeti" // Grup sohbeti için başlık
                : GetNameByUserId(userIds.Find(id => id != currentUserId)); // Birebir sohbette karşı kullanıcının adı

            // 4) Konuşmayı Firebase'e kaydet
            var conversationData = new
            {
                participants = userIds,
                title = conversationTitle,
                lastMessage = "Henüz mesaj yok."
            };

            _firebaseManager.PutJson($"conversations/{newConversationId}", JsonConvert.SerializeObject(conversationData));

            // 5) Her kullanıcı için konuşmayı ekle
            foreach (string userId in userIds)
            {
                _conversationService.AddConversationToUser(userId, newConversationId);
            }

            // 6) Yeni konuşmayı UI'de göster
            AddChatTile(conversationTitle, "Henüz mesaj yok.", newConversationId, string.Join(", ", emails));

            MessageBox.Show("Sohbet başarıyla oluşturuldu.");
        }



        private void btnSend_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedConversationId))
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

            // 1. Mesajı gönder
            _conversationService.SendMessage(selectedConversationId, currentUserId, msg);

            // 2. Mesaj kutusunu temizle
            txtMessage.Text = string.Empty;

            // 3. Katılımcıları bul ve tetikleme gönder
            List<string> participants = GetParticipants(selectedConversationId);
            long unixTimestamp = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;

            foreach (var participantId in participants)
            {
                if (participantId != currentUserId) // Kendiniz için tetikleme göndermeyin
                {
                    string triggerUrl = $"userUpdates/{participantId}";
                    _firebaseManager.PutJson(triggerUrl, unixTimestamp.ToString());
                }
            }

            // 4. Kendi ekranınızı güncelleyin
            LoadMessages(selectedConversationId);
        }


        private void LoadMessages(string conversationId)
        {
            lstMessages.Items.Clear();

            var messages = _conversationService.GetMessages(conversationId);
            if (messages == null) return;

            foreach (var msg in messages)
            {
                string senderId = msg.Value.sender;
                string text = msg.Value.text;

                string senderName = senderId == currentUserId ? "Ben" : GetNameByUserId(senderId);
                lstMessages.Items.Add($"{senderName}: {text}");
            }
        }

        private void LoadConversations()
        {
            string convsJson = _firebaseManager.GetJson($"userConversations/{currentUserId}.json");
            if (string.IsNullOrEmpty(convsJson) || convsJson == "null") return;

            var conversations = JsonConvert.DeserializeObject<Dictionary<string, bool>>(convsJson);
            if (conversations == null) return;

            tileControlChats.Groups.Clear();
            var tileGroup = new TileGroup();
            tileControlChats.Groups.Add(tileGroup);

            foreach (var kvp in conversations)
            {
                string conversationId = kvp.Key;
                string convData = _firebaseManager.GetJson($"conversations/{conversationId}.json");
                if (string.IsNullOrEmpty(convData) || convData == "null") continue;

                dynamic convObj = JsonConvert.DeserializeObject<dynamic>(convData);

                string chatTitle;
                var participants = convObj.participants;
                if (participants.Count == 2) // Eğer iki kişilik bir sohbetse
                {
                    string otherUserId = participants[0].ToString() == currentUserId
                        ? participants[1].ToString()
                        : participants[0].ToString();
                    chatTitle = GetNameByUserId(otherUserId); // Karşı tarafın adını kullan
                }
                else // Grup sohbeti için başlığı kullan
                {
                    chatTitle = convObj.title != null ? convObj.title.ToString() : "Grup";
                }

                string lastMessage = convObj.lastMessage != null ? (string)convObj.lastMessage : "Henüz mesaj yok.";

                AddChatTile(chatTitle, lastMessage, conversationId, chatTitle);
            }
        }


        private void AddChatTile(string userName, string lastMessage, string conversationId, string otherUserId)
        {
            if (tileControlChats.Groups.Count == 0)
            {
                tileControlChats.Groups.Add(new TileGroup());
            }
            var tileGroupChats = tileControlChats.Groups[0];

            var tileItem = new TileItem
            {
                Tag = new { ConversationId = conversationId, OtherUserId = otherUserId },
                ItemSize = TileItemSize.Medium
            };

            TileItemElement titleElement = new TileItemElement
            {
                Text = userName,
                TextAlignment = TileItemContentAlignment.TopLeft
            };

            TileItemElement subtitleElement = new TileItemElement
            {
                Text = lastMessage,
                TextAlignment = TileItemContentAlignment.BottomLeft
            };

            tileItem.Elements.Add(titleElement);
            tileItem.Elements.Add(subtitleElement);

            tileItem.AppearanceItem.Normal.BackColor = Color.LightBlue;

            tileItem.ItemClick += (s, e) =>
            {
                var tag = (dynamic)((TileItem)s).Tag;
                selectedConversationId = tag.ConversationId; // Sohbet ID'si atanıyor
                selectedOtherUserId = tag.OtherUserId; // Seçilen diğer kullanıcı atanıyor
                LoadMessages(selectedConversationId); // Mesajları yükle
            };

            tileGroupChats.Items.Add(tileItem);
        }

        private List<string> GetParticipants(string conversationId)
        {
            string json = _firebaseManager.GetJson($"conversations/{conversationId}.json");
            if (string.IsNullOrEmpty(json) || json == "null") return new List<string>();

            dynamic conversation = JsonConvert.DeserializeObject<dynamic>(json);
            List<string> participants = new List<string>();

            foreach (var participant in conversation.participants)
            {
                participants.Add(participant.ToString());
            }

            return participants;
        }

        private bool IsConversationExists(string userId1, string userId2)
        {
            string userConversationsJson = _firebaseManager.GetJson($"userConversations/{userId1}.json");
            if (string.IsNullOrEmpty(userConversationsJson) || userConversationsJson == "null")
                return false;

            var conversations = JsonConvert.DeserializeObject<Dictionary<string, bool>>(userConversationsJson);
            if (conversations == null) return false;

            foreach (var conversationId in conversations.Keys)
            {
                string convJson = _firebaseManager.GetJson($"conversations/{conversationId}.json");
                if (string.IsNullOrEmpty(convJson) || convJson == "null")
                    continue;

                dynamic convObj = JsonConvert.DeserializeObject<dynamic>(convJson);
                var participants = convObj.participants;

                // Eğer katılımcılar arasında userId2 varsa, konuşma zaten vardır
                if (participants != null && participants.Contains(userId2))
                    return true;
            }

            return false;
        }



        private void CheckForNewMessages()
        {
            string triggerUrl = $"userUpdates/{currentUserId}.json";
            string triggerData = _firebaseManager.GetJson(triggerUrl);

            if (!string.IsNullOrEmpty(triggerData) && triggerData != "null")
            {
                long triggerTime = long.Parse(triggerData);

                if (triggerTime > lastCheckedUpdate)
                {
                    lastCheckedUpdate = triggerTime;

                    // Sohbeti yenile
                    LoadConversations();

                    if (!string.IsNullOrEmpty(selectedConversationId))
                    {
                        LoadMessages(selectedConversationId);
                    }
                }
            }
        }


        // Bu metotları da ConversationService'e taşıyabilirsiniz, fakat örnek olarak burada bıraktık
        private string GetUserIdByEmail(string Email)
        {
            string emailKey = Email.Replace(".", ",");
            string json = _firebaseManager.GetJson("users.json");
            if (string.IsNullOrEmpty(json) || json == "null") return null;

            var users = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(json);
            foreach (var user in users)
            {
                string userEmail = (string)user.Value.email;
                if (userEmail == emailKey)
                {
                    return user.Key;
                }
            }
            return null;
        }

        private string GetExistingConversationId(string userId1, string userId2)
        {
            string userConversationsJson = _firebaseManager.GetJson($"userConversations/{userId1}.json");
            if (string.IsNullOrEmpty(userConversationsJson) || userConversationsJson == "null")
                return null;

            var conversations = JsonConvert.DeserializeObject<Dictionary<string, bool>>(userConversationsJson);
            if (conversations == null) return null;

            foreach (var conversation in conversations)
            {
                string conversationId = conversation.Key;

                string convData = _firebaseManager.GetJson($"conversations/{conversationId}.json");
                if (string.IsNullOrEmpty(convData) || convData == "null") continue;

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

        private string GetNameByUserId(string userId)
        {
            string json = _firebaseManager.GetJson($"users/{userId}.json");
            if (string.IsNullOrEmpty(json) || json == "null") return "Bilinmeyen";

            dynamic userObj = JsonConvert.DeserializeObject<dynamic>(json);
            string name = (string)userObj.nickname;
            otherUserName = name;
            return name;
        }

        // Diğer event’ler (UI)
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

        private void btnLogout_Click_Click(object sender, EventArgs e)
        {
            this.Hide();
            Login loginForm = new Login();
            loginForm.Show();
            this.Close();
        }

        private void lstMessages_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void selfName_Click(object sender, EventArgs e)
        {
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            // Timer ile asenkron da yapabilirsiniz, ama basitçe UI thread’de
            CheckForNewMessages();
        }

        private void tileControlChats_Click(object sender, EventArgs e)
        {

        }

        private void simpleButton1_Click_1(object sender, EventArgs e)
        {

        }

        private void selfName_Click_1(object sender, EventArgs e)
        {

        }

        private void selfID_Click(object sender, EventArgs e)
        {

        }

        private void chatSettings_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedConversationId))
            {
                MessageBox.Show("Lütfen bir sohbet seçin.");
                return;
            }

            // Firebase'den mevcut sohbet verisini alın
            string convData = _firebaseManager.GetJson($"conversations/{selectedConversationId}.json");
            if (string.IsNullOrEmpty(convData) || convData == "null")
            {
                MessageBox.Show("Sohbet verisi alınamadı.");
                return;
            }

            dynamic convObj = JsonConvert.DeserializeObject<dynamic>(convData);
            var participants = convObj.participants;

            // Katılımcı sayısını kontrol edin
            if (participants == null || participants.Count < 3)
            {
                MessageBox.Show("Bu sohbet bir grup sohbeti değil. Grup adını yalnızca grup sohbetlerinde değiştirebilirsiniz.");
                return;
            }

            // Kullanıcıdan yeni sohbet adını alın
            string currentTitle = convObj.title != null ? convObj.title.ToString() : "Mevcut Ad Yok";
            string newChatTitle = XtraInputBox.Show($"Mevcut Ad: {currentTitle}\nYeni grup adını girin:", "Grup Ayarları", "");

            if (string.IsNullOrEmpty(newChatTitle))
            {
                MessageBox.Show("Grup adı boş olamaz.");
                return;
            }

            // Firebase'de grup adını güncelle
            var updateData = new { title = newChatTitle };
            _firebaseManager.PatchJson($"conversations/{selectedConversationId}", JsonConvert.SerializeObject(updateData));

            // Arayüzü güncelle
            LoadConversations();

            MessageBox.Show("Grup adı başarıyla değiştirildi.");
        }

        private void CheckForAnnouncement()
        {
            // Firebase'den duyuru bilgisini al
            string announcementJson = _firebaseManager.GetJson("announcement.json");
            if (string.IsNullOrEmpty(announcementJson) || announcementJson == "null")
            {
                Console.WriteLine("No announcement found.");
                return;
            }

            try
            {
                // Duyuruyu ayrıştır
                dynamic announcement = JsonConvert.DeserializeObject<dynamic>(announcementJson);
                string message = announcement.message;
                long announcementTimestamp = announcement.timestamp;

                // Kullanıcı bilgilerini al
                string userJson = _firebaseManager.GetJson($"users/{currentUserId}.json");
                if (string.IsNullOrEmpty(userJson) || userJson == "null")
                {
                    Console.WriteLine("User data not found.");
                    return;
                }

                dynamic userData = JsonConvert.DeserializeObject<dynamic>(userJson);
                long lastReadTimestamp = userData.announcementRead != null ? (long)userData.announcementRead : 0;

                Console.WriteLine($"Current Announcement Timestamp: {announcementTimestamp}, Last Read Timestamp: {lastReadTimestamp}");

                // Eğer duyuru yeni ise
                if (announcementTimestamp > lastReadTimestamp)
                {
                    // Kullanıcıya duyuruyu göster
                    ShowAnnouncementForm(message);

                    try
                    {
                        // Firebase'de `announcementRead` alanını güncelle
                        string patchUrl = $"users/{currentUserId}/announcementRead.json";
                        string jsonBody = JsonConvert.SerializeObject(announcementTimestamp);

                        _firebaseManager.PatchJson(patchUrl, jsonBody);

                        // Güncellenen veriyi kontrol et
                        string updatedUserJson = _firebaseManager.GetJson($"users/{currentUserId}.json");
                        Console.WriteLine($"Updated User JSON: {updatedUserJson}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error while updating announcementRead: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while checking announcement: {ex.Message}");
            }
        }


        private void ShowAnnouncementForm(string message)
        {
            Form announcementForm = new Form
            {
                Text = "Güncelleme Bildirimi",
                Size = new Size(400, 200),
                StartPosition = FormStartPosition.CenterScreen,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            Label messageLabel = new Label
            {
                Text = message,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Tahoma", 10, FontStyle.Regular)
            };

            Button okButton = new Button
            {
                Text = "Tamam",
                Dock = DockStyle.Bottom,
                Height = 40
            };
            okButton.Click += (s, e) => { announcementForm.Close(); };

            announcementForm.Controls.Add(messageLabel);
            announcementForm.Controls.Add(okButton);
            announcementForm.ShowDialog();
        }

        private void updateBtn_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Closing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();

        }
    }
}
