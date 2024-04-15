using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace bookify.Web.Core.ViewModels
{
    public class SubscriberFormViewModel
    {
		public string? Key{ get; set; }

		[Display(Name = "First Name"), RegularExpression(RegexPatterns.DenySpecialCharacters, ErrorMessage = Errors.DenySpecialCharacters)]
		[MaxLength(100,ErrorMessage = Errors.MaxLength)]
		public string FirstName { get; set; } = null!;

		[Display(Name = "Last Name"), RegularExpression(RegexPatterns.DenySpecialCharacters, ErrorMessage = Errors.DenySpecialCharacters)]
		[MaxLength(100, ErrorMessage = Errors.MaxLength)]
		public string LastName { get; set; } = null!;

		[Display(Name = "Date Of Birth")]
		[AssertThat("DateOfBirth <= Today()", ErrorMessage = Errors.NotAllowfutureDate)]
		public DateTime DateOfBirth { get; set; } = DateTime.Now;

        [Display(Name = "National ID")]
		[MaxLength(14, ErrorMessage = Errors.MaxLength)]
		[RegularExpression(RegexPatterns.NationalId,ErrorMessage =Errors.InvalidNationalId)]
		[Remote("AllowNationalId",null!, AdditionalFields = "Key", ErrorMessage = Errors.Duplicated)]
		public string NationalId { get; set; } = null!;

		[Display(Name = "Mobile Number")]
		[RegularExpression(RegexPatterns.MobileNumber,ErrorMessage =Errors.InvalidMobileNumber)]
		[MaxLength(15, ErrorMessage = Errors.MaxLength)]
		[Remote("AllowMobileNumber", null!, AdditionalFields = "Key", ErrorMessage = Errors.Duplicated)]
		public string MobileNumber { get; set; } = null!;

		[Display(Name = "Has WhatssApp?")]
		public bool HasWhatsApp { get; set; }

		[MaxLength(150, ErrorMessage = Errors.MaxLength)]
		[Remote("AllowEmail", null!, AdditionalFields = "Key", ErrorMessage = Errors.Duplicated)]
		public string Email { get; set; } = null!;

		[RequiredIf("Key == ''",ErrorMessage =Errors.EmptyImage)]
		public IFormFile? Image { get; set; } 
		public string? ImageUrl { get; set; } 
		public string? ImageThumbnailUrl { get; set; }

		[Display(Name = "Area")]
		public int AreaId { get; set; }
		public IEnumerable<SelectListItem>? Areas { get; set; }=new List<SelectListItem>();

		[Display(Name = "Governorate")]
		public int GovernorateId { get; set; }
		public IEnumerable<SelectListItem>? Governorates { get; set; }

		public string Address { get; set; } = null!;

	}
}
