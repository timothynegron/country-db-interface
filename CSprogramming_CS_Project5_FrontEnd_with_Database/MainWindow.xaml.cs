//***************************************************************************
//File: MainWindow.xaml.cs
//
//Purpose: To display a graphical user interface that allows the user to see
//  data related to Countries. The data is extracted from a database. The
//  user will also be able to store data from a JSON file to the database.
//
//Written By: Timothy Negron
//
//Compiler: Visual Studio C# 2017
//
//***************************************************************************


using System;
using System.Collections.Generic;
using System.IO;
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
using Microsoft.Extensions.Configuration; // Library to help read in the connection string from the json file (NuGet Packages Downloaded)
using Microsoft.Win32;
using CSprogramming_CS_DLL_for_Country_Data; // My Imported DLL
using System.Runtime.Serialization.Json;
using System.Data.SqlClient;
using System.Data;

namespace CSprogramming_CS_Project5_FrontEnd_with_Database
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Attributes
        string FilePath = @"D:\e BACKUP October 2020\CSprogramming_CS_Project5_FrontEnd_with_Database\CSprogramming_CS_Project5_FrontEnd_with_Database";
        List<Country> ListOfCountries = new List<Country>();
        SqlConnection sqlConn;
        string connString;
        OpenFileDialog openFileDialog = new OpenFileDialog();
        #endregion

        #region MainWindow
        //***************************************************************************
        //Method: MainWindow()
        //
        //Purpose: To create the WPF interface.
        //
        //***************************************************************************
    
        public MainWindow()
        {
            InitializeOpenFileDialog();

            InitializeComponent();

            ReadConnectionString();

            SetListBox();
        }
        #endregion

        #region Open Countries From JSON Button
        //***************************************************************************
        //Method: ButtonCountriesFromJSON_Click(object sender, RoutedEventArgs e)
        //
        //Purpose: To import data from a JSON file to the database
        //
        //***************************************************************************

        private void ButtonCountriesFromJSON_Click(object sender, RoutedEventArgs e)
        {
            ShowOpenFileDialog();
        }
        #endregion

        #region Exit Button
        //***************************************************************************
        //Method: ButtonExit_Click(object sender, RoutedEventArgs e)
        //
        //Purpose: To close the application.
        //
        //***************************************************************************

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        #region About Button
        //***************************************************************************
        //Method: ButtonAbout_Click(object sender, RoutedEventArgs e)
        //
        //Purpose: To display developer information.
        //
        //***************************************************************************
        
        private void ButtonAbout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Country GUI \nVersion 2.0 \nWritten by Timothy Negron", "About Country GUI 2.0");
        }
        #endregion

        #region Methods

        //***************************************************************************
        //Method: SetListBox()
        //
        //Purpose: To set prepare and display the ListBox with country data.
        //
        //***************************************************************************

        private void SetListBox()
        {
            ReadFile();

            ClearDatabase();

            // Get data from each country and insert it into the database
            foreach (Country c in ListOfCountries)
            {
                InsertToDatabase(c.Name, c.Capital, c.Region, c.Subregion, c.Population);
            }

            DisplayData();
        }

        //***************************************************************************
        //Method: DisplayData()
        //
        //Purpose: To display the name of the countries in the ListBox.
        //
        //***************************************************************************

        private void DisplayData()
        {
            string sql = "SELECT * FROM Country";

            OpenSQLConnection();

            SqlCommand command = new SqlCommand(sql, sqlConn);

            SqlDataReader reader = command.ExecuteReader();

            ListBoxCountry.ItemsSource = reader;

            ListBoxCountry.DisplayMemberPath = "Name";   
        }

        //***************************************************************************
        //Method: InsertToDatabase(string Name, string Capital, string Region, string 
        // Subregion, int Population)
        //
        //Purpose: To insert country data to the database country table.
        //
        //***************************************************************************

        private void InsertToDatabase(string Name, string Capital, string Region, string Subregion, int Population)
        {
            string sql = "INSERT INTO Country" +
                         "(Name, Capital, Region, Subregion, Population) Values" +
                         "(@name, @capital, @region, @subregion, @population)";

            OpenSQLConnection();

            SqlCommand command = new SqlCommand(sql, sqlConn);

            command.Parameters.Add(new SqlParameter("@name", Name));
            command.Parameters.Add(new SqlParameter("@capital", Capital));
            command.Parameters.Add(new SqlParameter("@region", Region));
            command.Parameters.Add(new SqlParameter("@subregion", Subregion));
            command.Parameters.Add(new SqlParameter("@population", Population));
            
            int rowsAffected = command.ExecuteNonQuery();

            sqlConn.Close();
        }

        //***************************************************************************
        //Method: ClearDatabase()
        //
        //Purpose: To remove all data from the database.
        //
        //***************************************************************************

        private void ClearDatabase()
        {
            string sql = "DELETE FROM Country";
            OpenSQLConnection();
            SqlCommand command = new SqlCommand(sql, sqlConn);
            int rowsAffected = command.ExecuteNonQuery();
        }

        //***************************************************************************
        //Method: InitializeOpenFileDialog()
        //
        //Purpose: To initialize the open file dialog.
        //
        //***************************************************************************

        private void InitializeOpenFileDialog()
        {
            openFileDialog.InitialDirectory = Environment.CurrentDirectory;
            openFileDialog.Title = "Get Countries Data from JSON file";
            openFileDialog.Filter = "Json files(*.json)| *.json";
        }

        //***************************************************************************
        //Method: OpenSQLConnection()
        //
        //Purpose: To open the sql connection.
        //
        //***************************************************************************

        private void OpenSQLConnection()
        {
            sqlConn = new SqlConnection(connString);
            sqlConn.Open();
        }

        //***************************************************************************
        //Method: ReadConnnectionString()
        //
        //Purpose: To read the connection string from the JSON file.
        //
        //***************************************************************************

        private void ReadConnectionString()
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.SetBasePath(FilePath);
            configurationBuilder.AddJsonFile("config.json");
            IConfiguration config = configurationBuilder.Build();
            connString = config["Data:DefaultConnection:ConnectionString"];
        }

        //***************************************************************************
        //Method: ShowOpenFileDialog()
        //
        //Purpose: To show the open file dialog. If a file is open, clears the
        // the interface and reads the file.
        //
        //***************************************************************************

        private void ShowOpenFileDialog()
        {
            if (openFileDialog.ShowDialog() == true)
            {
               SetListBox();
            }
        }

        //***************************************************************************
        //Method: ReadFile()
        //
        //Purpose: To read the JSON file. Stores the data in ListOfCountries.
        //
        //***************************************************************************

        private void ReadFile()
        {
            // Get the filename
            string filename = openFileDialog.FileName;

            if (filename == "") // Only "", true, when app starts
            {
                filename = "countries.json";
            }

            // Deserialize JSON 
            StreamReader sr = new StreamReader(filename);
            FileStream readFile = new FileStream(filename, FileMode.Open, FileAccess.Read);
            StreamReader streamReader = new StreamReader(readFile, Encoding.UTF8);
            string jsonString = streamReader.ReadToEnd();
            byte[] byteArray = Encoding.UTF8.GetBytes(jsonString);
            MemoryStream stream = new MemoryStream(byteArray);

            // Store data to List
            DataContractJsonSerializer serListOfCountryData = new DataContractJsonSerializer(typeof(List<Country>));
            ListOfCountries = (List<Country>)serListOfCountryData.ReadObject(stream);
            readFile.Close();
        }

        #endregion
    }
}
