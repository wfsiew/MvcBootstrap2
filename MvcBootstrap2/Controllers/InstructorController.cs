using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcBootstrap2.Models;
using MvcBootstrap2.Helper;
using MvcBootstrap2.ViewModels;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using PagedList;
using DoddleReport;
using DoddleReport.Web;

namespace MvcBootstrap2.Controllers
{
    public class InstructorController : Controller
    {
        public const string MENU = "Instructor";

        //
        // GET: /Instructor/

        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page, string id, string courseID)
        {
            ViewBag.menu = MENU;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = string.IsNullOrEmpty(sortOrder) ? "Name_desc" : "";
            ViewBag.FirstNameSortParm = sortOrder == "FirstName" ? "FirstName_desc" : "FirstName";
            ViewBag.DateSortParm = sortOrder == "Date" ? "Date_desc" : "Date";
            ViewBag.LocationSortParm = sortOrder == "Loc" ? "Loc_desc" : "Loc";

            InstructorIndexData viewModel = new InstructorIndexData();

            if (searchString != null)
                page = 1;

            else
                searchString = currentFilter;

            string keyword = string.IsNullOrEmpty(searchString) ? null : searchString.ToUpper();

            ViewBag.CurrentFilter = searchString;
            MongoCursor<Instructor> c = null;
            IOrderedEnumerable<Instructor> el = null;
            var instructors = Instructor.GetCollection();

            //var instructors = repository.GetInstructors()
            //    .Include(i => i.OfficeAssignment)
            //    .Include(i => i.Courses.Select(x => x.Department));

            if (!string.IsNullOrEmpty(keyword))
            {
                var q = Query<Instructor>.Where(x => x.LastName.ToUpper().Contains(keyword) ||
                    x.FirstMidName.ToUpper().Contains(keyword));
                c = instructors.Find(q);
            }

            else
                c = instructors.FindAll();

            if (!string.IsNullOrEmpty(id))
            {
                ViewBag.PersonID = id;
                var q = Query<Instructor>.EQ(x => x.Id, new ObjectId(id));
                viewModel.Courses = instructors.FindOne(q).Courses;
            }

            if (courseID != null)
            {
                ViewBag.CourseID = courseID;
                viewModel.Enrollments = viewModel.Courses.Where(x => x.Id.Equals(new ObjectId(courseID))).Single().Enrollments;
            }

            switch (sortOrder)
            {
                case "Name_desc":
                    el = c.OrderByDescending(x => x.LastName);
                    break;

                case "FirstName":
                    el = c.OrderBy(x => x.FirstMidName);
                    break;

                case "FirstName_desc":
                    el = c.OrderByDescending(x => x.FirstMidName);
                    break;

                case "Date":
                    el = c.OrderBy(x => x.HireDate);
                    break;

                case "Date_desc":
                    el = c.OrderByDescending(x => x.HireDate);
                    break;

                case "Loc":
                    el = c.OrderBy(x => x.OfficeAssignment.Location);
                    break;

                case "Loc_desc":
                    el = c.OrderByDescending(x => x.OfficeAssignment.Location);
                    break;

                default:
                    el = c.OrderBy(x => x.LastName);
                    break;
            }

            int pageSize = Constants.PAGE_SIZE;
            int pageNumber = (page ?? 1);

            viewModel.Instructors = el.ToPagedList(pageNumber, pageSize);

            return View(viewModel);
        }

        //
        // GET: /Instructor/Details/5

        public ActionResult Details(string id)
        {
            ViewBag.menu = MENU;
            var instructors = Instructor.GetCollection();
            var q = Query<Instructor>.EQ(x => x.Id, new ObjectId(id));
            Instructor instructor = instructors.FindOne(q);
            if (instructor == null)
            {
                return HttpNotFound();
            }

            return View(instructor);
        }

        //
        // GET: /Instructor/Create

        public ActionResult Create()
        {
            ViewBag.menu = MENU;
            PopulateAssignedCourseData();
            return View();
        }

        //
        // POST: /Instructor/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(InstructorModel instructor, FormCollection fc, string[] selectedCourses)
        {
            ViewBag.menu = MENU;

            if (TryUpdateModel(instructor, "",
                new string[] { "LastName", "FirstMidName", "HireDate", "OfficeAssignment" }))
            {
                Instructor o = new Instructor();
                o.FirstMidName = instructor.FirstMidName;
                o.HireDate = instructor.HireDate;
                o.LastName = instructor.LastName;

                if (selectedCourses == null)
                    o.CourseIdList = new List<ObjectId>();

                else
                    o.CourseIdList = selectedCourses.Select(k => new ObjectId(k)).ToList();

                var instructors = Instructor.GetCollection();
                instructors.Insert(o);

                var officeAssignments = DbHelper.Db.GetCollection<OfficeAssignment>("officeassignments");
                OfficeAssignment oa = new OfficeAssignment();
                oa.PersonId = o.Id;
                oa.Location = instructor.OfficeAssignment.Location;
                officeAssignments.Insert(oa);

                instructors.Save(o);
                TempData["message"] = string.Format("{0} has been saved", o.FullName);

                return RedirectToAction("Index");
            }

            PopulateAssignedCourseData();
            return View(instructor);
        }

