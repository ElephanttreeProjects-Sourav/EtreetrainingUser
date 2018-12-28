//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EtreetrainingUser.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Web;

    public partial class Tbl_ADMN
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please provide Admin Id")]
        [Display(Name = "Admin ID")]
        [DataType(DataType.Password)]
        public int Admin_id { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please provide Admin Name")]
        [Display(Name = "Admin Name")]
        public string AdminName { get; set; }
        [Required(ErrorMessage = "Please provide email")]
        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                           @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                           @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$",
                           ErrorMessage = "Email is not valid")]
        [Display(Name = "Email ID")]
        public string Admin_EmailId { get; set; }
        [DataType(DataType.PhoneNumber)]
        [MinLength(10, ErrorMessage = "Phone number should be minimim 10 digits")]
        [MaxLength(10, ErrorMessage = "Phone number should be maximum 10 digits")]
        //[Required(AllowEmptyStrings = false, ErrorMessage = "Please provide Admin Number")]
        [Display(Name = "Admin PH.Number")]
        public string Admin_MNum { get; set; }

        public string admin_PicTitle { get; set; }
        public string Admin_ImagePath { get; set; }
        public string admin_DocTitle { get; set; }
        public string Admin_DocPath { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please provide Password")]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password must be minimum 6 characters")]
        [RegularExpression("(?!^[0-9]*$)(?!^[a-zA-Z]*$)^([a-zA-Z0-9]{8,16})$", ErrorMessage = "Password must be between 8 to 16 characters,contain at least one digit and one alphabetic character and must not contain special characters")]

        [System.Web.Mvc.AllowHtml]
        public string Admin_Pass { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please provide re-password")]
        [Display(Name = "Admin Confirm password")]
        [DataType(DataType.Password)]
        [Compare("Admin_Pass", ErrorMessage = "Password and confirm password do not match")]
        [System.Web.Mvc.AllowHtml]
        public string Admin_RePass { get; set; }

        public string Admin_ResetPassCode { get; set; }


        public Nullable<bool> IsEmailVerified { get; set; }


        public Nullable<System.Guid> ActivationCode { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please provide image")]
        public HttpPostedFileBase image { get; set; }

        public HttpPostedFileBase DocFile { get; set; }


        [Display(Name = "Remember me")]
        public bool rememberMe { get; set; }



    }
}

