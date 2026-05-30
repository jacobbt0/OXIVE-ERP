<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true"
    CodeFile="SourceMaster.aspx.cs"
    Inherits="Masters_SourceMaster" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <style>

        body {
            font-family: 'Segoe UI';
            background-color: #f4f6f9;
        }

        .page-container {
            background: #ffffff;
            padding: 25px;
            border-radius: 12px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.08);
            margin-top: 20px;
        }

        .page-title {
            font-size: 28px;
            font-weight: 600;
            color: #0d6efd;
            margin-bottom: 25px;
            border-bottom: 2px solid #e9ecef;
            padding-bottom: 10px;
        }

        .form-table {
            width: 100%;
        }

        .form-table td {
            padding: 12px;
            vertical-align: middle;
        }

        .form-label {
            font-weight: 600;
            color: #495057;
            width: 180px;
        }

        .form-control {
            width: 280px;
            padding: 10px;
            border: 1px solid #ced4da;
            border-radius: 6px;
            font-size: 14px;
            transition: 0.3s;
        }

        .form-control:focus {
            border-color: #0d6efd;
            outline: none;
            box-shadow: 0 0 5px rgba(13,110,253,0.3);
        }

        .radio-list {
            margin-top: 5px;
        }

        .button-section {
            margin-top: 15px;
        }

        .btn {
            border: none;
            padding: 10px 18px;
            border-radius: 6px;
            color: white;
            font-size: 14px;
            cursor: pointer;
            margin-right: 8px;
            transition: 0.3s;
        }

        .btn:hover {
            opacity: 0.9;
        }

        .btn-save {
            background-color: #198754;
        }

        .btn-update {
            background-color: #0d6efd;
        }

        .btn-delete {
            background-color: #dc3545;
        }

        .btn-clear {
            background-color: #6c757d;
        }

        .btn-edit {
            background-color: #fd7e14;
            padding: 6px 14px;
            font-size: 12px;
            border-radius: 5px;
            color: white;
            border: none;
            cursor: pointer;
        }

        .search-box {
            width: 300px;
            padding: 10px;
            border: 1px solid #ced4da;
            border-radius: 6px;
            margin-top: 15px;
        }

        .table {
            width: 100%;
            border-collapse: collapse;
            margin-top: 20px;
            background: white;
        }

        .table th {
            background-color: #0d6efd;
            color: white;
            padding: 12px;
            text-align: left;
            font-size: 14px;
        }

        .table td {
            padding: 12px;
            border-bottom: 1px solid #dee2e6;
            font-size: 14px;
        }

        .table tr:nth-child(even) {
            background-color: #f8f9fa;
        }

        .table tr:hover {
            background-color: #eef4ff;
        }

    </style>

    <div class="page-container">

        <div class="page-title">
            Source Master
        </div>

        <table class="form-table">

            <tr>
                <td class="form-label">
                    Source Code
                </td>

                <td>
                    <asp:TextBox ID="txtCode"
                        runat="server"
                        CssClass="form-control"
                        Enabled="false">
                    </asp:TextBox>
                </td>
            </tr>

            <tr>
                <td class="form-label">
                    Source Description
                </td>

                <td>
                    <asp:TextBox ID="txtDescription"
                        runat="server"
                        CssClass="form-control">
                    </asp:TextBox>
                </td>
            </tr>

            <tr>
                <td class="form-label">
                    Station
                </td>

                <td>

                    <asp:DropDownList ID="ddlStation"
                        runat="server"
                        CssClass="form-control">

                        <asp:ListItem Value="">
                            -- Select Station --
                        </asp:ListItem>

                        <asp:ListItem Text="CMP Station"
                            Value="CMP Station">
                        </asp:ListItem>

                        <asp:ListItem Text="PRC Station"
                            Value="PRC Station">
                        </asp:ListItem>

                        <asp:ListItem Text="RMC Station"
                            Value="RMC Station">
                        </asp:ListItem>

                    </asp:DropDownList>

                </td>
            </tr>

            <tr>
                <td class="form-label">
                    Wastage Trip Location
                </td>

                <td>

                    <asp:RadioButtonList ID="rblWastage"
                        runat="server"
                        RepeatDirection="Horizontal"
                        CssClass="radio-list">

                        <asp:ListItem Text="Yes"
                            Value="1">
                        </asp:ListItem>

                        <asp:ListItem Text="No"
                            Value="0"
                            Selected="True">
                        </asp:ListItem>

                    </asp:RadioButtonList>

                </td>
            </tr>

            <tr>
                <td class="form-label">
                    Is Active
                </td>

                <td>

                    <asp:RadioButtonList ID="rblActive"
                        runat="server"
                        RepeatDirection="Horizontal"
                        CssClass="radio-list">

                        <asp:ListItem Text="Yes"
                            Value="1"
                            Selected="True">
                        </asp:ListItem>

                        <asp:ListItem Text="No"
                            Value="0">
                        </asp:ListItem>

                    </asp:RadioButtonList>

                </td>
            </tr>

            <tr>
                <td colspan="2" class="button-section">

                    <asp:Button ID="btnSave"
                        runat="server"
                        Text="Save"
                        CssClass="btn btn-save"
                        OnClick="btnSave_Click" />

                    <asp:Button ID="btnUpdate"
                        runat="server"
                        Text="Update"
                        CssClass="btn btn-update"
                        OnClick="btnUpdate_Click" />

                    <asp:Button ID="btnDelete"
                        runat="server"
                        Text="Delete"
                        CssClass="btn btn-delete"
                        OnClick="btnDelete_Click" />

                    <asp:Button ID="btnClear"
                        runat="server"
                        Text="Clear"
                        CssClass="btn btn-clear"
                        OnClick="btnClear_Click" />

                </td>
            </tr>

        </table>

        <asp:TextBox ID="txtSearch"
            runat="server"
            CssClass="search-box"
            AutoPostBack="true"
            Placeholder="Search..."
            OnTextChanged="txtSearch_TextChanged">
        </asp:TextBox>

        <asp:GridView ID="gvSource"
            runat="server"
            AutoGenerateColumns="False"
            CssClass="table"
            DataKeyNames="SourceCode"
            OnRowCommand="gvSource_RowCommand">

            <Columns>

                <asp:TemplateField HeaderText="Action">

                    <ItemTemplate>

                        <asp:Button ID="btnEdit"
                            runat="server"
                            Text="Edit"
                            CssClass="btn-edit"
                            CommandName="EditRow"
                            CommandArgument='<%# Eval("SourceCode") %>' />

                    </ItemTemplate>

                </asp:TemplateField>

                <asp:BoundField
                    DataField="SourceCode"
                    HeaderText="Code" />

                <asp:BoundField
                    DataField="SourceDescription"
                    HeaderText="Description" />

                <asp:BoundField
                    DataField="StationID"
                    HeaderText="Station" />

                <asp:BoundField
                    DataField="WastageTripLocation"
                    HeaderText="Wastage Trip" />

                <asp:BoundField
                    DataField="IsActive"
                    HeaderText="Active" />

            </Columns>

        </asp:GridView>

    </div>

</asp:Content>
