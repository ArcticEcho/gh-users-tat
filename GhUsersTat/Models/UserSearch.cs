using System.ComponentModel.DataAnnotations;

using Newtonsoft.Json;

namespace GhUsersTat.Models
{
    public class UserSearch
    {
        [Required(ErrorMessage = "Please enter a username")]
        [StringLength(
            maximumLength: 39,
            MinimumLength = 1,
            ErrorMessage = "Please enter a username less than 40 characters")]
        [RegularExpression(
            @"^[A-Za-z0-9-]*$",
            ErrorMessage = "Usernames can only contain alphanumeric characters or hyphens")]
        [Display(Name = "Search for a GitHub user")]
        public string Username { get; set; }
    }
}