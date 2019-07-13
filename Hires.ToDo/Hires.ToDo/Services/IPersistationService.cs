using System.Threading.Tasks;

namespace Hires.ToDo.Services
{
    public interface IPersistationService
    {
        Task<T> LoadData<T>(string fileName) where T : class;
        Task SaveData<T>(T data, string fileName) where T : class;
    }
}
