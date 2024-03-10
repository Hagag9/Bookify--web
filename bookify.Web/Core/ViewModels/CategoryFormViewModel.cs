
namespace bookify.Web.Core.ViewModels
{
    public class CategoryFormViewModel
    {
        public int Id { get; set; }
        [MaxLength(100, ErrorMessage = Errors.MaxLength),Display(Name ="Category"),
			RegularExpression(RegexPatterns.CharactersOnly_Eng, ErrorMessage = Errors.OnlyEnglishLetters)]
        [Remote("UniqueName", null!,AdditionalFields="Id", ErrorMessage = Errors.Duplicated)]
        public string Name { get; set; } = null!;
    }
}
