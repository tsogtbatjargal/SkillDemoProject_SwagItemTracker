using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace caa_mis.Models
{
    public enum BranchRoles
    {
        None,
        [Display(Name = "Welland")]
        Welland,
        [Display(Name = "Niagara Falls")]
        NiagaraFalls,
        [Display(Name = "Grimsby")]
        Grimsby,
        [Display(Name = "Thorold")]
        Thorold,
        [Display(Name = "St. Catharines")]
        StCatharines,
    }
}
