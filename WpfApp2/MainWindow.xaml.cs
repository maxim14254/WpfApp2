using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp2
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SqlConnection connection;
        public ObservableCollection<User> Users { get; set; }
        public List<string> Rols { get; set; }
        public string ThisRol { get; set; }
        public bool IsAdmin { get; set; }
        public User User { get; set; }
        public MainWindow()
        {

            Avtorization avtorization = new Avtorization();
            var form = avtorization.ShowDialog();
            if (form != System.Windows.Forms.DialogResult.OK)
            {
                this.Close();
                return;
            }

            string[] path = System.Windows.Forms.Application.StartupPath.Split('\\');
            string connectionString = $@"Data Source=(LocalDB)\MSSQLLocalDB; AttachDbFilename={string.Join("\\", path.Take(path.Count() - 2))}\DATABASE1.MDF; Integrated Security=True";

            connection = new SqlConnection(connectionString);

            string query = $"SELECT RoleName FROM Roles JOIN User_Role ON RoleID=ID WHERE UserID=@Id";

            SqlCommand myCommand = new SqlCommand(query, connection);
            myCommand.Parameters.Add("@Id", SqlDbType.Int).Value = GetList(avtorization.Data).First()[0];

            SqlDataAdapter sqlData = new SqlDataAdapter();
            sqlData.SelectCommand = myCommand;
            DataTable data = new DataTable();
            sqlData.Fill(data);
            IsAdmin = GetList(data).Where(w => w.Contains("Администратор")).Count() > 0;
            if (IsAdmin)
            {
                ThisRol = "Администратор";
            }
            else
            {
                ThisRol = "Редактор справочников";
            }

            query = $"SELECT * FROM Users";
            myCommand = new SqlCommand(query, connection);
            sqlData.SelectCommand = myCommand;
            data = new DataTable();
            sqlData.Fill(data);
            Users = new ObservableCollection<User>();
            GetList(data).ForEach(obj =>
            {
                query = $"SELECT RoleName FROM Roles JOIN User_Role ON RoleID=ID WHERE UserID=@Id";
                myCommand = new SqlCommand(query, connection);
                myCommand.Parameters.Add("@Id", SqlDbType.Int).Value = obj[0].ToString();
                sqlData.SelectCommand = myCommand;
                var data2 = new DataTable();
                sqlData.Fill(data2);

                Users.Add(new User(obj[0].ToString(), obj[1].ToString(), obj[2].ToString(), obj[3].ToString(), obj[4].ToString(), GetList(data2).ConvertAll(obj2 => obj2[0].ToString())));
            });

            query = $"SELECT RoleName FROM Roles";
            myCommand = new SqlCommand(query, connection);
            sqlData.SelectCommand = myCommand;
            data = new DataTable();
            sqlData.Fill(data);
            Rols = GetList(data).ConvertAll(obj => obj[0].ToString());

            InitializeComponent();

            if (ThisRol != "Администратор")
            {
                pass.Visibility = Visibility.Hidden;
            }
        }

        public static List<object[]> GetList(DataTable data)
        {
            List<object[]> rez = new List<object[]>();
            for (int i = 0; i < data.Rows.Count; i++)
            {
                rez.Add(data.Rows[i].ItemArray);
            }
            return rez;
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (User != null)
            {
                AddRoles addRoles = new AddRoles(Rols, User.Rols ?? new List<string>());
                addRoles.ShowDialog();
                User.Rols = addRoles.Rezult;
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Users.Add(new User());
            button.IsEnabled = true;
        }

        private void button_Click_1(object sender, RoutedEventArgs e)
        {
            button.IsEnabled = false;
            
            var newUsers = Users.Where(w => w.stasus == CreateOrUpdate.Create).ToList();
            try
            {
                connection.Open();
                for (int i = 0; i < newUsers.Count; i++)
                {
                    if (!(string.IsNullOrEmpty(newUsers[i].Login) || string.IsNullOrEmpty(newUsers[i].Name) || string.IsNullOrEmpty(newUsers[i].Password)))
                    {
                        SqlCommand command = new SqlCommand($"INSERT INTO Users (Login, Name, Password, Email) VALUES (@login, @name, @pass, @email)", connection);

                        command.Parameters.Add("@login", SqlDbType.VarChar).Value = newUsers[i].Login;
                        command.Parameters.Add("@name", SqlDbType.NVarChar).Value = newUsers[i].Name;
                        command.Parameters.Add("@pass", SqlDbType.VarChar).Value = newUsers[i].Password;
                        command.Parameters.Add("@email", SqlDbType.VarChar).Value = newUsers[i].Email == null ? "" : newUsers[i].Email;

                        if (command.ExecuteNonQuery() == 1)
                        {
                            for (int j = 0; j < newUsers[i].Rols?.Count; j++)
                            {
                                command = new SqlCommand($"INSERT INTO User_Role(UserID, RoleID) VALUES ((SELECT ID FROM Users WHERE Login = @login), (SELECT ID FROM Roles WHERE RoleName = @roleId))", connection);
                                command.Parameters.Add("@login", SqlDbType.VarChar).Value = newUsers[i].Login;
                                command.Parameters.Add("@roleId", SqlDbType.NVarChar).Value = newUsers[i].Rols[j];
                                if (command.ExecuteNonQuery() != 1)
                                    throw new Exception($"Не удалось добавить роли пользователю \"{newUsers[i].Login}\"");
                                newUsers[i].stasus = CreateOrUpdate.Update;
                            }
                        }
                        else
                        {
                            Users.Remove(newUsers[i]);
                            throw new Exception($"Не удалось добавить пользователя \"{newUsers[i].Login}\"");
                        }
                    }
                    else
                    {
                        button.IsEnabled = true;
                        throw new Exception($"Заполните все колонки для пользователя \"{newUsers[i].Login}\" и попробуйте сохранить еще раз");
                    }
                }
            }
            catch(Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, "Ошибка", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            finally
            {
                connection.Close();
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (User != null) 
            {
                connection.Open();

                var command = new SqlCommand($"DELETE FROM Users WHERE ID = (SELECT ID FROM Users WHERE Login = @login);", connection);
                command.Parameters.Add("@login", SqlDbType.VarChar).Value = User.Login;

                if (command.ExecuteNonQuery() != 1)
                    System.Windows.Forms.MessageBox.Show($"Не удалось удалить пользователя \"{User.Login}\"", "Ошибка", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                else
                    Users.Remove(User);

                connection.Close();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (connection.State == ConnectionState.Open) 
                connection.Close();
        }
    }
}
