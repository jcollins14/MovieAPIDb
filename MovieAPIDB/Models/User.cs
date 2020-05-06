using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; //Needed for ConfirmPassword
using System.Linq;
using System.Threading.Tasks;

namespace MovieAPIDB.Models
{
    public class User
    {
        public int ID { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [NotMapped] // Does not effect with your database
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Confirm password does not match")]
        public string ConfirmPassword { get; set; }
    }
}
