using DataLayer;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EPS.Models
{
    public class NewLibraryItemModel : LibraryItem
    {
        [Required(ErrorMessage = "An item name is required.")]
        public new String ItemName { get; set; }

        [Required(ErrorMessage = "An item description is required.")]
        public new String ItemDesc { get; set; }

        [Required(ErrorMessage = "A library file (dll) is required.")]
        public HttpPostedFileBase LibraryPathFile { get; set; }
    }
}