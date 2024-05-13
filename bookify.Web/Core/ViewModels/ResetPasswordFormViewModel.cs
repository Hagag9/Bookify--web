namespace bookify.Web.Core.ViewModels
{
    public class ResetPasswordFormViewModel
    {
        public string Id { get; set; } = null!;

        [Display(Name = "New password"), StringLength(100, ErrorMessage = Errors.MaxMinLength, MinimumLength = 8), DataType(DataType.Password),
            RegularExpression(RegexPatterns.Password, ErrorMessage = Errors.WeakPassword)]
        public string NewPassword { get; set; } = null!;

        [Compare("NewPassword", ErrorMessage = Errors.ConfirmPasswordNotmatch), DataType(DataType.Password), Display(Name = "Confirm new password")]
        public string ConfirmNewPassword { get; set; } = null!;
    }
}
