<%@ Page Title="Vendor Master" Language="C#" EnableEventValidation="false" MasterPageFile="~/Masters/MastersSite.master"
    AutoEventWireup="true" CodeFile="VendorMaster.aspx.cs" Inherits="Masters_VendorMaster" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="PageTitle" runat="server">Vendor Master</asp:Content>
<asp:Content ID="NavTitleContent" ContentPlaceHolderID="NavTitle" runat="server">Vendor Master</asp:Content>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
<style>
    #vmToast{display:none;position:fixed;bottom:28px;right:28px;min-width:260px;max-width:420px;
        padding:13px 18px;border-radius:6px;font-size:13.5px;font-family:inherit;
        box-shadow:0 4px 16px rgba(0,0,0,.18);z-index:99999;line-height:1.45;
        word-break:break-word;opacity:1;transition:opacity .3s;}
</style>
<script type="text/javascript">
    var _toastTimer = null;
    function showToast(msg, type) {
        var el = document.getElementById('vmToast');
        if (!el) { alert(msg); return; }
        clearTimeout(_toastTimer);
        el.style.opacity = '1';
        el.style.display = 'block';
        el.style.background = (type === 'success') ? '#2e7d32' : '#c62828';
        el.style.color = '#fff';
        el.style.borderLeft = '5px solid ' + ((type === 'success') ? '#1b5e20' : '#7f0000');
        el.innerHTML = (type === 'success' ? '&#10003; ' : '&#9888; ') + msg;
        _toastTimer = setTimeout(function () {
            el.style.opacity = '0';
            setTimeout(function () { el.style.display = 'none'; el.style.opacity = '1'; }, 320);
        }, 5000);
    }
</script>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
<div class="vm-wrap">

<!-- ===================== HIDDEN FIELDS ===================== -->
<asp:HiddenField ID="hfVendorCode"    runat="server" Value="0" />
<asp:HiddenField ID="hfCurrentTab"   runat="server" Value="tab-details" />
<asp:HiddenField ID="hfView"         runat="server" Value="list" />
<asp:HiddenField ID="hfBalFilter"    runat="server" Value="0" />
<asp:HiddenField ID="hfPageIndex"    runat="server" Value="0" />
<asp:HiddenField ID="hfPageSize"     runat="server" Value="5" />
<asp:HiddenField ID="hfNHSLNO"      runat="server" Value="0" />
<asp:Button ID="btnPopupSelect" runat="server" Text="" 
    Style="display:none;" 
    OnClick="btnPopupSelect_Click" />

<!-- ===================== LIST PANEL ===================== -->
<div class="vm-list-panel" id="divListPanel">

    <!-- TOOLBAR -->
    <div class="vm-toolbar">
        <asp:Button ID="btnAddNew" runat="server" Text="+" CssClass="vm-btn-add"
            OnClick="btnAddNew_Click" ToolTip="Add New Vendor" />

        <div class="vm-radio-group">
            <label><asp:RadioButton ID="rbWithout" runat="server" GroupName="BalFilter" Text="Without Balance" Checked="true" AutoPostBack="true" OnCheckedChanged="rbFilter_Changed" /></label>
            <label><asp:RadioButton ID="rbWith"    runat="server" GroupName="BalFilter" Text="With Balance"    AutoPostBack="true" OnCheckedChanged="rbFilter_Changed" /></label>
            <label><asp:RadioButton ID="rbWithTxn" runat="server" GroupName="BalFilter" Text="With Transaction" AutoPostBack="true" OnCheckedChanged="rbFilter_Changed" /></label>
        </div>

        <div class="vm-toolbar-sep"></div>

        <button type="button" class="vm-icon-btn" onclick="refreshGrid()" title="Refresh">&#x21BB;</button>
        <asp:Button ID="btnExportExcel" runat="server" Text="&#128196;" CssClass="vm-icon-btn" OnClick="btnExportExcel_Click" ToolTip="Export to Excel" />
        <button type="button" class="vm-icon-btn" onclick="window.print()" title="Print">&#128438;</button>

        <div class="vm-search-top">
            <span>&#128269;</span>
            <input type="text" id="txtGlobalSearch" placeholder="Search..." oninput="applyGlobalSearch(this.value)" />
        </div>
    </div>

    <!-- GRID -->
    <div class="vm-grid-wrap">
        <table class="vm-grid" id="vmMainGrid">
            <thead>
                <tr class="hdr-main">
                    <th>Code</th>
                    <th>Name</th>
                    <th>Phone</th>
                    <th>Vendor Type</th>
                    <th>Pay Term</th>
                    <th>Credit Limit</th>
                    <th>Status</th>
                    <th>Address</th>
                    <th>Location</th>
                    <th>Contract Person</th>
                    <th>Person Designation</th>
                    <th>Person Mob</th>
                    <th>VAT REG NO</th>
                </tr>
                <tr class="hdr-filter">
                    <th><input type="text" placeholder="&#128269;" oninput="filterCol(0,this.value)" /></th>
                    <th><input type="text" placeholder="&#128269;" oninput="filterCol(1,this.value)" /></th>
                    <th><input type="text" placeholder="&#128269;" oninput="filterCol(2,this.value)" /></th>
                    <th><input type="text" placeholder="&#128269;" oninput="filterCol(3,this.value)" /></th>
                    <th><input type="text" placeholder="&#128269;" oninput="filterCol(4,this.value)" /></th>
                    <th><input type="text" placeholder="&#128269;" oninput="filterCol(5,this.value)" /></th>
                    <th><input type="text" placeholder="&#128269;" oninput="filterCol(6,this.value)" /></th>
                    <th><input type="text" placeholder="&#128269;" oninput="filterCol(7,this.value)" /></th>
                    <th></th>
                    <th><input type="text" placeholder="&#128269;" oninput="filterCol(9,this.value)" /></th>
                    <th></th>
                    <th></th>
                    <th><input type="text" placeholder="&#128269;" oninput="filterCol(12,this.value)" /></th>
                </tr>
            </thead>
            <tbody id="vmGridBody">
                <!-- filled by JS from hfGridData -->
            </tbody>
        </table>
    </div>

    <!-- PAGINATION -->
    <div class="vm-pagination">
        <div class="vm-page-size">
            <select id="ddlPageSize" onchange="changePageSize(this.value)">
                <option value="5">5</option>
                <option value="10">10</option>
                <option value="25">25</option>
                <option value="50">50</option>
            </select>
            <span>rows per page</span>
        </div>
        <span class="vm-page-info" id="pageInfo"></span>
        <div class="vm-page-nav">
            <button onclick="goPage('first')">&laquo;</button>
            <button onclick="goPage('prev')">&lsaquo;</button>
            <span id="pageNumDisp" style="padding:0 8px;font-size:12.5px;"></span>
            <button onclick="goPage('next')">&rsaquo;</button>
            <button onclick="goPage('last')">&raquo;</button>
        </div>
    </div>

    <!-- Hidden: grid data as JSON from server -->
    <asp:HiddenField ID="hfGridData"    runat="server" Value="[]" />
    <asp:HiddenField ID="hfGridPageIdx" runat="server" Value="0" />
    <asp:HiddenField ID="hfGridPageSz"  runat="server" Value="5" />

