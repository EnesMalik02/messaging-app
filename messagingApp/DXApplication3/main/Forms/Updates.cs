using main.Services;
using System;
using System.Windows.Forms;

namespace main.Forms
{
    public partial class Updates : DevExpress.XtraEditors.XtraForm
    {
        private readonly FirebaseManager _firebaseManager; // Firebase bağlantısı için
        private string _firebaseUrl = "https://messaging-app-11f5f-default-rtdb.europe-west1.firebasedatabase.app";

        public Updates()
        {
            InitializeComponent();
            _firebaseManager = new FirebaseManager(_firebaseUrl); // FirebaseManager'ı başlat
        }

        private void Updates_Load(object sender, EventArgs e)
        {
            LoadUpdates(); // Form yüklendiğinde güncellemeleri yükle
        }

        private void LoadUpdates()
        {
            try
            {
                // Veritabanından metni çek
                string updatesJson = _firebaseManager.GetJson("updates.json"); // Örnek URL: updates.json

                if (!string.IsNullOrEmpty(updatesJson) && updatesJson != "null")
                {
                    dynamic updates = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(updatesJson);

                    Console.WriteLine(updates);
                    string date = updates.date;

                    // Eğer RichTextBox kullanıyorsanız
                    richTextBox1.Text = updates.message ?? "Güncelleme bulunamadı."; // 'message' anahtarına göre güncelleme içeriğini çek

                    // Eğer Label kullanıyorsanız
                    label2.Text = $"Son Güncelleme {date}" ?? "Güncelleme bulunamadı.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Güncellemeleri yüklerken bir hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
