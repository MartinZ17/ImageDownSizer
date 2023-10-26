using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageDownSizer1
{
    public class ImageResizer
    {
        public static byte[,,] ResizeImage(byte[,,] originalImage, int originalWidth, int originalHeight, int newWidth, int newHeight)
        {
            byte[,,] resizedImage = new byte[newWidth, newHeight, 3];

            for (int x = 0; x < newWidth; x++)
            {
                for (int y = 0; y < newHeight; y++)
                {
                    float sourceX = x * (originalWidth / (float)newWidth);
                    float sourceY = y * (originalHeight / (float)newHeight);

                    for (int channel = 0; channel < 3; channel++)
                    {
                        resizedImage[x, y, channel] = BilinearInterpolation(originalImage, sourceX, sourceY, channel, originalWidth, originalHeight);
                    }
                }
            }

            return resizedImage;
        }
        private static byte BilinearInterpolation(byte[,,] image, float x, float y, int channel, int width, int height)
        {
            int x1 = (int)x;
            int y1 = (int)y;
            int x2 = Math.Min(x1 + 1, width - 1);
            int y2 = Math.Min(y1 + 1, height - 1);

            float dx = x - x1;
            float dy = y - y1;

            byte value1 = image[x1, y1, channel];
            byte value2 = image[x2, y1, channel];
            byte value3 = image[x1, y2, channel];
            byte value4 = image[x2, y2, channel];

            byte interpolatedValue = (byte)((1 - dx) * (1 - dy) * value1 + dx * (1 - dy) * value2 + (1 - dx) * dy * value3 + dx * dy * value4);

            return interpolatedValue;
        }
    }
}
