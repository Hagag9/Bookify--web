using Microsoft.AspNetCore.Mvc;

namespace bookify.Web.Controllers
{
    public class SubscribersController : Controller
    {
        private readonly ApplicationDbContext _context;
		private readonly IMapper _mapper;
        private readonly IImageService _imageService;

		public SubscribersController(ApplicationDbContext context, IMapper mapper, IImageService imageService)
		{
			_context = context;
			_mapper = mapper;
			_imageService = imageService;
		}

		public IActionResult Index()
        {
        
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Search(SearchFormViewModel model)
        {
			if(!ModelState.IsValid)
				return BadRequest(ModelState);

			var subscriber = _context.Subscribers
				.SingleOrDefault(s=>s.NationalId == model.Value||s.MobileNumber == model.Value||s.Email== model.Value);
			var viewModel = _mapper.Map<SubscriberSearchResultViewModel>(subscriber);
			return PartialView("_Result", viewModel);
			
        }
		public IActionResult Details(int id)
		{
			var subscriber = _context.Subscribers

                .Include(s=>s.Governorate)
				.Include(s=>s.Area)
				.SingleOrDefault(s=>s.Id==id);

			if(subscriber is null)
				return NotFound();

			var viewModel = _mapper.Map<SubscriberViewModel>(subscriber);

			return View("Details",viewModel);
		}
        public IActionResult Create()
        {
			return View("Form", PopulateViewModel());
        }
        [HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(SubscriberFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form",PopulateViewModel(model));

			var subscriber = _mapper.Map<Subscriber>(model);

			var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image!.FileName)}";

            var (isUpload, errorMessage) = await _imageService.UploadAsync(model.Image,imageName,"/images/subscribers/",true);
             if(!isUpload)
            {
				ModelState.AddModelError(nameof(Image), errorMessage!);
				return View("Form", PopulateViewModel(model));
			}
            subscriber.ImageUrl=$"/images/subscribers/{imageName}";
            subscriber.ImageThumbnailUrl = $"/images/subscribers/thumb/{imageName}";
            subscriber.CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            _context.Subscribers.Add(subscriber);
            _context.SaveChanges();

			return RedirectToAction(nameof(Details), new {id=subscriber.Id});
		}
		public IActionResult Edit(int id)
		{
			var subscriber = _context.Subscribers.Find(id);
			if (subscriber is null)
				return NotFound();
			var viewModel = _mapper.Map<SubscriberFormViewModel>(subscriber);
			return View("Form", PopulateViewModel(viewModel));
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(SubscriberFormViewModel model)
		{
			if (!ModelState.IsValid)
				return View("Form", PopulateViewModel(model));

			var subscriber = _context.Subscribers.Find(model.Id);
			if (subscriber is null)
				return NotFound();

			if(model.Image is not null)
			{
				var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image!.FileName)}";
				var (isUpload, errorMessage) = await _imageService.UploadAsync(model.Image, imageName, "/images/subscribers/", true);
				if (!isUpload)
				{
					ModelState.AddModelError(nameof(Image), errorMessage!);
					return View("Form", PopulateViewModel(model));
				}
				if(!string.IsNullOrEmpty(subscriber.ImageUrl))
				_imageService.Delete(subscriber.ImageUrl, subscriber.ImageThumbnailUrl);
				model.ImageUrl = $"/images/subscribers/{imageName}";
				model.ImageThumbnailUrl= $"/images/subscribers/thumb/{imageName}";
			}
			else if(!string.IsNullOrEmpty(subscriber.ImageUrl))
			{
				model.ImageUrl=subscriber.ImageUrl;
				model.ImageThumbnailUrl= subscriber.ImageThumbnailUrl;
			}
			subscriber = _mapper.Map(model,subscriber);
			subscriber.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
			subscriber.LastUpdatedOn = DateTime.Now;
			_context.SaveChanges();

			return RedirectToAction(nameof(Details), new { id = subscriber.Id });
		}
		[AjaxOnly]
        public IActionResult GetAreas(int governorateId)
        {
            var areas = _context.Areas.Where(a => a.GovernorateId == governorateId && !a.IsDeleted)
                .OrderBy(a => a.Name).ToList();
            return Ok(_mapper.Map<IEnumerable<SelectListItem>>(areas));
        }
        public IActionResult AllowNationalId(SubscriberFormViewModel model)
        {
            var subscriber= _context.Subscribers.SingleOrDefault(s=>s.NationalId==model.NationalId);
            bool isAllow = subscriber is null || subscriber.Id==model.Id;
            return Json(isAllow);
		}
		public IActionResult AllowMobileNumber(SubscriberFormViewModel model)
		{
			var subscriber = _context.Subscribers.SingleOrDefault(s => s.MobileNumber == model.MobileNumber);
			bool isAllow = subscriber is null || subscriber.Id == model.Id;
			return Json(isAllow);
		}
		public IActionResult AllowEmail(SubscriberFormViewModel model)
		{
			var subscriber = _context.Subscribers.SingleOrDefault(s => s.Email == model.Email);
			bool isAllow = subscriber is null || subscriber.Id == model.Id;
			return Json(isAllow);
		}
        private SubscriberFormViewModel PopulateViewModel(SubscriberFormViewModel? model = null)
        {
			SubscriberFormViewModel viewModel =model is null ? new SubscriberFormViewModel() : model;   

            var governorate = _context.Governorates.Where(g=>!g.IsDeleted).OrderBy(g=>g.Name).ToList();
            viewModel.Governorates = _mapper.Map<IEnumerable<SelectListItem>>(governorate);
            if(model?.GovernorateId>0)
            {
				var Areas = _context.Areas.Where(g =>g.GovernorateId==model.GovernorateId && !g.IsDeleted).OrderBy(g => g.Name).ToList();
				viewModel.Areas = _mapper.Map<IEnumerable<SelectListItem>>(Areas);
			}
            return viewModel;
		}
	}
}
