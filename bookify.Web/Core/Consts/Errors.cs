namespace bookify.Web.Core.Consts
{
    public static class Errors
    {
        public const string MaxLength = "Length annot be more than {1} characters!";
        public const string RequierdField = "This field is requierd!";
        public const string MaxMinLength = "The {0} must be at least {2} and at max {1} characters long.";
        public const string Duplicated = "Another record with the same {0} is already exist!";
        public const string DuplicatedBook = "Book with the same title is already exist with the same author!";
        public const string NotAllowedExtension = "Only .png, .jpg, .jpeg files are allowed!";
        public const string MaxSize = "File canot more be than 2 MB!";
        public const string NotAllowfutureDate = "Date cannot be in the future!";
        public const string Range = "{0} should be between {1} and {2}!";
        public const string ConfirmPasswordNotmatch = "The password and confirmation password do not match.";
        public const string WeakPassword = "Passwords contain an uppercase character, lowercase character, a digit, and a non-alphanumeric character. Passwords must be at least 8 characters long";
        public const string InvalidUserName = "Username can only contain letters or digits.";
        public const string OnlyEnglishLetters = "Only English letters are allowed.";
        public const string OnlyArabicLetters = "Only Arabic letters are allowed.";
        public const string OnlyNumbersAndLetters = "Only Arabic/English letters or digits are allowed.";
        public const string DenySpecialCharacters = "Special characters are not allowed.";
        public const string InvalidMobileNumber = "Invalid mobile number.";
        public const string InvalidNationalId = "Invalid national ID.";
        public const string InvalidSerialNumber = "Invalid serial number.";
        public const string NotAvilableRental = "This book/copy is not available for rental.";
        public const string EmptyImage = "Please select an image.";
        public const string BlackListedSubscriber = "This subscriber is blacklisted.";
        public const string InActiveSubscriber = "This subscriber is InActive.";
        public const string MaxCopiesReached = "This subscriber has reached the max number for rentals.";
        public const string CopyIsInRental = "This copy is already rentaled.";
        public const string RentalNotAllowedForBlacklisted = "Rental cannot be extended for blacklisted subscribers.";
        public const string RentalNotAllowedForInactive = "Rental cannot be extended for this subscribers before renewal.";
        public const string ExtendNotAllowed = "Rental cannot be extended.";
        public const string PenaltyShouldBePaid = "Penalty should be paid.";
        public const string InvalidStartDate = "Invalid start date.";
        public const string InvalidEndDate = "Invalid end date.";
    }
}
