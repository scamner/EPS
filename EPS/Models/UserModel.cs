using DataLayer;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EPS.Models
{
    public class UserModel : User
    {
        [Required(ErrorMessage = "You must enter your username.")]
        public new String Username { get; set; }

        [Required(ErrorMessage = "A password is required.")]
        public String Password { get; set; }
    }
}