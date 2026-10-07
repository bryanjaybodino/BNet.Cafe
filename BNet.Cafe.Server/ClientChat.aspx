<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ClientChat.aspx.cs" Inherits="BNet.Cafe.Server.ClientChat" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Client Chat Screen</title>
    <% Response.Write(BNet.Cafe.Server.Services.FileCssHelper.StyleSheetVersion("Assets/Pages/Style.css")); %>
    <% Response.Write(BNet.Cafe.Server.Services.FileCssHelper.StyleSheetVersion("Assets/fontawesome/font-awesome.min.css")); %>
    <% Response.Write(BNet.Cafe.Server.Services.FileCssHelper.StyleSheetVersion("Assets/ClientChat/Style.css")); %>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" EnableCdn="false" EnablePageMethods="true" EnablePartialRendering="true" AsyncPostBackTimeout="99999999" ScriptMode="Release" ValidateRequestMode="Enabled" EnableScriptLocalization="true" EnableScriptGlobalization="true" LoadScriptsBeforeUI="false" CompositeScript-ScriptMode="Release" CompositeScript-ResourceUICultures="Release" runat="server"></asp:ScriptManager>
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
    

                <div class="full-chat-wrapper">
                    <div class="full-chat-header">
                        <div class="header-info">
                            <div class="header-avatar">
                                <i class="fa-solid fa-desktop"></i>
                            </div>
                            <div class="header-title-area">
                                <h2 id="clientTitle">Client Chat</h2>
                                <span class="header-subtitle">Cafe Station Support</span>
                            </div>
                        </div>
                        <div class="status-badge">
                            <div id="statusDot" class="status-dot"></div>
                            <span id="connectionStatus">Connecting...</span>
                        </div>
                    </div>

                    <div id="chatLogs" class="full-chat-logs"></div>

                    <div class="full-chat-footer">
                        <div class="input-wrapper">
                            <input type="text" id="messageInput" class="form-control" placeholder="Type a message..." autofocus autocomplete="off">
                        </div>
                        <button type="button" id="LinkButton_SaveMessage" runat="server" class="btn btn-primary btn-send">
                            <span>Send</span>
                            <i class="fa-solid fa-paper-plane"></i>
                        </button>

                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </form>
</body>
</html>
