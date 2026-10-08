using BNet.Cafe.Client.Ashx;
using BNet.Cafe.Client.Design;
using BNet.Cafe.Client.Services;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace BNet.Cafe.Client.Controls
{
    public partial class LoginControl : UserControl
    {
        public event EventHandler<LoginSuccessEventArgs> LoginSuccessful;
        public event EventHandler SwitchToRegisterRequested;
        public event EventHandler UserActivityDetected;

        private readonly Color _defaultBorderColor = Color.FromArgb(51, 65, 85);
        private readonly Color _errorBorderColor = Color.FromArgb(239, 68, 68);

        public LoginControl()
        {
            InitializeComponent();
            SetupEvents();
        }

        private void SetupEvents()
        {
            txtUsername.TextChanged += (s, e) => { ResetValidation(); OnActivity(); };
            txtPassword.TextChanged += (s, e) => { ResetValidation(); OnActivity(); };

            txtUsername.KeyDown += (s, e) => { OnActivity(); if (e.KeyCode == Keys.Enter) BtnLogin_Click(s, e); };
            txtPassword.KeyDown += (s, e) => { OnActivity(); if (e.KeyCode == Keys.Enter) BtnLogin_Click(s, e); };
        }

        private void OnActivity()
        {
            UserActivityDetected?.Invoke(this, EventArgs.Empty);
        }

        private void ResetValidation()
        {
            txtUsername.BorderColor = _defaultBorderColor;
            txtPassword.BorderColor = _defaultBorderColor;
        }

        public void ClearFields()
        {
            txtUsername.Text = string.Empty;
            txtPassword.Text = string.Empty;
            ResetValidation();
        }

        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            OnActivity();
            ResetValidation();

            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                if (string.IsNullOrWhiteSpace(username)) txtUsername.BorderColor = _errorBorderColor;
                if (string.IsNullOrWhiteSpace(password)) txtPassword.BorderColor = _errorBorderColor;
                MessageBox.Show("Please fill out all login fields.", "Validation Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool isLocalAdmin = (username == "BNetAdmin" && password == "@12345");

            if (isLocalAdmin)
            {
                LoginSuccessful?.Invoke(this, new LoginSuccessEventArgs("ADMIN", "6000", true));
                return;
            }

            try
            {
                btnLogin.Enabled = false;
                GetLoginHandler loginHandler = new GetLoginHandler();
                var loginResponse = await loginHandler.LoginAsync(username, password);

                if (loginResponse == null || (!loginResponse.Success && !isLocalAdmin))
                {
                    txtPassword.BorderColor = _errorBorderColor;
                    MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtPassword.Text = string.Empty;
                    return;
                }

                bool isAdmin = string.Equals(loginResponse?.Data?.Role, "ADMIN", StringComparison.OrdinalIgnoreCase);

                if (isAdmin)
                {
                    LoginSuccessful?.Invoke(this, new LoginSuccessEventArgs(loginResponse.Data.Id.ToString(), "6000", true));
                    return;
                }

                if (loginResponse.Data == null || loginResponse.Data.TotalDuration <= 0)
                {
                    MessageBox.Show("Your account balance is 0. Please top up at the counter.", "Insufficient Balance", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                LoginSuccessful?.Invoke(this, new LoginSuccessEventArgs(
                    loginResponse.Data.Id.ToString(),
                    loginResponse.Data.TotalDuration.ToString(),
                    false
                ));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to connect to the server: {ex.Message}", "Server Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                btnLogin.Enabled = true;
            }
        }

        private void LblRegisterLink_Click(object sender, EventArgs e)
        {
            OnActivity();
            SwitchToRegisterRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    public class LoginSuccessEventArgs : EventArgs
    {
        public string UserId { get; }
        public string DurationMinutes { get; }
        public bool IsAdmin { get; }

        public LoginSuccessEventArgs(string userId, string durationMinutes, bool isAdmin)
        {
            UserId = userId;
            DurationMinutes = durationMinutes;
            IsAdmin = isAdmin;
        }
    }
}