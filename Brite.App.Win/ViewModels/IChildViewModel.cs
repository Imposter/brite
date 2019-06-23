using System.Windows.Controls;

namespace Brite.App.Win.ViewModels
{
    public interface IChildViewModel : IViewModel
    {
        int Order { get; }
        string Title { get; }
        Control Icon { get; }
    }
}
