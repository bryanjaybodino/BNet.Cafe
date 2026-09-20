using BNet.Cafe.Client.Ashx;
using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace BNet.Cafe.Client
{
    public partial class RegisterForm : Form
    {
        private readonly CreateUserHandler _createUserHandler = new CreateUserHandler();

        // Color definitions for validation states
        private readonly Color _defaultBorderColor = Color.FromArgb(203, 213, 225); // Slate 300
        private readonly Color _errorBorderColor = Color.FromArgb(239, 68, 68);     // Modern Red (Rose 500)

        public RegisterForm()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = true;
            this.ControlBox = false;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }

        private async void Button_SubmitRegister_Click(object sender, EventArgs e)
        {
            // Reset all borders to default state before validating
            ResetValidationState();
            string role = "USER";
            string name = TextBox_FullName.Text.Trim();
            string email = TextBox_Email.Text.Trim();
            string password = TextBox_Password.Text;
            string confirmPassword = TextBox_ConfirmPassword.Text;

            // 1. Required Fields Validation
            bool hasEmptyFields = false;

            if (string.IsNullOrWhiteSpace(name))
            {
                TextBox_FullName.BorderColor = _errorBorderColor;
                hasEmptyFields = true;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                TextBox_Email.BorderColor = _errorBorderColor;
                hasEmptyFields = true;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                TextBox_Password.BorderColor = _errorBorderColor;
                hasEmptyFields = true;
            }

            if (string.IsNullOrWhiteSpace(confirmPassword))
            {
                TextBox_ConfirmPassword.BorderColor = _errorBorderColor;
                hasEmptyFields = true;
            }

            if (hasEmptyFields)
            {
                MessageBox.Show("All fields are required. Please fill out the highlighted fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Email Format Validation
            if (!IsValidEmail(email))
            {
                TextBox_Email.BorderColor = _errorBorderColor;
                MessageBox.Show("Please enter a valid email address (e.g. user@example.com).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                TextBox_Email.Focus();
                return;
            }

            // 3. Confirm Password Match Validation
            if (password != confirmPassword)
            {
                TextBox_Password.BorderColor = _errorBorderColor;
                TextBox_ConfirmPassword.BorderColor = _errorBorderColor;
                MessageBox.Show("Passwords do not match. Please re-enter your password.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                TextBox_ConfirmPassword.Focus();
                return;
            }

            var userHandler = new UserHandler();
            var data = await userHandler.GetByEmailAsync(email);
            if (data != null && data.Success != false && data.Data != null)
            {
                TextBox_Email.BorderColor = _errorBorderColor;
                MessageBox.Show($"{email} is already taken.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                TextBox_Email.Focus();
                return;
            }

            Button_SubmitRegister.Enabled = false;

            try
            {          
                ApiResponse response = await _createUserHandler.CreateUserAsync(email, password, name, role);
                if (response != null && response.Success)
                {
                    MessageBox.Show("Account created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close(); // Return to existing MainForm
                }
                else
                {
                    string errorMsg = response?.Message ?? "Registration failed.";
                    MessageBox.Show(errorMsg, "Registration Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while connecting to the server: {ex.Message}", "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Button_SubmitRegister.Enabled = true;
            }
        }

        private void ResetValidationState()
        {
            TextBox_FullName.BorderColor = _defaultBorderColor;
            TextBox_Email.BorderColor = _defaultBorderColor;
            TextBox_Password.BorderColor = _defaultBorderColor;
            TextBox_ConfirmPassword.BorderColor = _defaultBorderColor;
        }

        private void Button_BackToLogin_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
            }
            catch
            {
                return false;
            }
        }
    }
}