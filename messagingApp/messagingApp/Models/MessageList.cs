namespace messagingApp.Models
{
    public class MessageList
    {
        public string Text { get; set; }
        public object Tag { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }
}
