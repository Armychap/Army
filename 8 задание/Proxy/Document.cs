namespace Proxy
{
    public class Document
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Department { get; set; }

        public Document(int id, string title, string content, string department)
        {
            Id = id;
            Title = title;
            Content = content;
            Department = department;
        }
    }
}