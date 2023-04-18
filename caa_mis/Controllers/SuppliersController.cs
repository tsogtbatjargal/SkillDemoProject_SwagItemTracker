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

namespace caa_mis.Controllers
{
    [Authorize(Roles = "Admin, Supervisor")]
    public class SuppliersController : CustomControllers.CognizantController
    {
        private readonly InventoryContext _context;

        public SuppliersController(InventoryContext context)
        {
            _context = context;
        }

        // GET: Suppliers
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

            var vendors = _context.Suppliers.AsNoTracking();

            //Add as many filters as needed
            if (!String.IsNullOrEmpty(SearchName))
            {
                vendors = vendors.Where(p => p.SupplierName.ToUpper().Contains(SearchName.ToUpper()));
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
                        .OrderBy(p => p.SupplierName);
                }
                else
                {
                    vendors = vendors
                        .OrderByDescending(p => p.SupplierName);
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
                        .OrderBy(p => p.SupplierName);
                }
                else
                {
                    vendors = vendors
                        .OrderByDescending(p => p.SupplierName);
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
            var pagedData = await PaginatedList<Supplier>.CreateAsync(vendors.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: Suppliers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Suppliers == null)
            {
                return NotFound();
            }

            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(m => m.ID == id);
            if (supplier == null)
            {
                return NotFound();
            }

            return View(supplier);
        }

        // GET: Suppliers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Suppliers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,SupplierName,Address1,Address2,City,Province,PostalCode,Phone,Email")] Supplier supplier)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(supplier);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch
            {
                ModelState.AddModelError("", "Saving Failed. Please try again or contact your System Administrator.");
            }

            return View(supplier);
        }

        // GET: Suppliers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Suppliers == null)
            {
                return NotFound();
            }

            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
            {
                return NotFound();
            }
            return View(supplier);
        }

        // POST: Suppliers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,SupplierName,Address1,Address2,City,Province,PostalCode,Phone,Email,Status")] Supplier supplier)
        {
            try
            {
                if (id != supplier.ID)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(supplier);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!SupplierExists(supplier.ID))
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

            return View(supplier);
        }

        // GET: Suppliers/Archive/5
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null || _context.Suppliers == null)
            {
                return NotFound();
            }

            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(m => m.ID == id);
            if (supplier == null)
            {
                return NotFound();
            }

            return View(supplier);
        }

        // POST: Suppliers/Archive/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            try
            {
                if (_context.Suppliers == null)
                {
                    return Problem("Entity set 'InventoryContext.Suppliers'  is null.");
                }
                var supplier = await _context.Suppliers.FindAsync(id);

                if (supplier != null)
                {
                    supplier.Status = Archived.Disabled;
                }

                await _context.SaveChangesAsync();
            }
            catch
            {
                ModelState.AddModelError("", "Saving Failed. Please try again or contact your System Administrator.");
            }

            return RedirectToAction(nameof(Index));
        }

        private bool SupplierExists(int id)
        {
          return _context.Suppliers.Any(e => e.ID == id);
        }
    }
}
