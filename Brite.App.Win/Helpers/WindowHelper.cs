using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace Brite.App.Win.Helpers
{
    public static class WindowHelper
    {
        public static readonly double WindowCaptionHeight = SystemParameters.WindowResizeBorderThickness.Top * 2 + SystemParameters.CaptionHeight + SystemParameters.BorderWidth;

        private static double CalculateWindowCaptionHeight(double value)
        {
            var pixels = value;

            var points = 0d;

            // NOTE: Ideally, we would get the source from a visual:
            // source = PresentationSource.FromVisual(visual);
            //
            using (var source = new HwndSource(new HwndSourceParameters()))
            {
                var matrix = source.CompositionTarget?.TransformToDevice;
                if (matrix.HasValue)
                {
                    points = pixels * matrix.Value.M22;
                }
            }

            return points;
        }
    }
}
