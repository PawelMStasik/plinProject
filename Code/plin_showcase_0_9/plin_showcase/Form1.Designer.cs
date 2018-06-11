/*
  Copyright (c) 2018 Paweł Marek Stasik

Licensed under MIT license. Please refer to LICENSE.txt file attached to the project for more information.
*/

namespace plin_showcase
{
    partial class Form1
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
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.tbFile = new System.Windows.Forms.TextBox();
            this.btnOpenFile = new System.Windows.Forms.Button();
            this.cbAlgorithm = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cbProximityCorrection = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cbTransition = new System.Windows.Forms.ComboBox();
            this.tbTransition = new System.Windows.Forms.TextBox();
            this.btnApply = new System.Windows.Forms.Button();
            this.pnlImage = new System.Windows.Forms.Panel();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.trbAngle = new System.Windows.Forms.TrackBar();
            this.labAngle = new System.Windows.Forms.Label();
            this.tbAngle = new System.Windows.Forms.TextBox();
            this.labScale = new System.Windows.Forms.Label();
            this.numScale = new System.Windows.Forms.NumericUpDown();
            this.pnlImage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trbAngle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numScale)).BeginInit();
            this.SuspendLayout();
            // 
            // openFileDialog
            // 
            this.openFileDialog.Filter = "Images|*.bmp;*.jpg;*.png;*.gif;*.tiff|All files|*.*";
            this.openFileDialog.Title = "Select an image";
            this.openFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileDialog_FileOk);
            // 
            // tbFile
            // 
            this.tbFile.Location = new System.Drawing.Point(12, 41);
            this.tbFile.Multiline = true;
            this.tbFile.Name = "tbFile";
            this.tbFile.ReadOnly = true;
            this.tbFile.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbFile.Size = new System.Drawing.Size(178, 43);
            this.tbFile.TabIndex = 0;
            this.tbFile.TabStop = false;
            // 
            // btnOpenFile
            // 
            this.btnOpenFile.Location = new System.Drawing.Point(12, 12);
            this.btnOpenFile.Name = "btnOpenFile";
            this.btnOpenFile.Size = new System.Drawing.Size(100, 23);
            this.btnOpenFile.TabIndex = 1;
            this.btnOpenFile.Text = "Open File";
            this.btnOpenFile.UseVisualStyleBackColor = true;
            this.btnOpenFile.Click += new System.EventHandler(this.btnOpenFile_Click);
            // 
            // cbAlgorithm
            // 
            this.cbAlgorithm.FormattingEnabled = true;
            this.cbAlgorithm.Items.AddRange(new object[] {
            "p-lin",
            "linear",
            "nearest neighbour"});
            this.cbAlgorithm.Location = new System.Drawing.Point(35, 115);
            this.cbAlgorithm.Name = "cbAlgorithm";
            this.cbAlgorithm.Size = new System.Drawing.Size(155, 21);
            this.cbAlgorithm.TabIndex = 2;
            this.cbAlgorithm.SelectedIndexChanged += new System.EventHandler(this.cbAlgorithm_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 99);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(110, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Interpolation algorithm";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 139);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(184, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Proximity-based Coefficient Correction";
            // 
            // cbProximityCorrection
            // 
            this.cbProximityCorrection.FormattingEnabled = true;
            this.cbProximityCorrection.Items.AddRange(new object[] {
            "no",
            "full"});
            this.cbProximityCorrection.Location = new System.Drawing.Point(35, 155);
            this.cbProximityCorrection.Name = "cbProximityCorrection";
            this.cbProximityCorrection.Size = new System.Drawing.Size(155, 21);
            this.cbProximityCorrection.TabIndex = 5;
            this.cbProximityCorrection.SelectedIndexChanged += new System.EventHandler(this.cbDistanceWaging_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 179);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(100, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Transition reduction";
            // 
            // cbTransition
            // 
            this.cbTransition.FormattingEnabled = true;
            this.cbTransition.Items.AddRange(new object[] {
            "none",
            "transition reduction"});
            this.cbTransition.Location = new System.Drawing.Point(35, 195);
            this.cbTransition.Name = "cbTransition";
            this.cbTransition.Size = new System.Drawing.Size(155, 21);
            this.cbTransition.TabIndex = 7;
            this.cbTransition.SelectedIndexChanged += new System.EventHandler(this.cbPassage_SelectedIndexChanged);
            // 
            // tbTransition
            // 
            this.tbTransition.Location = new System.Drawing.Point(35, 222);
            this.tbTransition.Name = "tbTransition";
            this.tbTransition.Size = new System.Drawing.Size(155, 20);
            this.tbTransition.TabIndex = 8;
            this.tbTransition.Text = "0";
            this.tbTransition.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // btnApply
            // 
            this.btnApply.Location = new System.Drawing.Point(115, 248);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(75, 23);
            this.btnApply.TabIndex = 9;
            this.btnApply.Text = "Apply";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // pnlImage
            // 
            this.pnlImage.AutoScroll = true;
            this.pnlImage.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlImage.Controls.Add(this.pictureBox);
            this.pnlImage.Location = new System.Drawing.Point(211, 12);
            this.pnlImage.Name = "pnlImage";
            this.pnlImage.Size = new System.Drawing.Size(414, 355);
            this.pnlImage.TabIndex = 10;
            // 
            // pictureBox
            // 
            this.pictureBox.Location = new System.Drawing.Point(0, 0);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(410, 351);
            this.pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox.TabIndex = 0;
            this.pictureBox.TabStop = false;
            // 
            // trbAngle
            // 
            this.trbAngle.LargeChange = 15;
            this.trbAngle.Location = new System.Drawing.Point(99, 382);
            this.trbAngle.Maximum = 360;
            this.trbAngle.Name = "trbAngle";
            this.trbAngle.Size = new System.Drawing.Size(524, 45);
            this.trbAngle.TabIndex = 11;
            this.trbAngle.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trbAngle.Scroll += new System.EventHandler(this.trbAngle_Scroll);
            this.trbAngle.ValueChanged += new System.EventHandler(this.trbAngle_ValueChanged);
            // 
            // labAngle
            // 
            this.labAngle.AutoSize = true;
            this.labAngle.Location = new System.Drawing.Point(12, 366);
            this.labAngle.Name = "labAngle";
            this.labAngle.Size = new System.Drawing.Size(81, 13);
            this.labAngle.TabIndex = 12;
            this.labAngle.Text = "Angle (degrees)";
            // 
            // tbAngle
            // 
            this.tbAngle.Location = new System.Drawing.Point(15, 382);
            this.tbAngle.Name = "tbAngle";
            this.tbAngle.ReadOnly = true;
            this.tbAngle.Size = new System.Drawing.Size(78, 20);
            this.tbAngle.TabIndex = 13;
            this.tbAngle.TabStop = false;
            this.tbAngle.Text = "0";
            this.tbAngle.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // labScale
            // 
            this.labScale.AutoSize = true;
            this.labScale.Location = new System.Drawing.Point(27, 304);
            this.labScale.Name = "labScale";
            this.labScale.Size = new System.Drawing.Size(37, 13);
            this.labScale.TabIndex = 14;
            this.labScale.Text = "Scale:";
            // 
            // numScale
            // 
            this.numScale.DecimalPlaces = 2;
            this.numScale.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numScale.Location = new System.Drawing.Point(70, 302);
            this.numScale.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numScale.Name = "numScale";
            this.numScale.Size = new System.Drawing.Size(120, 20);
            this.numScale.TabIndex = 15;
            this.numScale.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numScale.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numScale.ValueChanged += new System.EventHandler(this.numScale_ValueChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(634, 412);
            this.Controls.Add(this.numScale);
            this.Controls.Add(this.labScale);
            this.Controls.Add(this.tbAngle);
            this.Controls.Add(this.labAngle);
            this.Controls.Add(this.trbAngle);
            this.Controls.Add(this.pnlImage);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.tbTransition);
            this.Controls.Add(this.cbTransition);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cbProximityCorrection);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbAlgorithm);
            this.Controls.Add(this.btnOpenFile);
            this.Controls.Add(this.tbFile);
            this.MinimumSize = new System.Drawing.Size(650, 450);
            this.Name = "Form1";
            this.Text = "p-lin Showcase";
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.pnlImage.ResumeLayout(false);
            this.pnlImage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trbAngle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numScale)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.TextBox tbFile;
        private System.Windows.Forms.Button btnOpenFile;
        private System.Windows.Forms.ComboBox cbAlgorithm;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbProximityCorrection;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cbTransition;
        private System.Windows.Forms.TextBox tbTransition;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Panel pnlImage;
        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.TrackBar trbAngle;
        private System.Windows.Forms.Label labAngle;
        private System.Windows.Forms.TextBox tbAngle;
        private System.Windows.Forms.Label labScale;
        private System.Windows.Forms.NumericUpDown numScale;
    }
}

