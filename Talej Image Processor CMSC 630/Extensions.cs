using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talej_Image_Processor_CMSC_630
{
    public static class Extensions
    {
        public static bool IsImage(this FileInfo file)
        {
            var allowedExtensions = new[] { ".jpg", ".png", ".gif", ".jpeg", ".bmp" };
            return allowedExtensions.Contains(file.Extension.ToLower());
        }
    }
}
