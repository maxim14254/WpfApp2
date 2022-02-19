using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WpfApp2
{
    public enum CreateOrUpdate
    {
        Create,
        Update
    }
    public class User : INotifyPropertyChanged
    {

        public CreateOrUpdate stasus;
        string id;
        string login;
        string name;
        string email;
        string password;
        List<string> rol;
        string connectionString;
        string[] path = System.Windows.Forms.Application.StartupPath.Split('\\');

        public User(string id, string login, string name, string email, string password, List<string> role)
        {
            this.id = id;
            this.login = login;
            this.name = name;
            this.email = email;
            this.password = password;
            this.rol = role;
            this.stasus = CreateOrUpdate.Update;
            connectionString = $@"Data Source=(LocalDB)\MSSQLLocalDB; AttachDbFilename={string.Join("\\", path.Take(path.Count() - 2))}\DATABASE1.MDF; Integrated Security=True";

        }
        public User()
        {
            connectionString = $@"Data Source=(LocalDB)\MSSQLLocalDB; AttachDbFilename={string.Join("\\", path.Take(path.Count() - 2))}\DATABASE1.MDF; Integrated Security=True";
            this.stasus = CreateOrUpdate.Create;
        }

        public string ID
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }

        public string Login
        {
            get
            {
                return login;
            }
            set
            {
                string cond = "^[A-Za-z]+$";

                if (Regex.IsMatch(value, cond))
                {
                    if (OnPropertyChanged(value))
                        login = value;
                }
                else
                    return;
            }
        }
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                OnPropertyChanged(value);
            }
        }
        public string Email
        {
            get
            {
                return email;
            }
            set
            {
                string cond = @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?";
                if (Regex.IsMatch(value, cond))
                {
                    if (OnPropertyChanged(value))
                        email = value;
                }
                else
                    return;
            }
        }
        public string Password
        {
            get
            {
                return password;
            }
            set
            {
                string cond = "^[A-Za-z0-9]{3,15}$";
                if (Regex.IsMatch(value, cond))
                {
                    password = value;
                    OnPropertyChanged(value);
                }
                else
                    return;
            }
        }
        public List<string> Rols
        {
            get
            {
                return rol;
            }
            set
            {
                rol = value;
                OnPropertyChanged(value);
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public bool OnPropertyChanged<T>(T value, [CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
            {
                var connection = new SqlConnection(connectionString);
                try
                {
                    if (propertyName == "Login" || propertyName == "Email")
                    {
                        string query = $"SELECT {propertyName} FROM Users";
                        var myCommand = new SqlCommand(query, new SqlConnection(connectionString));
                        SqlDataAdapter sqlData = new SqlDataAdapter();
                        sqlData.SelectCommand = myCommand;
                        var data = new DataTable();
                        sqlData.Fill(data);
                        
                        if (MainWindow.GetList(data).ConvertAll(obj => obj[0].ToString()).Contains(value.ToString()))
                            throw new Exception($"Данный {propertyName} уже существует, введите другой");
                    }

                    if (this.stasus == CreateOrUpdate.Update)
                    {
                        connection.Open();
                        SqlCommand command;
                        if (propertyName == "Rols")
                        {
                            command = new SqlCommand($"DELETE FROM User_Role WHERE UserID = @id", connection);
                            command.Parameters.Add("@id", SqlDbType.VarChar).Value = id;
                            command.ExecuteNonQuery();
                            for (int i = 0; i < Rols.Count; i++)
                            {
                                command = new SqlCommand($"DECLARE @rolesId INT " +
                                    $"SELECT @rolesId = (SELECT Roles.ID FROM Roles WHERE RoleName = @roleName) " +
                                    $"INSERT INTO User_Role VALUES(@id, @rolesId)", connection);

                                command.Parameters.Add("@roleName", SqlDbType.NVarChar).Value = Rols[i];
                                command.Parameters.Add("@id", SqlDbType.VarChar).Value = id;
                                command.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            command = new SqlCommand($"UPDATE Users SET {propertyName} = @val WHERE ID = @id", connection);

                            command.Parameters.Add("@val", SqlDbType.NVarChar).Value = value;
                            command.Parameters.Add("@id", SqlDbType.VarChar).Value = id;

                            if (command.ExecuteNonQuery() != 1)
                                throw new Exception($"Не удалось изменить {propertyName}");
                        }

                        connection.Close();
                    }

                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    connection.Close();
                    return false;
                }
            }
            else
                return true;
        }
    }
}
