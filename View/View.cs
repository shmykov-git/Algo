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
        private Shape2 baseshape;
        private List<int[]> debugInfo = new List<int[]>();
        private int debugCount = 0;

        public View()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
        }

        public void DrawPolygon(Shape2 shape)
        {
            this.baseshape = shape;

            Refresh();
        }

        private void View_Resize(object sender, System.EventArgs e)
        {
            Refresh();
        }

        public void DrawDebug(int[] inds)
        {
            debugInfo.Add(inds);
        }

        private void pnlCanvas_Paint(object sender, PaintEventArgs e)
        {
            if (baseshape == null)
                return;

            ShowInfo();

            var g = e.Graphics;
            this.BackColor = Color.White;
            Model.Size size = (pnlCanvas.Width, pnlCanvas.Height);
            var polygon = baseshape.Polygon.Move((0.5, 0.5)).Scale(size).MirrorY(size);

            if (baseshape.IsFilled)
            {
                Model.Size halfSize = size / 2;
                var border = halfSize * 0.025;

                var leftTopInfo = baseshape.ModifyPolygon(polygon.Mult(0.45).Move(border));
                var rightTopInfo = baseshape.ModifyPolygon(polygon.Mult(0.45).Move((halfSize.Width, 0)).Move(border));
                var leftButtomInfo = baseshape.ModifyPolygon(polygon.Mult(0.45).Move((0, halfSize.Height)).Move(border));
                var rightButtomInfo = baseshape.ModifyPolygon(polygon.Mult(0.45).Move(halfSize).Move(border));

                DrawLines(g, leftTopInfo.Polygon, Pens.Black);
                DrawPoints(g, leftTopInfo.Polygon, Brushes.DodgerBlue);

                DrawNet(g, rightTopInfo, rightTopInfo.IsValid ? Pens.Green : Pens.Red);
                DrawPoints(g, rightTopInfo.Polygon, Brushes.DodgerBlue);

                FillPolygon(g, leftButtomInfo.Polygon, debugInfo.Skip(debugCount).First(), Brushes.Gray);
                DrawLines(g, leftButtomInfo.Polygon, Pens.Black);
                DrawPoints(g, leftButtomInfo.Polygon, Brushes.DodgerBlue);
                DrawPointLabels(g, leftButtomInfo.Polygon, Brushes.Blue);

                FillNet(g, rightButtomInfo, rightButtomInfo.IsValid ? Brushes.Green : Brushes.Red);
            }
            else
            {
                var border = size * 0.025;
                var info = baseshape.ModifyPolygon(polygon.Mult(0.95).Move(border));
                DrawLines(g, info.Polygon, Pens.Black);
                DrawPoints(g, info.Polygon, Brushes.DodgerBlue);
                DrawPointLabels(g, info.Polygon, Brushes.Blue);
            }
        }

        private void FillNet(Graphics g, Shape2 info, Brush brush)
        {
            foreach (var convex in info.Convexes)
            {
                g.FillPolygon(brush, convex.Select(i => info.Polygon[i].ToPoint()).ToArray());
            }
        }

        private void FillPolygon(Graphics g, Polygon polygon, int[] part, Brush brush)
        {
            g.FillPolygon(brush, part.Select(i=> polygon[i].ToPoint()).ToArray());
        }

        private void DrawNet(Graphics g, Shape2 info, Pen pen)
        {
            foreach (var edge in info.Convexes.SelectMany(convex => convex.SelectCirclePair((i, j)=>(i,j))))
            {
                var a = info.Polygon[edge.i];
                var b = info.Polygon[edge.j];

                g.DrawLine(pen, a.ToPoint(), b.ToPoint());
            }
        }

        private void DrawLines(Graphics g, Polygon polygon, Pen pen)
        {
            foreach (var line in polygon.Lines)
            {
                g.DrawLine(pen, line.A.ToPoint(), line.B.ToPoint());
            }
        }
        private void DrawPointLabels(Graphics g, Polygon polygon, Brush brush)
        {
            var font = new Font("Arial", 10);
            for (var i=0; i<polygon.Points.Length; i++)
            {
                var p = polygon[i];
                g.DrawString(i.ToString(), font, brush, p.ToPoint());
            }
        }
        private void DrawPoints(Graphics g, Polygon polygon, Brush brush)
        {
            foreach (var p in polygon.Points)
            {
                g.FillEllipse(brush, p.ToRectangle((10, 10)));
            }
        }

        private void ShowInfo()
        {
            lblCount.Text = debugCount.ToString();
            if (debugInfo.Count > 0)
                lblDebugInfo.Text = string.Join(", ", debugInfo[debugCount].Select(v => v.ToString()));
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
            debugCount = debugCount == debugInfo.Count-1 ? debugInfo.Count - 1 : debugCount + 1;
            Refresh();
        }
    }
}
