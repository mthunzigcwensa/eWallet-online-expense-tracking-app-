using Expense_Tracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Expense_Tracker.Controllers
{
    public class DashboardController : Controller
    {

        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ActionResult> Index()
        {
            //Last 7 Days
            DateTime StartDate = DateTime.Today.AddDays(-6);
            DateTime EndDate = DateTime.Today;

            List<Transaction> SelectedTransactions = await _context.Transactions
                .Include(x => x.Category)
                .Where(y => y.Date >= StartDate && y.Date <= EndDate)
                .ToListAsync();

            DashboardData j = new DashboardData();

            //Total Income
            decimal TotalIncome = j.TotalIncome();
                
            ViewBag.TotalIncome = TotalIncome.ToString("C0");

            //Total Expense
            decimal TotalExpense = j.TotalExpense();
                
            ViewBag.TotalExpense = TotalExpense.ToString("C0");

            //Balance
            decimal Balance = TotalIncome - TotalExpense;
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
            culture.NumberFormat.CurrencyNegativePattern = 1;
            ViewBag.Balance = Balance.ToString("C0");

            DashboardData dashboardData = new DashboardData();
            List<object> doughnutChartData = dashboardData.GetDoughnutChartData();
            ViewBag.DoughnutChartData = doughnutChartData;

            //Spline Chart - Income vs Expense

            List<SplineChartData> IncomeSummary = dashboardData.GetSplineChartData();


            //Expense
            List<SplineChartData> ExpenseSummary = SelectedTransactions
                .Where(i => i.Category.Type == "Expense")
                .GroupBy(j => j.Date)
                .Select(k => new SplineChartData()
                {
                    day = k.First().Date.ToString("dd-MMM"),
                    expense = k.Sum(l => l.Amount)
                })
                .ToList();

            //Combine Income & Expense
            string[] Last7Days = Enumerable.Range(0, 7)
                .Select(i => StartDate.AddDays(i).ToString("dd-MMM"))
                .ToArray();

            ViewBag.SplineChartData = from day in Last7Days
                                      join income in IncomeSummary on day equals income.day into dayIncomeJoined
                                      from income in dayIncomeJoined.DefaultIfEmpty()
                                      join expense in ExpenseSummary on day equals expense.day into expenseJoined
                                      from expense in expenseJoined.DefaultIfEmpty()
                                      select new
                                      {
                                          day = day,
                                          income = income == null ? 0 : income.income,
                                          expense = expense == null ? 0 : expense.expense,
                                      };
            //Recent Transactions
            ViewBag.RecentTransactions = await _context.Transactions
                .Include(i => i.Category)
                .OrderByDescending(j => j.Date)
                .Take(5)
                .ToListAsync();


            return View();
        }
    }

    public class SplineChartData
    {
        public string day;
        public int income;
        public int expense;

    }
}
