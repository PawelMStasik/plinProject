/*
 Copyright (c) 2018 Paweł Marek Stasik

Licensed under MIT license. Please refer to LICENSE.txt file attached to the project for more information.
  
 */

/*
 * INTERPOLATION PROCESS:
 * 
 * coordinates in the target image -> coordines in the original image --> relative position --> reduced transition -> coefficients --> proxmity-based coefficient correction --> apply coefficients -> value in target point
 *                                                                    \                      \-------------------------------------/                                         /
 *                                                                     \--> positioning values -----------------------------------------------------------------------------/
 *                                                                     
 * Each interpolation algorithm should consist of interpolation function and positioning function.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Casting
{
    using Transformations;

    /// <summary>
    /// An ImagePoint structure for any kind of values
    /// (used in this project for integer coordinates).
    /// </summary>
    /// <typeparam name="T">Numeric class</typeparam>
    public struct ImPoint<T>
    {
        /// <summary>
        /// The X coordinate of the point.
        /// </summary>
        public T ix;
        /// <summary>
        /// They Y coordinate of the point.
        /// </summary>
        public T iy;

        /*public static implicit operator ImPoint<int>(ImPoint p) { return new ImPoint<int>() { ix = (int)p.ix, iy = (int)p.iy }; }
        public static implicit operator ImPoint(ImPoint<int> p) { return new ImPoint() { ix = (float)p.ix, iy = (float)p.iy }; }
        public static implicit operator ImPoint<float>(ImPoint p) { return new ImPoint<float>() { ix = p.ix, iy = p.iy }; }
        public static implicit operator ImPoint(ImPoint<float> p) { return new ImPoint() { ix = p.ix, iy = p.iy }; }*/
    }

    /// <summary>
    /// A general ImagePoint structure for floating-point
    /// (single precision) coordiantes.
    /// </summary>
    public struct ImPoint
    {
        /// <summary>
        /// The X coordinate of the point.
        /// </summary>
        public float ix;
        /// <summary>
        /// The Y coordinate of the point.
        /// </summary>
        public float iy;
    }

    /// <summary>
    /// An one-dimensional interpolation point - composition of
    /// a floating-point precision value (reference/normalized point)
    /// and an integer index for addressing table cells. Used for
    /// postionoing coefficients obtained from interpolation algorithm.
    /// </summary>
    public struct InterpPoint1D
    {
        /// <summary>
        /// A precision value [0:1] for information where between two point
        /// the interpolated point is being located (normalized position).
        /// </summary>
        public float ni;
        /// <summary>
        /// The left-most position of an original images's pixel which
        /// the obtained coefficients are applied to.
        /// </summary>
        public int i;
    }

    /// <summary>
    /// A two-dimensional interpolation point - a composition of normalized
    /// to [0:1] coordinate point and an integer coordinates of a reference
    /// point in the original image.
    /// </summary>
    public struct InterpPoint2D
    {
        /// <summary>
        /// A normalized position of the interpolated point - a cast to [0:1]
        /// values between bounding points.
        /// </summary>
        public ImPoint nPoint;
        /// <summary>
        /// A position of the most top-left point of the original image. Used for
        /// positioning the interpolation coefficients.
        /// </summary>
        public ImPoint<int> I0;
    }

    /// <summary>
    /// A one-dimensional casting function delegate. Provides a position of
    /// the interpolated point in the original image's space.
    /// </summary>
    /// <param name="n">Position in the target image.</param>
    /// <returns>Position in the original image.</returns>
    public delegate float Cast1D(float n);
    /// <summary>
    /// A two-dimensional casting function delegate. Provides a position of
    /// the interpolated point in the original image's space.
    /// </summary>
    /// <param name="nx">The X coordinate of the interpolated
    /// point in the target image's coordinates.</param>
    /// <param name="ny">The Y coordinate of the interpolated
    /// point in the target image's coordinates.</param>
    /// <returns>Position in the original image.</returns>
    public delegate ImPoint Cast2D(float nx, float ny);
    /// <summary>
    /// A two-dimensional function used for cutting out edges of
    /// the output image. It returns an alpha parameter (opacity)
    /// for a given point of the picture.
    /// </summary>
    /// <param name="nx">The X coordinate of the interpolated point
    /// in the original image's coordinates.</param>
    /// <param name="ny">The Y coordinate of the interpolated point
    /// in the original image's coordinates.</param>
    /// <returns>Value of the alpha parameter.</returns>
    public delegate float Blending2D(float nx, float ny);
    /// <summary>
    /// A simple transition function. It casts values from range 0-1 to 0-1.
    /// </summary>
    /// <param name="x">An input value from a range of [0,1].</param>
    /// <returns>An output value from a range of [0,1].</returns>
    public delegate float TransitionFunction(float x);

    /// <summary>
    /// A static class for combining casting functions and generating
    /// pre-defined casting functions.
    /// </summary>
    public static class Function
    {
        /// <summary>
        /// Combines two one-dimensional casting functions into a two-dimensional one.
        /// </summary>
        /// <param name="castX">A casting function for coordinaes along the X axis.</param>
        /// <param name="castY">A casting function for coordinaes along the Y axis.</param>
        /// <returns>A composite two-dimensional casting function.</returns>
        public static Cast2D CombineCasting(Cast1D castX, Cast1D castY)
        {
            return new Cast2D((float nx, float ny) =>
            {
                return new ImPoint()
                {
                    ix = castX(nx),
                    iy = castY(ny)
                };
            });
        }

        /// <summary>
        /// Genarates a fast casting function - a linear transition from 0 to last index.
        /// Results in small zoom and cuts edges off.
        /// </summary>
        /// <param name="originalLength">Size of the original image in a given dimension.</param>
        /// <param name="targetLength">Size of the target image in the same dimension.</param>
        /// <returns>A one-dimensional casting function.</returns>
        public static Cast1D GenFastCasting(int originalLength, int targetLength)
        {
            float J = (float)(targetLength-1) / (float)(originalLength-1);
            return new Cast1D((float n) => { return n / J; });
        }

        /// <summary>
        /// Genarates a proper casting function where pixels in both target and original image
        /// are positioned in accordance to their centers.
        /// </summary>
        /// <param name="originalLength">Size of the original image in a given dimension.</param>
        /// <param name="targetLength">Size of the target image in the same dimension.</param>
        /// <returns>A one-dimensional casting function.</returns>
        public static Cast1D GenProperCasting(int originalLength, int targetLength)
        {
            float S = (float)(targetLength) / (float)(originalLength);
            //float b = (1.0f - S) / (2.0f * S);
            return new Cast1D((float n) => { return (n + 0.5f) / S - 0.5f; }); //A little bit slower? But it's clearer.
            //return new Cast1D((float n) => { return n / S + b; }); //An alternative approach.
        }

        /// <summary>
        /// Creates a buffer for a one-dimensional casting function.
        /// </summary>
        /// <param name="targetLength">Size of the target image in a given dimension.</param>
        /// <param name="casting">Casting function.</param>
        /// <returns>The buffered casting function.</returns>
        public static Cast1D GenBufferCasting(int targetLength, Cast1D casting)
        {
            float[] Indices = new float[targetLength];
            if (casting != null)
            {
                for (int i = targetLength - 1; i >= 0; --i)
                    Indices[i] = casting(i);
            }

            return new Cast1D((float n) => { return Indices[(int)n]; });
        }

        /// <summary>
        /// Generates a two-dimensional casting function from a transformation matrix.
        /// </summary>
        /// <param name="t">A transformation matrix.</param>
        /// <param name="correctionX">A correction of X-axis coordinates. 0.5 places
        /// points in the middle of the pixels.</param>
        /// <param name="correctionY">A correction of Y-axis coordinates. 0.5 places
        /// points in the middle of the pixels.</param>
        /// <returns>A two-dimensional casting function.</returns>
        public static Cast2D FromTransformationMatrix(TransformationMatrix t, float correctionX = 0.5f, float correctionY = 0.5f)
        {
            TransformationMatrix transf = t;

            return new Cast2D((float nx, float ny) =>
            {
                nx = nx + correctionX;
                ny = ny + correctionY;
                return new ImPoint()
                {
                    ix = t.Ax * nx + t.Ax_cy * ny + t.bx - correctionX,
                    iy = t.Ay_cx * nx + t.Ay * ny + t.by - correctionY
                };
            });
        }

        /// <summary>
        /// Generates a two-dimensional casting function from a transformation matrix.
        /// </summary>
        /// <param name="t">A transformation matrix.</param>
        /// <param name="coordCorrection">A correction of X-axis and Y-axis coordinates. 0.5 places
        /// points in the middle of the pixels.</param>
        /// <returns>A two-dimensional casting function.</returns>
        public static Cast2D FromTransformationMatrix(TransformationMatrix t, float coordCorrection = 0.5f)
        {
            return FromTransformationMatrix(t, coordCorrection, coordCorrection);
        }

        /// <summary>
        /// Generates a fast blending function for a given casting function. Each point
        /// within defined borders will be passed as a visible, each outside as a transparent.
        /// </summary>
        /// <param name="casting">A casting function.</param>
        /// <param name="xRange">A range in X-axis that is supposed to be a part
        /// of the image on the left and right side. (0.5 is a good, standard value.)</param>
        /// <param name="yRange">A range in Y-axis that is supposed to be a part
        /// of the image on the top and bottom side. (0.5 is a good, standard value.)</param>
        /// <param name="originalWidth">Width of the original image.</param>
        /// <param name="originalHeight">Height of the original image.</param>
        /// <returns>Blending function.</returns>
        public static Blending2D FastBlendingFromCasting(Cast2D casting, float xRange, float yRange, float originalWidth, float originalHeight)
        {
            float left = -xRange;
            float top = -yRange;
            float right = originalWidth - 1.0f + xRange;
            float bottom = originalHeight - 1.0f + yRange;

            return new Blending2D((float nx, float ny) =>
            {
                ImPoint p = casting(nx, ny);
                if (p.ix >= left && p.iy >= top && p.ix < right && p.iy < bottom)
                    return 1.0f;
                return 0.0f;
            });
        }

        /// <summary>
        /// Generates a fast blending function for a given casting function using
        /// a given blending function. Each point within defined borders will be
        /// passed as a visible, each outside as a transparent.
        /// 
        /// Points on the borders will be passed by a transition function
        /// (0 means point being closer to inside).
        /// </summary>
        /// <param name="casting">A casting function.</param>
        /// <param name="blending">A blending function in a form of a transition function.</param>
        /// <param name="xRange">A range in X-axis that is supposed to be a part
        /// of the image on the left and right side. (0.5 is a good, standard value.)</param>
        /// <param name="yRange">A range in Y-axis that is supposed to be a part
        /// of the image on the top and bottom side. (0.5 is a good, standard value.)</param>
        /// <param name="originalWidth">Width of the original image.</param>
        /// <param name="originalHeight">Height of the original image.</param>
        /// <returns>Blending function.</returns>
        public static Blending2D BlendingFromCasting(Cast2D casting, TransitionFunction blending, float xRange, float yRange, float originalWidth, float originalHeight)
        {
            float left_out = -xRange;
            float left_in = xRange;
            float top_out = -yRange;
            float top_in = yRange;
            float right_out = originalWidth - 1.0f + xRange;
            float rignt_in = originalWidth - 1.0f - xRange;
            float bottom_out = originalHeight - 1.0f + yRange;
            float bottom_in = originalHeight - 1.0f - yRange;

            float DX = 2 * xRange;
            float DY = 2 * yRange;

            return new Blending2D((float nx, float ny) =>
            {
                ImPoint p = casting(nx, ny);
                float coefX = 0.0f;
                float coefY = 0.0f;

                if (p.ix > left_out && p.ix < right_out)
                {
                    if (p.ix < left_in) coefX = blending((left_in - p.ix) / DX);
                    else if (p.ix > rignt_in) coefX = blending((p.ix - rignt_in) / DX);
                    else coefX = 1.0f;
                }

                if (p.iy > top_out && p.iy < bottom_out)
                {
                    if (p.iy < top_in) coefY = blending((top_in - p.iy) / DY);
                    else if(p.iy > bottom_in) coefY = blending((p.iy - bottom_in) / DY);
                    else coefY = 1.0f;
                }

                return coefX * coefY;
            });
        }

        /// <summary>
        /// A linear blending function (from 0, which is assigned 1, to 1,
        /// which is assigned 0).
        /// </summary>
        public static TransitionFunction LinearBlending = (float i) => 1.0f - i;

        /// <summary>
        /// A point (step) blending function (equivallent to the nearest neighbour).
        /// Values lower than 0.5 are assigned 1.
        /// </summary>
        public static TransitionFunction PointBlending = (float i) => (i < 0.5f) ? 1.0f : 0.0f;

        /// <summary>
        /// A p-lin blending function (from 0, which is assigned 1, to 1,
        /// which is assigned 0).
        /// </summary>
        public static TransitionFunction PlinBlending = (float i) => 1.0f - i * i / ((1.0f - i) * (1.0f - i) + i * i);
    }
}

