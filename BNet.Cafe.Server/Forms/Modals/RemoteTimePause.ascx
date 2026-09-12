<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RemoteTimePause.ascx.cs" Inherits="BNet.Cafe.Server.Forms.Modals.RemoteTimePause" %>

<asp:HiddenField ID="HiddenField_SelectedComputerName" runat="server" />
<asp:HiddenField ID="HiddenField_ActionType" runat="server" />

<!-- BNet Pause/Resume Modal Template -->
<div id="pauseModal" class="bnet-modal-overlay">
    <div class="bnet-modal-container">
        <div class="bnet-modal-header">
            <h3 class="bnet-modal-title"><i class="fa-solid fa-circle-pause"></i> Pause / Resume Session</h3>
            <span class="bnet-modal-close" onclick="closePauseModal()">&times;</span>
        </div>
        <div class="bnet-modal-body">
            <!-- Mode 1: Selected PC Controls -->
            <div id="sectionSelectedPc" style="margin-bottom: 16px;">
                <label style="display: block; font-size: 13px; font-weight: 600; margin-bottom: 5px;">Selected PC Action</label>
                <div style="display: flex; gap: 8px;">
                    <asp:DropDownList ID="DropDownList_OccupiedPcs" runat="server" CssClass="form-control" Style="flex: 1; padding: 8px; border-radius: 6px; border: 1px solid #ccc;">
                    </asp:DropDownList>
                </div>
                <div style="display: flex; gap: 8px; margin-top: 10px;">
                    <asp:LinkButton ID="LinkButton_PauseSelected" OnClientClick="setActionType('PAUSE_SELECTED')" OnClick="LinkButton_ExecuteAction_Click" CssClass="btn btn-warning" Style="flex: 1; text-align: center;" runat="server">
                        <i class="fa-solid fa-pause"></i> Pause Selected
                    </asp:LinkButton>
                    <asp:LinkButton ID="LinkButton_ResumeSelected" OnClientClick="setActionType('RESUME_SELECTED')" OnClick="LinkButton_ExecuteAction_Click" CssClass="btn btn-success" Style="flex: 1; text-align: center;" runat="server">
                        <i class="fa-solid fa-play"></i> Resume Selected
                    </asp:LinkButton>
                </div>
            </div>

            <hr style="border: 0; border-top: 1px solid var(--border-color, #e5e7eb); margin: 15px 0;" />

            <!-- Mode 2: All PCs Controls -->
            <div>
                <label style="display: block; font-size: 13px; font-weight: 600; margin-bottom: 5px;">Global Action (All Active PCs)</label>
                <div style="display: flex; gap: 8px;">
                    <asp:LinkButton ID="LinkButton_PauseAll" OnClientClick="setActionType('PAUSE_ALL')" OnClick="LinkButton_ExecuteAction_Click" CssClass="btn btn-danger" Style="flex: 1; text-align: center;" runat="server">
                        <i class="fa-solid fa-pause"></i> Pause All PCs
                    </asp:LinkButton>
                    <asp:LinkButton ID="LinkButton_ResumeAll" OnClientClick="setActionType('RESUME_ALL')" OnClick="LinkButton_ExecuteAction_Click" CssClass="btn btn-primary" Style="flex: 1; text-align: center;" runat="server">
                        <i class="fa-solid fa-play"></i> Resume All PCs
                    </asp:LinkButton>
                </div>
            </div>
        </div>
        <div class="bnet-modal-footer">
            <span class="btn btn-secondary" onclick="closePauseModal()">Close</span>
        </div>
    </div>
</div>

<script type="text/javascript">
    let pauseModalInstance = null;

    function getPauseModal() {
        if (!pauseModalInstance) {
            pauseModalInstance = new BNetModal('#pauseModal');
        }
        return pauseModalInstance;
    }

    function openPauseModal(selectedPcName) {
        var ddl = document.getElementById('<%= DropDownList_OccupiedPcs.ClientID %>');
        if (selectedPcName && ddl) {
            for (var i = 0; i < ddl.options.length; i++) {
                if (ddl.options[i].value === selectedPcName) {
                    ddl.selectedIndex = i;
                    break;
                }
            }
        }
        getPauseModal().open();
    }

    function closePauseModal() {
        getPauseModal().close();
    }

    function setActionType(action) {
        document.getElementById('<%= HiddenField_ActionType.ClientID %>').value = action;
        var ddl = document.getElementById('<%= DropDownList_OccupiedPcs.ClientID %>');
        if (ddl) {
            document.getElementById('<%= HiddenField_SelectedComputerName.ClientID %>').value = ddl.value;
        }
    }

    if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            pauseModalInstance = new BNetModal('#pauseModal');
        });
    }
</script>