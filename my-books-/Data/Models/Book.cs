using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Web;

namespace my_books.Data.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsRead { get; set; }
        public DateTime? DateRead { get; set; }
        public int? rate { get; set; }
        public string CoverUrl { get; set; }

        public DateTime DateAdded { get; set; }
        public string Genre { get; set; }

        // Foreign key
        public int? PublisherId { get; set; }

        // Navigation property - each book belongs to one publisher
        public Publisher? Publisher { get; set; }

        public List<Book_Author>? Book_Authors { get; set; }
    }
}