namespace Transformations
{
    using Casting;

    /*
     * |   Ax   Ax_cy  bx  | |x|
     * | Ay_cx    Ay   by  | |y|
     * |   0      0    1   | |1|
    */
    /// <summary>
    /// A structure representing an affine transformation matrix
    /// for a two-dimensional space.
    /// |   Ax   Ax_cy  bx  | |x|
    /// | Ay_cx    Ay   by  | |y|
    /// |   0      0    1   | |1|
    /// </summary>
    public struct TransformationMatrix
    {
        public float Ax;
        public float Ax_cy;
        public float Ay;
        public float Ay_cx;
        public float bx;
        public float by;

        /// <summary>
        /// Get a transformation matrix that is obtained by applying
        /// this matrix after another.
        /// </summary>
        /// <param name="m">The matrix to be applied as first.</param>
        /// <returns>The resulting transformation matrix.</returns>
        public TransformationMatrix ApplyAfter(TransformationMatrix m)
        {
            TransformationMatrix result = new TransformationMatrix();

            result.Ax = Ax * m.Ax + Ax_cy * m.Ay_cx;
            result.Ax_cy = Ax * m.Ax_cy + Ax_cy * m.Ay;
            result.Ay_cx = Ay_cx * m.Ax + Ay * m.Ay_cx;
            result.Ay = Ay_cx * m.Ax_cy + Ay * m.Ay;
            result.bx = Ax * m.bx + Ax_cy * m.by + bx;
            result.by = Ay_cx * m.bx + Ay * m.by + by;

            return result;
        }
        /// <summary>
        /// Get a transformation matrix that is obtained by applying
        /// this matrix before another.
        /// </summary>
        /// <param name="m">The matrix to be applied as second.</param>
        /// <returns>The resulting transformation matrix.</returns>
        public TransformationMatrix ApplyBefore(TransformationMatrix m)
        {
            TransformationMatrix result = new TransformationMatrix();

            result.Ax = m.Ax * Ax + m.Ax_cy * Ay_cx;
            result.Ax_cy = m.Ax * Ax_cy + m.Ax_cy * Ay;
            result.Ay_cx = m.Ay_cx * Ax + m.Ay * Ay_cx;
            result.Ay = m.Ay_cx * Ax_cy + m.Ay * Ay;
            result.bx = m.Ax * bx + m.Ax_cy * by + m.bx;
            result.by = m.Ay_cx * bx + m.Ay * by + m.by;

            return result;
        }
        /// <summary>
        /// Get a point by using this transformation matrix.
        /// </summary>
        /// <param name="p">Image point to cast.</param>
        /// <returns>The point after the transformation.</returns>
        public ImPoint Apply(ImPoint p)
        {
            return new ImPoint()
            {
                ix = Ax * p.ix + Ax_cy * p.iy + bx,
                iy = Ay_cx * p.ix + Ay * p.iy + by
            };
        }

        public static TransformationMatrix operator *(TransformationMatrix m1, TransformationMatrix m2)
        {
            return m1.ApplyAfter(m2);
        }

        public static ImPoint operator *(TransformationMatrix m, ImPoint p) { return m.Apply(p); }
        /// <summary>
        /// Returns a unity transformation matrix.
        /// </summary>
        public static TransformationMatrix UnityTransformation
        {
            get
            {
                return new TransformationMatrix()
                {
                    Ax = 1,
                    Ax_cy = 0,
                    Ay_cx = 0,
                    Ay = 1,
                    bx = 0,
                    by = 0,
                };
            }
        }
    }

    /// <summary>
    /// A prototype of transformation for casting between two images, which one is being
    /// considered as an original and second as a target.
    /// </summary>
    public struct TransformationPrototype
    {
        /// <summary>
        /// A transformation matrix for casting original image's coordinates into
        /// target image's cooridnate space. 
        /// </summary>
        public TransformationMatrix T_Original2Target;
        /// <summary>
        /// A transformation matrix for casting target image's cooridinates into
        /// original image's coordinate space.
        /// </summary>
        public TransformationMatrix T_Target2Original;
        /// <summary>
        /// Width of the target image.
        /// </summary>
        public float TargetWidth;
        /// <summary>
        /// Height of the target image.
        /// </summary>
        public float TargetHeight;
        /// <summary>
        /// Width of the original image.
        /// </summary>
        public float OriginalWidth;
        /// <summary>
        /// Height of the original image.
        /// </summary>
        public float OriginalHeight;
    }

    /*
     * Order of operations:
     * 0. Expansion
     * 1. Translation
     * 2. Scaling
     * 3. Roatation
     * 4. Expansion (aftercut)
    */
    /// <summary>
    /// A struct for holding information needed to generate a transformation prototype.
    /// The order of operations: translation, scaling, rotation. Expansion can be after
    /// or before them all and in each case it relies on original coordinate space and size.
    /// </summary>
    public struct TransformationSetup
    {
        /// <summary>
        /// An angle of rotation (anti-clockwise).
        /// </summary>
        public double RotationAngle;
        /// <summary>
        /// Is rotation supposed to rescale the image.
        /// </summary>
        public bool RotationRescaling;
        /// <summary>
        /// Is rotation angle given in degrees. If not, then in radians.
        /// </summary>
        public bool RotationInDegrees;

        /// <summary>
        /// A scaling value along the X-axis.
        /// </summary>
        public float ScalingX;
        /// <summary>
        /// A scaling value along the Y-axis.
        /// </summary>
        public float ScalingY;
        /// <summary>
        /// Are both scaling values represent a scaling factors? If not,
        /// then they represent the target size.
        /// </summary>
        public bool RelativeScaling;

        /// <summary>
        /// How many pixels move the image along the X-axis (right-positive).
        /// </summary>
        public float TranslateX;
        /// <summary>
        /// How many pixles move the image along the Y-axis (bottom-positive).
        /// </summary>
        public float TranslateY;

        /// <summary>
        /// Image expansion on the left in pixels.
        /// </summary>
        public float ExpandLeft;
        /// <summary>
        /// Image expansion on the top in pixels.
        /// </summary>
        public float ExpandTop;
        /// <summary>
        /// Image expansion on the right in pixels.
        /// </summary>
        public float ExpandRight;
        /// <summary>
        /// Image expansion on the bottom in pixels.
        /// </summary>
        public float ExpandBottom;
        /// <summary>
        /// If expansion should be applied as last. If so, it will cut out
        /// the image to the bounds based on original dimensions.
        /// </summary>
        public bool ApplyExpansionLast;

        /// <summary>
        /// Width of the original image.
        /// </summary>
        public float OriginalWidth;
        /// <summary>
        /// Height of the original image.
        /// </summary>
        public float OriginalHeight;

        /// <summary>
        /// Calculates a transformation prototype from current setup.
        /// </summary>
        /// <returns>A transformation prototype.</returns>
        public TransformationPrototype CalculateTransformation()
        {
            TransformationPrototype result = new TransformationPrototype();
            TransformationMatrix t = TransformationMatrix.UnityTransformation;
            TransformationMatrix l = TransformationMatrix.UnityTransformation;

            float width = OriginalWidth;
            float height = OriginalHeight;

            if (!ApplyExpansionLast)
            {
                t.bx = ExpandLeft;
                t.by = ExpandTop;

                width += ExpandLeft + ExpandRight;
                height += ExpandTop + ExpandBottom;
            }

            t.bx += TranslateX;
            t.by += TranslateY;

            if (RelativeScaling)
            {
                t.Ax = ScalingX;
                t.Ay = ScalingY;

                width *= ScalingX;
                height *= ScalingY;
            }
            else
            {
                t.Ax = ScalingX / OriginalWidth;
                t.Ay = ScalingY / OriginalHeight;

                width = ScalingX;
                height = ScalingY;
            }

            l.bx = -t.bx;
            l.by = -t.by;
            l.Ax = 1 / t.Ax;
            l.Ay = 1 / t.Ay;

            TransformationMatrix rot = new TransformationMatrix();
            TransformationMatrix revrot = new TransformationMatrix();
            double angle = RotationInDegrees ? RotationAngle * Math.PI / 180.0 : RotationAngle;
            float cos = (float)Math.Cos(angle);
            float sin = (float)Math.Sin(angle);

            rot.Ax = cos;
            rot.Ax_cy = sin;
            rot.Ay_cx = -sin;
            rot.Ay = cos;
            rot.bx = 0;
            rot.by = 0;
            //rot.by = width * sin; //only true for 0-90 dgr!
            if (sin >= 0) rot.by += width * sin;
            else rot.bx -= height * sin;

            if (cos < 0)
            {
                rot.bx -= width * cos;
                rot.by -= height * cos;
            }

            revrot.Ax = cos;
            revrot.Ax_cy = -sin;
            revrot.Ay_cx = sin;
            revrot.Ay = cos;
            if (sin >= 0)
            {
                if (cos >= 0)
                {
                    revrot.bx = width * sin * sin;
                    revrot.by = -width * sin * cos;
                }
                else
                {
                    revrot.bx = width - height * sin * cos;
                    revrot.by = height * cos * cos;
                }
            }
            else
            {
                if (cos >= 0)
                {
                    revrot.bx = height * sin * cos;
                    revrot.by = height * sin * sin;
                }
                else
                {
                    revrot.bx = width * cos * cos;
                    revrot.by = height + width * sin * cos;
                }
            }

            result.OriginalWidth = OriginalWidth;
            result.OriginalHeight = OriginalHeight;
            result.T_Original2Target = t.ApplyBefore(rot);
            result.T_Target2Original = revrot.ApplyBefore(l);
            result.TargetWidth = WidthAfterRotationRad(angle, width, height);
            result.TargetHeight = HeightAfterRotationRad(angle, width, height);

            if (ApplyExpansionLast)
            {
                result.TargetWidth = ExpandLeft + OriginalWidth + ExpandRight;
                result.TargetHeight = ExpandTop + OriginalHeight + ExpandBottom;
                result.T_Original2Target.bx += ExpandLeft;
                result.T_Original2Target.by += ExpandTop;
                result.T_Target2Original.bx -= ExpandLeft;
                result.T_Target2Original.by -= ExpandTop;
            }

            return result;
        }

        public TransformationSetup(bool standard = true)
        {
            RotationAngle = 0;
            RotationRescaling = standard;
            RotationInDegrees = standard;

            ScalingX = standard ? 1 : 0;
            ScalingY = standard ? 1 : 0;
            RelativeScaling = standard;

            TranslateX = 0;
            TranslateY = 0;

            ExpandLeft = 0;
            ExpandTop = 0;
            ExpandRight = 0;
            ExpandBottom = 0;
            ApplyExpansionLast = false;

            OriginalWidth = 0;
            OriginalHeight = 0;
        }

