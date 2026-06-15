<%@ Page Title="Employee Salary Details" Language="C#" MasterPageFile="~/Masters/MastersSite.master" AutoEventWireup="true" CodeFile="EmployeeSalaryDetails.aspx.cs" Inherits="Masters_EmployeeSalaryDetails" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="PageTitle" runat="server">Employee Salary Details</asp:Content>
<asp:Content ID="NavTitleContent" ContentPlaceHolderID="NavTitle" runat="server">Salary Details</asp:Content>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        /* Tab styling */
        .salary-tabs { display: flex; border-bottom: 1px solid #ddd; margin-bottom: 20px; background: #f8f9fa; }
        .salary-tab { padding: 10px 20px; cursor: pointer; background: #e9ecef; margin-right: 5px; border-radius: 5px 5px 0 0; }
        .salary-tab.active { background: #007bff; color: white; }
        .tab-pane { display: none; }
        .tab-pane.active { display: block; }
        .salary-grid { margin-top: 15px; overflow-x: auto; }
        .salary-grid table { width: 100%; border-collapse: collapse; }
        .salary-grid th, .salary-grid td { border: 1px solid #ddd; padding: 8px; text-align: left; }
        .salary-grid th { background: #f2f2f2; }
        .btn-sm { padding: 4px 8px; font-size: 12px; }
        .attachment-row { margin-bottom: 10px; }
    </style>
    <script type="text/javascript">
        function switchTab(tabId) {
            document.querySelectorAll('.salary-tab').forEach(t => t.classList.remove('active'));
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
            } else alert(msg);
        }
        function confirmDelete() { return confirm('Delete this record?'); }
    </script>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="form-card">
        <!-- Employee Search Bar -->
        <div class="form-row">
            <span class="form-label">Select Employee:</span>
            <asp:DropDownList ID="ddlEmployee" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlEmployee_SelectedIndexChanged" Width="300px" />
            <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary" OnClick="btnSearch_Click" style="margin-left:10px;" />
            <asp:Button ID="btnClearForm" runat="server" Text="Clear" CssClass="btn btn-secondary" OnClick="btnClearForm_Click" CausesValidation="false" style="margin-left:10px;" />
        </div>
        <asp:Label ID="lblMessage" runat="server" ForeColor="Red" Font-Size="12px" />
        <hr class="divider" />

        <!-- Tabs -->
        <div class="salary-tabs">
            <div class="salary-tab active" onclick="switchTab('working')">Working Location</div>
            <div class="salary-tab" onclick="switchTab('official')">Employee Official Information</div>
            <div class="salary-tab" onclick="switchTab('salary')">Employee Salary Structure</div>
            <div class="salary-tab" onclick="switchTab('deduction')">Employee Deduction Structure</div>
            <div class="salary-tab" onclick="switchTab('provision')">Provision Account Information</div>
            <div class="salary-tab" onclick="switchTab('attachment')">Attachment Info</div>
        </div>
        <asp:HiddenField ID="hfCurrentTab" runat="server" Value="working" />
        <asp:HiddenField ID="hfEmpCode" runat="server" Value="" />

        <!-- Tab Panes -->
        <!-- 1. Working Location -->
        <div id="pane_working" class="tab-pane active">
            <div class="form-row"><span class="form-label">Working Location:</span><asp:TextBox ID="txtWorkingLocation" runat="server" CssClass="form-input" Width="300px" /></div>
            <div class="form-row"><span class="form-label">Division:</span><asp:TextBox ID="txtDivision" runat="server" CssClass="form-input" Width="300px" /></div>
            <div class="form-row"><span class="form-label">Employee Status:</span><asp:DropDownList ID="ddlEmpStatus" runat="server"><asp:ListItem>Active</asp:ListItem><asp:ListItem>Inactive</asp:ListItem></asp:DropDownList></div>
        </div>

        <!-- 2. Employee Official Information -->
        <div id="pane_official" class="tab-pane">
            <div class="form-row"><span class="form-label">Date of Join:</span><asp:TextBox ID="txtJoinDate" runat="server" TextMode="Date" CssClass="form-input" /></div>
            <div class="form-row"><span class="form-label">Join Type:</span><asp:TextBox ID="txtJoinType" runat="server" CssClass="form-input" /></div>
            <div class="form-row"><span class="form-label">Contract Type:</span><asp:DropDownList ID="ddlContractType" runat="server"><asp:ListItem>UNLIMITED</asp:ListItem><asp:ListItem>LIMITED</asp:ListItem></asp:DropDownList></div>
            <div class="form-row"><span class="form-label">Last Provision Date:</span><asp:TextBox ID="txtLastProvisionDate" runat="server" TextMode="Date" CssClass="form-input" /></div>
            <div class="form-row"><span class="form-label">Normal Working Hrs.:</span><asp:TextBox ID="txtNormalWorkingHrs" runat="server" CssClass="form-input" /></div>
            <div class="form-row"><span class="form-label">Break Hours:</span><asp:TextBox ID="txtBreakHours" runat="server" CssClass="form-input" /></div>
            <div class="form-row"><span class="form-label">Emp. Overtime Rate:</span><asp:TextBox ID="txtOvertimeRate" runat="server" CssClass="form-input" /></div>
            <div class="form-row"><span class="form-label">Emp. Incentive Rate:</span><asp:TextBox ID="txtIncentiveRate" runat="server" CssClass="form-input" /></div>
            <div class="form-row"><span class="form-label">Last ESOB Date:</span><asp:TextBox ID="txtLastESOBDate" runat="server" TextMode="Date" CssClass="form-input" /></div>
            <div class="form-row"><span class="form-label">ReAgreement Date:</span><asp:TextBox ID="txtReAgreementDate" runat="server" TextMode="Date" CssClass="form-input" /></div>
            <div class="form-row"><span class="form-label">Employee Type:</span><asp:TextBox ID="txtEmployeeType" runat="server" CssClass="form-input" /></div>
            <div class="form-row"><span class="form-label">Grade Sub:</span><asp:TextBox ID="txtGradeSub" runat="server" CssClass="form-input" /></div>
        </div>

        <!-- 3. Employee Salary Structure (Table) -->
        <div id="pane_salary" class="tab-pane">
            <div class="salary-grid">
                <asp:GridView ID="gvSalaryComponents" runat="server" AutoGenerateColumns="false" OnRowCommand="gvSalaryComponents_RowCommand" DataKeyNames="ID">
                    <Columns>
                        <asp:TemplateField HeaderText="Code"><ItemTemplate><%# Eval("ID") %></ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Salary Type"><ItemTemplate><asp:TextBox ID="txtSalType" runat="server" Text='<%# Eval("SalaryType") %>' Width="100px" /></ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Account Code"><ItemTemplate><asp:TextBox ID="txtAccCode" runat="server" Text='<%# Eval("AccountCode") %>' Width="100px" /></ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Account Name"><ItemTemplate><asp:TextBox ID="txtAccName" runat="server" Text='<%# Eval("AccountName") %>' Width="150px" /></ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Daily Amount"><ItemTemplate><asp:TextBox ID="txtDaily" runat="server" Text='<%# Eval("DailyAmount") %>' Width="80px" /></ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Hourly Amount"><ItemTemplate><asp:TextBox ID="txtHourly" runat="server" Text='<%# Eval("HourlyAmount") %>' Width="80px" /></ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Salary N"><ItemTemplate><asp:TextBox ID="txtSalaryN" runat="server" Text='<%# Eval("SalaryN") %>' Width="80px" /></ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="LedgerType"><ItemTemplate><asp:TextBox ID="txtLedgerType" runat="server" Text='<%# Eval("LedgerType") %>' Width="100px" /></ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Deduction Type"><ItemTemplate><asp:TextBox ID="txtDedType" runat="server" Text='<%# Eval("DeductionType") %>' Width="100px" /></ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Employer Dr A/C"><ItemTemplate><asp:TextBox ID="txtEmpDrAc" runat="server" Text='<%# Eval("EmployerDrAc") %>' Width="100px" /></ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Employer Dr A/C Name"><ItemTemplate><asp:TextBox ID="txtEmpDrName" runat="server" Text='<%# Eval("EmployerDrAcName") %>' Width="150px" /></ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Employer Cr A/C"><ItemTemplate><asp:TextBox ID="txtEmpCrAc" runat="server" Text='<%# Eval("EmployerCrAc") %>' Width="100px" /></ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Actions">
                            <ItemTemplate>
                                <asp:LinkButton ID="lnkRemove" runat="server" CommandName="RemoveRow" CommandArgument='<%# Eval("ID") %>' OnClientClick="return confirmDelete();" CssClass="link-btn delete">Delete</asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
                <asp:Button ID="btnAddSalaryRow" runat="server" Text="Add Row" CssClass="btn btn-secondary btn-sm" OnClick="btnAddSalaryRow_Click" style="margin-top:10px;" />
            </div>
        </div>

        <!-- 4. Employee Deduction Structure (similar table but with deduction fields, we'll reuse same grid with dynamic columns? To keep simple, separate GridView) -->
        <div id="pane_deduction" class="tab-pane">
            <div class="form-row"><span class="form-label">Food Allowance Paid by Comp:</span><asp:CheckBox ID="chkFoodAllowancePaid" runat="server" /></div>
            <div class="form-row"><span class="form-label">Food Allowance Ded Amount:</span><asp:TextBox ID="txtFoodAllowanceDed" runat="server" CssClass="form-input" /></div>
            <div class="form-row"><span class="form-label">Trip Emp Fixed Salary:</span><asp:TextBox ID="txtTripEmpFixedSalary" runat="server" CssClass="form-input" /></div>
            <div class="form-row"><span class="form-label">Air Sector:</span><asp:TextBox ID="txtAirSector" runat="server" CssClass="form-input" /></div>
            <div class="form-row"><span class="form-label">Air Ticket Interval:</span><asp:TextBox ID="txtAirTicketInterval" runat="server" CssClass="form-input" /></div>
            <div class="form-row"><span class="form-label">Air Ticket (Total Count):</span><asp:TextBox ID="txtAirTicketTotalCount" runat="server" CssClass="form-input" /></div>
            <div class="form-row"><span class="form-label">No of child ticket:</span><asp:TextBox ID="txtNoChildTicket" runat="server" CssClass="form-input" /></div>
            <div class="form-row"><span class="form-label">No of infant ticket:</span><asp:TextBox ID="txtNoInfantTicket" runat="server" CssClass="form-input" /></div>
            <div class="form-row"><span class="form-label">Pensions (Yes/No):</span><asp:CheckBox ID="chkPensions" runat="server" Text="Yes" /></div>
            <div class="form-row"><span class="form-label">Extra Benefit Amt Y/N:</span><asp:CheckBox ID="chkExtraBenefit" runat="server" Text="Yes" /></div>
        </div>

        <!-- 5. Provision Account Information -->
        <div id="pane_provision" class="tab-pane">
            <div class="salary-grid">
                <asp:GridView ID="gvProvision" runat="server" AutoGenerateColumns="false" OnRowCommand="gvProvision_RowCommand" DataKeyNames="ID">
                    <Columns>
                        <asp:TemplateField HeaderText="Provision Type"><ItemTemplate><asp:TextBox ID="txtProvType" runat="server" Text='<%# Eval("ProvisionType") %>' Width="120px" /></ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Provisional Account"><ItemTemplate><asp:TextBox ID="txtProvAcc" runat="server" Text='<%# Eval("ProvisionalAccount") %>' Width="120px" /></ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Pro.Account Name"><ItemTemplate><asp:TextBox ID="txtProvAccName" runat="server" Text='<%# Eval("ProAccountName") %>' Width="150px" /></ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Expense Account"><ItemTemplate><asp:TextBox ID="txtExpAcc" runat="server" Text='<%# Eval("ExpenseAccount") %>' Width="120px" /></ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Exp.Account Name"><ItemTemplate><asp:TextBox ID="txtExpAccName" runat="server" Text='<%# Eval("ExpAccountName") %>' Width="150px" /></ItemTemplate></asp:TemplateField>
                        <asp:TemplateField HeaderText="Actions">
                            <ItemTemplate>
                                <asp:LinkButton ID="lnkRemoveProv" runat="server" CommandName="RemoveProv" CommandArgument='<%# Eval("ID") %>' OnClientClick="return confirmDelete();" CssClass="link-btn delete">Delete</asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
                <asp:Button ID="btnAddProvisionRow" runat="server" Text="Add Row" CssClass="btn btn-secondary btn-sm" OnClick="btnAddProvisionRow_Click" />
            </div>
        </div>

        <!-- 6. Attachment Info -->
        <div id="pane_attachment" class="tab-pane">
            <div class="attachment-row">
                <span class="form-label">Remarks:</span><asp:TextBox ID="txtAttachRemarks" runat="server" CssClass="form-input" Width="300px" />
                <asp:FileUpload ID="fuAttachment" runat="server" />
                <asp:Button ID="btnUploadAttachment" runat="server" Text="Upload" CssClass="btn btn-primary btn-sm" OnClick="btnUploadAttachment_Click" />
            </div>
            <div class="salary-grid">
                <asp:GridView ID="gvAttachments" runat="server" AutoGenerateColumns="false" OnRowCommand="gvAttachments_RowCommand" DataKeyNames="ID">
                    <Columns>
                        <asp:BoundField DataField="FileName" HeaderText="File Name" />
                        <asp:BoundField DataField="Remarks" HeaderText="Remarks" />
                        <asp:TemplateField HeaderText="Delete">
                            <ItemTemplate>
                                <asp:LinkButton ID="lnkDelAtt" runat="server" CommandName="DeleteAtt" CommandArgument='<%# Eval("ID") %>' OnClientClick="return confirmDelete();" CssClass="link-btn delete">Delete</asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>

        <hr class="divider" />
        <!-- Approval & Buttons -->
        <div class="form-row">
            <span class="form-label">Verified By:</span><asp:TextBox ID="txtVerifiedBy" runat="server" CssClass="form-input" Width="150px" />
            <span class="form-label">Remarks:</span><asp:TextBox ID="txtRemarks" runat="server" CssClass="form-input" Width="300px" />
        </div>
        <div class="btn-row">
            <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-secondary" OnClick="btnClear_Click" />
            <asp:Button ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
            <asp:Button ID="btnUpdate" runat="server" Text="Update" CssClass="btn btn-warning" OnClick="btnUpdate_Click" />
            <asp:Button ID="btnPrint" runat="server" Text="Print" CssClass="btn btn-info" OnClick="btnPrint_Click" />
        </div>
    </div>
    <script type="text/javascript">
        // Restore active tab after postback
        window.addEventListener('load', function() {
            var savedTab = document.getElementById('<%= hfCurrentTab.ClientID %>').value;
            if (savedTab) switchTab(savedTab);
        });
    </script>
</asp:Content>