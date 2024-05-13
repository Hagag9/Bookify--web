using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace bookify.Web.Core.ViewModels
{
    public class RentalReturnFormViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Penalty Paid?")]
        [AssertThat("(TotalDelayInDays == 0 && PenaltyPaid == false) ||  && PenaltyPaid == true ", ErrorMessage = Errors.PenaltyShouldBePaid)]
        public bool PenaltyPaid { get; set; }

        public IList<RentalCopyViewModel> Copies { get; set; } = new List<RentalCopyViewModel>();

        public List<ReturnCopyViewModel> SelectedCopies { get; set; } = new List<ReturnCopyViewModel> { };

        public bool AllowExtend { get; set; }

        public int TotalDelayInDays
        {
            get
            {
                return Copies.Sum(c => c.DelayInDays);
            }
        }
    }
}
