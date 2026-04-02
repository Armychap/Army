using System.Collections.Generic;

namespace Proxy
{
    public interface IDocumentService
    {
        string ReadDocument(int docId);
        void WriteDocument(int docId, string content);
        void DeleteDocument(int docId);
        void CreateDocument(string title, string content);
        List<string> GetAllDocuments();
        string GetDocumentInfo(int docId);
    }
}