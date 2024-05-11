using HashidsNet;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.Generic;
using System.Diagnostics;

namespace bookify.Web.Controllers
{
   
    public class HomeController : Controller
    {
		private readonly ILogger<HomeController> _logger;
		private readonly ApplicationDbContext _context;
		private readonly IMapper _mapper;
		private readonly IHashids _hashids;
		public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IMapper mapper, IHashids hashids)
		{
			_logger = logger;
			_context = context;
			_mapper = mapper;
			_hashids = hashids;
		}
		public IActionResult Index()
        {
			if (User.Identity!.IsAuthenticated)
				return RedirectToAction(nameof(Index), "Dashboard");

			var LastAddedBooks = _context.Books.Include(b => b.Author)
				.Where(b => !b.IsDeleted)
				.OrderByDescending(b => b.Id)
				.Take(10)
				.ToList();
			var viewModel = _mapper.Map<IEnumerable<BookViewModel>>(LastAddedBooks);
			foreach (var book in viewModel)
			{
				book.Key = _hashids.EncodeHex(book.Id.ToString());
			}
			return View(viewModel);
        }
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int statusCode=500)
        {
            return View(new ErrorViewModel {ErrorCode= statusCode, Errordescription=ReasonPhrases.GetReasonPhrase(statusCode)});
        }
    }
}