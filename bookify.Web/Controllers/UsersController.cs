using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace bookify.Web.Controllers
{
	[Authorize(Roles=AppRoles.Admin)]
	public class UsersController : Controller
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly IMapper _mapper;

		public UsersController(UserManager<ApplicationUser> userManager, IMapper mapper, RoleManager<IdentityRole> roleManager)
		{
			_userManager = userManager;
			_mapper = mapper;
			_roleManager = roleManager;
		}

		public async Task<IActionResult> Index()
		{
			var users= await _userManager.Users.ToListAsync();
			var viewModel=_mapper.Map<IEnumerable<UserViewModel>>(users);
			return View(viewModel);
		}
		[AjaxOnly]
		public async Task<IActionResult> Create()
		{
			var viewModel = new UserFormViewModel
			{
				Roles = await _roleManager.Roles.
				Select(r => new SelectListItem 
				{ 
					Text = r.Name,
					Value = r.Name 
				}).ToListAsync()
			};
			return PartialView("_Form", viewModel);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(UserFormViewModel model)
		{
			if (!ModelState.IsValid)
				return BadRequest();
			ApplicationUser user = new()
			{
				FullName=model.FullName,
				UserName = model.UserName,
				Email = model.Email,
				CreatedById=User.FindFirst(ClaimTypes.NameIdentifier)!.Value
			};
			var result =  await _userManager.CreateAsync(user,model.Password);
			if (result.Succeeded) 
			{
				await _userManager.AddToRolesAsync(user, model.SelectedRoles);
				return PartialView("_UserRow",_mapper.Map<UserViewModel>(user));
			}
			return BadRequest(string.Join(',',result.Errors.Select(e=>e.Description)));
		}
		[HttpGet]
		[AjaxOnly]
		public async Task<IActionResult> Edit(string id)
		{
			var user = await _userManager.FindByIdAsync(id);
			if (user is null)
				return NotFound();
			var viewModel = _mapper.Map<UserFormViewModel>(user);
			viewModel.SelectedRoles = await _userManager.GetRolesAsync(user);
				viewModel.Roles = await _roleManager.Roles.Select(r => new SelectListItem
				{
					Text = r.Name,
					Value = r.Name
				}).ToListAsync();
			return PartialView("_Form", viewModel);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(UserFormViewModel model)
		{
			if (!ModelState.IsValid)
				return BadRequest();
			var user = await _userManager.FindByIdAsync(model.Id);
			if (user is null)
				return NotFound();

			user = _mapper.Map(model,user);
			user.LastUpdatedOn = DateTime.Now;
			user.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
			var result = await _userManager.UpdateAsync(user);
			if(result.Succeeded)
			{
				var CurrentRoles = await _userManager.GetRolesAsync(user);
				var roledupdated = !CurrentRoles.SequenceEqual(model.SelectedRoles);
				if (roledupdated)
				{
					await _userManager.RemoveFromRolesAsync(user, CurrentRoles);
					await _userManager.AddToRolesAsync(user, model.SelectedRoles);
				}
				return PartialView("_UserRow", _mapper.Map<UserViewModel>(user));
			}	
			return BadRequest(string.Join(',', result.Errors.Select(e => e.Description)));			
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ToggleStatus(string id)
		{
			var user = await _userManager.FindByIdAsync(id);
			if (user is null)
				return NotFound();
			user.IsDeleted = !user.IsDeleted;
			user.LastUpdatedOn = DateTime.Now;
			user.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
			await _userManager.UpdateAsync(user);

			return Ok(user.LastUpdatedOn.ToString());
		}
		[AjaxOnly]
		public async Task<IActionResult> ResetPassword(string id)
		{
			var user = await _userManager.FindByIdAsync(id);
			if (user is null)
				return NotFound();
			ResetPasswordFormViewModel model = new() { Id = user.Id };
			return PartialView("_ResetPasswordForm", model);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ResetPassword(ResetPasswordFormViewModel model)
		{
			if (!ModelState.IsValid)
				return BadRequest();
			var user = await _userManager.FindByIdAsync(model.Id);
			if (user is null)
				return NotFound();

			var currentPasswordHash = user.PasswordHash;
			await _userManager.RemovePasswordAsync(user);
	         var result =  await _userManager.AddPasswordAsync(user,model.NewPassword);
			if (result.Succeeded)
			{
				user.LastUpdatedOn=DateTime.Now;
				user.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
				await _userManager.UpdateAsync(user);
				return PartialView("_UserRow", _mapper.Map<UserViewModel>(user));
			}
			user.PasswordHash=currentPasswordHash;
			await _userManager.UpdateAsync(user);
			return BadRequest(string.Join(',', result.Errors.Select(e => e.Description)));
		}
		public async Task<IActionResult> AllowUserName(UserFormViewModel model)
		{
			var user = await _userManager.FindByNameAsync(model.UserName);
			bool isAllaw = user is null || user.Id.Equals(model.Id);
			return Json(isAllaw);
		}
		public async Task<IActionResult> AllowEmail(UserFormViewModel model)
		{
			var user = await _userManager.FindByEmailAsync(model.Email);
			bool isAllaw = user is null || user.Id.Equals(model.Id);
			return Json(isAllaw);
		}
		//public async Task<IActionResult> CheckCurrentPass(ResetPasswordFormViewModel model)
		//{
		//	var user = await _userManager.FindByIdAsync(model.Id);
		//	bool isAllaw = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);
		//	return Json(isAllaw);
		//}
	}
}
