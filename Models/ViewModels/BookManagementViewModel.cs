using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShare.Models.ViewModels {
    public class BookManagementViewModel {
        public Book NewBook { get; set; } = new Book(); // model formularza
        public IEnumerable<Book> Books { get; set; } = new List<Book>(); // lista książek

    }
}