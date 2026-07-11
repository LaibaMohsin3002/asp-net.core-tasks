using System.ComponentModel.DataAnnotations;

namespace my_books.Data.ViewModels
{
    public class TokenRequestVM
    {
        [Required]
        public string Token { get; set; }

        [Required]
        public string RefreshToken { get; set; }
    }
}