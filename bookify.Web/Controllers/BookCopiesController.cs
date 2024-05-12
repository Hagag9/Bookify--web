namespace bookify.Web.Controllers
{
	[Authorize(Roles = AppRoles.Archive)]
	public class BookCopiesController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly IMapper _mapper;

		public BookCopiesController(ApplicationDbContext context, IMapper mapper)
		{
			_context = context;
			_mapper = mapper;
		}
		public IActionResult Index()
		{
			return View();
		}
		[AjaxOnly]
		public IActionResult Create(int bookId)
		{
			var book = _context.Books.Find(bookId);
			if (book is null) 
				return NotFound();
			var viewModel = new BookCopyFormViewModel
			{
				BookId = bookId,
				ShowRentalInput = book.IsAvailableForRental
			};
			return PartialView("_Form",viewModel);
		}
		[HttpPost]
        public IActionResult Create(BookCopyFormViewModel model)
        {
			if (!ModelState.IsValid)
			return BadRequest();
            var book = _context.Books.Find(model.BookId);
            if (book is null)
                return NotFound();
            var copy = new BookCopy
            {
              EditionNumber = model.EditionNumber,
			  IsAvailableForRental = model.IsAvailableForRental && book.IsAvailableForRental,
			  CreatedById = User.GetUserId()
		};
			book.Copies.Add(copy);
			_context.SaveChanges();
			var viewModel = _mapper.Map<BookCopyViewModel>(copy);
            return PartialView("_BookcopyRow",viewModel);
        }
		[AjaxOnly]
		public IActionResult Edit(int id)
		{
			var copy = _context.BookCopies.Include(c => c.Book).SingleOrDefault(c => c.Id == id);
			if (copy is null)
				return NotFound();
			var viewModel = _mapper.Map<BookCopyFormViewModel>(copy);
			viewModel.ShowRentalInput=copy.Book!.IsAvailableForRental;
			return PartialView("_Form", viewModel);
		}
		[HttpPost]
		public IActionResult Edit(BookCopyFormViewModel model) 
		{
			if(!ModelState.IsValid)  
				return BadRequest();
			var copy = _context.BookCopies.Include(c=>c.Book).SingleOrDefault(c=>c.Id == model.Id);
			if (copy is null)
				return NotFound();
			copy = _mapper.Map(model, copy);
			copy.IsAvailableForRental = model.IsAvailableForRental && copy.Book!.IsAvailableForRental;
			copy.LastUpdatedById = User.GetUserId();
			copy.LastUpdatedOn=DateTime.Now;
			_context.SaveChanges();
			var viewModel = _mapper.Map<BookCopyViewModel>(copy);
			return PartialView("_BookcopyRow", viewModel);
		}
		public IActionResult RentalHistory(int id)
		{
            var copyHistory = _context.RentalCopies
                .Include(c => c.Rental)
                .ThenInclude(r => r!.Subscriber)
                .Where(c => c.BookCopyId == id)
                .OrderByDescending(c => c.RentalDate)
                .ToList();

            var viewModel = _mapper.Map<IEnumerable<CopyHistoryViewModel>>(copyHistory);

            return View(viewModel);
        }
        [HttpPost]
		public IActionResult ToggleStatus(int id)
		{
			var copy = _context.BookCopies.Find(id);
			if (copy == null) 
				return NotFound();
			copy.IsDeleted=!copy.IsDeleted;
			copy.LastUpdatedById = User.GetUserId();
			copy.LastUpdatedOn= DateTime.Now;
			_context.SaveChanges();
			return Ok();
		}
	}
}