        //
        // GET: /Instructor/Edit/5

        public ActionResult Edit(string id)
        {
            ViewBag.menu = MENU;
            var instructors = Instructor.GetCollection();
            var q = Query<Instructor>.EQ(x => x.Id, new ObjectId(id));
            Instructor instructor = instructors.FindOne(q);
            if (instructor == null)
            {
                return HttpNotFound();
            }

            PopulateAssignedCourseData(instructor);
            return View(instructor);
        }

        //
        // POST: /Instructor/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string id, FormCollection fc, string[] selectedCourses)
        {
            ViewBag.menu = MENU;
            var instructors = Instructor.GetCollection();
            var q = Query<Instructor>.EQ(x => x.Id, new ObjectId(id));
            Instructor instructorToUpdate = instructors.FindOne(q);
            InstructorModel m = new InstructorModel();

            if (TryUpdateModel(m, "",
                new string[] { "LastName", "FirstMidName", "HireDate", "OfficeAssignment" }))
            {
                try
                {
                    if (selectedCourses == null)
                        instructorToUpdate.CourseIdList = new List<ObjectId>();

                    else
                        instructorToUpdate.CourseIdList = selectedCourses.Select(k => new ObjectId(k)).ToList();

                    instructorToUpdate.LastName = m.LastName;
                    instructorToUpdate.FirstMidName = m.FirstMidName;
                    instructorToUpdate.HireDate = m.HireDate;

                    var officeAssignments = DbHelper.Db.GetCollection<OfficeAssignment>("officeassignments");
                    var qo = Query<OfficeAssignment>.EQ(x => x.PersonId, instructorToUpdate.Id);
                    OfficeAssignment oa = officeAssignments.FindOne(qo);

                    if (oa != null)
                    {
                        if (string.IsNullOrWhiteSpace(m.OfficeAssignment.Location))
                        {
                            officeAssignments.Remove(qo);
                        }

                        else
                        {
                            oa.Location = m.OfficeAssignment.Location;
                            officeAssignments.Save(oa);
                        }
                    }

                    else
                    {
                        if (!string.IsNullOrWhiteSpace(m.OfficeAssignment.Location))
                        {
                            oa = new OfficeAssignment();
                            oa.PersonId = instructorToUpdate.Id;
                            oa.Location = m.OfficeAssignment.Location;
                            officeAssignments.Insert(oa);
                        }
                    }

                    instructors.Save(instructorToUpdate);
                    TempData["message"] = string.Format("{0} has been saved", instructorToUpdate.FullName);

                    return RedirectToAction("Index");
                }

                catch (Exception)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }

            PopulateInstructorsDropDownList(instructorToUpdate.Id);
            PopulateAssignedCourseData(instructorToUpdate);
            return View(instructorToUpdate);
        }

        //
        // GET: /Instructor/Delete/5

        public ActionResult Delete(string id)
        {
            ViewBag.menu = MENU;
            var instructors = Instructor.GetCollection();
            var q = Query<Instructor>.EQ(x => x.Id, new ObjectId(id));
            Instructor instructor = instructors.FindOne(q);
            if (instructor == null)
            {
                return HttpNotFound();
            }

            return View(instructor);
        }

