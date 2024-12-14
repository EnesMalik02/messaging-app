using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows.Forms;
using Newtonsoft.Json; // NuGet'ten eklemelisiniz.

namespace messagingApp
{
    public partial class LoginForm : Form
    {
        private string firebaseUrl = "https://messaging-app-11f5f-default-rtdb.europe-west1.firebasedatabase.app";
        // Kendi Firebase Realtime Database URL'nizi buraya yazın.

        public LoginForm()
        {
            InitializeComponent();
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            // Form başlatıldığında yapılacak işlemler
        }

        private void label2_Click(object sender, EventArgs e)
        {
            // İsteğe bağlı label click event
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Giriş Butonuna tıklandığında
            string nickName = txtNickName.Text.Trim(); // Kullanıcı adı (nickname)
            if (string.IsNullOrEmpty(nickName))
            {
                MessageBox.Show("Lütfen bir kullanıcı adı girin.");
                return;
            }

            string email = txtEmail.Text.Trim();
            if (string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Lütfen e-posta adresinizi girin.");
                return;
            }

            // E-posta doğrulama
            if (!IsValidEmail(email))
            {
                MessageBox.Show("Lütfen geçerli bir e-posta adresi girin.");
                return;
            }

            // Kullanıcıyı Firebase'den çek
            string uniqueUserId = GetUserIdByEmail(email); // Eğer kullanıcı varsa ID'yi al
            string currentUserId;

            if (string.IsNullOrEmpty(uniqueUserId))
            {
                // Kullanıcı yok, yeni oluştur
                string newId = Guid.NewGuid().ToString(); // Benzersiz ID oluştur
                bool success = CreateUserRecord(newId, email, nickName);
                if (!success)
                {
                    MessageBox.Show("Kullanıcı oluşturulamadı. Daha sonra tekrar deneyin.");
                    return;
                }
                currentUserId = newId;
                MessageBox.Show("Yeni kullanıcı oluşturuldu: " + currentUserId);
            }
            else
            {
                // Kayıtlı kullanıcı
                currentUserId = uniqueUserId;
                MessageBox.Show("Hoş geldiniz, mevcut kullanıcı ID: " + currentUserId);
            }

            // ClientForm'u aç
            var clientForm = new ClientForm(email, currentUserId, nickName);
            clientForm.Show();
            this.Hide();
        }

        private string GetJson(string url)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.ContentType = "application/json";

                using (var response = (HttpWebResponse)request.GetResponse())
                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    return sr.ReadToEnd();
                }
            }
            catch
            {
                return null; // Hata durumunda `null` döner
            }
        }


        private string GetUserIdByEmail(string email)
        {
            string emailKey = email.Replace(".", ","); // Firebase'de e-posta adresindeki "." ile çalışılamaz
            string url = $"{firebaseUrl}/users.json";
            string json = GetJson(url);

            if (string.IsNullOrEmpty(json) || json == "null") return null;

            var users = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(json);

            foreach (var user in users)
            {
                if (user.Value.email == emailKey)
                {
                    return user.Key; // Kullanıcının benzersiz ID'sini döndür
                }
            }
            return null;
        }

        private bool CreateUserRecord(string userId, string email, string nickName)
        {
            string url = $"{firebaseUrl}/users/{userId}.json";
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "PUT";
            request.ContentType = "application/json";

            // Kullanıcı verisi (ID, e-posta ve nickname ile birlikte)
            var data = new
            {
                email = email.Replace(".", ","), // Firebase'e uygun format
                nickname = nickName
            };

            string jsonData = JsonConvert.SerializeObject(data);

            try
            {
                using (var sw = new StreamWriter(request.GetRequestStream()))
                {
                    sw.Write(jsonData);
                }

                var response = (HttpWebResponse)request.GetResponse();
                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    string result = sr.ReadToEnd();
                    // Eğer "null" değilse başarı kabul edilebilir
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }


        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
            // Label 1 Click event
        }
    }
}
