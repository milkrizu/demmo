using demmo.DataBase;
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

namespace demmo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int _currentPage = 1;
        private int _pageSize = 10;
        private string _searchText = string.Empty;
        private string _genderFilter = "Все";
        private bool _birthdayFilter = false;

        public MainWindow()
        {
            InitializeComponent();
            LoadClients();
        }

        public void LoadClients()
        {
            var query = DatabaseConnectionClass.DatabaseConnection.Client.AsQueryable();

            // Фильтрация по поиску
            if (!string.IsNullOrEmpty(_searchText))
            {
                query = query.Where(c => c.LastName.Contains(_searchText) ||
                                        c.FirstName.Contains(_searchText) ||
                                        c.Email.Contains(_searchText) ||
                                        c.Phone.Contains(_searchText));
            }


            // Фильтрация по полу
            if (_genderFilter != "Все")
            {
                query = query.Where(c => c.GenderCode == _genderFilter);
            }

            // Фильтрация по дням рождения в текущем месяце
            if (_birthdayFilter)
            {
                var currentMonth = DateTime.Now.Month;
                query = query.Where(c => c.Birthday.HasValue && c.Birthday.Value.Month == currentMonth);
            }

            // Сортировка по фамилии (алфавитный порядок)
            query = query.OrderBy(c => c.LastName);

            // Пагинация
            var clients = query
                .Skip((_currentPage - 1) * _pageSize)
                .Take(_pageSize)
                .ToList();

            ClientsGrid.ItemsSource = clients;
            PageInfo.Text = $"{clients.Count} из {query.Count()}";
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchText = SearchBox.Text;
            _currentPage = 1;
            LoadClients();
        }

        private void GenderFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = (sender as ComboBox).SelectedItem as ComboBoxItem;
            _genderFilter = selectedItem?.Content.ToString();
            _currentPage = 1;
            LoadClients();
        }

        private void BirthdayFilter_Checked(object sender, RoutedEventArgs e)
        {
            _birthdayFilter = true;
            _currentPage = 1;
            LoadClients();
        }

        private void BirthdayFilter_Unchecked(object sender, RoutedEventArgs e)
        {
            _birthdayFilter = false;
            _currentPage = 1;
            LoadClients();
        }

        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                LoadClients();
            }
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            _currentPage++;
            LoadClients();
        }

        private void DeleteClient_Click(object sender, RoutedEventArgs e)
        {
            var selectedClient = ClientsGrid.SelectedItem as Client;
            if (selectedClient != null)
            {
                if (DatabaseConnectionClass.DatabaseConnection.ClientService.Any(cs => cs.ClientID == selectedClient.ID))
                {
                    MessageBox.Show("Нельзя удалить клиента с посещениями.");
                }
                else
                {
                    DatabaseConnectionClass.DatabaseConnection.Client.Remove(selectedClient);
                    DatabaseConnectionClass.DatabaseConnection.SaveChanges();
                    LoadClients();
                }
            }
        }

        private void AddClient_Click(object sender, RoutedEventArgs e)
        {
            var addEditWindow = new AddEditClientWindow();
            addEditWindow.ShowDialog();
            LoadClients();
        }

        // Обработчик для редактирования клиента
        private void EditClient_Click(object sender, RoutedEventArgs e)
        {
            var selectedClient = ClientsGrid.SelectedItem as Client;
            if (selectedClient != null)
            {
                var editWindow = new AddEditClientWindow(selectedClient);
                editWindow.ShowDialog();
                LoadClients(); // Обновляем список клиентов после редактирования
            }
            else
            {
                MessageBox.Show("Выберите клиента для редактирования.");
            }
        }

        private void ClientsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Логика обработки выбора клиента
            var selectedClient = ClientsGrid.SelectedItem as Client;
            if (selectedClient != null)
            {
                // Действия с выбранным клиентом
            }
        }

        private void VisitsButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedClient = ClientsGrid.SelectedItem as Client;
            if (selectedClient != null)
            {
                var visitsWindow = new VisitsWindow(selectedClient);
                visitsWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("Выберите клиента для просмотра посещений.");
            }
        }
    }
}
