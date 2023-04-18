using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using caa_mis.Data;
using caa_mis.Models;
using caa_mis.ViewModels;
using caa_mis.Utilities;
using Newtonsoft.Json;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Drawing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.CodeAnalysis.Operations;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using DNTBreadCrumb.Core;
using System.Security.Policy;

namespace caa_mis.Controllers
{
    [Authorize(Roles = "Admin, Supervisor")]
    [BreadCrumb(Title = "Transfers", Order = 0, IgnoreAjaxRequests = true, Url = "Transactions")]
    [BreadCrumb(Title = "Transfer Details", UseDefaultRouteUrl = true, Order = 0, IgnoreAjaxRequests = true)]
    public class TransactionItemsController : CustomControllers.CognizantController
    {
        private readonly InventoryContext _context;

        public TransactionItemsController(InventoryContext context)
        {
            _context = context;
        }

        // GET: TransactionItems
        public async Task<IActionResult> Index(int? TransactionID, string sortDirectionCheck, string sortFieldID, int? ItemID,
            int? page, int? pageSizeID, string actionButton, string sortDirection = "asc", string sortField = "Product Name")
        {
            //Clear the sort/filter/paging URL Cookie for Controller
            CookieHelper.CookieSet(HttpContext, ControllerName() + "URL", "", -1);

            //Change colour of the button when filtering by setting this default
            ViewData["Filtering"] = "btn-outline-primary";

            ViewDataReturnURL();

            if (!TransactionID.HasValue)
            {
                //Go back to the proper return URL for the Transactions controller
                return Redirect(ViewData["returnURL"].ToString());
            }

            //Change colour of the button when filtering by setting this default
            ViewData["Filtering"] = "btn-danger";

            //List of sort options.
            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptions = new[] { "Product Name", "Quantity", "Quantity Received" };

            PopulateDropDownLists();
            

            var item = from a in _context.TransactionItems
                .Include(t => t.Item)
                where a.TransactionID == TransactionID.GetValueOrDefault()
                select a;


            var transactions = _context.Transactions
                .Include(t => t.Destination)
                .Include(t => t.Employee)
                .Include(t => t.Origin)
                .Include(t => t.TransactionStatus)
                .Include(t => t.TransactionType)
                .Where(p => p.ID == TransactionID.GetValueOrDefault())
                .AsNoTracking()
                .FirstOrDefault();

            ViewBag.Transactions = transactions;

            ViewData["ProductID"] = ItemListPerBranch(transactions.OriginID);

            if (ItemID.HasValue)
            {
                item = item.Where(p => p.ItemID == ItemID);
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
            if (sortField == "Quantity")
            {
                if (sortDirection == "asc")
                {
                    item = item
                        .OrderBy(p => p.Quantity);
                }
                else
                {
                    item = item
                        .OrderByDescending(p => p.Quantity);
                }
            }
            if(sortField == "Quantity Received")
            {
                if (sortDirection == "asc")
                {
                    item = item
                        .OrderBy(p => p.ReceivedQuantity);
                }
                else
                {
                    item = item
                        .OrderByDescending(p => p.ReceivedQuantity);
                }
            }
            else //Sorting by Name
            {
                if (sortDirection == "asc")
                {
                    item = item
                        .OrderBy(p => p.Item.Name);
                }
                else
                {
                    item = item
                        .OrderByDescending(p => p.Item.Name);
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
            var pagedData = await PaginatedList<TransactionItem>.CreateAsync(item.AsNoTracking(), page ?? 1, pageSize);



            return View(pagedData);
        }

        // GET: TransactionItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            ViewDataReturnURL();

            if (id == null || _context.TransactionItems == null)
            {
                return Redirect(ViewData["returnURL"].ToString());
            }

            var transactionItem = await _context.TransactionItems
                .Include(t => t.Item)
                .Include(t => t.Transaction)
                .FirstOrDefaultAsync(t => t.ID == id);
            if (transactionItem == null)
            {
                return NotFound();
            }

            return View(transactionItem);
        }

        // GET: TransactionItems/Create
        public IActionResult Create(int? TransactionID, string TransactionName)
        {
            ViewDataReturnURL();

            if (!TransactionID.HasValue)
            {
                return Redirect(ViewData["returnURL"].ToString());
            }
            ViewData["TransactionName"] = TransactionName;

            TransactionItem a = new()
            {
                TransactionID = TransactionID.GetValueOrDefault()
            };

            PopulateDropDownLists();
            return View(a);
        }

        // POST: TransactionItems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,ProductID,TransactionID,Quantity")] TransactionItemVM transactionItem)
        {
            ViewDataReturnURL();
            

            TransactionItem tI = new TransactionItem
            {
                ID = transactionItem.ID,
                TransactionID = transactionItem.TransactionID,
                ItemID = transactionItem.ProductID,
                Quantity = transactionItem.Quantity,
                ReceivedQuantity = transactionItem.Quantity
            };

            var itemExists = _context.TransactionItems
                .Where(p => p.TransactionID == transactionItem.TransactionID && p.ItemID == transactionItem.ProductID)
                .FirstOrDefault();

            if (itemExists == null)
            {
                if (transactionItem.Quantity > 0)
                {
                    if (validateOnHand(transactionItem.TransactionID, transactionItem.ProductID, transactionItem.Quantity))
                    {
                        if (ModelState.IsValid)
                        {
                            _context.Add(tI);
                            await _context.SaveChangesAsync();
                            return Redirect(ViewData["returnURL"].ToString());
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "The changes cannot be saved because the quantity entered is higher than the available stock in the branch.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "The changes cannot be saved. Quantity cannot be negative or 0.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "The changes cannot be saved. There is already an existing product in your list.";
            }

            PopulateDropDownLists(tI);
            return Redirect(ViewData["returnURL"].ToString());
        }

        // GET: TransactionItems/Edit/5
        [BreadCrumb(Title = "Edit", Order = 1, IgnoreAjaxRequests = true)]
        public async Task<IActionResult> Edit(int? id)
        {
            ViewDataReturnURL();

            if (id == null || _context.TransactionItems == null)
            {
                return NotFound();
            }

            var transactionItem = await _context.TransactionItems.Include(p=>p.Item).FirstOrDefaultAsync(p => p.ID == id);
            if (transactionItem == null)
            {
                return NotFound();
            }

            PopulateDropDownLists(transactionItem);

            return View(transactionItem);
        }

        // GET: TransactionItems/Edit/5
        public async Task<IActionResult> EditIncoming(int? id)
        {
            ViewDataReturnURL();

            if (id == null || _context.TransactionItems == null)
            {
                return NotFound();
            }

            var transactionItem = await _context.TransactionItems.FindAsync(id);
            if (transactionItem == null)
            {
                return NotFound();
            }

            PopulateDropDownLists(transactionItem);

            return View(transactionItem);
        }

        // POST: TransactionItems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,ItemID,TransactionID,Quantity")] TransactionItem transactionItem)
        {
            ViewDataReturnURL();

            if (id != transactionItem.ID)
            {
                return NotFound();
            }

            TransactionItem tI = new TransactionItem
            {
                ID = transactionItem.ID,
                TransactionID = transactionItem.TransactionID,
                ItemID = transactionItem.ItemID,
                Quantity = transactionItem.Quantity,
                ReceivedQuantity = transactionItem.Quantity
            };
            if (validateOnHand(transactionItem.TransactionID, transactionItem.ItemID, transactionItem.Quantity))
            {
                if (transactionItem.Quantity > 0)
                {
                    if (ModelState.IsValid)
                    {
                        try
                        {
                            _context.Update(tI);
                            await _context.SaveChangesAsync();
                            return Redirect(ViewData["returnURL"].ToString());
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            if (!TransactionItemExists(transactionItem.ID))
                            {
                                return NotFound();
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("", "The changes cannot be saved. Quantity cannot be negative or 0.");
                }
            }
            else
            {
                ModelState.AddModelError("", "The changes cannot be saved because the quantity entered is higher than the available stock in the branch.");
            }

            PopulateDropDownLists(transactionItem);

            return View(transactionItem);
        }
        // POST: TransactionItems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditIncoming(int id, [Bind("ID,ItemID,TransactionID,Quantity,ReceivedQuantity")] TransactionItem transactionItem)
        {
            ViewDataReturnURL();

            if (id != transactionItem.ID)
            {
                return NotFound();
            }
            
            transactionItem.IsEdited = true;
            
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transactionItem);
                    await _context.SaveChangesAsync();
                    
                    return RedirectToAction("Incoming", new { TransactionID = transactionItem.TransactionID });
                    //return Redirect(ViewData["returnURL"].ToString());
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionItemExists(transactionItem.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            PopulateDropDownLists(transactionItem);

            return View(transactionItem);
        }
        // GET: TransactionItems/Delete/5
        [BreadCrumb(Title = "Delete", Order = 1, IgnoreAjaxRequests = true)]
        public async Task<IActionResult> Delete(int? id)
        {
            ViewDataReturnURL();

            if (id == null || _context.TransactionItems == null)
            {
                return NotFound();
            }

            var transactionItem = await _context.TransactionItems
                .Include(t => t.Item)
                .Include(t => t.Transaction)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (transactionItem == null)
            {
                return NotFound();
            }

            return View(transactionItem);
        }

        // POST: TransactionItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            ViewDataReturnURL();
            if (_context.TransactionItems == null)
            {
                return Problem("Entity set 'InventoryContext.TransactionItems'  is null.");
            }
            var transactionItem = await _context.TransactionItems.FindAsync(id);

            if (transactionItem != null)
            {
                try
                {
                    _context.TransactionItems.Remove(transactionItem);
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem " +
                        "persists see your system administrator.");
                }
            }

            return View(transactionItem);
        }
        
        public bool validateOnHand(int TransactionID, int ProductID, int Quantity)
        {
            //validate quantity vs the item
            var trans = _context.Transactions
                .FirstOrDefault(p => p.ID == TransactionID);

            if(trans.OriginID == 1)
            {
                return true;
            }
            var currentOnHand = _context.Stocks
                .Where(p => p.ItemID == ProductID && p.BranchID == trans.OriginID)
                .FirstOrDefault();

            if(currentOnHand == null)
            {
                return false;
            }
            
            return currentOnHand.Quantity >= Quantity;
        }
        [Produces("application/json")]
        public IActionResult SearchProduct(int branchID, string term = null)
        {

            IQueryable<ProductListVM> sumQ = _context.ProductList.OrderBy(p=>p.Name);
            IQueryable<Item> items = _context.Items.Where(p => p.ItemStatusID == 1).OrderBy(p => p.Name);

            IQueryable<Item> result;
            IQueryable<ProductListVM> result2;
            //head office or others
            if (branchID == 1 && term != null)
            {
                result = items
                .AsNoTracking()
                .Where(p => p.Name.ToUpper().Contains(term.ToUpper()) || p.SKUNumber.Contains(term));

                return Json(result);
            }
            else if (branchID == 1 && term == null)
            {
                result = items
                .AsNoTracking();
                
                return Json(result);
            }
            
            if (term != null)
            {
                result2 = sumQ
                 .AsNoTracking()
                 .Where(p => p.BranchID == branchID && p.Name.ToUpper().Contains(term.ToUpper()) || p.SKUNumber.Contains(term));
            }
            else
            {
                result2 = sumQ
               .AsNoTracking()
               .Where(p => p.BranchID == branchID);
                
            }
               
            return Json(result2);
        }

        [Produces("application/json")]
        public IActionResult SearchAllProduct(string term = null)
        {
            IQueryable<Item> result;
            if (term != null)
            {
                result = _context.Items
                .OrderBy(p => p.Name)
                .AsNoTracking()
                .Where(p => p.Name.ToUpper().Contains(term.ToUpper()) || p.SKUNumber.Contains(term));
            }
            else
            {
                result = _context.Items
                .OrderBy(p => p.Name)
               .AsNoTracking();
            }

            return Json(result);
        }

       


        //Incoming stock items
        public async Task<IActionResult> Incoming(int? TransactionID, string sortDirectionCheck, string sortFieldID, int? ItemID,
            int? page, int? pageSizeID, string actionButton, string sortDirection = "asc", string sortField = "Product Name")
        {
            //Clear the sort/filter/paging URL Cookie for Controller
            CookieHelper.CookieSet(HttpContext, ControllerName() + "URL", "", -1);

            ViewDataReturnURL();

            if (!TransactionID.HasValue)
            {
                //Go back to the proper return URL for the Transactions controller
                return Redirect(ViewData["returnURL"].ToString());
            }

            //List of sort options.
            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptions = new[] { "Product Name", "Quantity" };

            PopulateDropDownLists();

            var item = from a in _context.TransactionItems
                .Include(t => t.Item)
                where a.TransactionID == TransactionID.GetValueOrDefault()
                select a;


            var transactions = _context.Transactions
                .Include(t => t.Destination)
                .Include(t => t.Employee)
                .Include(t => t.Origin)
                .Include(t => t.TransactionStatus)
                .Include(t => t.TransactionType)
                .Where(p => p.ID == TransactionID.GetValueOrDefault())
                .AsNoTracking()
                .FirstOrDefault();

            ViewBag.Transactions = transactions;

            if (ItemID.HasValue)
            {
                item = item.Where(p => p.ItemID == ItemID);
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
            if (sortField == "Quantity")
            {
                if (sortDirection == "asc")
                {
                    item = item
                        .OrderBy(p => p.Quantity);
                }
                else
                {
                    item = item
                        .OrderByDescending(p => p.Quantity);
                }
            }
            else //Sorting by Name
            {
                if (sortDirection == "asc")
                {
                    item = item
                        .OrderBy(p => p.Item.Name);
                }
                else
                {
                    item = item
                        .OrderByDescending(p => p.Item.Name);
                }
            }
            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;
            //SelectList for Sorting Options
            ViewBag.sortFieldID = new SelectList(sortOptions, sortField.ToString());

            //Handle Paging
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, "IncomingItems");
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<TransactionItem>.CreateAsync(item.AsNoTracking(), page ?? 1, pageSize);



            return View(pagedData);
        }
        private bool TransactionItemExists(int id)
        {
          return _context.TransactionItems.Any(e => e.ID == id);
        }

        private void ViewDataReturnURL()
        {
            ViewData["returnURL"] = MaintainURL.ReturnURL(HttpContext, ControllerName());
        }

        private SelectList ItemList(int? selectedId)
        {
            return new SelectList(_context
                .Items
                .OrderBy(m => m.Name), "ID", "Name", selectedId);
        }

        private SelectList ItemListPerBranch(int? branchID)
        {
            IQueryable<ProductListVM> sumQ = _context.ProductList;

            return new SelectList(
                sumQ
                .Where(p => p.BranchID == branchID)
                .OrderBy(m => m.Name), "ID", "ProductName");
        }
        private SelectList TransactionList(int? selectedId)
        {
            return new SelectList(_context
                .Transactions
                .OrderBy(m => m.Description), "ID", "Name", selectedId);
        }
        private void PopulateDropDownLists(TransactionItem transactionItem = null)
        {
            ViewData["ItemID"] = ItemList(transactionItem?.ItemID);
            ViewData["TransactionID"] = TransactionList(transactionItem?.TransactionID);
        }
        private SelectList BranchList(int[] selectedId)
        {
            return new SelectList(_context.Branches
                .OrderBy(d => d.Name), "ID", "Name", selectedId);
        }

    }
}
