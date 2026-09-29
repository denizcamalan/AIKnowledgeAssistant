namespace AIKnowledgeAssistant.Application.Chat.PromptLab;

public static class PromptLabTemplates
{
    public const string DefaultGroundingContext =
        """
        Şirket uzaktan çalışma politikası:
        - Haftada en fazla 2 gün uzaktan çalışılabilir.
        - Uzaktan günler salı ve perşembe ile sınırlıdır.
        - Ofis saatleri 09:00–18:00 arasındadır.
        """;

    public static ChatPrompt Baseline(string question) => new(question.Trim());

    public static ChatPrompt Constrained(string question) =>
        new(
            Message: question.Trim(),
            SystemMessage:
            """
            Sen kısa ve net cevap veren bir asistansın.
            Türkçe yanıtla. En fazla 3 cümle kullan.
            Bilmediğin konularda uydurma; emin değilsen belirt.
            """,
            FewShotExamples:
            [
                new ChatMessage("user", "Fransa'nın başkenti neresi?"),
                new ChatMessage("assistant", "Fransa'nın başkenti Paris'tir."),
            ]);

    public static ChatPrompt Grounded(string question, string groundingContext) =>
        new(
            Message:
            $"""
            Bağlam:
            ---
            {groundingContext.Trim()}
            ---

            Soru: {question.Trim()}

            Yalnızca bağlamdaki bilgilere dayanarak yanıtla.
            """,
            SystemMessage:
            """
            Verilen bağlam dışına çıkma.
            Cevap bağlamda yoksa yalnızca "Bağlamda bulamadım." de.
            """);
}
