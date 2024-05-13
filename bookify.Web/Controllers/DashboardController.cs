namespace bookify.Web.Controllers
{
    [Authorize]
    public class DashBoardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public DashBoardController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IActionResult Index()
        {
            // var numberOfCopies= _context.BookCopies.Count(c => !c.IsDeleted);
            var numberOfCopies = _context.Books.Count(b => !b.IsDeleted);
            numberOfCopies = numberOfCopies <= 10 ? numberOfCopies : numberOfCopies / 10 * 10;

            var numberOfSubscriber = _context.Subscribers.Count(s => !s.IsDeleted);

            var LastAddedBooks = _context.Books.Include(b => b.Author)
                .Where(b => !b.IsDeleted)
                .OrderByDescending(b => b.Id)
                .Take(8)
                .ToList();

            var topBooks = _context.RentalCopies
                .Include(c => c.BookCopy)
                .ThenInclude(c => c!.Book)
                .ThenInclude(b => b!.Author)
                .GroupBy(c => new
                {
                    c.BookCopy!.BookId,
                    c.BookCopy.Book!.Title,
                    c.BookCopy.Book.ImageThumbnailUrl,
                    AuthorName = c.BookCopy.Book.Author!.Name
                })
                .Select(b => new
                {
                    b.Key.BookId,
                    b.Key.Title,
                    b.Key.ImageThumbnailUrl,
                    b.Key.AuthorName,
                    Count = b.Count()
                }).OrderByDescending(b => b.Count)
                .Take(6)
                .Select(b => new BookViewModel
                {
                    Id = b.BookId,
                    Title = b.Title,
                    ImageThumbnailUrl = b.ImageThumbnailUrl,
                    Author = b.AuthorName,
                })
                .ToList();
            var viewModel = new DashboardViewModel()
            {
                NumberOfCopies = numberOfCopies,
                NumberOfSubscribers = numberOfSubscriber,
                LastAddedBooks = _mapper.Map<IEnumerable<BookViewModel>>(LastAddedBooks),
                TopBooks = topBooks
            };
            return View(viewModel);
        }

        [AjaxOnly]
        public IActionResult GetRentalsPerDay(DateTime? startDate, DateTime? endDate)
        {
            startDate ??= DateTime.Today.AddDays(-29);
            endDate ??= DateTime.Today;

            var data = _context.RentalCopies
                .Where(c => c.RentalDate >= startDate && c.RentalDate <= endDate)
                .GroupBy(c => new { Date = c.RentalDate })
                .Select(g => new ChartItemViewModel
                {
                    Label = g.Key.Date.ToString("d MMM"),
                    Value = g.Count().ToString()
                }).ToList();

            List<ChartItemViewModel> figures = new();

            for (var day = startDate; day <= endDate; day = day.Value.AddDays(1))
            {
                var dayData = data.SingleOrDefault(d => d.Label == day.Value.ToString("d MMM"));

                ChartItemViewModel item = new()
                {
                    Label = day.Value.ToString("d MMM"),
                    Value = dayData is null ? "0" : dayData.Value
                };
                figures.Add(item);
            }
            return Ok(figures);
        }
        [AjaxOnly]
        public IActionResult GetSubscribersPerCity()
        {
            var data = _context.Subscribers
                .Include(s => s.Governorate)
                .Where(s => !s.IsDeleted)
                .GroupBy(s => new { GovernorateName = s.Governorate!.Name })
                .Select(g => new ChartItemViewModel
                {
                    Label = g.Key.GovernorateName,
                    Value = g.Count().ToString()
                }).ToList();

            return Ok(data);
        }
    }
}
