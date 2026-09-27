namespace AIKnowledgeAssistant.Domain.Documents;

public sealed class DuplicateDocumentException : Exception
{
    public DuplicateDocumentException(string fileName)
        : base($"A document with file name '{fileName}' already exists.")
    {
        FileName = fileName;
    }

    public string FileName { get; }
}
