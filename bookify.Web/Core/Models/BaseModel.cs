namespace bookify.Web.Core.Models
{
	public class BaseModel
	{

		public bool IsDeleted { get; set; }

		public DateTime CreatedOn { get; set; } = DateTime.Now;

		public DateTime? LastUpdatedOn { get; set; }
	}
}
