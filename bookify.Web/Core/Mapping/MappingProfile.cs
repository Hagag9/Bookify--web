﻿

namespace bookify.Web.Core.Mapping
{
    public class MappingProfile:Profile
    {
        public MappingProfile()
        {
            //category
            CreateMap<Category, CategoryViewModel>();
			CreateMap<CategoryFormViewModel, Category>().ReverseMap();

            //Authors
            CreateMap<Author, AuthorViewModel>();
			CreateMap<AuthorFormViewModel, Author>().ReverseMap();

		}
    }
}
