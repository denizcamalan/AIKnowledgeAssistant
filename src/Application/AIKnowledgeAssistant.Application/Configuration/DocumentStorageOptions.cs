namespace AIKnowledgeAssistant.Application.Configuration;

public sealed class DocumentStorageOptions
{
    public const string SectionName = "DocumentStorage";

    public string RootPath { get; set; } = "uploads";

    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024;

    public string[] AllowedExtensions { get; set; } = [".pdf", ".txt"];
}
