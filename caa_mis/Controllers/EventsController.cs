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
using Microsoft.EntityFrameworkCore.Storage;
using DNTBreadCrumb.Core;

namespace caa_mis.Controllers
{
    [Authorize(Roles = "Admin, Supervisor")]
    [BreadCrumb(Title = "Events", UseDefaultRouteUrl = true, Order = 0, IgnoreAjaxRequests = true)]
    public class EventsController : CustomControllers.CognizantController
    {
        private readonly InventoryContext _context;

        public EventsController(InventoryContext context)
        {
            
            _context = context;
        }

        // GET: Events
        public async Task<IActionResult> Index(string sortDirectionCheck, string sortFieldID, 
            int? TransactionStatusID, int? BranchID,int? EmployeeID,
            int? page, int? pageSizeID, string actionButton, string sortDirection = "desc", string sortField = "Date")
        {
            //Clear the sort/filter/paging URL Cookie for Controller
            CookieHelper.CookieSet(HttpContext, ControllerName() + "URL", "", -1);

            //Change colour of the button when filtering by setting this default
            ViewData["Filtering"] = "btn-outline-primary";

            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptions = new[] { "Branch", "Date", "Status" };
            
            PopulateDropDownLists();
            ViewDataReturnURL();

            
            var inventory = _context.Events
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
            var pagedData = await PaginatedList<Event>.CreateAsync(inventory.AsNoTracking(), page ?? 1, pageSize);


            return View(pagedData);
        }

        // GET: Events/Details/5
        [BreadCrumb(Title = "Details", Order = 1, IgnoreAjaxRequests = true)]
        public async Task<IActionResult> Details(int? id)
        {
            ViewDataReturnURL();
            
            if (id == null || _context.Events == null)
            {
                return NotFound();
            }

            var Event = await _context.Events
                .Include(b => b.Branch)
                .Include(b => b.Employee)
                .Include(b => b.TransactionStatus)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (Event == null)
            {
                return NotFound();
            }

            return View(Event);
        }

        // GET: Events/Create
        [BreadCrumb(Title = "Create", Order = 1, IgnoreAjaxRequests = true)]
        public IActionResult Create()
        {
            PopulateDropDownLists();
            return View();
        }

        // POST: Events/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,EmployeeID,TransactionStatusID,BranchID,Date,Name")] Event Event)
        {
            ViewDataReturnURL();
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(Event);
                    TempData["SuccessMessage"] = "Created Event successfully.";
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "EventItems", new { EventID = Event.ID });
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed: Event.Name"))
                {
                    ModelState.AddModelError("Name", "Unable to save changes. You cannot add an event with the same name and date");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }
            PopulateDropDownLists(Event);
            return View(Event);
        }

        // GET: Events/Edit/5
        [BreadCrumb(Title = "Edit", Order = 1, IgnoreAjaxRequests = true)]
        public async Task<IActionResult> Edit(int? id)
        {

            ViewDataReturnURL();
            if (id == null || _context.Events == null)
            {
                return NotFound();
            }

            var Event = await _context.Events.FindAsync(id);
            if (Event == null)
            {
                return NotFound();
            }
            PopulateDropDownLists(Event);
            return View(Event);
        }

        // POST: Events/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,EmployeeID,TransactionStatusID,BranchID,Date,Name")] Event Event)
        {
            ViewDataReturnURL();
            
            if (id != Event.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(Event);
                    TempData["SuccessMessage"] = "Updated Event details successfully.";
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(Event.ID))
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
            PopulateDropDownLists(Event);
            return View(Event);
        }

