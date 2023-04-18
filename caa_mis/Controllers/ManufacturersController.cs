using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using caa_mis.Data;
using caa_mis.Models;
using caa_mis.Utilities;
using Microsoft.AspNetCore.Authorization;
using DNTBreadCrumb.Core;

namespace caa_mis.Controllers
{
    [Authorize(Roles = "Admin, Supervisor")]
    [BreadCrumb(Title = "Vendors", UseDefaultRouteUrl = true, Order = 0, IgnoreAjaxRequests = true)]
    public class ManufacturersController : CustomControllers.CognizantController
    {
        private readonly InventoryContext _context;

        public ManufacturersController(InventoryContext context)
        {
            _context = context;
        }

        // GET: Manufacturers
        public async Task<IActionResult> Index(string sortDirectionCheck, string sortFieldID, string SearchName, string SearchPhone, Archived? Status,
            int? page, int? pageSizeID, string actionButton, string sortDirection = "asc", string sortField = "Name")
        {
            //Clear the sort/filter/paging URL Cookie for Controller
            CookieHelper.CookieSet(HttpContext, ControllerName() + "URL", "", -1);

            //Change colour of the button when filtering by setting this default
            ViewData["Filtering"] = "btn-outline-primary";

            //List of sort options.
            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptions = new[] { "Name", "Address 1", "Address 2", "City", "Province", "Postal Code", "Phone", "Email", "Status" };
            
            var vendors = _context.Manufacturers.AsNoTracking();

            //Add as many filters as needed
            if (!String.IsNullOrEmpty(SearchName))
            {
                vendors = vendors.Where(p => p.Name.ToUpper().Contains(SearchName.ToUpper()));
                ViewData["Filtering"] = "btn-danger";
            }
            if (!String.IsNullOrEmpty(SearchPhone))
            {
                vendors = vendors.Where(p => p.Phone.ToUpper().Contains(SearchPhone.ToUpper()));
                ViewData["Filtering"] = "btn-danger";
            }
            if (Status != null)
            {
                vendors = vendors.Where(p => p.Status == Status);
                ViewData["Filtering"] = "btn-danger";
            }
            //Before we sort, see if we have called for a change of filtering or sorting
            if (!String.IsNullOrEmpty(actionButton)) //Form Submitted!
            {
                page = 1;//Reset page to start

                if (sortOptions.Contains(actionButton))//Change of sort is requested
                {
                    if (actionButton == sortField) //Reverse order on same field
                    {
                        sortDirection = sortDirection == "asc" ? "desc" : "asc";
                    }
                    sortField = actionButton;//Sort by the button clicked
                }
                else //Sort by the controls in the filter area
                {
                    sortDirection = String.IsNullOrEmpty(sortDirectionCheck) ? "asc" : "desc";
                    sortField = sortFieldID;
                }
            }
            //Now we know which field and direction to sort by
            if (sortField == "Name")
            {
                if (sortDirection == "asc")
                {
                    vendors = vendors
                        .OrderBy(p => p.Name);
                }
                else
                {
                    vendors = vendors
                        .OrderByDescending(p => p.Name);
                }
            }
            else if (sortField == "Address 1")
            {
                if (sortDirection == "asc")
                {
                    vendors = vendors
                        .OrderByDescending(p => p.Address1);
                }
                else
                {
                    vendors = vendors
                        .OrderBy(p => p.Address1);
                }
            }
            else if (sortField == "Address 2")
            {
                if (sortDirection == "asc")
                {
                    vendors = vendors
                        .OrderByDescending(p => p.Address2);
                }
                else
                {
                    vendors = vendors
                        .OrderBy(p => p.Address2);
                }
            }
            else if (sortField == "City")
            {
                if (sortDirection == "asc")
                {
                    vendors = vendors
                        .OrderByDescending(p => p.City);
                }
                else
                {
                    vendors = vendors
                        .OrderBy(p => p.City);
                }
            }
            else if (sortField == "Province")
            {
                if (sortDirection == "asc")
                {
                    vendors = vendors
                        .OrderByDescending(p => p.Province);
                }
                else
                {
                    vendors = vendors
                        .OrderBy(p => p.Province);
                }
            }
            else if (sortField == "Postal Code")
            {
                if (sortDirection == "asc")
                {
                    vendors = vendors
                        .OrderByDescending(p => p.PostalCode);
                }
                else
                {
                    vendors = vendors
                        .OrderBy(p => p.PostalCode);
                }
            }
            else if (sortField == "Phone")
            {
                if (sortDirection == "asc")
                {
                    vendors = vendors
                        .OrderByDescending(p => p.Phone);
                }
                else
                {
                    vendors = vendors
                        .OrderBy(p => p.Phone);
                }
            }
            else if (sortField == "Email")
            {
                if (sortDirection == "asc")
                {
                    vendors = vendors
                        .OrderByDescending(p => p.Email);
                }
                else
                {
                    vendors = vendors
                        .OrderBy(p => p.Email);
                }
            }
            else if (sortField == "Status")
            {
                if (sortDirection == "asc")
                {
                    vendors = vendors
                        .OrderBy(p => p.Status);
                }
                else
                {
                    vendors = vendors
                        .OrderByDescending(p => p.Status);
                }
            }
            else //Sorting by Name
            {
                if (sortDirection == "asc")
                {
                    vendors = vendors
                        .OrderBy(p => p.Name);
                }
                else
                {
                    vendors = vendors
                        .OrderByDescending(p => p.Name);
                }
            }

            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;
            //SelectList for Sorting Options
            ViewBag.sortFieldID = new SelectList(sortOptions, sortField.ToString());

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, "Items");
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<Manufacturer>.CreateAsync(vendors.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: Manufacturers/Details/5
        [BreadCrumb(Title = "Details", Order = 1, IgnoreAjaxRequests = true)]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Manufacturers == null)
            {
                return NotFound();
            }

