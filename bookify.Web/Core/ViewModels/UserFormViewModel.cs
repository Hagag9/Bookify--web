using CloudinaryDotNet.Actions;
using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace bookify.Web.Core.ViewModels
{
	public class UserFormViewModel
	{
		public string? Id { get; set; }

		[MaxLength(100, ErrorMessage = Errors.MaxLength),Display(Name ="Full Name"),
			RegularExpression(RegexPatterns.CharactersOnly_Eng,ErrorMessage =Errors.OnlyEnglishLetters)]
		public string FullName { get; set; } = null!;


		[MaxLength(20,ErrorMessage=Errors.MaxLength),
			RegularExpression(RegexPatterns.Username,ErrorMessage =Errors.InvalidUserName)]
		[Remote("AllowUserName",null!, AdditionalFields ="Id",ErrorMessage =Errors.Duplicated)]
		public string UserName { get; set; } = null!;

		
		[MaxLength(200, ErrorMessage = Errors.MaxLength), EmailAddress]
		[Remote("AllowEmail",null!, AdditionalFields = "Id", ErrorMessage = Errors.Duplicated)]
		public string Email { get; set; } = null!;

		[StringLength(100, ErrorMessage =Errors.MaxMinLength, MinimumLength = 8), DataType(DataType.Password),
			RegularExpression(RegexPatterns.Password,ErrorMessage =Errors.WeakPassword)]
		[RequiredIf("Id == null",ErrorMessage =Errors.RequierdField)]
		public string? Password { get; set; } 

		[Compare("Password", ErrorMessage =Errors.ConfirmPasswordNotmatch), DataType(DataType.Password), Display(Name = "Confirm password")]
		[RequiredIf("Id == null", ErrorMessage = Errors.RequierdField)]
		public string? ConfirmPassword { get; set; } 

		[Display(Name = "Roles")]
		public IList<string> SelectedRoles { get; set; } = new List<string>();
		public IEnumerable<SelectListItem>? Roles { get; set; } 
	}

}

