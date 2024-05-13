namespace bookify.Web.Services
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private List<string> _allowedExtensions = new() { ".jpg", ".jpeg", ".png" };
        private int _maxAllowedSize = 2097152;  // 2MG

        public ImageService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }


        public async Task<(bool isUploaded, string? errorMessage)> UploadAsync(IFormFile image, string imageName, string folderPath, bool hasThumbnail)
        {
            var extension = Path.GetExtension(image.FileName);

            if (!_allowedExtensions.Contains(extension))
                return (isUploaded: false, errorMessage: Errors.NotAllowedExtension);

            if (image.Length > _maxAllowedSize)
                return (isUploaded: false, errorMessage: Errors.MaxSize);

            var path = Path.Combine($"{_webHostEnvironment.WebRootPath}{folderPath}", imageName);

            using var stream = System.IO.File.Create(path);
            await image.CopyToAsync(stream);

            if (hasThumbnail)
            {
                var thumbPath = Path.Combine($"{_webHostEnvironment.WebRootPath}{folderPath}/thumb", imageName);

                using var loadedImage = Image.Load(image.OpenReadStream());
                var ratio = (float)loadedImage.Width / 200;
                var height = loadedImage.Height / ratio;
                loadedImage.Mutate(i => i.Resize(width: 200, height: (int)height));
                loadedImage.Save(thumbPath);
            }

            return (true, null);
        }
        public void Delete(string imagePath, string? thumbnailPath)
        {
            var oldImagePath = $"{_webHostEnvironment.WebRootPath}{imagePath}";

            if (File.Exists(oldImagePath))
                File.Delete(oldImagePath);

            if (!string.IsNullOrEmpty(thumbnailPath))
            {
                var oldThumbPath = $"{_webHostEnvironment.WebRootPath}{thumbnailPath}";
                if (File.Exists(oldThumbPath))
                    File.Delete(oldImagePath);
            }
        }

    }
}
