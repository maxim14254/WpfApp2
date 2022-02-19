using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WpfApp2
{
    public partial class Avtorization : Form
    {
        bool LoginVal = false;
        bool PasswordVal = false;
        public DataTable Data;
        public SqlConnection connection;
        public Avtorization()
        {
            InitializeComponent();

            LoginIn.Enabled = false;
        }

        private void LoginIn_Click(object sender, EventArgs e)
        {
            try
            {
                var path = Application.StartupPath.Split('\\');
                string connectionString = $@"Data Source=(LocalDB)\MSSQLLocalDB; AttachDbFilename={string.Join("\\", path.Take(path.Count() - 2))}\DATABASE1.MDF; Integrated Security=True";

                connection = new SqlConnection(connectionString);
                string query = $"SELECT * FROM Users WHERE Login=@log AND Password=@pas";

                SqlCommand myCommand = new SqlCommand(query, connection);
                myCommand.Parameters.Add("@log", SqlDbType.VarChar).Value = Login.Text;
                myCommand.Parameters.Add("@pas", SqlDbType.VarChar).Value = Password.Text;

                SqlDataAdapter sqlData = new SqlDataAdapter();
                Data = new DataTable();
                sqlData.SelectCommand = myCommand;
                sqlData.Fill(Data);

                if (Data.Rows.Count > 0)
                {
                    this.DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    MessageBox.Show("Неверный логин или(и) пароль!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ButtonEnabled()
        {
            if (PasswordVal && LoginVal)
                LoginIn.Enabled = true;
            else
                LoginIn.Enabled = false;
        }

        private void Login_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Login.Text))
                LoginVal = true;
            else
                LoginVal = false;

            ButtonEnabled();
        }

        private void Password_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Password.Text))
                PasswordVal = true;
            else
                PasswordVal = false;

            ButtonEnabled();
        }
    }
}
