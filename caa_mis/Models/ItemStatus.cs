using System.ComponentModel.DataAnnotations;

namespace caa_mis.Models
{
    public class ItemStatus
    {
        public int ID { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        public Archived Status { get; set; }

        [StringLength(255)]
        public string Description { get; set; }
        public ICollection<Item> Items { get; set; } = new HashSet<Item>();
    }
}
