<%@ Page Title="Weight Bridge Party Master" Language="C#" AutoEventWireup="true"
    CodeFile="WeightBridgePartyMaster.aspx.cs"
    Inherits="Masters_WeightBridgePartyMaster"
    MasterPageFile="~/Masters/MastersSite.master"%>


<asp:Content ID="Content1"
    ContentPlaceHolderID="MainContent"
    runat="server">

    <style>

        .page-container {
            padding: 10px;
            font-family: Segoe UI;
        }

        .page-title {
            background: #2f75b5;
            color: white;
            padding: 8px;
            font-size: 30px;
            font-weight: 600;
            text-align: center;
            margin-bottom: 10px;
        }

        .main-box {
            border: 1px solid #cfcfcf;
            background: #f9f9f9;
            padding: 10px;
        }

        .row {
            display: flex;
            align-items: center;
            margin-bottom: 10px;
        }

        .label {
            width: 140px;
            font-size: 14px;
        }

        .textbox {
            width: 300px;
            height: 30px;
            border: 1px solid #bdbdbd;
            border-radius: 4px;
            padding-left: 5px;
        }

        .textbox-large {
            width: 600px;
        }

        .search-icon {
            font-size: 24px;
            color: #0078d7;
            text-decoration: none;
        }

        .status-section {
            margin-left: 40px;
        }

        .fieldset {
            border: 1px solid #bdbdbd;
            margin-top: 10px;
            padding: 10px;
        }

        .fieldset legend {
            padding: 0 5px;
            font-weight: bold;
        }

        .contact-wrapper {
            display: flex;
            gap: 10px;
        }

        .left-side {
            width: 48%;
        }

        .right-side {
            width: 52%;
        }

        .remarks-box {
            width: 95%;
            height: 80px;
            resize: none;
        }

        .button-panel {
            text-align: center;
            margin-top: 20px;
        }

        .btn {
            background: #0078d7;
            color: white;
            border: none;
            padding: 10px 25px;
            border-radius: 5px;
            cursor: pointer;
            margin: 0 5px;
            font-size: 14px;
        }

        .btn:hover {
            background: #005ea6;
        }

        .btn-delete {
            border: 2px solid red;
        }

        .grid {
            width: 100%;
            border-collapse: collapse;
        }

        .grid th {
            background: #2f75b5;
            color: white;
            padding: 10px;
        }

        .grid td {
            border: 1px solid #ddd;
            padding: 8px;
        }

        .popup-overlay {
            position: fixed;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: rgba(0,0,0,0.5);
            z-index: 9999;
        }

        .popup-box {
            width: 700px;
            background: white;
            margin: 80px auto;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0px 0px 15px #000;
        }

        .popup-header {
            background: #2f75b5;
            color: white;
            padding: 12px;
            font-size: 18px;
            font-weight: bold;
        }

        .popup-body {
            padding: 15px;
            max-height: 400px;
            overflow-y: auto;
        }

        .popup-footer {
            padding: 10px;
            text-align: center;
            border-top: 1px solid #ccc;
        }

    </style>

    <div class="page-container">

        <div class="page-title">
            Weight Bridge Party Master
        </div>

        <div class="main-box">

            <div class="row">

                <div class="label">
                    Search :
                </div>

                <asp:LinkButton ID="btnSearchPopup"
                    runat="server"
                    CssClass="search-icon"
                    OnClick="btnSearchPopup_Click">
                    🔍
                </asp:LinkButton>

            </div>

            <div class="row">

                <div class="label">
                    Code :
                </div>

                <asp:TextBox ID="txtCode"
                    runat="server"
                    CssClass="textbox" 
                    ReadOnly="true" >
                </asp:TextBox>

                <div class="status-section">

                    Status :

                    <asp:RadioButton ID="rbActive"
                        runat="server"
                        GroupName="Status"
                        Text="Active"
                        Checked="true" />

                    <asp:RadioButton ID="rbInactive"
                        runat="server"
                        GroupName="Status"
                        Text="Inactive" />

                </div>

            </div>

            <div class="row">

                <div class="label">
                    Name :
                </div>

                <asp:TextBox ID="txtName"
                    runat="server"
                    CssClass="textbox textbox-large">
                </asp:TextBox>

            </div>

            <div class="row">

                <div class="label">
                    Full Name (LL) :
                </div>

                <asp:TextBox ID="txtFullName"
                    runat="server"
                    CssClass="textbox textbox-large">
                </asp:TextBox>

            </div>

            <fieldset class="fieldset">

                <legend>Contact Info.</legend>

                <div class="contact-wrapper">

                    <div class="left-side">

                        <div class="row">

                            <div class="label">
                                P.O Box :
                            </div>

                            <asp:TextBox ID="txtPOBox"
                                runat="server"
                                CssClass="textbox">
                            </asp:TextBox>

                        </div>

                        <div class="row">

                            <div class="label">
                                Address :
                            </div>

                            <asp:TextBox ID="txtAddress"
                                runat="server"
                                CssClass="textbox">
                            </asp:TextBox>

                        </div>

                        <div class="row">

                            <div class="label">
                                City :
                            </div>

                            <asp:TextBox ID="txtCity"
                                runat="server"
                                CssClass="textbox">
                            </asp:TextBox>

                        </div>

                        <div class="row">

                            <div class="label">
                                Telephone :
                            </div>

                            <asp:TextBox ID="txtPhone"
                                runat="server"
                                CssClass="textbox">
                            </asp:TextBox>

                        </div>

                        <div class="row">

                            <div class="label">
                                Fax No :
                            </div>

                            <asp:TextBox ID="txtFax"
                                runat="server"
                                CssClass="textbox">
                            </asp:TextBox>

                        </div>

                        <div class="row">

                            <div class="label">
                                Email :
                            </div>

                            <asp:TextBox ID="txtEmail"
                                runat="server"
                                CssClass="textbox">
                            </asp:TextBox>

                        </div>

                        <div class="row">

                            <div class="label">
                                Web Site :
                            </div>

                            <asp:TextBox ID="txtWebsite"
                                runat="server"
                                CssClass="textbox">
                            </asp:TextBox>

                        </div>

                    </div>

                    <div class="right-side">

                        <fieldset class="fieldset">

                            <legend>Contact Person Details</legend>

                            <div class="row">

                                <div class="label">
                                    Name :
                                </div>

                                <asp:TextBox ID="txtContactPerson"
                                    runat="server"
                                    CssClass="textbox">
                                </asp:TextBox>

                            </div>

                            <div class="row">

                                <div class="label">
                                    Designation :
                                </div>

                                <asp:TextBox ID="txtDesignation"
                                    runat="server"
                                    CssClass="textbox">
                                </asp:TextBox>

                            </div>

                            <div class="row">

                                <div class="label">
                                    Mobile No :
                                </div>

                                <asp:TextBox ID="txtMobile"
                                    runat="server"
                                    CssClass="textbox">
                                </asp:TextBox>

                            </div>

                        </fieldset>

                        <div class="row" style="margin-top:20px;">

                            <div class="label">
                                Remarks :
                            </div>

                            <asp:TextBox ID="txtRemarks"
                                runat="server"
                                TextMode="MultiLine"
                                CssClass="remarks-box">
                            </asp:TextBox>

                        </div>

                    </div>

                </div>

            </fieldset>

            <div class="button-panel">

                <asp:Button ID="btnClear"
                    runat="server"
                    Text="Clear"
                    CssClass="btn"
                    OnClick="btnClear_Click" />

                <asp:Button ID="btnSave"
                    runat="server"
                    Text="Save"
                    CssClass="btn"
                    OnClick="btnSave_Click" />

                <asp:Button ID="btnUpdate"
                    runat="server"
                    Text="Update"
                    CssClass="btn"
                    OnClick="btnUpdate_Click" />

                <asp:Button ID="btnDelete"
                    runat="server"
                    Text="Delete"
                    CssClass="btn btn-delete"
                    OnClick="btnDelete_Click" />

            </div>

        </div>

    </div>

    <!-- SEARCH POPUP -->

    <asp:Panel ID="pnlPopup"
        runat="server"
        CssClass="popup-overlay"
        Style="display:none;">

        <div class="popup-box">

            <div class="popup-header">
                Search Weight Bridge Party
            </div>

            <div class="popup-body">

                <asp:GridView ID="gvSearch"
                    runat="server"
                    AutoGenerateColumns="False"
                    CssClass="grid"
                    OnSelectedIndexChanged="gvSearch_SelectedIndexChanged">

                    <Columns>

                        <asp:BoundField DataField="Code" HeaderText="Code" />
                        <asp:BoundField DataField="Name" HeaderText="Name" />
                        <asp:BoundField DataField="Status" HeaderText="Status" />

                        <asp:CommandField ShowSelectButton="True"
                            SelectText="Select" />

                    </Columns>

                </asp:GridView>

            </div>

            <div class="popup-footer">

                <asp:Button ID="btnClosePopup"
                    runat="server"
                    Text="Close"
                    CssClass="btn"
                    OnClientClick="hidePopup(); return false;" />

            </div>

        </div>

    </asp:Panel>

    <script>

        function showPopup() {

            document.getElementById('<%= pnlPopup.ClientID %>').style.display = 'block';

        }

        function hidePopup() {

            document.getElementById('<%= pnlPopup.ClientID %>').style.display = 'none';

        }

    </script>

</asp:Content>
