using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using System.Linq.Dynamic.Core;

namespace bookify.Web.Controllers
{
	public class BooksController : Controller
	{
		private readonly IWebHostEnvironment _webHostEnvironment;
		private readonly ApplicationDbContext _context;
		private readonly IMapper _mapper;
		private readonly Cloudinary _cloudinary;
		private List<string> _allowedExtensions = new() { ".jpg", ".jpeg", ".png" };
		private int _maxAllowedSize = 2097152;  // 2MG

		public BooksController(ApplicationDbContext context, IMapper mapper, IWebHostEnvironment webHostEnvironment,
			IOptions<CloudinarySettings> cloudinary)
		{
			_context = context;
			_mapper = mapper;
			_webHostEnvironment = webHostEnvironment;

			Account account = new ()
			{
				Cloud=cloudinary.Value.Cloud,
				ApiKey=cloudinary.Value.ApiKey,
				ApiSecret=cloudinary.Value.ApiSecret,
			};
			_cloudinary= new Cloudinary(account);
		}

		public IActionResult Index()
		{
			return View();
		}
		[HttpPost]
		public IActionResult GetBooks()
		{
			var skip = int.Parse(Request.Form["start"]);
			var pageSize = int.Parse(Request.Form["length"]);

			var searchValue = Request.Form["search[value]"];

			var sortColIndex = Request.Form["order[0][column]"];
			var sortCol = Request.Form[$"columns[{sortColIndex}][name]"];
			var sortColDir = Request.Form["order[0][dir]"];

			IQueryable<Book> books = _context.Books.Include(b=>b.Author)
													.Include(b => b.Categories)
													.ThenInclude(c => c.Category);

			if (!string.IsNullOrEmpty(searchValue))
				books = books.Where(b => b.Title.Contains(searchValue) || b.Author!.Name.Contains(searchValue));

			books = books.OrderBy($"{sortCol} {sortColDir}");
			var data = books.Skip(skip).Take(pageSize).ToList();
			var mappedData = _mapper.Map<IEnumerable<BookViewModel>>(data);
			var recordsTotal=books.Count();

			return Ok(new {recordsFiltered = recordsTotal , recordsTotal , data= mappedData });
		}
		[HttpGet]
		public IActionResult Details(int id)
		{
			var book = _context.Books.Include(b => b.Author)
									.Include(b=>b.Categories)
									.ThenInclude(c=>c.Category)
									.SingleOrDefault(b => b.Id == id);
			if (book is null)
				return NotFound();
			var model = _mapper.Map<BookViewModel>(book);
			return View(model);
		}
		[HttpGet]
		public IActionResult Create()
		{
		
			return View("Form", PopulateViewModel());
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(BookFormViewModel model)
		{
			if (!ModelState.IsValid) 
				return View("Form", PopulateViewModel(model));
			var book = _mapper.Map<Book>(model);
			if(model.Image is not null)
			{
				var extension = Path.GetExtension(model.Image.FileName);
				if (!_allowedExtensions.Contains(extension))
				{
					ModelState.AddModelError(nameof(model.Image), Errors.NotAllowedExtension);
					return View("Form", PopulateViewModel(model));
				}
				if(model.Image.Length > _maxAllowedSize)
				{
					ModelState.AddModelError(nameof(model.Image), Errors.MaxSize);
					return View("Form", PopulateViewModel(model));
				}
				var imageName = $"{Guid.NewGuid()}{extension}";

				var path = Path.Combine($"{_webHostEnvironment.WebRootPath}/images/books", imageName);
				var thumbPath = Path.Combine($"{_webHostEnvironment.WebRootPath}/images/books/thumb", imageName);
				using var stream = System.IO.File.Create(path);
				await model.Image.CopyToAsync(stream);
				book.ImageUrl = $"/images/books/{imageName}";
				book.ImageThumbnailUrl = $"/images/books/thumb/{imageName}";

				using var image = Image.Load(model.Image.OpenReadStream());
				var ratio = (float)image.Width / 200;
				var height = image.Height/ratio;
				image.Mutate(i=>i.Resize(width:200, height:(int)height));
				image.Save(thumbPath);
				// >>>>>>Clpodinary<<<<<<//
				//using var stream = model.Image.OpenReadStream();
				//var imageParams = new ImageUploadParams
				//{
				//	File = new FileDescription(imageName, stream),
				//	UseFilename=true
				//};
				//try
				//{
				//	var result = await _cloudinary.UploadAsync(imageParams);
				//	book.ImageUrl = result.SecureUrl.ToString();
				//	book.ImageThumbnailUrl = GetThmbnailUrl(book.ImageUrl);
				//	book.ImagePublicId = result.PublicId;
				//}
				//catch
				//{
				//	return BadRequest();
				//}
			}
			foreach (var category in model.SelectedCategories)
				book.Categories.Add(new BookCategory { CategoryId = category });

			_context.Books.Add(book);
			_context.SaveChanges();
			return RedirectToAction(nameof(Details), new {id=book.Id});
		}
		[HttpGet]
		public IActionResult Edit(int Id)
		{
			var book = _context.Books.Include(b=>b.Categories).SingleOrDefault(b=>b.Id==Id);	
			if(book is null)
				return NotFound();

			var viewModel = _mapper.Map<BookFormViewModel>(book);
			viewModel.SelectedCategories=book.Categories.Select(c=>c.CategoryId).ToList();
						
			return View("Form", PopulateViewModel(viewModel));
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(BookFormViewModel model)
		{
			if (!ModelState.IsValid)
				return View("Form", PopulateViewModel(model));
			var book = _context.Books.Include(b => b.Categories).SingleOrDefault(b => b.Id == model.Id);
			if (book is null)
				return NotFound();
			string imageId = null!;
			if (model.Image is not null)
			{
				if(!string.IsNullOrEmpty(book.ImageUrl))
				{
					var oldImagePath =$"{_webHostEnvironment.WebRootPath}{book.ImageUrl}";
					var oldThumbPath =$"{_webHostEnvironment.WebRootPath}{book.ImageThumbnailUrl}";
					if (System.IO.File.Exists(oldImagePath))
						System.IO.File.Delete(oldImagePath);

					if (System.IO.File.Exists(oldThumbPath))
						System.IO.File.Delete(oldThumbPath);


					//await _cloudinary.DeleteResourcesAsync(book.ImagePublicId);
				}
				var extension = Path.GetExtension(model.Image.FileName);
				if (!_allowedExtensions.Contains(extension))
				{
					ModelState.AddModelError(nameof(model.Image), Errors.NotAllowedExtension);
					return View("Form", PopulateViewModel(model));
				}
				if (model.Image.Length > _maxAllowedSize)
				{
					ModelState.AddModelError(nameof(model.Image), Errors.MaxSize);
					return View("Form", PopulateViewModel(model));
				}
				var imageName = $"{Guid.NewGuid()}{extension}";
				var path = Path.Combine($"{_webHostEnvironment.WebRootPath}/images/books", imageName);
				var thumbPath = Path.Combine($"{_webHostEnvironment.WebRootPath}/images/books/thumb", imageName);
				using var stream = System.IO.File.Create(path);
				await model.Image.CopyToAsync(stream);
				model.ImageUrl = $"/images/books/{imageName}";
				model.ImageThumbnailUrl = $"/images/books/thumb/{imageName}";

				using var image = Image.Load(model.Image.OpenReadStream());
				var ratio = (float)image.Width / 200;
				var height = image.Height / ratio;
				image.Mutate(i => i.Resize(width: 200, height: (int)height));
				image.Save(thumbPath);

				//using var stream = model.Image.OpenReadStream();
				//var Imageparams = new ImageUploadParams
				//{
				//	File= new FileDescription(imageName,stream),
				//	UseFilename=true
				//};
				//try {
				//	var result = await _cloudinary.UploadAsync(Imageparams);
				//	model.ImageUrl = result.SecureUrl.ToString();
				//	model.ImageThumbnailUrl = GetThmbnailUrl(model.ImageUrl);
				//	imageId = result.PublicId;
				//}
				//catch
				//{
				//	return BadRequest();
				//}
			}
			else if(!string.IsNullOrEmpty(book.ImageUrl))
			{
				model.ImageUrl=book.ImageUrl;
				model.ImageThumbnailUrl=book.ImageUrl;
			}
			book = _mapper.Map(model,book);
			book.LastUpdatedOn= DateTime.Now;
			book.ImagePublicId = imageId;	
		
			foreach (var category in model.SelectedCategories)
				book.Categories.Add(new BookCategory { CategoryId = category });

			_context.SaveChanges();
			return RedirectToAction(nameof(Details), new { id = book.Id });
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult ToggleStatus(int id)
		{
			var Book = _context.Books.Find(id);
			if (Book == null)
				return NotFound();
			Book.IsDeleted = !Book.IsDeleted;
			Book.LastUpdatedOn = DateTime.Now;
			_context.SaveChanges();
			return Ok();
		}
		public IActionResult UniqueName(BookFormViewModel model)
		{
			var book = _context.Books.SingleOrDefault(b => b.Title == model.Title && b.AuthorId==model.AuthorId);
			var isAllow = book is null || book.Id == model.Id;

			return Json(isAllow);
		}
		private BookFormViewModel PopulateViewModel(BookFormViewModel? model=null)
		{
			BookFormViewModel ViewModel= model is null ? new BookFormViewModel() : model;

			var authors = _context.Authors.Where(a => !a.IsDeleted).OrderBy(a => a.Name).ToList();
			var categories = _context.Categories.Where(a => !a.IsDeleted).OrderBy(a => a.Name).ToList();

			ViewModel.Authors = _mapper.Map<IEnumerable<SelectListItem>>(authors);
			ViewModel.Categories = _mapper.Map<IEnumerable<SelectListItem>>(categories);

			return ViewModel;
		}
		private string GetThmbnailUrl (string url)
		{
			var seperator = "image/upload/";
		    var urlParts = url.Split(seperator);
			return $"{urlParts[0]}{seperator}c_thumb,w_200,g_face/{urlParts[1]}";
		}
	}
}
