/*
  Copyright (c) 2018 Paweł Marek Stasik

Licensed under MIT license. Please refer to LICENSE.txt file attached to the project for more information.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Casting;
using Transformations;

namespace plin_showcase
{
    public partial class Form1 : Form
    {
        int pos_scale_desc_Y;
        int pos_scale_num_Y;
        int pos_angle_desc_Y;
        int pos_angle_value_Y;
        int pos_angle_track_value_Y;
        int pos_angle_track_value_right;

        int panel_right;
        int panel_bottom;

        Interpolations.Interpolations.Implemented SelectedAlgorithm;
        Interpolations.Interpolations.ProximityBasedCoefficientReduction SelectedPBCC;
        int selectedTransition;

        Transformations.TransformationSetup t = new Transformations.TransformationSetup(true);

        float scalingFactor;
        double angle;

        System.Drawing.Bitmap Bitmap = null;
        Processing.Resizer Resizer = new Processing.Resizer();
        Blending2D Blending = null;

        private Processing.Image ToProcessableImage(System.Drawing.Bitmap image)
        {
            if (image == null) return null;
            Processing.Image converted = new Processing.Image(image.Width, image.Height, 3);

            int W_1 = converted.Width - 1;
            for(int y = converted.Height - 1; y >= 0; --y)
                for (int x = W_1; x >= 0; --x)
                {
                    var c = image.GetPixel(x, y);
                    converted[x, y, 0] = c.R;
                    converted[x, y, 1] = c.G;
                    converted[x, y, 2] = c.B;
                }

            return converted;
        }

        private float stdBlending(float x, float y) { return 1; }

        private System.Drawing.Bitmap ToSystemImage(Processing.Image image, Blending2D blending = null)
        {
            if (image == null) return null;
            System.Drawing.Bitmap converted = new Bitmap(image.Width, image.Height);

            if (blending == null) blending = stdBlending;

            int W_1 = converted.Width - 1;
            for (int y = converted.Height - 1; y >= 0; --y)
                for (int x = W_1; x >= 0; --x)
                {
                    float alpha = blending(x, y);

                    int rc = (int)((float)image[x, y, 0] * alpha);
                    int gc = (int)((float)image[x, y, 1] * alpha);
                    int bc = (int)((float)image[x, y, 2] * alpha);
                    int a = (int)((float)255 * alpha);

                    converted.SetPixel(x, y, Color.FromArgb(a, rc, gc, bc));
                }

            return converted;
        }

        private void UpdateViewedImage()
        {
            if (Resizer == null)
            {
                pictureBox.Image = null;
                return;
            }
            pictureBox.Image = ToSystemImage(Resizer.Result, Blending);
        }

        private void BigUpdate()
        {
            if (Bitmap == null)
            {
                Resizer.Original = null;
                UpdateViewedImage();
                return;
            }

            Resizer = new Processing.Resizer();
            Resizer.Original = ToProcessableImage(Bitmap);
            
            t.OriginalWidth = Resizer.Original.Width;
            t.OriginalHeight = Resizer.Original.Height;
            t.RelativeScaling = true;
            t.ScalingX = scalingFactor;
            t.ScalingY = scalingFactor;

            t.RotationInDegrees = true;
            t.RotationRescaling = true;
            t.RotationAngle = angle;

            Transformations.TransformationPrototype prototype = t.CalculateTransformation();

            int targetHeight = (int)(Math.Round((double)prototype.TargetHeight));
            int targetWidth = (int)(Math.Round((double)prototype.TargetWidth));
            Resizer.Result = new Processing.Image(targetWidth, targetHeight, 3);

            Cast2D casting = Casting.Function.FromTransformationMatrix(prototype.T_Target2Original, 0.5f);

            Resizer.Interpolation = new Interpolations.Interp();
            Resizer.Interpolation.InitializeTransformation(Resizer.Original.Width, Resizer.Original.Height, Resizer.Result.Width, Resizer.Result.Height, casting);

            float passage = 0;
            System.Single.TryParse(tbTransition.Text, out passage);

            Resizer.Interpolation.InitializeFunctions(SelectedAlgorithm, SelectedPBCC);
            if (selectedTransition > 0) Resizer.Interpolation.InitializePassageReduction(passage);
            else Resizer.Interpolation.TransitionReduction = null;

            Blending = Casting.Function.FastBlendingFromCasting(casting, 0.5f, 0.5f, Resizer.Original.Width, Resizer.Original.Height);

            Resizer.Optimalize(true);
            Resizer.Resize();

            UpdateViewedImage();
        }

        public Form1()
        {
            InitializeComponent();

            panel_right = this.Width - pnlImage.Right;
            panel_bottom = this.Height - pnlImage.Bottom;

            pos_scale_desc_Y = this.Height - labScale.Location.Y;
            pos_scale_num_Y = this.Height - numScale.Location.Y;
            pos_angle_desc_Y = this.Height - labAngle.Location.Y;
            pos_angle_value_Y = this.Height - tbAngle.Location.Y;
            pos_angle_track_value_Y = this.Height - trbAngle.Location.Y;
            pos_angle_track_value_right = this.Width - trbAngle.Right;

            cbAlgorithm.SelectedIndex = 0;
            cbProximityCorrection.SelectedIndex = 0;
            cbTransition.SelectedIndex = 0;

            scalingFactor = 1;
            angle = 0;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            pnlImage.Width = this.Width - pnlImage.Left - panel_right;
            pnlImage.Height = this.Height - pnlImage.Top - panel_bottom;

            labScale.Location = new Point(labScale.Location.X, this.Height - pos_scale_desc_Y);
            numScale.Location = new Point(numScale.Location.X, this.Height - pos_scale_num_Y);
            labAngle.Location = new Point(labAngle.Location.X, this.Height - pos_angle_desc_Y);
            tbAngle.Location = new Point(tbAngle.Location.X, this.Height - pos_angle_value_Y);
            trbAngle.Location = new Point(trbAngle.Location.X, this.Height - pos_angle_track_value_Y);
            //trbAngle.Width = this.Width - trbAngle.Left - pos_angle_track_value_right;
        }

        private void trbAngle_Scroll(object sender, EventArgs e)
        {
            tbAngle.Text = trbAngle.Value.ToString();
            angle = trbAngle.Value;
            //BigUpdate();
        }

        private void cbAlgorithm_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cbAlgorithm.SelectedIndex)
            {
                case 1: SelectedAlgorithm = Interpolations.Interpolations.Implemented.Linear; break;
                case 2: SelectedAlgorithm = Interpolations.Interpolations.Implemented.NearestNeighbour; break;
                default:
                case 0: SelectedAlgorithm = Interpolations.Interpolations.Implemented.PLin; break;
            }
        }

        private void cbDistanceWaging_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cbProximityCorrection.SelectedIndex)
            {
                case 1: SelectedPBCC = Interpolations.Interpolations.ProximityBasedCoefficientReduction.Full; break;
                default:
                case 0: SelectedPBCC = Interpolations.Interpolations.ProximityBasedCoefficientReduction.None; break;
            }
        }

        private void cbPassage_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cbTransition.SelectedIndex)
            {
                case 1: selectedTransition = 1; break;
                default:
                case 0: selectedTransition = 0; break;
            }
        }

        private void numScale_ValueChanged(object sender, EventArgs e)
        {
            scalingFactor = (float)numScale.Value;
            BigUpdate();
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            BigUpdate();
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            openFileDialog.ShowDialog();
        }

        private void openFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            string filename = openFileDialog.FileName;

            try
            {
                using (System.Drawing.Bitmap newBitmap = new Bitmap(filename))
                {
                    if(Bitmap != null) Bitmap.Dispose();
                    Bitmap = new Bitmap(newBitmap);
                }

                tbFile.Text = filename;
                tbFile.Select(filename.Length, 0);
                BigUpdate();
            }
            catch(System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Can't open this file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void trbAngle_ValueChanged(object sender, EventArgs e)
        {
            BigUpdate();
        }
    }
}
