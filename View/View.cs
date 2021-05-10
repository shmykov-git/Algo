using Model;
using Model.Extensions;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using View.Extensions;

namespace View
{
    public partial class View : Form, IView
    {
        private Shape2 baseShape;
        private bool isValid;
        private int debugCount = 0;

        public View()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
        }

        public void DrawPolygon(bool isFilled, Shape2 shape)
        {
            this.baseShape = shape;
            this.isValid = isFilled;

            Refresh();
        }

        private void View_Resize(object sender, System.EventArgs e)
        {
            Refresh();
        }

        private void pnlCanvas_Paint(object sender, PaintEventArgs e)
        {
            if (baseShape == null)
                return;

            ShowShape();

            var g = e.Graphics;
            this.BackColor = Color.White;
            Vector2 size = (pnlCanvas.Width, pnlCanvas.Height);
            var shape = baseShape.Move((0.5, 0.5)).Scale(size).MirrorY(size);

            if (isValid)
            {
                Vector2 halfSize = size / 2;
                var border = halfSize * 0.025;

                var leftTopShape = shape.Mult(0.45).Move(border);
                var rightTopShape = shape.Mult(0.45).Move((halfSize.X, 0)).Move(border);
                var leftButtomShape = shape.Mult(0.45).Move((0, halfSize.Y)).Move(border);
                var rightButtomShape = shape.Mult(0.45).Move(halfSize).Move(border);

                DrawLines(g, leftTopShape, Pens.Black);
                DrawPoints(g, leftTopShape, Brushes.DodgerBlue);

                DrawNet(g, rightTopShape, isValid ? Pens.Green : Pens.Red);
                DrawPoints(g, rightTopShape, Brushes.DodgerBlue);

                FillPolygon(g, leftButtomShape, shape.Convexes.Skip(debugCount).First(), Brushes.Gray);
                DrawLines(g, leftButtomShape, Pens.Black);
                DrawPoints(g, leftButtomShape, Brushes.DodgerBlue);
                DrawPointLabels(g, leftButtomShape, Brushes.Blue);

                FillNet(g, rightButtomShape, isValid ? Brushes.Green : Brushes.Red);
            }
            else
            {
                var border = size * 0.025;
                shape = shape.Mult(0.95).Move(border);
                DrawLines(g, shape, Pens.Black);
                DrawPoints(g, shape, Brushes.DodgerBlue);
                DrawPointLabels(g, shape, Brushes.Blue);
            }
        }

        private void FillNet(Graphics g, Shape2 shape, Brush brush)
        {
            foreach (var convex in shape.Convexes)
            {
                g.FillPolygon(brush, convex.Select(i => shape[i].ToPoint()).ToArray());
            }
        }

        private void FillPolygon(Graphics g, Shape2 shape, int[] part, Brush brush)
        {
            g.FillPolygon(brush, part.Select(i=> shape[i].ToPoint()).ToArray());
        }

        private void DrawNet(Graphics g, Shape2 shape, Pen pen)
        {
            foreach (var edge in shape.Convexes.SelectMany(convex => convex.SelectCirclePair((i, j)=>(i,j))))
            {
                var a = shape[edge.i];
                var b = shape[edge.j];

                g.DrawLine(pen, a.ToPoint(), b.ToPoint());
            }
        }

        private void DrawLines(Graphics g, Shape2 polygon, Pen pen)
        {
            foreach (var line in polygon.Lines)
            {
                g.DrawLine(pen, line.A.ToPoint(), line.B.ToPoint());
            }
        }
        private void DrawPointLabels(Graphics g, Shape2 polygon, Brush brush)
        {
            var font = new Font("Arial", 10);
            for (var i=0; i<polygon.Points.Length; i++)
            {
                var p = polygon[i];
                g.DrawString(i.ToString(), font, brush, p.ToPoint());
            }
        }
        private void DrawPoints(Graphics g, Shape2 polygon, Brush brush)
        {
            foreach (var p in polygon.Points)
            {
                g.FillEllipse(brush, p.ToRectangle((10, 10)));
            }
        }

        private void ShowShape()
        {
            lblCount.Text = debugCount.ToString();
            if (baseShape.Convexes.Length > 0)
                lblDebugInfo.Text = string.Join(", ", baseShape.Convexes[debugCount].Select(v => v.ToString()));
            else
                lblDebugInfo.Text = "";
        }

        private void btnMinus_Click(object sender, System.EventArgs e)
        {
            debugCount = debugCount == 0 ? 0 : debugCount - 1;
            Refresh();
        }

        private void btnPlus_Click(object sender, System.EventArgs e)
        {
            debugCount = debugCount == baseShape.Convexes.Length-1 ? baseShape.Convexes.Length - 1 : debugCount + 1;
            Refresh();
        }
    }
}
