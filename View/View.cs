using Model;
using Model.Extensions;
using System.Drawing;
using System.Windows.Forms;
using View.Extensions;

namespace View
{
    public partial class View : Form, IView
    {
        private Poligon basePoligon;

        public View()
        {
            InitializeComponent();
        }

        public void DrawPoligon(Poligon poligon)
        {
            this.basePoligon = poligon;

            Refresh();
        }

        private void View_Resize(object sender, System.EventArgs e)
        {
            Refresh();
        }

        private void View_Paint(object sender, PaintEventArgs e)
        {
            if (basePoligon == null)
                return;

            var g = e.Graphics;
            Model.Size size = (Width, Height);
            var poligon = basePoligon.Scale(size).MirrorY(size);

            DrawLines(g, poligon, Pens.Red);
            DrawPoints(g, poligon, Brushes.DodgerBlue);
        }

        private void DrawLines(Graphics g, Poligon poligon, Pen pen)
        {
            foreach (var line in poligon.Lines)
            {
                g.DrawLine(pen, line.A.ToPoint(), line.B.ToPoint());
            }
        }
        private void DrawPoints(Graphics g, Poligon poligon, Brush brush)
        {
            foreach (var p in poligon.Points)
            {
                g.FillEllipse(brush, p.ToRectangle((10, 10)));
            }
        }
    }
}
