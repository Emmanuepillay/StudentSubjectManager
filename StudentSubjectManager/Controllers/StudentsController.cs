using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StudentSubjectManager.Controllers
{
    [Authorize]
    public class StudentsController : Controller
    {
        public StudentsController()
        {
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int id)
        {
            ViewBag.StudentId = id;
            return View();
        }
        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Edit(int id)
        {
            ViewBag.StudentId = id;
            return View();
        }

        public IActionResult Delete(int id)
        {
            ViewBag.StudentId = id;
            return View();
        }

    }
}
