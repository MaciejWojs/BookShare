using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BookShare.Models.ViewModels {
    public class CategoryManagementViewModel {
        public Category NewCategory { get; set; } = new Category(); // model formularza
        public IEnumerable<Category> Categories { get; set; } = new List<Category>(); // lista kategorii
    }
}