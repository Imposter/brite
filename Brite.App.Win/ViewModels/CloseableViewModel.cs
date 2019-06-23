using Brite.App.Win.Commands;
using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Brite.App.Win.ViewModels
{
    public abstract class CloseableViewModel : BaseViewModel, ICloseableViewModel
    {
        public virtual bool CanConfirm => false;
        public virtual bool CanDeny => false;

        private readonly Subject<Unit> _confirmed;
        private readonly Subject<Unit> _denied;
        private readonly Subject<Unit> _closed;

        public IObservable<Unit> Confirmed => _confirmed;
        public IObservable<Unit> Denied => _denied;
        public IObservable<Unit> Closed => _closed;

        public ReactiveCommand CancelCommand { get; }
        public ReactiveCommand ConfirmCommand { get; }
        public ReactiveCommand DenyCommand { get; }

        public CloseableViewModel()
        {
            _confirmed = new Subject<Unit>()
                .DisposeWith(this);

            _denied = new Subject<Unit>()
                .DisposeWith(this);

            _closed = new Subject<Unit>()
                .DisposeWith(this);

            CancelCommand = ReactiveCommand.Create()
                .DisposeWith(this);

            CancelCommand.Subscribe(x => _closed.OnNext(Unit.Default))
                .DisposeWith(this);

            ConfirmCommand = ReactiveCommand.Create(Observable.Return(CanConfirm))
              .DisposeWith(this);

            ConfirmCommand.Subscribe(x =>
                {
                    _confirmed.OnNext(Unit.Default);
                    _closed.OnNext(Unit.Default);
                })
                .DisposeWith(this);

            DenyCommand = ReactiveCommand.Create(Observable.Return(CanDeny))
                .DisposeWith(this);

            DenyCommand.Subscribe(x =>
                {
                    _denied.OnNext(Unit.Default);
                    _closed.OnNext(Unit.Default);
                })
                .DisposeWith(this);
        }
    }
}