        /// <summary>
        /// Calculates width of the image after a rotation.
        /// </summary>
        /// <param name="angle_degrees">Rotation angle in degrees.</param>
        /// <param name="originalWidth">Width before the rotation.</param>
        /// <param name="originalHeight">Height before the rotation.</param>
        /// <returns>Width after the rotation.</returns>
        public static float WidthAfterRotationDgr(double angle_degrees, float originalWidth, float originalHeight)
        {
            double angl = angle_degrees * Math.PI / 180.0;
            return originalWidth * (float) Math.Abs(Math.Cos(angl)) + originalHeight * (float) Math.Abs(Math.Sin(angl));
        }
        /// <summary>
        /// Calculates height of the image after a rotation.
        /// </summary>
        /// <param name="angle_degrees">Rotation angle in degrees.</param>
        /// <param name="originalWidth">Width before the rotation.</param>
        /// <param name="originalHeight">Height before the rotation.</param>
        /// <returns>Height after the rotation.</returns>
        public static float HeightAfterRotationDgr(double angle_degrees, float originalWidth, float originalHeight)
        {
            double angl = angle_degrees * Math.PI / 180.0;
            return originalWidth * (float) Math.Abs(Math.Sin(angl)) + originalHeight * (float) Math.Abs(Math.Cos(angl));
        }
        /// <summary>
        /// Calculates width of the image after a rotation.
        /// </summary>
        /// <param name="angle_radians">Rotation angle in radians.</param>
        /// <param name="originalWidth">Width before the rotation.</param>
        /// <param name="originalHeight">Height before the rotation.</param>
        /// <returns>Width after the rotation.</returns>
        public static float WidthAfterRotationRad(double angle_radians, float originalWidth, float originalHeight)
        {
            return originalWidth * (float) Math.Abs(Math.Cos(angle_radians)) + originalHeight * (float) Math.Abs(Math.Sin(angle_radians));
        }
        /// <summary>
        /// Calculates height of the image after a rotation.
        /// </summary>
        /// <param name="angle_radians">Rotation angle in radians.</param>
        /// <param name="originalWidth">Width before the rotation.</param>
        /// <param name="originalHeight">Height before the rotation.</param>
        /// <returns>Height after the rotation.</returns>
        public static float HeightAfterRotationRad(double angle_radians, float originalWidth, float originalHeight)
        {
            return originalWidth * (float) Math.Abs(Math.Sin(angle_radians)) + originalHeight * (float) Math.Abs(Math.Cos(angle_radians));
        }
    }
}

namespace Interpolations{
    using Casting;

    /// <summary>
    /// A delegate for a one-dimanesional interpolation function that turns
    /// a normalized position (values [0:1]) into an array of coefficients. Indices
    /// of the points which the coefficients are applied to are obtained through
    /// different function.
    /// </summary>
    /// <param name="ni">A normalized position of the interpolated point.</param>
    /// <returns>An array of coefficients.</returns>
    public delegate float[] Interpolate1D(float ni);
    /// <summary>
    /// A delegate for a two-dimanesional interpolation function that turns
    /// a normalized point into a two-dimnesional array of coefficients. Indices
    /// of the points which the coefficients are applied to are obtained through
    /// different function.
    /// </summary>
    /// <param name="nix">A normalized X coordinate of the interpolated point.</param>
    /// <param name="niy">A normalized Y coordinate of the interpolated point.</param>
    /// <returns>A matrix of wages.</returns>
    public delegate float[,] Interpolate2D(float nix, float niy);

    /// <summary>
    /// A delegate for an one-dimensional function for getting values to obtain coefficients
    /// and to position them.
    /// </summary>
    /// <param name="i">A position of the interpolated point in the original image's coordinates.</param>
    /// <returns>A positioning point - a normalized value and the left-most index
    /// in the original image's coordinates for positioning the wages.</returns>
    public delegate InterpPoint1D Interp1DStart(float i);
    /// <summary>
    /// A delegate for a two-dimensional function for getting values to obtain coefficients
    /// and position them.
    /// </summary>
    /// <param name="ix">The X coordinate of the interpolated point
    /// in the original image's coordinates.</param>
    /// <param name="iy">The Y coordinate of the interpolated point
    /// in the original image's coordinates.</param>
    /// <returns>A positioning point - normalized value and the most top-left
    /// indices in the original image's coordinates for positioning the coefficients.</returns>
    public delegate InterpPoint2D Interp2DStart(float ix, float iy);

    /// <summary>
    /// A delegate for PBCC process.
    /// </summary>
    /// <param name="w">A two-dimensional array of coefficients (matrix).</param>
    /// <param name="nix">A normalized X coordiante of the interpolated point.</param>
    /// <param name="niy">A normalized Y coordinate of the interpolated point.</param>
    /// <returns>Corrected coefficients using the PBCC algorithm.</returns>
    public delegate float[,] PBCCFunc(float[,] w, float nix, float niy);

    /// <summary>
    /// A delegate for a transition-reduction function.
    /// </summary>
    /// <param name="nix">A normalized X coordinate of the interpolated point.</param>
    /// <param name="niy">A normalized Y coordinate of the interpolated point.</param>
    /// <returns>New positions (normalized).</returns>
    public delegate ImPoint TransitionReductionFunc(float nix, float niy);

    /// <summary>
    /// A static class for generating interpolation functions.
    /// </summary>
    public static class Interpolations
    {
        /// <summary>
        /// Pre-defined distance waging algorithms.
        /// </summary>
        public enum ProximityBasedCoefficientReduction
        {
            /// <summary>
            /// No PBCC (use null for Resizer class instead).
            /// </summary>
            None = 0,
            /// <summary>
            /// Full, proper PBCC.
            /// </summary>
            Full = 1
        }

        /// <summary>
        /// Pre-defined interpolation algorithms.
        /// </summary>
        public enum Implemented
        {
            /// <summary>
            /// Nearest neighbour algorithm (with rounding down of the positions of 0.5).
            /// </summary>
            NearestNeighbour = 0,
            /// <summary>
            /// Bilinear interpolation.
            /// </summary>
            Linear = 1,
            /// <summary>
            /// D-lin interpolation.
            /// </summary>
            PLin = 2
        }

        //closeness function for full PBCC algorithm
        private static float proximityFull(float nix, float niy)
        {
            if (nix == 0.0f && niy == 0.0f) return 1.0f;
            return (float)(1.0d - Math.Sqrt((nix * nix + niy * niy) / 2.0d));
        }

        /// <summary>
        /// Combines two one-dimensional interpolation functions into a two-dimensional one.
        /// </summary>
        /// <param name="interpolateX">The interpolation function along the X axis.</param>
        /// <param name="interpolateY">The interpolation function along the Y axis.</param>
        /// <returns>A composite two-dimensional interpolation function.</returns>
        public static Interpolate2D CombineInterpolations(Interpolate1D interpolateX, Interpolate1D interpolateY)
        {
            return new Interpolate2D((float nix, float niy) =>
            {
                float[] coeffX = interpolateX(nix);
                float[] coeffY = interpolateY(niy);

                float[,] coeff = new float[coeffX.Length, coeffY.Length];
                for (int i = coeffX.Length - 1; i >= 0; --i)
                    for (int j = coeffY.Length - 1; j >= 0; --j)
                        coeff[i, j] = coeffX[i] * coeffY[j];

                return coeff;
            });
        }
        /// <summary>
        /// Combines two one-dimensional interpolation functions into a two-dimensional one
        /// with set size of the output matrix.
        /// </summary>
        /// <param name="interpolateX">The interpolation function along the X axis.</param>
        /// <param name="interpolateY">The interpolation function along the Y axis.</param>
        /// <param name="_width">Width of the output matrix (along the X axis).</param>
        /// <param name="_height">Height of the output matrix (along the Y axis).</param>
        /// <returns>A composite two-dimensional interpolation function.</returns>
        public static Interpolate2D CombineInterpolations(Interpolate1D interpolateX, Interpolate1D interpolateY, int _width, int _height)
        {
            return new Interpolate2D((float nix, float niy) =>
            {
                float[] coeffX = interpolateX(nix);
                float[] coeffY = interpolateY(niy);

                float[,] coeff = new float[_width, _height];
                for (int i = coeffX.Length - 1; i >= 0; --i)
                    for (int j = coeffY.Length - 1; j >= 0; --j)
                        coeff[i, j] = coeffX[i] * coeffY[j];

                return coeff;
            });
        }

        /// <summary>
        /// Combines two one-dimensional positioning functions for interpolation purposes.
        /// </summary>
        /// <param name="posX">A positioning function for the interpolation along the X axis.</param>
        /// <param name="posY">A positioning function for the interpolation along the Y axis.</param>
        /// <returns>A composite positioning function.</returns>
        public static Interp2DStart CombineStarts(Interp1DStart posX, Interp1DStart posY)
        {
            return new Interp2DStart((float ix, float iy) =>
            {
                InterpPoint1D px = posX(ix);
                InterpPoint1D py = posY(iy);
                return new InterpPoint2D()
                {
                    I0 = new ImPoint<int>() { ix = px.i, iy = py.i },
                    nPoint = new ImPoint() { ix = px.ni, iy = py.ni }
                };
            });
        }

        /// <summary>
        /// Generates an one-dimensional linear interpolation function.
        /// </summary>
        /// <returns>Linear interpolation</returns>
        public static Interpolate1D GenLinear()
        {
            return new Interpolate1D((float ni) =>
            {
                return new float[2] { 1.0f - ni, ni };
            });
        }

        /// <summary>
        /// Generates an one-dimensional p-lin interpolation function.
        /// </summary>
        /// <returns>P-lin interpolation</returns>
        public static Interpolate1D GenPlin()
        {
            return new Interpolate1D((float ni) =>
            {
                float q = ni * ni;
                float a = q / ((1.0f - ni) * (1.0f - ni) + q);
                return new float[2] { 1.0f - a, a };
            });
        }

        /// <summary>
        /// Generates an one-dimensional nearest neighbour algorithm.
        /// </summary>
        /// <returns>Nearest neighbour</returns>
        public static Interpolate1D GenNearest()
        {
            return new Interpolate1D((float ni) =>
            {
                if (ni < 0.5f) return new float[2] { 1.0f, 0.0f };
                return new float[2] { 0.0f, 1.0f };
            });
        }

        /// <summary>
        /// Generates an one-dimensional interpolation function from a given transition function.
        /// </summary>
        /// <param name="passageFunction">A transition function.</param>
        /// <returns>A custom interpolation function.</returns>
        public static Interpolate1D GenFromPassage(TransitionFunction passageFunction)
        {
            return new Interpolate1D((float ni) =>
            {
                return new float[2] { passageFunction(ni), passageFunction(1 - ni) };
            });
        }

        /// <summary>
        /// Generates an one-dimensional positioning function for classical two-point interpolations
        /// (nearest neighbour, linear, p-lin).
        /// </summary>
        /// <returns>Positioning function.</returns>
        public static Interp1DStart GenClassicStart()
        {
            return new Interp1DStart((float i) =>
            {
                float i0 = (float)Math.Floor(i);
                return new InterpPoint1D() { i = (int)i0, ni = i - i0 };
            });
        }

        /// <summary>
        /// Generates the proximity-based coefficient correction algorithm.
        /// </summary>
        /// <param name="mode">A selected variant of the PBCC algorihtm.</param>
        /// <returns>PBCC algorithm.</returns>
        public static PBCCFunc GenProximityBasedCoefficientCorrection(ProximityBasedCoefficientReduction mode)
        {
            if (mode == ProximityBasedCoefficientReduction.Full)
                return new PBCCFunc((float[,] w, float nix, float niy) =>
                {
                    float[,] output = new float[2, 2];
                    output[0, 0] = proximityFull(nix, niy) * w[0, 0];
                    output[0, 1] = proximityFull(nix, 1.0f - niy) * w[0, 1];
                    output[1, 0] = proximityFull(1.0f - nix, niy) * w[1, 0];
                    output[1, 1] = proximityFull(1.0f - nix, 1.0f - niy) * w[1, 1];

                    float S = output[0, 0] + output[0, 1] + output[1, 0] + output[1, 1];

                    output[0, 0] /= S;
                    output[1, 0] /= S;
                    output[0, 1] /= S;
                    output[1, 1] /= S;

                    return output;
                });

            return new PBCCFunc((float[,] w, float nix, float niy) => { return w; });
        }

