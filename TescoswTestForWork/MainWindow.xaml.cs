using Microsoft.Win32;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Xml.Linq;
using TescoswTestForWork.Model;

namespace TescoswTestForWork
{

    public partial class MainWindow : Window
    {
        private List<Car> _allCars = new List<Car>();
        private string filePath = string.Empty;
        private FileInfo fileInfo;
        private string fileInfoSTR = string.Empty;

        public MainWindow()
        {
            InitializeComponent();

            responseTextBlock.Text = "Prosím, načtěte soubor XML, abyste mohli začít!";
        }

        // Načítání XML souboru

        private void LoadXMLButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML files (*.xml)|*.xml";
            if (openFileDialog.ShowDialog() == true)
            {
                filePath = openFileDialog.FileName;
                fileInfo = new FileInfo(filePath);

                XDocument xmlDoc = XDocument.Load(filePath);

                _allCars = xmlDoc.Descendants("car")
                    .Select(c => new Car
                    {
                        Model = c.Element("model").Value,
                        Price = double.Parse(c.Element("price").Value),
                        Vat = double.Parse(c.Element("vat").Value),
                        SaleDate = DateTime.Parse(c.Element("saleDate").Value)
                    }).ToList();

                // Vyplnění tabulky
                carDataGrid.ItemsSource = _allCars;

                // Vytváření informací o souboru
                fileInfoSTR = $"Informace o souboru\n"
                    + $"--- --- ---\n\n"
                    + $"Název XML souboru: {fileInfo.Name}\n\n"
                    + $"Cesta k XML souboru: {filePath}\n\n"
                    + $"Velikost souboru: {fileInfo.Length} bytes\n\n"
                    + $"Čas vytvoření: {fileInfo.CreationTime}\n\n"
                    + $"Čas poslední úpravy: {fileInfo.LastWriteTime}\n\n"
                    + $"---\n\n"
                    + $"Celkový počet automobilů: {_allCars.Count}\n\n";
                responseTextBlock.Text = fileInfoSTR;

            }
        }

        // Výpočet celkové hodnoty automobilů prodaných o víkendech
        private void CalculateWeekendValue_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckDataLoaded()) return;

            // Pouze víkendy 
            var weekendCars = _allCars.Where(c =>
                            c.SaleDate.DayOfWeek == DayOfWeek.Saturday || c.SaleDate.DayOfWeek == DayOfWeek.Sunday);

            var result = weekendCars
                .GroupBy(c => c.Model)
                .Select(group => new
                {
                    Model = group.Key,
                    TotalPrice = group.Sum(c => c.Price),
                    TotalPriceWithVat = group.Sum(c => c.Price + (c.Price * c.Vat / 100))
                }).ToList();

            // Vytváření výsledků
            string response = "Výsledky výpočtu:\n\n";
            foreach (var car in result)
            {
                response += $"Model: {car.Model}\n";
                response += $"Cena bez DPH: {car.TotalPrice:0.00} Kč\n";
                response += $"Cena s DPH: {car.TotalPriceWithVat:0.00} Kč\n\n";
            }

            // Zobrazení výsledků
            responseTextBlock.Text = response;
            carDataGrid.ItemsSource = result;

        }

        // Vyčištění výsledků
        private void ClearResults_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckDataLoaded()) return;

            responseTextBlock.Text = fileInfoSTR;
            carDataGrid.ItemsSource = _allCars;
        }

        // Odstranění dat k zpracování
        private void DeleteData_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckDataLoaded()) return;

            _allCars.Clear();
            carDataGrid.ItemsSource = null;
            responseTextBlock.Text = "Prosím, načtěte soubor XML, abyste mohli začít!";
        }

        // Logika pro zobrazení/skrytí okna s odpověďmi
        private void ShowResponseCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            // Zobrazit okno s odpověďmi
            ResponseColumn.Width = new GridLength(1, GridUnitType.Star);
            SplitterColumn.Width = new GridLength(5);
            responseViewer.Visibility = Visibility.Visible;
        }

        private void ShowResponseCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            // Skrýt okno s odpověďmi
            ResponseColumn.Width = new GridLength(0);
            SplitterColumn.Width = new GridLength(0);
            responseViewer.Visibility = Visibility.Hidden;
        }

        //Počet prodaných modelů
        private void CalculateTotalCarsPerModel_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckDataLoaded()) return;

            var CalculateTotalCarsPerModel = _allCars.GroupBy(c => c.Model)
                .Select(group => new
                {
                    Model = group.Key,
                    TotalPrice = group.Count()
                }).ToList();

            // Vytváření výsledků
            string response = "Výsledky výpočtu\nPočet prodaných modelů:\n\n";
            foreach (var car in CalculateTotalCarsPerModel)
            {
                response += $"Model: {car.Model}\n";
                response += $"Počet: {car.TotalPrice}\n"; ;
            }

            // Zobrazení výsledků
            responseTextBlock.Text = response;
            carDataGrid.ItemsSource = CalculateTotalCarsPerModel;
        }
        private void CalculateTotalVATAndSales_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckDataLoaded()) return;

            // Celková částka všech prodejů (částka bez DPH)
            double totalSales = _allCars.Sum(c => c.Price);

            // Celková částka DPH (vypočítáno jako Price * (VAT / 100))
            double totalVAT = _allCars.Sum(c => c.Price * (c.Vat / 100));

            // Vytváření odpovědi
            string response = $"Celková částka prodejů: {totalSales} Kč\n"
                        + $"Celková částka DPH: {totalVAT} Kč\n";
            responseTextBlock.Text = response;

            carDataGrid.ItemsSource =
                new List<object>
                {
                    new {
                        Label = "Celková částka prodejů",
                        Value = totalSales
                    },
                    new {
                        Label = "Celková částka DPH",
                        Value = totalVAT
                    }
                };
        }

        // Kontrola, zda jsou data načtena
        private bool CheckDataLoaded()
        {
            if (!_allCars.Any())
            {
                MessageBox.Show("Prosím, načtěte soubor XML, abyste mohli začít!", "Data nejsou načtena", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }
    }

}
