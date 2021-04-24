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
        private PoligonInfo basePoligonInfo;
        private List<int[]> debugInfo = new List<int[]>();
        private int debugCount = 0;

        public View()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
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

        public void DrawDebug(int[] inds)
        {
            debugInfo.Add(inds);
        }

        private void pnlCanvas_Paint(object sender, PaintEventArgs e)
        {
            if (basePoligonInfo == null)
                return;

            ShowInfo();

            var g = e.Graphics;
            this.BackColor = Color.White;
            Model.Size size = (pnlCanvas.Width, pnlCanvas.Height);
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

                FillPoligon(g, leftButtomInfo.Poligon, debugInfo.Skip(debugCount).First(), Brushes.Gray);
                DrawLines(g, leftButtomInfo.Poligon, Pens.Black);
                DrawPoints(g, leftButtomInfo.Poligon, Brushes.DodgerBlue);
                DrawPointLabels(g, leftButtomInfo.Poligon, Brushes.Blue);

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

        private void FillPoligon(Graphics g, Poligon poligon, int[] part, Brush brush)
        {
            g.FillPolygon(brush, part.Select(i=> poligon[i].ToPoint()).ToArray());
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
        private void DrawPointLabels(Graphics g, Poligon poligon, Brush brush)
        {
            Vector2 shift = (5, 5);
            var font = new Font("Arial", 10);
            for (var i=0; i<poligon.Points.Length; i++)
            {
                var p = poligon[i];
                g.DrawString(i.ToString(), font, brush, p.ToPoint());
            }
        }
        private void DrawPoints(Graphics g, Poligon poligon, Brush brush)
        {
            foreach (var p in poligon.Points)
            {
                g.FillEllipse(brush, p.ToRectangle((10, 10)));
            }
        }

        private void ShowInfo()
        {
            lblCount.Text = debugCount.ToString();
            lblDebugInfo.Text = string.Join(", ", debugInfo[debugCount].Select(v => v.ToString()));
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