        /// <summary>
        /// Generates a transition reduction function for given transition in the target image.
        /// </summary>
        /// <param name="targetTransition">Size of passage (in pixels) in the target image</param>
        /// <param name="targetWidth">Width of the target image</param>
        /// <param name="targetHeight">Height of the target image.</param>
        /// <param name="originalWidth">Width of the original image.</param>
        /// <param name="originalHeight">Height of the original image.</param>
        /// <returns>TAR function.</returns>
        public static TransitionReductionFunc GenTransitionReduction(float targetTransition, int targetWidth, int targetHeight, int originalWidth, int originalHeight)
        {
            float Qx = (float)originalWidth / (float)targetWidth;
            float Qy = (float)originalHeight / (float)targetHeight;

            float passageX = Qx * targetTransition;
            if (passageX < 0) passageX = 0.0f;
            else if (passageX > 1.0f) passageX = 1.0f;

            float passageY = Qy * targetTransition;
            if (passageY < 0) passageY = 0.0f;
            else if (passageY > 1.0f) passageY = 1.0f;

            float lockX = (1.0f - passageX) / 2.0f;
            float lockY = (1.0f - passageY) / 2.0f;

            return new TransitionReductionFunc((float nix, float niy) =>
            {
                return new ImPoint()
                {
                    ix = _limit01((nix - lockX) / passageX),
                    iy = _limit01((niy - lockY) / passageY)
                };
            });
        }

        //limiting values to [0:1]
        private static float _limit01(float v)
        {
            if (v <= 0.0f) return 0.0f;
            if (v >= 1.0f) return 1.0f;
            return v;
        }
    }

    /// <summary>
    /// A class for holding a set up parameters and functions for a given interpolation of two images.
    /// </summary>
    public class Interp
    {
        /// <summary>
        /// The two-dimensional interpolation function in use.
        /// </summary>
        public Interpolate2D Interpolation;
        /// <summary>
        /// The positioning function for the interpolation function in use.
        /// </summary>
        public Interp2DStart PositionWages;
        /// <summary>
        /// The transition reduction function in use for current dimensions of images. Null if none.
        /// </summary>
        public TransitionReductionFunc TransitionReduction = null;
        /// <summary>
        /// The PBCC algorithm in use. Null if none.
        /// </summary>
        public PBCCFunc ProximityBasedCoefficientCorrection = null;

        /// <summary>
        /// [Read-only] The two-dimensional casting function to convert target image's
        /// coordinates to original image's coordinates.
        /// </summary>
        public Cast2D CastPosition { get; private set; }
        /// <summary>
        /// [Read-only] Width of the original image for the current settings of interpolation.
        /// </summary>
        public int OriginalWidth { get; private set; }
        /// <summary>
        /// [Read-only] Height of the original image for the current settings of interpolation.
        /// </summary>
        public int OriginalHeight { get; private set; }
        /// <summary>
        /// [Read-only] Width of the target image for the current settings of interpolation.
        /// </summary>
        public int TargetWidth { get; private set; }
        /// <summary>
        /// [Read-only] Height of the target image for the current settings of interpolation.
        /// </summary>
        public int TargetHeight { get; private set; }

        /// <summary>
        /// An initialization for dimnesions and a casting function
        /// (Casting.Function.GenProperCasting will be used).
        /// </summary>
        /// <param name="originalWidth">Width of the original image.</param>
        /// <param name="originalHeight">Height of the original image.</param>
        /// <param name="targetWidth">Width of the target image.</param>
        /// <param name="targetHeight">Height of the target image.</param>
        public void InitializeResize(int originalWidth, int originalHeight, int targetWidth, int targetHeight)
        {
            OriginalHeight = originalHeight;
            OriginalWidth = originalWidth;
            TargetHeight = targetHeight;
            TargetWidth = targetWidth;

            CastPosition = Casting.Function.CombineCasting(
                Casting.Function.GenProperCasting(originalWidth, targetWidth),
                Casting.Function.GenProperCasting(originalHeight, TargetHeight));
        }
        /// <summary>
        /// An initialization for dimanesions and a casting function (the given casting
        /// function will be used for both X and Y axis - use for square images).
        /// </summary>
        /// <param name="originalWidth">Width of the original image.</param>
        /// <param name="originalHeight">Height of the original image.</param>
        /// <param name="targetWidth">Width of the target image.</param>
        /// <param name="targetHeight">Height of the target image.</param>
        /// <param name="casting">A casting function.</param>
        public void InitializeResize(int originalWidth, int originalHeight, int targetWidth, int targetHeight, Cast1D casting)
        {
            OriginalHeight = originalHeight;
            OriginalWidth = originalWidth;
            TargetHeight = targetHeight;
            TargetWidth = targetWidth;

            CastPosition = Casting.Function.CombineCasting(casting, casting);
        }
        /// <summary>
        /// An initialization for dimensions and casting functions
        /// (both functions have to be provided).
        /// </summary>
        /// <param name="originalWidth">Width of the original image.</param>
        /// <param name="originalHeight">Height of the original image.</param>
        /// <param name="targetWidth">Width of the target image.</param>
        /// <param name="targetHeight">Height of the target image.</param>
        /// <param name="castingX">A casting function along the X axis.</param>
        /// <param name="castingY">A casting function along the Y axis.</param>
        public void InitializeResize(int originalWidth, int originalHeight, int targetWidth, int targetHeight, Cast1D castingX, Cast1D castingY)
        {
            OriginalHeight = originalHeight;
            OriginalWidth = originalWidth;
            TargetHeight = targetHeight;
            TargetWidth = targetWidth;

            CastPosition = Casting.Function.CombineCasting(castingX, castingY);
        }

        public void InitializeTransformation(int originalWidth, int originalHeight, int targetWidth, int targetHeight, Cast2D casting)
        {
            OriginalHeight = originalHeight;
            OriginalWidth = originalWidth;
            TargetHeight = targetHeight;
            TargetWidth = targetWidth;

            CastPosition = casting;
        }

        /// <summary>
        /// An initialization for interpolation functions - interpolation, positioning,
        /// transition reduction and PBCC.
        /// </summary>
        /// <param name="interp">One of the pre-defined interpolations.</param>
        /// <param name="pbcc">One of the pre-defined PBCC algorithms.</param>
        /// <param name="reduceTransition">A value of the transition reduction (pixels).
        /// Zero or negative means no transition reduction function will be used.</param>
        public void InitializeFunctions(Interpolations.Implemented interp, Interpolations.ProximityBasedCoefficientReduction pbcc, float reduceTransition)
        {
            switch (interp)
            {
                case Interpolations.Implemented.NearestNeighbour:
                    Interpolation = Interpolations.CombineInterpolations(Interpolations.GenNearest(), Interpolations.GenNearest());
                    PositionWages = Interpolations.CombineStarts(Interpolations.GenClassicStart(), Interpolations.GenClassicStart());
                    break;
                case Interpolations.Implemented.Linear:
                    Interpolation = Interpolations.CombineInterpolations(Interpolations.GenLinear(), Interpolations.GenLinear());
                    PositionWages = Interpolations.CombineStarts(Interpolations.GenClassicStart(), Interpolations.GenClassicStart());
                    break;
                case Interpolations.Implemented.PLin:
                default:
                    Interpolation = Interpolations.CombineInterpolations(Interpolations.GenPlin(), Interpolations.GenPlin());
                    PositionWages = Interpolations.CombineStarts(Interpolations.GenClassicStart(), Interpolations.GenClassicStart());
                    break;
            }

            if (pbcc == Interpolations.ProximityBasedCoefficientReduction.None) ProximityBasedCoefficientCorrection = null;
            else ProximityBasedCoefficientCorrection = Interpolations.GenProximityBasedCoefficientCorrection(pbcc);

            if (reduceTransition <= 0) TransitionReduction = null;
            else TransitionReduction = Interpolations.GenTransitionReduction(reduceTransition, TargetWidth, TargetHeight, OriginalWidth, OriginalHeight);
        }
        /// <summary>
        /// An initialization for interpolation functions - interpolation, positioning
        /// and PBCC. No transition reduction.
        /// </summary>
        /// <param name="interp">One of the pre-defined interpolations.</param>
        /// <param name="pbcc">One of the pre-defined PBCC algorithms.</param>
        public void InitializeFunctions(Interpolations.Implemented interp, Interpolations.ProximityBasedCoefficientReduction pbcc) { InitializeFunctions(interp, pbcc, 0); }
        /// <summary>
        /// An initialization for interpolation functions - interpolation, positioning
        /// and transition reduction. PBCC won't be used.
        /// </summary>
        /// <param name="interp">One of the pre-defined interpolations.</param>
        /// <param name="reduceTransition">Value of the transition reduction (pixels). Negative value means
        /// that a nieghbourhood pull will be used instead. Zero means no transition reduction function will
        /// be used.</param>
        public void InitializeFunctions(Interpolations.Implemented interp, float reduceTransition) { InitializeFunctions(interp, Interpolations.ProximityBasedCoefficientReduction.None, reduceTransition); }
        /// <summary>
        /// An initialization for interpolation functions. No transition reduction and PBCC.
        /// </summary>
        /// <param name="interp">One of the pre-defined interpolations.</param>
        public void InitializeFunctions(Interpolations.Implemented interp) { InitializeFunctions(interp, Interpolations.ProximityBasedCoefficientReduction.None, 0); }

        /// <summary>
        /// Initialization for custom transition reduction function.
        /// </summary>
        /// <param name="reduceTransition">Size of the transition in the target image (pixels). If zero or
        /// lower - no transition reduction.</param>
        public void InitializePassageReduction(float reduceTransition)
        {
            if (reduceTransition >= 0.0f) TransitionReduction = Interpolations.GenTransitionReduction(reduceTransition, TargetWidth, TargetHeight, OriginalWidth, OriginalHeight);
            else TransitionReduction = null;
        }
    }
}

namespace Processing
{
    using Casting;
    using Interpolations;

    /// <summary>
    /// A simple, byte-based, three-dimensional representation of an image (for purposes of
    /// multi-threading, not locked).
    /// </summary>
    public class Image
    {
        public const int Red = 0;
        public const int Green = 1;
        public const int Blue = 2;

        public const int R = 0;
        public const int G = 1;
        public const int B = 2;

        private byte[, ,] _image;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Colors { get; private set; }

        public byte this[int x, int y, int c]
        {
            get
            {
                if (x < 0) x = 0;
                else if (x >= Width) x = Width - 1;
                if (y < 0) y = 0;
                else if (y >= Height) y = Height - 1;
                if (c < 0) c = 0;
                else if (c >= Colors) c = Colors - 1;

                return _image[x, y, c];
            }

            set
            {
                if (x < 0) return;
                if (x >= Width) return;
                if (y < 0) return;
                if (y >= Height) return;
                if (c < 0) return;
                if (c >= Colors) return;

                _image[x, y, c] = value;
            }
        }

        public Image(int width, int height, int colors = 3)
        {
            _image = new byte[width, height, colors];
            Width = width;
            Height = height;
            Colors = colors;
        }
    }

    /// <summary>
    /// Class for resizing an image using a given interpolation.
    /// </summary>
    public class Resizer
    {
        /// <summary>
        /// The original image.
        /// </summary>
        public Image Original;
        /// <summary>
        /// The target image. Number of colours must match number of colours in
        /// the original image. Have to be user-defined.
        /// </summary>
        public Image Result;