</div><!-- /list panel -->

<!-- ===================== FORM PANEL ===================== -->
<div class="vm-form-panel" id="divFormPanel">

    <!-- TABS -->
    <div class="vm-tabs">
        <div class="vm-tab active" id="tab-details"          onclick="switchTab('tab-details')">Details</div>
        <div class="vm-tab vm-tab-disabled"        id="tab-bank">Bank Details</div>
        <div class="vm-tab"        id="tab-namehistory"      onclick="switchTab('tab-namehistory')">Name History</div>
        <div class="vm-tab"        id="tab-attachments"      onclick="switchTab('tab-attachments')">Attachment Details</div>
        <div class="vm-tab"        id="tab-branchmapping"    onclick="switchTab('tab-branchmapping')">Vendor Branch Mapping</div>
        <div class="vm-tab"        id="tab-checklist"        onclick="switchTab('tab-checklist')">Vendor CheckList</div>
        <div class="vm-tab"        id="tab-audit"            onclick="switchTab('tab-audit')">Vendor Audit</div>
    </div>

    <!-- ===== TAB: DETAILS ===== -->
  <div class="vm-tab-body">
    <div id="tc-tab-details" class="vm-tab-content active">

        <!-- Code / Name row -->
        <div class="vm-form-row">
            <span class="vm-lbl">Code:</span>
            <asp:TextBox ID="txtCode" runat="server" CssClass="vm-inp vm-inp-sm" ReadOnly="true" />
            <asp:DropDownList ID="ddlCodeFilter" runat="server" CssClass="vm-sel vm-sel-md">
                <asp:ListItem Value="ALL">ALL</asp:ListItem>
                <asp:ListItem Value="ACTIVE">ACTIVE</asp:ListItem>
                <asp:ListItem Value="IN ACTIVE">IN ACTIVE</asp:ListItem>
            </asp:DropDownList>
            <asp:Button ID="btnSearchCode" runat="server" Text="&#128269; Search" CssClass="vm-btn vm-btn-save vm-btn-sm" />
            <asp:CheckBox ID="chkWithBalance" runat="server" Text="With Balance" />
            <span style="margin-left:auto; font-size:12px; color:#555;">
                History Name Search &nbsp;<button type="button" class="vm-icon-btn" style="width:22px;height:22px;font-size:12px;">&#128269;</button>
            </span>
        </div>
        <div class="vm-form-row">
            <span class="vm-lbl">Name:</span>
            <asp:TextBox ID="txtName" runat="server" CssClass="vm-inp vm-inp-xl" />
        </div>

        <!-- ===================== VENDOR SEARCH POPUP ===================== -->
<div id="vmSearchOverlay" style="display:none;position:fixed;inset:0;background:rgba(0,0,0,.45);z-index:100000;align-items:center;justify-content:center;">
    <div style="background:#fff;border-radius:8px;box-shadow:0 8px 32px rgba(0,0,0,.28);
                width:min(820px,95vw);max-height:80vh;display:flex;flex-direction:column;overflow:hidden;">
        <!-- Header -->
        <div style="display:flex;align-items:center;justify-content:space-between;
                    padding:12px 16px;background:#1565c0;color:#fff;border-radius:8px 8px 0 0;">
            <span style="font-weight:700;font-size:14px;">&#128269; Select Vendor</span>
            <button onclick="closeVMSearch()" style="background:none;border:none;color:#fff;font-size:18px;cursor:pointer;line-height:1;">&times;</button>
        </div>
        <!-- Search bar -->
        <div style="padding:10px 16px;border-bottom:1px solid #e0e0e0;">
            <input id="vmPopupSearch" type="text" placeholder="Type to filter by Code, Name, Phone, Type, Status..."
                   oninput="vmPopupFilter(this.value)"
                   style="width:100%;padding:7px 10px;border:1px solid #bdbdbd;border-radius:4px;
                          font-size:13px;box-sizing:border-box;" />
        </div>
        <!-- Grid -->
        <div style="overflow:auto;flex:1;">
            <table style="width:100%;border-collapse:collapse;font-size:12.5px;">
                <thead>
                    <tr style="background:#e3f2fd;position:sticky;top:0;z-index:1;">
                        <th style="padding:7px 10px;text-align:left;border-bottom:2px solid #90caf9;white-space:nowrap;">Code</th>
                        <th style="padding:7px 10px;text-align:left;border-bottom:2px solid #90caf9;">Name</th>
                        <th style="padding:7px 10px;text-align:left;border-bottom:2px solid #90caf9;white-space:nowrap;">Phone</th>
                        <th style="padding:7px 10px;text-align:left;border-bottom:2px solid #90caf9;white-space:nowrap;">Vendor Type</th>
                        <th style="padding:7px 10px;text-align:left;border-bottom:2px solid #90caf9;white-space:nowrap;">Status</th>
                        <th style="padding:7px 10px;text-align:left;border-bottom:2px solid #90caf9;">Address</th>
                        <th style="padding:7px 10px;text-align:left;border-bottom:2px solid #90caf9;white-space:nowrap;">VAT Reg No</th>
                    </tr>
                </thead>
                <tbody id="vmPopupBody"></tbody>
            </table>
        </div>
        <!-- Footer -->
        <div id="vmPopupCount" style="padding:7px 16px;border-top:1px solid #e0e0e0;
                                      font-size:12px;color:#757575;text-align:right;"></div>
    </div>
