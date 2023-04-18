using System.ComponentModel.DataAnnotations;
namespace caa_mis.Models
{
    public class Supplier
    {
        public int ID { get; set; }

        [Required]
        [StringLength(255)]
        public string SupplierName { get; set; }

        [Required]
        [StringLength(255)]
        public string Address1 { get; set; }

        [StringLength(255)]
        public string Address2 { get; set; }

        [Required]
        [StringLength(255)]
        public string City { get; set; }

        [Required]
        [StringLength(255)]
        public string Province { get; set; }

        [Required]
        [StringLength(6)]
        public string PostalCode { get; set; }

        [Required]
        [StringLength(20)]
        public string Phone { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public Archived Status { get; set; }

        public ICollection<ItemSupplier> ItemSuppliers { get; set; } = new HashSet<ItemSupplier>();

    }
}
