using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace caa_mis.Models
{
    public class Branch
    {
        public int ID { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }        

        [Required]
        [StringLength(255)]
        public string Address { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "City")]
        public string Location { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        public Archived Status { get; set; }

        public ICollection<Bulk> Bulks { get; set; } = new HashSet<Bulk>();
        public ICollection<Event> Events { get; set; } = new HashSet<Event>();
        [JsonIgnore]
        public ICollection<Stock> Stocks { get; set; } = new HashSet<Stock>();
        public ICollection<Transaction> Origins { get; set; } = new HashSet<Transaction>();
        public ICollection<Transaction> Destinations { get; set; } = new HashSet<Transaction>();
    }
}