</div>

        <div class="vm-details-cols">
            <!-- LEFT COLUMN -->
            <div class="vm-col-left">
                <div class="vm-form-row">
                    <span class="vm-lbl">Full Name (LL):</span>
                    <asp:TextBox ID="txtFullNameLL" runat="server" CssClass="vm-inp vm-inp-lg" />
                </div>
                <div class="vm-form-row">
                    <span class="vm-lbl">Print in Cheque:</span>
                    <asp:TextBox ID="txtPrintInCheque" runat="server" CssClass="vm-inp vm-inp-lg" />
                </div>
                <div class="vm-form-row">
                    <span class="vm-lbl">Account Group:</span>
                    <asp:DropDownList ID="ddlAccountGroup" runat="server" CssClass="vm-sel vm-sel-lg">
                        <asp:ListItem Value="">---- Select Account Group ----</asp:ListItem>
                        <asp:ListItem Value="DIRECT MATERIAL SUPPLIERS">DIRECT MATERIAL SUPPLIERS</asp:ListItem>
                        <asp:ListItem Value="INDIRECT MATERIAL SUPPLIERS">INDIRECT MATERIAL SUPPLIERS</asp:ListItem>
                    </asp:DropDownList>
                </div>

                <!-- CONTACT INFO -->
                <div class="vm-section-box" style="margin-top:10px;">
                    <span class="vm-section-title">Contact Info.</span>
                    <div class="vm-form-row">
                        <span class="vm-lbl">Supplier Origin Country</span>
                        <asp:TextBox ID="txtOriginCode" runat="server" CssClass="vm-inp vm-inp-sm" />
                        <button type="button" class="vm-icon-btn" style="width:24px;height:24px;font-size:12px;">&#128269;</button>
                        <asp:TextBox ID="txtOriginCountry" runat="server" CssClass="vm-inp vm-inp-md" />
                    </div>
                    <div class="vm-form-row">
                        <span class="vm-lbl">State</span>
                        <asp:DropDownList ID="ddlState" runat="server" CssClass="vm-sel vm-sel-lg">
                            <asp:ListItem Value="">---- Select State ----</asp:ListItem>
                            <asp:ListItem Value="VARIOUS EMIRATES">VARIOUS EMIRATES</asp:ListItem>
                            <asp:ListItem Value="ABU DHABI">ABU DHABI</asp:ListItem>
                            <asp:ListItem Value="AJMAN">AJMAN</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="vm-form-row">
                        <span class="vm-lbl">P.O Box</span>
                        <asp:TextBox ID="txtPOBox" runat="server" CssClass="vm-inp vm-inp-lg" />
                    </div>
                    <div class="vm-form-row">
                        <span class="vm-lbl">Address</span>
                        <asp:TextBox ID="txtAddress" runat="server" CssClass="vm-inp vm-inp-lg" />
                    </div>
                    <div class="vm-form-row">
                        <span class="vm-lbl">City</span>
                        <asp:TextBox ID="txtCity" runat="server" CssClass="vm-inp vm-inp-lg" />
                    </div>
                    <div class="vm-form-row">
                        <span class="vm-lbl">Tele Phone No</span>
                        <asp:TextBox ID="txtTelephoneNo" runat="server" CssClass="vm-inp vm-inp-lg" />
                    </div>
                    <div class="vm-form-row">
                        <span class="vm-lbl">Fax No</span>
                        <asp:TextBox ID="txtFaxNo" runat="server" CssClass="vm-inp vm-inp-lg" />
                    </div>
                    <div class="vm-form-row">
                        <span class="vm-lbl">Email</span>
                        <asp:TextBox ID="txtEmail" runat="server" CssClass="vm-inp vm-inp-lg" />
                    </div>
                    <div class="vm-form-row">
                        <span class="vm-lbl">Web Site</span>
                        <asp:TextBox ID="txtWebSite" runat="server" CssClass="vm-inp vm-inp-lg" />
                    </div>
                    <div class="vm-form-row">
                        <span class="vm-lbl">Vendor Remarks</span>
                        <asp:TextBox ID="txtVendorRemarks" runat="server" CssClass="vm-inp vm-inp-lg" />
                    </div>
                    <div class="vm-form-row">
                        <span class="vm-lbl">Eval. Remarks</span>
                        <asp:TextBox ID="txtEvalRemarks" runat="server" CssClass="vm-inp vm-inp-lg" />
                    </div>
                </div><!-- /contact info -->
            </div><!-- /col-left -->

            <!-- RIGHT COLUMN -->
            <div class="vm-col-right">
                <div class="vm-form-row">
                    <span class="vm-lbl" style="width:110px;">Invoicing Qty</span>
                    <asp:RadioButtonList ID="rblInvoicingQty" runat="server" RepeatDirection="Horizontal" Font-Size="12px">
                        <asp:ListItem Text="Supplier"     Value="Supplier"     Selected="True" />
                        <asp:ListItem Text="Measured Qty" Value="Measured Qty" />
                    </asp:RadioButtonList>
                </div>
                <div class="vm-form-row">
                    <span class="vm-lbl" style="width:110px;">Branch:</span>
                    <asp:DropDownList ID="ddlBranch" runat="server" CssClass="vm-sel vm-sel-md">
                        <asp:ListItem Value="">---- Select Branch ----</asp:ListItem>
                        <asp:ListItem Value="CEMENT PRODUCTS FACTORY L.L.C - SOLE PROPRIETORSHIP L.L.C">CEMENT PRODUCTS FACTORY L.L.C - SOLE PROPRIETORSHIP L.L.C</asp:ListItem>
                        <asp:ListItem Value="PRECAST FACTORY L.L.C - SOLE PROPRIETORSHIP L.L.C">PRECAST FACTORY L.L.C - SOLE PROPRIETORSHIP L.L.C</asp:ListItem>
                        <asp:ListItem Value="READY MIX MANUFACTURING - SOLE PROPRIETORSHIP LLC">READY MIX MANUFACTURING - SOLE PROPRIETORSHIP LLC</asp:ListItem>

                    </asp:DropDownList>
                </div>
                <div class="vm-form-row">
                    <span class="vm-lbl" style="width:110px;">Vendor Type</span>
                    <asp:DropDownList ID="ddlVendorType" runat="server" CssClass="vm-sel vm-sel-md">
                        <asp:ListItem Value="RAW MATERIAL SUPPLIER">RAW MATERIAL SUPPLIER</asp:ListItem>
                        <asp:ListItem Value="TRANSPORTER And EQUIPMENT">TRANSPORTER And EQUIPMENT</asp:ListItem>
                        <asp:ListItem Value="SERVICE PROVIDER">SERVICE PROVIDER</asp:ListItem>
                    </asp:DropDownList>
                </div>

                <!-- CONTACT PERSON -->
                <div class="vm-section-box" style="margin-top:12px;">
                    <span class="vm-section-title">Contact Person.</span>
                    <table class="vm-cp-table">
                        <thead>
                            <tr>
                                <th>Name</th><th>Desig</th><th>Mobile No</th><th>Email</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr>
                                <td><asp:TextBox ID="txtCP1Name"   runat="server" CssClass="vm-inp" style="width:100%" /></td>
                                <td><asp:TextBox ID="txtCP1Desig"  runat="server" CssClass="vm-inp" style="width:100%" /></td>
                                <td><asp:TextBox ID="txtCP1Mobile" runat="server" CssClass="vm-inp" style="width:100%" /></td>
                                <td><asp:TextBox ID="txtCP1Email"  runat="server" CssClass="vm-inp" style="width:100%" /></td>
                            </tr>
                            <tr>
                                <td><asp:TextBox ID="txtCP2Name"   runat="server" CssClass="vm-inp" style="width:100%" /></td>
                                <td><asp:TextBox ID="txtCP2Desig"  runat="server" CssClass="vm-inp" style="width:100%" /></td>
                                <td><asp:TextBox ID="txtCP2Mobile" runat="server" CssClass="vm-inp" style="width:100%" /></td>
                                <td><asp:TextBox ID="txtCP2Email"  runat="server" CssClass="vm-inp" style="width:100%" /></td>
                            </tr>
                        </tbody>
                    </table>
                </div>

                <!-- BUSINESS DETAILS -->
               <div class="vm-section-box" style="margin-top:12px;">
    <span class="vm-section-title">Business Details</span>

    <div class="vm-form-row">
        <span class="vm-lbl" style="width:130px;">Business Type</span>
        <asp:TextBox ID="txtBusinessType" runat="server" CssClass="vm-inp vm-inp-md" />
    </div>

    <div class="vm-form-row">
        <span class="vm-lbl" style="width:130px;">Trade License No</span>
        <asp:TextBox ID="txtTradeLicenseNo" runat="server" CssClass="vm-inp vm-inp-md" />
    </div>

    <div class="vm-form-row">
        <span class="vm-lbl" style="width:130px;">Evaluation Category</span>
        <asp:DropDownList ID="ddlEvalCategory" runat="server" CssClass="vm-sel vm-sel-md">
            <asp:ListItem Value="A">A - CATEGORY</asp:ListItem>
            <asp:ListItem Value="B">B - CATEGORY</asp:ListItem>
            <asp:ListItem Value="C">C - CATEGORY</asp:ListItem>
        </asp:DropDownList>
    </div>

    <div class="vm-form-row">
        <span class="vm-lbl" style="width:130px;">License Exp Date</span>
        <asp:TextBox ID="txtLicenseExpDate" runat="server"
            CssClass="vm-inp vm-inp-sm"
            TextMode="Date" />
    </div>

    <div class="vm-form-row">
        <span class="vm-lbl" style="width:130px;">Eval Exp Date</span>
        <asp:TextBox ID="txtEvalExpDate" runat="server"
            CssClass="vm-inp vm-inp-sm"
            TextMode="Date" />
    </div>

    <div class="vm-form-row">
        <span class="vm-lbl" style="width:130px;">VAT Reg No</span>
        <asp:TextBox ID="txtVATRegNo" runat="server"
            CssClass="vm-inp vm-inp-md" />
    </div>

    <div class="vm-form-row">
        <span class="vm-lbl" style="width:130px;">TRN Date</span>
        <asp:TextBox ID="txtTRNDate" runat="server"
            CssClass="vm-inp vm-inp-sm"
            TextMode="Date" />
    </div>