            var manufacturer = await _context.Manufacturers
                .FirstOrDefaultAsync(m => m.ID == id);
            if (manufacturer == null)
            {
                return NotFound();
            }

            return View(manufacturer);
        }

        // GET: Manufacturers/Create
        [BreadCrumb(Title = "Create", Order = 1, IgnoreAjaxRequests = true)]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Manufacturers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,Address1,Address2,City,Province,PostalCode,Phone,Email")] Manufacturer manufacturer)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(manufacturer);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch
            {
                ModelState.AddModelError("", "Saving Failed. Please try again or contact your System Administrator.");
            }
            return View(manufacturer);
        }

        // GET: Manufacturers/Edit/5
        [BreadCrumb(Title = "Edit", Order = 1, IgnoreAjaxRequests = true)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Manufacturers == null)
            {
                return NotFound();
            }

            var manufacturer = await _context.Manufacturers.FindAsync(id);
            if (manufacturer == null)
            {
                return NotFound();
            }
            return View(manufacturer);
        }

        // POST: Manufacturers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,Address1,Address2,City,Province,PostalCode,Phone,Email,Status")] Manufacturer manufacturer)
        {
            try
            {
                if (id != manufacturer.ID)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(manufacturer);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!ManufacturerExists(manufacturer.ID))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    return RedirectToAction(nameof(Index));
                }
            }
            catch
            {
                ModelState.AddModelError("", "Saving Failed. Please try again or contact your System Administrator.");
            }

            return View(manufacturer);
        }

        // GET: Manufacturers/Archive/5
        [BreadCrumb(Title = "Archive", Order = 1, IgnoreAjaxRequests = true)]
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null || _context.Manufacturers == null)
            {
                return NotFound();
            }

            var manufacturer = await _context.Manufacturers
                .FirstOrDefaultAsync(m => m.ID == id);
            if (manufacturer == null)
            {
                return NotFound();
            }

            return View(manufacturer);
        }

        // POST: Manufacturers/Archive/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            try
            {
                if (_context.Manufacturers == null)
                {
                    return Problem("Entity set 'InventoryContext.Manufacturers' is null.");
                }
                var manufacturer = await _context.Manufacturers.FindAsync(id);

                if (manufacturer != null)
                {
                    manufacturer.Status = Archived.Disabled;
                }

                await _context.SaveChangesAsync();
            }
            catch
            {
                ModelState.AddModelError("", "Saving Failed. Please try again or contact your System Administrator.");
            }
            
            return RedirectToAction(nameof(Index));
        }

        private bool ManufacturerExists(int id)
        {
          return _context.Manufacturers.Any(e => e.ID == id);
        }
    }
}
