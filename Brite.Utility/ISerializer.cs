using System.Threading.Tasks;
using Brite.Utility.IO;

namespace Brite.Utility
{
    public interface ISerializer
    {
        Task SerializeAsync<T>(IStream stream, T obj);
        Task<T> DeserializeAsync<T>(IStream stream);
    }
}
