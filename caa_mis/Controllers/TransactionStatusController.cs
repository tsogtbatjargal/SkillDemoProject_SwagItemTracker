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
    [BreadCrumb(Title = "Inventory Status", UseDefaultRouteUrl = true, Order = 0, IgnoreAjaxRequests = true)]
    public class TransactionStatusController : CustomControllers.CognizantController
    {
        private readonly InventoryContext _context;

        public TransactionStatusController(InventoryContext context)
        {
            _context = context;
        }

        // GET: TransactionStatus
        public async Task<IActionResult> Index(string sortDirectionCheck, string sortFieldID, string SearchName, string SearchDesc, Archived? Status,
            int? page, int? pageSizeID, string actionButton, string sortDirection = "asc", string sortField = "Name")
        {
            //Clear the sort/filter/paging URL Cookie for Controller
            CookieHelper.CookieSet(HttpContext, ControllerName() + "URL", "", -1);

            //Change colour of the button when filtering by setting this default
            ViewData["Filtering"] = "btn-outline-primary";

            //List of sort options.
            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptions = new[] { "Name", "Description", "Status" };

            var transactionStatuses = _context.TransactionStatuses
                                        .AsNoTracking();

            //Add as many filters as needed
            if (!String.IsNullOrEmpty(SearchName))
            {
                transactionStatuses = transactionStatuses.Where(p => p.Name.ToUpper().Contains(SearchName.ToUpper()));
                ViewData["Filtering"] = "btn-danger";
            }
            if (!String.IsNullOrEmpty(SearchDesc))
            {
                transactionStatuses = transactionStatuses.Where(p => p.Description.ToUpper().Contains(SearchDesc.ToUpper()));
                ViewData["Filtering"] = "btn-danger";
            }
            if (Status != null)
            {
                transactionStatuses = transactionStatuses.Where(p => p.Status == Status);
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
                    transactionStatuses = transactionStatuses
                        .OrderBy(p => p.Name);
                }
                else
                {
                    transactionStatuses = transactionStatuses
                        .OrderByDescending(p => p.Name);
                }
            }
            else if (sortField == "Description")
            {
                if (sortDirection == "asc")
                {
                    transactionStatuses = transactionStatuses
                        .OrderByDescending(p => p.Description);
                }
                else
                {
                    transactionStatuses = transactionStatuses
                        .OrderBy(p => p.Description);
                }
            }
            else if (sortField == "Status")
            {
                if (sortDirection == "asc")
                {
                    transactionStatuses = transactionStatuses
                        .OrderBy(p => p.Status);
                }
                else
                {
                    transactionStatuses = transactionStatuses
                        .OrderByDescending(p => p.Status);
                }
            }
            else //Sorting by Name
            {
                if (sortDirection == "asc")
                {
                    transactionStatuses = transactionStatuses
                        .OrderBy(p => p.Name)
                        .ThenBy(p => p.Description);
                }
                else
                {
                    transactionStatuses = transactionStatuses
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
            var pagedData = await PaginatedList<TransactionStatus>.CreateAsync(transactionStatuses.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: TransactionStatus/Details/5
        [BreadCrumb(Title = "Details", Order = 1, IgnoreAjaxRequests = true)]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.TransactionStatuses == null)
            {
                return NotFound();
            }

            var transactionStatus = await _context.TransactionStatuses
                .FirstOrDefaultAsync(m => m.ID == id);
            if (transactionStatus == null)
            {
                return NotFound();
            }

            return View(transactionStatus);
        }

        // GET: TransactionStatus/Create
        [BreadCrumb(Title = "Create", Order = 1, IgnoreAjaxRequests = true)]
        public IActionResult Create()
        {
            return View();
        }

        // POST: TransactionStatus/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,Description")] TransactionStatus transactionStatus)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(transactionStatus);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch
            {
                ModelState.AddModelError("", "Saving Failed. Please try again or contact your System Administrator.");
            }

            return View(transactionStatus);

        }

        // GET: TransactionStatus/Edit/5
        [BreadCrumb(Title = "Edit", Order = 1, IgnoreAjaxRequests = true)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.TransactionStatuses == null)
            {
                return NotFound();
            }

            var transactionStatus = await _context.TransactionStatuses.FindAsync(id);
            if (transactionStatus == null)
            {
                return NotFound();
            }
            return View(transactionStatus);
        }

        // POST: TransactionStatus/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,Description,Status")] TransactionStatus transactionStatus)
        {
            try
            {
                if (id != transactionStatus.ID)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(transactionStatus);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!TransactionStatusExists(transactionStatus.ID))
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

            return View(transactionStatus);
        }

        // GET: TransactionStatus/Archive/5
        [BreadCrumb(Title = "Archive", Order = 1, IgnoreAjaxRequests = true)]
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null || _context.TransactionStatuses == null)
            {
                return NotFound();
            }

            var transactionStatus = await _context.TransactionStatuses
                .FirstOrDefaultAsync(m => m.ID == id);
            if (transactionStatus == null)
            {
                return NotFound();
            }

            return View(transactionStatus);
        }

        // POST: TransactionStatus/Archive/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            try
            {
                if (_context.TransactionStatuses == null)
                {
                    return Problem("Entity set 'InventoryContext.TransactionStatuses'  is null.");
                }

                var transactionStatus = await _context.TransactionStatuses.FindAsync(id);

                if (transactionStatus != null)
                {
                    transactionStatus.Status = Archived.Disabled;
                }

                await _context.SaveChangesAsync();
            }
            catch
            {
                ModelState.AddModelError("", "Saving Failed. Please try again or contact your System Administrator.");
            }

            return RedirectToAction(nameof(Index));
        }

        private bool TransactionStatusExists(int id)
        {
            return _context.TransactionStatuses.Any(e => e.ID == id);
        }
    }
}
