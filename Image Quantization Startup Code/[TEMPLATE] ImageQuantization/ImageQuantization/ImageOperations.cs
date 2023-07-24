using System;
using System.Collections.Generic;

using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Linq;

///Algorithms Project
///Intelligent Scissors
///

namespace ImageQuantization
{
    /// <summary>
    /// Holds the pixel color in 3 byte values: red, green and blue
    /// </summary>
    public struct RGBPixel
    {
        public byte red, green, blue;
    }
    public struct RGBPixelD
    {
        public double red, green, blue;
    }


    /// <summary>
    /// Library of static functions that deal with images
    /// </summary>
    public static class ImageOperations
    {
        /// <summary>
        /// Open an image and load it into 2D array of colors (size: Height x Width)
        /// </summary>
        /// <param name="ImagePath">Image file path</param>
        /// <returns>2D array of colors</returns>
        public static RGBPixel[,] OpenImage(string ImagePath)
        {
            Bitmap original_bm = new Bitmap(ImagePath);
            int Height = original_bm.Height;
            int Width = original_bm.Width;

            RGBPixel[,] Buffer = new RGBPixel[Height, Width];

            unsafe
            {
                BitmapData bmd = original_bm.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, original_bm.PixelFormat);
                int x, y;
                int nWidth = 0;
                bool Format32 = false;
                bool Format24 = false;
                bool Format8 = false;

                if (original_bm.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    Format24 = true;
                    nWidth = Width * 3;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format32bppArgb || original_bm.PixelFormat == PixelFormat.Format32bppRgb || original_bm.PixelFormat == PixelFormat.Format32bppPArgb)
                {
                    Format32 = true;
                    nWidth = Width * 4;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    Format8 = true;
                    nWidth = Width;
                }
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (y = 0; y < Height; y++)
                {
                    for (x = 0; x < Width; x++)
                    {
                        if (Format8)
                        {
                            Buffer[y, x].red = Buffer[y, x].green = Buffer[y, x].blue = p[0];
                            p++;
                        }
                        else
                        {
                            Buffer[y, x].red = p[2];
                            Buffer[y, x].green = p[1];
                            Buffer[y, x].blue = p[0];
                            if (Format24) p += 3;
                            else if (Format32) p += 4;
                        }
                    }
                    p += nOffset;
                }
                original_bm.UnlockBits(bmd);
            }

            return Buffer;
        }

        /// <summary>
        /// Get the height of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Height</returns>
        public static int GetHeight(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(0);
        }

        /// <summary>
        /// Get the width of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Width</returns>
        public static int GetWidth(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(1);
        }

        /// <summary>
        /// Display the given image on the given PictureBox object
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <param name="PicBox">PictureBox object to display the image on it</param>
        public static void DisplayImage(RGBPixel[,] ImageMatrix, PictureBox PicBox)
        {
            // Create Image:
            //==============
            int Height = ImageMatrix.GetLength(0);
            int Width = ImageMatrix.GetLength(1);

            Bitmap ImageBMP = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);

