using Expense_Tracker.Controllers;
using System;
using System.Data.SqlClient;

namespace Expense_Tracker.Models
{
    public class DashboardData
    {
        string ConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=transactionDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;";

        public decimal TotalIncome()
        {
            decimal totalIncome = 0;

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                string query = "SELECT SUM(Amount) FROM Transactions WHERE CategoryId IN (SELECT CategoryId FROM Categories WHERE Type = 'Income')";
                SqlCommand command = new SqlCommand(query, connection);

                object result = command.ExecuteScalar();

                if (result != DBNull.Value)
                {
                    totalIncome = Convert.ToDecimal(result);
                }

                connection.Close();
            }

            return totalIncome;
        }

        public decimal TotalExpense()
        {
            decimal totalExpense = 0;

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                string query = "SELECT SUM(Amount) FROM Transactions WHERE CategoryId IN (SELECT CategoryId FROM Categories WHERE Type = 'Expense')";
                SqlCommand command = new SqlCommand(query, connection);

                object result = command.ExecuteScalar();

                if (result != DBNull.Value)
                {
                    totalExpense = Convert.ToDecimal(result);
                }

                connection.Close();
            }

            return totalExpense;
        }

        public void GetTransactionsByCategory(int categoryId)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                string query = "SELECT * FROM Transactions WHERE CategoryId = @CategoryId";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CategoryId", categoryId);

                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    int transactionId = reader.GetInt32(0);
                    int category = reader.GetInt32(1);
                    decimal amount = reader.GetDecimal(2);
                    string note = reader.IsDBNull(3) ? null : reader.GetString(3);
                    DateTime date = reader.GetDateTime(4);

                    // Process the retrieved data as needed
                    Console.WriteLine($"TransactionId: {transactionId}, CategoryId: {category}, Amount: {amount}, Note: {note}, Date: {date}");
                }

                reader.Close();
                connection.Close();
            }
        }

        public List<object> GetDoughnutChartData()
        {
            List<object> doughnutChartData = new List<object>();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                string query = @"
            SELECT 
                C.Icon + ' ' + C.Title AS CategoryTitleWithIcon,
                CAST(SUM(T.Amount) AS DECIMAL(18, 2)) AS Amount, -- CAST to handle potential decimal conversion
                FORMAT(SUM(T.Amount), 'C0') AS FormattedAmount
            FROM 
                Transactions T
                INNER JOIN Categories C ON T.CategoryId = C.CategoryId
            WHERE 
                C.Type = 'Expense'
            GROUP BY 
                C.Icon, C.Title, T.CategoryId
            ORDER BY 
                Amount DESC
        ";

                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var categoryTitleWithIcon = reader.GetString(0);
                    var amount = reader.GetDecimal(1);  // Use GetDecimal for decimal types
                    var formattedAmount = reader.GetString(2);

                    doughnutChartData.Add(new
                    {
                        categoryTitleWithIcon,
                        amount,
                        formattedAmount
                    });
                }

                reader.Close();
                connection.Close();
            }

            return doughnutChartData;
        }

        public List<SplineChartData> GetSplineChartData()
        {
            List<SplineChartData> splineChartData = new List<SplineChartData>();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                string query = @"
            SELECT 
                CONVERT(VARCHAR(5), T.Date, 13) AS Day,
                CAST(SUM(T.Amount) AS DECIMAL(18, 2)) AS Income
            FROM 
                Transactions T
                INNER JOIN Categories C ON T.CategoryId = C.CategoryId
            WHERE 
                C.Type = 'Income'
            GROUP BY 
                CONVERT(VARCHAR(5), T.Date, 13)
            ORDER BY 
                Day
        ";

                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var day = reader.GetString(0);
                    var income = reader.GetDecimal(1);  // Use GetDecimal for decimal types

                    var dataPoint = new SplineChartData
                    {
                        day = day,
                        income = (int)income
                    };

                    splineChartData.Add(dataPoint);
                }

                reader.Close();
                connection.Close();
            }

            return splineChartData;
        }


    }
}
