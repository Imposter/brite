using System.Threading.Tasks;
using Brite.Utility.IO;

namespace Brite.Micro.Formats
{
    public interface IBinarySerializer
    {
        Task<MemoryStream> SerializeAsync(IStream stream);
        Task<MemoryStream> DeserializeAsync(IStream stream);
    }
}