            unsafe
            {
                BitmapData bmd = ImageBMP.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, ImageBMP.PixelFormat);
                int nWidth = 0;
                nWidth = Width * 3;
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        p[2] = ImageMatrix[i, j].red;
                        p[1] = ImageMatrix[i, j].green;
                        p[0] = ImageMatrix[i, j].blue;
                        p += 3;
                    }

                    p += nOffset;
                }
                ImageBMP.UnlockBits(bmd);
            }
            PicBox.Image = ImageBMP;
        }


        /// <summary>
        /// Apply Gaussian smoothing filter to enhance the edge detection 
        /// </summary>
        /// <param name="ImageMatrix">Colored image matrix</param>
        /// <param name="filterSize">Gaussian mask size</param>
        /// <param name="sigma">Gaussian sigma</param>
        /// <returns>smoothed color image</returns>
        public static RGBPixel[,] GaussianFilter1D(RGBPixel[,] ImageMatrix, int filterSize, double sigma)
        {
            int Height = GetHeight(ImageMatrix);
            int Width = GetWidth(ImageMatrix);

            RGBPixelD[,] VerFiltered = new RGBPixelD[Height, Width];
            RGBPixel[,] Filtered = new RGBPixel[Height, Width];


            // Create Filter in Spatial Domain:
            //=================================
            //make the filter ODD size
            if (filterSize % 2 == 0) filterSize++;

            double[] Filter = new double[filterSize];

            //Compute Filter in Spatial Domain :
            //==================================
            double Sum1 = 0;
            int HalfSize = filterSize / 2;
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                //Filter[y+HalfSize] = (1.0 / (Math.Sqrt(2 * 22.0/7.0) * Segma)) * Math.Exp(-(double)(y*y) / (double)(2 * Segma * Segma)) ;
                Filter[y + HalfSize] = Math.Exp(-(double)(y * y) / (double)(2 * sigma * sigma));
                Sum1 += Filter[y + HalfSize];
            }
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                Filter[y + HalfSize] /= Sum1;
            }

            //Filter Original Image Vertically:
            //=================================
            int ii, jj;
            RGBPixelD Sum;
            RGBPixel Item1;
            RGBPixelD Item2;

            for (int j = 0; j < Width; j++)
                for (int i = 0; i < Height; i++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int y = -HalfSize; y <= HalfSize; y++)
                    {
                        ii = i + y;
                        if (ii >= 0 && ii < Height)
                        {
                            Item1 = ImageMatrix[ii, j];
                            Sum.red += Filter[y + HalfSize] * Item1.red;
                            Sum.green += Filter[y + HalfSize] * Item1.green;
                            Sum.blue += Filter[y + HalfSize] * Item1.blue;
                        }
                    }
                    VerFiltered[i, j] = Sum;
                }

            //Filter Resulting Image Horizontally:
            //===================================
            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int x = -HalfSize; x <= HalfSize; x++)
                    {
                        jj = j + x;
                        if (jj >= 0 && jj < Width)
                        {
                            Item2 = VerFiltered[i, jj];
                            Sum.red += Filter[x + HalfSize] * Item2.red;
                            Sum.green += Filter[x + HalfSize] * Item2.green;
                            Sum.blue += Filter[x + HalfSize] * Item2.blue;
                        }
                    }
                    Filtered[i, j].red = (byte)Sum.red;
                    Filtered[i, j].green = (byte)Sum.green;
                    Filtered[i, j].blue = (byte)Sum.blue;
                }

            return Filtered;
        }
       
        public static List<HashSet<Color>> clusters;
        public static Dictionary<Color, bool> visited;
        public static Dictionary<Color, List<Color>> neighbours;
        public static Dictionary<Color, Color> pallet = new Dictionary<Color, Color>();
        public static void ImageQuantization(ref RGBPixel[,] ImageMatrix)
        {
            int width = GetWidth(ImageMatrix);
            int height = GetHeight(ImageMatrix);
            //int red=0,green=0, blue=0;
            Color color, color_new;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    //red=ImageMatrix[i,j].red;
                    //green=ImageMatrix[i,j].green;
                    //blue=ImageMatrix[i,j].blue;
                    color = RGBpixelToColor(ImageMatrix[i, j]);
                    //color=Color.FromArgb(red,green,blue);
                    color_new = pallet[color];
                    ImageMatrix[i, j] = ColorToRGBpixel(color_new);

                }
            }
        }

        public static void PalletGereration()
        {
            pallet = new Dictionary<Color, Color>();
            int red = 0, green = 0, blue = 0;
            Color represntative = new Color();
            foreach (var cluster in clusters)
            {
                red = 0; green = 0; blue = 0;
                represntative = new Color();
                foreach (Color c in cluster)
                {
                    red += c.R;
                    green += c.G;
                    blue += c.B;
                }
                red = red / cluster.Count;
                green = green / cluster.Count;
                blue = blue / cluster.Count;
                represntative = Color.FromArgb(red, green, blue);
                foreach (Color color in cluster)
                {
                    pallet.Add(color, represntative);
                }
            }
        }
        public static void Cluster(int k)
        {
            GetNeighbours(k);
           /* visited = new Dictionary<Color, bool>();
            foreach (var vetiex in vertcies)
            {
                visited.Add(vetiex, false);
            }*/
            clusters = new List<HashSet<Color>>();
            foreach (var vertex in neighbours)
            {
                if (!visited[vertex.Key])
                {
                    HashSet<Color> cluster = new HashSet<Color>();
                    BFS(vertex.Key, ref cluster);
                    clusters.Add(cluster);

                }
            }
        }
        public static void BFS(Color color, ref HashSet<Color> cluster)
        {
            Queue<Color> x=new Queue<Color>();
            x.Enqueue(color);
           

           
            
            while (x.Count()!=0)
            {
                Color vertix = x.Dequeue();
                cluster.Add(vertix);
                visited[vertix] = true;
                foreach (var neighbour in neighbours[vertix])
                {

                    if (!visited[neighbour])
                        x.Enqueue(neighbour);
                }
            }
        }

        private static Random rng = new Random();

        public static void Shuffle<T>(this IList<T> list)
        {
            
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        public static void GetNeighbours(int k)
        {

            MST_list = MST_list.OrderBy(o => o.wight).ToList();

            for (int i = MST_list.Count - (k - 1); i < MST_list.Count; i++)
            {
                Edge e;
                e = MST_list[i];

                e.wight = 0;
                MST_list[i] = e;
            }
            Shuffle(MST_list);
           
            neighbours = new Dictionary<Color, List<Color>>();
            foreach (var mst_edge in MST_list)
            {
                if (mst_edge.wight != 0)
                {
                    if (neighbours.ContainsKey(mst_edge.source))
                    {
                        neighbours[mst_edge.source].Add(mst_edge.destination);
                    }
                    else
                    {
                        List<Color> col = new List<Color>();
                        col.Add(mst_edge.destination);
                        neighbours.Add(mst_edge.source, col);
                    }
                    if (neighbours.ContainsKey(mst_edge.destination))
                    {
                        neighbours[mst_edge.destination].Add(mst_edge.source);
                    }
                    else
                    {
                        List<Color> col = new List<Color>();
                        col.Add(mst_edge.source);
                        neighbours.Add(mst_edge.destination, col);
                    }
                }
                else
                {
                    if (!neighbours.ContainsKey(mst_edge.source))
                    {
                        List<Color> col = new List<Color>();
                        neighbours.Add(mst_edge.source, col);
                    }
                    if (!neighbours.ContainsKey(mst_edge.destination))
                    {
                        List<Color> col = new List<Color>();
                        neighbours.Add(mst_edge.destination, col);
                    }

                }
            }

        }

        public static List<Color> vertcies;
        public static List<Edge> MST_list;
        public static Dictionary<Color, int> color;
        public static void MST()
        {

            color = new Dictionary<Color, int>();
            MST_list = new List<Edge>();
            // Console.WriteLine("time at start : " + DateTime.Now.Minute.ToString() + " :" + DateTime.Now.Second.ToString());
            Dictionary<int, Color> vertcies_local = new Dictionary<int, Color>();
            List<int> iner_vertcies = new List<int>();
            for (int i = 0; i < vertcies.Count; i++)
            {
                vertcies_local.Add(i, vertcies[i]);
                iner_vertcies.Add(i);
                color.Add(vertcies[i], 0);
            }
            int[] parant = new int[vertcies.Count];
            double[] key = new double[vertcies.Count];


            foreach (var vertix in vertcies_local)
            {
                key[vertix.Key] = double.MaxValue;

            }

            key[0] = 0;
            parant[0] = -1;
            int k = 0;
            double min_val = double.MaxValue;
            int next_node = 0;

            foreach (var vertix in vertcies_local) //n
            {
                min_val = double.MaxValue;
                k = next_node;
                //for (int i = 0; i < vertcies_local.Count; i++)
                foreach (var i in iner_vertcies) //n
                {

                    double wight = GetEdgeWight(vertcies_local[k], vertcies_local[i]);

                    if (wight < min_val)
                    {
                        min_val = wight;
                        next_node = i;
                    }
                    if (key[i] < min_val)
                    {
                        min_val = key[i];
                        next_node = i;
                    }
                    if (wight < key[i])
                    {
                        parant[i] = k;
                        key[i] = wight;
                    }


                }

                iner_vertcies.Remove(next_node);
            }
            Edge edge = new Edge();
            Color c1 = new Color();
            Color c2 = new Color();
            double weight = 0;
            for (int i = 0; i < vertcies_local.Count; i++)
            {
                if (parant[i] != -1)
                {
                    c1 = vertcies_local[i];
                    c2 = vertcies_local[parant[i]];
                    weight = GetEdgeWight(c1, c2);
                    edge.source = c1;
                    edge.destination = c2;
                    edge.wight = weight;
                    MST_list.Add(edge);

                }

            }

            double sum = 0, summ = 0;
            // Console.WriteLine("double max value : " + double.MaxValue);
            foreach (var kk in key)
            {
                sum += kk;

            }
            foreach (var edg in MST_list)
            {
                summ += edg.wight;

            }
            MessageBox.Show(sum.ToString());
            //  Console.WriteLine("mst : " + sum);
            //  Console.WriteLine("edges count : " + MST_list.Count);
            // Console.WriteLine("edges count : " + summ);
            //  Console.WriteLine("time after finsh : " + DateTime.Now.Minute.ToString() + " :" + DateTime.Now.Second.ToString());
        }


        public struct Edge
        {
            public Color source, destination;
            public double wight;
        }


        private static double GetEdgeWight(Color color1, Color color2)
        {
            double r_part = ((double)(color2.R - color1.R) * (double)(color2.R - color1.R));
            double g_part = ((double)(color2.G - color1.G) * (double)(color2.G - color1.G));
            double b_part = ((double)(color2.B - color1.B) * (double)(color2.B - color1.B));
            double sum = r_part + g_part + b_part;
            return Math.Sqrt(sum);
        }

        public static HashSet<Color> DistinctColors(RGBPixel[,] ImageMatrix)
        {
            visited = new Dictionary<Color, bool>();
            vertcies = new List<Color>();
            HashSet<Color> vertcies_set = new HashSet<Color>();
            int width = GetWidth(ImageMatrix);
            int hight = GetHeight(ImageMatrix);
            Color color;
           // HashSet<Color> colors = new HashSet<Color>();
            for (int x = 0; x < hight; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    color = RGBpixelToColor(ImageMatrix[x, y]);

                    vertcies_set.Add(color);
                    if(!visited.ContainsKey(color))
                    {
                        visited.Add(color, false);
                    }
                }
            }
            vertcies = vertcies_set.ToList();
            return vertcies_set;

        }



        private static RGBPixel ColorToRGBpixel(Color color)
        {
            RGBPixel r_pixel = new RGBPixel();
            r_pixel.red = color.R;
            r_pixel.green = color.G;
            r_pixel.blue = color.B;
            return r_pixel;
        }
        private static Color RGBpixelToColor(RGBPixel rgb_pixel)
        {
            return Color.FromArgb(rgb_pixel.red, rgb_pixel.green, rgb_pixel.blue);
        }
    }
}
