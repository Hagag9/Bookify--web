namespace bookify.Web.Core.Consts
{
	public static class Errors
	{
		public const string MaxLength = "Length annot be more than {1} characters!";
		public const string Duplicated = "{0} with the same name is already exist!";
		public const string DuplicatedBook = "Book with the same title is already exist with the same author!";
		public const string NotAllowedExtension = "Only .png, .jpg, .jpeg files are allowed!";
		public const string MaxSize = "File canot more be than 2 MB!";
		public const string NotAllowfutureDate = "Date cannot be in the future!";
		public const string Range = "{0} should be between {1} and {2}!";
	}
}
