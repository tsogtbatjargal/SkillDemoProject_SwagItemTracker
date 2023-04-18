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
    [BreadCrumb(Title = "Product Categories", UseDefaultRouteUrl = true, Order = 0, IgnoreAjaxRequests = true)]
    public class CategoriesController : CustomControllers.CognizantController
    {
        private readonly InventoryContext _context;

        public CategoriesController(InventoryContext context)
        {
            _context = context;
        }

        // GET: Categories
        public async Task<IActionResult> Index(string sortDirectionCheck, string sortFieldID, string SearchName, string SearchDesc, Archived? Status,
            int? page, int? pageSizeID, string actionButton, string sortDirection = "asc", string sortField = "Name")
        {
            //Clear the sort/filter/paging URL Cookie for Controller
            CookieHelper.CookieSet(HttpContext, ControllerName() + "URL", "", -1);

            //Change colour of the button when filtering by setting this default
            ViewData["Filtering"] = "btn-outline-primary";

            //PopulateDropDownLists();

            //List of sort options.
            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptions = new[] { "Name", "Description", "In/Out", "Status" };

            var category = _context.Categories
                                    .AsNoTracking();

            //Add as many filters as needed
            if (!String.IsNullOrEmpty(SearchName))
            {
                category = category.Where(p => p.Name.ToUpper().Contains(SearchName.ToUpper()));
                ViewData["Filtering"] = "btn-danger";
            }
            if (!String.IsNullOrEmpty(SearchDesc))
            {
                category = category.Where(p => p.Description.ToUpper().Contains(SearchDesc.ToUpper()));
                ViewData["Filtering"] = "btn-danger";
            }
            if (Status != null)
            {
                category = category.Where(p => p.Status == Status);
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
                    category = category
                        .OrderBy(p => p.Name);
                }
                else
                {
                    category = category
                        .OrderByDescending(p => p.Name);
                }
            }
            else if (sortField == "Description")
            {
                if (sortDirection == "asc")
                {
                    category = category
                        .OrderByDescending(p => p.Description);
                }
                else
                {
                    category = category
                        .OrderBy(p => p.Description);
                }
            }
            else if (sortField == "Status")
            {
                if (sortDirection == "asc")
                {
                    category = category
                        .OrderBy(p => p.Status);
                }
                else
                {
                    category = category
                        .OrderByDescending(p => p.Status);
                }
            }
            else //Sorting by Name
            {
                if (sortDirection == "asc")
                {
                    category = category
                        .OrderBy(p => p.Name)
                        .ThenBy(p => p.Description);
                }
                else
                {
                    category = category
                        .OrderByDescending(p => p.Name)
                        .ThenByDescending(p => p.Description);
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
            var pagedData = await PaginatedList<Category>.CreateAsync(category.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: Categories/Details/5
        [BreadCrumb(Title = "Details", Order = 1, IgnoreAjaxRequests = true)]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.ID == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Categories/Create
        [BreadCrumb(Title = "Create", Order = 1, IgnoreAjaxRequests = true)]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,Description,InOut")] Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categories/Edit/5
        [BreadCrumb(Title = "Edit", Order = 1, IgnoreAjaxRequests = true)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Category/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,Description,InOut,Status")] Category category)
        {
            if (id != category.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.ID))
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
            return View(category);
        }

        // GET: Categories/Archive/5
        [BreadCrumb(Title = "Archive", Order = 1, IgnoreAjaxRequests = true)]
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.ID == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Categories/Archive/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            if (_context.Categories == null)
            {
                return Problem("Entity set 'InventoryContext.Categories'  is null.");
            }

            var category = await _context.Categories.FindAsync(id);

            if (category != null)
            {
                category.Status = Archived.Disabled;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.ID == id);
        }
        
    }
}
