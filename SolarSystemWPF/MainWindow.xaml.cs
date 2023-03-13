using SpaceSim;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Resources;
using System.Windows.Threading;

namespace SolarSystemWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SpaceSystem solarSystem;
        private SpaceObject? focusObject = null;
        private ScaleTransform scaleTransformer = new();
        private TranslateTransform translateTransformer = new();

        public MainWindow()
        {
            InitializeComponent();

            Uri uri = new Uri("/solarsystem.csv", UriKind.Relative);
            StreamResourceInfo info = Application.GetResourceStream(uri);

            draw.Background = new SolidColorBrush(Colors.Black);

            TransformGroup transformGroup = new();
            transformGroup.Children.Add(scaleTransformer);
            transformGroup.Children.Add(translateTransformer);
            draw.RenderTransform = transformGroup;

            solarSystem = new SpaceSystem(info.Stream);
            solarSystem.SetupDrawable(draw);

            DispatcherTimer t = new()
            {
                Interval = TimeSpan.FromMilliseconds(6)
            };

            t.Tick += Tick;
            t.Start();
        }

        private void MoveWithObject()
        {
            if (focusObject == null)
                return;

            Point objectPosition = focusObject.GetScaledPosition(solarSystem.time);

            translateTransformer.Y = -objectPosition.Y * 2 - draw.RenderSize.Height / 2;
            translateTransformer.X = -objectPosition.X * 2 - draw.RenderSize.Width / 2;
        }

        private void changeFocus(object sender, MouseButtonEventArgs e)
        {
            Point clickPos = e.GetPosition(draw);

            Boolean found = false;

            foreach (SpaceObject obj in solarSystem.SpaceObjects)
            {
                if (obj.WithinBounds(draw, clickPos, solarSystem.time))
                {
                    focusObject = obj;
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                focusObject = null;
            }
        }

        private void Tick(object? sender, EventArgs e)
        {
            solarSystem.Advance(0.1);
            solarSystem.Render(draw);

            if (focusObject != null)
            {
                scaleTransformer.ScaleX = 2;
                scaleTransformer.ScaleY = 2;
                MoveWithObject();
            }
            else
            {
                scaleTransformer.ScaleX = 1;
                scaleTransformer.ScaleY = 1;
                translateTransformer.X = 0;
                translateTransformer.Y = 0;
            }

            draw.InvalidateVisual();
        }
    }
}
