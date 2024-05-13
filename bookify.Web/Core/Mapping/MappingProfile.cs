

namespace bookify.Web.Core.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //categories
            CreateMap<Category, CategoryViewModel>();
            CreateMap<CategoryFormViewModel, Category>().ReverseMap();
            CreateMap<Category, SelectListItem>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Name));


            //Authors
            CreateMap<Author, AuthorViewModel>();
            CreateMap<AuthorFormViewModel, Author>().ReverseMap();
            CreateMap<Author, SelectListItem>()
                            .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Id))
                            .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Name));

            //Books
            CreateMap<BookFormViewModel, Book>().ReverseMap()
                .ForMember(dest => dest.Categories, opt => opt.Ignore());
            CreateMap<Book, BookViewModel>().ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.Author!.Name)).
                ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.Categories.Select(c => c.Category!.Name).ToList()));

            //BookCopy
            CreateMap<BookCopy, BookCopyViewModel>()
                .ForMember(dest => dest.BookTitle, opt => opt.MapFrom(src => src.Book!.Title))
                .ForMember(dest => dest.BookId, opt => opt.MapFrom(src => src.Book!.Id))
                .ForMember(dest => dest.BookThumbnailUrl, opt => opt.MapFrom(src => src.Book!.ImageThumbnailUrl));
            CreateMap<BookCopy, BookCopyFormViewModel>().ReverseMap();

            //Users
            CreateMap<ApplicationUser, UserViewModel>();
            CreateMap<UserFormViewModel, ApplicationUser>().ForMember(dest => dest.NormalizedEmail, opt => opt.MapFrom(src => src.Email.ToUpper()))
                .ForMember(dest => dest.NormalizedUserName, opt => opt.MapFrom(src => src.UserName.ToUpper())).ReverseMap();

            //Area
            CreateMap<Area, SelectListItem>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Name));
            //Governorate
            CreateMap<Governorate, SelectListItem>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Name));

            //subscriper
            CreateMap<SubscriberFormViewModel, Subscriber>().ReverseMap();
            CreateMap<Subscriber, SubscriberViewModel>()
                .ForMember(dest => dest.Governorate, opt => opt.MapFrom(src => src.Governorate!.Name))
                .ForMember(dest => dest.Area, opt => opt.MapFrom(src => src.Area!.Name))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));
            CreateMap<Subscriber, SubscriberSearchResultViewModel>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));

            //subscribtions
            CreateMap<Subscription, SubscriptionViewModel>();

            //Rental
            CreateMap<Rental, RentalViewModel>();
            CreateMap<RentalCopy, RentalCopyViewModel>();
            CreateMap<RentalCopy, CopyHistoryViewModel>()
                .ForMember(dest => dest.SubscriberMobile, opt => opt.MapFrom(src => src.Rental!.Subscriber!.MobileNumber))
                .ForMember(dest => dest.SubscriberName, opt => opt.MapFrom(src => $"{src.Rental!.Subscriber!.FirstName} {src.Rental!.Subscriber!.LastName}"));
        }
    }
}
