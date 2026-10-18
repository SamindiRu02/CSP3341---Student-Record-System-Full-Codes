
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

using Font = System.Drawing.Font;
using Label = System.Windows.Forms.Label;

namespace StudentRecordSystem;

public sealed class MainForm : Form
{
    // Keep the student record indexed by ID
    private readonly Dictionary<string, Student> students =
        new(StringComparer.OrdinalIgnoreCase);

    private Student? currentStudent;

    // Save records in the local application data folder
    private readonly string dataFile = Path.Combine(
        Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData),
        "StudentRecordSystem",
        "students.json");

    private readonly TextBox txtStudentId = new();
    private readonly TextBox txtFullName = new();
    private readonly ComboBox cmbStatus = new();

    private readonly TextBox txtSearch = new();

    private readonly TextBox txtCourseCode = new();
    private readonly TextBox txtCourseName = new();
    private readonly TextBox txtCredits = new();
    private readonly TextBox txtMark = new();

    private readonly DataGridView grid = new();
    private readonly Label lblStudent = new();
    private readonly Label lblWam = new();

    private readonly Color navy = Color.FromArgb(30, 58, 95);
    private readonly Color blue = Color.FromArgb(37, 99, 235);


    // Set up the new window and load saved records
    public MainForm()
    {
        Text = "Student Record System";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(1050, 900);
        MinimumSize = new Size(950, 750);
        BackColor = Color.FromArgb(245, 247, 250);
        Font = new Font("Segoe UI", 10);

        BuildInterface();
        LoadRecords();
    }

    // Build the main sections of the form
    private void BuildInterface()
    {
        var main = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            Padding = new Padding(20),
            AutoScroll = true
        };

        main.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 100));

        // Giving the enough vertical space
        int[] heights = { 80, 185, 110, 220, 300 };

        foreach (int height in heights)
        {
            main.RowStyles.Add(
                new RowStyle(SizeType.Absolute, height));
        }

        Controls.Add(main);

        // Header
        var header = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = navy,
            Margin = new Padding(0, 0, 0, 12)
        };

        header.Controls.Add(new Label
        {
            Text = "STUDENT RECORD SYSTEM",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 19, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(20, 12)
        });

        header.Controls.Add(new Label
        {
            Text = "Academic Records Management",
            ForeColor = Color.LightGray,
            AutoSize = true,
            Location = new Point(22, 49)
        });

        main.Controls.Add(header, 0, 0);

        // Student registration
        var studentBox = CreateGroup("Student Registration");
        main.Controls.Add(studentBox, 0, 1);

        AddLabel(studentBox, "Student ID", 20, 35);
        SetInput(txtStudentId, studentBox, 150, 32, 240);

        AddLabel(studentBox, "Full Name", 420, 35);
        SetInput(txtFullName, studentBox, 550, 32, 300);

        AddLabel(studentBox, "Status", 20, 80);

        cmbStatus.Items.AddRange(
            new object[] { "Active", "Inactive" });
        cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbStatus.SelectedIndex = 0;
        cmbStatus.SetBounds(150, 78, 240, 30);
        studentBox.Controls.Add(cmbStatus);

        var btnCreate = CreateButton(
            "Register Student", blue, 420, 77);

        btnCreate.Click += CreateStudent;
        studentBox.Controls.Add(btnCreate);

        lblStudent.Text = "No student selected.";
        lblStudent.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        lblStudent.AutoSize = true;
        lblStudent.ForeColor = navy;
        lblStudent.Location = new Point(20, 130);
        studentBox.Controls.Add(lblStudent);

        // Student search
        var searchBox = CreateGroup("Find Existing Student");
        main.Controls.Add(searchBox, 0, 2);

        AddLabel(searchBox, "Student ID", 20, 38);
        SetInput(txtSearch, searchBox, 150, 35, 240);

        var btnSearch = CreateButton(
            "Search Student", blue, 420, 32);

        btnSearch.Click += SearchStudent;
        searchBox.Controls.Add(btnSearch);

        // Course enrolment
        var courseBox = CreateGroup("Course Enrolment");
        main.Controls.Add(courseBox, 0, 3);

        AddLabel(courseBox, "Course Code", 20, 35);
        SetInput(txtCourseCode, courseBox, 150, 32, 240);

        AddLabel(courseBox, "Course Name", 420, 35);
        SetInput(txtCourseName, courseBox, 550, 32, 300);

        AddLabel(courseBox, "Credits", 20, 85);
        SetInput(txtCredits, courseBox, 150, 82, 240);

        AddLabel(courseBox, "Mark (0-100)", 420, 85);
        SetInput(txtMark, courseBox, 550, 82, 300);

        var btnAdd = CreateButton(
            "Add Enrolment", blue, 20, 135);

        btnAdd.Click += AddEnrolment;
        courseBox.Controls.Add(btnAdd);

        // Enrolment records
        var recordsBox = CreateGroup("Enrolment Records");
        main.Controls.Add(recordsBox, 0, 4);

        grid.SetBounds(20, 35, 900, 175);
        grid.Anchor = AnchorStyles.Top |
                      AnchorStyles.Left |
                      AnchorStyles.Right;

        // To show enrolments without allowing direct table edits
        grid.ReadOnly = true;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.MultiSelect = false;
        grid.SelectionMode =
            DataGridViewSelectionMode.FullRowSelect;

        grid.AutoSizeColumnsMode =
            DataGridViewAutoSizeColumnsMode.Fill;

        grid.RowHeadersVisible = false;
        grid.BackgroundColor = Color.White;
        grid.BorderStyle = BorderStyle.None;

        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersDefaultCellStyle.BackColor = navy;
        grid.ColumnHeadersDefaultCellStyle.ForeColor =
            Color.White;

        grid.AlternatingRowsDefaultCellStyle.BackColor =
            Color.FromArgb(240, 245, 250);

        grid.Columns.Add("Code", "Course Code");
        grid.Columns.Add("Name", "Course Name");
        grid.Columns.Add("Credits", "Credits");
        grid.Columns.Add("Mark", "Mark");
        grid.Columns.Add("Grade", "Grade");

        recordsBox.Controls.Add(grid);

        var btnRemove = CreateButton(
            "Remove Selected",
            Color.FromArgb(185, 28, 28),
            20, 225);

        btnRemove.Click += RemoveSelectedEnrolment;
        recordsBox.Controls.Add(btnRemove);

        var btnWam = CreateButton(
            "Calculate WAM",
            Color.FromArgb(22, 101, 52),
            210, 225);

        btnWam.Click += CalculateWam;
        recordsBox.Controls.Add(btnWam);

        lblWam.Text = "Weighted Average: —";
        lblWam.Font = new Font(
            "Segoe UI", 11, FontStyle.Bold);
        lblWam.ForeColor = navy;
        lblWam.AutoSize = true;
        lblWam.Location = new Point(420, 232);
        recordsBox.Controls.Add(lblWam);
    }

    private static GroupBox CreateGroup(string title)
    {
        return new GroupBox
        {
            Text = title,
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Font = new Font(
                "Segoe UI", 10, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 12)
        };
    }

    private static void AddLabel(
        Control parent, string text, int x, int y)
    {
        parent.Controls.Add(new Label
        {
            Text = text,
            Location = new Point(x, y),
            AutoSize = true,
            Font = new Font("Segoe UI", 10)
        });
    }

    private static void SetInput(
        TextBox input, Control parent,
        int x, int y, int width)
    {
        input.SetBounds(x, y, width, 30);
        parent.Controls.Add(input);
    }

    private static Button CreateButton(
        string text, Color color, int x, int y)
    {
        var button = new Button
        {
            Text = text,
            BackColor = color,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Location = new Point(x, y),
            Size = new Size(175, 35),
            Cursor = Cursors.Hand
        };

        button.FlatAppearance.BorderSize = 0;
        return button;
    }

    // Register a student after checking details
    private void CreateStudent(object? sender, EventArgs e)
    {
        try
        {
            var student = new Student(
                txtStudentId.Text,
                txtFullName.Text,
                cmbStatus.Text);

            if (students.ContainsKey(student.StudentId))
            {
                ShowError("This Student ID already exists.");
                return;
            }

            students.Add(student.StudentId, student);

            if (!SaveRecords())
            {
                students.Remove(student.StudentId);
                return;
            }

            SelectStudent(student);

            MessageBox.Show(
                "Student registered and saved successfully.",
                "Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (ArgumentException ex)
        {
            ShowError(ex.Message);
        }
    }

    // To find a student using their student ID
    private void SearchStudent(object? sender, EventArgs e)
    {
        string id = txtSearch.Text.Trim();

        // To check if the student ID is entered
        if (string.IsNullOrWhiteSpace(id))
        {
            ShowError("Please enter a Student ID to search.");
            return;
        }

        if (students.TryGetValue(id, out Student? student))
        {
            SelectStudent(student);
        }
        else
        {
            ShowError("Student record not found.");
        }
    }

    // To show the selected student and the course
    private void SelectStudent(Student student)
    {
        currentStudent = student;

        txtStudentId.Text = student.StudentId;
        txtFullName.Text = student.FullName;
        cmbStatus.SelectedItem = student.Status;

        lblStudent.Text =
            $"Student: {student.StudentId} - " +
            $"{student.FullName} ({student.Status})";

        RefreshGrid();
        lblWam.Text = "Weighted Average: —";
    }

    // To add a course to selected student
    private void AddEnrolment(object? sender, EventArgs e)
    {
        if (currentStudent is null)
        {
            ShowError("Register or search for a student first.");
            return;
        }

        if (!int.TryParse(txtCredits.Text, out int credits))
        {
            ShowError("Credits must be a whole number.");
            return;
        }

        if (!decimal.TryParse(
            txtMark.Text,
            NumberStyles.Number,
            CultureInfo.CurrentCulture,
            out decimal mark))
        {
            ShowError("Enter a valid numeric mark.");
            return;
        }

        try
        {
            var course = new Course(
                txtCourseCode.Text,
                txtCourseName.Text,
                credits);

            var enrolment = new Enrolment(course, mark);

            currentStudent.AddEnrolment(enrolment);

            if (!SaveRecords())
            {
                currentStudent.RemoveEnrolment(course.Code);
                return;
            }

            RefreshGrid();
            lblWam.Text = "Weighted Average: —";

            MessageBox.Show(
                "Enrolment added and saved successfully.",
                "Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (ArgumentException ex)
        {
            ShowError(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            ShowError(ex.Message);
        }
    }

    // To reload the table
    private void RefreshGrid()
    {
        grid.Rows.Clear();

        if (currentStudent is null)
            return;

        foreach (var enrolment in currentStudent.Enrolments)
        {
            grid.Rows.Add(
                enrolment.Course.Code,
                enrolment.Course.Name,
                enrolment.Course.Credits,
                enrolment.Mark,
                enrolment.Grade);
        }
    }

    // To remove selected course
    private void RemoveSelectedEnrolment(
        object? sender, EventArgs e)
    {
        if (currentStudent is null ||
            grid.SelectedRows.Count == 0)
        {
            ShowError("Select an enrolment first.");
            return;
        }

        string code = grid.SelectedRows[0]
            .Cells[0].Value?.ToString() ?? "";

        var enrolment = currentStudent.Enrolments
            .FirstOrDefault(e => e.Course.Code == code);

        if (enrolment is null)
            return;

        if (currentStudent.RemoveEnrolment(code))
        {
            if (!SaveRecords())
            {
                currentStudent.AddEnrolment(enrolment);
                return;
            }

            RefreshGrid();
            lblWam.Text = "Weighted Average: —";
        }
    }

    // To calculate and display the weighted average mark
    private void CalculateWam(object? sender, EventArgs e)
    {
        if (currentStudent is null)
        {
            ShowError("Select a student first.");
            return;
        }

        try
        {
            decimal wam = currentStudent.CalculateWAM();

            lblWam.Text =
                $"Weighted Average: {wam:F2}";
        }
        catch (InvalidOperationException ex)
        {
            ShowError(ex.Message);
        }
    }

    // To save all student records as JSON.
    private bool SaveRecords()
    {
        try
        {
            string? folder = Path.GetDirectoryName(dataFile);

            if (folder is null)
                throw new IOException("Invalid storage location.");

            Directory.CreateDirectory(folder);

            var records = students.Values.Select(s => new
            {
                s.StudentId,
                s.FullName,
                s.Status,
                Enrolments = s.Enrolments.Select(e => new
                {
                    CourseCode = e.Course.Code,
                    CourseName = e.Course.Name,
                    Credits = e.Course.Credits,
                    e.Mark
                }).ToList()
            }).ToList();

            string json = JsonSerializer.Serialize(
                records,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

            // Write to a temporary file before replacing data.
            string tempFile = dataFile + ".tmp";
            File.WriteAllText(tempFile, json);
            File.Move(tempFile, dataFile, true);

            return true;
        }
        catch (Exception ex) when (
            ex is IOException ||
            ex is UnauthorizedAccessException)
        {
            ShowError("Could not save records: " + ex.Message);
            return false;
        }
    }

    // Restore previously saved student records.
    private void LoadRecords()
    {
        if (!File.Exists(dataFile))
            return;

        try
        {
            string json = File.ReadAllText(dataFile);

            var records = JsonSerializer.Deserialize<
                List<SavedStudent>>(json);

            if (records is null)
                return;

            var loaded = new Dictionary<string, Student>(
                StringComparer.OrdinalIgnoreCase);

            foreach (var record in records)
            {
                var student = new Student(
                    record.StudentId,
                    record.FullName,
                    record.Status);

                foreach (var item in record.Enrolments)
                {
                    var course = new Course(
                        item.CourseCode,
                        item.CourseName,
                        item.Credits);

                    student.AddEnrolment(
                        new Enrolment(course, item.Mark));
                }

                loaded.Add(student.StudentId, student);
            }

            students.Clear();

            foreach (var pair in loaded)
                students.Add(pair.Key, pair.Value);
        }
        catch (Exception ex) when (
            ex is IOException ||
            ex is UnauthorizedAccessException ||
            ex is JsonException ||
            ex is ArgumentException ||
            ex is InvalidOperationException)
        {
            ShowError(
                "Could not load saved records: " + ex.Message);
        }
    }

    // Data structures used only for reading the JSON file.
    private sealed class SavedStudent
    {
        public string StudentId { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Status { get; set; } = "";
        public List<SavedEnrolment> Enrolments { get; set; } = new();
    }

    private sealed class SavedEnrolment
    {
        public string CourseCode { get; set; } = "";
        public string CourseName { get; set; } = "";
        public int Credits { get; set; }
        public decimal Mark { get; set; }
    }

    private static void ShowError(string message)
    {
        MessageBox.Show(
            message,
            "Student Record System",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
    }
}
