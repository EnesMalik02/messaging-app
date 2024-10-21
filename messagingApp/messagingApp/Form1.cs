using System;
using System.Windows.Forms;


namespace messagingApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Kullanıcı giriş yapmış mı kontrol ediliyor
            if (!UserLoggedIn())
            {
                var loginForm = new LoginForm();
                loginForm.Show();
                this.Hide();
            }
        }

        private bool UserLoggedIn()
        {
            return true;
            // Kullanıcı ID kontrolü
        }
    }
}
