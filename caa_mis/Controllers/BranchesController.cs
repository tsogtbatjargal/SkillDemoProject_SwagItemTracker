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
    [BreadCrumb(Title = "Branches", UseDefaultRouteUrl = true, Order = 0, IgnoreAjaxRequests = true)]
    public class BranchesController : CustomControllers.CognizantController
    {
        private readonly InventoryContext _context;

        public BranchesController(InventoryContext context)
        {
            _context = context;
        }

        // GET: Branch
        public async Task<IActionResult> Index(string sortDirectionCheck, string sortFieldID, string SearchName, string SearchLoc, Archived? Status,
            int? page, int? pageSizeID, string actionButton, string sortDirection = "asc", string sortField = "Name")
        {
            //Clear the sort/filter/paging URL Cookie for Controller
            CookieHelper.CookieSet(HttpContext, ControllerName() + "URL", "", -1);

            //Change colour of the button when filtering by setting this default
            ViewData["Filtering"] = "btn-outline-primary";

            //PopulateDropDownLists();

            //List of sort options.
            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptions = new[] { "Name","Address", "Location", "Phone Number", "Status" };

            var branches = _context.Branches
                                    .AsNoTracking();

            //Add as many filters as needed
            if (!String.IsNullOrEmpty(SearchName))
            {
                branches = branches.Where(p => p.Name.ToUpper().Contains(SearchName.ToUpper()));
                ViewData["Filtering"] = "btn-danger";
            }
            if (!String.IsNullOrEmpty(SearchLoc))
            {
                branches = branches.Where(p => p.Location.ToUpper().Contains(SearchLoc.ToUpper()));
                ViewData["Filtering"] = "btn-danger";
            }            
            if (Status != null)
            {
                branches = branches.Where(p => p.Status == Status);
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
                    branches = branches
                        .OrderBy(p => p.Name);
                }
                else
                {
                    branches = branches
                        .OrderByDescending(p => p.Name);
                }
            }
            else if (sortField == "Address")
            {
                if (sortDirection == "asc")
                {
                    branches = branches
                        .OrderByDescending(p => p.Address);
                }
                else
                {
                    branches = branches
                        .OrderBy(p => p.Address);
                }
            }
            else if (sortField == "Location")
            {
                if (sortDirection == "asc")
                {
                    branches = branches
                        .OrderByDescending(p => p.Location);
                }
                else
                {
                    branches = branches
                        .OrderBy(p => p.Location);
                }
            }
            else if (sortField == "Phone Number")
            {
                if (sortDirection == "asc")
                {
                    branches = branches
                        .OrderByDescending(p => p.PhoneNumber);
                }
                else
                {
                    branches = branches
                        .OrderBy(p => p.PhoneNumber);
                }
            }
            else if (sortField == "Status")
            {
                if (sortDirection == "asc")
                {
                    branches = branches
                        .OrderBy(p => p.Status);
                }
                else
                {
                    branches = branches
                        .OrderByDescending(p => p.Status);
                }
            }
            else //Sorting by Name
            {
                if (sortDirection == "asc")
                {
                    branches = branches
                        .OrderBy(p => p.Name)
                        .ThenBy(p => p.Location);
                }
                else
                {
                    branches = branches
                        .OrderByDescending(p => p.Name)
                        .ThenByDescending(p => p.Location);
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
            var pagedData = await PaginatedList<Branch>.CreateAsync(branches.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: Branch/Details/5
        [BreadCrumb(Title = "Details", Order = 1, IgnoreAjaxRequests = true)]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Branches == null)
            {
                return NotFound();
            }

            var branches = await _context.Branches
                .FirstOrDefaultAsync(m => m.ID == id);
            if (branches == null)
            {
                return NotFound();
            }

            return View(branches);
        }

        // GET: Branch/Create
        [BreadCrumb(Title = "Create", Order = 1, IgnoreAjaxRequests = true)]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Branch/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,Address,Location,PhoneNumber")] Branch branches)
        {
            if (ModelState.IsValid)
            {
                _context.Add(branches);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(branches);
        }

        // GET: Branch/Edit/5
        [BreadCrumb(Title = "Edit", Order = 1, IgnoreAjaxRequests = true)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Branches == null)
            {
                return NotFound();
            }

            var branches = await _context.Branches.FindAsync(id);
            if (branches == null)
            {
                return NotFound();
            }
            return View(branches);
        }

        // POST: Branches/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,Address,Location,PhoneNumber,Status")] Branch branches)
        {
            if (id != branches.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(branches);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BranchExists(branches.ID))
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
            return View(branches);
        }

        // GET: Branch/Archive/5
        [BreadCrumb(Title = "Archive", Order = 1, IgnoreAjaxRequests = true)]
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null || _context.Branches == null)
            {
                return NotFound();
            }

            var branches = await _context.Branches
                .FirstOrDefaultAsync(m => m.ID == id);
            if (branches == null)
            {
                return NotFound();
            }

            return View(branches);
        }

        // POST: Branch/Archive/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            if (_context.Branches == null)
            {
                return Problem("Entity set 'InventoryContext.Branches'  is null.");
            }

            var branches = await _context.Branches.FindAsync(id);

            if (branches != null)
            {
                branches.Status = Archived.Disabled;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BranchExists(int id)
        {
            return _context.Branches.Any(e => e.ID == id);
        }
        
    }
}
