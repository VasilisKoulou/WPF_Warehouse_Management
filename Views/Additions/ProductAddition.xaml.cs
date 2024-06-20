using PdfSharp.Drawing;
using System.Configuration;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;

namespace NvvmFinal.Views.Additions
{
    /// <summary>
    /// Interaction logic for ProductAddition.xaml
    /// </summary>
    public partial class ProductAddition : Window
    {
        public ProductAddition()
        {
            InitializeComponent();
            PopulateLocationIDs();
        }
        private string GetConnectionString()
        {

            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings["AdventureWorks2019"];
            if (settings != null)
                return settings.ConnectionString;
            throw new Exception("Connection string for AdventureWorks2019 not found.");
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MnmzBtn_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void BtnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = txtName.Text;
                string productNumber = txtProductNumber.Text;
                string color = txtColor.Text;
                decimal standardCost = decimal.Parse(txtStandardCost.Text);
                decimal listPrice = decimal.Parse(txtListPrice.Text);
                bool makeFlag = cmbMakeFlag.SelectedIndex == 0;
                bool finishedGoodsFlag = cmbFinishedGoodsFlag.SelectedIndex == 0;
                int safetyStockLevel = int.Parse(txtSafetyStockLevel.Text);
                int reorderPoint = int.Parse(txtReorderPoint.Text);
                int daysToManufacture = int.Parse(txtDaysToManufacture.Text);
                DateTime sellStartDate = DateTime.Parse(txtSellStartDate.Text);
                DateTime modifiedDate = dpModifiedDate.SelectedDate ?? DateTime.Now;

                short? locationID = null;
                if (cmbLocationID.SelectedItem != null && short.TryParse(cmbLocationID.SelectedItem.ToString(), out short locID))
                {
                    locationID = locID;
                }
                else
                {
                    MessageBox.Show("Location ID cannot be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                short? initialQuantity = null;
                if (!string.IsNullOrEmpty(txtInitialStockQuantity.Text) && short.TryParse(txtInitialStockQuantity.Text, out short initQty))
                {
                    initialQuantity = initQty;
                }
                else
                {
                    MessageBox.Show("Initial Stock Quantity cannot be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                byte? bin = null;
                if (cmbBin.SelectedItem != null && byte.TryParse(cmbBin.SelectedItem.ToString(), out byte binVal))
                {
                    bin = binVal;
                }
                else
                {
                    MessageBox.Show("Bin cannot be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string shelf = cmbShelf.SelectedItem?.ToString();

                using (SqlConnection connection = new SqlConnection(GetConnectionString()))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("AddNewProduct", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Name", name);
                        command.Parameters.AddWithValue("@ProductNumber", productNumber);
                        command.Parameters.AddWithValue("@Color", color);
                        command.Parameters.AddWithValue("@StandardCost", standardCost);
                        command.Parameters.AddWithValue("@ListPrice", listPrice);
                        command.Parameters.AddWithValue("@MakeFlag", makeFlag);
                        command.Parameters.AddWithValue("@FinishedGoodsFlag", finishedGoodsFlag);
                        command.Parameters.AddWithValue("@SafetyStockLevel", safetyStockLevel);
                        command.Parameters.AddWithValue("@ReorderPoint", reorderPoint);
                        command.Parameters.AddWithValue("@DaysToManufacture", daysToManufacture);
                        command.Parameters.AddWithValue("@SellStartDate", sellStartDate);
                        command.Parameters.AddWithValue("@ModifiedDate", modifiedDate);
                        command.Parameters.AddWithValue("@LocationID", locationID);
                        command.Parameters.AddWithValue("@InitialStockQuantity", initialQuantity);
                        command.Parameters.AddWithValue("@Bin", bin);
                        command.Parameters.AddWithValue("@Shelf", shelf);

                        command.ExecuteNonQuery();

                        MessageBox.Show("New product added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Invalid input format. Please ensure all fields are filled correctly.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CmbLocationID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbLocationID.SelectedItem != null)
            {
                PopulateBins(Convert.ToInt16(cmbLocationID.SelectedItem));
            }
        }

        private void CmbBin_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbBin.SelectedItem != null)
            {
                PopulateShelves(Convert.ToByte(cmbBin.SelectedItem));
            }
        }

        private void CmbShelf_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (cmbShelf.SelectedItem != null)
            {

                MessageBox.Show("Selected Shelf: " + cmbShelf.SelectedItem.ToString(), "Selection Changed", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void PopulateLocationIDs()
        {
            cmbLocationID.Items.Clear();
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                connection.Open();
                string query = "SELECT DISTINCT LocationID FROM Production.ProductInventory";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cmbLocationID.Items.Add(reader.GetInt16(0));
                        }
                    }
                }
            }
        }
        private void PopulateBins(short locationID)
        {
            cmbBin.Items.Clear();
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                connection.Open();
                string query = "SELECT DISTINCT Bin FROM Production.ProductInventory WHERE LocationID = @LocationID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@LocationID", locationID);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cmbBin.Items.Add(reader.GetByte(0));
                        }
                    }
                }
            }
        }

        private void PopulateShelves(byte bin)
        {
            cmbShelf.Items.Clear();
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                connection.Open();


                string countQuery = "SELECT COUNT(*) FROM Production.ProductInventory WHERE Bin = @Bin";
                using (SqlCommand countCommand = new SqlCommand(countQuery, connection))
                {
                    countCommand.Parameters.AddWithValue("@Bin", bin);
                    int count = (int)countCommand.ExecuteScalar();


                    if (count > 0)
                    {
                        string query = "SELECT DISTINCT Shelf FROM Production.ProductInventory WHERE Bin = @Bin";
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Bin", bin);
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    cmbShelf.Items.Add(reader.GetString(0));
                                }
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("No records available for the specified bin.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }
    }
}
