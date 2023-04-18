using caa_mis.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace caa_mis.ViewModels
{
    /// <summary>
    /// Add back in any Restricted Properties and list of UserRoles
    /// </summary>
    [ModelMetadataType(typeof(EmployeeMetaData))]
    public class EmployeeAdminVM : EmployeeVM
    {
        public string Email { get; set; }
        public bool Prescriber { get; set; }
        public BranchRoles BranchRoles { get; set; }
        public bool Active { get; set; }

        [Display(Name = "Roles")]
        public List<string> UserRoles { get; set; } = new List<string>();

        
    }
}