</div>
            </div><!-- /col-right -->
        </div><!-- /details-cols -->

    </div><!-- /tc-tab-details -->

    <!-- ===== TAB: BANK DETAILS ===== -->
    <div id="tc-tab-bank" class="vm-tab-content">
        
    </div>

    <!-- ===== TAB: NAME HISTORY ===== -->
    <div id="tc-tab-namehistory" class="vm-tab-content">
        <div class="vm-form-row" style="margin-bottom:10px;">
            <span class="vm-lbl">History Name</span>
            <asp:TextBox ID="txtHistoryName" runat="server" CssClass="vm-inp vm-inp-lg" />
            <span style="margin-left:20px; font-weight:600; font-size:12.5px; color:#555;">Replace Date</span>
            <asp:TextBox ID="txtReplaceDate" runat="server" CssClass="vm-inp vm-inp-sm" TextMode="Date" />
        </div>
        <div class="vm-form-row" style="margin-bottom:10px;">
            <span class="vm-lbl">CHQ History Name</span>
            <asp:TextBox ID="txtCHQHistoryName" runat="server" CssClass="vm-inp vm-inp-lg" />
            <asp:Button ID="btnAddHistory" runat="server" Text="Add" CssClass="vm-btn vm-btn-save vm-btn-sm"
                OnClick="btnAddHistory_Click" style="margin-left:10px;" />
        </div>
        <div class="vm-sub-grid-wrap">
            <asp:GridView ID="gvNameHistory" runat="server" AutoGenerateColumns="false"
                CssClass="vm-sub-grid" DataKeyNames="SLNO"
                OnRowCommand="gvNameHistory_RowCommand">
                <Columns>
                    <asp:BoundField DataField="SLNO"          HeaderText="SL NO" />
                    <asp:BoundField DataField="VendorCode"    HeaderText="Code" />
                    <asp:BoundField DataField="HistoryName"   HeaderText="History Name" />
                    <asp:BoundField DataField="CHQHistoryName" HeaderText="CHQ History Name" />
                    <asp:BoundField DataField="ChangeDate"    HeaderText="Change Date" DataFormatString="{0:dd/MM/yyyy}" />
                    <asp:TemplateField HeaderText="Edit">
                        <ItemTemplate>
                            <asp:Button ID="btnNHEdit" runat="server" Text="Edit" CssClass="vm-btn vm-btn-update vm-btn-sm"
                                CommandName="EditNH" CommandArgument='<%# Eval("SLNO") %>' />
                            <asp:Button ID="btnNHDel"  runat="server" Text="Del"  CssClass="vm-btn vm-btn-del vm-btn-sm"
                                CommandName="DeleteNH" CommandArgument='<%# Eval("SLNO") %>'
                                OnClientClick="return confirm('Delete this history record?');" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </div>

    <!-- ===== TAB: ATTACHMENT DETAILS ===== -->
     <div id="tc-tab-attachments" class="vm-tab-content">
     <div class="vm-attach-bar">
         <span class="vm-lbl">Remarks:</span>
         <asp:TextBox ID="txtAttachRemarks" runat="server" CssClass="vm-inp vm-inp-lg" TextMode="MultiLine" Rows="2" style="height:42px;" />
         <span class="vm-file-note">*Maximum Size: 3MB</span>
         <asp:FileUpload ID="fuAttach" runat="server" />
         <asp:Button ID="btnUpload" runat="server" Text="UpLoad" CssClass="vm-btn vm-btn-save vm-btn-sm" OnClick="btnUpload_Click" />
     </div>
     <div class="vm-section-box" style="padding:8px;">
         <span class="vm-section-title">Attaching Files</span>
         <div class="vm-sub-grid-wrap">
            
    
            <asp:GridView ID="gvAttachments" runat="server"
    AutoGenerateColumns="false"
    CssClass="vm-sub-grid"
    DataKeyNames="AttachID"
    OnRowCommand="gvAttachments_RowCommand">

    <Columns>
        <asp:TemplateField HeaderText="">
         
            <ItemTemplate>
                <asp:CheckBox ID="chkSelect" runat="server" />
            </ItemTemplate>
        </asp:TemplateField>

        <asp:BoundField DataField="AttachID" HeaderText="#" />
        <asp:BoundField DataField="VendorCode" HeaderText="Vendor ID" />
        <asp:BoundField DataField="Remarks" HeaderText="Remarks" />
        <asp:BoundField DataField="FilePath" HeaderText="File Path" />

        <asp:TemplateField HeaderText="">
            <ItemTemplate>
                <asp:Button ID="btnAttDel" runat="server"
                    Text="Delete"
                    CssClass="vm-btn vm-btn-del vm-btn-sm"
                    CommandName="DeleteAtt"
                    CommandArgument='<%# Eval("AttachID") %>'
                    OnClientClick="return confirm('Delete this attachment?');" />
            </ItemTemplate>
        </asp:TemplateField>
    </Columns>
