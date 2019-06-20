using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brite.RPC.Helpers;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using Newtonsoft.Json;

namespace Brite.RPC
{
    public sealed class RpcService : IDisposable
    {
        public const int GetTargetTimeout = 1000; // ms

        private readonly AppServiceConnection _connection;
        private readonly Guid _id;
        private readonly Dictionary<string, object> _objects;
        private bool _running;

        public RpcService(AppServiceConnection connection, Guid id)
        {
            _connection = connection;
            _id = id;
            _objects = new Dictionary<string, object>();
            _running = false;
        }

        public void Start()
        {
            if (_running)
                return;

            // Add handler
            _connection.RequestReceived += ConnectionOnRequestReceived;

            _running = true;
        }

        public void Stop()
        {
            if (!_running)
                return;

            // Remove handler
            _connection.RequestReceived -= ConnectionOnRequestReceived;

            _running = false;
        }

        public void Dispose()
        {
            if (_running)
                _connection.RequestReceived -= ConnectionOnRequestReceived;
        }

        #region Server

        public void AddObject(string name, object obj)
        {
            _objects.Add(name, obj);
        }

        private async void ConnectionOnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            // Get message deferral
            var deferral = args.GetDeferral();

            try
            {
                // Parse request
                var req = args.Request;
                var reqSet = args.Request.Message;
                var source = new Guid((string)reqSet["RPC_Source"]);
                var command = (RpcCommand)reqSet["RPC_Command"];

                if (reqSet.ContainsKey("RPC_Target"))
                {
                    var target = new Guid((string)reqSet["RPC_Target"]);
                    if (target != _id)
                        return;
                }

                var resSet = new ValueSet();
                if (command == RpcCommand.GetTarget)
                {
                    resSet["ID"] = _id;
                }
                else if (command == RpcCommand.GetObjects)
                {
                    resSet["Objects"] = _objects.Keys.ToList();
                }
                else if (command == RpcCommand.GetObject)
                {
                    var name = (string)reqSet["ObjectName"];
                    if (_objects.ContainsKey(name))
                    {
                        resSet["Object"] = name;
                    }
                    else
                    {
                        resSet["Error"] = $"Object \"{name}\" not found";
                    }
                }
                else if (command == RpcCommand.GetObjectProperty)
                {
                    var name = (string)reqSet["ObjectName"];
                    var propName = (string)reqSet["PropertyName"];
                    if (_objects.ContainsKey(name))
                    {
                        var obj = _objects[name];
                        var type = obj.GetType();
                        var prop = type.GetProperty(propName);
                        if (prop != null)
                        {
                            var value = prop.GetValue(obj);
                            resSet["Value"] = JsonConvert.SerializeObject(value);
                        }
                        else
                        {
                            resSet["Error"] = $"Property \"{propName}\" not found on object \"{name}\" of type {type}";
                        }
                    }
                }
                else if (command == RpcCommand.SetObjectProperty)
                {
                    var name = (string)reqSet["ObjectName"];
                    var propName = (string)reqSet["PropertyName"];
                    var propValue = JsonConvert.DeserializeObject((string)reqSet["PropertyValue"]);
                    if (_objects.ContainsKey(name))
                    {
                        var obj = _objects[name];
                        var type = obj.GetType();
                        var prop = type.GetProperty(propName);
                        if (prop != null)
                        {
                            prop.SetValue(obj, propValue);
                        }
                        else
                        {
                            resSet["Error"] = $"Property \"{propName}\" not found on object \"{name}\" of type {type}";
                        }
                    }
                }
                else if (command == RpcCommand.CallObjectMethod)
                {
                    var name = (string)reqSet["ObjectName"];
                    var methodName = (string)reqSet["MethodName"];
                    var methodArgs = JsonConvert.DeserializeObject<object[]>((string)reqSet["MethodArguments"]);
                    if (_objects.ContainsKey(name))
                    {
                        var obj = _objects[name];
                        var type = obj.GetType();
                        var method = type.GetMethod(methodName);
                        if (method != null)
                        {
                            // https://github.com/SvenEV/UWP-Networking-Essentials/blob/master/UwpNetworkingEssentials/Rpc/RpcHelper.cs
                            dynamic res = method.Invoke(obj, methodArgs);
                            if (res is Task task)
                            {
                                await task;

                                if (res.GetType().GetGenericTypeDefinition() == typeof(Task<>) &&
                                    res.GetType().GetGenericArguments()[0].Name != "VoidTaskResult")
                                    res = ((Task<object>)task).Result;
                                else res = null;
                            }

                            resSet["Result"] = JsonConvert.SerializeObject(res);
                        }
                        else
                        {
                            resSet["Error"] = $"Method \"{methodName}\" not found on object \"{name}\" of type {type}";
                        }
                    }
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }

                // Send response
                resSet["RPC_Source"] = _id.ToString();
                resSet["RPC_Command"] = (int)command;
                await req.SendResponseAsync(resSet).AsTask();
            }
            finally
            {
                // Release
                deferral.Complete();
            }
        }

        #endregion

        #region Client

        public async Task<RpcTarget> GetTargetAsync(Guid id, int timeout = GetTargetTimeout)
        {
            await SendMessageAsync(RpcCommand.GetTarget, new Dictionary<string, object> { { "RPC_Target", id } }).TimeoutAfter(TimeSpan.FromMilliseconds(timeout));
            return new RpcTarget(this, id);
        }

        internal async Task<Dictionary<string, object>> SendMessageAsync(RpcCommand command, Dictionary<string, object> args)
        {
            var reqSet = new ValueSet
            {
                ["RPC_Source"] = _id.ToString(),
                ["RPC_Command"] = (int)command
            };

            foreach (var p in args)
                reqSet.Add(p);

            var response = await _connection.SendMessageAsync(reqSet);
            if (response.Status != AppServiceResponseStatus.Success)
                throw new RpcException($"Failed to receive response for message {response.Status}");

            return response.Message.ToDictionary(p => p.Key, p => p.Value);
        }

        // TODO: How to share code between .NET Core and .NET Framework
        internal async Task<Dictionary<string, object>> SendMessageAsync(Guid target, RpcCommand command, Dictionary<string, object> args)
        {
            var reqSet = new ValueSet
            {
                ["RPC_Source"] = _id.ToString(),
                ["RPC_Target"] = target.ToString(),
                ["RPC_Command"] = (int)command
            };

            foreach (var p in args)
                reqSet.Add(p);

            var response = await _connection.SendMessageAsync(reqSet);
            if (response.Status != AppServiceResponseStatus.Success)
                throw new RpcException($"Failed to receive response for message {response.Status}");

            return response.Message.ToDictionary(p => p.Key, p => p.Value);
        }

        #endregion
    }
}
