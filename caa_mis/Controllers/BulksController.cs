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
using caa_mis.ViewModels;
using Microsoft.AspNetCore.Authorization;
using DNTBreadCrumb.Core;

namespace caa_mis.Controllers
{
    [Authorize(Roles = "Admin")]
    [BreadCrumb(Title = "Bulks", UseDefaultRouteUrl = true, Order = 0, IgnoreAjaxRequests = true)]
    public class BulksController : CustomControllers.CognizantController
    {
        private readonly InventoryContext _context;

        public BulksController(InventoryContext context)
        {
            
            _context = context;
        }

        // GET: Bulks
        public async Task<IActionResult> Index(string sortDirectionCheck, string sortFieldID, 
            int? TransactionStatusID, int? BranchID,int? EmployeeID,
            int? page, int? pageSizeID, string actionButton, string sortDirection = "asc", string sortField = "BranchID")
        {
            //Clear the sort/filter/paging URL Cookie for Controller
            CookieHelper.CookieSet(HttpContext, ControllerName() + "URL", "", -1);

            //Change colour of the button when filtering by setting this default
            ViewData["Filtering"] = "btn-outline-primary";

            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptions = new[] { "Branch", "Date", "Status" };
            
            PopulateDropDownLists();
            ViewDataReturnURL();

            
            var inventory = _context.Bulks
                .Include(b => b.Branch)
                .Include(b => b.Employee)
                .Include(b => b.TransactionStatus)
                .AsNoTracking();

            if (BranchID.HasValue)
            {
                inventory = inventory.Where(p => p.BranchID == BranchID);
                ViewData["Filtering"] = "btn-danger";
            }
            if (TransactionStatusID.HasValue)
            {
                inventory = inventory.Where(p => p.TransactionStatusID == TransactionStatusID);
                ViewData["Filtering"] = "btn-danger";
            }
            if (EmployeeID.HasValue)
            {
                inventory = inventory.Where(p => p.EmployeeID == EmployeeID);
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
            if (sortField == "Branch")
            {
                if (sortDirection == "asc")
                {
                    inventory = inventory
                        .OrderBy(p => p.Branch.Name);
                }
                else
                {
                    inventory = inventory
                        .OrderByDescending(p => p.Branch.Name);
                }
            }
            else if (sortField == "Status")
            {
                if (sortDirection == "asc")
                {
                    inventory = inventory
                        .OrderByDescending(p => p.TransactionStatus.Name);
                }
                else
                {
                    inventory = inventory
                        .OrderBy(p => p.TransactionStatus.Name);
                }
            }
            else
            {
                if (sortDirection == "asc")
                {
                    inventory = inventory
                        .OrderBy(p => p.Date);
                }
                else
                {
                    inventory = inventory
                        .OrderByDescending(p => p.Date);
                }
            }
            
            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;
            //SelectList for Sorting Options
            ViewBag.sortFieldID = new SelectList(sortOptions, sortField.ToString());

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, "InitialInventory");
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<Bulk>.CreateAsync(inventory.AsNoTracking(), page ?? 1, pageSize);


            return View(pagedData);
        }

        // GET: Bulks/Details/5
        [BreadCrumb(Title = "Details", Order = 1, IgnoreAjaxRequests = true)]
        public async Task<IActionResult> Details(int? id)
        {
            ViewDataReturnURL();
            
            if (id == null || _context.Bulks == null)
            {
                return NotFound();
            }

            var bulk = await _context.Bulks
                .Include(b => b.Branch)
                .Include(b => b.Employee)
                .Include(b => b.TransactionStatus)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (bulk == null)
            {
                return NotFound();
            }

            return View(bulk);
        }

        // GET: Bulks/Create
        [BreadCrumb(Title = "Create", Order = 1, IgnoreAjaxRequests = true)]
        public IActionResult Create()
        {
            PopulateDropDownLists();
            return View();
        }

        // POST: Bulks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,EmployeeID,TransactionStatusID,BranchID,Date")] Bulk bulk)
        {
            ViewDataReturnURL();
            if (ModelState.IsValid)
            {
                _context.Add(bulk);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "BulkItems", new { BulkID = bulk.ID });
            }
            PopulateDropDownLists(bulk);
            return View(bulk);
        }

        // GET: Bulks/Edit/5
        [BreadCrumb(Title = "Edit", Order = 1, IgnoreAjaxRequests = true)]
        public async Task<IActionResult> Edit(int? id)
        {

            ViewDataReturnURL();
            if (id == null || _context.Bulks == null)
            {
                return NotFound();
            }

            var bulk = await _context.Bulks.FindAsync(id);
            if (bulk == null)
            {
                return NotFound();
            }
            PopulateDropDownLists(bulk);
            return View(bulk);
        }

