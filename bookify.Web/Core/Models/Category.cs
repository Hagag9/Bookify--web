
namespace bookify.Web.Core.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class Category : BaseModel
    {
        public int Id { get; set; }
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        public ICollection<BookCategory> Bokks { get; set; } = new List<BookCategory>();
    }
}
