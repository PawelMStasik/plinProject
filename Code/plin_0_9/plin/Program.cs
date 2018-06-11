/*
 Copyright (c) 2018 Paweł Marek Stasik

Licensed under MIT license. Please refer to LICENSE.txt file attached to the project for more information.
  
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Casting;
using Processing;
using Interpolations;

namespace Plin
{
    public class Program
    {
        //plin {input} {scale} {output}
        //plin {input} {width} {height} {output}
        //plin {input} -{algorithm(s)} {scale} {output}
        //plin {input} {scale} -{algorithms} {output}
        //...

        //algorithms:
        //-nearest -nn -near
        //-linear -lin -l -bilinear
        //-plin -p-lin
        //-proximity_correction -pbcc | {true,1,full,yes},{false,0,no}
        //-proximity_correction -pbcc -full_proximity_correction
        //-fast_proximity_correction -fpbcc -fast_pbcc
        //-no_proximity_correction
        //-transition -tran -t | {}
        //-tran:2.2
        //plin lorem.png 2 lorem_2.png
        //plin lorem.png 10 10 -nn lorem_2_nn.png


        //poniżej - komendy wiersza poleceń (ciąg znaków | rzutowanie)
        //below - command-line arguments (argument's string | cast)
        static readonly string[] argsAlgorithms = { "nearest", "nn", "near", "linear", "lin", "l", "bilinear", "plin", "p-lin" };
        static readonly string[] argsAlgorithmsCast = { "nn", "nn", "nn", "lin", "lin", "lin", "lin", "plin", "plin" };

        static readonly string[] argsProximityCorrection = { "proximity_correction", "pbcc", "full_proximity_correction", "fast_proximity_correction", "fpbcc", "fast_pbcc", "no_proximity_correction", "no_pbcc" };
        static readonly string[] argsProximityCorrectionCast = { "full", "full", "full", "fast", "fast", "fast", "no", "no" };

        static readonly string[] argsPBCCOptions = { "true", "1", "full", "yes", "false", "0", "no"};
        static readonly string[] argsPBCCOptionsCast = { "full", "full", "full", "full", "no", "no", "no"};

        static readonly string[] argsTransitionReduction = { "transition", "tran", "t", "tar", "pull", "nearpull", "near_pull" };
        static readonly string[] argsTRansitionReductionCast = { "tran", "tran", "tran", "tran", "np", "np", "np" };

        static readonly string[] argsRotation = { "rotate", "rot", "r", "rotate_degrees", "rotate_dgr", "rot_dgr", "rdgr", "rotate_radians", "rotate_rad", "rot_rad", "rrad" };
        static readonly string[] argsRotationCast = { "dgr", "dgr", "dgr", "dgr", "dgr", "dgr", "dgr", "rad", "rad", "rad", "rad" };

        static readonly string[] argsParallel = { "parallel", "multithreading", "multithread", "mt" };

        static readonly string[] argsOptionsBoolean = { "true", "1", "yes", "t", "false", "0", "no", "f"};
        static readonly string[] argsOptionsBooleanCast = { "true", "true", "true", "true", "false", "false", "false", "false"};

        static readonly string[] argsWait = { "wait", "w", "pause" };

        static readonly string[] argsTime = { "time", "t", "measure", "measure_time", "diag", "diagnostics" };

        static readonly char[] argsSeparators = { ':', '=' };

        //ułatwienia rzutowania wartości liczbowych:
        //for easing up casting of numerical values:

        //static readonly System.Globalization.NumberStyles _numStyle = System.Globalization.NumberStyles.Float;
        //static readonly IFormatProvider _fprov = System.Globalization.CultureInfo.InvariantCulture;

        static bool TryParseDouble(string s, out double result) { return Double.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out result); }
        static bool TryParseSingle(string s, out float result) { return Single.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out result); }

        static System.Globalization.CultureInfo _CI = System.Globalization.CultureInfo.InvariantCulture;

        public static void Main(string[] args)
        {
            //pomoc przy wywołaniu bez argumentów
            if (args.Length == 0)
            {
                Func<bool> exit = () =>
                {
                    Console.WriteLine();
                    Console.WriteLine("[Any key - continue, Esc - exit]");
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Escape) return true;
                    return false;
                };

                Console.WriteLine("\tP-Lin Interpolation --- Program Help");
                Console.WriteLine();
                Console.WriteLine("Normal callouts:");
                Console.WriteLine("\tplin.exe {input_file} width height {output_file}");
                Console.WriteLine("\tplin.exe {input_file} scale {output_file}");
                Console.WriteLine();
                Console.WriteLine("\tplin.exe      - name of this program (if not changed)");
                Console.WriteLine("\t{input_file}  - path to original image, can be quoted to allow spaces");
                Console.WriteLine("\t{output_file} - path to save the file, can be quoted, if target file exists, it will be overwritten, format is obtained from file extension");
                Console.WriteLine("\twidth height  - size of the image after rescaling (integers)");
                Console.WriteLine("\tscale         - scaling factor for rescaling (floating point)");
                Console.WriteLine();
                Console.WriteLine("Supported file formats (extensions): *.bmp, *.emf, *.gif, *.jpeg, *.jpg, *.png, *.tif, *.tiff, *.wmf.");
                if (exit()) return;

                Console.WriteLine();
                Console.WriteLine("Special arguments:");
                Console.WriteLine("Remark: Special arguments might be placed anywhere after the program's name and each has to be prefixed with dash (-), e.g. -linear. Options (if avaible) are separated with ':' or '=' (no spaces)");
                Console.WriteLine();
                Console.WriteLine(" For selecting an interpolation algorithm:");
                Console.WriteLine("\tnearest near nn - nearest nieghbour");
                Console.WriteLine("\tlinear lin l bilinear - bilinear");
                Console.WriteLine("\tplin p-lin - P-Lin");
                Console.WriteLine();
                Console.WriteLine(" For a proximity-based coefficient correction:");
                Console.WriteLine("\tproximity_correction pbcc - general use; full when no parameters");
                Console.WriteLine("\t options: full, fast, no");
                Console.WriteLine("\tfull_proximity_correction = pbcc:full pbcc:yes pbcc:1 pbcc:true");
                Console.WriteLine("\tno_proximity_correction = pbcc:no pbcc:0 pbcc:false no_pbcc");
                if(exit()) return;

                Console.WriteLine(" For transition reduction:");
                Console.WriteLine("\ttransition tran tar t - for reducing a transition of interpolation between original points");
                Console.WriteLine("\tparameter means number of pixels in the output image used for the effect (floating point)");
                Console.WriteLine("\te.g. -transition:2 -tran:1.5 -tar:1");
                Console.WriteLine();
                Console.WriteLine(" Additional:");
                Console.WriteLine("\trotation rot r rotate_degrees rotate_dgr rot_dgr rdgr - rotate image counterclockwise (by degrees)");
                Console.WriteLine("\trotate_radians rotate_rad rot_rad rrad - rotate image counterclockwise (by radians)");
                Console.WriteLine("\tparallel multithreading multithread mt - allow use of Parallel.For (multithreading)");
                Console.WriteLine("\twait w pause - after processing wait for user to press any key to continue");
                Console.WriteLine("\ttime t measure measure_time diagnostics diag - measure time taken by each step and show at the end");
                Console.WriteLine(" Calling with no parameters means to turn those options on (or with zero degrees). They can also be called with parameter (boolean):");
                Console.WriteLine("\ttrue t 1 yes - for true (turn the option on)");
                Console.WriteLine("\tfalse f 0 no - for false (tunr the option off)");
                Console.WriteLine("\te.g. -mt:no -wait:true -time:yes");
                Console.WriteLine("\te.g. -rot_dgr:60.35");

                Console.WriteLine();
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey(true);
                return;
            }

            //podział poleceń na podstawowe (bez myślnika) i specjalne (z myślnikiem)
            //the commands are separated into basic- (without a dash) and special (with a dash)
            int[] iiSpecial = args.Select((arg, i) => arg.StartsWith("-") ? i : -1).Where(i => i != -1).ToArray();
            string[] basic = args.Where((arg, i) => !iiSpecial.Contains(i)).ToArray();
            string[] special = args.Where((arg, i) => iiSpecial.Contains(i)).ToArray();
            special = special.Select((arg) => arg.Substring(1).ToLowerInvariant()).ToArray();

            //odczytywane dane | acquired data
            string inputFile = null;
            string outputFile = null;
            string algorithm = null;
            string proximity_correction = null;
            float reduce = -1.0f;
            string reduction = null;
            int targetWidth = -1;
            int targetHeight = -1;
            double factor = -1.0d;
            string allowParallel = null;
            string wait = null;
            string measureTime = null;
            string rotation = null;
            double angle = 0;


            //interpretacja komend wiersza poleceń | interpretation of the command-line arguments
            if (basic.Length < 3)
            {
                Console.WriteLine("!\tNot enought arguments (missing input, output and/or size/scaling factor).");
                return;
            }
            else if (basic.Length == 3)
            {
                inputFile = basic[0];
                outputFile = basic[2];

                if (TryParseDouble(basic[1], out factor))
                {
                    if (factor < 0) factor = -factor;
                    else if (factor == 0) factor = 1;
                }
                else
                {
                    Console.WriteLine("!\tImproper scaling factor.");
                    return;
                }
            }
            else
            {
                inputFile = basic[0];
                outputFile = basic[3];
                Int32.TryParse(basic[1], out targetWidth);
                Int32.TryParse(basic[2], out targetHeight);
                factor = -1;
                if (targetHeight <= 0 || targetWidth <= 0)
                {
                    Console.WriteLine("!\tImproper output dimensions.");
                    return;
                }
            }

            foreach (string cmd in special)
            {
                string a, e;
                int sep = cmd.IndexOfAny(argsSeparators);
                if (sep < 0)
                {
                    a = cmd;
                    e = String.Empty;
                }
                else
                {
                    a = cmd.Substring(0, sep);
                    e = cmd.Substring(sep + 1);
                }

                int iArg = Array.IndexOf<string>(argsAlgorithms, a);
                if (iArg >= 0)
                {
                    if (algorithm == null) algorithm = argsAlgorithmsCast[iArg];
                    continue;
                }
                iArg = Array.IndexOf<string>(argsProximityCorrection, a);
                if (iArg >= 0)
                {
                    if (proximity_correction == null)
                    {
                        if (iArg <= 1 && e != String.Empty)
                        {
                            int dwOpt = Array.IndexOf<string>(argsPBCCOptions, e);
                            if (dwOpt >= 0) proximity_correction = argsPBCCOptionsCast[dwOpt];
                        }
                        else proximity_correction = argsProximityCorrectionCast[iArg];
                    }
                    continue;
                }
                iArg = Array.IndexOf<string>(argsTransitionReduction, a);
                if (iArg >= 0)
                {
                    if (reduction == null)
                    {
                        if (TryParseSingle(e, out reduce))
                        {
                            if (reduce < 0) reduce = -reduce;
                            if (reduce != 0) reduction = argsTRansitionReductionCast[iArg];
                        }
                    }
                }

                iArg = Array.IndexOf<string>(argsParallel, a);
                if (iArg >= 0)
                {
                    if (allowParallel == null)
                    {
                        if (e != String.Empty)
                        {
                            int prlOpt = Array.IndexOf<String>(argsOptionsBoolean, e);
                            if (prlOpt >= 0) allowParallel = argsOptionsBooleanCast[prlOpt];
                        }
                        else allowParallel = "true";
                    }
                }
                iArg = Array.IndexOf<string>(argsWait, a);
                if (iArg >= 0)
                {
                    if (wait == null)
                    {
                        if (e != String.Empty)
                        {
                            int wtOpt = Array.IndexOf<String>(argsOptionsBoolean, e);
                            if (wtOpt >= 0) wait = argsOptionsBooleanCast[wtOpt];
                        }
                        else wait = "true";
                    }
                }
                iArg = Array.IndexOf<string>(argsTime, a);
                if (iArg >= 0)
                {
                    if (measureTime == null)
                    {
                        if (e != String.Empty)
                        {
                            int mtOpt = Array.IndexOf<String>(argsOptionsBoolean, e);
                            if (mtOpt >= 0) measureTime = argsOptionsBooleanCast[mtOpt];
                        }
                        else measureTime = "true";
                    }
                }
                iArg = Array.IndexOf<string>(argsRotation, a);
                if (iArg >= 0)
                {
                    if (rotation == null)
                    {
                        if (e != String.Empty)
                        {
                            if (TryParseDouble(e, out angle))
                            {
                                rotation = argsRotationCast[iArg];
                            }
                        }
                    }
                }
            }

            //ustawianie wartości domyślnych jeśli nie określono parametró
            //settinf up default values for variables if those parametrów weren't defined
            if (reduction == null) reduction = "no";
            if (algorithm == null) algorithm = "plin";
            if (proximity_correction == null) proximity_correction = "no";
            if (allowParallel == null) allowParallel = "true";
            if (wait == null) wait = "false";
            if (measureTime == null) measureTime = "false";
            if (rotation == null) rotation = "no";

            bool diagnostics = measureTime == "true";
            bool parallel = allowParallel == "true";
            bool stable = rotation == "no";

            //inputFile = inputFile.Trim();
            //outputFile = outputFile.Trim();

            System.DateTime T00 = new DateTime(); //rozpoczęcie pracy | start of work
            System.DateTime T10 = new DateTime(); //rozpoczęcie ustawiania interpolacji | start of setting-up the interpolation
            System.DateTime T20 = new DateTime(); //rozpoczęcie interpolacji | start of the interpolation
            System.DateTime T30 = new DateTime(); //rozpoczęcie zapisywania wyniku | start of saving the result
            System.DateTime T40 = new DateTime(); //po zapisaniu wyniku | after saving the result

            if (diagnostics) T00 = System.DateTime.Now;

            if (!System.IO.File.Exists(inputFile))
            {
                Console.WriteLine("!\tFile \"{0}\" doesn't exists.", inputFile);
                return;
            }


            Image original;
            using (System.Drawing.Bitmap img = new System.Drawing.Bitmap(inputFile))
            {

                original = new Image(img.Width, img.Height, 3);
                for (int y = original.Height - 1; y >= 0; --y)
                    for (int x = original.Width - 1; x >= 0; --x)
                    {
                        var c = img.GetPixel(x, y);
                        original[x, y, Image.R] = c.R;
                        original[x, y, Image.G] = c.G;
                        original[x, y, Image.B] = c.B;
                    }
            }

            if (diagnostics) T10 = System.DateTime.Now;

            if (factor > 0)
            {
                targetHeight = (int)(Math.Round(factor * (double)original.Height));
                targetWidth = (int)(Math.Round(factor * (double)original.Width));
            }

            Image target = new Image(targetWidth, targetHeight, 3);



            Interpolations.Interpolations.Implemented theAlgorithm = Interpolations.Interpolations.Implemented.PLin;
            Interpolations.Interpolations.ProximityBasedCoefficientReduction theDW = Interpolations.Interpolations.ProximityBasedCoefficientReduction.None;

            switch (algorithm)
            {
                case "lin":
                    theAlgorithm = Interpolations.Interpolations.Implemented.Linear;
                    break;
                case "nn":
                    theAlgorithm = Interpolations.Interpolations.Implemented.NearestNeighbour;
                    break;
                case "plin":
                default:
                    theAlgorithm = Interpolations.Interpolations.Implemented.PLin;
                    break;
            }
            switch (proximity_correction)
            {
                case "full":
                    theDW = Interpolations.Interpolations.ProximityBasedCoefficientReduction.Full;
                    break;
                case "no":
                default:
                    theDW = Interpolations.Interpolations.ProximityBasedCoefficientReduction.None;
                    break;
            }

            //ustawianie paramaterów i funkcji przeskalowania
            //setting up the parameters and functions of the resize
            Casting.Blending2D blending = null;
            Resizer resizer = new Resizer();
            resizer.Original = original;

            if (stable)
            {
                resizer.Result = target;

                resizer.Interpolation = new Interp();
                resizer.Interpolation.InitializeResize(original.Width, original.Height, target.Width, target.Height,
                    Casting.Function.GenProperCasting(original.Width, target.Width), Casting.Function.GenProperCasting(original.Height, target.Height));
                //resizer.Interpolation.InitializeFunctions(Interpolations.Interpolations.Implemented.DLin, Interpolations.Interpolations.DistanceWaging.Full, 2);
                resizer.Interpolation.InitializeFunctions(theAlgorithm, theDW);
            }
            else
            {
                Transformations.TransformationSetup t = new Transformations.TransformationSetup(true);
                t.OriginalWidth = original.Width;
                t.OriginalHeight = original.Height;
                t.RelativeScaling = false;
                t.ScalingX = targetWidth;
                t.ScalingY = targetHeight;

                t.RotationInDegrees = rotation == "dgr";
                t.RotationRescaling = true;
                t.RotationAngle = angle;

                Transformations.TransformationPrototype prototype = t.CalculateTransformation();

                targetHeight = (int)(Math.Round((double)prototype.TargetHeight));
                targetWidth = (int)(Math.Round((double)prototype.TargetWidth));
                target = new Image(targetWidth, targetHeight, 3);

                resizer.Result = target;

                Cast2D casting = Casting.Function.FromTransformationMatrix(prototype.T_Target2Original, 0.5f);

                resizer.Interpolation = new Interp();
                resizer.Interpolation.InitializeTransformation(original.Width, original.Height, target.Width, target.Height, casting);
                resizer.Interpolation.InitializeFunctions(theAlgorithm, theDW);

                //blending = Casting.Function.FastBlendingFromCasting(casting, 0.5f, 0.5f, original.Width, original.Height);
                blending = Casting.Function.BlendingFromCasting(casting, Casting.Function.PlinBlending, 0.5f, 0.5f, original.Width, original.Height);
            }

            if (reduction == "tran") resizer.Interpolation.InitializePassageReduction(reduce);
            else resizer.Interpolation.TransitionReduction = null;



            if (!resizer.Buffer()) resizer.RemoveBuffer();

            resizer.Optimalize(parallel);

            if (diagnostics) T20 = System.DateTime.Now;

            bool r = resizer.Resize();

            if (diagnostics) T30 = System.DateTime.Now;

            //zapisywanie obrazu wynikowego
            //saving up the resulting image
            using (System.Drawing.Bitmap img2 = new System.Drawing.Bitmap(target.Width, target.Height))
            {
                if (blending == null)
                    for (int y = target.Height - 1; y >= 0; --y)
                        for (int x = target.Width - 1; x >= 0; --x)
                        {
                            img2.SetPixel(x, y, System.Drawing.Color.FromArgb(target[x, y, Image.R], target[x, y, Image.G], target[x, y, Image.B]));
                        }
                else
                {
                    for (int y = target.Height - 1; y >= 0; --y)
                        for (int x = target.Width - 1; x >= 0; --x)
                        {
                            float alpha = blending(x, y);

                            int rc = (int)((float)target[x, y, Image.R] * alpha);
                            int gc = (int)((float)target[x, y, Image.G] * alpha);
                            int bc = (int)((float)target[x, y, Image.B] * alpha);
                            int a = (int)((float)255 * alpha);

                            img2.SetPixel(x, y, System.Drawing.Color.FromArgb(a, rc, gc, bc));
                        }
                }

                try
                {
                    using (System.IO.Stream stream = new System.IO.FileStream(outputFile, System.IO.FileMode.Create))
                    {
                        string ext = System.IO.Path.GetExtension(outputFile).ToLowerInvariant();
                        System.Drawing.Imaging.ImageFormat format;

                        switch (ext)
                        {
                            case ".bmp":
                                format = System.Drawing.Imaging.ImageFormat.Bmp;
                                break;
                            case ".emf":
                                format = System.Drawing.Imaging.ImageFormat.Emf;
                                break;
                            case ".gif":
                                format = System.Drawing.Imaging.ImageFormat.Gif;
                                break;
                            case ".jpg":
                            case ".jpeg":
                                format = System.Drawing.Imaging.ImageFormat.Jpeg;
                                break;
                            case ".tiff":
                            case ".tif":
                                format = System.Drawing.Imaging.ImageFormat.Tiff;
                                break;
                            case ".wmf":
                                format = System.Drawing.Imaging.ImageFormat.Wmf;
                                break;
                            case ".png":
                            default:
                                format = System.Drawing.Imaging.ImageFormat.Png;
                                break;
                        }

                        img2.Save(stream, format);
                        //img2.Save(outputFile);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            if (diagnostics) T40 = System.DateTime.Now;

            //jeśli ma wyświetlić pomierzony czas
            //if have to show the measured time 
            if (diagnostics)
            {
                System.TimeSpan t_loading = T10 - T00;
                System.TimeSpan t_saving = T40 - T30;
                System.TimeSpan t_interpolation = T30 - T10;
                System.TimeSpan t_setup = T20 - T10;
                System.TimeSpan t_resizing = T30 - T20;
                System.TimeSpan t_total = T40 - T10;

                Console.WriteLine("DIAGNOSTICS:");
                Console.WriteLine("\tfor: {0}", String.Join("; ", special));
                Console.WriteLine();
                Console.WriteLine("\t    Started work | Loading input: [T0] {0:yyyy:MM:dd hh:mm:ss.fff}", T00);
                Console.WriteLine("\t    Input loaded | Setting up:    [T1] {0:yyyy:MM:dd hh:mm:ss.fff}", T10);
                Console.WriteLine("\tAlgorithm set up | Resizing:      [T2] {0:yyyy:MM:dd hh:mm:ss.fff}", T20);
                Console.WriteLine("\t         Resized | Saving output: [T3] {0:yyyy:MM:dd hh:mm:ss.fff}", T30);
                Console.WriteLine("\t    Output saved | Finished work: [T4] {0:yyyy:MM:dd hh:mm:ss.fff}", T40);
                Console.WriteLine();
                Console.WriteLine(" * Total time: {0} ms (ap. {1} s).", t_total.TotalMilliseconds.ToString(_CI), t_total.TotalSeconds.ToString("0.###",_CI));
                Console.WriteLine(" * Loading the input file took {0} ms (ap. {1} s).", t_loading.TotalMilliseconds.ToString(_CI), t_loading.TotalSeconds.ToString("0.###", _CI));
                Console.WriteLine(" * Saving the output file took {0} ms (ap. {1} s).", t_saving.TotalMilliseconds.ToString(_CI), t_saving.TotalSeconds.ToString("0.###", _CI));
                Console.WriteLine(" * Whole interpolation algorithm took {0} ms (ap. {1} s).", t_interpolation.TotalMilliseconds.ToString(_CI), t_interpolation.TotalSeconds.ToString("0.###", _CI));
                Console.WriteLine(" * Setting out the interpolation algorihtm took {0} ms (ap. {1} s).", t_setup.TotalMilliseconds.ToString(_CI), t_setup.TotalSeconds.ToString("0.###", _CI));
                Console.WriteLine(" * Resizing the image took {0} ms (ap. {1} s).", t_resizing.TotalMilliseconds.ToString(_CI), t_resizing.TotalSeconds.ToString("0.###", _CI));
                Console.WriteLine();
            }

            //jeśli ma spauzować na końcu
            //if there is pasue on the end
            if (wait == "true")
            {
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey(true);
            }
        }
    }
}