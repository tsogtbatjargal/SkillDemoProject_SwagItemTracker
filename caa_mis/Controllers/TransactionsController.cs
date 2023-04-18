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
using Newtonsoft.Json;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Drawing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;
using DNTBreadCrumb.Core;

namespace caa_mis.Controllers
{
    [Authorize(Roles = "Admin, Supervisor")]
    [BreadCrumb(Title = "Transfer", UseDefaultRouteUrl = true, Order = 0, IgnoreAjaxRequests = true)]
    public class TransactionsController : CustomControllers.CognizantController
    {
        private readonly InventoryContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public TransactionsController(InventoryContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Transactions
        public async Task<IActionResult> Index(string sortDirectionCheck, string sortFieldID, string SearchString, int? TransactionTypeID, int? TransactionStatusID, int? DestinationID,
            int? page, int? pageSizeID, string actionButton, string sortDirection = "desc", string sortField = "Date")
        {
            //Clear the sort/filter/paging URL Cookie for Controller
            CookieHelper.CookieSet(HttpContext, ControllerName() + "URL", "", -1);

            //Change colour of the button when filtering by setting this default
            ViewData["Filtering"] = "btn-outline-primary";

            //List of sort options.
            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptions = new[] { "Type", "Description", "Origin", "Destination", "Date", "Status" };

            PopulateDropDownLists();
            ViewDataReturnURL();

            var inventory = from a in _context.Transactions
                .Include(t => t.Destination)
                .Include(t => t.Employee)
                .Include(t => t.Origin)
                .Include(t => t.TransactionStatus)
                .Include(t => t.TransactionType)
                where a.TransactionTypeID == 2
                select a;

            if (TransactionTypeID.HasValue)
            {
                inventory = inventory.Where(p => p.TransactionTypeID == TransactionTypeID);
                ViewData["Filtering"] = "btn-danger";
            }
            if (TransactionStatusID.HasValue)
            {
                inventory = inventory.Where(p => p.TransactionStatusID == TransactionStatusID);
                ViewData["Filtering"] = "btn-danger";
            }
            if (DestinationID.HasValue)
            {
                inventory = inventory.Where(p => p.DestinationID == DestinationID);
                ViewData["Filtering"] = "btn-danger";
            }
            if (!String.IsNullOrEmpty(SearchString))
            {
                inventory = inventory.Where(p => p.Description.ToUpper().Contains(SearchString.ToUpper()));
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
            if (sortField == "Type")
            {
                if (sortDirection == "asc")
                {
                    inventory = inventory
                        .OrderBy(p => p.TransactionType.Name);
                }
                else
                {
                    inventory = inventory
                        .OrderByDescending(p => p.TransactionType.Name);
                }
            }
            else if (sortField == "Description")
            {
                if (sortDirection == "asc")
                {
                    inventory = inventory
                        .OrderByDescending(p => p.Description);
                }
                else
                {
                    inventory = inventory
                        .OrderBy(p => p.Description);
                }
            }
            else if (sortField == "Origin")
            {
                if (sortDirection == "asc")
                {
                    inventory = inventory
                        .OrderBy(p => p.Origin.Name);
                }
                else
                {
                    inventory = inventory
                        .OrderByDescending(p => p.Origin.Name);
                }
            }
            else if (sortField == "Destination")
            {
                if (sortDirection == "asc")
                {
                    inventory = inventory
                        .OrderBy(p => p.Destination.Name);
                }
                else
                {
                    inventory = inventory
                        .OrderByDescending(p => p.Destination.Name);
                }
            }
            else if (sortField == "Date")
            {
                if (sortDirection == "asc")
                {
                    inventory = inventory
                        .OrderBy(p => p.TransactionDate);
                }
                else
                {
                    inventory = inventory
                        .OrderByDescending(p => p.TransactionDate);
                }
            }
            else //Sorting by Name
            {
                if (sortDirection == "asc")
                {
                    inventory = inventory
                        .OrderBy(p => p.TransactionStatus.Name);
                }
                else
                {
                    inventory = inventory
                        .OrderByDescending(p => p.TransactionStatus.Name);
                }
            }

            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;
            //SelectList for Sorting Options
            ViewBag.sortFieldID = new SelectList(sortOptions, sortField.ToString());

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, "Transactions");
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<Transaction>.CreateAsync(inventory.AsNoTracking(), page ?? 1, pageSize);


            return View(pagedData);
        }

        // GET: Transactions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            ViewDataReturnURL();

            if (id == null || _context.Transactions == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .Include(t => t.Destination)
                .Include(t => t.Employee)
                .Include(t => t.Origin)
                .Include(t => t.TransactionStatus)
                .Include(t => t.TransactionType)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // GET: Transactions/Create
        [BreadCrumb(Title = "Create", Order = 1, IgnoreAjaxRequests = true)]
        public IActionResult Create()
        {
            PopulateDropDownLists();
            return View();
        }

        // POST: Transactions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,EmployeeID,TransactionStatusID,TransactionTypeID,OriginID,DestinationID,TransactionDate,ReceivedDate,Description,Shipment")] Transaction transaction)
        {
            ViewDataReturnURL();
            if (ModelState.IsValid)
            {
                _context.Add(transaction);
                TempData["SuccessMessage"] = "Created Transfer transaction successfully.";
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "TransactionItems", new { TransactionID = transaction.ID });
            }
            PopulateDropDownLists(transaction);
            return View(transaction);
        }

        // GET: Transactions/Edit/5
        [BreadCrumb(Title = "Edit", Order = 1, IgnoreAjaxRequests = true)]
        public async Task<IActionResult> Edit(int? id)
        {
            ViewDataReturnURL();
            if (id == null || _context.Transactions == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }
            PopulateDropDownLists(transaction);
            return View(transaction);
        }

        // POST: Transactions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,EmployeeID,TransactionStatusID,TransactionTypeID,OriginID,DestinationID,TransactionDate,ReceivedDate,Description,Shipment")] Transaction transaction)
        {
            ViewDataReturnURL();
            if (id != transaction.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transaction);
                    TempData["SuccessMessage"] = "Updated transfer details successfully.";
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "TransactionItems", new { TransactionID = transaction.ID });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionExists(transaction.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }                
            }
            PopulateDropDownLists(transaction);
            return View(transaction);
        }

