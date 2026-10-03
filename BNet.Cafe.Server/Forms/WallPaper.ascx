<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Wallpaper.ascx.cs" Inherits="BNet.Cafe.Server.Forms.Wallpaper" %>
<%@ Register Src="~/Forms/Modals/WallpaperDelete.ascx" TagPrefix="uc1" TagName="WallpaperDelete" %>

<asp:UpdatePanel ID="UpdatePanel1" runat="server">
    <ContentTemplate>
        <div class="bnet-table-wrapper">
            <div class="content-header" style="margin-bottom: 20px;">
                <h1 style="font-size: 20px; display: flex; align-items: center; gap: 8px;">
                    <i class="fa-solid fa-image" style="color: var(--primary);"></i>Wallpaper Gallery Setup
                </h1>
                <p>Upload and manage high-resolution wallpapers for client desktops.</p>
            </div>

            <!-- Drag & Drop Upload Zone -->
            <div class="drop-zone" id="dropZone">
                <i class="fa-solid fa-cloud-arrow-up drop-zone-icon"></i>
                <div class="drop-zone-text">Drag & Drop wallpapers here or <span class="browse-btn">browse files</span></div>
                <div class="drop-zone-hint">Supports JPG, PNG, WEBP up to 10MB each (Recommended resolution: 1920x1080 or higher)</div>
                <input type="file" id="fileInput" multiple accept="image/jpeg,image/png,image/webp" class="d-none" />
            </div>

            <!-- Hidden Field for JSON Array -->
            <asp:HiddenField ID="HiddenField_WallpaperData" runat="server" />

            <!-- Gallery Header & Controls -->
            <div id="previewHeader" class="preview-header d-none">
                <span id="imageCount">0 Wallpaper(s) Selected</span>
                <button type="button" class="btn btn-secondary btn-sm" onclick="clearAllWallpapers()">Clear All</button>
            </div>

            <!-- Wallpapers Grid Preview -->
            <div id="wallpaperGrid" class="wallpaper-grid"></div>

            <!-- Form Actions -->
            <div class="form-grid-action">
                <asp:LinkButton ID="LinkButton_Submit" OnClick="LinkButton_Submit_Click" CssClass="btn btn-primary" OnClientClick="return ValidateWallpaperUpload();" runat="server">
                    <i class="fa fa-save"></i> Save Wallpapers
                </asp:LinkButton>
            </div>
        </div>

        <!-- Wallpaper Delete Modal Control -->
        <uc1:WallpaperDelete runat="server" ID="WallpaperDeleteModal" />
    </ContentTemplate>
</asp:UpdatePanel>