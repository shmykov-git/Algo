using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ViewMotion
{
    /// <summary>
    /// Interaction logic for Viewer.xaml
    /// </summary>
    public partial class Viewer : Window
    {
        private ViewerModel viewerModel;

        public Viewer()
        {
            InitializeComponent();

            this.MouseWheel += (o, a) =>
            {
                viewerModel?.Wheel(a.Delta);
            };

            Point? p0 = null;

            this.MouseMove += (o, a) =>
            {
                if (a.LeftButton != MouseButtonState.Pressed)
                {
                    p0 = null;

                    return;
                }

                if (p0 == null)
                {
                    p0 = a.GetPosition(AnimatedObject);
                    
                    return;
                }

                var size = Math.Min(this.Width, this.Height);

                var p = a.GetPosition(AnimatedObject);
                var move = p - p0.Value;

                if (a.RightButton == MouseButtonState.Pressed)
                {
                    viewerModel?.Move(move.X / size, move.Y / size);
                }
                else
                {
                    viewerModel?.Rotate(move.X / size, move.Y / size);
                }
                p0 = p;
            };

            this.SizeChanged += (o, a) =>
            {
                var (w, h) = (a.NewSize.Width, a.NewSize.Height);

                this.Width = Canvas.Width = AnimatedObject.Width = w;
                this.Height = Canvas.Height = AnimatedObject.Height = h;
            };

            this.DataContextChanged += (o, a) =>
            {
                viewerModel = DataContext as ViewerModel;

                if (viewerModel == null)
                    return;

                foreach (var element in viewerModel.Lights)
                {
                    AnimatedObject.Children.Add(element);
                }

                AnimatedObject.Children.Add(viewerModel.Model);

                viewerModel.UpdateModel = model => AnimatedObject.Children[^1] = model;
            };
        }
    }
}
