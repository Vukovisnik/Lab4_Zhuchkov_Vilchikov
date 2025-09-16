using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace UniversityManagementSystem
{
    public partial class Form1 : Form
    {
        private TextBox textBoxLogin, textBoxPassword;
        private Button buttonLogin;
        private Label labelError, labelLogin, labelPassword;
        private DataGridView dataGridViewGrades;
        private Button buttonRefreshStudent;
        private Label labelStudentWelcome;
        private ComboBox comboBoxStudents, comboBoxSubjects;
        private NumericUpDown numericUpDownGrade;
        private DateTimePicker dateTimePickerDate;
        private Button buttonAddGrade, buttonRefreshTeacher;
        private Label labelTeacherWelcome, labelStudent, labelSubject, labelGrade, labelDate;
        private DataGridView dataGridViewUsers;
        private Button buttonRefreshAdmin, buttonAddUser;
        private Label labelAdminWelcome;

        private User currentUser;
        private string connectionString = "Data Source=SQL;Initial Catalog=ISPP6_Var3;Integrated Security=true";

        public Form1()
        {
            form();
            InitializeComponent();
            ShowAuthForm();
        }

        private void form()
        {
            this.Text = "Система управления учебным процессом";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void ShowAuthForm()
        {
            ClearAllControls();

            labelLogin = new Label();
            labelLogin.Text = "Логин:";
            labelLogin.Location = new Point(300, 150);
            labelLogin.Size = new Size(100, 20);
            this.Controls.Add(labelLogin);
            textBoxLogin = new TextBox();
            textBoxLogin.Location = new Point(400, 150);
            textBoxLogin.Size = new Size(200, 20);
            this.Controls.Add(textBoxLogin);
            labelPassword = new Label();
            labelPassword.Text = "Пароль:";
            labelPassword.Location = new Point(300, 180);
            labelPassword.Size = new Size(100, 20);
            this.Controls.Add(labelPassword);
            textBoxPassword = new TextBox();
            textBoxPassword.Location = new Point(400, 180);
            textBoxPassword.Size = new Size(200, 20);
            textBoxPassword.PasswordChar = '*';
            this.Controls.Add(textBoxPassword);
            buttonLogin = new Button();
            buttonLogin.Text = "Войти";
            buttonLogin.Location = new Point(400, 210);
            buttonLogin.Size = new Size(100, 30);
            buttonLogin.Click += ButtonLogin_Click;
            this.Controls.Add(buttonLogin);
            labelError = new Label();
            labelError.Text = "";
            labelError.ForeColor = Color.Red;
            labelError.Location = new Point(300, 250);
            labelError.Size = new Size(300, 40);
            this.Controls.Add(labelError);
            Label testData = new Label();
            testData.Text = "Тестовые данные:\nadmin/admin123 (Админ)\nteacher_ivanova/teach123 (Преподаватель)\nstudent_sidorov/stud123 (Студент)";
            testData.Location = new Point(300, 300);
            testData.Size = new Size(300, 60);
            this.Controls.Add(testData);
        }

        private void ButtonLogin_Click(object sender, EventArgs e)
        {
            string login = textBoxLogin.Text.Trim();
            string password = textBoxPassword.Text;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                labelError.Text = "Введите логин и пароль!";
                return;
            }

            string query = @"
                SELECT u.UserID, u.Login, u.Password, u.FirstName, u.LastName, 
                       u.RoleID, r.RoleName 
                FROM Users u 
                INNER JOIN Roles r ON u.RoleID = r.RoleID 
                WHERE u.Login = @Login AND u.Password = @Password";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Login", login);
                    command.Parameters.AddWithValue("@Password", password);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            currentUser = new User
                            {
                                UserID = reader.GetInt32(0),
                                Login = reader.GetString(1),
                                Password = reader.GetString(2),
                                FirstName = reader.GetString(3),
                                LastName = reader.GetString(4),
                                RoleID = reader.GetInt32(5),
                                RoleName = reader.GetString(6)
                            };

                            ClearAllControls();

                            switch (currentUser.RoleName)
                            {
                                case "Administrator":
                                    ShowAdminForm();
                                    break;
                                case "Teacher":
                                    ShowTeacherForm();
                                    break;
                                case "Student":
                                    ShowStudentForm();
                                    break;
                            }
                        }
                        else
                        {
                            labelError.Text = "Неверный логин или пароль!";
                        }
                    }
                }
            }
        }

        private void ShowStudentForm()
        {
            this.Text = $"Студент: {currentUser.FirstName} {currentUser.LastName}";

            labelStudentWelcome = new Label();
            labelStudentWelcome.Text = $"Добро пожаловать, {currentUser.FirstName} {currentUser.LastName}!\nВаши оценки:";
            labelStudentWelcome.Location = new Point(20, 20);
            labelStudentWelcome.Size = new Size(400, 40);
            this.Controls.Add(labelStudentWelcome);
            dataGridViewGrades = new DataGridView();
            dataGridViewGrades.Location = new Point(20, 70);
            dataGridViewGrades.Size = new Size(740, 400);
            this.Controls.Add(dataGridViewGrades);
            buttonRefreshStudent = new Button();
            buttonRefreshStudent.Text = "Обновить";
            buttonRefreshStudent.Location = new Point(20, 480);
            buttonRefreshStudent.Size = new Size(100, 30);
            buttonRefreshStudent.Click += (s, e) => LoadStudentGrades();
            this.Controls.Add(buttonRefreshStudent);
            Button buttonLogout = new Button();
            buttonLogout.Text = "Выйти";
            buttonLogout.Location = new Point(660, 480);
            buttonLogout.Size = new Size(100, 30);
            buttonLogout.Click += (s, e) => ShowAuthForm();
            this.Controls.Add(buttonLogout);

            LoadStudentGrades();
        }

        private void LoadStudentGrades()
        {
            string query = @"
                SELECT s.SubjectName as Предмет, g.Grade as Оценка, g.Date as Дата
                FROM Grades g
                INNER JOIN Subjects s ON g.SubjectID = s.SubjectID
                WHERE g.StudentID = @StudentID
                ORDER BY g.Date DESC";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StudentID", currentUser.UserID);
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    dataGridViewGrades.DataSource = table;
                }
            }
        }

        private void ShowTeacherForm()
        {
            this.Text = $"Преподаватель: {currentUser.FirstName} {currentUser.LastName}";

            labelTeacherWelcome = new Label();
            labelTeacherWelcome.Text = $"Добро пожаловать, {currentUser.FirstName} {currentUser.LastName}!";
            labelTeacherWelcome.Location = new Point(20, 20);
            labelTeacherWelcome.Size = new Size(400, 20);
            this.Controls.Add(labelTeacherWelcome);
            labelStudent = new Label();
            labelStudent.Text = "Студент:";
            labelStudent.Location = new Point(20, 60);
            labelStudent.Size = new Size(100, 20);
            this.Controls.Add(labelStudent);
            comboBoxStudents = new ComboBox();
            comboBoxStudents.Location = new Point(120, 60);
            comboBoxStudents.Size = new Size(200, 20);
            this.Controls.Add(comboBoxStudents);
            labelSubject = new Label();
            labelSubject.Text = "Предмет:";
            labelSubject.Location = new Point(20, 90);
            labelSubject.Size = new Size(100, 20);
            this.Controls.Add(labelSubject);
            comboBoxSubjects = new ComboBox();
            comboBoxSubjects.Location = new Point(120, 90);
            comboBoxSubjects.Size = new Size(200, 20);
            this.Controls.Add(comboBoxSubjects);
            labelGrade = new Label();
            labelGrade.Text = "Оценка:";
            labelGrade.Location = new Point(20, 120);
            labelGrade.Size = new Size(100, 20);
            this.Controls.Add(labelGrade);
            numericUpDownGrade = new NumericUpDown();
            numericUpDownGrade.Location = new Point(120, 120);
            numericUpDownGrade.Size = new Size(100, 20);
            numericUpDownGrade.Minimum = 1;
            numericUpDownGrade.Maximum = 5;
            numericUpDownGrade.Value = 5;
            this.Controls.Add(numericUpDownGrade);
            labelDate = new Label();
            labelDate.Text = "Дата:";
            labelDate.Location = new Point(20, 150);
            labelDate.Size = new Size(100, 20);
            this.Controls.Add(labelDate);
            dateTimePickerDate = new DateTimePicker();
            dateTimePickerDate.Location = new Point(120, 150);
            dateTimePickerDate.Size = new Size(200, 20);
            dateTimePickerDate.Value = DateTime.Today;
            this.Controls.Add(dateTimePickerDate);
            buttonAddGrade = new Button();
            buttonAddGrade.Text = "Добавить оценку";
            buttonAddGrade.Location = new Point(20, 180);
            buttonAddGrade.Size = new Size(150, 30);
            buttonAddGrade.Click += ButtonAddGrade_Click;
            this.Controls.Add(buttonAddGrade);
            buttonRefreshTeacher = new Button();
            buttonRefreshTeacher.Text = "Обновить списки";
            buttonRefreshTeacher.Location = new Point(180, 180);
            buttonRefreshTeacher.Size = new Size(150, 30);
            buttonRefreshTeacher.Click += (s, e) => LoadTeacherData();
            this.Controls.Add(buttonRefreshTeacher);
            Button buttonLogout = new Button();
            buttonLogout.Text = "Выйти";
            buttonLogout.Location = new Point(660, 480);
            buttonLogout.Size = new Size(100, 30);
            buttonLogout.Click += (s, e) => ShowAuthForm();
            this.Controls.Add(buttonLogout);

            LoadTeacherData();
        }

        private void LoadTeacherData()
        {
            string studentsQuery = "SELECT UserID, FirstName + ' ' + LastName as FullName FROM Users WHERE RoleID = 3";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlDataAdapter adapter = new SqlDataAdapter(studentsQuery, connection);
                DataTable table = new DataTable();
                adapter.Fill(table);
                comboBoxStudents.DataSource = table;
                comboBoxStudents.DisplayMember = "FullName";
                comboBoxStudents.ValueMember = "UserID";
            }

            string subjectsQuery = "SELECT SubjectID, SubjectName FROM Subjects";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlDataAdapter adapter = new SqlDataAdapter(subjectsQuery, connection);
                DataTable table = new DataTable();
                adapter.Fill(table);
                comboBoxSubjects.DataSource = table;
                comboBoxSubjects.DisplayMember = "SubjectName";
                comboBoxSubjects.ValueMember = "SubjectID";
            }
        }

        private void ButtonAddGrade_Click(object sender, EventArgs e)
        {
            if (comboBoxStudents.SelectedValue == null || comboBoxSubjects.SelectedValue == null)
            {
                MessageBox.Show("Выберите студента и предмет!");
                return;
            }

            int studentID = (int)comboBoxStudents.SelectedValue;
            int subjectID = (int)comboBoxSubjects.SelectedValue;
            int grade = (int)numericUpDownGrade.Value;
            DateTime date = dateTimePickerDate.Value;

            string query = @"
                INSERT INTO Grades (StudentID, SubjectID, Grade, Date)
                VALUES (@StudentID, @SubjectID, @Grade, @Date)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StudentID", studentID);
                    command.Parameters.AddWithValue("@SubjectID", subjectID);
                    command.Parameters.AddWithValue("@Grade", grade);
                    command.Parameters.AddWithValue("@Date", date);

                    int result = command.ExecuteNonQuery();
                    if (result > 0)
                    {
                        MessageBox.Show("Оценка добавлена успешно!");
                    }
                }
            }
        }

        private void ShowAdminForm()
        {
            this.Text = $"Администратор: {currentUser.FirstName} {currentUser.LastName}";

            labelAdminWelcome = new Label();
            labelAdminWelcome.Text = $"Добро пожаловать, {currentUser.FirstName} {currentUser.LastName}!\nСписок пользователей:";
            labelAdminWelcome.Location = new Point(20, 20);
            labelAdminWelcome.Size = new Size(400, 40);
            this.Controls.Add(labelAdminWelcome);
            dataGridViewUsers = new DataGridView();
            dataGridViewUsers.Location = new Point(20, 70);
            dataGridViewUsers.Size = new Size(740, 400);
            this.Controls.Add(dataGridViewUsers);

            buttonRefreshAdmin = new Button();
            buttonRefreshAdmin.Text = "Обновить";
            buttonRefreshAdmin.Location = new Point(20, 480);
            buttonRefreshAdmin.Size = new Size(100, 30);
            buttonRefreshAdmin.Click += (s, e) => LoadAdminData();
            this.Controls.Add(buttonRefreshAdmin);
            buttonAddUser = new Button();
            buttonAddUser.Text = "Добавить пользователя";
            buttonAddUser.Location = new Point(130, 480);
            buttonAddUser.Size = new Size(150, 30);
            buttonAddUser.Click += (s, e) => MessageBox.Show("Функция добавления пользователя будет реализована здесь");
            this.Controls.Add(buttonAddUser);

            Button buttonLogout = new Button();
            buttonLogout.Text = "Выйти";
            buttonLogout.Location = new Point(660, 480);
            buttonLogout.Size = new Size(100, 30);
            buttonLogout.Click += (s, e) => ShowAuthForm();
            this.Controls.Add(buttonLogout);

            LoadAdminData();
        }

        private void LoadAdminData()
        {
            string query = @"
                SELECT u.UserID, u.Login, u.FirstName, u.LastName, r.RoleName
                FROM Users u
                INNER JOIN Roles r ON u.RoleID = r.RoleID
                ORDER BY u.UserID";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                DataTable table = new DataTable();
                adapter.Fill(table);
                dataGridViewUsers.DataSource = table;
            }
        }

        private void ClearAllControls()
        {
            foreach (Control control in this.Controls)
            {
                control.Dispose();
            }
            this.Controls.Clear();
        }

        public class User
        {
            public int UserID { get; set; }
            public string Login { get; set; }
            public string Password { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public int RoleID { get; set; }
            public string RoleName { get; set; }
        }
    }
}