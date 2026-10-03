using BNet.Cafe.Client.Ashx;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace BNet.Cafe.Client.Controls
{
    public partial class RegisterControl : UserControl
    {
        public event EventHandler SwitchToLoginRequested;
        public event EventHandler RegistrationCompleted;
        public event EventHandler UserActivityDetected;

        private readonly CreateUserHandler _createUserHandler = new CreateUserHandler();
        private readonly Color _defaultBorderColor = Color.FromArgb(51, 65, 85);
        private readonly Color _errorBorderColor = Color.FromArgb(239, 68, 68);

        public RegisterControl()
        {
            InitializeComponent();
        }

        private void OnActivity()
        {
            UserActivityDetected?.Invoke(this, EventArgs.Empty);
        }

        public void ClearFields()
        {
            txtFullName.Text = string.Empty;
            txtEmail.Text = string.Empty;
            txtPassword.Text = string.Empty;
            txtConfirmPassword.Text = string.Empty;
            ResetValidationState();
        }

        private void ResetValidationState()
        {
            txtFullName.BorderColor = _defaultBorderColor;
            txtEmail.BorderColor = _defaultBorderColor;
            txtPassword.BorderColor = _defaultBorderColor;
            txtConfirmPassword.BorderColor = _defaultBorderColor;
        }

        private async void BtnSubmit_Click(object sender, EventArgs e)
        {
            OnActivity();
            ResetValidationState();

            string name = txtFullName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text;
            string confirmPassword = txtConfirmPassword.Text;

            bool hasEmptyFields = false;

            if (string.IsNullOrWhiteSpace(name)) { txtFullName.BorderColor = _errorBorderColor; hasEmptyFields = true; }
            if (string.IsNullOrWhiteSpace(email)) { txtEmail.BorderColor = _errorBorderColor; hasEmptyFields = true; }
            if (string.IsNullOrWhiteSpace(password)) { txtPassword.BorderColor = _errorBorderColor; hasEmptyFields = true; }
            if (string.IsNullOrWhiteSpace(confirmPassword)) { txtConfirmPassword.BorderColor = _errorBorderColor; hasEmptyFields = true; }

            if (hasEmptyFields)
            {
                MessageBox.Show("All fields are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (password != confirmPassword)
            {
                txtPassword.BorderColor = _errorBorderColor;
                txtConfirmPassword.BorderColor = _errorBorderColor;
                MessageBox.Show("Passwords do not match.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var userHandler = new GetUserHandler();
            var data = await userHandler.GetByEmailAsync(email);
            if (data != null && data.Success != false && data.Data != null)
            {
                txtEmail.BorderColor = _errorBorderColor;
                MessageBox.Show($"{email} is already taken.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                btnSubmit.Enabled = false;
                ApiResponse response = await _createUserHandler.CreateUserAsync(email, password, name, "MEMBER");
                if (response != null && response.Success)
                {
                    MessageBox.Show("Account created successfully! You may now login.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RegistrationCompleted?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    MessageBox.Show(response?.Message ?? "Registration failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Server error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnSubmit.Enabled = true;
            }
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            OnActivity();
            SwitchToLoginRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}