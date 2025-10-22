using DataModel.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Bankapp.Controllers
{
    public class EmployeeController : Controller
    {
       
        BankDbEntities db = new BankDbEntities();

        public ActionResult OpenAccount()
        {
            return View();
        }

       
        public ActionResult ViewTransactionsEmp()
        {
            return View();
        }

       
        public ActionResult SavingsAccount()
        {
            return View();
        }

        
        public ActionResult FDAccount()
        {
            return View();
        }

        
        [HttpPost]
        public ActionResult CheckCustomer(string pan)
        {
            var customer = db.Customers.FirstOrDefault(c => c.C_PAN == pan);

            if (customer != null)
            {
                Session["CustomerID"] = customer.C_ID;
                return RedirectToAction("CreateSavingsAccount");
            }
            else
            {
                TempData["Error"] = "Customer not found. Please register first.";
                return RedirectToAction("Register", "Bank");
            }
        }

        
        public ActionResult CreateSavingsAccount()
        {
            if (Session["CustomerID"] == null)
            {
                TempData["ErrorMessage"] = "Customer not selected.";
                return RedirectToAction("OpenAccount");
            }

            int customerId = (int)Session["CustomerID"];
            ViewBag.CustomerID = customerId;
            return View();
        }

       
        [HttpPost]
        public ActionResult CreateSavingsAccount(decimal SB_BALANCE)
        {
            if (Session["CustomerID"] == null)
            {
                TempData["ErrorMessage"] = "Customer not logged in.";
                return RedirectToAction("CreateSavingsAccount");
            }

            int customerId = (int)Session["CustomerID"];
            string newAccountId = GenerateNewAccountId();

            var account = new Savings_Account
            {
                SB_ACCOUNT_ID = newAccountId,
                SB_CUSTOMER_ID = customerId,
                SB_BALANCE = SB_BALANCE
            };

            if (ModelState.IsValid)
            {
                db.Savings_Account.Add(account);
                db.SaveChanges();

                TempData["SuccessMessage"] = $"Savings Account created successfully! Account ID: {newAccountId}";
                return RedirectToAction("CreateSavingsAccount");
            }

            TempData["ErrorMessage"] = "Failed to create account.";
            return RedirectToAction("CreateSavingsAccount");
        }

      
        private string GenerateNewAccountId()
        {
            var lastAccount = db.Savings_Account
                .OrderByDescending(a => a.SB_ACCOUNT_ID)
                .FirstOrDefault();

            int nextId = 1;

            if (lastAccount != null)
            {
                string lastId = lastAccount.SB_ACCOUNT_ID.Substring(2);
                if (int.TryParse(lastId, out int parsed))
                {
                    nextId = parsed + 1;
                }
            }

            return "SB" + nextId.ToString("D3");
        }

        
        [HttpGet]
        public ActionResult CloseAccount()
        {
            ViewBag.SavingsAccounts = db.Savings_Account.ToList();
            return View();
        }

        
        [HttpPost]
        public ActionResult CloseSavingsAccount(string accountId)
        {
            if (string.IsNullOrEmpty(accountId))
            {
                TempData["Message"] = "Invalid account id.";
                return RedirectToAction("CloseAccount");
            }

            var acc = db.Savings_Account.FirstOrDefault(a => a.SB_ACCOUNT_ID == accountId);
            if (acc != null)
            {
                db.Savings_Account.Remove(acc);
                db.SaveChanges();
                TempData["Message"] = "Savings account closed successfully!";
            }
            else
            {
                TempData["Message"] = "Savings account not found.";
            }

            return RedirectToAction("CloseAccount");
        }

        
        [HttpPost]
        public ActionResult CheckCustomers(string pan)
        {
            var customer = db.Customers.FirstOrDefault(c => c.C_PAN == pan);

            if (customer != null)
            {
                Session["CustomerID"] = customer.C_ID;
                return RedirectToAction("CreateFDAccount");
            }
            else
            {
                TempData["Error"] = "Customer not found. Please register first.";
                return RedirectToAction("Register", "Bank");
            }
        }

        
        [HttpGet]
        public ActionResult CreateFDAccount()
        {
            ViewBag.Customers = db.Customers.ToList();
            return View();
        }

        
        [HttpPost]
        public ActionResult CreateFDAccount(int customerId, decimal amount, int tenure)
        {
            try
            {
                var customer = db.Customers.Find(customerId);
                if (customer == null)
                {
                    TempData["ErrorMessage"] = "Customer not found!";
                    return RedirectToAction("CreateFDAccount");
                }

                if (amount <= 0)
                {
                    TempData["ErrorMessage"] = "Amount must be greater than zero.";
                    return RedirectToAction("CreateFDAccount");
                }

                if (tenure <= 0)
                {
                    TempData["ErrorMessage"] = "Tenure must be greater than zero.";
                    return RedirectToAction("CreateFDAccount");
                }

                var fd = new Fixed_Deposit_Account
                {
                    FD_ACCOUNT_ID = GenerateFDAccountId(),
                    FD_CUSTOMER_ID = customerId,
                    FD_START_DATE = DateTime.Now,
                    FD_END_DATE = DateTime.Now.AddMonths(tenure),
                    FD_ROI = 6.5M, 
                    FD_TENURE = tenure,
                    FD_AMOUNT = amount
                };

                db.Fixed_Deposit_Account.Add(fd);
                db.SaveChanges();

                TempData["SuccessMessage"] = $"FD Account created successfully for {customer.C_NAME}!";
                return RedirectToAction("CreateFDAccount");
            }
            catch (DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors
                    .SelectMany(x => x.ValidationErrors)
                    .Select(x => x.PropertyName + ": " + x.ErrorMessage);

                string fullError = string.Join("; ", errorMessages);
                TempData["ErrorMessage"] = "Validation Error(s): " + fullError;

                return RedirectToAction("CreateFDAccount");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error: " + ex.Message;
                return RedirectToAction("CreateFDAccount");
            }
        }

        
        private string GenerateFDAccountId()
        {
            var lastFD = db.Fixed_Deposit_Account
                .OrderByDescending(f => f.FD_ACCOUNT_ID)
                .FirstOrDefault();

            int nextId = 1;

            if (lastFD != null && lastFD.FD_ACCOUNT_ID.Length >= 3)
            {
                string lastIdNumPart = lastFD.FD_ACCOUNT_ID.Substring(2);
                if (int.TryParse(lastIdNumPart, out int parsed))
                {
                    nextId = parsed + 1;
                }
            }

            return "FD" + nextId.ToString("D3");
        }

      
        [HttpGet]
        public ActionResult CloseFDAccount()
        {
            return View();
        }

       
        [HttpPost]
        public ActionResult CloseFDAccount(string fdAccountId)
        {
            if (string.IsNullOrWhiteSpace(fdAccountId))
            {
                TempData["ErrorMessage"] = "FD Account ID is required.";
                return RedirectToAction("CloseFDAccount");
            }

            var fd = db.Fixed_Deposit_Account.FirstOrDefault(f => f.FD_ACCOUNT_ID == fdAccountId);

            if (fd == null)
            {
                TempData["ErrorMessage"] = "FD Account not found.";
                return RedirectToAction("CloseFDAccount");
            }

            db.Fixed_Deposit_Account.Remove(fd);
            db.SaveChanges();

            TempData["SuccessMessage"] = $"FD Account {fd.FD_ACCOUNT_ID} has been closed successfully.";
            return RedirectToAction("CloseFDAccount");
        }

      
        [HttpGet]
        public ActionResult OpenLoanAccount()
        {
            return View();
        }

     
        [HttpPost]
        public ActionResult OpenLoanAccount(Home_Loan_Account loan)
        {
            if (ModelState.IsValid)
            {
                bool exists = db.Home_Loan_Account.Any(l => l.LN_ACCOUNT_ID == loan.LN_ACCOUNT_ID);
                if (exists)
                {
                    TempData["ErrorMessage"] = "Account ID already exists!";
                    return View(loan);
                }

                db.Home_Loan_Account.Add(loan);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Loan account opened successfully!";
                return RedirectToAction("OpenLoanAccount");
            }

            TempData["ErrorMessage"] = "Please fill all fields correctly.";
            return View(loan);
        }

       
        [HttpGet]
        public ActionResult CloseLoanAccount()
        {
            return View();
        }

      
        [HttpPost]
        public ActionResult CloseLoanAccount(string loanAccountId)
        {
            if (string.IsNullOrEmpty(loanAccountId))
            {
                TempData["ErrorMessage"] = "Please enter Loan Account ID.";
                return View();
            }

            var loanAccount = db.Home_Loan_Account.FirstOrDefault(l => l.LN_ACCOUNT_ID == loanAccountId);

            if (loanAccount == null)
            {
                TempData["ErrorMessage"] = "Loan account not found.";
                return View();
            }

            db.Home_Loan_Account.Remove(loanAccount);
            db.SaveChanges();

            TempData["SuccessMessage"] = "Loan account closed successfully!";
            return RedirectToAction("CloseLoanAccount");
        }
    }
}
    
