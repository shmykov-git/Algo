using Model;
using Model.Extensions;
using System.Drawing;
using System.Windows.Forms;
using View.Extensions;

namespace View
{
    public partial class View : Form, IView
    {
        private PoligonInfo basePoligonInfo;

        public View()
        {
            InitializeComponent();
        }

        public void DrawPoligon(PoligonInfo poligonInfo)
        {
            this.basePoligonInfo = poligonInfo;

            Refresh();
        }

        private void View_Resize(object sender, System.EventArgs e)
        {
            Refresh();
        }

        private void View_Paint(object sender, PaintEventArgs e)
        {
            if (basePoligonInfo == null)
                return;

            var g = e.Graphics;
            this.BackColor = Color.White;
            Model.Size size = (Width, Height);
            var poligon = basePoligonInfo.Poligon.Scale(size).MirrorY(size);

            if (basePoligonInfo.IsFilled)
            {
                Model.Size halfSize = size / 2;

                var leftTopInfo = basePoligonInfo.ModifyPoligon(poligon.Mult(0.5));
                var rightTopInfo = basePoligonInfo.ModifyPoligon(poligon.Mult(0.5).Move((halfSize.Width, 0)));
                var leftButtomInfo = basePoligonInfo.ModifyPoligon(poligon.Mult(0.5).Move((0, halfSize.Height)));
                var rightButtomInfo = basePoligonInfo.ModifyPoligon(poligon.Mult(0.5).Move(halfSize));

                DrawLines(g, leftTopInfo.Poligon, Pens.Black);
                DrawPoints(g, leftTopInfo.Poligon, Brushes.DodgerBlue);

                DrawNet(g, rightTopInfo, rightTopInfo.IsValid ? Pens.Green : Pens.Red);
                DrawPoints(g, rightTopInfo.Poligon, Brushes.DodgerBlue);

                FillNet(g, leftButtomInfo, leftButtomInfo.IsValid ? Brushes.Green : Brushes.Brown);
                DrawNet(g, leftButtomInfo, Pens.Red);

                FillNet(g, rightButtomInfo, rightButtomInfo.IsValid ? Brushes.Green : Brushes.Red);
            }
            else
            {
                DrawLines(g, poligon, Pens.Black);
                DrawPoints(g, poligon, Brushes.DodgerBlue);
            }
        }

        private void FillNet(Graphics g, PoligonInfo info, Brush brush)
        {
            foreach (var trio in info.Trios)
            {
                var a = info.Poligon[trio.I];
                var b = info.Poligon[trio.J];
                var c = info.Poligon[trio.K];

                g.FillPolygon(brush, new[] { a.ToPoint(), b.ToPoint(), c.ToPoint()});
            }
        }

        private void DrawNet(Graphics g, PoligonInfo info, Pen pen)
        {
            foreach (var trio in info.Trios)
            {
                var a = info.Poligon[trio.I];
                var b = info.Poligon[trio.J];
                var c = info.Poligon[trio.K];

                g.DrawLine(pen, a.ToPoint(), b.ToPoint());
                g.DrawLine(pen, b.ToPoint(), c.ToPoint());
                g.DrawLine(pen, c.ToPoint(), a.ToPoint());
            }
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
