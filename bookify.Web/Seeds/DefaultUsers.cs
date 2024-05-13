namespace bookify.Web.Seeds
{
    public class DefaultUsers
    {
        public static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
        {
            ApplicationUser admin = new()
            {
                UserName = "admin",
                Email = "admin@booify.com",
                FullName = "admin",
                EmailConfirmed = true,
            };
            var user = await userManager.FindByEmailAsync(admin.Email);
            if (user == null)
            {
                await userManager.CreateAsync(admin, "P@ssord123");
                await userManager.AddToRoleAsync(admin, AppRoles.Admin);
            }
        }
    }
}
