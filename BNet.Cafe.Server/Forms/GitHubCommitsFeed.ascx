<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="GitHubCommitsFeed.ascx.cs" Inherits="BNet.Cafe.Server.Forms.Controls.GitHubCommitsFeed" %>

<div class="card" style="margin-top: 16px;">


    <!-- System Notice & Deployment Instructions -->
    <div style="background-color: var(--bg-light-tertiary); border-left: 4px solid #3b82f6; border-radius: 6px; padding: 14px; margin-bottom: 16px; display: flex; flex-direction: column; gap: 12px;">

        <!-- Account Credentials -->
        <div>
            <div style="display: flex; align-items: center; gap: 8px; margin-bottom: 6px;">
                <i class="fas fa-key" style="color: #3b82f6; font-size: 14px;"></i>
                <span style="font-weight: 600; font-size: 13px; color: var(--text-light);">Client Bypass Credentials</span>
            </div>
            <div style="display: flex; gap: 16px; font-size: 12px; font-family: monospace; color: var(--text-light-secondary);">
                <div><strong>Username:</strong> <code style="background: rgba(0,0,0,0.15); padding: 2px 6px; border-radius: 4px; color: var(--text-light);">BNetAdmin</code></div>
                <div><strong>Password:</strong> <code style="background: rgba(0,0,0,0.15); padding: 2px 6px; border-radius: 4px; color: var(--text-light);">@12345</code></div>
            </div>
        </div>

        <hr style="border: 0; border-top: 1px solid var(--border-light); margin: 2px 0;" />

        <!-- Update / Deployment Notes -->
        <div>
            <div style="display: flex; align-items: center; gap: 8px; margin-bottom: 6px;">
                <i class="fas fa-arrows-rotate" style="color: #f59e0b; font-size: 14px;"></i>
                <span style="font-weight: 600; font-size: 13px; color: var(--text-light);">Update & Deployment Guidelines</span>
            </div>
            <ul style="margin: 0; padding-left: 18px; font-size: 12px; color: var(--text-light-secondary); display: flex; flex-direction: column; gap: 4px;">
                <li>
                    <strong>Client Application Updates:</strong> For existing client installations, do <strong>NOT</strong> overwrite <code style="background: rgba(0,0,0,0.15); padding: 1px 5px; border-radius: 3px; color: #ef4444;">BNet.Cafe.Client.exe.config</code> to prevent resetting the client configuration settings.
                </li>
                <li>
                    <strong>Server Application Updates:</strong> Replace <strong>ALL</strong> server files during deployment to ensure full update compatibility.
                </li>
            </ul>
        </div>

    </div>

    <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px;">
        <div style="display: flex; align-items: center; gap: 10px;">
            <i class="fab fa-github" style="font-size: 20px; color: var(--text-light);"></i>
            <h3 style="font-size: 16px; font-weight: 600; color: var(--text-light); margin: 0;">Latest Top 10 GitHub Commits</h3>
        </div>
        <asp:LinkButton ID="LinkButton_Refresh" runat="server" OnClick="LinkButton_Refresh_Click" CssClass="btn btn-secondary" Style="padding: 6px 12px; font-size: 12px;">
            <i class="fas fa-rotate"></i> Refresh Feed
        </asp:LinkButton>
    </div>

    <!-- Error Alert -->
    <asp:Panel ID="Panel_Error" runat="server" Visible="false" Style="background-color: rgba(239, 68, 68, 0.1); border: 1px solid #ef4444; color: #ef4444; padding: 10px 14px; border-radius: 8px; font-size: 13px; margin-bottom: 12px;">
        <i class="fas fa-triangle-exclamation"></i>Unable to load GitHub commit feed.
    </asp:Panel>

    <!-- Commits Repeater -->
    <asp:Repeater ID="Repeater_Commits" runat="server">
        <HeaderTemplate>
            <div style="display: flex; flex-direction: column; gap: 10px;">
        </HeaderTemplate>
        <ItemTemplate>
            <div style="background-color: var(--bg-light-tertiary); border: 1px solid var(--border-light); border-radius: 8px; padding: 12px; display: flex; flex-direction: column; gap: 8px;">
                <div style="display: flex; justify-content: space-between; align-items: flex-start; gap: 12px;">
                    <div style="display: flex; gap: 10px; align-items: center;">
                        <img src='<%# Eval("AvatarUrl") %>' alt="Avatar" style="width: 24px; height: 24px; border-radius: 50%;" onerror="this.style.display='none';" />
                        <div>
                            <div style="font-weight: 600; font-size: 14px; color: var(--text-light);">
                                <%# Eval("Title") %>
                            </div>
                            <div style="font-size: 11px; color: var(--text-light-secondary); margin-top: 2px;">
                                By <span style="font-weight: 600;"><%# Eval("Author") %></span> • <%# Eval("UpdatedDate") %>
                            </div>
                        </div>
                    </div>
                    <a href='<%# Eval("Link") %>' target="_blank" class="btn btn-secondary" style="padding: 4px 8px; font-size: 11px; white-space: nowrap;">
                        <i class="fas fa-external-link-alt"></i>View Commit
                    </a>
                </div>

                <!-- Commit Body / Detailed Info -->
                <%# !string.IsNullOrEmpty(Eval("Info").ToString()) ? @"
                <div style='background: rgba(0,0,0,0.15); padding: 8px 12px; border-radius: 6px; font-family: monospace; font-size: 12px; white-space: pre-wrap; color: var(--text-light-secondary); margin-top: 4px;'>
                    " + HttpUtility.HtmlEncode(Eval("Info").ToString()) + @"
                </div>" : "" %>
            </div>
        </ItemTemplate>
        <FooterTemplate>
            </div>
        </FooterTemplate>
    </asp:Repeater>
</div>
