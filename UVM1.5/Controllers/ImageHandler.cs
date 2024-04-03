using Microsoft.AspNetCore.Mvc;

namespace UVM1._5.Controllers
{
    public class ImageHandler
    {
        public static byte[] ConvertImageFile(IFormFile imageFile)
        {
            if (!IsSupportedImageType(imageFile.ContentType))
            {
                throw new NotSupportedException("Unsupported image type. Please upload a valid image file.");
            }
            byte[]? image = null;

            if (imageFile.Length > 0)
            {
                using var ms = new MemoryStream();
                imageFile.CopyTo(ms);
                image = ms.ToArray();
            }
            return image;
        }

        public static bool IsSupportedImageType(string contentType)
        {
            if (contentType.StartsWith("image/"))
            {
                return true;
            }
            else
            { return false; }
        }

    }
}
