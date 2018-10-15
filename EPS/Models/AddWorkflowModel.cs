using DataLayer;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EPS.Models
{
    public class AddWorkflowModel : Workflow
    {
        public AddWorkflowModel()
        {
            items = new List<LibraryItem>();
        }

        [Required(ErrorMessage = "You must enter a workflow name.")]
        public new String WorkflowName { get; set; }

        [Required(ErrorMessage = "You must enter a workflow description.")]
        public new String WorkflowDesc { get; set; }

        public List<LibraryItem> items { get; set; }
    }
}