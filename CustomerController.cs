using DataModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Bankapp.Controllers
{
  

public class CustomerController : Controller
    {
        private BankDbEntities db = new BankDbEntities(); // Replace with your actual DbContext

        // Customer Home - Dashboard
        public ActionResult CustomerHome()
        {
            if (Session["Username"] == null || (string)Session["Role"] != "Customer")
            {
                return RedirectToAction("Login", "Bank");
            }

            ViewBag.Username = Session["Username"];
            ViewBag.CustomerID = (int)Session["CustomerID"];
            return View();
        }

        // View all accounts for logged in customer
        public ActionResult ViewAccounts()
        {
            if (Session["CustomerID"] == null)
                return RedirectToAction("Login", "Bank");

            int customerId = (int)Session["CustomerID"];

            // Fetch accounts - replace with your actual model properties
            var savingsAccounts = db.Savings_Account.Where(a => a.SB_CUSTOMER_ID == customerId).ToList();
            var fdAccounts = db.Fixed_Deposit_Account.Where(f => f.FD_CUSTOMER_ID == customerId).ToList();
            var loanAccounts = db.Home_Loan_Account.Where(l => l.LN_CUSTOMER_ID == customerId).ToList();

            ViewBag.SavingsAccounts = savingsAccounts;
            ViewBag.FDAccounts = fdAccounts;
            ViewBag.LoanAccounts = loanAccounts;

            return View();
        }

        // Deposit - Show form
        [HttpGet]
        public ActionResult Deposit()
        {
            if (Session["CustomerID"] == null)
                return RedirectToAction("Login", "Bank");

            int customerId = (int)Session["CustomerID"];
            var accounts = db.Savings_Account.Where(a => a.SB_CUSTOMER_ID == customerId).ToList();
            ViewBag.Accounts = accounts;

            return View();
        }

        // Deposit - Process form
        [HttpPost]
        public ActionResult Deposit(string accountId, decimal amount)
        {
            if (Session["CustomerID"] == null)
                return RedirectToAction("Login", "Bank");

            if (string.IsNullOrEmpty(accountId) || amount <= 0)
            {
                TempData["Error"] = "Invalid account or amount.";
                return RedirectToAction("Deposit");
            }

            var account = db.Savings_Account.FirstOrDefault(a => a.SB_ACCOUNT_ID == accountId);
            if (account == null)
            {
                TempData["Error"] = "Account not found.";
                return RedirectToAction("Deposit");
            }

            account.SB_BALANCE += amount;
            db.SaveChanges();

            TempData["Success"] = $"₹{amount} deposited successfully!";
            return RedirectToAction("ViewAccounts");
        }

        // Withdraw - Show form
        [HttpGet]
        public ActionResult Withdraw()
        {
            if (Session["CustomerID"] == null)
                return RedirectToAction("Login", "Bank");

            int customerId = (int)Session["CustomerID"];
            var accounts = db.Savings_Account.Where(a => a.SB_CUSTOMER_ID == customerId).ToList();
            ViewBag.Accounts = accounts;

            return View();
        }

        // Withdraw - Process form
        [HttpPost]
        public ActionResult Withdraw(string accountId, decimal amount)
        {
            if (Session["CustomerID"] == null)
                return RedirectToAction("Login", "Bank");

            if (string.IsNullOrEmpty(accountId) || amount <= 0)
            {
                TempData["Error"] = "Invalid account or amount.";
                return RedirectToAction("Withdraw");
            }

            var account = db.Savings_Account.FirstOrDefault(a => a.SB_ACCOUNT_ID == accountId);
            if (account == null)
            {
                TempData["Error"] = "Account not found.";
                return RedirectToAction("Withdraw");
            }

            if (account.SB_BALANCE < amount)
            {
                TempData["Error"] = "Insufficient balance.";
                return RedirectToAction("Withdraw");
            }

            account.SB_BALANCE -= amount;
            db.SaveChanges();

            TempData["Success"] = $"₹{amount} withdrawn successfully!";
            return RedirectToAction("ViewAccounts");
        }
    }

}
