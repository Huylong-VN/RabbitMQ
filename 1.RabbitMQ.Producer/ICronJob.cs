using Hangfire;

[AutomaticRetry(Attempts = 0)]
public interface ICronJob
{
    Task<string> Run();
}