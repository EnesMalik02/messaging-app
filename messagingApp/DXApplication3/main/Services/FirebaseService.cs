using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace messagingApp.Services
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
            string url = $"{_firebaseUrl}/{relativeUrl}";
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "PUT";
            request.ContentType = "application/json";
            using (var sw = new StreamWriter(request.GetRequestStream()))
            {
                sw.Write(jsonBody);
            }
            request.GetResponse().Close();
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
            string url = $"{_firebaseUrl}/{relativeUrl}";
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
