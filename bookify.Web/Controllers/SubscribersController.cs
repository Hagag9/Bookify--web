using Hangfire;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace bookify.Web.Controllers
{
    [Authorize(Roles = AppRoles.Reception)]
    public class SubscribersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IDataProtector _dataProtector;
        private readonly IMapper _mapper;
        private readonly IWhatsAppClient _whatsAppClient;
        private readonly IImageService _imageService;
        private readonly IEmailBodyBuilder _emailBodyBuilder;
        private readonly IEmailSender _emailSender;

        public SubscribersController(ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment,
            IDataProtectionProvider dataProtector,
            IMapper mapper, IWhatsAppClient whatsAppClient,
            IImageService imageService, IEmailBodyBuilder emailBodyBuilder,
            IEmailSender emailSender)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _dataProtector = dataProtector.CreateProtector("MySecureKey");
            _mapper = mapper;
            _whatsAppClient = whatsAppClient;
            _imageService = imageService;
            _emailBodyBuilder = emailBodyBuilder;
            _emailSender = emailSender;
        }

        public IActionResult Index()
        {

            return View();
        }
        [HttpPost]
        public IActionResult Search(SearchFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var subscriber = _context.Subscribers
                .SingleOrDefault(s => s.NationalId == model.Value || s.MobileNumber == model.Value || s.Email == model.Value);

            var viewModel = _mapper.Map<SubscriberSearchResultViewModel>(subscriber);

            if (subscriber is not null)
                viewModel.Key = _dataProtector.Protect(subscriber.Id.ToString());

            return PartialView("_Result", viewModel);
        }
        public IActionResult Details(string id)
        {
            var SubscriberId = int.Parse(_dataProtector.Unprotect(id));
            var subscriber = _context.Subscribers

                .Include(s => s.Governorate)
                .Include(s => s.Area)
                .Include(s => s.Subscriptions)
                .Include(s => s.Rentals)
                .ThenInclude(r => r.RentalCopies)
                .SingleOrDefault(s => s.Id == SubscriberId);

            if (subscriber is null)
                return NotFound();

            var viewModel = _mapper.Map<SubscriberViewModel>(subscriber);
            viewModel.Key = id;
            return View("Details", viewModel);
        }
        public IActionResult Create()
        {
            return View("Form", PopulateViewModel());
        }
        [HttpPost]
        public async Task<IActionResult> Create(SubscriberFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", PopulateViewModel(model));

            var subscriber = _mapper.Map<Subscriber>(model);

            var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image!.FileName)}";

            var (isUpload, errorMessage) = await _imageService.UploadAsync(model.Image, imageName, "/images/subscribers/", true);
            if (!isUpload)
            {
                ModelState.AddModelError(nameof(Image), errorMessage!);
                return View("Form", PopulateViewModel(model));
            }
            subscriber.ImageUrl = $"/images/subscribers/{imageName}";
            subscriber.ImageThumbnailUrl = $"/images/subscribers/thumb/{imageName}";
            subscriber.CreatedById = User.GetUserId();

            subscriber.Subscriptions.Add(
                  new Subscription()
                  {
                      CreatedById = subscriber.CreatedById,
                      CreatedOn = subscriber.CreatedOn,
                      StartDate = DateTime.Today,
                      EndDate = DateTime.Today.AddYears(1)
                  });

            _context.Subscribers.Add(subscriber);
            _context.SaveChanges();
            //send welcome Email
            var placeHolders = new Dictionary<string, string>()
                {
                    {"imageUrl","https://res.cloudinary.com/ahagag/image/upload/v1712359686/icon-positive-vote-2_gpjp26.png"},
                    {"header", $"Welcome {model.FirstName},"},
                    {"body" , "thanks for joining Bookify 😍"},
                };

            var body = _emailBodyBuilder.GetEmailBody(EmailTemplates.Notification, placeHolders);
            BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(model.Email, "Welcome to Bookify", body));
            //send welcome WhatsApp message 
            if (model.HasWhatsApp)
            {
                var components = new List<WhatsAppComponent>()
            {
                new WhatsAppComponent
                {
                    Type="body",
                    Parameters = new List<object>()
                    {
                        new WhatsAppTextParameter { Text = model.FirstName}
                    }
                }
            };
                var mobileNumber = _webHostEnvironment.IsDevelopment() ? "01227232423" : model.MobileNumber;
                BackgroundJob.Enqueue(() => _whatsAppClient.SendMessage($"2{mobileNumber}", WhatsAppLanguageCode.English_US, WhatsAppTemplates.WelcomeMessage, components));
            }
            var subscriberId = _dataProtector.Protect(subscriber.Id.ToString());
            return RedirectToAction(nameof(Details), new { id = subscriberId });
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
        public async Task<IActionResult> Edit(SubscriberFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", PopulateViewModel(model));

            var subscriberId = int.Parse(_dataProtector.Unprotect(model.Key!));
            var subscriber = _context.Subscribers.Find(subscriberId);

            if (subscriber is null)
                return NotFound();

            if (model.Image is not null)
            {
                var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image!.FileName)}";
                var (isUpload, errorMessage) = await _imageService.UploadAsync(model.Image, imageName, "/images/subscribers/", true);
                if (!isUpload)
                {
                    ModelState.AddModelError(nameof(Image), errorMessage!);
                    return View("Form", PopulateViewModel(model));
                }
                if (!string.IsNullOrEmpty(subscriber.ImageUrl))
                    _imageService.Delete(subscriber.ImageUrl, subscriber.ImageThumbnailUrl);
                model.ImageUrl = $"/images/subscribers/{imageName}";
                model.ImageThumbnailUrl = $"/images/subscribers/thumb/{imageName}";
            }
            else if (!string.IsNullOrEmpty(subscriber.ImageUrl))
            {
                model.ImageUrl = subscriber.ImageUrl;
                model.ImageThumbnailUrl = subscriber.ImageThumbnailUrl;
            }
            subscriber = _mapper.Map(model, subscriber);
            subscriber.LastUpdatedById = User.GetUserId();
            subscriber.LastUpdatedOn = DateTime.Now;
            _context.SaveChanges();

            return RedirectToAction(nameof(Details), new { id = model.Key });
        }
        [HttpPost]
        public IActionResult RenewSubscription(string sKey)
        {
            var subscriberId = int.Parse(_dataProtector.Unprotect(sKey));
            var subscriber = _context.Subscribers.Include(s => s.Subscriptions).FirstOrDefault(s => s.Id == subscriberId);
            if (subscriber == null)
                return NotFound();
            if (subscriber.IsBlackListed)
                return BadRequest();

            var lastsubscription = subscriber.Subscriptions.Last();
            var startDate = lastsubscription.EndDate < DateTime.Today ? DateTime.Today : lastsubscription.EndDate.AddDays(1);
            Subscription newSubscription = new()
            {
                CreatedById = User.GetUserId(),
                CreatedOn = DateTime.Now,
                StartDate = startDate,
                EndDate = startDate.AddYears(1)
            };
            subscriber.Subscriptions.Add(newSubscription);
            _context.SaveChanges();
            //TODO : Send Email and whatsApp message
            //send welcome Email
            var placeHolders = new Dictionary<string, string>()
                {
                    {"imageUrl","https://res.cloudinary.com/ahagag/image/upload/v1712359686/icon-positive-vote-2_gpjp26.png"},
                    {"header", $"Hello {subscriber.FirstName},"},
                    {"body" , $"Your subscription has been renewed through {newSubscription.EndDate.ToString("d MMM, yyyy")} 🎉🎉"},
                };
            var body = _emailBodyBuilder.GetEmailBody(EmailTemplates.Notification, placeHolders);
            BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(subscriber.Email, "Bookify Subscription Renewal", body));

            //send welcome WhatsApp message 
            if (subscriber.HasWhatsApp)
            {
                var components = new List<WhatsAppComponent>()
            {
                new WhatsAppComponent
                {
                    Type="body",
                    Parameters = new List<object>()
                    {
                        new WhatsAppTextParameter { Text = subscriber.FirstName},
                        new WhatsAppTextParameter { Text = newSubscription.EndDate.ToString("d MMM, yyyy")}
                    }
                }
            };
                var mobileNumber = _webHostEnvironment.IsDevelopment() ? "01227232423" : subscriber.MobileNumber;
                BackgroundJob.Enqueue(() => _whatsAppClient.SendMessage($"2{mobileNumber}", WhatsAppLanguageCode.English_US, WhatsAppTemplates.RenewSubscription, components));
            }
            return PartialView("_SubscriptionRow", _mapper.Map<SubscriptionViewModel>(newSubscription));
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
            if (!string.IsNullOrEmpty(model.Key))
                SubscriberId = int.Parse(_dataProtector.Unprotect(model.Key));

            var subscriber = _context.Subscribers.SingleOrDefault(s => s.NationalId == model.NationalId);
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
            SubscriberFormViewModel viewModel = model is null ? new SubscriberFormViewModel() : model;

            var governorate = _context.Governorates.Where(g => !g.IsDeleted).OrderBy(g => g.Name).ToList();
            viewModel.Governorates = _mapper.Map<IEnumerable<SelectListItem>>(governorate);
            if (model?.GovernorateId > 0)
            {
                var Areas = _context.Areas.Where(g => g.GovernorateId == model.GovernorateId && !g.IsDeleted).OrderBy(g => g.Name).ToList();
                viewModel.Areas = _mapper.Map<IEnumerable<SelectListItem>>(Areas);
            }
            return viewModel;
        }
    }
}
