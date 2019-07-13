namespace Hires.ToDo.Services
{
    public interface ISettingsService
    {
        string Language { get; }
        string Subscription { get; }
        string Region { get; }
    }
}
