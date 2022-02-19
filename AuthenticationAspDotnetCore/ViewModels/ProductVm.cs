using System.Collections.Generic;
using AuthenticationAspDotnetCore.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AuthenticationAspDotnetCore.ViewModels
{
    public class ProductVm
    {
        public Product Product { get; set; }
        public IEnumerable<SelectListItem> CategoriesList { get; set; }
    }
}