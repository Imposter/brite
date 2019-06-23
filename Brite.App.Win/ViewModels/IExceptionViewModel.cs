namespace Brite.App.Win.ViewModels
{
    public interface IExceptionViewModel : ICloseableViewModel
    {
        string Message { get; }
    }
}
