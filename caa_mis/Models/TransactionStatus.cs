using System.ComponentModel.DataAnnotations;
namespace caa_mis.Models
{
    public class TransactionStatus
    {
        public int ID { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [StringLength(255)]
        public string Description { get; set; }

        public Archived Status { get; set; }

        public ICollection<Bulk> Bulks { get; set; } = new HashSet<Bulk>();

        public ICollection<Event> Events { get; set; } = new HashSet<Event>();
        public ICollection<Transaction> Transactions { get; set; } = new HashSet<Transaction>();
    }
}
