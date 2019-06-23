using System;
using System.Reactive.Disposables;

namespace Brite.App.Win.Models
{
    public abstract class DisposableObject : IDisposable
    {
        private readonly CompositeDisposable _disposable;

        protected DisposableObject()
        {
            _disposable = new CompositeDisposable();
        }

        public virtual void Dispose()
        {
            _disposable.Dispose();
        }

        public static implicit operator CompositeDisposable(DisposableObject disposable)
        {
            return disposable._disposable;
        }
    }
}
