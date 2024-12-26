﻿using DevExpress.XtraEditors;
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
                //lastUpdate = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            string convJson = JsonConvert.SerializeObject(convData);

            _firebaseManager.PutJson($"conversations/{newConvId}.json", convJson);

            // Kullanıcılar altına ekle
            _conversationService.AddConversationToUser(currentUserId, newConvId);
            _conversationService.AddConversationToUser(otherUserId, newConvId);
            string otherUserName = GetNameByUserId(otherUserId);

            AddChatTile(otherUserName, "Henüz mesaj yok.", newConvId, otherUserId);

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

            // 1) Mesajı Firebase'e ekle
            var msgObj = new
            {
                sender = currentUserId,
                text = msg,
                //timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            _firebaseManager.PostJson($"messages/{selectedConversationId}.json", JsonConvert.SerializeObject(msgObj));

            // 2) Conversation'ı güncelle
            var convUpdate = new
            {
                lastMessage = msg,
                //lastUpdate = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            _firebaseManager.PatchJson($"conversations/{selectedConversationId}.json", JsonConvert.SerializeObject(convUpdate));

            // 3) Mesaj kutusunu temizle
            txtMessage.Text = string.Empty;

            // 4) Diğer kullanıcı için tetikleme oluştur
            long unixTimestamp = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            _firebaseManager.PutJson($"userUpdates/{selectedOtherUserId}.json", unixTimestamp.ToString());

            // 5) Kendi ekranını güncelle
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
            //Tüm konuşmaları çek
            string convsJson = _firebaseManager.GetJson($"userConversations/{currentUserId}.json");
            if (string.IsNullOrEmpty(convsJson) || convsJson == "null") return;

            var conversations = JsonConvert.DeserializeObject<Dictionary<string, bool>>(convsJson);
            if (conversations == null) return;

            //TileControl’u vs. temizlemek istiyorsanız (opsiyonel)
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
            // Eğer hiç grup yoksa, bir tane ekleyelim
            if (tileControlChats.Groups.Count == 0)
            {
                tileControlChats.Groups.Add(new TileGroup());
            }
            var tileGroupChats = tileControlChats.Groups[0];

            // Mevcut TileItem oluştur
            var tileItem = new TileItem
            {
                // Metin yerine 2 ayrı element kullanacağız
                Tag = new { ConversationId = conversationId, OtherUserId = otherUserId },
                ItemSize = TileItemSize.Medium
            };

            // --- 1) Başlık (örnek: userName) ---
            TileItemElement titleElement = new TileItemElement
            {
                Text = userName,
                TextAlignment = TileItemContentAlignment.TopLeft
            };
            // Font & renk ayarları
            titleElement.Appearance.Normal.Font = new Font("Tahoma", 10, FontStyle.Bold);
            titleElement.Appearance.Normal.ForeColor = Color.Black;

            // --- 2) Alt Bilgi (örnek: lastMessage) ---
            TileItemElement subtitleElement = new TileItemElement
            {
                Text = lastMessage,
                TextAlignment = TileItemContentAlignment.BottomLeft
            };
            subtitleElement.Appearance.Normal.Font = new Font("Tahoma", 8, FontStyle.Regular);
            subtitleElement.Appearance.Normal.ForeColor = Color.Black;

            // Elemanları tileItem’a ekle
            tileItem.Elements.Add(titleElement);
            tileItem.Elements.Add(subtitleElement);

            // Arka plan rengi
            tileItem.AppearanceItem.Normal.BackColor = Color.LightBlue;
            tileItem.AppearanceItem.Normal.Options.UseBackColor = true;

            // Buton (tile) tıklandığında yapacağımız işlemler
            tileItem.ItemClick += (s, e) =>
            {
                var tag = (dynamic)((TileItem)s).Tag;
                selectedConversationId = tag.ConversationId;
                selectedOtherUserId = tag.OtherUserId;

                // Tıklanınca mesajları yükle
                LoadMessages(selectedConversationId);
            };

            // Son olarak tileGroup içine ekleyelim
            tileGroupChats.Items.Add(tileItem);
        }

        private void CheckForNewMessages()
        {
            string triggerData = _firebaseManager.GetJson($"userUpdates/{currentUserId}.json");

            if (!string.IsNullOrEmpty(triggerData) && triggerData != "null")
            {
                long triggerTime = long.Parse(triggerData);

                if (triggerTime > lastCheckedUpdate)
                {
                    lastCheckedUpdate = triggerTime;

                    // Eğer bir konuşma seçiliyse mesajları yükle
                    if (!string.IsNullOrEmpty(selectedConversationId))
                    {
                        LoadMessages(selectedConversationId);
                    }

                    // Konuşmaların listesini yenile
                    LoadConversations();
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
    }
}
