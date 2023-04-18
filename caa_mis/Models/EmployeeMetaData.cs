
using caa_mis.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.Security.AccessControl;

namespace caa_mis.Models
{
    public class EmployeeMetaData
    {
        public int ID { get; set; }

        [Display(Name = "Employee")]
        public string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }

        public string FormalName
        {
            get
            {
                return LastName + ", " + FirstName;
            }
        }

        [Display(Name = "Phone")]
        public string PhoneNumber
        {
            get
            {
                if (String.IsNullOrEmpty(Phone))
                {
                    return "";
                }
                else
                {
                    return "(" + Phone.Substring(0, 3) + ") " + Phone.Substring(3, 3) + "-" + Phone.Substring(6, 4);
                }
            }
        }

        [Display(Name = "First Name")]
        [Required(ErrorMessage = "You cannot leave the first name blank.")]
        [StringLength(50, ErrorMessage = "First name cannot be more than 50 characters long.")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Required(ErrorMessage = "You cannot leave the last name blank.")]
        [StringLength(50, ErrorMessage = "Last name cannot be more than 50 characters long.")]
        public string LastName { get; set; }

        [RegularExpression("^\\d{10}$", ErrorMessage = "Please enter a valid 10-digit phone number (no spaces).")]
        [DataType(DataType.PhoneNumber)]
        [StringLength(10)]
        public string Phone { get; set; }
        
        [Display(Name = "Branch")]
        public BranchRoles BranchRoles { get; set; } = BranchRoles.None;//Not needed

        [Required(ErrorMessage = "Email Address is required.")]
        [StringLength(255)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        public bool Active { get; set; } = true;

        public ICollection<Bulk> Bulks { get; set; } = new HashSet<Bulk>();
        public ICollection<Event> Events { get; set; } = new HashSet<Event>();
        public ICollection<Transaction> Transactions { get; set; } = new HashSet<Transaction>();
        public ICollection<Subscription> Subscriptions { get; set; }
    }
}
