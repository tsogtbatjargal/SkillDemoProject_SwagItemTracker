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
    public class TransactionTypesController : CustomControllers.CognizantController
    {
        private readonly InventoryContext _context;

        public TransactionTypesController(InventoryContext context)
        {
            _context = context;
        }

        // GET: TransactionTypes
        public async Task<IActionResult> Index(string sortDirectionCheck, string sortFieldID, string SearchName, string SearchDesc, InOut? InOutStatus, Archived? Status,
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

            var transactionTypes = _context.TransactionTypes
                                    .AsNoTracking();

            //Add as many filters as needed
            if (!String.IsNullOrEmpty(SearchName))
            {
                transactionTypes = transactionTypes.Where(p => p.Name.ToUpper().Contains(SearchName.ToUpper()));
                ViewData["Filtering"] = "btn-danger";
            }
            if (!String.IsNullOrEmpty(SearchDesc))
            {
                transactionTypes = transactionTypes.Where(p => p.Description.ToUpper().Contains(SearchDesc.ToUpper()));
                ViewData["Filtering"] = "btn-danger";
            }
            if (InOutStatus != null)
            {
                transactionTypes = transactionTypes.Where(p => p.InOut == InOutStatus);
                ViewData["Filtering"] = "btn-danger";
            }
            if (Status != null)
            {
                transactionTypes = transactionTypes.Where(p => p.Status == Status);
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
                    transactionTypes = transactionTypes
                        .OrderBy(p => p.Name);
                }
                else
                {
                    transactionTypes = transactionTypes
                        .OrderByDescending(p => p.Name);
                }
            }
            else if (sortField == "Description")
            {
                if (sortDirection == "asc")
                {
                    transactionTypes = transactionTypes
                        .OrderByDescending(p => p.Description);
                }
                else
                {
                    transactionTypes = transactionTypes
                        .OrderBy(p => p.Description);
                }
            }
            else if (sortField == "In/Out")
            {
                if (sortDirection == "asc")
                {
                    transactionTypes = transactionTypes
                        .OrderBy(p => p.InOut);
                }
                else
                {
                    transactionTypes = transactionTypes
                        .OrderByDescending(p => p.InOut);
                }
            }
            else if (sortField == "Status")
            {
                if (sortDirection == "asc")
                {
                    transactionTypes = transactionTypes
                        .OrderBy(p => p.Status);
                }
                else
                {
                    transactionTypes = transactionTypes
                        .OrderByDescending(p => p.Status);
                }
            }
            else //Sorting by Name
            {
                if (sortDirection == "asc")
                {
                    transactionTypes = transactionTypes
                        .OrderBy(p => p.Name)
                        .ThenBy(p => p.Description);
                }
                else
                {
                    transactionTypes = transactionTypes
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
            var pagedData = await PaginatedList<TransactionType>.CreateAsync(transactionTypes.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: TransactionTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.TransactionTypes == null)
            {
                return NotFound();
            }

            var transactionType = await _context.TransactionTypes
                .FirstOrDefaultAsync(m => m.ID == id);
            if (transactionType == null)
            {
                return NotFound();
            }

            return View(transactionType);
        }

        // GET: TransactionTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TransactionTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,Description,InOut")] TransactionType transactionType)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(transactionType);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch
            {
                ModelState.AddModelError("", "Saving Failed. Please try again or contact your System Administrator.");
            }

            return View(transactionType);
        }

        // GET: TransactionTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.TransactionTypes == null)
            {
                return NotFound();
            }

            var transactionType = await _context.TransactionTypes.FindAsync(id);
            if (transactionType == null)
            {
                return NotFound();
            }
            return View(transactionType);
        }

        // POST: TransactionTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,Description,InOut,Status")] TransactionType transactionType)
        {
            try
            {
                if (id != transactionType.ID)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(transactionType);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!TransactionTypeExists(transactionType.ID))
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

            return View(transactionType);
        }

        // GET: TransactionTypes/Archive/5
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null || _context.TransactionTypes == null)
            {
                return NotFound();
            }

            var transactionType = await _context.TransactionTypes
                .FirstOrDefaultAsync(m => m.ID == id);
            if (transactionType == null)
            {
                return NotFound();
            }

            return View(transactionType);
        }

        // POST: TransactionTypes/Archive/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            try
            {
                if (_context.TransactionTypes == null)
                {
                    return Problem("Entity set 'InventoryContext.TransactionTypes'  is null.");
                }

                var transactionType = await _context.TransactionTypes.FindAsync(id);

                if (transactionType != null)
                {
                    transactionType.Status = Archived.Disabled;
                }

                await _context.SaveChangesAsync();
            }
            catch
            {
                ModelState.AddModelError("", "Saving Failed. Please try again or contact your System Administrator.");
            }

            return RedirectToAction(nameof(Index));
        }

        private bool TransactionTypeExists(int id)
        {
            return _context.TransactionTypes.Any(e => e.ID == id);
        }

        private SelectList InOutSelectList(InOut selectedId)
        {
            return new SelectList(_context.TransactionTypes
                .OrderBy(d => d.InOut), "ID", "In/Out", selectedId);
        }

        private void PopulateDropDownLists(TransactionType transaction = null)
        {
            ViewData["InOutStatus"] = InOutSelectList((InOut)(transaction?.InOut));
        }
    }
}
