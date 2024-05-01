
using Microsoft.AspNetCore.Identity.UI.Services;


namespace bookify.Web.Tasks
{
	public class HangfireTasks
	{
		private readonly ApplicationDbContext _context;
		private readonly IWebHostEnvironment _webHostEnvironment;
		private readonly IWhatsAppClient _whatsAppClient;
		private readonly IEmailBodyBuilder _emailBodyBuilder;
		private readonly IEmailSender _emailSender;

		public HangfireTasks(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment,  IWhatsAppClient whatsAppClient,  IEmailBodyBuilder emailBodyBuilder, IEmailSender emailSender)
		{
			_context = context;
			_webHostEnvironment = webHostEnvironment;
			_whatsAppClient = whatsAppClient;
			_emailBodyBuilder = emailBodyBuilder;
			_emailSender = emailSender;
		}
		public async Task PrepareExpirationAlert()
		{
			var subscribers = _context.Subscribers.Include(s => s.Subscriptions)
				.Where(s => !s.IsBlackListed && s.Subscriptions.OrderByDescending(x => x.EndDate).First().EndDate == DateTime.Today.AddDays(5)).ToList();

			foreach (var subscriber in subscribers)
			{
				var endDate = subscriber.Subscriptions.Last().EndDate.ToString("d MMM, yyyy");
				var placeHolders = new Dictionary<string, string>()
				{
					{"imageUrl","https://res.cloudinary.com/ahagag/image/upload/v1713545232/calendar_zfohjc_zbrzjr.png"},
					{"header", $"Hello {subscriber.FirstName},"},
					{"body" , $"Your subscription will be expired by {endDate} 🙁"},
				};
				var body = _emailBodyBuilder.GetEmailBody(EmailTemplates.Notification, placeHolders);
				await _emailSender.SendEmailAsync(subscriber.Email, "Bookify Subscription Expiration", body);

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
								new WhatsAppTextParameter { Text = endDate}
							}
						}
					};
					var mobileNumber = _webHostEnvironment.IsDevelopment() ? "01227232423" : subscriber.MobileNumber;
					await _whatsAppClient.SendMessage($"2{mobileNumber}", WhatsAppLanguageCode.English, WhatsAppTemplates.SubscriptionExpiration, components);
				}
			}
		}

		public async Task RentalsExpirationAlert()
		{
			var tomorrow = DateTime.Today.AddDays(1);

			var rentals = _context.Rentals.Include(r => r.Subscriber)
				.Include(r => r.RentalCopies)
				.ThenInclude(c => c.BookCopy)
				.ThenInclude(bc => bc!.Book)
				.Where(r => r.RentalCopies.Any(r => r.EndDate.Date == tomorrow && !r.ReturnDate.HasValue))
				.ToList();

			foreach (var rental in rentals)
			{
				var expiredCopies = rental.RentalCopies.Where( c=>c.EndDate.Date == tomorrow).ToList();

				var message = $"Your rental for the below book(s) will be expired by tomorrow {tomorrow.ToString("dd MMM,yyyy")} 💔 :";
				message += "<ul>";
				foreach (var copy in expiredCopies)
				{
					message += $"<li>{copy.BookCopy!.Book!.Title}</li>";
				}
				message += "</ul>";
				var placeHolders = new Dictionary<string, string>()
				{
					{"imageUrl","https://res.cloudinary.com/ahagag/image/upload/v1713545232/calendar_zfohjc_zbrzjr.png"},
					{"header", $"Hello {rental.Subscriber!.FirstName},"},
					{"body" ,message}
				};
				var body = _emailBodyBuilder.GetEmailBody(EmailTemplates.Notification, placeHolders);
				await _emailSender.SendEmailAsync(rental.Subscriber.Email, "Bookify Rental Expiration 🔔", body);
			}
		}
	}
}
