using DataModel;
using DataModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static System.Web.Razor.Parser.SyntaxConstants;
namespace Bankapp.Controllers
{
    public class BankController : Controller
    {
        // GET: Bank
          BankDbEntities db=new BankDbEntities();

        public object C_ID { get; private set; }

        //[HttpGet]
        //public ActionResult Login()
        //{
        //    return View();
        //}

        //[HttpPost]
        //public ActionResult Login(string Username, string Pwd)
        //{
        //    var user = db.LoginPages.FirstOrDefault(x => x.USERNAME == Username && x.Pwd == Pwd);

        //    if (user != null)
        //    {
        //        Session["Username"] = user.USERNAME;
        //        Session["Userrole"] = user.Userrole;
        //        return RedirectToAction("Index", "Home");
        //    }
        //    else
        //    {
        //        ViewBag.ErrorMessage = "Invalid Username or Password";
        //        return View();
        //    }
        public ActionResult Login()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        public ActionResult Login(string Role, string Username, string Password)
        {
            if (string.IsNullOrEmpty(Role) || string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                ViewBag.Message = "All fields are required!";
                return View();
            }

            var user = db.LoginPages.FirstOrDefault(u => u.USERNAME == Username && u.Pwd == Password && u.Userrole == Role);

            if (user == null)
            {
                ViewBag.Message = "Invalid username, password, or role!";
                return View();
            }

            
            Session["Username"] = user.USERNAME;
            Session["Role"] = user.Userrole;
            Session["UserID"] = user.USERNAME; 
            
            switch (Role)
            {
                case "Customer":
                    return RedirectToAction("CustomerHome");
                case "Employee":
                    return RedirectToAction("EmployeeHome");
                case "Manager":
                    return RedirectToAction("ManagerHome");
                default:
                    ViewBag.Message = "Invalid role selected!";
                    return View();
            }
        }



      
        public ActionResult Register()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Register(Customer customer)
        {
            if (!ModelState.IsValid)
            {
                return View(customer);
            }


            var existingUser = db.LoginPages.FirstOrDefault(u => u.USERNAME == customer.C_NAME);
            if (existingUser != null)
            {
                ModelState.AddModelError("C_NAME", "Username already exists. Please choose another.");
                return View(customer);
            }


            db.Customers.Add(customer);
            db.SaveChanges();


            db.LoginPages.Add(new LoginPage
            {
                USERNAME = customer.C_NAME,
                Pwd = customer.C_PASSWORD,
                Userrole = "Customer"
            });
            db.SaveChanges();

            TempData["SuccessMessage"] = $"Registration successful! Please login. Username: {customer.C_NAME}";
            return RedirectToAction("Register");
        }
            private string GenerateNewCustId()
        {
            var lastAccount = db.Customers
                .OrderByDescending(a => a.C_ID)
                .FirstOrDefault();

            int nextId = 1;

            //if (lastAccount != null)
            //{
            //    string lastId = lastAccount.C_ID(Substr);
            //    if (int.TryParse(lastId, out int parsed))
            //    {
            //        nextId = parsed + 1;
            //    }
            //}

            return "CU" + nextId.ToString("D3");
        }
        
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon(); 
            return RedirectToAction("Login");
        }
        //public ActionResult EmployeeHome()
        //{
        //    if (Session["Username"] == null || (string)Session["Role"] != "Employee")
        //    {
        //        return RedirectToAction("Login");
        //    }
        //    ViewBag.Username = Session["Username"];
        //    return View();
        //}
        public ActionResult EmployeeHome()
        {
            return View();
        }
        public ActionResult ManagerHome()
        {
            if (Session["Username"] == null || (string)Session["Role"] != "Manager")
            {
                return RedirectToAction("Login");
            }
            ViewBag.Username = Session["Username"];
            return View();
        }
        public ActionResult CustomerHome()
        {
            if (Session["Username"] == null || (string)Session["Role"] != "Customer")
            {
                return RedirectToAction("Login");
            }

            ViewBag.Username = Session["Username"];
            return View();
        }

    }
}
