using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace Talej_Image_Processor_CMSC_630
{

    public class ImageProcessing
    {
        public static Bitmap GenerateHistogram(Bitmap grayScaleImage)
        {
            Bitmap bmp = grayScaleImage;
            var histogram = new int[256];

            //24bit image = 1 pixel value per 3 bytes


            //Image handling based on https://docs.microsoft.com/en-us/dotnet/api/system.drawing.imaging.bitmapdata?view=dotnet-plat-ext-6.0
            //Feed in the bitmap image

            //Get width and height from bitmap. In the case of the cancer cells, the images are uniform (768 px width, 568 px height)
            int width = grayScaleImage.Width;
            int height = grayScaleImage.Height;

            //Lock dimensions of image into memory
            BitmapData BMPData = grayScaleImage.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);

            //Calculate size of the image 
            //int bytes = image_data.Stride * image_data.Height;
            int bytes = Math.Abs(BMPData.Stride) * BMPData.Height;

            //Create new byte arrays of the same size
            byte[] grayImageBuffer = new byte[bytes];

            //Copy the original image into buffer as suggested by Dr. Krawczyk...bad things happen if we modify the original image as we read from it. 
            Marshal.Copy(BMPData.Scan0, grayImageBuffer, 0, bytes);

            //Release original image from memory
            grayScaleImage.UnlockBits(BMPData);


            byte intensity = 0;
            float maxIntensity = 0;
            //Grayscale grade series clear zero
            //Calculate the number of pixels of each grayscale
            //Theoretically it should be incremented by one, but because for whatever reason it keeps the image as 24bpp, we have to increment by 3

            for (int i = 0; i < bytes; i += 3)
            {
                //store the intensity value of the byte
                intensity = grayImageBuffer[i];
                
                //Add the intensity to the histogram
                histogram[intensity]++;
                if (histogram[intensity] > maxIntensity)
                {
                    //Set the new highest intensity value 
                    maxIntensity = histogram[intensity];
                }
            }

            int histHeight = 128;
            Bitmap img = new Bitmap(256, histHeight + 10);
            using (Graphics g = Graphics.FromImage(img))
            {
                //loop through all intensity values in the histogram array
                for (int i = 0; i < histogram.Length; i++)
                {
                    float pct = histogram[i] / maxIntensity;   //get ratio of given histogram intensity element compared to the maximum recorded intesnity
                    g.DrawLine(Pens.Black,
                        new Point(i, img.Height - 5),
                        new Point(i, img.Height - 5 - (int)(pct * histHeight))  
                        );
                }
            }
            return img;
        }
        public static Bitmap ImpulseNoise(Bitmap grayScaleImage, int noiseIntensity)
        {
            bool corruptionEnable = false;

            if (noiseIntensity > 0)
            {
            //24bit image = 1 pixel value per 3 bytes

            
            //Image handling based on https://docs.microsoft.com/en-us/dotnet/api/system.drawing.imaging.bitmapdata?view=dotnet-plat-ext-6.0
            //Feed in the bitmap image

            //Get width and height from bitmap. In the case of the cancer cells, the images are uniform (768 px width, 568 px height)
            int width = grayScaleImage.Width;
            int height = grayScaleImage.Height;

            //Lock dimensions of image into memory
            BitmapData BMPData = grayScaleImage.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);

            //Calculate size of the image 
            //int bytes = image_data.Stride * image_data.Height;
            int bytes = Math.Abs(BMPData.Stride) * BMPData.Height;

            //Create new byte arrays of the same size
            byte[] ogImageBuffer = new byte[bytes];

            //Byte array to make modifications to
            byte[] modifiedImageBuffer = new byte[bytes];

            //Copy the original image into buffer as suggested by Dr. Krawczyk...bad things happen if we modify the original image as we read from it. 
            Marshal.Copy(BMPData.Scan0, ogImageBuffer, 0, bytes);

            //Release original image from memory
            grayScaleImage.UnlockBits(BMPData);


                //Impulse Noise aka Salt and Pepper Algorithm
                Random rnd = new Random();


                //loop through the byte array. since this is a 24 bit image, 3 bytes = 1 pixel, so increment step is 3 to loop through the pixels
                for (int i = 0; i < bytes; i += 3)
                {

                    //I had to move the chance of execution random out of this method entirely because of a weird bug where if I compared to the input noise, it would generate within the true range ALWAYS. 
                    corruptionEnable = ChanceExecution(noiseIntensity);
                    int BlackorWhite = rnd.Next(0, 2);

                    //Loop through each byte ("color channel") in the pixel. It is still RGB format for whatever reason even though all 3 bytes in the pixel are set to the same color in the previous processing step
                    for (int j = 0; j < 3; j++)
                    {

                        if (corruptionEnable == true && BlackorWhite ==0)
                        {
                            //Corrupt the byte with black
                            modifiedImageBuffer[i + j] = 0;
                        }
                        if (corruptionEnable == true && BlackorWhite == 1)
                        {
                            //Corrupt the byte with white
                            modifiedImageBuffer[i + j] = 255;
                        }
                        if (corruptionEnable == false)
                        {
                            //Leave color byte uncorrupted
                            modifiedImageBuffer[i + j] = ogImageBuffer[i + j];
                        }
                    }
                }

            Bitmap corruptedImage = new Bitmap(width, height);
            BitmapData corruptedImageData = corruptedImage.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb);
            Marshal.Copy(modifiedImageBuffer, 0, corruptedImageData.Scan0, bytes);
            corruptedImage.UnlockBits(corruptedImageData);
                return corruptedImage;
            }
            else
            {
                //Do nothing. Slider was set to 0 so dont add impulse noise 
                return grayScaleImage;
            }
        }

        //Deprecated due to poor performance. Getpixel/setpixel sucks

        //public static Bitmap RGB2GrayscaleImage(Bitmap imageBMP)
        //{
        //    //Iterate through vertical pixels
        //    for (int i = 0; i < imageBMP.Width; i++)
        //        //iterate through horizontal pixels
        //        for (int j = 0; j < imageBMP.Height; j++)
        //        {
        //            //Get the color of the pixel at coordinate
        //            Color RGB = imageBMP.GetPixel(i, j);

        //            //Get Red channel intensity
        //            int red = RGB.R;
        //            //Get green channel intensity
        //            int green = RGB.G;

        //            //Get blue channel intensity
        //            int blue = RGB.B;

        //            //Multiply each channel intensity by coefficient to turn it gray
        //            int gray = (byte)(.299 * red + 0.587 * green + 0.114 * blue);

        //            // set RGB channels to gray values
        //            red = gray;
        //            green = gray;
        //            blue = gray;

        //            //Apply color channel intensity changes to the pixel
        //            imageBMP.SetPixel(i, j, Color.FromArgb(red, green, blue));

        //        }
        //    return imageBMP;
        //}

        public static Bitmap RGB2GrayscaleImage(Bitmap imageBMP)
        {
            //Much faster method

            //Accepted weighted averages for converting RGB to grayscale
            //Disabled for now
            //double redcoeff = 0.299;
            //double greencoeff = 0.587;
            //double bluecoeff = 0.114;

            int width = imageBMP.Width; 
            int height = imageBMP.Height;

            //source images are 24 bit RGB images, so we have 3 bytes per pixel
            var bmpData = imageBMP.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);

            var pixelSize = bmpData.Stride * bmpData.Height;


            byte[] originalimagebuffer = new byte[pixelSize];
            byte[] modifiedimagebuffer = new byte[pixelSize];

            Marshal.Copy(bmpData.Scan0, originalimagebuffer, 0, originalimagebuffer.Length);

            //Loop through the byte array. step size is 3 due to 3 bytes per pixel (only valid for 24 bit images)
            for (var i = 0; i < pixelSize; i += 3)
            {

                //Determine gray value of the "new" byte. there are two accepted ways of doing this. For "human vision" we apply weighted coefficients to each byte of a pixel. For "computer vision" we add RGB and divide by 3. 
                // 24 bits per pixel divided by 8 = 3 bytes per pixel. We are going to 8 bit image , so the value of 3 bytes will be the same. 
                
                //Coefficient method
                //var grayval = (byte)((0.114 * modifiedimagebuffer[i] + 0.587 * modifiedimagebuffer[i + 1] + 0.299 * modifiedimagebuffer[i + 2]));
                
                //Add up the 3 bytes then divide by 3, in essence taking an average of the 3 bytes and applying it across all 3 bytes. 
                var grayval = (byte)((originalimagebuffer[i] + originalimagebuffer[i + 1] + originalimagebuffer[i +2])/3);

                modifiedimagebuffer[i] = grayval;
                modifiedimagebuffer[i + 1] = grayval;
                modifiedimagebuffer[i + 2] = grayval;
            }

            Marshal.Copy(modifiedimagebuffer, 0, bmpData.Scan0, modifiedimagebuffer.Length);
            imageBMP.UnlockBits(bmpData);


            Bitmap grayscaleImage = new Bitmap(width, height);
            BitmapData grayscaleImageData = grayscaleImage.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb);        //I have no idea why I cannot set this to 8bit image format....... 
            Marshal.Copy(modifiedimagebuffer, 0, grayscaleImageData.Scan0, modifiedimagebuffer.Length);
            grayscaleImage.UnlockBits(grayscaleImageData);
            return grayscaleImage;
        }

        //Recycled code from stackoverflow just to get the preview window working
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        public static bool ChanceExecution(int noiseintensity)
        {
            bool execute = false;

            Random random = new Random();

            int randomint = random.Next(0, 501);
            if (randomint <= noiseintensity)
            {
                execute = true;
            }
            return execute;
        }
    }
}