        /// <summary>
        /// An interpolation setup. The dimensions must match dimensions of both the original
        /// and the target image.
        /// </summary>
        public Interp Interpolation;

        //current resizing function.
        private Action _currentFunction;

        //table of buffered coefficients
        private Coeff[,][] _buffer = null;

        //for setting up default resize function
        public Resizer() { _currentFunction = ParallelResize_Safe; ;}

        /// <summary>
        /// Safe resize function.
        /// </summary>
        /// <returns>If false - missing one of the elements (image, interpolation setup) or missing
        /// current function. If true - the image has been resized.</returns>
        public bool Resize()
        {
            if (Original == null) return false;
            if (Result == null) return false;
            if (Interpolation == null) return false;

            if (Interpolation.CastPosition == null) return false;
            if (Interpolation.Interpolation == null) return false;
            if (Interpolation.PositionWages == null) return false;

            if (Interpolation.TargetHeight != Result.Height) return false;
            if (Interpolation.TargetWidth != Result.Width) return false;
            if (Interpolation.OriginalHeight != Original.Height) return false;
            if (Interpolation.OriginalWidth != Original.Width) return false;

            if (Original.Colors != Result.Colors) return false;

            if (_currentFunction == null) return false;

            _currentFunction();
            //ParallelResize_Safe();

            return true;
        }

        /// <summary>
        /// Sets current function to safe option.
        /// </summary>
        /// <param name="allowParallel">Will the safe option be a parallel variant?</param>
        public void Reset(bool allowParallel = true)
        {
            if (allowParallel) _currentFunction = ParallelResize_Safe;
            else _currentFunction = SingleResize_Safe;
        }

