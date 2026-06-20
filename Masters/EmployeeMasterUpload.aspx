<%@ Page Title="Employee Master Upload" Language="C#" 
    MasterPageFile="~/Masters/MastersSite.master" 
    AutoEventWireup="true" 
    CodeFile="EmployeeMasterUpload.aspx.cs" 
    Inherits="Masters_EmployeeMasterUpload" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="PageTitle" runat="server">
    Employee Master Upload Utility
</asp:Content>

<asp:Content ID="NavTitleContent" ContentPlaceHolderID="NavTitle" runat="server">
    Employee Master Upload
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="form-card">

        <div class="btn-row" style="margin-bottom: 15px;">
            <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-secondary"
                OnClick="btnClear_Click" CausesValidation="false" />
            <asp:Button ID="btnSave" runat="server" Text="💾 Save to Database" CssClass="btn btn-primary"
                OnClick="btnSave_Click" Enabled="false" />
        </div>

        <div class="form-row">
            <span class="form-label">Select Excel File:</span>
            <asp:FileUpload ID="fuExcel" runat="server" CssClass="form-input" accept=".xlsx, .xls" />
            <asp:Button ID="btnUpload" runat="server" Text="Upload & Preview" 
                CssClass="btn btn-primary" OnClick="btnUpload_Click" 
                style="margin-left: 10px;" />
        </div>

        <asp:Label ID="lblMessage" runat="server" ForeColor="Red" Font-Size="12px" />

        <hr class="divider" />

        <div class="grid-container" style="overflow-x: auto;">
            <asp:GridView ID="gvPreview" runat="server" CssClass="data-grid"
                AutoGenerateColumns="true" EmptyDataText="No data loaded."
                GridLines="None" AllowPaging="true" PageSize="10"
                OnPageIndexChanging="gvPreview_PageIndexChanging">
                <EmptyDataRowStyle CssClass="text-center" ForeColor="Gray" />
            </asp:GridView>
        </div>

        <asp:HiddenField ID="hfPreviewData" runat="server" />
    </div>

</asp:Content>

<asp:Content ID="ScriptContent" ContentPlaceHolderID="ScriptContent" runat="server">
    <script type="text/javascript">
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
    </script>
</asp:Content>