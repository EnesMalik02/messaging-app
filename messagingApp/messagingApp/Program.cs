using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace messagingApp
{
    static class Program
    {
        /// <summary>
        /// Uygulamanın ana girdi noktası.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // İlk LoginForm örneği
            LoginForm login1 = new LoginForm();
            // İkinci LoginForm örneği
            LoginForm login2 = new LoginForm();

            // Her iki formu da göster
            login1.Show();
            login2.Show();

            //// Bu noktada henüz Application.Run() ile bir form belirtilmedi.
            // Fakat Application.Run() parametresiz çağırılırsa, açık formlar olduğu sürece döngü devam eder.
            Application.Run();
        }

    }
}
