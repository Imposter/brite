using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Interop;

namespace Brite.App.Win.Converters
{
    // From: https://stackoverflow.com/questions/3899674/how-to-get-the-height-of-the-title-bar-of-the-main-application-windows
    public class PixelToScreenConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var pixels = (double)value;
            var horizontal = Equals(parameter, true);

            var points = 0d;

            // NOTE: Ideally, we would get the source from a visual:
            // source = PresentationSource.FromVisual(visual);
            //
            using (var source = new HwndSource(new HwndSourceParameters()))
            {
                var matrix = source.CompositionTarget?.TransformToDevice;
                if (matrix.HasValue)
                {
                    points = pixels * (horizontal ? matrix.Value.M11 : matrix.Value.M22);
                }
            }

            return points;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
