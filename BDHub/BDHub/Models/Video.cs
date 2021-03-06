//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BDHub.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class Video
    {
        public int videoID { get; set; }
        public string title { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0,00}")]
        public decimal price { get; set; }
        public Nullable<int> viewsCount { get; set; }
        public string filepath { get; set; }
        public string about { get; set; }
        public int userID { get; set; }
    
        public virtual CertUser CertUser { get; set; }
    }
}
