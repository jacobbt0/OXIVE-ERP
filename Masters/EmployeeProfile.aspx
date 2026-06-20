<%@ Page Title="Employee Profile" Language="C#" 
    MasterPageFile="~/Masters/MastersSite.master" 
    AutoEventWireup="true" 
    CodeFile="EmployeeProfile.aspx.cs" 
    Inherits="Masters_EmployeeProfile" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="PageTitle" runat="server">
    Employee Profile
</asp:Content>

<asp:Content ID="NavTitleContent" ContentPlaceHolderID="NavTitle" runat="server">
    Employee Profile
</asp:Content>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .profile-tabs { display: flex; border-bottom: 1px solid #ddd; margin-bottom: 15px; background: #f8f9fa; }
        .profile-tab { padding: 8px 16px; cursor: pointer; background: #e9ecef; margin-right: 5px; border-radius: 4px 4px 0 0; }
        .profile-tab.active { background: #007bff; color: white; }
        .tab-pane { display: none; }
        .tab-pane.active { display: block; }
        .profile-grid { margin-top: 10px; overflow-x: auto; }
        .profile-grid table { width: 100%; border-collapse: collapse; }
        .profile-grid th, .profile-grid td { border: 1px solid #ddd; padding: 6px; text-align: left; font-size: 13px; }
        .profile-grid th { background: #f2f2f2; }
        .profile-form-row { display: flex; align-items: center; margin-bottom: 8px; }
        .profile-form-row label { width: 150px; font-weight: 600; font-size: 13px; }
        .profile-form-row .field { flex: 1; }
        .profile-form-row .field input, .profile-form-row .field select { width: 100%; padding: 6px; border: 1px solid #ccc; border-radius: 4px; }
        .profile-form-row .field input[type="checkbox"] { width: auto; }
        .action-buttons { margin-top: 15px; text-align: right; }
        .btn-sm { padding: 4px 10px; font-size: 12px; }
        .search-box { margin-bottom: 10px; display: flex; gap: 10px; align-items: center; flex-wrap: wrap; }
        .search-box input { padding: 6px; border: 1px solid #ccc; border-radius: 4px; width: 300px; }
        .list-panel, .detail-panel { padding: 10px; }
        .panel-hidden { display: none; }
        .link-btn { color: #007bff; cursor: pointer; text-decoration: underline; }
        .link-btn.delete { color: #dc3545; }
        .link-btn:hover { opacity: 0.7; }
        .salary-grid { margin-top: 10px; overflow-x: auto; }
        .salary-grid table { width: 100%; border-collapse: collapse; }
        .salary-grid th, .salary-grid td { border: 1px solid #ddd; padding: 6px; text-align: left; font-size: 13px; }
        .salary-grid th { background: #f2f2f2; }
    </style>
    <script type="text/javascript">
        function switchTab(tabId) {
            document.querySelectorAll('.profile-tab').forEach(t => t.classList.remove('active'));
            document.querySelectorAll('.tab-pane').forEach(p => p.classList.remove('active'));
            document.getElementById('tab_' + tabId).classList.add('active');
            document.getElementById('pane_' + tabId).classList.add('active');
            document.getElementById('<%= hfCurrentTab.ClientID %>').value = tabId;
        }
        function showMessage(msg, isError) {
            var lbl = document.getElementById('<%= lblMessage.ClientID %>');
            if (lbl) {
                lbl.style.color = isError ? 'red' : 'green';
                lbl.innerText = msg;
                setTimeout(function () { lbl.innerText = ''; }, 5000);
            } else {
                alert(msg);
            }
        }
        function confirmDelete() { return confirm('Are you sure you want to delete this item?'); }
        function togglePanels(showList) {
            var listPanel = document.getElementById('divListPanel');
            var detailPanel = document.getElementById('divDetailPanel');
            if (showList) {
                listPanel.style.display = 'block';
                detailPanel.style.display = 'none';
            } else {
                listPanel.style.display = 'none';
                detailPanel.style.display = 'block';
            }
        }
        window.addEventListener('load', function () {
            var savedTab = document.getElementById('<%= hfCurrentTab.ClientID %>').value;
            if (savedTab) switchTab(savedTab);
            var hfView = document.getElementById('<%= hfView.ClientID %>');
            if (hfView && hfView.value === 'detail') {
                togglePanels(false);
            } else {
                togglePanels(true);
            }
        });
    </script>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="form-card">

        <asp:Label ID="lblMessage" runat="server" ForeColor="Red" Font-Size="12px" />
        <asp:HiddenField ID="hfCurrentTab" runat="server" Value="info" />
        <asp:HiddenField ID="hfARNo" runat="server" Value="" />
        <asp:HiddenField ID="hfView" runat="server" Value="list" />

        <!-- LIST PANEL -->
        <div id="divListPanel" class="list-panel">
            <div class="search-box">
                <span>Search:</span>
                <asp:TextBox ID="txtSearch" runat="server" placeholder="Type name or AR No..." />
                <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary btn-sm" OnClick="btnSearch_Click" />
                <asp:Button ID="btnAddNew" runat="server" Text="+ Add New" CssClass="btn btn-success btn-sm" OnClick="btnAddNew_Click" />
                <asp:Button ID="btnRefresh" runat="server" Text="↻ Refresh" CssClass="btn btn-secondary btn-sm" OnClick="btnRefresh_Click" />
            </div>
            <div class="profile-grid">
                <asp:GridView ID="gvEmployees" runat="server" AutoGenerateColumns="false"
                    CssClass="data-grid" DataKeyNames="ARNo"
                    OnRowCommand="gvEmployees_RowCommand" OnPageIndexChanging="gvEmployees_PageIndexChanging"
                    AllowPaging="true" PageSize="10" EmptyDataText="No employees found.">
                    <Columns>
                        <asp:BoundField DataField="ARNo" HeaderText="AR No" ItemStyle-Width="80px" />
                        <asp:BoundField DataField="EmployeeName" HeaderText="Name" />
                        <asp:BoundField DataField="Nationality" HeaderText="Nationality" />
                        <asp:BoundField DataField="Designation" HeaderText="Designation" />
                        <asp:BoundField DataField="Status" HeaderText="Status" />
                        <asp:TemplateField HeaderText="Actions" ItemStyle-Width="120px" ItemStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <asp:LinkButton ID="lnkEdit" runat="server" CommandName="EditRow" 
                                    CommandArgument='<%# Eval("ARNo") %>' CssClass="link-btn">Edit</asp:LinkButton>
                                &nbsp;
                                <asp:LinkButton ID="lnkDelete" runat="server" CommandName="DeleteRow" 
                                    CommandArgument='<%# Eval("ARNo") %>' CssClass="link-btn delete"
                                    OnClientClick="return confirmDelete();">Delete</asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>

        <!-- DETAIL PANEL -->
        <div id="divDetailPanel" class="detail-panel" style="display:none;">

            <!-- Tabs -->
            <div class="profile-tabs">
                <div class="profile-tab active" id="tab_info" onclick="switchTab('info')">Employee Info</div>
                <div class="profile-tab" id="tab_visa" onclick="switchTab('visa')">Visa & Passport</div>
                <div class="profile-tab" id="tab_salary" onclick="switchTab('salary')">Salary</div>
            </div>

            <!-- Tab: Employee Info -->
            <div id="pane_info" class="tab-pane active">
                <div class="profile-form-row">
                    <label>AR No:</label>
                    <div class="field"><asp:TextBox ID="txtARNo" runat="server" /></div>
                </div>
                <div class="profile-form-row">
                    <label>Employee Name:</label>
                    <div class="field"><asp:TextBox ID="txtEmployeeName" runat="server" /></div>
                </div>
                <div class="profile-form-row">
                    <label>Nationality:</label>
                    <div class="field">
                        <asp:DropDownList ID="ddlNationality" runat="server" DataValueField="NationalityID" DataTextField="NationalityName" />
                    </div>
                </div>
                <div class="profile-form-row">
                    <label>Designation:</label>
                    <div class="field">
                        <asp:DropDownList ID="ddlDesignation" runat="server" DataValueField="DesignationID" DataTextField="DesignationName" />
                    </div>
                </div>
                <div class="profile-form-row">
                    <label>Join Date:</label>
                    <div class="field"><asp:TextBox ID="txtJoinDate" runat="server" TextMode="Date" /></div>
                </div>
                <div class="profile-form-row">
                    <label>Status:</label>
                    <div class="field">
                        <asp:DropDownList ID="ddlStatus" runat="server" DataValueField="StatusID" DataTextField="StatusName" />
                    </div>
                </div>
            </div>

            <!-- Tab: Visa & Passport -->
            <div id="pane_visa" class="tab-pane">
                <div class="profile-form-row">
                    <label>Visa Status:</label>
                    <div class="field">
                        <asp:DropDownList ID="ddlVisaStatus" runat="server" DataValueField="VisaStatusID" DataTextField="VisaStatusName" />
                    </div>
                </div>
                <div class="profile-form-row">
                    <label>Passport No:</label>
                    <div class="field"><asp:TextBox ID="txtPassportNo" runat="server" /></div>
                </div>
                <div class="profile-form-row">
                    <label>Passport Expiry:</label>
                    <div class="field"><asp:TextBox ID="txtPassportExpiry" runat="server" TextMode="Date" /></div>
                </div>
                <div class="profile-form-row">
                    <label>Passport with Company:</label>
                    <div class="field"><asp:CheckBox ID="chkPassportWithCompany" runat="server" /></div>
                </div>
                <div class="profile-form-row">
                    <label>Visit Visa Expiry:</label>
                    <div class="field"><asp:TextBox ID="txtVisitVisaExpiry" runat="server" TextMode="Date" /></div>
                </div>
                <div class="profile-form-row">
                    <label>ID Number:</label>
                    <div class="field"><asp:TextBox ID="txtIDNumber" runat="server" /></div>
                </div>
                <div class="profile-form-row">
                    <label>ID Expiry Date:</label>
                    <div class="field"><asp:TextBox ID="txtIDExpiryDate" runat="server" TextMode="Date" /></div>
                </div>
                <div class="profile-form-row">
                    <label>Visa Under Company:</label>
                    <div class="field">
                        <asp:DropDownList ID="ddlCompany" runat="server" DataValueField="CompanyID" DataTextField="CompanyName" />
                    </div>
                </div>
            </div>

            <!-- Tab: Salary (Normalized) -->
            <div id="pane_salary" class="tab-pane">
                <div class="salary-grid">
                    <asp:GridView ID="gvSalary" runat="server" AutoGenerateColumns="false" 
                        OnRowCommand="gvSalary_RowCommand" DataKeyNames="ID">
                        <Columns>
                            <asp:BoundField DataField="ComponentName" HeaderText="Component" ReadOnly="true" />
                            <asp:TemplateField HeaderText="Amount">
                                <ItemTemplate>
                                    <asp:TextBox ID="txtAmount" runat="server" Text='<%# Eval("Amount") %>' 
                                        Width="120px" CssClass="form-input" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Actions" ItemStyle-Width="140px">
                                <ItemTemplate>
                                    <asp:LinkButton ID="lnkSave" runat="server" CommandName="SaveRow" 
                                        CommandArgument='<%# Eval("ID") %>' CssClass="link-btn">Save</asp:LinkButton>
                                    &nbsp;
                                    <asp:LinkButton ID="lnkDelete" runat="server" CommandName="DeleteRow" 
                                        CommandArgument='<%# Eval("ID") %>' CssClass="link-btn delete"
                                        OnClientClick="return confirmDelete();">Delete</asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                        <EmptyDataTemplate>
                            <tr><td colspan="3" style="text-align:center; padding:10px; color:#888;">No salary components. Click "Add Component" to add.</td></tr>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>
                <div style="margin-top:10px;">
                    <asp:DropDownList ID="ddlComponentToAdd" runat="server" DataValueField="ComponentID" DataTextField="ComponentName" />
                    <asp:Button ID="btnAddComponent" runat="server" Text="Add Component" CssClass="btn btn-primary btn-sm" OnClick="btnAddComponent_Click" />
                    <asp:Button ID="btnCalculateTotal" runat="server" Text="Calculate Total" CssClass="btn btn-secondary btn-sm" OnClick="btnCalculateTotal_Click" />
                    <asp:Label ID="lblTotal" runat="server" Font-Bold="true" ForeColor="DarkGreen" />
                </div>
            </div>

            <!-- Action Buttons -->
            <div class="action-buttons">
                <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-secondary" OnClick="btnCancel_Click" CausesValidation="false" />
                <asp:Button ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-danger" OnClick="btnDelete_Click" OnClientClick="return confirmDelete();" />
                <asp:Button ID="btnUpdate" runat="server" Text="Update" CssClass="btn btn-warning" OnClick="btnUpdate_Click" />
                <asp:Button ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
            </div>
        </div>

    </div>

</asp:Content>