        //
        // POST: /Instructor/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            ViewBag.menu = MENU;
            var instructors = Instructor.GetCollection();
            var q = Query<Instructor>.EQ(x => x.Id, new ObjectId(id));
            instructors.Remove(q);
            TempData["message"] = "Instructor was deleted";
            return RedirectToAction("Index");
        }

        public ActionResult HtmlReport(string sortOrder, string currentFilter, string searchString)
        {
            Report report = GetReport(sortOrder, currentFilter, searchString);
            return new ReportResult(report);
        }

        public ActionResult PdfReport(string sortOrder, string currentFilter, string searchString)
        {
            Report report = GetReport(sortOrder, currentFilter, searchString);
            return new ReportResult(report, new DoddleReport.iTextSharp.PdfReportWriter(), "application/pdf");
        }

        public ActionResult ExcelReport(string sortOrder, string currentFilter, string searchString)
        {
            var report = GetReport(sortOrder, currentFilter, searchString);
            return new ReportResult(report, new DoddleReport.OpenXml.ExcelReportWriter(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }

        public ActionResult CsvReport(string sortOrder, string currentFilter, string searchString)
        {
            var report = GetReport(sortOrder, currentFilter, searchString);
            return new ReportResult(report, new DoddleReport.Writers.DelimitedTextReportWriter(), "text/plain;charset=UTF-8");
        }

        private void PopulateInstructorsDropDownList(object selectedInstructor = null)
        {
            var officeAssignments = DbHelper.Db.GetCollection<OfficeAssignment>("officeassignments");
            var c = officeAssignments.FindAll();
            var l = c.AsEnumerable();
            ViewBag.PersonID_ = new SelectList(l, "PersonId", "Location", selectedInstructor);
        }

        private void PopulateAssignedCourseData(Instructor instructor = null)
        {
            MongoCursor<Course> mc = Course.GetCollection().FindAll();
            List<Course> allCourses = mc.ToList();
            HashSet<ObjectId> instructorCourses = null;

            if (instructor != null)
                instructorCourses = new HashSet<ObjectId>(instructor.Courses.Select(c => c.Id));

            List<AssignedCourseData> viewModel = new List<AssignedCourseData>();
            foreach (Course course in allCourses)
            {
                viewModel.Add(new AssignedCourseData
                {
                    CourseID = course.CourseID,
                    CourseId = course.Id.ToString(),
                    Title = course.Title,
                    Assigned = instructor == null ? false : instructorCourses.Contains(course.Id)
                });
            }

            ViewBag.Courses = viewModel;
        }

        private Report GetReport(string sortOrder, string currentFilter, string searchString)
        {
            ViewBag.menu = MENU;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = string.IsNullOrEmpty(sortOrder) ? "Name_desc" : "";
            ViewBag.FirstNameSortParm = sortOrder == "FirstName" ? "FirstName_desc" : "FirstName";
            ViewBag.DateSortParm = sortOrder == "Date" ? "Date_desc" : "Date";
            ViewBag.LocationSortParm = sortOrder == "Loc" ? "Loc_desc" : "Loc";

            InstructorIndexData viewModel = new InstructorIndexData();

            if (searchString == null)
                searchString = currentFilter;

            string keyword = string.IsNullOrEmpty(searchString) ? null : searchString.ToUpper();

            ViewBag.CurrentFilter = searchString;
            MongoCursor<Instructor> c = null;
            IOrderedEnumerable<Instructor> el = null;
            var instructors = Instructor.GetCollection();

            //var instructors = repository.GetInstructors()
            //    .Include(i => i.OfficeAssignment)
            //    .Include(i => i.Courses.Select(x => x.Department));

            if (!string.IsNullOrEmpty(keyword))
            {
                var q = Query<Instructor>.Where(x => x.LastName.ToUpper().Contains(keyword) ||
                    x.FirstMidName.ToUpper().Contains(keyword));
                c = instructors.Find(q);
            }

            else
                c = instructors.FindAll();

            switch (sortOrder)
            {
                case "Name_desc":
                    el = c.OrderByDescending(x => x.LastName);
                    break;

                case "FirstName":
                    el = c.OrderBy(x => x.FirstMidName);
                    break;

                case "FirstName_desc":
                    el = c.OrderByDescending(x => x.FirstMidName);
                    break;

                case "Date":
                    el = c.OrderBy(x => x.HireDate);
                    break;

                case "Date_desc":
                    el = c.OrderByDescending(x => x.HireDate);
                    break;

                case "Loc":
                    el = c.OrderBy(x => x.OfficeAssignment.Location);
                    break;

                case "Loc_desc":
                    el = c.OrderByDescending(x => x.OfficeAssignment.Location);
                    break;

                default:
                    el = c.OrderBy(x => x.LastName);
                    break;
            }

            var lr = el.Select(x => new
            {
                LastName = x.LastName,
                FirstName = x.FirstMidName,
                HireDate = x.HireDate,
                Office = x.OfficeAssignment,
                Courses = x.GetCourses()
            });
            Report report = new Report(lr.ToReportSource());
            report.TextFields.Title = "Instructors Report";
            report.TextFields.Header = string.Format(@"Report Generated: {0} Total Instructors: {1}", DateTime.Now, c.Count());

            report.RenderHints.FreezeRows = 4;

            report.RenderingRow += report_RenderingRow;

            return report;
        }

        private void report_RenderingRow(object sender, ReportRowEventArgs e)
        {
        }
	}
}