using System.ComponentModel.DataAnnotations;

namespace caa_mis.Models
{
    public class TransactionType
    {
        public int ID { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [StringLength(255)]
        public string Description { get; set; }

        [Display(Name = "In/Out")]
        [Required]
        public InOut InOut { get; set; }

        public Archived Status { get; set; }

        public ICollection<Transaction> Transactions { get; set; } = new HashSet<Transaction>();
    }
}