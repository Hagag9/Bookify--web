namespace bookify.Web.Core.ViewModels
{
	public class RentalCopyViewModel
	{
		public BookCopyViewModel? BookCopy { get; set; }

		public DateTime RentalDate { get; set; } 
		public DateTime EndDate { get; set; }

		public DateTime? ReturnDate { get; set; }
		public DateTime? ExtendedOn { get; set; }

		public int DelayInDays
		{
			get
			{
				return (ReturnDate.HasValue && ReturnDate > EndDate) ? (int)(ReturnDate.Value - EndDate).TotalDays :
						(!ReturnDate.HasValue && DateTime.Today > EndDate) ? (int)(DateTime.Today - EndDate).TotalDays:0 ;
			}
		}
	}
}
