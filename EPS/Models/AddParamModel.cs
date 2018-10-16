using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace EPS.Models
{
    public class AddParamModel : DataLayer.Parameter
    {
        [Required(ErrorMessage = "A parameter name is required.")]
        public new String ParamName { get; set; }

        [Required(ErrorMessage = "A parameter description is required.")]
        public new String ParamDesc { get; set; }

        [Required(ErrorMessage = "A parameter value is required.")]
        public new String ParamValue { get; set; }
    }
}