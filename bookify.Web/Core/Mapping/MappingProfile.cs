using AutoMapper;

namespace bookify.Web.Core.Mapping
{
    public class MappingProfile:Profile
    {
        public MappingProfile()
        {
            //category
            CreateMap<Category, CategoryViewModel>();
			CreateMap<CategoryFormViewModel, Category>().ReverseMap();
		}
    }
}
