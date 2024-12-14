using System;
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

            // E-posta adresindeki '.' karakterlerini ',' ile değiştirelim:
            string firebaseKey = email.Replace(".", ",");

            // Kullanıcıyı Firebase'den çek
            var userRecord = GetUserByEmail(firebaseKey);
            string currentUserId;
            string currentUserEmail = email;

            if (userRecord == null)
            {
                // Kullanıcı yok, yeni oluştur
                string newId = Guid.NewGuid().ToString();
                bool success = CreateUserRecord(firebaseKey, newId);
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
                currentUserId = (string)userRecord.id;
                MessageBox.Show("Hoş geldiniz, ID: " + currentUserId);
            }

            // ClientForm'u aç
            var clientForm = new ClientForm(currentUserEmail, currentUserId);
            clientForm.Show();
            this.Hide();
        }

        private dynamic GetUserByEmail(string firebaseKey)
        {
            string url = $"{firebaseUrl}/users/{firebaseKey}.json";
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "application/json";

            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    string result = sr.ReadToEnd();
                    if (result == "null") return null;
                    return JsonConvert.DeserializeObject<dynamic>(result);
                }
            }
            catch
            {
                return null;
            }
        }

        private bool CreateUserRecord(string firebaseKey, string userId)
        {
            string url = $"{firebaseUrl}/users/{firebaseKey}.json";
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "PUT";
            request.ContentType = "application/json";

            var data = new { id = userId };
            string jsonData = JsonConvert.SerializeObject(data);

            using (var sw = new StreamWriter(request.GetRequestStream()))
            {
                sw.Write(jsonData);
            }

            var response = (HttpWebResponse)request.GetResponse();
            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                string result = sr.ReadToEnd();
                // result "null" değilse ve hata vermediyse başarı kabul edebiliriz.
            }
            return true;
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
    }
}