</asp:GridView>


         </div>
     </div>
     <div style="margin-top:8px; text-align:center;">
         <asp:Button ID="btnViewDownload" runat="server" Text="View/DownLoad" CssClass="vm-btn vm-btn-update" OnClick="btnViewDownload_Click" />
     </div>
 </div>

 <!-- ===== TAB: VENDOR BRANCH MAPPING ===== -->
 <div id="tc-tab-branchmapping" class="vm-tab-content">
     <table class="vm-bm-table" style="width:100%;">
         <thead>
             <tr>
                 <th style="width:30px;"></th>
                 <th style="width:160px;">Label Name</th>
                 <th>CMP</th>
                 <th>PRC</th>
                 <th>RMC</th>
             </tr>
         </thead>
         <tbody>
             <tr>
                 <td class="vm-bm-label" style="text-align:center;">1</td>
                 <td class="vm-bm-label">Payment Terms</td>
                 <td><asp:DropDownList ID="DropDownList1" runat="server" CssClass="vm-sel" style="width:100%;"><asp:ListItem Value="0">0</asp:ListItem></asp:DropDownList></td>
                 <td><asp:DropDownList ID="DropDownList2" runat="server" CssClass="vm-sel" style="width:100%;"><asp:ListItem Value="0">0</asp:ListItem></asp:DropDownList></td>
                 <td>
                     <asp:DropDownList ID="DropDownList3" runat="server" CssClass="vm-sel" style="width:100%;">
                         <asp:ListItem Value="0">0</asp:ListItem>
                         <asp:ListItem Value="120 DAYS">120 DAYS</asp:ListItem>
                         <asp:ListItem Value="120 DAYS CDS">120 DAYS CDS</asp:ListItem>
                     </asp:DropDownList>

                 </td>
             </tr>
             <tr>
                 <td class="vm-bm-label" style="text-align:center;">2</td>
                 <td class="vm-bm-label">Tax Type</td>
                 <td><asp:DropDownList ID="DropDownList4" runat="server" CssClass="vm-sel" style="width:100%;"><asp:ListItem Value="">STANDARD RATE</asp:ListItem></asp:DropDownList></td>
                 <td><asp:DropDownList ID="DropDownList5" runat="server" CssClass="vm-sel" style="width:100%;"><asp:ListItem Value="">STANDARD RATE</asp:ListItem></asp:DropDownList></td>
                 <td>
                     <asp:DropDownList ID="DropDownList6" runat="server" CssClass="vm-sel" style="width:100%;">
                         <asp:ListItem Value="STANDARD RATE">STANDARD RATE</asp:ListItem>
                         <asp:ListItem Value="Zero rated">Zero rated</asp:ListItem>
                         <asp:ListItem Value="Intra GCC">Intra GCC</asp:ListItem>
                     </asp:DropDownList>
                 </td>
             </tr>
             <tr>
                 <td class="vm-bm-label" style="text-align:center;">3</td>
                 <td class="vm-bm-label">Credit Limit Required</td>
                 <td>
                     <asp:RadioButtonList ID="RadioButtonList1" runat="server" RepeatDirection="Horizontal" Font-Size="12px">
                         <asp:ListItem Text="Yes" Value="1" /><asp:ListItem Text="No" Value="0" Selected="True" />
                     </asp:RadioButtonList>
                 </td>
                 <td>
                     <asp:RadioButtonList ID="RadioButtonList2" runat="server" RepeatDirection="Horizontal" Font-Size="12px">
                         <asp:ListItem Text="Yes" Value="1" /><asp:ListItem Text="No" Value="0" Selected="True" />
                     </asp:RadioButtonList>
                 </td>
                 <td>
                     <asp:RadioButtonList ID="RadioButtonList3" runat="server" RepeatDirection="Horizontal" Font-Size="12px">
                         <asp:ListItem Text="Yes" Value="1" /><asp:ListItem Text="No" Value="0" Selected="True" />
                     </asp:RadioButtonList>
                 </td>
             </tr>
             <tr>
                 <td class="vm-bm-label" style="text-align:center;">4</td>
                 <td class="vm-bm-label">Credit Limit Amount</td>
                 <td><asp:TextBox ID="TextBox1" runat="server" CssClass="vm-inp" style="width:90px;text-align:right;" Text="0" /></td>
                 <td><asp:TextBox ID="TextBox2" runat="server" CssClass="vm-inp" style="width:90px;text-align:right;" Text="0" /></td>
                 <td><asp:TextBox ID="TextBox3" runat="server" CssClass="vm-inp" style="width:90px;text-align:right;" Text="0" /></td>
             </tr>
             <tr>
                 <td class="vm-bm-label" style="text-align:center;">5</td>
                 <td class="vm-bm-label">Vendor Status</td>
                 <td>
                     <asp:RadioButtonList ID="RadioButtonList4" runat="server" RepeatDirection="Horizontal" Font-Size="12px">
                         <asp:ListItem Text="Active" Value="Active" /><asp:ListItem Text="InActive" Value="InActive" Selected="True" />
                     </asp:RadioButtonList>
                 </td>
                 <td>
                     <asp:RadioButtonList ID="RadioButtonList5" runat="server" RepeatDirection="Horizontal" Font-Size="12px">
                         <asp:ListItem Text="Active" Value="Active" /><asp:ListItem Text="InActive" Value="InActive" Selected="True" />
                     </asp:RadioButtonList>
                 </td>
                 <td>
                     <asp:RadioButtonList ID="RadioButtonList6" runat="server" RepeatDirection="Horizontal" Font-Size="12px">
                         <asp:ListItem Text="Active" Value="Active" /><asp:ListItem Text="InActive" Value="InActive" Selected="True" />
                     </asp:RadioButtonList>
                 </td>
             </tr>
         </tbody>
     </table>
 </div>

    <!-- ===== TAB: VENDOR BRANCH MAPPING ===== -->
    <div id="tc-tab-branchmapping" class="vm-tab-content">
        <table class="vm-bm-table" style="width:100%;">
            <thead>
                <tr>
                    <th style="width:30px;"></th>
                    <th style="width:160px;">Label Name</th>
                    <th>CMP</th>
                    <th>PRC</th>
                    <th>RMC</th>
                </tr>
            </thead>
            <tbody>
                <tr>
                    <td class="vm-bm-label" style="text-align:center;">1</td>
                    <td class="vm-bm-label">Payment Terms</td>
                    <td><asp:DropDownList ID="ddlCMPPayTerm" runat="server" CssClass="vm-sel" style="width:100%;"><asp:ListItem Value="0">0</asp:ListItem></asp:DropDownList></td>
                    <td><asp:DropDownList ID="ddlPRCPayTerm" runat="server" CssClass="vm-sel" style="width:100%;"><asp:ListItem Value="0">0</asp:ListItem></asp:DropDownList></td>
                    <td>
                        <asp:DropDownList ID="ddlRMCPayTerm" runat="server" CssClass="vm-sel" style="width:100%;">
                            <asp:ListItem Value="0">0</asp:ListItem>
                            <asp:ListItem Value="120 DAYS">120 DAYS</asp:ListItem>
                            <asp:ListItem Value="120 DAYS CDS">120 DAYS CDS</asp:ListItem>
                        </asp:DropDownList>

                    </td>
                </tr>
                <tr>
                    <td class="vm-bm-label" style="text-align:center;">2</td>
                    <td class="vm-bm-label">Tax Type</td>
                    <td><asp:DropDownList ID="ddlCMPTaxType" runat="server" CssClass="vm-sel" style="width:100%;"><asp:ListItem Value="">STANDARD RATE</asp:ListItem></asp:DropDownList></td>
                    <td><asp:DropDownList ID="ddlPRCTaxType" runat="server" CssClass="vm-sel" style="width:100%;"><asp:ListItem Value="">STANDARD RATE</asp:ListItem></asp:DropDownList></td>
                    <td>
                        <asp:DropDownList ID="ddlRMCTaxType" runat="server" CssClass="vm-sel" style="width:100%;">
                            <asp:ListItem Value="STANDARD RATE">STANDARD RATE</asp:ListItem>
                            <asp:ListItem Value="Zero rated">Zero rated</asp:ListItem>
                            <asp:ListItem Value="Intra GCC">Intra GCC</asp:ListItem>
                        </asp:DropDownList>
                    </td>
                </tr>
                <tr>
                    <td class="vm-bm-label" style="text-align:center;">3</td>
                    <td class="vm-bm-label">Credit Limit Required</td>
                    <td>
                        <asp:RadioButtonList ID="rblCMPCreditReq" runat="server" RepeatDirection="Horizontal" Font-Size="12px">
                            <asp:ListItem Text="Yes" Value="1" /><asp:ListItem Text="No" Value="0" Selected="True" />
                        </asp:RadioButtonList>
                    </td>
                    <td>
                        <asp:RadioButtonList ID="rblPRCCreditReq" runat="server" RepeatDirection="Horizontal" Font-Size="12px">
                            <asp:ListItem Text="Yes" Value="1" /><asp:ListItem Text="No" Value="0" Selected="True" />
                        </asp:RadioButtonList>
                    </td>
                    <td>
                        <asp:RadioButtonList ID="rblRMCCreditReq" runat="server" RepeatDirection="Horizontal" Font-Size="12px">
                            <asp:ListItem Text="Yes" Value="1" /><asp:ListItem Text="No" Value="0" Selected="True" />
                        </asp:RadioButtonList>
                    </td>
                </tr>
                <tr>
                    <td class="vm-bm-label" style="text-align:center;">4</td>
                    <td class="vm-bm-label">Credit Limit Amount</td>
                    <td><asp:TextBox ID="txtCMPCreditAmt" runat="server" CssClass="vm-inp" style="width:90px;text-align:right;" Text="0" /></td>
                    <td><asp:TextBox ID="txtPRCCreditAmt" runat="server" CssClass="vm-inp" style="width:90px;text-align:right;" Text="0" /></td>
                    <td><asp:TextBox ID="txtRMCCreditAmt" runat="server" CssClass="vm-inp" style="width:90px;text-align:right;" Text="0" /></td>
                </tr>
                <tr>
                    <td class="vm-bm-label" style="text-align:center;">5</td>
                    <td class="vm-bm-label">Vendor Status</td>
                    <td>
                        <asp:RadioButtonList ID="rblCMPStatus" runat="server" RepeatDirection="Horizontal" Font-Size="12px">
                            <asp:ListItem Text="Active" Value="Active" /><asp:ListItem Text="InActive" Value="InActive" Selected="True" />
                        </asp:RadioButtonList>
                    </td>
                    <td>
                        <asp:RadioButtonList ID="rblPRCStatus" runat="server" RepeatDirection="Horizontal" Font-Size="12px">
                            <asp:ListItem Text="Active" Value="Active" /><asp:ListItem Text="InActive" Value="InActive" Selected="True" />
                        </asp:RadioButtonList>
                    </td>
                    <td>
                        <asp:RadioButtonList ID="rblRMCStatus" runat="server" RepeatDirection="Horizontal" Font-Size="12px">
                            <asp:ListItem Text="Active" Value="Active" /><asp:ListItem Text="InActive" Value="InActive" Selected="True" />
                        </asp:RadioButtonList>
                    </td>
                </tr>
            </tbody>
        </table>
    </div>

    <!-- ===== TAB: VENDOR CHECKLIST ===== -->
    <div id="tc-tab-checklist" class="vm-tab-content">
        <asp:GridView ID="gvChecklist" runat="server" AutoGenerateColumns="false"
            CssClass="vm-cl-table" DataKeyNames="CheckID">
            <Columns>
                <asp:BoundField DataField="RowNum"      HeaderText="" ItemStyle-Width="30px" ItemStyle-HorizontalAlign="Center" />
                <asp:TemplateField HeaderText="Y/N" ItemStyle-Width="40px" ItemStyle-HorizontalAlign="Center">
                    <ItemTemplate>
                        <asp:CheckBox ID="chkItem" runat="server"
                            Checked='<%# Convert.ToBoolean(Eval("IsChecked")) %>' />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="Description" HeaderText="Description" />
                <asp:TemplateField HeaderText="Remarks">
                    <ItemTemplate>
                        <asp:TextBox ID="txtCLRemarks" runat="server"
                            Text='<%# Eval("Remarks") %>'
                            CssClass="vm-inp" style="width:100%;" />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
        <div style="margin-top:8px; text-align:right;">
            <asp:Button ID="btnSaveChecklist" runat="server" Text="Save Checklist" CssClass="vm-btn vm-btn-save vm-btn-sm" OnClick="btnSaveChecklist_Click" />
        </div>
    </div>

    <!-- ===== TAB: VENDOR AUDIT ===== -->
    <div id="tc-tab-audit" class="vm-tab-content">
        <div class="vm-sub-grid-wrap">
            <asp:GridView ID="gvAudit" runat="server" AutoGenerateColumns="false" CssClass="vm-sub-grid">
                <Columns>
                    <asp:BoundField DataField="VendorCode" HeaderText="Document ID" />
                    <asp:BoundField DataField="VendorName" HeaderText="Name" />
                    <asp:BoundField DataField="CreatedDate" HeaderText="Created Date" DataFormatString="{0:dd/MM/yyyy HH:mm}" />
                </Columns>
            </asp:GridView>
        </div>
    </div>

    </div><!-- /vm-tab-body -->

    <!-- BOTTOM BAR -->
    <div class="vm-bottom-bar">
        <div class="vm-audit-field">
            <asp:TextBox ID="txtVerifiedBy" runat="server" CssClass="vm-inp" style="width:120px;" />
            <label>Verified DateTime</label>
            <asp:TextBox ID="txtVerifiedDateTime" runat="server" CssClass="vm-inp" style="width:140px;" ReadOnly="true" />
        </div>
        <div class="vm-audit-field" style="margin-left:10px;">
            <asp:TextBox ID="txtApprovedBy" runat="server" CssClass="vm-inp" style="width:120px;" />
            <label>Approved DateTime</label>
            <asp:TextBox ID="txtApprovedDateTime" runat="server" CssClass="vm-inp" style="width:140px;" ReadOnly="true" />
        </div>
        <div class="vm-bottom-btns">
            <asp:Button ID="btnClear"  runat="server" Text="Clear"  CssClass="vm-btn vm-btn-clear"  OnClick="btnClear_Click" />
            <asp:Button ID="btnSave"   runat="server" Text="&#128190; Save"   CssClass="vm-btn vm-btn-save"   OnClick="btnSave_Click" />
            <asp:Button ID="btnUpdate" runat="server" Text="&#128228; Update" CssClass="vm-btn vm-btn-update" OnClick="btnUpdate_Click" />
            <asp:Button ID="btnDelete" runat="server" Text="Delete" CssClass="vm-btn vm-btn-del" OnClick="btnDelete_Click"
                OnClientClick="return confirm('Delete this vendor?');" />
            <asp:Button ID="btnBackToList" runat="server" Text="&#8592; List" CssClass="vm-btn vm-btn-clear" OnClick="btnBackToList_Click" />
        </div>
    </div>

