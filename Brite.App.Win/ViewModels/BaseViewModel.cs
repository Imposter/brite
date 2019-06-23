using System.Reactive.Disposables;
using ReactiveUI;

namespace Brite.App.Win.ViewModels
{
    public abstract class BaseViewModel : ReactiveObject, IViewModel
    {
        private readonly CompositeDisposable _disposable;

        protected BaseViewModel()
        {
            _disposable = new CompositeDisposable();
        }

        public virtual void Dispose()
        {
            _disposable.Dispose();
        }

        public static implicit operator CompositeDisposable(BaseViewModel disposable)
        {
            return disposable._disposable;
        }
    }
}
