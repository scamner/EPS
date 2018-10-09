using DataLayer;
using System;
using System.ComponentModel.DataAnnotations;

namespace EPS.Models
{
    public class ADUser : User
    {
        public string ADGUID { get; set; }
        public Boolean Exists { get; set; }

        [Required(ErrorMessage = "At least part of a first name is required.")]
        public new String FirstName { get; set; }

        [Required(ErrorMessage = "At least part of a last name is required.")]
        public new String LastName { get; set; }

        public Boolean ForUser { get; set; }
    }
}