
namespace bookify.Web.Controllers
{
    public class AuthorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public AuthorsController(ApplicationDbContext context,IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        [HttpGet]
        public IActionResult Index()
        {
            var Authors=_context.Authors.AsNoTracking().ToList();
            return View(_mapper.Map<IEnumerable<AuthorViewModel>>(Authors));
        }
        [HttpGet]
        [AjaxOnly]
        public IActionResult Create()
        {
            return PartialView("_Form");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(AuthorFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var Author=_mapper.Map<Author>(model);
            _context.Authors.Add(Author);
            _context.SaveChanges();
            return PartialView("_AuthorRow", _mapper.Map<AuthorViewModel>(Author));
        }

        [HttpGet]
        [AjaxOnly]
        public IActionResult Edit(int Id)
        {
            var Author = _context.Authors.Find(Id);
            if (Author == null) 
               return NotFound(); 
            return PartialView("_Form", _mapper.Map<AuthorFormViewModel>(Author));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(AuthorFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var Author = _context.Authors.Find(model.Id);
            if (Author == null) 
            return NotFound(); 
            Author = _mapper.Map(model, Author);
            Author.LastUpdatedOn=DateTime.Now;
            _context.SaveChanges();
            return PartialView("_AuthorRow", _mapper.Map<AuthorViewModel>(Author));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int id)
        {
            var Author= _context.Authors.Find(id);
            if (Author == null)
                return NotFound();
            Author.IsDeleted = !Author.IsDeleted;
            Author.LastUpdatedOn= DateTime.Now;
            _context.SaveChanges();
            return Ok(Author.LastUpdatedOn.ToString());
        }
        public IActionResult UniqueName(AuthorFormViewModel model)
        {
			if (model.Name == (_context.Authors.Find(model.Id))?.Name)
				return Json(true);
			if (_context.Authors.Any(c => c.Name == model.Name))
                return Json(false);
            else return Json(true);
        }
    }
}
