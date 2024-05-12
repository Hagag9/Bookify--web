

namespace bookify.Web.Controllers
{
	[Authorize(Roles = AppRoles.Archive)]
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
        public IActionResult Create(AuthorFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var Author=_mapper.Map<Author>(model);
            Author.CreatedById = User.GetUserId();
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
        public IActionResult Edit(AuthorFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var Author = _context.Authors.Find(model.Id);
            if (Author == null) 
            return NotFound(); 
            Author = _mapper.Map(model, Author);
			Author.LastUpdatedById = User.GetUserId();
			Author.LastUpdatedOn=DateTime.Now;          
			_context.SaveChanges();
            return PartialView("_AuthorRow", _mapper.Map<AuthorViewModel>(Author));
        }
        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            var Author= _context.Authors.Find(id);
            if (Author == null)
                return NotFound();
            Author.IsDeleted = !Author.IsDeleted;
			Author.LastUpdatedById = User.GetUserId();
			Author.LastUpdatedOn= DateTime.Now;
            _context.SaveChanges();
            return Ok(Author.LastUpdatedOn.ToString());
        }
        public IActionResult UniqueName(AuthorFormViewModel model)
        {
		    var author = _context.Authors.SingleOrDefault(a => a.Name== model.Name);
            var isAllow = author is null || author.Id == model.Id;

            return Json(isAllow);
        }
    }
}
