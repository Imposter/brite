using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using Brite.App.Win.Models;
using Brite.App.Win.ViewModels;

namespace Brite.App.Win.Services
{
    public sealed class MessageService : DisposableObject, IMessageService
    {
        private readonly Subject<Message> _show;
        private readonly Queue<Message> _messages;
        private readonly object _sync;

        public IObservable<Message> Show => _show;

        public MessageService()
        {
            _show = new Subject<Message>()
                .DisposeWith(this);

            _messages = new Queue<Message>();
            Disposable.Create(() => _messages.Clear())
                .DisposeWith(this);

            _sync = new object();
        }

        public void Post(string title, ICloseableViewModel viewModel)
        {
            // Create new message
            var message = new Message(title, viewModel);

            message.ViewModel.Closed
                .Subscribe(o =>
                {
                    // Take next message if there is one
                    Message m = null;
                    lock (_sync)
                        if (_messages.Any())
                            m = _messages.Dequeue();

                    // Show message
                    if (m != null)
                        _show.OnNext(m);
                });

            var show = false;
            lock (_messages)
            {
                if (!_messages.Any()) show = true;
                else _messages.Enqueue(message);
            }

            if (show) _show.OnNext(message);
        }
    }
}
