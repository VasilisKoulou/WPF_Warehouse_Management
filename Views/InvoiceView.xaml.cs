using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using NvvmFinal.Views.Additions;
using System.Windows.Forms;
using NvvmFinal.ViewModels;


namespace NvvmFinal.Views
{
    /// <summary>
    /// Interaction logic for InvoiceView.xaml
    /// </summary>
    public partial class InvoiceView : System.Windows.Controls.UserControl
    {
        string str = null;
        public InvoiceView()
        {
            InitializeComponent();
        }
        private void UserControl_Loaded_2(object sender, RoutedEventArgs e)
        {

        }

        private string GetConnectionString()
        {

            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings["AdventureWorks2019"];
            if (settings != null)
                return settings.ConnectionString;
            throw new Exception("Connection string for AdventureWorks2019 not found.");
        }


        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string str = txtSearch.Text;

            if (!string.IsNullOrEmpty(str))
            {
                string connectionString = GetConnectionString();
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    try
                    {
                        con.Open();

                        string query = "";
                        SqlCommand cmd = new SqlCommand();

                        if (chkSearchByCustomerID.IsChecked == true && chkSearchByEmployeeID.IsChecked == true)
                        {
                            query = $"SELECT CAST(soh.CustomerID AS NVARCHAR) AS ID, pp.LastName, pp.FirstName, CAST(soh.SalesOrderNumber AS NVARCHAR) AS OrderNumber, CAST(sod.ProductID AS NVARCHAR) AS ProductID, CAST(soh.TotalDue AS NVARCHAR) AS TotalDue " +
                                    $"FROM sales.SalesOrderHeader AS soh " +
                                    $"JOIN sales.SalesOrderDetail AS sod ON soh.SalesOrderID = sod.SalesOrderID " +
                                    $"JOIN sales.Customer AS sc ON soh.CustomerID = sc.CustomerID " +
                                    $"JOIN person.Person AS pp ON sc.PersonID = pp.BusinessEntityID " +
                                    $"WHERE pp.LastName LIKE @SearchText " +
                                    $"UNION " +
                                    $"SELECT CAST(poh.EmployeeID AS NVARCHAR) AS ID, pp.LastName, pp.FirstName, CAST(poh.PurchaseOrderID AS NVARCHAR) AS OrderNumber, CAST(pod.ProductID AS NVARCHAR) AS ProductID, CAST(poh.SubTotal AS NVARCHAR) AS TotalDue " +
                                    $"FROM Purchasing.PurchaseOrderHeader AS poh " +
                                    $"JOIN Purchasing.PurchaseOrderDetail AS pod ON poh.PurchaseOrderID = pod.PurchaseOrderID " +
                                    $"JOIN HumanResources.Employee AS e ON poh.EmployeeID = e.BusinessEntityID " +
                                    $"JOIN Person.Person AS pp ON e.BusinessEntityID = pp.BusinessEntityID " +
                                    $"WHERE pp.LastName LIKE @SearchText " +
                                    $"ORDER BY  pp.LastName;";
                            cmd = new SqlCommand(query, con);
                            cmd.Parameters.AddWithValue("@SearchText", str + "%");
                        }
                        else if (chkSearchByCustomerID.IsChecked == true)
                        {
                            query = $"SELECT CAST(soh.SalesOrderNumber AS NVARCHAR) AS OrderNumber, CAST(soh.CustomerID AS NVARCHAR) AS CustomerID, pp.LastName, pp.FirstName, CAST(sod.ProductID AS NVARCHAR) AS ProductID, CAST(soh.TotalDue AS NVARCHAR) AS TotalDue, 'Customer' AS SearchType " +
                                    $"FROM sales.SalesOrderHeader AS soh " +
                                    $"JOIN sales.SalesOrderDetail AS sod ON soh.SalesOrderID = sod.SalesOrderID " +
                                    $"JOIN sales.Customer AS sc ON soh.CustomerID = sc.CustomerID " +
                                    $"JOIN person.Person AS pp ON sc.PersonID = pp.BusinessEntityID " +
                                    $"WHERE soh.SalesOrderNumber LIKE @SearchText " +
                                    $"ORDER BY soh.CustomerID";
                            cmd = new SqlCommand(query, con);
                            cmd.Parameters.AddWithValue("@SearchText", str + "%");
                        }
                        else if (chkSearchByEmployeeID.IsChecked == true)
                        {
                            query = $"SELECT CAST(poh.PurchaseOrderID AS NVARCHAR) AS PurchaseOrderID, CAST(poh.EmployeeID AS NVARCHAR) AS EmployeeID, pp.LastName, pp.FirstName,  CAST(pod.ProductID AS NVARCHAR) AS ProductID, CAST(poh.SubTotal AS NVARCHAR) AS TotalDue, 'Employee' AS SearchType " +
                                    $"FROM Purchasing.PurchaseOrderHeader AS poh " +
                                    $"JOIN Purchasing.PurchaseOrderDetail AS pod ON poh.PurchaseOrderID = pod.PurchaseOrderID " +
                                    $"JOIN HumanResources.Employee AS e ON poh.EmployeeID = e.BusinessEntityID " +
                                    $"JOIN Person.Person AS pp ON e.BusinessEntityID = pp.BusinessEntityID " +
                                    $"WHERE poh.PurchaseOrderID LIKE @SearchText " +
                                    $"ORDER BY poh.EmployeeID";
                            cmd = new SqlCommand(query, con);
                            cmd.Parameters.AddWithValue("@SearchText", str + "%");
                        }

                        if (!string.IsNullOrEmpty(query))
                        {
                            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            MyDataGrid.ItemsSource = dt.DefaultView;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.Forms.MessageBox.Show($"Failed to load data. Error: {ex.Message}");
                    }
                    finally
                    {
                        con.Close();
                    }
                }
            }
        }



