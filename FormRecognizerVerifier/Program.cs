using ImageMagick;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace FormRecognizerVerifier
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args == null || args.Length < 2)
            {
                ConsoleColor previousColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Syntax:");
                Console.WriteLine("    FormRecognizerVerifier scannedFormImage.jpg formRecognizerOutput.json");
                Console.WriteLine("    - Scanned form images can be any supported image format (PNG, GIF, JPEG), but not PDF");
                Console.ForegroundColor = previousColor;
                return;
            }

            DrawBoxes(args[0], args[1]);
        }

        /// <summary>
        /// Parse Json document and draw bounding boxes with differentn colours, also printing out the detected labels
        /// </summary>
        /// <param name="imageFilename">Name of image file (GIF, JPEG, PNG)</param>
        /// <param name="formRecognizerOutputJsonFile">Name of file with output of Form Recognizer service</param>
        private static void DrawBoxes(string imageFilename, string formRecognizerOutputJsonFile)
        {
            IMagickImage outputImage = new MagickImage(imageFilename).Clone();
            int imageHeight = outputImage.Height;
            outputImage.AutoOrient();

            string json = File.ReadAllText(formRecognizerOutputJsonFile);

            JObject resource = JObject.Parse(json);

            foreach(var outputRecord in resource["pages"][0]["keyValuePairs"].Children()) // top-level keys/values -- 3 in my sample JSON file
            {
                // for each of these there's a key 
                string text = outputRecord["key"][0]["text"].ToString();
                double[] bb = new double[8];
                string valueColour = "purple";

                if (!text.Contains("_Tokens_")) // skip writting out the box for this, as it's just a marker
                {
                    for (int j = 0; j < 8; j++)
                    {
                        bb[j] = Double.Parse(outputRecord["key"][0]["boundingBox"][j].ToString());
                    }

                    WriteLabelAndBoundingBox(outputImage, imageHeight, text, bb, "blue");
                    Console.WriteLine(text);

                    valueColour = "blue";
                }
                else
                {
                    valueColour = "purple";
                }

                // and 1+ values for the above key
                foreach(var valuesRecord in outputRecord["value"])
                {
                    text = valuesRecord["text"].ToString();

                    if (!text.Contains("thisisawatermark")) // not sure why this is in the output, but if it shows up just skip it
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            bb[j] = Double.Parse(valuesRecord["boundingBox"][j].ToString());
                        }

                        WriteLabelAndBoundingBox(outputImage, imageHeight, text, bb, valueColour);
                        Console.WriteLine(text);
                    }
                }
            }

            string outputFilename = "out-" + Path.GetFileName(imageFilename);
            outputImage.Write(outputFilename);
            Console.Write("\nWrote out to {0} . \nPress enter to exit...", outputFilename);
            Console.ReadLine();
        }

        /// <summary>
        /// Write the label and bounding box to the output image, using a specified drawing colour
        /// </summary>
        /// <param name="outputImage">Image Magick image/param>
        /// <param name="imageHeight">Height of output image (needed because of carterian coordinate use)</param>
        /// <param name="text">Text to write above the bounding box</param>
        /// <param name="bb">Four points of the bounding box with cartesian coordinates (ie, (0,0) is bottom left)</param>
        /// <param name="drawColor">Image Magick colour name</param>
        private static void WriteLabelAndBoundingBox(IMagickImage outputImage, int imageHeight, string text, double[] bb, string drawColor)
        {
            DrawableStrokeColor strokeColor = new DrawableStrokeColor(new MagickColor(drawColor));
            DrawableStrokeWidth strokeWidth = new DrawableStrokeWidth(1);
            DrawableFillColor fillColor = new DrawableFillColor(new MagickColor(50, 50, 50, 128));
            DrawableRectangle dr = new DrawableRectangle(bb[0], imageHeight - bb[1], bb[2], imageHeight - bb[5]); // {[  45.0,  469.0,  85.0,  469.0,  85.0,  457.0,  45.0,  457.0]}
            DrawableText dt = new DrawableText(bb[0], imageHeight - bb[1], text);
            outputImage.Draw(strokeColor, strokeWidth, fillColor, dr, dt);
        }
    }
}
