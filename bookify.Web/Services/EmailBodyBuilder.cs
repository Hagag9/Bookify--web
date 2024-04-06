namespace bookify.Web.Services
{
    public class EmailBodyBuilder : IEmailBodyBuilder
    {
        private readonly IWebHostEnvironment _webHostEnviroment;

        public EmailBodyBuilder(IWebHostEnvironment webHostEnviroment)
        {
            _webHostEnviroment = webHostEnviroment;
        }

        public string GetEmailBody(string imageUrl, string header, string body, string url, string linkTitle)
        {
            var filePath = $"{_webHostEnviroment.WebRootPath}/templates/email.html";
            StreamReader str = new(filePath);
            var template = str.ReadToEnd();
            str.Close();
            return template
                .Replace("[imageUrl]", imageUrl)
                .Replace("[header]", header)
                .Replace("[body]", body)
                .Replace("[url]", url)
                .Replace("[linkTitle]", linkTitle);
        }
    }
}
