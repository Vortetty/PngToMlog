using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using System.Configuration;
using System.Collections.Generic;

namespace test
{

    class program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            // Prompt user for file
            FileStream fs;

            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "Portable Network Graphic|*.png";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;

            // Make sure the file is chosen, program exits if it isnt
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                if ((fs = (FileStream)openFileDialog.OpenFile()) != null)
                {
                    // init vars for this portion
                    fs.Close();
                    int[] sSize = new int[2];
                    int[] iSize = new int[2];
                    int ipp;
                    int optimize;

                    Console.WriteLine("vanilla small screen is 80x80");
                    Console.WriteLine("vanilla big screen is 176x176");
                    Console.WriteLine("Screen width:");
                    sSize[0] = int.Parse(Console.ReadLine());

                    Console.WriteLine("Screen height:");
                    sSize[1] = int.Parse(Console.ReadLine());

                    Console.WriteLine("Desired image width:");
                    iSize[0] = int.Parse(Console.ReadLine());

                    Console.WriteLine("Desired image height:");
                    iSize[1] = int.Parse(Console.ReadLine());

                    Console.WriteLine("Instructions per processor, 990 recommended as it ensures you dont run out of space before this finishes:");
                    ipp = int.Parse(Console.ReadLine());

                    Console.WriteLine("Image optimization amount, 0 is no optimization, 100 is 100% optimization");
                    optimize = 100-Math.Max(0, Math.Min(100, int.Parse(Console.ReadLine())));

                    // Create image
                    read(openFileDialog, sSize, iSize, ipp, optimize);
                }
            }
        }

        public static void read(OpenFileDialog dialogObj, int[] screenSize, int[] imageSize, int instructionsPerProcessor, int optimize)
        {
            //create image
            Image<Rgba32> image = Image.Load<Rgba32>(File.ReadAllBytes(dialogObj.FileName));
            string outText = "";
            //Clipboard.SetText(outText);
            //color 0 0 0 255
            //draw rect 0 0 1 1 0 0
            //drawflush display1

            //draw color r g b 255 0 0
            //draw rect x y 1 1 0 0
            //drawflush display1

            //set width and height vars
            int width = imageSize[0];
            int height = imageSize[1];


            //resize image and flip it vertically to compensate for differences in how lists and displays work
            image.Mutate(
                ctx => ctx.Resize(width, height, new NearestNeighborResampler())
                    .Flip(FlipMode.Vertical)
            );

            //save image, code it, optimize it, then load optimized version back into memory. only if the program is present ofc.
            try
            {
                FileStream buffer = new FileStream(@"buf.png", FileMode.OpenOrCreate);
                image.SaveAsPng(buffer);
                buffer.Close();
                Process process = Process.Start(@"pngquant\pngquant.exe", string.Format(@"--force --verbose --ordered --speed=1 --quality=0-{0} --ext -processed.png buf.png", optimize));
                process.WaitForExit();
                image = Image.Load(File.ReadAllBytes(@"buf-processed.png"));
            }
            catch { }

            //init vars
            int processorCounter = 0;
            int i = 0;
            int i1 = 0;

            List<String> colors = new List<String>();
            HashSet<Point> usedCoords = new HashSet<Point>();
            HashSet<Point> usedImagePoints = new HashSet<Point>();
            Dictionary<String, List<List<Point>>> imageCoords = new Dictionary<String, List<List<Point>>>();

            // create list of colors
            for (int _ = 0; _ < image.Width; _++)
            {
                for (int __ = 0; __ < image.Height; __++)
                {
                    colors.Add(image[_, __].ToString());
                }
            }

            //deduplicate colors
            colors = colors.Distinct().ToList();

            //construct dictionary, mapping colors to lists. each list contains lists defining top left and bottom right corners of the rectangle
            for (int _ = 0; _ < colors.Count; _++)
            {
                imageCoords.Add(colors[_],
                    new List<List<Point>>()
                );
            }

            
            // Scan for rectangles and add them to the final rectangle list
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (!usedImagePoints.Contains(new Point(x, y)))
                    {
                        string currentColor = image[x, y].ToString();
                        int firsty = y;
                        int newx = x;
                        List<int> xs = new List<int>();

                        while (newx + 1 < width && image[newx + 1, y].ToString() == currentColor)
                        {
                            newx++;
                        }

                        xs.Add(newx);
                        newx = x;

                        while (y + 1 < height && image[x, y + 1].ToString() == currentColor)
                        {
                            y++;

                            while (newx + 1 < width && image[newx + 1, y].ToString() == currentColor)
                            {
                                newx++;
                            }
                            xs.Add(newx);
                            newx = x;
                        }

                        imageCoords[currentColor].Add(new List<Point>() { new Point(x, firsty), new Point(xs.Min(), y) });

                        for (int x1 = x; x1 <= xs.Min(); x1++)
                        {
                            for (int y1 = firsty; y1 <= y; y1++)
                            {
                                usedImagePoints.Add(new Point(x1, y1));
                            }
                        }
                    }
                }
            }
            
            // iterate over each color
            foreach (KeyValuePair<String, List<List<Point>>> entryBuf in imageCoords)
            {
                //deduplicate just in case
                KeyValuePair<String, List<List<Point>>> entry = new KeyValuePair<String, List<List<Point>>>(entryBuf.Key, entryBuf.Value.Distinct().ToList());

                //initialize section of color
                string[] color = entry.Key.Replace("Rgba32(", "").Replace(")", "").Replace(" ", "").Split(',', ' ');
                outText += string.Format(@"draw color {0} {1} {2} {3} 0 0
", color[0], color[1], color[2], color[3]);
                i1++;
                i++;

                //iterate over rects
                foreach (List<Point> coord in entry.Value)
                {
                    //get xes for calculations
                    int x = coord[0].X;
                    int y = coord[0].Y;
                    int x1 = coord[1].X;
                    int y1 = coord[1].Y;


                    if (true /*!usedCoords.Contains(new Point(coord[0], coord[1]))*/)
                    {
                        //usedCoords.Add(new Point(coord[0], coord[1]));

                        // add the rect
                        outText += string.Format(@"draw rect {0} {1} {2} {3} 0 0
", x * Math.Max(1, screenSize[0] / width), y * Math.Max(1, screenSize[1] / height), (x1-x+1)*Math.Max(1, screenSize[0] / width), (y1-y+1)*Math.Max(1, screenSize[1] / height));

                        // increase counters
                        i1 += 1;
                        i += 1;

                        // if enough draw commands have been sent, add a draw buffer so we dont lose data, and set the color again
                        if (i >= 250)
                        {
                            i = 0;
                            outText += @"drawflush display1
";
                            outText += string.Format(@"draw color {0} {1} {2} {3} 0 0
", color[0], color[1], color[2], color[3]);
                            i1++;
                            i1++;
                        }

                        //if the instructions per processor is reached then set the clipboard and prepare for next processor
                        if (i1 > instructionsPerProcessor)
                        {
                            i1 = 0;
                            outText += string.Format(@"drawflush display1
");

                            processorCounter++;

                            Clipboard.SetText(outText);
                            Console.WriteLine(outText);
                            outText = "";
                            outText += string.Format(@"draw color {0} {1} {2} {3} 0 0
", color[0], color[1], color[2], color[3]);
                            Console.WriteLine(string.Format("\n\n\n\npress enter for next segment"));
                            Console.ReadKey();
                        }
                    }
                }
            }

            //when we reach the end, draw what we have and then exit
            outText += @"drawflush display1
";

            Clipboard.SetText(outText);
            Console.WriteLine(outText);
            Console.ReadKey();
        }
    }
}
