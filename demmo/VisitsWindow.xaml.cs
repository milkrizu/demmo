using demmo.DataBase;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
using System.Windows.Shapes;

namespace demmo
{
    /// <summary>
    /// Interaction logic for VisitsWindow.xaml
    /// </summary>
    public partial class VisitsWindow : Window
    {
        private Client _client;
        public VisitsWindow(Client client)
        {
            InitializeComponent();
            _client = client;
            LoadVisits();
        }

        private void LoadVisits()
        {
            // Строка подключения к базе данных
            string connectionString = "Server=DESKTOP-027MGS0\\SQLEXPRESS;Database=Demo;Integrated Security=True;";

            // SQL-запрос для получения посещений клиента
            string query = "SELECT ID, StartTime, Comment FROM ClientService WHERE ClientID = @ClientID";

            var visits = new List<ClientService>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Передаем параметр ClientID
                    command.Parameters.AddWithValue("@ClientID", _client.ID);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var visit = new ClientService
                            {
                                ID = (int)reader["ID"],
                                StartTime = reader["StartTime"] as DateTime?,
                                Comment = reader["Comment"] as string
                            };
                            visits.Add(visit);
                        }
                    }
                }
            }

            VisitsGrid.ItemsSource = visits;
        }
    }
}
