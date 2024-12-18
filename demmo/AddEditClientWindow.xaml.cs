using demmo.DataBase;
using Microsoft.Win32;
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
using System.Windows.Shapes;

namespace demmo
{
    /// <summary>
    /// Interaction logic for AddEditClientWindow.xaml
    /// </summary>
    public partial class AddEditClientWindow : Window
    {
        private Client _client;
        private bool _isNewClient;

        public AddEditClientWindow(Client client = null)
        {
            InitializeComponent();
            _client = client ?? new Client();
            _isNewClient = client == null;

            LoadClientData();
            LoadTags();
        }
        public void LoadClientData()
        {
            if (_isNewClient)
            {
                IdBox.Text = "Будет сгенерирован автоматически";
            }
            else
            {
                IdBox.Text = _client.ID.ToString();
                LastNameBox.Text = _client.LastName;
                FirstNameBox.Text = _client.FirstName;
                PatronymicBox.Text = _client.Patronymic;
                PhoneBox.Text = _client.Phone;
                EmailBox.Text = _client.Email;
                BirthdayPicker.SelectedDate = _client.Birthday;
                MaleRadioButton.IsChecked = _client.GenderCode == "M";
                FemaleRadioButton.IsChecked = _client.GenderCode == "F";
                RegistrationDatePicker.SelectedDate = _client.RegistrationDate;

                if (!string.IsNullOrEmpty(_client.PhotoPath) && File.Exists(_client.PhotoPath))
                {
                    PhotoPreview.Source = new BitmapImage(new Uri(_client.PhotoPath));
                }

                LoadTags();
            }
        }

        private void LoadTags()
        {
            var tags = DatabaseConnectionClass.DatabaseConnection.Tag.ToList();
            TagsListBox.ItemsSource = tags;

            if (!_isNewClient)
            {
                var clientTags = DatabaseConnectionClass.DatabaseConnection.TagOfClient
                    .Where(tc => tc.ClientID == _client.ID)
                    .Select(tc => tc.TagID)
                    .ToList();

                foreach (Tag tag in TagsListBox.Items)
                {
                    if (clientTags.Contains(tag.ID))
                    {
                        TagsListBox.SelectedItems.Add(tag);
                    }
                }
            }
        }

        private void SelectPhoto_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg;*.png)|*.jpg;*.png",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var filePath = openFileDialog.FileName;
                if (new FileInfo(filePath).Length > 2 * 1024 * 1024) // Проверка на размер (2 МБ)
                {
                    MessageBox.Show("Размер фотографии не должен превышать 2 МБ.");
                    return;
                }

                _client.PhotoPath = filePath;
                PhotoPreview.Source = new BitmapImage(new Uri(filePath));
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация данных
                if (!ValidateInput())
                {
                    return;
                }

                // Заполнение данных клиента
                _client.LastName = LastNameBox.Text;
                _client.FirstName = FirstNameBox.Text;
                _client.Patronymic = PatronymicBox.Text;
                _client.Phone = PhoneBox.Text;
                _client.Email = EmailBox.Text;
                _client.Birthday = BirthdayPicker.SelectedDate ?? DateTime.Now;
                _client.GenderCode = MaleRadioButton.IsChecked == true ? "M" : "F";
                _client.RegistrationDate = RegistrationDatePicker.SelectedDate ?? DateTime.Now;

                // Сохранение в базу данных
                if (_isNewClient)
                {
                    DatabaseConnectionClass.DatabaseConnection.Client.Add(_client);
                }

                // Сохранение изменений в базе данных
                DatabaseConnectionClass.DatabaseConnection.SaveChanges();

                // Сохранение тегов
                SaveTags();

                // Обновление списка клиентов в главном окне
                ((MainWindow)Application.Current.MainWindow).LoadClients();

                // Закрытие окна
                Close();
            }
            catch (Exception ex)
            {
                // Вывод сообщения об ошибке
                MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}\n\n{ex.StackTrace}");
            }

            MessageBox.Show("Данные успешно сохранены!");
            Close();
        }

        private void SaveTags()
        {
            try
            {
                var selectedTags = TagsListBox.SelectedItems.Cast<Tag>()
                    .Select(tag => new TagOfClient { ClientID = _client.ID, TagID = tag.ID })
                    .ToList();

                var existingTags = DatabaseConnectionClass.DatabaseConnection.TagOfClient
                    .Where(tc => tc.ClientID == _client.ID)
                    .ToList();

                // Удаляем старые теги
                foreach (var tag in existingTags)
                {
                    DatabaseConnectionClass.DatabaseConnection.TagOfClient.Remove(tag);
                }

                // Добавляем новые теги
                foreach (var tag in selectedTags)
                {
                    DatabaseConnectionClass.DatabaseConnection.TagOfClient.Add(tag);
                }

                DatabaseConnectionClass.DatabaseConnection.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении тегов: {ex.Message}\n\n{ex.StackTrace}");
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(LastNameBox.Text) || LastNameBox.Text.Length > 50)
            {
                MessageBox.Show("Фамилия должна быть заполнена и не превышать 50 символов.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(FirstNameBox.Text) || FirstNameBox.Text.Length > 50)
            {
                MessageBox.Show("Имя должно быть заполнено и не превышать 50 символов.");
                return false;
            }

            if (PatronymicBox.Text?.Length > 50)
            {
                MessageBox.Show("Отчество не должно превышать 50 символов.");
                return false;
            }

            if (!IsValidEmail(EmailBox.Text))
            {
                MessageBox.Show("Некорректный email.");
                return false;
            }

            if (!IsValidPhone(PhoneBox.Text))
            {
                MessageBox.Show("Некорректный номер телефона.");
                return false;
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPhone(string phone)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(phone, @"^\+?\d{1,4}?[-.\s]?\(?\d{1,3}?\)?[-.\s]?\d{1,4}[-.\s]?\d{1,4}[-.\s]?\d{1,9}$");
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
