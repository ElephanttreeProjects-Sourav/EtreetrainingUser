using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EtreetrainingUser.Models
{
    public class Cand_login
    {
   
            [Required(AllowEmptyStrings = false, ErrorMessage = "please provide Admin Id")]
            [Display(Name = "Admin id")]
            [DataType(DataType.Password)]
            public int Admin_id { get; set; }


            [DataType(DataType.EmailAddress)]
            [Required(AllowEmptyStrings = false, ErrorMessage = "please provide Email Id")]
            [Display(Name = "Email id")]
            public string Admin_EmailId { get; set; }


            [Required(AllowEmptyStrings = false, ErrorMessage = "please provide a valid Password")]
            [Display(Name = "Admin Password")]
            [DataType(DataType.Password)]
            [MinLength(6, ErrorMessage = "Minimum 6 characters required")]
            public string Admin_Pass { get; set; }

            [Display(Name = "Remember me")]
            public bool rememberMe { get; set; }


        }
    }

