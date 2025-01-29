using System.ComponentModel.DataAnnotations;

using Newtonsoft.Json;

namespace GhUsersTat.Models
{
    public class UserSearch
    {
        [Required(ErrorMessage = "Please enter a username")]
        [StringLength(
            maximumLength: 50,
            MinimumLength = 3,
            ErrorMessage = "Please enter a username with at least 3 characters")]
        [Display(Name = "Search for a GitHub user")]
        public string Username { get; set; }
    }
}