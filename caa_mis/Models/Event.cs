using System.ComponentModel.DataAnnotations;

namespace caa_mis.Models
{
    public class Event
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "You cannot leave the Event Name blank.")]
        [StringLength(100, ErrorMessage = " Event Name cannot be more than 100 characters long.")]
        public string Name { get; set; }

        [Required]
        public int EmployeeID { get; set; }
        public Employee Employee { get; set; }

        [Required]
        public int TransactionStatusID { get; set; }
        public TransactionStatus TransactionStatus { get; set; }

        [Required]
        public int BranchID { get; set; }
        public Branch Branch { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? DateReturned { get; set; }


        public ICollection<EventItem> EventItems { get; set; } = new HashSet<EventItem>();
    }
}