        public string getSalesOrderNumber()
        {
            string salesOrderNumber = null;

            if (MyDataGrid.SelectedItem is DataRowView rowView)
            {
                salesOrderNumber = rowView["OrderNumber"].ToString();
            }

            return salesOrderNumber;
        }


        public int getProductID()
        {
            int productID = 0;

            if (MyDataGrid.SelectedItem is DataRowView rowView)
            {
                productID = Convert.ToInt32(rowView["ProductID"]);
            }

            return productID;
        }

        private InvoicesDetails detailsInvoiceWindow;
        private Purchase_Details detailsPurchaseWindow;

        private void MyDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int productID = getProductID();

            if (detailsInvoiceWindow != null && detailsInvoiceWindow.IsLoaded)
            {
                detailsInvoiceWindow.Close();
            }

            if (detailsPurchaseWindow != null && detailsPurchaseWindow.IsLoaded)
            {
                detailsPurchaseWindow.Close();
            }

            if (MyDataGrid.SelectedItem is DataRowView rowView)
            {
                string orderNumber = rowView["OrderNumber"].ToString();

                if (chkSearchByEmployeeID.IsChecked == true && chkSearchByCustomerID.IsChecked == true)
                {
                    if (int.TryParse(orderNumber, out int number))
                    {
                        detailsPurchaseWindow = new Purchase_Details(productID);
                        detailsPurchaseWindow.Show();
                    }
                    else
                    {
                        string salesOrderNumber = getSalesOrderNumber();
                        detailsInvoiceWindow = new InvoicesDetails(salesOrderNumber, productID);
                        detailsInvoiceWindow.Show();
                    }
                }
                else if (chkSearchByEmployeeID.IsChecked == true)
                {
                    detailsPurchaseWindow = new Purchase_Details(productID);
                    detailsPurchaseWindow.Show();
                }
                else if (chkSearchByCustomerID.IsChecked == true)
                {
                    string salesOrderNumber = getSalesOrderNumber();
                    detailsInvoiceWindow = new InvoicesDetails(salesOrderNumber, productID);
                    detailsInvoiceWindow.Show();
                }

            }
        }
    }
}