        // POST: Bulks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,EmployeeID,TransactionStatusID,BranchID,Date")] Bulk bulk)
        {
            ViewDataReturnURL();
            
            if (id != bulk.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bulk);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BulkExists(bulk.ID))
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
            PopulateDropDownLists(bulk);
            return View(bulk);
        }

        // GET: Bulks/Delete/5
        [BreadCrumb(Title = "Delete", Order = 1, IgnoreAjaxRequests = true)]
        public async Task<IActionResult> Delete(int? id)
        {
            ViewDataReturnURL();
            
            if (id == null || _context.Bulks == null)
            {
                return NotFound();
            }

            var bulk = await _context.Bulks
                .Include(b => b.Branch)
                .Include(b => b.Employee)
                .Include(b => b.TransactionStatus)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (bulk == null)
            {
                return NotFound();
            }

            return View(bulk);
        }

        // POST: Bulks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            ViewDataReturnURL();
            
            if (_context.Bulks == null)
            {
                return Problem("Entity set 'InventoryContext.Bulks'  is null.");
            }
            var bulk = await _context.Bulks.FindAsync(id);
            if (bulk != null)
            {
                _context.Bulks.Remove(bulk);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Transactions/Release/5
        [BreadCrumb(Title = "Release", Order = 1, IgnoreAjaxRequests = true)]

        public async Task<IActionResult> Release(int? id)
        {
            ViewDataReturnURL();
            if (id == null || _context.Transactions == null)
            {
                return NotFound();
            }

            var transaction = await _context.Bulks
                .Include(b => b.Branch)
                .Include(b => b.Employee)
                .Include(b => b.TransactionStatus)
                .FirstOrDefaultAsync(m => m.ID == id);

            var items = from a in _context.BulkItems
                .Include(t => t.Item)
                .OrderBy(t => t.Item.Name)
                        where a.BulkID == id.GetValueOrDefault()
                        select a;

            ViewBag.Items = items.AsNoTracking();

            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // POST: Transactions/Release/5
        [HttpPost, ActionName("Release")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReleaseConfirmed(int id)
        {
            ViewDataReturnURL();
            if (_context.Bulks == null)
            {
                return Problem("Entity set 'InventoryContext.Bulk'  is null.");
            }

            var status = await _context.TransactionStatuses
                .FirstOrDefaultAsync(m => m.Name == "Released");

            var trans = new Bulk { ID = id, TransactionStatusID = status.ID };
            ReleaseTransaction(id);
            if (ModelState.IsValid)
            {
                _context.Bulks.Attach(trans).Property(x => x.TransactionStatusID).IsModified = true;
                _context.SaveChanges();

                return Redirect(ViewData["returnURL"].ToString());
            }

            return RedirectToAction(nameof(Index));
        }
        public void ReleaseTransaction(int id)
        {
            //get the transaction details
            var bulk = _context.Bulks
                .AsNoTracking()
               .FirstOrDefault(m => m.ID == id);
            //get the transaction items
            var bulkItems = _context.BulkItems
                .Where(m => m.BulkID == id)
                .AsNoTracking();

            //delete all in stocks in branch
            var stocks = _context.Stocks
                .Where(m => m.BranchID == bulk.BranchID)
                .AsNoTracking();
            _context.Stocks.RemoveRange(stocks);
            
            //insert bulk items to stocks using bulk.BranchID
            foreach (var item in bulkItems)
            {
                var stock = new Stock
                {
                    ItemID = item.ItemID,
                    Quantity = item.Quantity,
                    BranchID = bulk.BranchID
                };
                _context.Stocks.Add(stock);
            }
            _context.SaveChanges(); 
        }

        private bool BulkExists(int id)
        {
          return _context.Bulks.Any(e => e.ID == id);
        }

        private void ViewDataReturnURL()
        {
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, ControllerName());
        }

        private SelectList BranchSelectList(int? selectedId)
        {
            return new SelectList(_context
                .Branches
                .OrderBy(m => m.Name), "ID", "Name", selectedId);
        }
        private SelectList EmployeeList(int? selectedId)
        {
            return new SelectList(_context
                .Employees
                .OrderBy(m => m.FirstName), "ID", "FirstName", selectedId);
        }

        private SelectList TransactionStatusList(int? selectedId)
        {
            return new SelectList(_context
                .TransactionStatuses
                .OrderBy(m => m.Name), "ID", "Name", selectedId);
        }
        private void PopulateDropDownLists(Bulk bulk = null)
        {
            ViewData["BranchID"] = BranchSelectList(bulk?.BranchID);
            ViewData["EmployeeID"] = EmployeeList(bulk?.EmployeeID);
            ViewData["TransactionStatusID"] = TransactionStatusList(bulk?.TransactionStatusID);
            
        }
    }
}
