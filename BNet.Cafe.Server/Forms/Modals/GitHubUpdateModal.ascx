<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="GitHubUpdateModal.ascx.cs" Inherits="BNet.Cafe.Server.Forms.Modals.GitHubUpdateModal" %>

<div id="githubUpdateModal" class="bnet-modal-overlay">
    <div class="bnet-modal-container">
        <div class="bnet-modal-header">
            <h3 class="bnet-modal-title">
                <i class="fas fa-arrows-rotate" style="color: var(--primary);"></i> New System Update Available
            </h3>
            <span class="bnet-modal-close" onclick="closeGitHubUpdateModal()">&times;</span>
        </div>
        <div class="bnet-modal-body">
            <div style="display: flex; gap: 12px; align-items: flex-start; margin-bottom: 12px;">
                <div style="background-color: var(--bg-light-tertiary); padding: 10px; border-radius: 8px;">
                    <i class="fab fa-github" style="font-size: 24px; color: var(--text-light);"></i>
                </div>
                <div>
                    <div style="font-weight: 600; font-size: 14px;" id="ghCommitMessage">Loading commit details...</div>
                    <div style="font-size: 12px; color: var(--text-light-secondary); margin-top: 4px;">
                        Committed by <span id="ghCommitAuthor" style="font-weight: 600;"></span> on <span id="ghCommitDate"></span>
                    </div>
                </div>
            </div>
            
            <div style="background-color: var(--bg-light-tertiary); padding: 8px 12px; border-radius: 6px; font-family: monospace; font-size: 12px; word-break: break-all; color: var(--text-light-secondary);">
                Commit SHA: <span id="ghCommitSha"></span>
            </div>
        </div>
        <div class="bnet-modal-footer">
            <span class="btn btn-secondary" onclick="closeGitHubUpdateModal()">Dismiss</span>
            <a id="ghCommitLink" href="#" target="_blank" class="btn btn-primary">
                <i class="fas fa-external-link-alt"></i> View on GitHub
            </a>
        </div>
    </div>
</div>

<script type="text/javascript">
    let githubUpdateModalInstance = null;

    function getGitHubUpdateModal() {
        if (!githubUpdateModalInstance) {
            githubUpdateModalInstance = new BNetModal('#githubUpdateModal');
        }
        return githubUpdateModalInstance;
    }

    function showGitHubUpdateModal(updateData) {
        if (!updateData) return;

        document.getElementById('ghCommitMessage').innerText = updateData.message || 'New commit released';
        document.getElementById('ghCommitAuthor').innerText = updateData.author || 'Contributor';
        document.getElementById('ghCommitDate').innerText = updateData.date || '';
        document.getElementById('ghCommitSha').innerText = updateData.sha || '';
        document.getElementById('ghCommitLink').href = updateData.url || '#';

        getGitHubUpdateModal().open();
    }

    function closeGitHubUpdateModal() {
        getGitHubUpdateModal().close();
    }

    if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            githubUpdateModalInstance = new BNetModal('#githubUpdateModal');
        });
    }
</script>