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
using messagingApp.Models;
using messagingApp.Services;

namespace messagingApp.Forms
{
    public partial class ClientForm : Form
    {
        private string currentUserEmail;
        private string currentUserId;
        private string otherUserName;
        private string selectedOtherUserId;
        private string selectedConversationId;
        private long lastCheckedUpdate = 0;

        // Bu projede tek bir URL var diyorsanız:
        private readonly FirebaseManager _firebaseManager;
        private readonly ConversationService _conversationService;

        public ClientForm(string email, string userId, string userName)
        {
            InitializeComponent();

            currentUserEmail = email;
            currentUserId = userId;
            selfID.Text = $"ID : {userId}";
            selfName.Text = userName;

            // FirebaseManager örneği
            string firebaseUrl = "https://messaging-app-11f5f-default-rtdb.europe-west1.firebasedatabase.app";
            _firebaseManager = new FirebaseManager(firebaseUrl);

            // ConversationService örneği
            _conversationService = new ConversationService(_firebaseManager);
        }

        private void ClientForm_Load(object sender, EventArgs e)
        {
            LoadConversations();
            timer1.Interval = 1000;
            timer1.Tick += (s, args) => CheckForNewMessages();
            timer1.Start();
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
            string otherUserEmail = txtOtherUserId.Text.Trim();
            if (string.IsNullOrEmpty(otherUserEmail))
            {
                MessageBox.Show("Lütfen diğer kullanıcının e-posta adresini girin.");
                return;
            }

            // Diğer userId'yi al (örnek: Kodunuzu ConversationService’e taşıyabilirsiniz)
            string otherUserId = GetUserIdByEmail(otherUserEmail);
            if (string.IsNullOrEmpty(otherUserId))
            {
                MessageBox.Show("Bu e-posta adresine sahip bir kullanıcı bulunamadı.");
                return;
            }

            // Var mı diye kontrol
            string existingConversationId = GetExistingConversationId(currentUserId, otherUserId);
            if (!string.IsNullOrEmpty(existingConversationId))
            {
                MessageBox.Show("Bu kişiyle zaten bir konuşmanız var.");
                return;
            }

            // Yeni konuşma oluştur
            string newConvId = Guid.NewGuid().ToString();
            var convData = new
            {
                participants = new string[] { currentUserId, otherUserId },
                lastMessage = "",
                lastUpdate = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            string convJson = JsonConvert.SerializeObject(convData);

            _firebaseManager.PutJson($"conversations/{newConvId}.json", convJson);

            // Kullanıcılar altına ekle
            _conversationService.AddConversationToUser(currentUserId, newConvId);
            _conversationService.AddConversationToUser(otherUserId, newConvId);

            AddChatTile("Yeni Kullanıcı", "Henüz mesaj yok.", newConvId, otherUserId);

            MessageBox.Show("Sohbet başarıyla başlatıldı.");
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

            // 1) Mesajı gönder
            _conversationService.SendMessage(selectedConversationId, currentUserId, msg);

            // 2) Mesaj kutusunu temizle
            txtMessage.Clear();

            // 3) Karşı tarafı bilgilendirmek için
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            _firebaseManager.PutJson($"userUpdates/{selectedOtherUserId}.json", now.ToString());

            // 4) Kendi arayüzünü güncelle
            LoadConversations();
            LoadMessages(selectedConversationId);
        }

        private void LoadMessages(string conversationId)
        {
            // Cross-thread safe
            if (lstMessages.InvokeRequired)
            {
                lstMessages.Invoke(new Action(() => LoadMessages(conversationId)));
                return;
            }

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
            // Tüm konuşmaları çek
            string convsJson = _firebaseManager.GetJson($"userConversations/{currentUserId}.json");
            if (string.IsNullOrEmpty(convsJson) || convsJson == "null") return;

            var conversations = JsonConvert.DeserializeObject<Dictionary<string, bool>>(convsJson);
            if (conversations == null) return;

            // TileControl’u vs. temizlemek istiyorsanız (opsiyonel)
            tileControlChats.Groups.Clear();
            var tileGroup = new TileGroup();
            tileControlChats.Groups.Add(tileGroup);

            foreach (var kvp in conversations)
            {
                string conversationId = kvp.Key;
                string convData = _firebaseManager.GetJson($"conversations/{conversationId}.json");
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

        private void AddChatTile(string userName, string lastMessage, string conversationId, string otherUserId)
        {
            // Tek bir TileGroup olduğunu varsayıyoruz
            if (tileControlChats.Groups.Count == 0)
            {
                tileControlChats.Groups.Add(new TileGroup());
            }
            var tileGroupChats = tileControlChats.Groups[0];

            var tileItem = new TileItem
            {
                Text = $"{userName}\n{lastMessage}",
                Tag = new { ConversationId = conversationId, OtherUserId = otherUserId },
                ItemSize = TileItemSize.Medium
            };

            tileItem.AppearanceItem.Normal.Font = new Font("Arial", 8);
            tileItem.AppearanceItem.Normal.BackColor = Color.LightBlue;
            tileItem.AppearanceItem.Normal.Options.UseBackColor = true;
            tileItem.AppearanceItem.Normal.Options.UseFont = true;

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

        private void CheckForNewMessages()
        {
            // userUpdates/currentUserId
            string triggerData = _firebaseManager.GetJson($"userUpdates/{currentUserId}.json");
            if (!string.IsNullOrEmpty(triggerData) && triggerData != "null")
            {
                long triggerTime = long.Parse(triggerData);
                if (triggerTime > lastCheckedUpdate)
                {
                    lastCheckedUpdate = triggerTime;
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
            LoginForm loginForm = new LoginForm();
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
    }
}
