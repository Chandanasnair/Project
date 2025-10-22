
using DataModel;
using DataModel.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace Bankapp.Controllers
{
    public class ManagerController : Controller
    {
        BankDbEntities db = new BankDbEntities();

        public ActionResult OpenAccount()
        {
            return View();
        }

        public ActionResult ViewTranscations()
        {
            return View();
        }
        public ActionResult AddEmp()
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
            int customerId = (int)Session["CustomerID"];
            ViewBag.CustomerID = customerId;
            return View();
        }

        //[HttpPost]
        //public ActionResult CreateSavingsAccount(string SB_ACCOUNT_ID, decimal SB_BALANCE)
        //{
        //    int customerId = (int)Session["CustomerID"];
        //    var account = new Savings_Account
        //    {
        //        SB_ACCOUNT_ID = SB_ACCOUNT_ID,
        //        SB_CUSTOMER_ID = customerId,
        //        SB_BALANCE = SB_BALANCE
        //    };
        //    if (ModelState.IsValid)
        //    {
        //        db.Savings_Account.Add(account);
        //        db.SaveChanges();

        //        TempData["SuccessMessage"] = "Savings Account created successfully!";
        //        return RedirectToAction("CreateSavingsAccount");
        //    }
        //    TempData["ErrorMessage"] = "An error occurred while creating the account. Please try again.";
        //    return RedirectToAction("OpenAccount");

        //}
        [HttpPost]
        public ActionResult CreateSavingsAccount(decimal SB_BALANCE)
        {
            if (Session["CustomerID"] == null)
            {
                TempData["ErrorMessage"] = "Customer not logged in.";
                return RedirectToAction("CreateSavingsAccount");
            }

            int customerId = (int)Session["CustomerID"];

            bool accountExists = db.Savings_Account.Any(a => a.SB_CUSTOMER_ID == customerId);
            if (accountExists)
            {
                TempData["ErrorMessage"] = "A savings account already exists for this customer.";
                return RedirectToAction("CreateSavingsAccount");
            }

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
        private static List<Savings_Transaction> _transactionsDb = new List<Savings_Transaction>();
        private static decimal _currentBalance = 10000m;  // Example initial balance

        //        [HttpGet]
        //        public ActionResult SavingsTransaction(string accountId)
        //        {
        //            if (string.IsNullOrEmpty(accountId))
        //            {
        //                // Return empty list if no account provided
        //                ViewBag.AccountID = "";
        //                ViewBag.CurrentBalance = 0m;
        //                return View(new List<Savings_Transaction>());
        //            }

        //            // Filter transactions for the account (assuming ST_Account_ID exists)
        //            var transactions = _transactionsDb.FindAll(t => t.ST_Account_ID == accountId);

        //            ViewBag.AccountID = accountId;
        //            ViewBag.CurrentBalance = _currentBalance;

        //            return View(transactions);
        //        }

        //        [HttpPost]
        //        public ActionResult SavingsTransaction(string ST_Account_ID, string ST_Deposit_Withdrawl, decimal ST_Amount)
        //        {
        //            if (string.IsNullOrEmpty(ST_Account_ID) || string.IsNullOrEmpty(ST_Deposit_Withdrawl) || ST_Amount < 100)
        //            {
        //                TempData["ErrorMessage"] = "Please fill all fields correctly. Amount must be at least ₹100.";
        //                return RedirectToAction("SavingsTransaction", new { accountId = ST_Account_ID });
        //            }

        //            // Handle deposit or withdraw
        //            if (ST_Deposit_Withdrawl == "Deposit")
        //            {
        //                _currentBalance += ST_Amount;
        //            }
        //            else if (ST_Deposit_Withdrawl == "Withdraw")
        //            {
        //                if (ST_Amount > _currentBalance)
        //                {
        //                    TempData["ErrorMessage"] = "Insufficient balance for withdrawal.";
        //                    return RedirectToAction("SavingsTransaction", new { accountId = ST_Account_ID });
        //                }
        //                _currentBalance -= ST_Amount;
        //            }
        //            else
        //            {
        //                TempData["ErrorMessage"] = "Invalid transaction type.";
        //                return RedirectToAction("SavingsTransaction", new { accountId = ST_Account_ID });
        //            }


        //            _transactionsDb.Add(new Savings_Transaction
        //            {
        //                ST_TRANSACTION_ID = Guid.NewGuid().ToString(),   
        //                ST_WITHDRAWL = ,
        //                ST_AMOUNT = ST_Amount,
        //                ST_Transaction_Date = DateTime.Now
        //            });

        //            TempData["SuccessMessage"] = "Transaction successful!";
        //            return RedirectToAction("SavingsTransaction", new { accountId = ST_Account_ID });
        //        }
        //}

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

        [HttpGet]
        public ActionResult AddEmployee()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddEmployee(Employee emp)
        {
           
            emp.E_ID = GenerateValidEID();

            if (ModelState.IsValid)
            {
                bool panExists = db.Employees.Any(e => e.E_PAN == emp.E_PAN);
                bool emailExists = !string.IsNullOrEmpty(emp.E_Email) && db.Employees.Any(e => e.E_Email == emp.E_Email);

                if (panExists)
                {
                    TempData["ErrorMessage"] = "Employee with this PAN already exists!";
                    return View(emp);
                }

                if (emailExists)
                {
                    TempData["ErrorMessage"] = "Employee with this Email already exists!";
                    return View(emp);
                }

                try
                {
                    db.Employees.Add(emp);
                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Employee added successfully!";
                    return RedirectToAction("AddEmployee");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Error saving to database: " + ex.Message;
                    return View(emp);
                }
            }

            // Show validation errors
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            TempData["ErrorMessage"] = "Failed to add employee. " + string.Join(", ", errors);
            return View(emp);
        }

        private int GenerateValidEID()
        {
            Random rnd = new Random();
            int eid;

            do
            {
                eid = rnd.Next(11111, 99999);
            }
            while (eid.ToString().Any(c => c == '0'));

            return eid;
        }



        public ActionResult ViewEmployees()
        {

            List<Employee> employeeList = db.Employees.ToList();
            return View(employeeList);

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

                    
                    decimal roi;
                    if (tenure <= 12)
                        roi = 6.0M;
                    else if (tenure <= 24)
                        roi = 7.0M;
                    else
                        roi = 8.0M;

                    
                    if (customer.C_DOB.HasValue)
                    {
                        int age = DateTime.Now.Year - customer.C_DOB.Value.Year;
                        if (customer.C_DOB.Value.Date > DateTime.Now.AddYears(-age)) age--;

                        if (age >= 60)
                            roi += 0.5M;
                    }

                    var fd = new Fixed_Deposit_Account
                    {
                        FD_ACCOUNT_ID = GenerateFDAccountId(),
                        FD_CUSTOMER_ID = customerId,
                        FD_START_DATE = DateTime.Now,
                        FD_END_DATE = DateTime.Now.AddMonths(tenure),
                        FD_ROI = roi,
                        FD_TENURE = tenure,
                        FD_AMOUNT = amount
                    };

                    db.Fixed_Deposit_Account.Add(fd);
                    db.SaveChanges();

                    TempData["SuccessMessage"] = $"FD Account created successfully for {customer.C_NAME} with ROI {roi}%!";
                    return RedirectToAction("CreateFDAccount");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Error: " + ex.Message;
                    return RedirectToAction("CreateFDAccount");
                }
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

            string newId = "FD" + nextId.ToString("D3");

            if (newId.Length > 5)
            {
                throw new Exception("FD_ACCOUNT_ID exceeded max length of 5 characters.");
            }

            return newId;
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
            return RedirectToAction("CloseAccount");
        }
        public ActionResult RemoveEmployee()
        {
            return View();
        }
        [HttpPost]
        public ActionResult RemoveEmployee(int employeeId)
        {
            var employee = db.Employees.Find(employeeId);

            if (employee == null)
            {
                TempData["ErrorMessage"] = "Employee not found!";
                return View();
            }

            try
            {
                db.Employees.Remove(employee);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Employee removed successfully!";
                return RedirectToAction("RemoveEmployee");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error removing employee: " + ex.Message;
                return View();
            }
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
                loan.LN_ACCOUNT_ID = GenerateLoanAccountId(); 

                db.Home_Loan_Account.Add(loan);
                db.SaveChanges();

                TempData["SuccessMessage"] = $"Loan account opened successfully! Account ID: {loan.LN_ACCOUNT_ID}";
                return RedirectToAction("OpenLoanAccount");
            }

            TempData["ErrorMessage"] = "Please fill all fields correctly.";
            return View(loan);
        }


       

        private string GenerateLoanAccountId()
        {
            var lastLoan = db.Home_Loan_Account
                .OrderByDescending(l => l.LN_ACCOUNT_ID)
                .FirstOrDefault();

            int nextId = 1;

            if (lastLoan != null && lastLoan.LN_ACCOUNT_ID.Length >= 3)
            {
                string lastIdNumPart = lastLoan.LN_ACCOUNT_ID.Substring(2);
                if (int.TryParse(lastIdNumPart, out int parsed))
                {
                    nextId = parsed + 1;
                }
            }

            return "LN" + nextId.ToString("D3");
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
            return RedirectToAction("CloseAccount");
        }


    }
}

        
