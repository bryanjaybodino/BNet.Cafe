<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Config.ascx.cs" Inherits="BNet.Cafe.Server.Forms.Config" %>

<style>
    /* ==========================================
   CUSTOM TOGGLE SWITCHES
   ========================================== */
    .toggle-label {
        display: flex;
        justify-content: space-between;
        align-items: center;
        cursor: pointer;
        width: 100%;
    }

    .toggle-text {
        display: flex;
        flex-direction: column;
        gap: 2px;
    }

    .toggle-title {
        font-size: 14px;
        font-weight: 600;
        color: var(--text-light);
    }

    .toggle-desc {
        font-size: 12px;
        color: var(--text-light-secondary);
    }

    /* Switch Container */
    .switch {
        position: relative;
        display: inline-block;
        width: 44px;
        height: 24px;
        flex-shrink: 0;
    }

        .switch input {
            opacity: 0;
            width: 0;
            height: 0;
        }

    /* Slider Track */
    .slider {
        position: absolute;
        cursor: pointer;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        background-color: var(--border-light);
        transition: 0.3s;
    }

        .slider:before {
            position: absolute;
            content: "";
            height: 18px;
            width: 18px;
            left: 3px;
            bottom: 3px;
            background-color: white;
            transition: 0.3s;
        }

    /* Active State */
    .switch input:checked + .slider {
        background-color: var(--primary);
    }

        .switch input:checked + .slider:before {
            transform: translateX(20px);
        }

    /* Rounded Sliders */
    .slider.round {
        border-radius: 24px;
    }

        .slider.round:before {
            border-radius: 50%;
        }
</style>
<asp:UpdatePanel ID="UpdatePanel1" runat="server">
    <ContentTemplate>
        <div class="bnet-table-wrapper">
            <div class="content-header" style="margin-bottom: 24px;">
                <h1 style="font-size: 20px; display: flex; align-items: center; gap: 8px;">
                    <i class="fa-solid fa-sliders" style="color: var(--primary);"></i>Client Configuration
                </h1>
                <p>Manage system behaviors, timers, and client feature switches.</p>
            </div>

            <div class="card" style="max-width: 650px;">
                <div class="form-group" style="margin-bottom: 20px;">
                    <label for="<%= CheckBox_AccountCreationAllowed.ClientID %>" class="toggle-label">
                        <div class="toggle-text">
                            <span class="toggle-title">Account Creation Allowed</span>
                            <span class="toggle-desc">Enable or disable client-side user account registration.</span>
                        </div>
                        <label class="switch">
                            <asp:CheckBox ID="CheckBox_AccountCreationAllowed" runat="server" />
                            <span class="slider round"></span>
                        </label>
                    </label>
                </div>

                <div class="form-group" style="margin-bottom: 20px;">
                    <label for="<%= TextBox_AutoShutDownInterval.ClientID %>">Auto Shutdown Interval (Seconds) <span class="required">*</span></label>
                    <asp:TextBox ID="TextBox_AutoShutDownInterval" oninput="NumberOnly(this);" TextMode="Number" runat="server" CssClass="form-control" placeholder="300" MaxLength="10"></asp:TextBox>
                </div>

                <div class="form-group" style="margin-bottom: 20px;">
                    <label for="<%= CheckBox_DesktopSlideShow.ClientID %>" class="toggle-label">
                        <div class="toggle-text">
                            <span class="toggle-title">Desktop Slide Show</span>
                            <span class="toggle-desc">Enable background desktop image slideshow when idle.</span>
                        </div>
                        <label class="switch">
                            <asp:CheckBox ID="CheckBox_DesktopSlideShow" runat="server" />
                            <span class="slider round"></span>
                        </label>
                    </label>
                </div>

                <div class="form-group" style="margin-bottom: 20px;">
                    <label for="<%= CheckBox_ResetShutdownCountdown.ClientID %>" class="toggle-label">
                        <div class="toggle-text">
                            <span class="toggle-title">Reset Shutdown Countdown</span>
                            <span class="toggle-desc">Automatically reset timer on user interaction.</span>
                        </div>
                        <label class="switch">
                            <asp:CheckBox ID="CheckBox_ResetShutdownCountdown" runat="server" />
                            <span class="slider round"></span>
                        </label>
                    </label>
                </div>

                <div class="form-grid-action" style="margin-top: 24px;">
                    <span onclick="navigateTo('BNetPage.aspx?Form=Config')" class="btn btn-secondary">Reload</span>
                    <asp:LinkButton ID="LinkButton_Submit" OnClick="LinkButton_Submit_Click" CssClass="btn btn-primary" OnClientClick="return ValidateConfig();" runat="server">
                        <i class="fa fa-save"></i> Save Configuration
                    </asp:LinkButton>
                </div>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
