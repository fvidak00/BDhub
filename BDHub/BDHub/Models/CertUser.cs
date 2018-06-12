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
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Numerics;

    public partial class CertUser
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public CertUser()
        {
            this.Videos = new HashSet<Video>();
        }
    
        public int certUserID { get; set; }

        [DisplayName("Username")]
        [Required(ErrorMessage = "This field is required")]
        public string username { get; set; }

        [DisplayName("First name")]
        public string firstName { get; set; }

        [DisplayName("Last name")]
        public string lastName { get; set; }

        [Required(ErrorMessage = "This field is required")]
        public string password { get; set; }

        [DisplayName("E-mail")]
        public string email { get; set; }

        [DisplayName("Bethernum Account Address")]
        public string beternumAddress { get; set; }

        public string LoginErrorMessage { get; set; }

        [DisplayName("BD Balance")]
        [DisplayFormat(DataFormatString = "{0:0.00000000000000000000}")]
        public decimal balance { get; set; }

        [DisplayName("Password for BDoken Account")]
        public string bdokenPass{ get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Video> Videos { get; set; }
    }
}
