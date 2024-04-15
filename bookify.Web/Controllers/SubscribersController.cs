﻿using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

namespace bookify.Web.Controllers
{
	[Authorize(Roles =AppRoles.Reception)]
    public class SubscribersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDataProtector _dataProtector;
		private readonly IMapper _mapper;
        private readonly IImageService _imageService;

        public SubscribersController(ApplicationDbContext context, IDataProtectionProvider dataProtector, IMapper mapper, IImageService imageService)
        {
            _context = context;
            _dataProtector = dataProtector.CreateProtector("MySecureKey");
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

			if(subscriber is not null)
				viewModel.Key=_dataProtector.Protect(subscriber.Id.ToString());

			return PartialView("_Result", viewModel);		
        }
		public IActionResult Details(string id)
		{
			var SubscriberId = int.Parse(_dataProtector.Unprotect(id));
			var subscriber = _context.Subscribers

                .Include(s=>s.Governorate)
				.Include(s=>s.Area)
				.SingleOrDefault(s=>s.Id == SubscriberId);

			if(subscriber is null)
				return NotFound();

			var viewModel = _mapper.Map<SubscriberViewModel>(subscriber);
			viewModel.Key = id;
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

			var subscriberId= _dataProtector.Protect(subscriber.Id.ToString());
			return RedirectToAction(nameof(Details), new {id = subscriberId});
		}
		public IActionResult Edit(string id)
		{
			var subscriberId = int.Parse(_dataProtector.Unprotect(id));
			var subscriber = _context.Subscribers.Find(subscriberId);
			if (subscriber is null)
				return NotFound();
			var viewModel = _mapper.Map<SubscriberFormViewModel>(subscriber);
			viewModel.Key = id;
			return View("Form", PopulateViewModel(viewModel));
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(SubscriberFormViewModel model)
		{
			if (!ModelState.IsValid)
				return View("Form", PopulateViewModel(model));

			var subscriberId = int.Parse(_dataProtector.Unprotect(model.Key!));	
			var subscriber = _context.Subscribers.Find(subscriberId);

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

			return RedirectToAction(nameof(Details), new { id = model.Key });
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
			int SubscriberId = 0;
			if(!string.IsNullOrEmpty(model.Key))
                SubscriberId = int.Parse(_dataProtector.Unprotect(model.Key));

            var subscriber= _context.Subscribers.SingleOrDefault(s=>s.NationalId==model.NationalId);
            bool isAllow = subscriber is null || subscriber.Id == SubscriberId;
            return Json(isAllow);
		}
		public IActionResult AllowMobileNumber(SubscriberFormViewModel model)
		{
            int SubscriberId = 0;
            if (!string.IsNullOrEmpty(model.Key))
                SubscriberId = int.Parse(_dataProtector.Unprotect(model.Key)); 

            var subscriber = _context.Subscribers.SingleOrDefault(s => s.MobileNumber == model.MobileNumber);
			bool isAllow = subscriber is null || subscriber.Id == SubscriberId;
			return Json(isAllow);
		}
		public IActionResult AllowEmail(SubscriberFormViewModel model)
		{
            int SubscriberId = 0;
            if (!string.IsNullOrEmpty(model.Key))
                SubscriberId = int.Parse(_dataProtector.Unprotect(model.Key));
            var subscriber = _context.Subscribers.SingleOrDefault(s => s.Email == model.Email);
			bool isAllow = subscriber is null || subscriber.Id == SubscriberId;
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