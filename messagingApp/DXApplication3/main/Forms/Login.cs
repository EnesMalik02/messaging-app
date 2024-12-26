using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Newtonsoft.Json; // NuGet'ten eklemelisiniz.
using System.IO;
using System.Net;

namespace main
{
    public partial class Login : DevExpress.XtraEditors.XtraForm
    {
        private string firebaseUrl = "https://messaging-app-11f5f-default-rtdb.europe-west1.firebasedatabase.app";

        public Login()
        {
            InitializeComponent();
        }

        private void Login_Load(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {
            // İsteğe bağlı label click event
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string password = txtPassword.Text.Trim(); // Şifre
            string email = txtEmail.Text.Trim();       // E-posta

            // Alanların doldurulup doldurulmadığını kontrol et
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Lütfen tüm alanları doldurun.");
                return;
            }

            // E-posta doğrulama
            if (!IsValidEmail(email))
            {
                MessageBox.Show("Lütfen geçerli bir e-posta adresi girin.");
                return;
            }

            // Kullanıcının ID'sini al
            string uniqueUserId = GetUserIdByEmail(email);
            if (string.IsNullOrEmpty(uniqueUserId))
            {
                // Kullanıcı bulunamadı
                MessageBox.Show("Bu e-posta adresiyle kayıtlı bir kullanıcı bulunamadı.");
                return;
            }

            // Şifre doğruluğunu kontrol et
            if (!IsPasswordCorrect(uniqueUserId, password))
            {
                MessageBox.Show("E-posta ve şifre eşleşmiyor. Lütfen tekrar deneyin.");
                return;
            }

            // Kullanıcı giriş yapabilir, nickname alınır
            string nickName = GetNickNameByUserId(uniqueUserId);
            if (string.IsNullOrEmpty(nickName))
            {
                nickName = "Kullanıcı"; // Kullanıcı adı yoksa varsayılan isim atanır
            }

            // Hoş geldiniz mesajı ve giriş ekranına yönlendirme
            MessageBox.Show($"Hoş geldiniz: {nickName}");

            // Giriş başarılı, ClientForm'a geçiş yap
            var clientForm = new Form1(email, uniqueUserId, nickName);
            clientForm.Show();
            this.Hide();
        }

        private string GetNickNameByUserId(string userId)
        {
            string url = $"{firebaseUrl}/users/{userId}.json";
            string userJson = GetJson(url);

            if (string.IsNullOrEmpty(userJson) || userJson == "null") return null;

            dynamic user = JsonConvert.DeserializeObject<dynamic>(userJson);
            return user.nickname != null ? (string)user.nickname : null;
        }



        private bool IsPasswordCorrect(string userId, string password)
        {
            string url = $"{firebaseUrl}/users/{userId}.json"; // Kullanıcı detaylarını al
            Console.WriteLine(url);
            string userJson = GetJson(url);

            if (string.IsNullOrEmpty(userJson) || userJson == "null") return false;

            dynamic user = JsonConvert.DeserializeObject<dynamic>(userJson);
            return user.password == password; // Şifreyi kontrol et
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
            string emailKey = email.Replace(".", ","); // Firebase'de e-posta adresindeki "." yerine "," kullanılır
            string url = $"{firebaseUrl}/users.json"; // Tüm kullanıcıları alır
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
            return null; // Kullanıcı bulunamazsa
        }

        private bool CreateUserRecord(string userId, string email, string password)
        {
            string url = $"{firebaseUrl}/users/{userId}.json";
            var data = new
            {
                email = email.Replace(".", ","), // Firebase için format
                pasword = password
            };

            string jsonData = JsonConvert.SerializeObject(data);

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "PUT";
                request.ContentType = "application/json";
                using (var sw = new StreamWriter(request.GetRequestStream()))
                {
                    sw.Write(jsonData);
                }

                var response = (HttpWebResponse)request.GetResponse();
                return true;
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


        private void btnSignup_Click(object sender, EventArgs e)
        {
            // Kayıt ekranını aç
            var registerForm = new Forms.Register();
            registerForm.Show();
            this.Hide();
        }
    }
}