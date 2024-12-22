using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace messagingApp.Services
{
    public class ConversationService
    {
        private readonly FirebaseManager _firebase;

        public ConversationService(FirebaseManager firebase)
        {
            _firebase = firebase;
        }

        // Mesaj Gönder
        public void SendMessage(string conversationId, string senderUserId, string message)
        {
            // 1) Mesaj obje
            var msgObj = new
            {
                sender = senderUserId,
                text = message,
                //timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            string msgJson = JsonConvert.SerializeObject(msgObj);

            // 2) Post et
            _firebase.PostJson($"messages/{conversationId}.json", msgJson);

            // 3) lastMessage & lastUpdate alanlarını güncelle
            var convUpdate = new
            {
                lastMessage = message,
                //lastUpdate = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            string convUpdateJson = JsonConvert.SerializeObject(convUpdate);
            _firebase.PatchJson($"conversations/{conversationId}.json", convUpdateJson);
        }

        public Dictionary<string, dynamic> GetMessages(string conversationId)
        {
            string msgData = _firebase.GetJson($"messages/{conversationId}.json");
            if (string.IsNullOrEmpty(msgData) || msgData == "null") return null;

            return JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(msgData);
        }

        public void DeleteConversation(string conversationId, string currentUserId, string otherUserId)
        {
            // Sohbeti tamamen sil
            _firebase.DeleteJson($"conversations/{conversationId}.json");

            // userConversations altından da sil
            _firebase.DeleteJson($"userConversations/{currentUserId}/{conversationId}.json");
            _firebase.DeleteJson($"userConversations/{otherUserId}/{conversationId}.json");
        }

        public void AddConversationToUser(string userId, string conversationId)
        {
            _firebase.PutJson($"userConversations/{userId}/{conversationId}.json", "\"true\"");
        }

        public string GetJson(string relativePath)
        {
            return _firebase.GetJson(relativePath);
        }

        public void PutJson(string relativePath, string jsonBody)
        {
            _firebase.PutJson(relativePath, jsonBody);
        }

        public void PostJson(string relativePath, string jsonBody)
        {
            _firebase.PostJson(relativePath, jsonBody);
        }

        public void PatchJson(string relativePath, string jsonBody)
        {
            _firebase.PatchJson(relativePath, jsonBody);
        }

        public void DeleteJson(string relativePath)
        {
            _firebase.DeleteJson(relativePath);
        }
    }
}
