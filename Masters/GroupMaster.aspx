<%@ Page Title="Group Master" Language="C#" MasterPageFile="~/Masters/MastersSite.master" AutoEventWireup="true" CodeFile="GroupMaster.aspx.cs" Inherits="Masters_GroupMaster" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="PageTitle" runat="server">Group Master</asp:Content>
<asp:Content ID="NavTitleContent" ContentPlaceHolderID="NavTitle" runat="server">Group Master</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

    <!-- FORM CARD -->
    <div class="form-card">

        <!-- Search Row -->
        <div class="form-row">
            <span class="form-label">Search:</span>
            <div class="input-with-icon">
                <asp:TextBox ID="txtSearch" runat="server" CssClass="form-input form-input-md"
                    placeholder="Search by code or description..." />
                <button type="button" class="search-icon-btn" onclick="doSearch()">&#128269;</button>
            </div>
        </div>

        <hr class="divider" />

        <!-- Category Code -->
        <div class="form-row">
            <span class="form-label">Category Code :</span>
            <asp:TextBox ID="txtCategoryCode" runat="server" CssClass="form-input form-input-sm"
                MaxLength="20"  Enabled="false"  />
        </div>

        <!-- Category Description -->
        <div class="form-row">
            <span class="form-label">Category Description :</span>
            <asp:TextBox ID="txtCategoryDesc" runat="server" CssClass="form-input form-input-xl"
                MaxLength="200" placeholder="Full description of the category" />
        </div>

        <!-- Category Prefix -->
        <div class="form-row">
            <span class="form-label">Category Prefix :</span>
            <asp:TextBox ID="txtCategoryPrefix" runat="server" CssClass="form-input form-input-sm"
                MaxLength="10" placeholder="e.g. CH" />
        </div>

        <!-- Remarks -->
        <div class="form-row" style="align-items:flex-start;">
            <span class="form-label" style="padding-top:5px;">Remarks</span>
            <asp:TextBox ID="txtRemarks" runat="server" CssClass="form-textarea"
                TextMode="MultiLine" Rows="3" MaxLength="500"
                placeholder="Additional remarks or notes..." />
        </div>

        <!-- Account Details Section -->
        <div class="section-header" style="margin-top:12px;">Account Details</div>

        <!-- Inventory Acc Code -->
        <div class="account-detail-row">
            <span class="form-label-wide">Inventory Acc code {5}:</span>
            <asp:TextBox ID="txtInvAccCode" runat="server" CssClass="acc-code-input"
                MaxLength="20" placeholder="Acc Code" />
            <button type="button" class="search-icon-btn" title="Search Account">&#128269;</button>
            <asp:TextBox ID="txtInvAccDesc" runat="server" CssClass="acc-desc-input"
                MaxLength="200" placeholder="Account Description" ReadOnly="true" />
        </div>

        <!-- Cost of Sales -->
        <div class="account-detail-row">
            <span class="form-label-wide">Cost of sales A/c {25}:</span>
            <asp:TextBox ID="txtCostAccCode" runat="server" CssClass="acc-code-input"
                MaxLength="20" placeholder="Acc Code" />
            <button type="button" class="search-icon-btn" title="Search Account">&#128269;</button>
            <asp:TextBox ID="txtCostAccDesc" runat="server" CssClass="acc-desc-input"
                MaxLength="200" placeholder="Account Description" ReadOnly="true" />
        </div>

        <!-- Income Acc Code -->
        <div class="account-detail-row">
            <span class="form-label-wide">Income Acc Code {23,24}:</span>
            <asp:TextBox ID="txtIncomeAccCode" runat="server" CssClass="acc-code-input"
                MaxLength="20" placeholder="Acc Code" />
            <button type="button" class="search-icon-btn" title="Search Account">&#128269;</button>
            <asp:TextBox ID="txtIncomeAccDesc" runat="server" CssClass="acc-desc-input"
                MaxLength="200" placeholder="Account Description" ReadOnly="true" />
        </div>

        <hr class="divider" />

        <!-- Action Buttons -->
        <div class="btn-row">
            <asp:Button ID="btnSave" runat="server" Text="&#128190; Save" CssClass="btn btn-primary"
                OnClick="btnSave_Click" />
            <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-secondary"
                OnClick="btnClear_Click" CausesValidation="false" />
            <asp:Button ID="btnUpdate" runat="server" Text="&#128260; Update" CssClass="btn btn-warning"
                OnClick="btnUpdate_Click" />
            <asp:Button ID="btnDelete" runat="server" Text="&#128465; Delete" CssClass="btn btn-danger"
                OnClick="btnDelete_Click" CausesValidation="false"
                OnClientClick="return confirm('Are you sure you want to delete this group?');" />
        </div>

        <asp:HiddenField ID="hfEditCode" runat="server" Value="" />

        <!-- Validation -->
        
        <asp:RequiredFieldValidator ID="rfvCatDesc" runat="server"
            ControlToValidate="txtCategoryDesc" ErrorMessage="Category Description is required."
            ForeColor="Red" Display="Dynamic" FontSize="11px" />
    </div>

    <!-- DATA GRID -->
    <div class="grid-container">
        <asp:GridView ID="gvGroup" runat="server"
            CssClass="data-grid"
            AutoGenerateColumns="false"
            OnRowCommand="gvGroup_RowCommand"
            EmptyDataText="No group records found."
            GridLines="None">
            <Columns>
                <asp:BoundField DataField="CategoryCode"   HeaderText="Category Code"   ItemStyle-Width="120px" />
                <asp:BoundField DataField="CategoryDesc"   HeaderText="Description" />
                <asp:BoundField DataField="CategoryPrefix" HeaderText="Prefix"      ItemStyle-Width="80px"  ItemStyle-HorizontalAlign="Center" />
                <asp:BoundField DataField="InvAccCode"     HeaderText="Inv A/c"     ItemStyle-Width="100px" />
                <asp:BoundField DataField="CostAccCode"    HeaderText="Cost A/c"    ItemStyle-Width="100px" />
                <asp:BoundField DataField="IncomeAccCode"  HeaderText="Income A/c"  ItemStyle-Width="100px" />
                <asp:TemplateField HeaderText="" ItemStyle-Width="90px" ItemStyle-HorizontalAlign="Center">
                    <ItemTemplate>
                        <asp:LinkButton ID="lnkEdit" runat="server"
                            CommandName="EditRow"
                            CommandArgument='<%# Eval("CategoryCode") %>'
                            CssClass="link-btn" CausesValidation="false">Edit</asp:LinkButton>
                        &nbsp;
                        <asp:LinkButton ID="lnkDelete" runat="server"
                            CommandName="DeleteRow"
                            CommandArgument='<%# Eval("CategoryCode") %>'
                            CssClass="link-btn delete" CausesValidation="false"
                            OnClientClick="return confirm('Delete this group?');">Delete</asp:LinkButton>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
    </div>

</asp:Content>

<asp:Content ID="ScriptContent" ContentPlaceHolderID="ScriptContent" runat="server">
    <script type="text/javascript">
        function doSearch() {
            var val = document.getElementById('<%= txtSearch.ClientID %>').value.toLowerCase();
            var rows = document.querySelectorAll('.data-grid tbody tr');
            rows.forEach(function (row) {
                row.style.display = (!val || row.textContent.toLowerCase().indexOf(val) >= 0) ? '' : 'none';
            });
        }

        var msg = '<%= savedMessage %>';
        if (msg) {
            window.addEventListener('load', function () { showNotification(msg, 'success'); });
        }
    </script>
</asp:Content>
