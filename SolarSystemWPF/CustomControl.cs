using System.Windows;
using System.Windows.Media;

namespace SolarSystemWPF
{
    internal class CustomControl : FrameworkElement
    {
        public CustomControl()
        {
            CompositionTarget.Rendering += (s, e) => InvalidateVisual();
        }
    }
}