</div><!-- /form panel -->

<div id="vmToast"></div>

</div><!-- /vm-wrap -->
</asp:Content>

<asp:Content ID="ScriptContent" ContentPlaceHolderID="ScriptContent" runat="server">
<script type="text/javascript">
    // ---- VIEW TOGGLE ----
    function getView() {
        var hf = document.getElementById('<%= hfView.ClientID %>');
        return hf ? hf.value : 'list';
    }

    window.onload = function () {
        applyView();
        renderGrid();
        restoreTab();
    }

    function applyView() {
        var lp = document.getElementById('divListPanel');
        var fp = document.getElementById('divFormPanel');
        if (!lp || !fp) return;
        var view = getView();
        if (view === 'form') {
            lp.style.display = 'none'; fp.style.display = 'block';

        } else {
            lp.style.display = 'block'; fp.style.display = 'none';

        }
    }



    // ---- TABS ----
    function switchTab(tabId) {
        document.querySelectorAll('.vm-tab').forEach(function (t) { t.classList.remove('active'); });
        document.querySelectorAll('.vm-tab-content').forEach(function (t) { t.classList.remove('active'); });
        document.getElementById(tabId).classList.add('active');
        document.getElementById('tc-' + tabId).classList.add('active');
        document.getElementById('<%= hfCurrentTab.ClientID %>').value = tabId;
    }

    function restoreTab() {
        var savedTab = document.getElementById('<%= hfCurrentTab.ClientID %>').value || 'tab-details';
    switchTab(savedTab);
}

