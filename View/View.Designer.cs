
namespace View
{
    partial class View
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pnlControl = new System.Windows.Forms.Panel();
            this.lblCount = new System.Windows.Forms.Label();
            this.btnMinus = new System.Windows.Forms.Button();
            this.btnPlus = new System.Windows.Forms.Button();
            this.pnlCanvas = new System.Windows.Forms.Panel();
            this.lblDebugInfo = new System.Windows.Forms.Label();
            this.pnlControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlControl
            // 
            this.pnlControl.Controls.Add(this.lblDebugInfo);
            this.pnlControl.Controls.Add(this.lblCount);
            this.pnlControl.Controls.Add(this.btnMinus);
            this.pnlControl.Controls.Add(this.btnPlus);
            this.pnlControl.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlControl.Location = new System.Drawing.Point(0, 480);
            this.pnlControl.Name = "pnlControl";
            this.pnlControl.Size = new System.Drawing.Size(785, 39);
            this.pnlControl.TabIndex = 0;
            // 
            // lblCount
            // 
            this.lblCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCount.Location = new System.Drawing.Point(691, 10);
            this.lblCount.Name = "lblCount";
            this.lblCount.Size = new System.Drawing.Size(30, 15);
            this.lblCount.TabIndex = 1;
            this.lblCount.Text = "0";
            this.lblCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnMinus
            // 
            this.btnMinus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMinus.Location = new System.Drawing.Point(636, 6);
            this.btnMinus.Name = "btnMinus";
            this.btnMinus.Size = new System.Drawing.Size(49, 23);
            this.btnMinus.TabIndex = 0;
            this.btnMinus.Text = "<<";
            this.btnMinus.UseVisualStyleBackColor = true;
            this.btnMinus.Click += new System.EventHandler(this.btnMinus_Click);
            // 
            // btnPlus
            // 
            this.btnPlus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPlus.Location = new System.Drawing.Point(724, 6);
            this.btnPlus.Name = "btnPlus";
            this.btnPlus.Size = new System.Drawing.Size(49, 23);
            this.btnPlus.TabIndex = 0;
            this.btnPlus.Text = ">>";
            this.btnPlus.UseVisualStyleBackColor = true;
            this.btnPlus.Click += new System.EventHandler(this.btnPlus_Click);
            // 
            // pnlCanvas
            // 
            this.pnlCanvas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCanvas.Location = new System.Drawing.Point(0, 0);
            this.pnlCanvas.Name = "pnlCanvas";
            this.pnlCanvas.Size = new System.Drawing.Size(785, 480);
            this.pnlCanvas.TabIndex = 1;
            this.pnlCanvas.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlCanvas_Paint);
            // 
            // lblDebugInfo
            // 
            this.lblDebugInfo.AutoSize = true;
            this.lblDebugInfo.Location = new System.Drawing.Point(12, 10);
            this.lblDebugInfo.Name = "lblDebugInfo";
            this.lblDebugInfo.Size = new System.Drawing.Size(36, 15);
            this.lblDebugInfo.TabIndex = 2;
            this.lblDebugInfo.Text = "[info]";
            // 
            // View
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(785, 519);
            this.Controls.Add(this.pnlCanvas);
            this.Controls.Add(this.pnlControl);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "View";
            this.Text = "View";
            this.Resize += new System.EventHandler(this.View_Resize);
            this.pnlControl.ResumeLayout(false);
            this.pnlControl.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlControl;
        private System.Windows.Forms.Panel pnlCanvas;
        private System.Windows.Forms.Button btnMinus;
        private System.Windows.Forms.Button btnPlus;
        private System.Windows.Forms.Label lblCount;
        private System.Windows.Forms.Label lblDebugInfo;
    }
}