        // GET: Events/Delete/5
        [BreadCrumb(Title = "Delete", Order = 1, IgnoreAjaxRequests = true)]
        public async Task<IActionResult> Delete(int? id)
        {
            ViewDataReturnURL();
            
            if (id == null || _context.Events == null)
            {
                return NotFound();
            }

            var Event = await _context.Events
                .Include(b => b.Branch)
                .Include(b => b.Employee)
                .Include(b => b.TransactionStatus)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (Event == null)
            {
                return NotFound();
            }

            return View(Event);
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            ViewDataReturnURL();
            
            if (_context.Events == null)
            {
                return Problem("Entity set 'InventoryContext.Events'  is null.");
            }
            var Event = await _context.Events.FindAsync(id);
            if (Event != null)
            {
                _context.Events.Remove(Event);
            }
            TempData["SuccessMessage"] = "Deleted Event successfully.";
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

            var transaction = await _context.Events
                .Include(b => b.Branch)
                .Include(b => b.Employee)
                .Include(b => b.TransactionStatus)
                .FirstOrDefaultAsync(m => m.ID == id);

            var items = from a in _context.EventItems
                .Include(t => t.Item)
                .OrderBy(t => t.Item.Name)
                        where a.EventID == id.GetValueOrDefault()
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
            if (_context.Events == null)
            {
                return Problem("Entity set 'InventoryContext.Event'  is null.");
            }

            var status = await _context.TransactionStatuses
                .FirstOrDefaultAsync(m => m.Name == "Released");

            var trans = new Event { ID = id, TransactionStatusID = status.ID };
            if (ReleaseTransaction(id))
            {
                if (ModelState.IsValid)
                {
                    _context.Events.Attach(trans).Property(x => x.TransactionStatusID).IsModified = true;
                    TempData["SuccessMessage"] = "Event release is successful.";
                    _context.SaveChanges();

                    return Redirect(ViewData["returnURL"].ToString());
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Cannot Release this event, there are Items that are currently out of stock.";
            }

            return RedirectToAction(nameof(Index));
        }
        public bool ReleaseTransaction(int id)
        {
            //get the transaction details
            var Event = _context.Events
                .AsNoTracking()
               .FirstOrDefault(m => m.ID == id);
            //get the transaction items
            var EventItems = _context.EventItems
                .Where(m => m.EventID == id)
                .AsNoTracking();


            // check item if have stocks in the branch
            if (Event.BranchID != 1)
            {
                foreach (var item in EventItems)
                {
                    var stockItem = _context.Stocks.AsNoTracking()
                            .FirstOrDefault(s => s.BranchID == Event.BranchID && s.ItemID == item.ItemID && s.Quantity >= item.Quantity);

                    //if we dont have enough stock return false;
                    if (stockItem == null)
                    {
                        return false;
                    }
                }
            }
            //do stock out
            foreach (var item in EventItems)
            {
                //no need to deduct item if it is from head office or supplier
                if (Event.BranchID != 1)
                {
                    //check if stock record already have the item
                    var stockItem = _context.Stocks.AsNoTracking()
                    .FirstOrDefault(s => s.BranchID == Event.BranchID && s.ItemID == item.ItemID);

                    //update the existing one. Deduct the item quantity to the current quantity
                    var updateStock = new Stock { ID = stockItem.ID, Quantity = stockItem.Quantity - item.Quantity };
                    _context.Stocks.Attach(updateStock).Property(x => x.Quantity).IsModified = true;
                    _context.SaveChanges();
                }

            }
            return true;
        }
        [BreadCrumb(Title = "Return", Order = 1, IgnoreAjaxRequests = true)]
        public async Task<IActionResult> Return(int? id)
        {
            ViewDataReturnURL();
            if (id == null || _context.Transactions == null)
            {
                return NotFound();
            }

            var transaction = await _context.Events
                .Include(b => b.Branch)
                .Include(b => b.Employee)
                .Include(b => b.TransactionStatus)
                .FirstOrDefaultAsync(m => m.ID == id);

            var items = from a in _context.EventItems
                .Include(t => t.Item)
                .OrderBy(t => t.Item.Name)
                        where a.EventID == id.GetValueOrDefault()
                        select a;

            ViewBag.Items = items.AsNoTracking();

            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // POST: Transactions/Release/5
        [HttpPost, ActionName("Return")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnConfirmed(int id)
        {
            ViewDataReturnURL();
            if (_context.Events == null)
            {
                return Problem("Entity set 'InventoryContext.Event'  is null.");
            }

            var status = await _context.TransactionStatuses
                .FirstOrDefaultAsync(m => m.Name == "Returned");

            var trans = new Event { ID = id, TransactionStatusID = status.ID };
            if (ReturnEvent(id))
            {
                if (ModelState.IsValid)
                {
                    _context.Events.Attach(trans).Property(x => x.TransactionStatusID).IsModified = true;
                    TempData["SuccessMessage"] = "Return Event Item Success.";
                    _context.SaveChanges();

                    return Redirect(ViewData["returnURL"].ToString());
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Cannot Release this event, there are Items that are currently out of stock.";
            }

            return RedirectToAction(nameof(Index));
        }

        public bool ReturnEvent(int id)
        {
            //get the transaction details
            var Event = _context.Events
                .AsNoTracking()
               .FirstOrDefault(m => m.ID == id);
            //get the transaction items
            var EventItems = _context.EventItems
                .Where(m => m.EventID == id)
                .AsNoTracking();


            //do stock in
            foreach (var item in EventItems)
            {
                //no need to deduct item if it is from head office or supplier
                if (Event.BranchID != 1)
                {
                    //check if stock record already have the item
                    var stockItem = _context.Stocks.AsNoTracking()
                    .FirstOrDefault(s => s.BranchID == Event.BranchID && s.ItemID == item.ItemID);

                    int newQty =(int)item.ReturnedQuantity;

                    if (stockItem == null)
                    {
                        //create a new record
                        Stock stock = new Stock
                        {
                            ID = 0,
                            BranchID = (int)Event.BranchID,
                            ItemID = item.ItemID,
                            Quantity = newQty
                        };

                        _context.Stocks.Add(stock);
                        _context.SaveChanges();
                    }
                    else
                    {
                        //update the existing one. add the item quantity to the current quantity
                        var updateStock = new Stock { ID = stockItem.ID, Quantity = stockItem.Quantity + newQty };
                        _context.Stocks.Attach(updateStock).Property(x => x.Quantity).IsModified = true;
                        _context.SaveChanges();
                    }
                }

            }
            return true;
        }
        private bool EventExists(int id)
        {
          return _context.Events.Any(e => e.ID == id);
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
        private void PopulateDropDownLists(Event Event = null)
        {
            ViewData["BranchID"] = BranchSelectList(Event?.BranchID);
            ViewData["EmployeeID"] = EmployeeList(Event?.EmployeeID);
            ViewData["TransactionStatusID"] = TransactionStatusList(Event?.TransactionStatusID);
            
        }
    }
}
