// Models/ViewModels/AdminProductViewModel.cs
using System.Collections.Generic;
using MiniE_TicaretPaneli.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MiniE_TicaretPaneli.Models.ViewModels
{
    public class AdminProductViewModel
    {
        public Product Product { get; set; } = new Product();

        public List<Category> GenderCategories { get; set; } = new List<Category>();
        public List<Category> MainCategories { get; set; } = new List<Category>();
        public List<Category> SubCategories { get; set; } = new List<Category>();

        public List<string> AllSizes { get; set; } = new List<string>();
        public List<string> AllColors { get; set; } = new List<string>();

        public IFormFile? ProductImage { get; set; }
        public string? ExistingImageUrl { get; set; }

        public List<string> SelectedSizes { get; set; } = new List<string>();
        public List<string> SelectedColors { get; set; } = new List<string>();
    }
}