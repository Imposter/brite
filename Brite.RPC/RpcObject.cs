using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using Brite.RPC.Helpers;
using Newtonsoft.Json;

/*
- RpcService() : IDisposable
 - AppServiceConnection connection
 - ConnectAsync()
 - DisconnectAsync()
 - AddObjectAsync(string name, RpcObject obj)
 - GetTargetAsync(string id): Task<RpcTarget>
 - SendMessageAsync(RpcMessage request): Task<RpcMessage>

- RpcTarget(string id)
 - RpcTransport transport
 - GetObjectAsync(string name): Task<RpcObject>
 - GetObjectsAsync(): Task<RpcObject>

- RpcObject(RpcTransport transport) : IDynamicMetaObjectProvider
 - RpcTransport transport
 - Call/TryMember, etc.
*/

namespace Brite.RPC
{
    public sealed class RpcObject
    {
        private readonly RpcService _service;
        private readonly Guid _targetId;
        private readonly string _name;

        public RpcObject(RpcService service, Guid targetId, string name)
        {
            _service = service;
            _targetId = targetId;
            _name = name;
        }

        public bool TryGetMember(GetMemberBinder binder, out object result)
        {
            try
            {
                var res = _service.SendMessageAsync(_targetId, RpcCommand.GetObjectProperty, new Dictionary<string, object>
                {
                    { "ObjectName", _name },
                    { "PropertyName", binder.Name }
                }).WaitForResult();

                if (res.ContainsKey("Error"))
                    throw new Exception((string)res["Error"]);

                result = res["Value"];
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        public bool TrySetMember(SetMemberBinder binder, object value)
        {
            try
            {
                var res = _service.SendMessageAsync(_targetId, RpcCommand.SetObjectProperty, new Dictionary<string, object>
                {
                    { "ObjectName", _name },
                    { "PropertyName", binder.Name },
                    { "PropertyValue", JsonConvert.SerializeObject(value) }
                }).WaitForResult();

                if (res.ContainsKey("Error"))
                    throw new Exception((string)res["Error"]);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            try
            {
                var res = _service.SendMessageAsync(_targetId, RpcCommand.CallObjectMethod, new Dictionary<string, object>
                {
                    { "ObjectName", _name },
                    { "MethodName", binder.Name },
                    { "MethodArguments", JsonConvert.SerializeObject(args) }
                }).WaitForResult();

                if (res.ContainsKey("Error"))
                    throw new Exception((string)res["Error"]);

                result = res["Result"];
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        public DynamicMetaObject GetMetaObject(Expression parameter)
        {
            throw new NotImplementedException();
        }
    }
}
