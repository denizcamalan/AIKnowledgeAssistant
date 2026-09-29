namespace AIKnowledgeAssistant.Application.Auth;

public interface IAuthService
{
    Task<LoginSuccessResult> LoginAsync(string email, string password, CancellationToken cancellationToken);
}
