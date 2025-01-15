using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace main.Forms
{
    public partial class Register : DevExpress.XtraEditors.XtraForm
    {
        
            private string firebaseUrl = "https://messaging-app-11f5f-default-rtdb.europe-west1.firebasedatabase.app";

            public Register()
            {
                InitializeComponent();
            }

            private void btnSignUp_Click(object sender, EventArgs e)
            {
                string nickName = txtNickName.Text.Trim();
                string email = txtEmail.Text.Trim();
                string password = txtPassword.Text.Trim();

                // Alanların kontrolü
                if (string.IsNullOrEmpty(nickName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Lütfen tüm alanları doldurun.");
                    return;
                }

                // E-posta doğrulama
                if (!IsValidEmail(email))
                {
                    MessageBox.Show("Geçerli bir e-posta adresi girin.");
                    return;
                }

                // Kullanıcının var olup olmadığını kontrol et
                if (IsEmailRegistered(email))
                {
                    MessageBox.Show("Bu e-posta adresi zaten kayıtlı.");
                    return;
                }

                // Kullanıcıyı Firebase'e kaydet
                string userId = Guid.NewGuid().ToString();
                bool success = CreateUserRecord(userId, nickName, email, password);

                if (success)
                {
                    MessageBox.Show("Kayıt başarıyla tamamlandı. Giriş yapabilirsiniz.");
                    this.Close(); // Formu kapat
                    var clientForm = new Login();
                    clientForm.Show();

                }
                else
                {
                    MessageBox.Show("Kayıt sırasında bir hata oluştu. Lütfen tekrar deneyin.");
                }
            }

            private bool IsEmailRegistered(string email)
            {
                string emailKey = email.Replace(".", ",");
                string url = $"{firebaseUrl}/users.json";
                string json = GetJson(url);

                if (string.IsNullOrEmpty(json) || json == "null")
                    return false;

                var users = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(json);
                foreach (var user in users)
                {
                    if (user.Value.email == emailKey)
                    {
                        return true; // E-posta zaten kayıtlı
                    }
                }

                return false;
            }

            private bool CreateUserRecord(string userId, string nickName, string email, string password)
            {
                string url = $"{firebaseUrl}/users/{userId}.json";
                var data = new
                {
                    nickname = nickName,
                    email = email.Replace(".", ","),
                    password = password,
                    registrationDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    announcementRead = 0 // Kullanıcı başlangıçta hiçbir duyuruyu görmedi

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
                    return null;
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

            private void RegisterForm_Load(object sender, EventArgs e)
            {
                // Form yüklendiğinde yapılacak işlemler
            }

        private void Register_Load(object sender, EventArgs e)
        {

        }

        private void Form_Closing(object sender, FormClosingEventArgs e)
        {
            //Application.Exit();
        }
    }
    }