        /// <summary>
        /// Select a current resize function according to current interpolation setup. Better optimization
        /// if the original image is defined.
        /// </summary>
        /// <param name="allowParallel">Will the optimalized function be a parallel variant?</param>
        /// <returns>If false, no interpolation setup. If true, the resizer has been optimalized.</returns>
        public bool Optimalize(bool allowParallel = true)
        {
            if (Interpolation == null) return false;
            int colors = Original == null ? -1 : Original.Colors;

            if (allowParallel)
            {
                if (_buffer != null && Result != null)
                {
                    if (_buffer.GetLength(0) != Result.Width || _buffer.GetLength(1) != Result.Height) return false;
                    switch (colors)
                    {
                        case 1: _currentFunction = ParallelResize_Buff_Mono; break;
                        case 3: _currentFunction = ParallelResize_Buff_RGB; break;
                        default: _currentFunction = ParallelResize_Buff_Clrs; break;
                    }
                    return true;
                }

                if (Interpolation.ProximityBasedCoefficientCorrection == null)
                {
                    if (Interpolation.TransitionReduction == null)
                    {
                        switch (colors)
                        {
                            case 1: _currentFunction = ParallelResize_Mono; break;
                            case 3: _currentFunction = ParallelResize_RGB; break;
                            default: _currentFunction = ParallelResize_Clrs; break;
                        }
                    }
                    else
                    {
                        switch (colors)
                        {
                            case 1: _currentFunction = ParallelResize_TR_Mono; break;
                            case 3: _currentFunction = ParallelResize_TR_RGB; break;
                            default: _currentFunction = ParallelResize_TR_Clrs; break;
                        }
                    }
                }
                else
                {
                    if (Interpolation.TransitionReduction == null)
                    {
                        switch (colors)
                        {
                            case 1: _currentFunction = ParallelResize_PBCC_Mono; break;
                            case 3: _currentFunction = ParallelResize_PBCC_RGB; break;
                            default: _currentFunction = ParallelResize_PBCC_Clrs; break;
                        }
                    }
                    else
                    {
                        switch (colors)
                        {
                            case 1: _currentFunction = ParallelResize_PBCC_TR_Mono; break;
                            case 3: _currentFunction = ParallelResize_PBCC_TR_RGB; break;
                            default: _currentFunction = ParallelResize_PBCC_TR_Clrs; break;
                        }
                    }
                }
            }
            else
            {
                if (_buffer != null && Result != null)
                {
                    if (_buffer.GetLength(0) != Result.Width || _buffer.GetLength(1) != Result.Height) return false;
                    switch (colors)
                    {
                        case 1: _currentFunction = SingleResize_Buff_Mono; break;
                        case 3: _currentFunction = SingleResize_Buff_RGB; break;
                        default: _currentFunction = SingleResize_Buff_Clrs; break;
                    }
                    return true;
                }


                if (Interpolation.ProximityBasedCoefficientCorrection == null)
                {
                    if (Interpolation.TransitionReduction == null)
                    {
                        switch (colors)
                        {
                            case 1: _currentFunction = SingleResize_Mono; break;
                            case 3: _currentFunction = SingleResize_RGB; break;
                            default: _currentFunction = SingleResize_Clrs; break;
                        }
                    }
                    else
                    {
                        switch (colors)
                        {
                            case 1: _currentFunction = SingleResize_TR_Mono; break;
                            case 3: _currentFunction = SingleResize_TR_RGB; break;
                            default: _currentFunction = SingleResize_TR_Clrs; break;
                        }
                    }
                }
                else
                {
                    if (Interpolation.TransitionReduction == null)
                    {
                        switch (colors)
                        {
                            case 1: _currentFunction = SingleResize_PBCC_Mono; break;
                            case 3: _currentFunction = SingleResize_PBCC_RGB; break;
                            default: _currentFunction = SingleResize_PBCC_Clrs; break;
                        }
                    }
                    else
                    {
                        switch (colors)
                        {
                            case 1: _currentFunction = SingleResize_PBCC_TR_Mono; break;
                            case 3: _currentFunction = SingleResize_PBCC_TR_RGB; break;
                            default: _currentFunction = SingleResize_PBCC_TR_Clrs; break;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Resize without checking if all elements are in place (faster).
        /// </summary>
        public void FastResize() { _currentFunction(); }

        /// <summary>
        /// Forces resize with a safe parallel resize function.
        /// </summary>
        public void ForceParallelResize() { ParallelResize_Safe(); }

        /// <summary>
        /// Forces resize with a safe singe-thread resize function.
        /// </summary>
        public void ForceSingleResize() { SingleResize_Safe(); }

        /// <summary>
        /// Buffers all coeffcients obtained by the current setting.
        /// </summary>
        /// <returns>True, if the buffering was successfull.</returns>
        public bool Buffer()
        {
            if (Original == null) return false;
            if (Result == null) return false;
            if (Interpolation == null) return false;

            if (Interpolation.CastPosition == null) return false;
            if (Interpolation.Interpolation == null) return false;
            if (Interpolation.PositionWages == null) return false;

            if (Interpolation.TargetHeight != Result.Height) return false;
            if (Interpolation.TargetWidth != Result.Width) return false;
            if (Interpolation.OriginalHeight != Original.Height) return false;
            if (Interpolation.OriginalWidth != Original.Width) return false;

            _buffer = new Coeff[Result.Width, Result.Height][];
            int height_1 = Result.Height - 1;

            for (int ixt = Result.Width - 1; ixt >= 0; --ixt)
            {
                for (int iyt = height_1; iyt >= 0; --iyt)
                {
                    ImPoint p = Interpolation.CastPosition(ixt, iyt);
                    InterpPoint2D p_prim = Interpolation.PositionWages(p.ix, p.iy);
                    ImPoint p_bis = (Interpolation.TransitionReduction == null) ? p_prim.nPoint : Interpolation.TransitionReduction(p_prim.nPoint.ix, p_prim.nPoint.iy);
                    float[,] w = Interpolation.Interpolation(p_bis.ix, p_bis.iy);
                    if (Interpolation.ProximityBasedCoefficientCorrection != null) w = Interpolation.ProximityBasedCoefficientCorrection(w, p_bis.ix, p_bis.iy);

                    _buffer[ixt, iyt] = new Coeff[w.Length];

                    for (int d = w.Length - 1; d >= 0; --d)
                    {
                        int dy = d / w.GetLength(0);
                        int dx = d % w.GetLength(0);

                        _buffer[ixt, iyt][d] = new Coeff()
                        {
                            X = p_prim.I0.ix + dx,
                            Y = p_prim.I0.iy + dy,
                            W = w[dx, dy]
                        };
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// If the last resizer setting was buffered.
        /// </summary>
        public bool IsBuffered { get { return _buffer != null; } }

        /// <summary>
        /// Removes current buffer.
        /// </summary>
        public void RemoveBuffer() { _buffer = null; }

        //below - optimized resize functions
        //for multithreading
        //safe variant
        private void ParallelResize_Safe()
        {
            int height_1 = Result.Height - 1;
            int colors_1 = Result.Colors - 1;
            Parallel.For(0, Result.Width, (ixt) =>
            {
                for (int iyt = height_1; iyt >= 0; --iyt)
                {
                    ImPoint p = Interpolation.CastPosition(ixt, iyt);
                    InterpPoint2D p_prim = Interpolation.PositionWages(p.ix, p.iy);
                    ImPoint p_bis = (Interpolation.TransitionReduction == null) ? p_prim.nPoint : Interpolation.TransitionReduction(p_prim.nPoint.ix, p_prim.nPoint.iy);
                    float[,] w = Interpolation.Interpolation(p_bis.ix, p_bis.iy);
                    if(Interpolation.ProximityBasedCoefficientCorrection != null) w = Interpolation.ProximityBasedCoefficientCorrection(w, p_bis.ix, p_bis.iy);

                    for (int c = colors_1; c >= 0; --c)
                    {
                        float S = 0;
                        for (int d = w.Length - 1; d >= 0; --d)
                        {
                            int dy = d / w.GetLength(0);
                            int dx = d % w.GetLength(0);

                            S += (float)Original[p_prim.I0.ix + dx, p_prim.I0.iy + dy, c] * w[dx, dy];
                        }
                        Result[ixt, iyt, c] = (byte)S;
                    }
                }
            });
        }

        //PBCC, transition reducntion, any number of colours
        private void ParallelResize_PBCC_TR_Clrs()
        {
            int height_1 = Result.Height - 1;
            int colors_1 = Result.Colors - 1;
            Parallel.For(0, Result.Width, (ixt) =>
            {
                for (int iyt = height_1; iyt >= 0; --iyt)
                {
                    ImPoint p = Interpolation.CastPosition(ixt, iyt);
                    InterpPoint2D p_prim = Interpolation.PositionWages(p.ix, p.iy);
                    ImPoint p_bis = Interpolation.TransitionReduction(p_prim.nPoint.ix, p_prim.nPoint.iy);
                    float[,] w = Interpolation.Interpolation(p_bis.ix, p_bis.iy);
                    w = Interpolation.ProximityBasedCoefficientCorrection(w, p_bis.ix, p_bis.iy);

                    for (int c = colors_1; c >= 0; --c)
                    {
                        float S = 0;
                        for (int d = w.Length - 1; d >= 0; --d)
                        {
                            int dy = d / w.GetLength(0);
                            int dx = d % w.GetLength(0);

                            S += (float)Original[p_prim.I0.ix + dx, p_prim.I0.iy + dy, c] * w[dx, dy];
                        }
                        Result[ixt, iyt, c] = (byte)S;
                    }
                }
            });
        }

        //PBCC, transition reduction, three colour components
        private void ParallelResize_PBCC_TR_RGB()
        {
            int height_1 = Result.Height - 1;
            Parallel.For(0, Result.Width, (ixt) =>
            {
                for (int iyt = height_1; iyt >= 0; --iyt)
                {
                    ImPoint p = Interpolation.CastPosition(ixt, iyt);
                    InterpPoint2D p_prim = Interpolation.PositionWages(p.ix, p.iy);
                    ImPoint p_bis = Interpolation.TransitionReduction(p_prim.nPoint.ix, p_prim.nPoint.iy);
                    float[,] w = Interpolation.Interpolation(p_bis.ix, p_bis.iy);
                    w = Interpolation.ProximityBasedCoefficientCorrection(w, p_bis.ix, p_bis.iy);


                    float SR = 0;
                    float SG = 0;
                    float SB = 0;
                    for (int d = w.Length - 1; d >= 0; --d)
                    {
                        int dy = d / w.GetLength(0);
                        int dx = d % w.GetLength(0);

                        SR += (float)Original[p_prim.I0.ix + dx, p_prim.I0.iy + dy, 0] * w[dx, dy];
                        SG += (float)Original[p_prim.I0.ix + dx, p_prim.I0.iy + dy, 1] * w[dx, dy];
                        SB += (float)Original[p_prim.I0.ix + dx, p_prim.I0.iy + dy, 2] * w[dx, dy];
                    }
                    Result[ixt, iyt, 0] = (byte)SR;
                    Result[ixt, iyt, 1] = (byte)SG;
                    Result[ixt, iyt, 2] = (byte)SB;
                }
            });
        }

        //PBCC, transition reduction, one colour component
        private void ParallelResize_PBCC_TR_Mono()
        {
            int height_1 = Result.Height - 1;
            Parallel.For(0, Result.Width, (ixt) =>
            {
                for (int iyt = height_1; iyt >= 0; --iyt)
                {
                    ImPoint p = Interpolation.CastPosition(ixt, iyt);
                    InterpPoint2D p_prim = Interpolation.PositionWages(p.ix, p.iy);
                    ImPoint p_bis = Interpolation.TransitionReduction(p_prim.nPoint.ix, p_prim.nPoint.iy);
                    float[,] w = Interpolation.Interpolation(p_bis.ix, p_bis.iy);
                    w = Interpolation.ProximityBasedCoefficientCorrection(w, p_bis.ix, p_bis.iy);


                    float S = 0;
                    for (int d = w.Length - 1; d >= 0; --d)
                    {
                        int dy = d / w.GetLength(0);
                        int dx = d % w.GetLength(0);

                        S += (float)Original[p_prim.I0.ix + dx, p_prim.I0.iy + dy, 0] * w[dx, dy];
                    }
                    Result[ixt, iyt, 0] = (byte)S;
                }
            });
        }

        //transition reduction, any number of colours; without PBCC
        private void ParallelResize_TR_Clrs()
        {
            int height_1 = Result.Height - 1;
            int colors_1 = Result.Colors - 1;
            Parallel.For(0, Result.Width, (ixt) =>
            {
                for (int iyt = height_1; iyt >= 0; --iyt)
                {
                    ImPoint p = Interpolation.CastPosition(ixt, iyt);
                    InterpPoint2D p_prim = Interpolation.PositionWages(p.ix, p.iy);
                    ImPoint p_bis = Interpolation.TransitionReduction(p_prim.nPoint.ix, p_prim.nPoint.iy);
                    float[,] w = Interpolation.Interpolation(p_bis.ix, p_bis.iy);

                    for (int c = colors_1; c >= 0; --c)
                    {
                        float S = 0;
                        for (int d = w.Length - 1; d >= 0; --d)
                        {
                            int dy = d / w.GetLength(0);
                            int dx = d % w.GetLength(0);

                            S += (float)Original[p_prim.I0.ix + dx, p_prim.I0.iy + dy, c] * w[dx, dy];
                        }
                        Result[ixt, iyt, c] = (byte)S;
                    }
                }
            });
        }

        //transition reduction, three colour components; without PBCC
        private void ParallelResize_TR_RGB()
        {
            int height_1 = Result.Height - 1;
            Parallel.For(0, Result.Width, (ixt) =>
            {
                for (int iyt = height_1; iyt >= 0; --iyt)
                {
                    ImPoint p = Interpolation.CastPosition(ixt, iyt);
                    InterpPoint2D p_prim = Interpolation.PositionWages(p.ix, p.iy);
                    ImPoint p_bis = Interpolation.TransitionReduction(p_prim.nPoint.ix, p_prim.nPoint.iy);
                    float[,] w = Interpolation.Interpolation(p_bis.ix, p_bis.iy);

                    float SR = 0;
                    float SG = 0;
                    float SB = 0;
                    for (int d = w.Length - 1; d >= 0; --d)
                    {
                        int dy = d / w.GetLength(0);
                        int dx = d % w.GetLength(0);

                        SR += (float)Original[p_prim.I0.ix + dx, p_prim.I0.iy + dy, 0] * w[dx, dy];
                        SG += (float)Original[p_prim.I0.ix + dx, p_prim.I0.iy + dy, 1] * w[dx, dy];
                        SB += (float)Original[p_prim.I0.ix + dx, p_prim.I0.iy + dy, 2] * w[dx, dy];
                    }
                    Result[ixt, iyt, 0] = (byte)SR;
                    Result[ixt, iyt, 1] = (byte)SG;
                    Result[ixt, iyt, 2] = (byte)SB;
                }
            });
        }

        //transition reduction, one colour component; without PBCC
        private void ParallelResize_TR_Mono()
        {
            int height_1 = Result.Height - 1;
            Parallel.For(0, Result.Width, (ixt) =>
            {
                for (int iyt = height_1; iyt >= 0; --iyt)
                {
                    ImPoint p = Interpolation.CastPosition(ixt, iyt);
                    InterpPoint2D p_prim = Interpolation.PositionWages(p.ix, p.iy);
                    ImPoint p_bis = Interpolation.TransitionReduction(p_prim.nPoint.ix, p_prim.nPoint.iy);
                    float[,] w = Interpolation.Interpolation(p_bis.ix, p_bis.iy);

                    float S = 0;
                    for (int d = w.Length - 1; d >= 0; --d)
                    {
                        int dy = d / w.GetLength(0);
                        int dx = d % w.GetLength(0);

                        S += (float)Original[p_prim.I0.ix + dx, p_prim.I0.iy + dy, 0] * w[dx, dy];
                    }
                    Result[ixt, iyt, 0] = (byte)S;
                }
            });
        }

        //PBCC, any number of colours; without transition reduction
        private void ParallelResize_PBCC_Clrs()
        {
            int height_1 = Result.Height - 1;
            int colors_1 = Result.Colors - 1;
            Parallel.For(0, Result.Width, (ixt) =>
            {
                for (int iyt = height_1; iyt >= 0; --iyt)
                {
                    ImPoint p = Interpolation.CastPosition(ixt, iyt);
                    InterpPoint2D p_prim = Interpolation.PositionWages(p.ix, p.iy);
                    float[,] w = Interpolation.Interpolation(p_prim.nPoint.ix, p_prim.nPoint.iy);
                    w = Interpolation.ProximityBasedCoefficientCorrection(w, p_prim.nPoint.ix, p_prim.nPoint.iy);

                    for (int c = colors_1; c >= 0; --c)
                    {
                        float S = 0;
                        for (int d = w.Length - 1; d >= 0; --d)
                        {
                            int dy = d / w.GetLength(0);
                            int dx = d % w.GetLength(0);

                            S += (float)Original[p_prim.I0.ix + dx, p_prim.I0.iy + dy, c] * w[dx, dy];
                        }
                        Result[ixt, iyt, c] = (byte)S;
                    }
                }
            });
        }

        //PBCC, three colour components; without transition reduction
        private void ParallelResize_PBCC_RGB()
        {
            int height_1 = Result.Height - 1;
            Parallel.For(0, Result.Width, (ixt) =>
            {
                for (int iyt = height_1; iyt >= 0; --iyt)
                {
                    ImPoint p = Interpolation.CastPosition(ixt, iyt);
                    InterpPoint2D p_prim = Interpolation.PositionWages(p.ix, p.iy);
                    float[,] w = Interpolation.Interpolation(p_prim.nPoint.ix, p_prim.nPoint.iy);
                    w = Interpolation.ProximityBasedCoefficientCorrection(w, p_prim.nPoint.ix, p_prim.nPoint.iy);

                    float SR = 0;
                    float SG = 0;
                    float SB = 0;
                    for (int d = w.Length - 1; d >= 0; --d)
                    {
                        int dy = d / w.GetLength(0);
                        int dx = d % w.GetLength(0);

                        SR += (float)Original[p_prim.I0.ix + dx, p_prim.I0.iy + dy, 0] * w[dx, dy];
                        SG += (float)Original[p_prim.I0.ix + dx, p_prim.I0.iy + dy, 1] * w[dx, dy];
                        SB += (float)Original[p_prim.I0.ix + dx, p_prim.I0.iy + dy, 2] * w[dx, dy];
                    }
                    Result[ixt, iyt, 0] = (byte)SR;
                    Result[ixt, iyt, 1] = (byte)SG;
                    Result[ixt, iyt, 2] = (byte)SB;
                }
            });
        }

        //PBCC, three colour components; without transition reduction
        private void ParallelResize_PBCC_Mono()
        {
            int height_1 = Result.Height - 1;
            Parallel.For(0, Result.Width, (ixt) =>
            {
                for (int iyt = height_1; iyt >= 0; --iyt)
                {
                    ImPoint p = Interpolation.CastPosition(ixt, iyt);
                    InterpPoint2D p_prim = Interpolation.PositionWages(p.ix, p.iy);
                    float[,] w = Interpolation.Interpolation(p_prim.nPoint.ix, p_prim.nPoint.iy);
                    w = Interpolation.ProximityBasedCoefficientCorrection(w, p_prim.nPoint.ix, p_prim.nPoint.iy);

                    float S = 0;
                    for (int d = w.Length - 1; d >= 0; --d)
                    {
                        int dy = d / w.GetLength(0);
                        int dx = d % w.GetLength(0);

                        S += (float)Original[p_prim.I0.ix + dx, p_prim.I0.iy + dy, 0] * w[dx, dy];
                    }
                    Result[ixt, iyt, 0] = (byte)S;
                }
            });
        }

        //any number of colours; without PBCC and transition reduction
        private void ParallelResize_Clrs()
        {
            int height_1 = Result.Height - 1;
            int colors_1 = Result.Colors - 1;
            Parallel.For(0, Result.Width, (ixt) =>
            {
                for (int iyt = height_1; iyt >= 0; --iyt)
                {
                    ImPoint p = Interpolation.CastPosition(ixt, iyt);
                    InterpPoint2D p_prim = Interpolation.PositionWages(p.ix, p.iy);
                    float[,] w = Interpolation.Interpolation(p_prim.nPoint.ix, p_prim.nPoint.iy);

                    for (int c = colors_1; c >= 0; --c)
                    {
                        float S = 0;
                        for (int d = w.Length - 1; d >= 0; --d)
                        {
                            int dy = d / w.GetLength(0);
                            int dx = d % w.GetLength(0);

                            S += (float)Original[p_prim.I0.ix + dx, p_prim.I0.iy + dy, c] * w[dx, dy];
                        }
                        Result[ixt, iyt, c] = (byte)S;
                    }
                }
            });
        }

        //three colour components; without PBCC and transition reduction
        private void ParallelResize_RGB()
        {
            int height_1 = Result.Height - 1;
            Parallel.For(0, Result.Width, (ixt) =>
            {
                for (int iyt = height_1; iyt >= 0; --iyt)
                {
                    ImPoint p = Interpolation.CastPosition(ixt, iyt);
                    InterpPoint2D p_prim = Interpolation.PositionWages(p.ix, p.iy);
                    float[,] w = Interpolation.Interpolation(p_prim.nPoint.ix, p_prim.nPoint.iy);

                    float SR = 0;
                    float SG = 0;
                    float SB = 0;
                    for (int d = w.Length - 1; d >= 0; --d)
                    {
                        int dy = d / w.GetLength(0);
                        int dx = d % w.GetLength(0);

                        SR += (float)Original[p_prim.I0.ix + dx, p_prim.I0.iy + dy, 0] * w[dx, dy];
                        SG += (float)Original[p_prim.I0.ix + dx, p_prim.I0.iy + dy, 1] * w[dx, dy];
                        SB += (float)Original[p_prim.I0.ix + dx, p_prim.I0.iy + dy, 2] * w[dx, dy];
                    }
                    Result[ixt, iyt, 0] = (byte)SR;
                    Result[ixt, iyt, 1] = (byte)SG;
                    Result[ixt, iyt, 2] = (byte)SB;
                }
            });
        }

        //one colour component; without PBCC and transition reduction
        private void ParallelResize_Mono()
        {
            int height_1 = Result.Height - 1;
            Parallel.For(0, Result.Width, (ixt) =>
            {
                for (int iyt = height_1; iyt >= 0; --iyt)
                {
                    ImPoint p = Interpolation.CastPosition(ixt, iyt);
                    InterpPoint2D p_prim = Interpolation.PositionWages(p.ix, p.iy);
                    float[,] w = Interpolation.Interpolation(p_prim.nPoint.ix, p_prim.nPoint.iy);

                    float S = 0;
                    for (int d = w.Length - 1; d >= 0; --d)
                    {
                        int dy = d / w.GetLength(0);
                        int dx = d % w.GetLength(0);

                        S += (float)Original[p_prim.I0.ix + dx, p_prim.I0.iy + dy, 0] * w[dx, dy];
                    }
                    Result[ixt, iyt, 0] = (byte)S;
                }
            });
        }

        //any amount of colours, buffered
        private void ParallelResize_Buff_Clrs()
        {
            int height_1 = Result.Height - 1;
            int colors_1 = Result.Colors - 1;
            Parallel.For(0, Result.Width, (ixt) =>
            {
                for (int iyt = height_1; iyt >= 0; --iyt)
                {
                    for (int c = colors_1; c >= 0; --c)
                    {
                        float S = 0;
                        for (int d = _buffer[ixt, iyt].Length - 1; d >= 0; --d)
                        {
                            S += (float)Original[_buffer[ixt, iyt][d].X, _buffer[ixt, iyt][d].Y, c] * _buffer[ixt, iyt][d].W;
                        }
                        Result[ixt, iyt, c] = (byte)S;
                    }
                }
            });
        }

        //three colour components, buffered
        private void ParallelResize_Buff_RGB()
        {
            int height_1 = Result.Height - 1;
            Parallel.For(0, Result.Width, (ixt) =>
            {
                for (int iyt = height_1; iyt >= 0; --iyt)
                {
                    float SR = 0;
                    float SG = 0;
                    float SB = 0;

                    for (int d = _buffer[ixt, iyt].Length - 1; d >= 0; --d)
                    {
                        SR += (float)Original[_buffer[ixt, iyt][d].X, _buffer[ixt, iyt][d].Y, Image.R] * _buffer[ixt, iyt][d].W;
                        SG += (float)Original[_buffer[ixt, iyt][d].X, _buffer[ixt, iyt][d].Y, Image.G] * _buffer[ixt, iyt][d].W;
                        SB += (float)Original[_buffer[ixt, iyt][d].X, _buffer[ixt, iyt][d].Y, Image.B] * _buffer[ixt, iyt][d].W;
                    }
                    Result[ixt, iyt, Image.R] = (byte)SR;
                    Result[ixt, iyt, Image.G] = (byte)SG;
                    Result[ixt, iyt, Image.B] = (byte)SB;
                }
            });
        }

        //one colour component, buffered
        private void ParallelResize_Buff_Mono()
        {
            int height_1 = Result.Height - 1;
            Parallel.For(0, Result.Width, (ixt) =>
            {
                for (int iyt = height_1; iyt >= 0; --iyt)
                {
                        float S = 0;
                        for (int d = _buffer[ixt, iyt].Length - 1; d >= 0; --d)
                        {
                            S += (float)Original[_buffer[ixt, iyt][d].X, _buffer[ixt, iyt][d].Y, 0] * _buffer[ixt, iyt][d].W;
                        }
                        Result[ixt, iyt, 0] = (byte)S;
                }
            });
        }


        //for single thread
        //safe variant
        private void SingleResize_Safe()
        {
            int height_1 = Result.Height - 1;
            int colors_1 = Result.Colors - 1;
            for (int ixt = Result.Width - 1; ixt >= 0; --ixt)
            {
                for (int iyt = height_1; iyt >= 0; --iyt)
                {
                    ImPoint p = Interpolation.CastPosition(ixt, iyt);
                    InterpPoint2D p_prim = Interpolation.PositionWages(p.ix, p.iy);
                    ImPoint p_bis = (Interpolation.TransitionReduction == null) ? p_prim.nPoint : Interpolation.TransitionReduction(p_prim.nPoint.ix, p_prim.nPoint.iy);
                    float[,] w = Interpolation.Interpolation(p_bis.ix, p_bis.iy);
                    if (Interpolation.ProximityBasedCoefficientCorrection != null) w = Interpolation.ProximityBasedCoefficientCorrection(w, p_bis.ix, p_bis.iy);

                    for (int c = colors_1; c >= 0; --c)
                    {
                        float S = 0;
                        for (int d = w.Length - 1; d >= 0; --d)
                        {
                            int dy = d / w.GetLength(0);
                            int dx = d % w.GetLength(0);

                            S += (float)Original[p_prim.I0.ix + dx, p_prim.I0.iy + dy, c] * w[dx, dy];
                        }
                        Result[ixt, iyt, c] = (byte)S;
                    }
                }
            }
        }

        //PBCC, transition reduction, any number of colours
        private void SingleResize_PBCC_TR_Clrs()
        {
            int height_1 = Result.Height - 1;
            int colors_1 = Result.Colors - 1;
            for (int ixt = Result.Width - 1; ixt >= 0; --ixt)
            {
                for (int iyt = height_1; iyt >= 0; --iyt)
                {
                    ImPoint p = Interpolation.CastPosition(ixt, iyt);
                    InterpPoint2D p_prim = Interpolation.PositionWages(p.ix, p.iy);
                    ImPoint p_bis = Interpolation.TransitionReduction(p_prim.nPoint.ix, p_prim.nPoint.iy);
                    float[,] w = Interpolation.Interpolation(p_bis.ix, p_bis.iy);
                    w = Interpolation.ProximityBasedCoefficientCorrection(w, p_bis.ix, p_bis.iy);

                    for (int c = colors_1; c >= 0; --c)
                    {
                        float S = 0;
                        for (int d = w.Length - 1; d >= 0; --d)
                        {
                            int dy = d / w.GetLength(0);
                            int dx = d % w.GetLength(0);

                            S += (float)Original[p_prim.I0.ix + dx, p_prim.I0.iy + dy, c] * w[dx, dy];
                        }
                        Result[ixt, iyt, c] = (byte)S;
                    }
                }
            }
        }

        //PBCC, transition reduction, three colour components
        private void SingleResize_PBCC_TR_RGB()
        {
            int height_1 = Result.Height - 1;
            for (int ixt = Result.Width - 1; ixt >= 0; --ixt)
            {
                for (int iyt = height_1; iyt >= 0; --iyt)
                {
                    ImPoint p = Interpolation.CastPosition(ixt, iyt);
                    InterpPoint2D p_prim = Interpolation.PositionWages(p.ix, p.iy);
                    ImPoint p_bis = Interpolation.TransitionReduction(p_prim.nPoint.ix, p_prim.nPoint.iy);
                    float[,] w = Interpolation.Interpolation(p_bis.ix, p_bis.iy);
                    w = Interpolation.ProximityBasedCoefficientCorrection(w, p_bis.ix, p_bis.iy);


                    float SR = 0;
                    float SG = 0;
                    float SB = 0;
                    for (int d = w.Length - 1; d >= 0; --d)
                    {
                        int dy = d / w.GetLength(0);
                        int dx = d % w.GetLength(0);

                        SR += (float)Original[p_prim.I0.ix + dx, p_prim.I0.iy + dy, 0] * w[dx, dy];
                        SG += (float)Original[p_prim.I0.ix + dx, p_prim.I0.iy + dy, 1] * w[dx, dy];
                        SB += (float)Original[p_prim.I0.ix + dx, p_prim.I0.iy + dy, 2] * w[dx, dy];
                    }
                    Result[ixt, iyt, 0] = (byte)SR;
                    Result[ixt, iyt, 1] = (byte)SG;
                    Result[ixt, iyt, 2] = (byte)SB;
                }
            }
        }

        //PBCC, transition reduction, one colour component
        private void SingleResize_PBCC_TR_Mono()
        {
            int height_1 = Result.Height - 1;
            for (int ixt = Result.Width - 1; ixt >= 0; --ixt)
            {
                for (int iyt = height_1; iyt >= 0; --iyt)
                {
                    ImPoint p = Interpolation.CastPosition(ixt, iyt);
                    InterpPoint2D p_prim = Interpolation.PositionWages(p.ix, p.iy);
                    ImPoint p_bis = Interpolation.TransitionReduction(p_prim.nPoint.ix, p_prim.nPoint.iy);
                    float[,] w = Interpolation.Interpolation(p_bis.ix, p_bis.iy);
                    w = Interpolation.ProximityBasedCoefficientCorrection(w, p_bis.ix, p_bis.iy);


                    float S = 0;
                    for (int d = w.Length - 1; d >= 0; --d)
                    {
                        int dy = d / w.GetLength(0);
                        int dx = d % w.GetLength(0);

                        S += (float)Original[p_prim.I0.ix + dx, p_prim.I0.iy + dy, 0] * w[dx, dy];
                    }
                    Result[ixt, iyt, 0] = (byte)S;
                }
            }
        }

        //trnasition reduction, any number of colours; without PBCC
        private void SingleResize_TR_Clrs()
        {
            int height_1 = Result.Height - 1;
            int colors_1 = Result.Colors - 1;
            for (int ixt = Result.Width - 1; ixt >= 0; --ixt)
            {
                for (int iyt = height_1; iyt >= 0; --iyt)
                {
                    ImPoint p = Interpolation.CastPosition(ixt, iyt);
                    InterpPoint2D p_prim = Interpolation.PositionWages(p.ix, p.iy);
                    ImPoint p_bis = Interpolation.TransitionReduction(p_prim.nPoint.ix, p_prim.nPoint.iy);
                    float[,] w = Interpolation.Interpolation(p_bis.ix, p_bis.iy);

                    for (int c = colors_1; c >= 0; --c)
                    {
                        float S = 0;
                        for (int d = w.Length - 1; d >= 0; --d)
                        {
                            int dy = d / w.GetLength(0);
                            int dx = d % w.GetLength(0);

                            S += (float)Original[p_prim.I0.ix + dx, p_prim.I0.iy + dy, c] * w[dx, dy];
                        }
                        Result[ixt, iyt, c] = (byte)S;
                    }
                }
            }
        }

        //transition reduction, three colour components; without PBCC
        private void SingleResize_TR_RGB()
        {
            int height_1 = Result.Height - 1;
            for (int ixt = Result.Width - 1; ixt >= 0; --ixt)
            {
                for (int iyt = height_1; iyt >= 0; --iyt)
                {
                    ImPoint p = Interpolation.CastPosition(ixt, iyt);
                    InterpPoint2D p_prim = Interpolation.PositionWages(p.ix, p.iy);
                    ImPoint p_bis = Interpolation.TransitionReduction(p_prim.nPoint.ix, p_prim.nPoint.iy);
                    float[,] w = Interpolation.Interpolation(p_bis.ix, p_bis.iy);

                    float SR = 0;
                    float SG = 0;
                    float SB = 0;
                    for (int d = w.Length - 1; d >= 0; --d)
                    {
                        int dy = d / w.GetLength(0);
                        int dx = d % w.GetLength(0);

                        SR += (float)Original[p_prim.I0.ix + dx, p_prim.I0.iy + dy, 0] * w[dx, dy];
                        SG += (float)Original[p_prim.I0.ix + dx, p_prim.I0.iy + dy, 1] * w[dx, dy];
                        SB += (float)Original[p_prim.I0.ix + dx, p_prim.I0.iy + dy, 2] * w[dx, dy];
                    }
                    Result[ixt, iyt, 0] = (byte)SR;
                    Result[ixt, iyt, 1] = (byte)SG;
                    Result[ixt, iyt, 2] = (byte)SB;
                }
            }
        }

        //passage reduction, one colour component; without PBCC
        private void SingleResize_TR_Mono()
        {
            int height_1 = Result.Height - 1;
            for (int ixt = Result.Width - 1; ixt >= 0; --ixt)
            {
                for (int iyt = height_1; iyt >= 0; --iyt)
                {
                    ImPoint p = Interpolation.CastPosition(ixt, iyt);
                    InterpPoint2D p_prim = Interpolation.PositionWages(p.ix, p.iy);
                    ImPoint p_bis = Interpolation.TransitionReduction(p_prim.nPoint.ix, p_prim.nPoint.iy);
                    float[,] w = Interpolation.Interpolation(p_bis.ix, p_bis.iy);

                    float S = 0;
                    for (int d = w.Length - 1; d >= 0; --d)
                    {
                        int dy = d / w.GetLength(0);
                        int dx = d % w.GetLength(0);

                        S += (float)Original[p_prim.I0.ix + dx, p_prim.I0.iy + dy, 0] * w[dx, dy];
                    }
                    Result[ixt, iyt, 0] = (byte)S;
                }
            }
        }

        //PBCC, any number of colours; without transition reduction
        private void SingleResize_PBCC_Clrs()
        {
            int height_1 = Result.Height - 1;
            int colors_1 = Result.Colors - 1;
            for (int ixt = Result.Width - 1; ixt >= 0; --ixt)
            {
                for (int iyt = height_1; iyt >= 0; --iyt)
                {
                    ImPoint p = Interpolation.CastPosition(ixt, iyt);
                    InterpPoint2D p_prim = Interpolation.PositionWages(p.ix, p.iy);
                    float[,] w = Interpolation.Interpolation(p_prim.nPoint.ix, p_prim.nPoint.iy);
                    w = Interpolation.ProximityBasedCoefficientCorrection(w, p_prim.nPoint.ix, p_prim.nPoint.iy);

                    for (int c = colors_1; c >= 0; --c)
                    {
                        float S = 0;
                        for (int d = w.Length - 1; d >= 0; --d)
                        {
                            int dy = d / w.GetLength(0);
                            int dx = d % w.GetLength(0);

                            S += (float)Original[p_prim.I0.ix + dx, p_prim.I0.iy + dy, c] * w[dx, dy];
                        }
                        Result[ixt, iyt, c] = (byte)S;
                    }
                }
            }
        }

        //PBCC, three colour components; without transition reduction
        private void SingleResize_PBCC_RGB()
        {
            int height_1 = Result.Height - 1;
            for (int ixt = Result.Width - 1; ixt >= 0; --ixt)
            {
                for (int iyt = height_1; iyt >= 0; --iyt)
                {
                    ImPoint p = Interpolation.CastPosition(ixt, iyt);
                    InterpPoint2D p_prim = Interpolation.PositionWages(p.ix, p.iy);
                    float[,] w = Interpolation.Interpolation(p_prim.nPoint.ix, p_prim.nPoint.iy);
                    w = Interpolation.ProximityBasedCoefficientCorrection(w, p_prim.nPoint.ix, p_prim.nPoint.iy);

                    float SR = 0;
                    float SG = 0;
                    float SB = 0;
                    for (int d = w.Length - 1; d >= 0; --d)
                    {
                        int dy = d / w.GetLength(0);
                        int dx = d % w.GetLength(0);

                        SR += (float)Original[p_prim.I0.ix + dx, p_prim.I0.iy + dy, 0] * w[dx, dy];
                        SG += (float)Original[p_prim.I0.ix + dx, p_prim.I0.iy + dy, 1] * w[dx, dy];
                        SB += (float)Original[p_prim.I0.ix + dx, p_prim.I0.iy + dy, 2] * w[dx, dy];
                    }
                    Result[ixt, iyt, 0] = (byte)SR;
                    Result[ixt, iyt, 1] = (byte)SG;
                    Result[ixt, iyt, 2] = (byte)SB;
                }
            }
        }

        //PBCC, three colour components; without transition reduction
        private void SingleResize_PBCC_Mono()
        {
            int height_1 = Result.Height - 1;
            for (int ixt = Result.Width - 1; ixt >= 0; --ixt)
            {
                for (int iyt = height_1; iyt >= 0; --iyt)
                {
                    ImPoint p = Interpolation.CastPosition(ixt, iyt);
                    InterpPoint2D p_prim = Interpolation.PositionWages(p.ix, p.iy);
                    float[,] w = Interpolation.Interpolation(p_prim.nPoint.ix, p_prim.nPoint.iy);
                    w = Interpolation.ProximityBasedCoefficientCorrection(w, p_prim.nPoint.ix, p_prim.nPoint.iy);

                    float S = 0;
                    for (int d = w.Length - 1; d >= 0; --d)
                    {
                        int dy = d / w.GetLength(0);
                        int dx = d % w.GetLength(0);

                        S += (float)Original[p_prim.I0.ix + dx, p_prim.I0.iy + dy, 0] * w[dx, dy];
                    }
                    Result[ixt, iyt, 0] = (byte)S;
                }
            }
        }

        //any number of colours; without PBCC and trnasition reduction
        private void SingleResize_Clrs()
        {
            int height_1 = Result.Height - 1;
            int colors_1 = Result.Colors - 1;
            for (int ixt = Result.Width - 1; ixt >= 0; --ixt)
            {
                for (int iyt = height_1; iyt >= 0; --iyt)
                {
                    ImPoint p = Interpolation.CastPosition(ixt, iyt);
                    InterpPoint2D p_prim = Interpolation.PositionWages(p.ix, p.iy);
                    float[,] w = Interpolation.Interpolation(p_prim.nPoint.ix, p_prim.nPoint.iy);

                    for (int c = colors_1; c >= 0; --c)
                    {
                        float S = 0;
                        for (int d = w.Length - 1; d >= 0; --d)
                        {
                            int dy = d / w.GetLength(0);
                            int dx = d % w.GetLength(0);

                            S += (float)Original[p_prim.I0.ix + dx, p_prim.I0.iy + dy, c] * w[dx, dy];
                        }
                        Result[ixt, iyt, c] = (byte)S;
                    }
                }
            }
        }

        //three colour components; without PBCC and trnasition reduction
        private void SingleResize_RGB()
        {
            int height_1 = Result.Height - 1;
            for (int ixt = Result.Width - 1; ixt >= 0; --ixt)
            {
                for (int iyt = height_1; iyt >= 0; --iyt)
                {
                    ImPoint p = Interpolation.CastPosition(ixt, iyt);
                    InterpPoint2D p_prim = Interpolation.PositionWages(p.ix, p.iy);
                    float[,] w = Interpolation.Interpolation(p_prim.nPoint.ix, p_prim.nPoint.iy);

                    float SR = 0;
                    float SG = 0;
                    float SB = 0;
                    for (int d = w.Length - 1; d >= 0; --d)
                    {
                        int dy = d / w.GetLength(0);
                        int dx = d % w.GetLength(0);

                        SR += (float)Original[p_prim.I0.ix + dx, p_prim.I0.iy + dy, 0] * w[dx, dy];
                        SG += (float)Original[p_prim.I0.ix + dx, p_prim.I0.iy + dy, 1] * w[dx, dy];
                        SB += (float)Original[p_prim.I0.ix + dx, p_prim.I0.iy + dy, 2] * w[dx, dy];
                    }
                    Result[ixt, iyt, 0] = (byte)SR;
                    Result[ixt, iyt, 1] = (byte)SG;
                    Result[ixt, iyt, 2] = (byte)SB;
                }
            }
        }

        //one colour component; without PBCC and transition reduction
        private void SingleResize_Mono()
        {
            int height_1 = Result.Height - 1;
            for (int ixt = Result.Width - 1; ixt >= 0; --ixt)
            {
                for (int iyt = height_1; iyt >= 0; --iyt)
                {
                    ImPoint p = Interpolation.CastPosition(ixt, iyt);
                    InterpPoint2D p_prim = Interpolation.PositionWages(p.ix, p.iy);
                    float[,] w = Interpolation.Interpolation(p_prim.nPoint.ix, p_prim.nPoint.iy);

                    float S = 0;
                    for (int d = w.Length - 1; d >= 0; --d)
                    {
                        int dy = d / w.GetLength(0);
                        int dx = d % w.GetLength(0);

                        S += (float)Original[p_prim.I0.ix + dx, p_prim.I0.iy + dy, 0] * w[dx, dy];
                    }
                    Result[ixt, iyt, 0] = (byte)S;
                }
            }
        }
        
        //any amount of colours, buffered
        private void SingleResize_Buff_Clrs()
        {
            int height_1 = Result.Height - 1;
            int colors_1 = Result.Colors - 1;
            for (int ixt = Result.Width - 1; ixt >= 0; --ixt)
            {
                for (int iyt = height_1; iyt >= 0; --iyt)
                {
                    for (int c = colors_1; c >= 0; --c)
                    {
                        float S = 0;
                        for (int d = _buffer[ixt, iyt].Length - 1; d >= 0; --d)
                        {
                            S += (float)Original[_buffer[ixt, iyt][d].X, _buffer[ixt, iyt][d].Y, c] * _buffer[ixt, iyt][d].W;
                        }
                        Result[ixt, iyt, c] = (byte)S;
                    }
                }
            }
        }

        //three colour components, buffered
        private void SingleResize_Buff_RGB()
        {
            int height_1 = Result.Height - 1;
            for (int ixt = Result.Width - 1; ixt >= 0; --ixt)
            {
                for (int iyt = height_1; iyt >= 0; --iyt)
                {
                    float SR = 0;
                    float SG = 0;
                    float SB = 0;

                    for (int d = _buffer[ixt, iyt].Length - 1; d >= 0; --d)
                    {
                        SR += (float)Original[_buffer[ixt, iyt][d].X, _buffer[ixt, iyt][d].Y, Image.R] * _buffer[ixt, iyt][d].W;
                        SG += (float)Original[_buffer[ixt, iyt][d].X, _buffer[ixt, iyt][d].Y, Image.G] * _buffer[ixt, iyt][d].W;
                        SB += (float)Original[_buffer[ixt, iyt][d].X, _buffer[ixt, iyt][d].Y, Image.B] * _buffer[ixt, iyt][d].W;
                    }
                    Result[ixt, iyt, Image.R] = (byte)SR;
                    Result[ixt, iyt, Image.G] = (byte)SG;
                    Result[ixt, iyt, Image.B] = (byte)SB;
                }
            }
        }

        //one colour component, buffered
        private void SingleResize_Buff_Mono()
        {
            int height_1 = Result.Height - 1;
            for (int ixt = Result.Width - 1; ixt >= 0; --ixt)
            {
                for (int iyt = height_1; iyt >= 0; --iyt)
                {
                    float S = 0;
                    for (int d = _buffer[ixt, iyt].Length - 1; d >= 0; --d)
                    {
                        S += (float)Original[_buffer[ixt, iyt][d].X, _buffer[ixt, iyt][d].Y, 0] * _buffer[ixt, iyt][d].W;
                    }
                    Result[ixt, iyt, 0] = (byte)S;
                }
            }
        }

        //struct for holding buffered coefficients
        private struct Coeff
        {
            public int X;
            public int Y;
            public float W;
        }
    }
}
