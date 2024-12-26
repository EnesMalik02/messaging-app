using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace main.Services
{
    public class FirebaseManager
    {
        private readonly string _firebaseUrl;

        public FirebaseManager(string firebaseUrl)
        {
            _firebaseUrl = firebaseUrl;
        }

        public string GetJson(string relativeUrl)
        {
            // relativeUrl: örnek "users.json" veya "userConversations/user123"
            string url = $"{_firebaseUrl}/{relativeUrl}";
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "application/json";
            try
            {
                var response = (HttpWebResponse)request.GetResponse();
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

        public void PutJson(string relativeUrl, string jsonBody)
        {
            string url = $"{_firebaseUrl}/{relativeUrl}.json";
            Console.WriteLine($"PUT Request URL: {url}");
            Console.WriteLine($"PUT Request Body: {jsonBody}");

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "PUT";
            request.ContentType = "application/json";

            try
            {
                using (var sw = new StreamWriter(request.GetRequestStream()))
                {
                    sw.Write(jsonBody);
                }
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    Console.WriteLine($"Response Status: {response.StatusCode}");
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    using (var sr = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        string responseText = sr.ReadToEnd();
                        Console.WriteLine($"Error Response: {responseText}");

                        // Firebase'den dönen detaylı hata mesajı
                        throw new Exception($"Firebase PUT işlemi başarısız oldu. Hata: {responseText}");
                    }
                }
                throw new Exception("Firebase PUT işlemi başarısız oldu. Hata: " + ex.Message);
            }
        }




        public void PostJson(string relativeUrl, string jsonBody)
        {
            string url = $"{_firebaseUrl}/{relativeUrl}";
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json";
            using (var sw = new StreamWriter(request.GetRequestStream()))
            {
                sw.Write(jsonBody);
            }
            request.GetResponse().Close();
        }

        public void PatchJson(string relativeUrl, string jsonBody)
        {
            string url = $"{_firebaseUrl}/{relativeUrl}.json";
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "PATCH";
            request.ContentType = "application/json";
            using (var sw = new StreamWriter(request.GetRequestStream()))
            {
                sw.Write(jsonBody);
            }
            request.GetResponse().Close();
        }

        public void DeleteJson(string relativeUrl)
        {
            string url = $"{_firebaseUrl}/{relativeUrl}";
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "DELETE";
            try
            {
                request.GetResponse().Close();
            }
            catch
            {
                // Silme hatasını yakalayabilirsiniz
            }
        }

        public async Task<string> GetJsonAsync(string relativeUrl)
        {
            string url = $"{_firebaseUrl}/{relativeUrl}";
            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync();
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}
