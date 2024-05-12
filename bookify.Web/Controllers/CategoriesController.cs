
namespace bookify.Web.Controllers
{
	[Authorize(Roles = AppRoles.Archive)]
	public class CategoriesController : Controller
    {
        readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CategoriesController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;       
        }
        [HttpGet]
        public IActionResult Index()
        {
            var Categories = _context.Categories .AsNoTracking().ToList();
            var ViewModel = _mapper.Map<IEnumerable<CategoryViewModel>>(Categories);
            return View(ViewModel);
        }
        [HttpGet]
        [AjaxOnly]
        public IActionResult Create()
        {      
            return PartialView("_Form");
        }
        [HttpPost]
		public IActionResult Create(CategoryFormViewModel model)
        {   
            if (!ModelState.IsValid)
                 return BadRequest();
            var category = _mapper.Map<Category>(model);
            category.CreatedById= User.GetUserId();
			_context.Add(category);
            _context.SaveChanges();
            var CatViewModel = _mapper.Map<CategoryViewModel>(category);
        
            return PartialView("_CategoryRow", CatViewModel);
        }
        [HttpGet]
		[AjaxOnly]
		public IActionResult Edit(int id)
		{
            var category = _context.Categories.Find(id);
            if (category is null)
                return NotFound();
            var CatFormModel = _mapper.Map<CategoryFormViewModel>(category);
			return PartialView("_Form", CatFormModel);
		}
        [HttpPost]
		public IActionResult Edit(CategoryFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var category = _context.Categories.Find(model.Id);
            if (category is null)
                return NotFound();

            category = _mapper.Map(model,category); 
            category.LastUpdatedById= User.GetUserId();
			category.LastUpdatedOn=DateTime.Now;
            _context.SaveChanges();
			var CatViewModel = _mapper.Map<CategoryViewModel>(category);
			return PartialView("_CategoryRow", CatViewModel);
		}
        [HttpPost]
		public IActionResult ToggleStatus(int id)
        {
			
			var category = _context.Categories.Find(id);
			if (category is null)
				return NotFound();

            category.IsDeleted = !category.IsDeleted;
            category.LastUpdatedById= User.GetUserId();
			category.LastUpdatedOn = DateTime.Now;
			_context.SaveChanges();
			return Ok(category.LastUpdatedOn.ToString());
		}
        public IActionResult UniqueName(CategoryFormViewModel model)
        {
			var category = _context.Categories.SingleOrDefault(a => a.Name == model.Name);
			var isAllow = category is null || category.Id == model.Id;

			return Json(isAllow);
		}
    }
}
