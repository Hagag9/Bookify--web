using Microsoft.AspNetCore.Mvc;

namespace bookify.Web.Core.ViewModels
{
    public class CategoryFormViewModel
    {
        public int Id { get; set; }
        [MaxLength(100,ErrorMessage ="Max 100 Chr.")]
        [Remote("UniqueName","Categories",ErrorMessage ="Ctegory with The same name is Already exist")]
        public string Name { get; set; } = null!;
    }
}
