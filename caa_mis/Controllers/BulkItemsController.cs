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
using DNTBreadCrumb.Core;

namespace caa_mis.Controllers
{
    [BreadCrumb(Title = "Inventory Reset", Order = 0, IgnoreAjaxRequests = true, Url = "Bulks")]
    [BreadCrumb(Title = "Inventory Reset Details", UseDefaultRouteUrl = true, Order = 0, IgnoreAjaxRequests = true)]

    public class BulkItemsController : CustomControllers.CognizantController
    {
        private readonly InventoryContext _context;

        public BulkItemsController(InventoryContext context)
        {
            _context = context;
        }

        // GET: BulkItems
        public async Task<IActionResult> Index(int? BulkID, string sortDirectionCheck, string sortFieldID, int? ItemID,
            int? page, int? pageSizeID, string actionButton, string sortDirection = "asc", string sortField = "Product Name")
        {
            //Clear the sort/filter/paging URL Cookie for Controller
            CookieHelper.CookieSet(HttpContext, ControllerName() + "URL", "", -1);

            //Change colour of the button when filtering by setting this default
            ViewData["Filtering"] = "btn-outline-primary";

            ViewDataReturnURL();

            if (!BulkID.HasValue)
            {
                //Go back to the proper return URL for the Transactions controller
                return Redirect(ViewData["returnURL"].ToString());
            }

            //List of sort options.
            //NOTE: make sure this array has matching values to the column headings
            string[] sortOptions = new[] { "Product Name", "Quantity" };

            PopulateDropDownLists();
            
            var item = from a in _context.BulkItems
                .Include(b => b.Bulk)
                .Include(t => t.Item)
                       where a.BulkID == BulkID.GetValueOrDefault()
                       select a;

            var transactions = _context.Bulks
                .Include(b => b.Branch)
                .Include(b => b.Employee)
                .Include(b => b.TransactionStatus)
                .Where(p => p.ID == BulkID.GetValueOrDefault())
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
            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, "BulkItems");
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<BulkItem>.CreateAsync(item.AsNoTracking(), page ?? 1, pageSize);



            return View(pagedData);
        }

        // GET: BulkItems/Details/5
        [BreadCrumb(Title = "Details", Order = 1, IgnoreAjaxRequests = true)]
        public async Task<IActionResult> Details(int? id)
        {
            ViewDataReturnURL();
            
            if (id == null || _context.BulkItems == null)
            {
                return NotFound();
            }

            var bulkItem = await _context.BulkItems
                .Include(b => b.Bulk)
                .Include(b => b.Item)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (bulkItem == null)
            {
                return NotFound();
            }

            return View(bulkItem);
        }

        // GET: BulkItems/Create
        [BreadCrumb(Title = "Create", Order = 1, IgnoreAjaxRequests = true)]
        public IActionResult Create()
        {
           
            PopulateDropDownLists();
            
            return View();
        }

        // POST: BulkItems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,ProductID,TransactionID,Quantity")] TransactionItemVM transactionItem)
        {
            ViewDataReturnURL();
            
            BulkItem bI = new BulkItem
            {
                ID = transactionItem.ID,
                BulkID = transactionItem.TransactionID,
                ItemID = transactionItem.ProductID,
                Quantity = transactionItem.Quantity
            };
            var itemExists = _context.BulkItems
                .Where(p => p.BulkID == transactionItem.TransactionID && p.ItemID == transactionItem.ProductID)
                .FirstOrDefault();

            if (itemExists == null)
            {
                if (transactionItem.Quantity > 0)
                {
                    if (ModelState.IsValid)
                    {
                        _context.Add(bI);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
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

            PopulateDropDownLists(bI);
            return Redirect(ViewData["returnURL"].ToString());
        }

        // GET: BulkItems/Edit/5
        [BreadCrumb(Title = "Edit", Order = 1, IgnoreAjaxRequests = true)]
        public async Task<IActionResult> Edit(int? id)
        {
            ViewDataReturnURL();
            
            if (id == null || _context.BulkItems == null)
            {
                return NotFound();
            }

            var bulkItem = await _context.BulkItems.FindAsync(id);
            if (bulkItem == null)
            {
                return NotFound();
            }
            PopulateDropDownLists(bulkItem);
            return View(bulkItem);
        }

        // POST: BulkItems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,ItemID,BulkID,Quantity")] BulkItem bulkItem)
        {
            ViewDataReturnURL();
            
            if (id != bulkItem.ID)
            {
                return NotFound();
            }
            var itemExists = _context.BulkItems
               .Where(p => p.BulkID == bulkItem.BulkID && p.ItemID == bulkItem.ItemID)
               .FirstOrDefault();

            if (itemExists == null)
            {
                if (bulkItem.Quantity > 0)
                {
                    if (ModelState.IsValid)
                    {
                        try
                        {
                            _context.Update(bulkItem);
                            await _context.SaveChangesAsync();
                            return Redirect(ViewData["returnURL"].ToString());
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            if (!BulkItemExists(bulkItem.ID))
                            {
                                return NotFound();
                            }
                            else
                            {
                                throw;
                            }
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
            }
            PopulateDropDownLists(bulkItem);
            return View(bulkItem);
        }

        // GET: BulkItems/Delete/5
        [BreadCrumb(Title = "Delete", Order = 1, IgnoreAjaxRequests = true)]
        public async Task<IActionResult> Delete(int? id)
        {
            ViewDataReturnURL();
            
            if (id == null || _context.BulkItems == null)
            {
                return NotFound();
            }

            var bulkItem = await _context.BulkItems
                .Include(b => b.Bulk)
                .Include(b => b.Item)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (bulkItem == null)
            {
                return NotFound();
            }

            return View(bulkItem);
        }

        // POST: BulkItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.BulkItems == null)
            {
                return Problem("Entity set 'InventoryContext.BulkItems'  is null.");
            }
            var bulkItem = await _context.BulkItems.FindAsync(id);
            if (bulkItem != null)
            {
                _context.BulkItems.Remove(bulkItem);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BulkItemExists(int id)
        {
          return _context.BulkItems.Any(e => e.ID == id);
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

        private SelectList TransactionList(int? selectedId)
        {
            return new SelectList(_context
                .Bulks
                .OrderBy(m => m.Branch.Name), "ID", "Name", selectedId);
        }
        private void PopulateDropDownLists(BulkItem bulkItem = null)
        {
            ViewData["ItemID"] = ItemList(bulkItem?.ItemID);
            ViewData["BulkID"] = TransactionList(bulkItem?.BulkID);

        }
    }
}