        // GET: Transactions/Delete/5
        [BreadCrumb(Title = "Delete", Order = 1, IgnoreAjaxRequests = true)]
        public async Task<IActionResult> Delete(int? id)
        {
            ViewDataReturnURL();
            if (id == null || _context.Transactions == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .Include(t => t.Destination)
                .Include(t => t.Employee)
                .Include(t => t.Origin)
                .Include(t => t.TransactionStatus)
                .Include(t => t.TransactionType)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // POST: Transactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            ViewDataReturnURL();
            if (_context.Transactions == null)
            {
                return Problem("Entity set 'InventoryContext.Transactions'  is null.");
            }
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
            }
            TempData["SuccessMessage"] = "Deleted transfer successfully.";
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

            var transaction = await _context.Transactions
                .Include(t => t.Destination)
                .Include(t => t.Employee)
                .Include(t => t.Origin)
                .Include(t => t.TransactionStatus)
                .Include(t => t.TransactionType)
                .FirstOrDefaultAsync(m => m.ID == id);

            var items = from a in _context.TransactionItems
                .Include(t => t.Item)
                .OrderBy(t => t.Item.Name)
                        where a.TransactionID == id.GetValueOrDefault()
                        select a;

            ViewBag.Items = items.AsNoTracking();

            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }


