using System.Windows.Controls;

namespace Brite.App.Win.ViewModels
{
    public interface IChildViewModel : IViewModel
    {
        string Title { get; }
        Control Icon { get; }
    }
}
