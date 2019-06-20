using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brite.Win.App.Daemon.Interface.Implementation.Update
{
    internal class Update001 : IUpdate001
    {
        public async Task<string> GetNameAsync()
        {
            return "Hello world";
        }
    }
}