        // GET: Transactions/Release/5
        public async Task<IActionResult> Receive(int? id)
        {
            ViewDataReturnURL();
            if (id == null || _context.Transactions == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .Include(t => t.Destination)
                .Include(t => t.Employee)
                .Include(t => t.Origin)
                .Include(t => t.TransactionStatus)
                .Include(t => t.TransactionType)
                .FirstOrDefaultAsync(m => m.ID == id);

            var items = from a in _context.TransactionItems
                .Include(t => t.Item)
                .OrderBy(t => t.Item.Name)
                        where a.TransactionID == id.GetValueOrDefault()
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
            if (_context.Transactions == null)
            {
                return Problem("Entity set 'InventoryContext.Transactions'  is null.");
            }

            var status = await _context.TransactionStatuses
                .FirstOrDefaultAsync(m => m.Name == "Released");

            var trans = new Transaction { ID = id, TransactionStatusID = status.ID };

            if(ReleaseTransaction(id))
            {
                if (ModelState.IsValid)
                {
                    _context.Transactions.Attach(trans).Property(x => x.TransactionStatusID).IsModified = true;
                    TempData["SuccessMessage"] = "Transfer is successful.";
                    _context.SaveChanges();

                    return Redirect(ViewData["returnURL"].ToString());
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Cannot Release this transfer, there are Items that are currently out of stock.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Transactions/Receive/5
        [HttpPost, ActionName("Receive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReceiveConfirmed(int id)
        {
            ViewDataReturnURL();
            if (_context.Transactions == null)
            {
                return Problem("Entity set 'InventoryContext.Transactions'  is null.");
            }

            var transToUpdate = await _context.Transactions.FindAsync(id);

            var status = await _context.TransactionStatuses
                .FirstOrDefaultAsync(m => m.Name == "Received");

            // var trans = new Transaction { ID = id, TransactionStatusID = status.ID, ReceivedDate = DateTime.Today };
            transToUpdate.ID = transToUpdate.ID;
            transToUpdate.EmployeeID = transToUpdate.EmployeeID;
            transToUpdate.TransactionStatusID = status.ID;
            transToUpdate.TransactionDate = transToUpdate.TransactionDate;
            transToUpdate.TransactionTypeID = transToUpdate.TransactionTypeID;
            transToUpdate.OriginID = transToUpdate.OriginID;
            transToUpdate.DestinationID = transToUpdate.DestinationID;
            transToUpdate.ReceivedDate = DateTime.Today;
            transToUpdate.Shipment = transToUpdate.Shipment;
            transToUpdate.Description = transToUpdate.Description;
            ReceiveTransaction(id);
            if (ModelState.IsValid)
            {
                //_context.Transactions.Attach(trans).Property(x => x.TransactionStatusID).IsModified = true;
                _context.Update(transToUpdate);
                _context.SaveChanges();

                return Redirect(ViewData["returnURL"].ToString());
            }

            return RedirectToAction(nameof(Index));
        }

        //adding item to the inventory
        public void ReceiveTransaction(int id)
        {
            //get the transaction details
            var transaction = _context.Transactions
                .AsNoTracking()
               .FirstOrDefault(m => m.ID == id);
            //get the transaction items
            var transactionItems = _context.TransactionItems
                .Where(m => m.TransactionID == id)
                .AsNoTracking();

            //do
            //
            foreach (var item in transactionItems)
            {
                //check if stock record already have the item
                var stockItem = _context.Stocks
                    .AsNoTracking()
                    .FirstOrDefault(s => s.BranchID == transaction.DestinationID && s.ItemID == item.ItemID);
                
               
                int newQty = (item.ReceivedQuantity != null) ? (int)item.ReceivedQuantity: item.Quantity;

                if (stockItem == null)
                {
                    //create a new record
                    Stock stock = new Stock
                    {
                        ID = 0,
                        BranchID = (int)transaction.DestinationID,
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
        //adding item to the inventory
        public bool ReleaseTransaction(int id)
        {
            //get the transaction details
            var transaction =  _context.Transactions
                .AsNoTracking()
               .FirstOrDefault(m => m.ID == id);
            //get the transaction items
            var transactionItems = _context.TransactionItems
                .Where(m => m.TransactionID == id)
                .AsNoTracking();

            // check item if have stocks in the branch
            if(transaction.OriginID != 1)
            {
                foreach (var item in transactionItems)
                {
                    var stockItem = _context.Stocks.AsNoTracking()
                            .FirstOrDefault(s => s.BranchID == transaction.OriginID && s.ItemID == item.ItemID && s.Quantity >= item.Quantity);

                    //if we dont have enough stock return false;
                    if (stockItem == null)
                    {
                        return false;
                    }
                }
            }
            //loop again to update the record
            foreach (var item in transactionItems)
            {
                //no need to deduct item if it is from head office or supplier
                if (transaction.OriginID != 1)
                {
                    //check if stock record already have the item
                    var stockItem = _context.Stocks.AsNoTracking()
                    .FirstOrDefault(s => s.BranchID == transaction.OriginID && s.ItemID == item.ItemID);

                    //update the existing one. Deduct the item quantity to the current quantity
                    var updateStock = new Stock { ID = stockItem.ID, Quantity = stockItem.Quantity - item.Quantity };
                    _context.Stocks.Attach(updateStock).Property(x => x.Quantity).IsModified = true;
                    _context.SaveChanges();
                }
                
                var stockItem2 = _context.Stocks
                    .AsNoTracking()
                    .FirstOrDefault(s => s.BranchID == transaction.DestinationID && s.ItemID == item.ItemID);
                
                if (stockItem2 == null)
                {
                    //create a new record
                    Stock stock = new Stock
                    {
                        ID = 0,
                        BranchID = (int)transaction.DestinationID,
                        ItemID = item.ItemID,
                        Quantity = item.Quantity
                    };

                    _context.Stocks.Add(stock);
                    _context.SaveChanges();
                }
                else
                {
                    //update the existing one. add the item quantity to the current quantity
                    var updateStock2 = new Stock { ID = stockItem2.ID, Quantity = stockItem2.Quantity + item.Quantity };
                    _context.Stocks.Attach(updateStock2).Property(x => x.Quantity).IsModified = true;
                    _context.SaveChanges();
                }
            }
            return true;
            
        }

            //incoming transactions
        public async Task<IActionResult> Incoming(string sortDirectionCheck, string sortFieldID, string SearchString, int? TransactionTypeID, int? TransactionStatusID, int? DestinationID,
           int? page, int? pageSizeID, string actionButton, string sortDirection = "asc", string sortField = "Type")
        {
            //Clear the sort/filter/paging URL Cookie for Controller
            CookieHelper.CookieSet(HttpContext, ControllerName() + "URL", "", -1);
            
            //List of sort options.
            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptions = new[] { "Type", "Description", "Origin", "Destination", "Transaction Date"};

            PopulateDropDownLists();
            ViewDataReturnURL();
            
            var inventory = _context.Transactions
                .Include(t => t.Destination)
                .Include(t => t.Employee)
                .Include(t => t.Origin)
                .Include(t => t.TransactionStatus)
                .Include(t => t.TransactionType)
                .Where(t => t.TransactionStatusID == 2 && t.TransactionTypeID == 2)
                .AsNoTracking();

            if (TransactionTypeID.HasValue)
            {
                inventory = inventory.Where(p => p.TransactionTypeID == TransactionTypeID);
                ViewData["Filtering"] = "btn-danger";
            }
            if (DestinationID.HasValue)
            {
                inventory = inventory.Where(p => p.DestinationID == DestinationID);
                ViewData["Filtering"] = "btn-danger";
            }
            if (!String.IsNullOrEmpty(SearchString))
            {
                inventory = inventory.Where(p => p.Description.ToUpper().Contains(SearchString.ToUpper()));
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
            if (sortField == "Type")
            {
                if (sortDirection == "asc")
                {
                    inventory = inventory
                        .OrderBy(p => p.TransactionType.Name);
                }
                else
                {
                    inventory = inventory
                        .OrderByDescending(p => p.TransactionType.Name);
                }
            }
            else if (sortField == "Description")
            {
                if (sortDirection == "asc")
                {
                    inventory = inventory
                        .OrderByDescending(p => p.Description);
                }
                else
                {
                    inventory = inventory
                        .OrderBy(p => p.Description);
                }
            }
            else if (sortField == "Origin")
            {
                if (sortDirection == "asc")
                {
                    inventory = inventory
                        .OrderBy(p => p.Origin.Name);
                }
                else
                {
                    inventory = inventory
                        .OrderByDescending(p => p.Origin.Name);
                }
            }
            else if (sortField == "Destination")
            {
                if (sortDirection == "asc")
                {
                    inventory = inventory
                        .OrderBy(p => p.Destination.Name);
                }
                else
                {
                    inventory = inventory
                        .OrderByDescending(p => p.Destination.Name);
                }
            }
            else if (sortField == "Transaction Date")
            {
                if (sortDirection == "asc")
                {
                    inventory = inventory
                        .OrderBy(p => p.TransactionDate);
                }
                else
                {
                    inventory = inventory
                        .OrderByDescending(p => p.TransactionDate);
                }
            }
            else //Sorting by Name
            {
                if (sortDirection == "asc")
                {
                    inventory = inventory
                        .OrderBy(p => p.TransactionStatus.Name);
                }
                else
                {
                    inventory = inventory
                        .OrderByDescending(p => p.TransactionStatus.Name);
                }
            }

            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;
            //SelectList for Sorting Options
            ViewBag.sortFieldID = new SelectList(sortOptions, sortField.ToString());

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, "IncomingTransactions");
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<Transaction>.CreateAsync(inventory.AsNoTracking(), page ?? 1, pageSize);


            return View(pagedData);
        }
        private bool TransactionExists(int id)
        {
          return _context.Transactions.Any(e => e.ID == id);
        }

        private void ViewDataReturnURL()
        {
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, ControllerName());
        }

        private SelectList DestinationSelectList(int? selectedId)
        { 
            return new SelectList(_context
                .Branches
                .OrderBy(m => m.Name), "ID", "Name", selectedId);
        }
        private SelectList EmployeeList(int? selectedId)
        {
           // var userId = _userManager.GetUserAsync(User);

            //selectedId = selectedId != null ? userId: selectedId;
            
            return new SelectList(_context
                .Employees
                .OrderBy(m => m.FirstName), "ID", "FirstName", selectedId);
        }
        private SelectList OriginList(int? selectedId)
        {
            return new SelectList(_context
                .Branches
                .OrderBy(m => m.Name), "ID", "Name", selectedId);
        }
        private SelectList TransactionStatusList(int? selectedId)
        {
            return new SelectList(_context
                .TransactionStatuses
                .OrderBy(m => m.Name), "ID", "Name", selectedId);
        }

        private SelectList TransactionTypeList(int? selectedId)
        {
            var a = _context.TransactionTypes.Select(s => new
            {
                s.ID,
                Name = s.Name + " - Stock " + s.InOut.ToString(),
                Name2 = s.Name
            });
            return new SelectList(a
                .OrderBy(s => s.Name2), "ID", "Name", selectedId);
            /*return new SelectList(_context
                .TransactionTypes
                .OrderBy(m => m.Name), "ID", "Name", selectedId);*/
        }
        
        private void PopulateDropDownLists(Transaction transaction = null)
        {
            ViewData["DestinationID"] = DestinationSelectList(transaction?.DestinationID);
            ViewData["EmployeeID"] = EmployeeList(transaction?.EmployeeID);
            ViewData["OriginID"] = OriginList(transaction?.OriginID);
            ViewData["TransactionStatusID"] = TransactionStatusList(transaction?.TransactionStatusID);
            ViewData["TransactionTypeID"] = TransactionTypeList(transaction?.TransactionTypeID);
        }
        
    }
}
