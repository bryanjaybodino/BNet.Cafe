<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RemoteTimePause.ascx.cs" Inherits="BNet.Cafe.Server.Forms.Modals.RemoteTimePause" %>

<asp:HiddenField ID="HiddenField_SelectedComputerName" runat="server" />
<asp:HiddenField ID="HiddenField_ActionType" runat="server" />

<!-- BNet Pause/Resume Modal Template -->
<div id="pauseModal" class="bnet-modal-overlay">
    <div class="bnet-modal-container">
        <div class="bnet-modal-header">
            <h3 class="bnet-modal-title"><i class="fa-solid fa-circle-pause"></i>Pause / Resume Session</h3>
            <span class="bnet-modal-close" onclick="closePauseModal()">&times;</span>
        </div>
        <div class="bnet-modal-body">
            <div style="margin-bottom: 16px;">
                <label style="display: block; font-size: 13px; font-weight: 600; margin-bottom: 8px;">Select Target Computer</label>
                <asp:DropDownList ID="DropDownList_OccupiedPcs" runat="server" CssClass="form-control" Style="width: 100%; padding: 8px; border-radius: 6px; border: 1px solid #ccc;" onchange="updateToggleState()">
                </asp:DropDownList>
            </div>
        </div>


        <div class="bnet-modal-footer">
            <span class="btn btn-secondary" onclick="closePauseModal()">Close</span>
            <asp:LinkButton ID="LinkButton_ToggleState" OnClientClick="setActionType()" OnClick="LinkButton_ExecuteAction_Click" CssClass="btn btn-danger" runat="server">
                    <i id="toggleIcon" class="fa-solid fa-pause"></i> <span id="toggleText">Pause</span>
            </asp:LinkButton>
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
        if (ddl) {
            var found = false;
            if (selectedPcName) {
                for (var i = 0; i < ddl.options.length; i++) {
                    if (ddl.options[i].value === selectedPcName) {
                        ddl.selectedIndex = i;
                        found = true;
                        break;
                    }
                }
            }
            if (!found && ddl.options.length > 0) {
                ddl.selectedIndex = 0;
            }
        }
        updateToggleState();
        getPauseModal().open();
    }

    function closePauseModal() {
        getPauseModal().close();
    }

    function updateToggleState() {
        var ddl = document.getElementById('<%= DropDownList_OccupiedPcs.ClientID %>');
        var toggleBtn = document.getElementById('<%= LinkButton_ToggleState.ClientID %>');
        var toggleText = document.getElementById('toggleText');
        var toggleIcon = document.getElementById('toggleIcon');

        if (!ddl || ddl.options.length === 0 || !ddl.value) {
            if (toggleBtn) toggleBtn.style.display = 'none';
            return;
        }

        if (toggleBtn) toggleBtn.style.display = 'inline-block';

        var selectedOption = ddl.options[ddl.selectedIndex];
        var isPaused = selectedOption.getAttribute('data-paused') === 'true';

        // If currently paused -> Display option to RESUME (Blue Button)
        if (isPaused) {
            toggleBtn.className = "btn btn-primary";
            if (toggleText) toggleText.innerText = "Resume";
            if (toggleIcon) toggleIcon.className = "fa-solid fa-play";
        }
        // If currently running -> Display option to PAUSE (Red Button)
        else {
            toggleBtn.className = "btn btn-danger";
            if (toggleText) toggleText.innerText = "Pause";
            if (toggleIcon) toggleIcon.className = "fa-solid fa-pause";
        }
    }

    function setActionType() {
        var ddl = document.getElementById('<%= DropDownList_OccupiedPcs.ClientID %>');
        if (ddl && ddl.options.length > 0) {
            var selectedOption = ddl.options[ddl.selectedIndex];
            var isPaused = selectedOption.getAttribute('data-paused') === 'true';

            // Set hidden field action based on current state
            document.getElementById('<%= HiddenField_ActionType.ClientID %>').value = isPaused ? 'RESUME' : 'PAUSE';
            document.getElementById('<%= HiddenField_SelectedComputerName.ClientID %>').value = ddl.value;
            disableToggleStateButton();
        }
    }

    if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            pauseModalInstance = new BNetModal('#pauseModal');
            updateToggleState();
        });
    }

    function disableToggleStateButton() {
        var btn = document.querySelector('[id$="LinkButton_ToggleState"]');
        if (btn) {
            btn.classList.add('disabled');
            btn.style.pointerEvents = 'none';
            btn.innerHTML = '<i class="fa fa-spinner fa-spin"></i> Saving...';
        }
    }
</script>
