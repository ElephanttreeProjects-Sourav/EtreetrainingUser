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

    public partial class User
    {
        public int id { get; set; }
        public string Name { get; set; }
        public string EmailId { get; set; }
        public string Number { get; set; }
        public string Photo_Title { get; set; }
        public string Image_Path { get; set; }
        public string Document_Title { get; set; }
        public string Document_Path { get; set; }
        public string Certificate { get; set; }
        public string Password { get; set; }
        public string Retype_Pass { get; set; }
        public string Reset_PassCode { get; set; }
        public Nullable<bool> IsEmailVerified { get; set; }
        public Nullable<System.Guid> ActivationCode { get; set; }
        public Nullable<System.Guid> ReferenceCode { get; set; }
    }
}


