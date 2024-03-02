

namespace bookify.Web.Core.Mapping
{
    public class MappingProfile:Profile
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
				ForMember(dest=>dest.Categories,opt=>opt.MapFrom(src=>src.Categories.Select(c=>c.Category!.Name).ToList()));
			
		}
    }
}