// ---- GRID RENDERING ----
var allRows = [];
var filteredRows = [];
var colFilters = {};
var globalFilter = '';
var pageSize = parseInt('<%= hfGridPageSz.Value %>') || 5;
var pageIdx  = parseInt('<%= hfGridPageIdx.Value %>') || 0;

function renderGrid() {
    try {
        var raw = document.getElementById('<%= hfGridData.ClientID %>').value;
        allRows = JSON.parse(raw) || [];
    } catch(e) { allRows = []; }
    applyFilters();
}

function applyFilters() {
    filteredRows = allRows.filter(function(r) {
        // global search
        if (globalFilter) {
            var combined = Object.values(r).join(' ').toLowerCase();
            if (combined.indexOf(globalFilter) === -1) return false;
        }
        // per-column filters
        var cols = ['Code','Name','Phone','VendorType','PayTerm','CreditLimit','Status','Address','','CP1Name','','','VATRegNo'];
        for (var i = 0; i < cols.length; i++) {
            if (!cols[i]) continue;
            var f = (colFilters[i] || '').toLowerCase();
            if (!f) continue;
            var val = (r[cols[i]] || '').toString().toLowerCase();
            if (val.indexOf(f) === -1) return false;
        }
        return true;
    });
    pageIdx = 0;
    renderPage();
}

function renderPage() {
    var tbody = document.getElementById('vmGridBody');
    if (!tbody) return;
    var total = filteredRows.length;
    var totalPages = Math.max(1, Math.ceil(total / pageSize));
    if (pageIdx >= totalPages) pageIdx = totalPages - 1;
    if (pageIdx < 0) pageIdx = 0;

    var start = pageIdx * pageSize;
    var end   = Math.min(start + pageSize, total);
    var html  = '';

    for (var i = start; i < end; i++) {
        var r = filteredRows[i];
        html += '<tr>';
        html += '<td><button class="vm-code-link" onclick="openVendor(' + r.Code + ')">' + escHtml(r.Code) + '</button></td>';
        html += '<td>' + escHtml(r.Name) + '</td>';
        html += '<td>' + escHtml(r.Phone) + '</td>';
        html += '<td>' + escHtml(r.VendorType) + '</td>';
        html += '<td>' + escHtml(r.PayTerm) + '</td>';
        html += '<td style="text-align:right;">' + escHtml(r.CreditLimit) + '</td>';
        html += '<td>' + escHtml(r.Status) + '</td>';
        html += '<td>' + escHtml(r.Address) + '</td>';
        html += '<td>' + escHtml(r.City) + '</td>';
        html += '<td>' + escHtml(r.CP1Name) + '</td>';
        html += '<td>' + escHtml(r.CP1Desig) + '</td>';
        html += '<td>' + escHtml(r.CP1Mobile) + '</td>';
        html += '<td>' + escHtml(r.VATRegNo) + '</td>';
        html += '</tr>';
    }

    if (!html) html = '<tr><td colspan="13" style="text-align:center;padding:20px;color:#aaa;">No records found.</td></tr>';
    tbody.innerHTML = html;

    document.getElementById('pageInfo').textContent = 'Showing ' + (total ? start+1 : 0) + ' - ' + end + ' of ' + total;
    document.getElementById('pageNumDisp').textContent = (pageIdx+1) + ' of ' + totalPages;
    document.getElementById('ddlPageSize').value = pageSize;
}

