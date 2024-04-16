namespace bookify.Web.Services
{
    public class EmailBodyBuilder : IEmailBodyBuilder
    {
        private readonly IWebHostEnvironment _webHostEnviroment;

        public EmailBodyBuilder(IWebHostEnvironment webHostEnviroment)
        {
            _webHostEnviroment = webHostEnviroment;
        }

        public string GetEmailBody(string template , Dictionary<string,string> placeholders)
        {
            var filePath = $"{_webHostEnviroment.WebRootPath}/templates/{template}.html";
            StreamReader str = new(filePath);
            var templateContent = str.ReadToEnd();
            str.Close();

            foreach (var placeholder in placeholders)
                templateContent= templateContent.Replace($"[{placeholder.Key}]",placeholder.Value);

            return templateContent;
        }
    }
}
