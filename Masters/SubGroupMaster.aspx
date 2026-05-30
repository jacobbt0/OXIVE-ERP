<%@ Page Title="Group Sub Category" Language="C#" MasterPageFile="~/Masters/MastersSite.master" AutoEventWireup="true" CodeFile="SubGroupMaster.aspx.cs" Inherits="Masters_SubGroupMaster" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="PageTitle" runat="server">Group Sub Category</asp:Content>
<asp:Content ID="NavTitleContent" ContentPlaceHolderID="NavTitle" runat="server">Group Sub Category</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

    <!-- FORM CARD -->
    <div class="form-card">

        <!-- Search Row -->
        <div class="form-row">
            <span class="form-label">Search :</span>
            <div class="input-with-icon">
                <asp:TextBox ID="txtSearch" runat="server" CssClass="form-input form-input-md"
                    placeholder="Search sub groups..." />
                <button type="button" class="search-icon-btn" onclick="doSearch()">&#128269;</button>
            </div>
        </div>

        <hr class="divider" />

        <!-- Group Code (Dropdown) -->
        <div class="form-row">
            <span class="form-label">Group Code :</span>
            <asp:DropDownList ID="ddlGroupCode" runat="server" CssClass="form-select"
                Style="min-width:280px;" AutoPostBack="false">
                <asp:ListItem Value="">-- Select Group --</asp:ListItem>
            </asp:DropDownList>
        </div>

        <!-- Sub Group Code -->
        <div class="form-row">
            <span class="form-label">Sub Group Code :</span>
            <asp:TextBox ID="txtSubGroupCode" runat="server" CssClass="form-input form-input-sm"
                MaxLength="20" placeholder="e.g. CHM001" />
        </div>

        <!-- Sub Group Desc -->
        <div class="form-row">
            <span class="form-label">Sub Group Desc :</span>
            <asp:TextBox ID="txtSubGroupDesc" runat="server" CssClass="form-input form-input-xl"
                MaxLength="200" placeholder="Sub group description" />
        </div>

        <!-- Precast Code Pattern -->
        <div class="form-row">
            <span class="form-label">Precast Code Pattern :</span>
            <div class="checkbox-group">
                <label class="radio-item">
                    <asp:RadioButton ID="rbPrecastYes" runat="server" GroupName="PrecastPattern" Text="Yes" />
                </label>
                <label class="radio-item">
                    <asp:RadioButton ID="rbPrecastNo" runat="server" GroupName="PrecastPattern" Text="No" Checked="true" />
                </label>
            </div>
        </div>

        <!-- Item Coding Group -->
        <div class="form-row">
            <span class="form-label">Item Coding Group</span>
            <asp:TextBox ID="txtItemCodingGroup" runat="server" CssClass="form-input form-input-sm"
                MaxLength="10" placeholder="Code" />
            <span style="font-size:12px; color:#555; margin-left:6px;">
                {Based on this value grouping the Items and Code generating Automatically}
            </span>
        </div>

        <!-- Remarks -->
        <div class="form-row" style="align-items:flex-start;">
            <span class="form-label" style="padding-top:5px;">Remarks:</span>
            <asp:TextBox ID="txtRemarks" runat="server" CssClass="form-textarea"
                TextMode="MultiLine" Rows="3" MaxLength="500"
                placeholder="Remarks or additional notes..." />
        </div>

        <!-- Budget Break Down -->
        <div class="form-row">
            <span class="form-label">Budge Break Down</span>
            <div class="budget-row">
                <asp:TextBox ID="txtBudgetCode" runat="server" CssClass="form-input form-input-sm"
                    MaxLength="20" placeholder="Code" />
                <button type="button" class="search-icon-btn" title="Search Budget">&#128269;</button>
                <asp:TextBox ID="txtBudgetDesc" runat="server" CssClass="form-input form-input-lg"
                    MaxLength="200"  
                    BackColor="#f8f9fa" />
            </div>
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
                MaxLength="200" placeholder="Account Description" />
        </div>

        <!-- Cost of Sales -->
        <div class="account-detail-row">
            <span class="form-label-wide">Cost of sales A/c {25}:</span>
            <asp:TextBox ID="txtCostAccCode" runat="server" CssClass="acc-code-input"
                MaxLength="20" placeholder="Acc Code" />
            <button type="button" class="search-icon-btn" title="Search Account">&#128269;</button>
            <asp:TextBox ID="txtCostAccDesc" runat="server" CssClass="acc-desc-input"
                MaxLength="200" placeholder="Account Description" />
        </div>

        <!-- Income Acc Code -->
        <div class="account-detail-row">
            <span class="form-label-wide">Income Acc Code {23,24}:</span>
            <asp:TextBox ID="txtIncomeAccCode" runat="server" CssClass="acc-code-input"
                MaxLength="20" placeholder="Acc Code" />
            <button type="button" class="search-icon-btn" title="Search Account">&#128269;</button>
            <asp:TextBox ID="txtIncomeAccDesc" runat="server" CssClass="acc-desc-input"
                MaxLength="200" placeholder="Account Description" />
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
                OnClientClick="return confirm('Are you sure you want to delete this sub-group?');" />
        </div>

        <asp:HiddenField ID="hfEditCode" runat="server" Value="" />

        <!-- Validation -->
        <asp:RequiredFieldValidator ID="rfvGroupCode" runat="server"
            ControlToValidate="ddlGroupCode" InitialValue=""
            ErrorMessage="Please select a Group Code."
            ForeColor="Red" Display="Dynamic" FontSize="11px" />
        <asp:RequiredFieldValidator ID="rfvSubCode" runat="server"
            ControlToValidate="txtSubGroupCode" ErrorMessage="Sub Group Code is required."
            ForeColor="Red" Display="Dynamic" FontSize="11px" />
        <asp:RequiredFieldValidator ID="rfvSubDesc" runat="server"
            ControlToValidate="txtSubGroupDesc" ErrorMessage="Sub Group Description is required."
            ForeColor="Red" Display="Dynamic" FontSize="11px" />
    </div>

    <!-- DATA GRID -->
    <div class="grid-container">
        <asp:GridView ID="gvSubGroup" runat="server"
            CssClass="data-grid"
            AutoGenerateColumns="false"
            OnRowCommand="gvSubGroup_RowCommand"
            EmptyDataText="No sub-group records found."
            GridLines="None">
            <Columns>
                <asp:BoundField DataField="GroupCode"       HeaderText="Group Code"     ItemStyle-Width="120px" />
                <asp:BoundField DataField="SubGroupCode"    HeaderText="Sub Group Code" ItemStyle-Width="130px" />
                <asp:BoundField DataField="SubGroupDesc"    HeaderText="Description" />
                <asp:BoundField DataField="ItemCodingGroup" HeaderText="Item Coding"   ItemStyle-Width="100px" ItemStyle-HorizontalAlign="Center" />
                <asp:TemplateField HeaderText="Precast" ItemStyle-Width="70px" ItemStyle-HorizontalAlign="Center">
                    <ItemTemplate>
                        <asp:Label ID="lblPrecast" runat="server"
                            Text='<%# (bool)Eval("PrecastCodePattern") ? "Yes" : "No" %>' />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="InvAccCode"      HeaderText="Inv A/c"       ItemStyle-Width="90px" />
                <asp:TemplateField HeaderText="" ItemStyle-Width="90px" ItemStyle-HorizontalAlign="Center">
                    <ItemTemplate>
                        <asp:LinkButton ID="lnkEdit" runat="server"
                            CommandName="EditRow"
                            CommandArgument='<%# Eval("SubGroupCode") %>'
                            CssClass="link-btn" CausesValidation="false">Edit</asp:LinkButton>
                        &nbsp;
                        <asp:LinkButton ID="lnkDelete" runat="server"
                            CommandName="DeleteRow"
                            CommandArgument='<%# Eval("SubGroupCode") %>'
                            CssClass="link-btn delete" CausesValidation="false"
                            OnClientClick="return confirm('Delete this sub-group?');">Delete</asp:LinkButton>
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
