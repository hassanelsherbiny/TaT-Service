﻿using PowerStore.Web.Models.Blogs;
using MediatR;

namespace PowerStore.Web.Features.Models.Blogs
{
    public class GetHomePageBlog: IRequest<HomePageBlogItemsModel>
    {
    }
}
