using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Brite.RPC
{
    public sealed class RpcTarget
    {
        private readonly RpcService _service;
        private readonly Guid _id;

        public RpcTarget(RpcService service, Guid id)
        {
            _service = service;
            _id = id;
        }

        public async Task<RpcObject> GetObjectAsync(string name)
        {
            var res = await _service.SendMessageAsync(_id, RpcCommand.GetObject, new Dictionary<string, object> { { "ObjectName", name } });
            if (res.ContainsKey("Error")) throw new RpcException((string)res["Error"]);
            return new RpcObject(_service, _id, name);
        }

        public async Task<RpcObject[]> GetObjectsAsync()
        {
            var res = await _service.SendMessageAsync(_id, RpcCommand.GetObjects, new Dictionary<string, object>());
            var list = new List<RpcObject>();
            foreach (var obj in (List<string>)res["Objects"])
                list.Add(new RpcObject(_service, _id, obj));
            return list.ToArray();
        }
    }
}
