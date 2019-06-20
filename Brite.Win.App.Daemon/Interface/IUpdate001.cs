using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brite.Win.App.Daemon.Interface
{
    public interface IUpdate001
    {
        // TODO: See if we can make async funcs work
        Task<string> GetNameAsync();
    }
}
