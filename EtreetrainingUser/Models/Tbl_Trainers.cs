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

    public partial class Tbl_Trainers
    {
        public int Trainr_id { get; set; }
        public string Trainr_Name { get; set; }
        public string Trainr_EmailId { get; set; }
        public string Trainr_MNum { get; set; }
        public string Trainr_ImagePath { get; set; }
        public string Trainr_DocTitle { get; set; }
        public string Trainr_DocPath { get; set; }
        public string Trainr_Pass { get; set; }
        public string Trainr_RePass { get; set; }
        public string Trainr_ResetPassCode { get; set; }
        public Nullable<bool> Trainr_IsEmailVerified { get; set; }
        public Nullable<System.Guid> Trainr_ActivationCode { get; set; }
        public Nullable<System.Guid> Trainr_ReferenceCode { get; set; }
    }
}
