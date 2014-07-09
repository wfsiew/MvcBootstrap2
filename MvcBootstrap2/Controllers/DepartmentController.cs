using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcBootstrap2.Models;
using MvcBootstrap2.Helper;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using PagedList;

namespace MvcBootstrap2.Controllers
{
    public class DepartmentController : Controller
    {
        public const string MENU = "Department";

        //
        // GET: /Department/

        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.menu = MENU;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = string.IsNullOrEmpty(sortOrder) ? "Name_desc" : "";
            ViewBag.BudgetSortParm = sortOrder == "Budget" ? "Budget_desc" : "Budget";
            ViewBag.DateSortParm = sortOrder == "Date" ? "Date_desc" : "Date";
            ViewBag.AdminSortParm = sortOrder == "Admin" ? "Admin_desc" : "Admin";

            if (searchString != null)
                page = 1;

            else
                searchString = currentFilter;

            string keyword = string.IsNullOrEmpty(searchString) ? null : searchString.ToUpper();

            ViewBag.CurrentFilter = searchString;
            MongoCursor<Department> c = null;
            IOrderedEnumerable<Department> el = null;
            var departments = Department.GetCollection();

            if (!string.IsNullOrEmpty(keyword))
            {
                var qi = Query<Instructor>.Where(x => x.FirstMidName.Contains(keyword) ||
                    x.LastName.Contains(keyword));
                var instructors = DbHelper.Db.GetCollection<Instructor>("instructors");
                var i = instructors.Find(qi);
                var iid = i.Select(x => x.Id);

                var q = Query<Department>.Where(x => x.Name.Contains(keyword) ||
                    iid.Contains(x.PersonId));
                c = departments.Find(q);
            }

            else
                c = departments.FindAll();

            switch (sortOrder)
            {
                case "Name_desc":
                    el = c.OrderByDescending(x => x.Name);
                    break;

                case "Budget":
                    el = c.OrderBy(x => x.Budget);
                    break;

                case "Budget_desc":
                    el = c.OrderByDescending(x => x.Budget);
                    break;

                case "Date":
                    el = c.OrderBy(x => x.StartDate);
                    break;

                case "Date_desc":
                    el = c.OrderByDescending(x => x.StartDate);
                    break;

                case "Admin":
                    el = c.OrderBy(x => x.Administrator.LastName);
                    break;

                case "Admin_desc":
                    el = c.OrderByDescending(x => x.Administrator.LastName);
                    break;

                default:
                    el = c.OrderBy(x => x.Name);
                    break;
            }

            int pageSize = Constants.PAGE_SIZE;
            int pageNumber = (page ?? 1);

            return View(el.ToPagedList(pageNumber, pageSize));
        }

        //
        // GET: /Department/Details/5

        public ActionResult Details(string id)
        {
            ViewBag.menu = MENU;
            var departments = Department.GetCollection();
            var q = Query<Department>.EQ(x => x.Id, new ObjectId(id));
            Department department = departments.FindOne(q);
            if (department == null)
            {
                return HttpNotFound();
            }

            return View(department);
        }

        //
        // GET: /Department/Create

        public ActionResult Create()
        {
            ViewBag.menu = MENU;
            PopulateInstructorsDropDownList();
            return View();
        }

        //
        // POST: /Department/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DepartmentModel department)
        {
            ViewBag.menu = MENU;
            if (ModelState.IsValid)
            {
                Department o = new Department();
                o.Budget = department.Budget;
                o.Name = department.Name;
                o.PersonId = new ObjectId(department.PersonId);
                o.RowVersion = department.RowVersion;
                o.StartDate = department.StartDate;
                var departments = Department.GetCollection();
                departments.Insert(o);
                TempData["message"] = string.Format("{0} has been saved", o.Name);
                return RedirectToAction("Index");
            }

            PopulateInstructorsDropDownList(department.PersonId);
            return View(department);
        }

        //
        // GET: /Department/Edit/5

        public ActionResult Edit(string id)
        {
            ViewBag.menu = MENU;
            var departments = Department.GetCollection();
            var q = Query<Department>.EQ(x => x.Id, new ObjectId(id));
            Department department = departments.FindOne(q);
            if (department == null)
            {
                return HttpNotFound();
            }

            DepartmentModel o = new DepartmentModel();
            o.Budget = department.Budget;
            o.Id = department.Id.ToString();
            o.Name = department.Name;
            o.PersonId = department.PersonId.ToString();
            o.RowVersion = department.RowVersion;
            o.StartDate = department.StartDate;

            PopulateInstructorsDropDownList(department.PersonId);
            return View(o);
        }

        //
        // POST: /Department/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id, Name, Budget, StartDate, RowVersion, PersonId")] DepartmentModel department)
        {
            ViewBag.menu = MENU;
            try
            {
                if (ModelState.IsValid)
                {
                    var departments = Department.GetCollection();
                    var q = Query<Department>.EQ(x => x.Id, new ObjectId(department.Id));
                    Department o = departments.FindOne(q);
                    o.Budget = department.Budget;
                    o.Name = department.Name;
                    o.PersonId = new ObjectId(department.PersonId);
                    o.RowVersion = department.RowVersion;
                    o.StartDate = department.StartDate;
                    TempData["message"] = string.Format("{0} has been saved", o.Name);
                    return RedirectToAction("Index");
                }
            }

            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Unable to save changes. Try again, and if the problem persists contact your system administrator.");
            }

            PopulateInstructorsDropDownList(department.PersonId);
            return View(department);
        }

        //
        // GET: /Department/Delete/5

        public ActionResult Delete(string id, bool? concurrencyError)
        {
            ViewBag.menu = MENU;
            var departments = Department.GetCollection();
            var q = Query<Department>.EQ(x => x.Id, new ObjectId(id));
            Department department = departments.FindOne(q);
            if (concurrencyError.GetValueOrDefault())
            {
                if (department == null)
                {
                    ViewBag.ConcurrencyErrorMessage = "The record you attempted to delete "
                        + "was deleted by another user after you got the original values. "
                        + "Click the Back to List hyperlink.";
                }

                else
                {
                    ViewBag.ConcurrencyErrorMessage = "The record you attempted to delete "
                        + "was modified by another user after you got the original values. "
                        + "The delete operation was canceled and the current values in the "
                        + "database have been displayed. If you still want to delete this "
                        + "record, click the Delete button again. Otherwise "
                        + "click the Back to List hyperlink.";
                }
            }

            return View(department);
        }

        //
        // POST: /Department/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(string id)
        {
            ViewBag.menu = MENU;
            var departments = Department.GetCollection();
            var q = Query<Department>.EQ(x => x.Id, new ObjectId(id));
            try
            {
                departments.Remove(q);
                TempData["message"] = "Department was deleted";
                return RedirectToAction("Index");
            }

            catch (Exception)
            {
                Department department = departments.FindOne(q);
                ModelState.AddModelError(string.Empty, "Unable to delete. Try again, and if the problem persists contact your system administrator.");
                return View(department);
            }
        }

        private void PopulateInstructorsDropDownList(object selectedInstructor = null)
        {
            var instructors = Instructor.GetCollection();
            var c = instructors.FindAll();
            var instructorsQuery = c.OrderBy(x => x.LastName);
            ViewBag.PersonID_ = new SelectList(instructorsQuery, "Id", "FullName", selectedInstructor);
        }
	}
}