function escHtml(v) {
    if (v === null || v === undefined) return '';
    return String(v).replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;');
}

function filterCol(colIdx, val) {
    colFilters[colIdx] = val.toLowerCase();
    applyFilters();
}

function applyGlobalSearch(val) {
    globalFilter = val.toLowerCase();
    applyFilters();
}

function changePageSize(val) {
    pageSize = parseInt(val);
    pageIdx = 0;
    renderPage();
}

function goPage(dir) {
    var total = filteredRows.length;
    var totalPages = Math.max(1, Math.ceil(total / pageSize));
    if (dir === 'first') pageIdx = 0;
    else if (dir === 'prev')  pageIdx = Math.max(0, pageIdx-1);
    else if (dir === 'next')  pageIdx = Math.min(totalPages-1, pageIdx+1);
    else if (dir === 'last')  pageIdx = totalPages-1;
    renderPage();
}

function refreshGrid() {
    __doPostBack('<%= btnAddNew.UniqueID.Replace("$","$") %>', 'refresh');
    location.reload();
}

// Open a vendor in form view via postback
function openVendor(code) {
    document.getElementById('<%= hfVendorCode.ClientID %>').value = code;
    document.getElementById('<%= hfView.ClientID %>').value = 'form';
    __doPostBack('<%= btnBackToList.UniqueID %>', 'open:' + code);
    }

    // ---- VENDOR SEARCH POPUP ----
    var _vmPopupRows = [];

    function openVMSearch() {
        try {
            var raw = document.getElementById('<%= hfGridData.ClientID %>').value;
            _vmPopupRows = JSON.parse(raw) || [];
        } catch (e) { _vmPopupRows = []; }

        document.getElementById('vmPopupSearch').value = '';
        vmPopupRender(_vmPopupRows);
        var overlay = document.getElementById('vmSearchOverlay');
        overlay.style.display = 'flex';
        setTimeout(function () { document.getElementById('vmPopupSearch').focus(); }, 80);
    }

    function closeVMSearch() {
        document.getElementById('vmSearchOverlay').style.display = 'none';
    }

    function vmPopupFilter(val) {
        var q = val.toLowerCase();
        var filtered = !q ? _vmPopupRows : _vmPopupRows.filter(function (r) {
            return (r.Code + ' ' + r.Name + ' ' + r.Phone + ' ' +
                r.VendorType + ' ' + r.Status + ' ' + r.Address + ' ' + r.VATRegNo)
                .toLowerCase().indexOf(q) !== -1;
        });
        vmPopupRender(filtered);
    }

    function vmPopupRender(rows) {
        var tbody = document.getElementById('vmPopupBody');
        var html = '';
        for (var i = 0; i < rows.length; i++) {
            var r = rows[i];
            var statusStyle = (r.Status && r.Status.toLowerCase() === 'active')
                ? 'background:#e8f5e9;color:#2e7d32;'
                : 'background:#fce4ec;color:#c62828;';
            html += '<tr style="cursor:pointer;" '
                + 'onmouseover="this.style.background=\'#e3f2fd\'" '
                + 'onmouseout="this.style.background=\'\'" '
                + 'onclick="vmPopupSelect(' + r.Code + ')">'
                + '<td style="padding:6px 10px;border-bottom:1px solid #f0f0f0;white-space:nowrap;">' + escHtml(r.Code) + '</td>'
                + '<td style="padding:6px 10px;border-bottom:1px solid #f0f0f0;">' + escHtml(r.Name) + '</td>'
                + '<td style="padding:6px 10px;border-bottom:1px solid #f0f0f0;white-space:nowrap;">' + escHtml(r.Phone) + '</td>'
                + '<td style="padding:6px 10px;border-bottom:1px solid #f0f0f0;">' + escHtml(r.VendorType) + '</td>'
                + '<td style="padding:6px 10px;border-bottom:1px solid #f0f0f0;">'
                + '<span style="padding:2px 8px;border-radius:10px;font-size:11px;font-weight:600;' + statusStyle + '">'
                + escHtml(r.Status) + '</span></td>'
                + '<td style="padding:6px 10px;border-bottom:1px solid #f0f0f0;">' + escHtml(r.Address) + '</td>'
                + '<td style="padding:6px 10px;border-bottom:1px solid #f0f0f0;white-space:nowrap;">' + escHtml(r.VATRegNo) + '</td>'
                + '</tr>';
        }
        if (!html) html = '<tr><td colspan="7" style="text-align:center;padding:24px;color:#aaa;">No vendors found.</td></tr>';
        tbody.innerHTML = html;
        document.getElementById('vmPopupCount').textContent = rows.length + ' vendor(s)';
    }

    function vmPopupSelect(code) {
        // 1. Store the selected vendor code in the hidden field the server already uses
        document.getElementById('<%= hfVendorCode.ClientID %>').value = code;
    closeVMSearch();

    // 2. Flash the code input for visual feedback
    var inp = document.getElementById('<%= txtCode.ClientID %>');
    if (inp) {
        inp.value = code;
        inp.style.background = '#e8f5e9';
        setTimeout(function () { inp.style.background = ''; }, 600);
    }

    // 3. Click the dedicated hidden postback button — no __doPostBack complexity
    var btn = document.getElementById('<%= btnPopupSelect.ClientID %>');
    if (btn) btn.click();
}

// Intercept the Search button to open popup instead of posting back
window.addEventListener('load', function () {
    var btn = document.getElementById('<%= btnSearchCode.ClientID %>');
    if (!btn) return;
    btn.onclick = function (e) {
        e.preventDefault();
        e.stopPropagation();
        openVMSearch();
        return false;
    };
});

    // Close on overlay background click
    document.addEventListener('click', function (e) {
        if (e.target === document.getElementById('vmSearchOverlay')) closeVMSearch();
    });

    // Close on Escape
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') closeVMSearch();
    });

</script>
</asp:Content>
