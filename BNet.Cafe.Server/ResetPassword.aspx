<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ResetPassword.aspx.cs" Inherits="BNet.Cafe.Server.ResetPassword" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <link href="Assets/fontawesome/font-awesome.min.css" rel="stylesheet" />
    <title>BNet Cafe - Reset Password</title>
    <% Response.Write(BNet.Cafe.Server.Services.FileCssHelper.StyleSheetVersion("Assets/ResetPassword/Style.css")); %>
    <% Response.Write(BNet.Cafe.Server.Services.FileCssHelper.StyleSheetVersion("Assets/fontawesome/font-awesome.min.css")); %>
</head>
<body>
    <div id="toastWrapper" class="toast-wrapper"></div>

    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" EnableCdn="false" EnablePageMethods="true" EnablePartialRendering="true" AsyncPostBackTimeout="99999999" ScriptMode="Release" ValidateRequestMode="Enabled" EnableScriptLocalization="true" EnableScriptGlobalization="true" LoadScriptsBeforeUI="false" CompositeScript-ScriptMode="Release" CompositeScript-ResourceUICultures="Release" runat="server"></asp:ScriptManager>
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
                <div class="reset-card">
                    <div class="card-header">
                        <div class="icon-badge">
                            <i class="fa-solid fa-key"></i>
                        </div>
                        <h1>Reset Password</h1>
                        <p>Enter your new password below.</p>
                    </div>

                    <div class="form-group">
                        <label for="<%= TextBox_Password.ClientID %>">New Password <span class="required-star">*</span></label>
                        <asp:TextBox ID="TextBox_Password" runat="server" TextMode="Password" CssClass="form-control" placeholder="Enter new password" MaxLength="100"></asp:TextBox>
                    </div>

                    <div class="form-group">
                        <label for="<%= TextBox_ConfirmPassword.ClientID %>">Confirm Password <span class="required-star">*</span></label>
                        <asp:TextBox ID="TextBox_ConfirmPassword" runat="server" TextMode="Password" CssClass="form-control" placeholder="Confirm new password" MaxLength="100"></asp:TextBox>
                    </div>

                    <asp:LinkButton ID="LinkButton_Submit" OnClick="LinkButton_Submit_Click" CssClass="btn-submit" OnClientClick="return Validate();" runat="server">
                        <i class="fa-solid fa-floppy-disk"></i> Change Password
                    </asp:LinkButton>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </form>
</body>
</html>
