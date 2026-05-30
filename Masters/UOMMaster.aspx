<%@ Page Title="UOM Master" Language="C#" MasterPageFile="~/Masters/MastersSite.master" AutoEventWireup="true" CodeFile="UOMMaster.aspx.cs" Inherits="Masters_UOMMaster" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="PageTitle" runat="server">UOM Master</asp:Content>
<asp:Content ID="NavTitleContent" ContentPlaceHolderID="NavTitle" runat="server">UOM Master</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">

    <!-- FORM CARD -->
    <div class="form-card">

        <!-- Code -->
        <div class="form-row">
            <span class="form-label">Code:</span>
            <asp:TextBox ID="txtCode" runat="server" CssClass="form-input form-input-sm"
                MaxLength="10" placeholder="e.g. 001" />
        </div>

        <!-- Name -->
        <div class="form-row">
            <span class="form-label">Name:</span>
            <asp:TextBox ID="txtName" runat="server" CssClass="form-input form-input-xl"
                MaxLength="100" placeholder="Unit of Measure Name" />
        </div>

        <!-- Category -->
        <div class="form-row">
            <span class="form-label">Category</span>
            <asp:DropDownList ID="ddlCategory" runat="server" CssClass="form-select">
                <asp:ListItem Value="">---- Select UOM Category ----</asp:ListItem>
                <asp:ListItem Value="NOT APPLICABLE">NOT APPLICABLE</asp:ListItem>
                <asp:ListItem Value="WEIGHT">WEIGHT</asp:ListItem>
                <asp:ListItem Value="VOLUME">VOLUME</asp:ListItem>
                <asp:ListItem Value="LENGTH">LENGTH</asp:ListItem>
                <asp:ListItem Value="COUNT">COUNT</asp:ListItem>
            </asp:DropDownList>
            <span style="font-size:12px; color:#888; margin-left:8px;">*using this category linking during invoice</span>
        </div>

        <!-- Checkboxes -->
        <div class="form-row">
            <span class="form-label">&nbsp;</span>
            <div class="checkbox-group">
                <label class="checkbox-item">
                    <asp:CheckBox ID="chkReadyMix" runat="server" />
                    ReadyMix
                </label>
                <label class="checkbox-item">
                    <asp:CheckBox ID="chkBlock" runat="server" />
                    Block
                </label>
                <label class="checkbox-item">
                    <asp:CheckBox ID="chkPrecast" runat="server" />
                    Precast
                </label>
            </div>
        </div>

        <!-- Buttons -->
        <div class="form-row">
            <span class="form-label">&nbsp;</span>
            <div class="btn-row">
                <asp:Button ID="btnSave" runat="server" Text="&#128190; Save" CssClass="btn btn-primary"
                    OnClick="btnSave_Click" />
                <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-secondary"
                    OnClick="btnCancel_Click" CausesValidation="false" />
                <span class="form-note">
                    Note * Do Not Change the Name (KILOGRAM) It's using in batching report
                </span>
            </div>
        </div>

        <!-- Hidden field for edit mode -->
        <asp:HiddenField ID="hfEditCode" runat="server" Value="" />

        <!-- Validation -->
        <asp:RequiredFieldValidator ID="rfvCode" runat="server"
            ControlToValidate="txtCode" ErrorMessage="Code is required."
            ForeColor="Red" Display="Dynamic" FontSize="11px" />
        <asp:RequiredFieldValidator ID="rfvName" runat="server"
            ControlToValidate="txtName" ErrorMessage="Name is required."
            ForeColor="Red" Display="Dynamic" FontSize="11px" />
    </div>

    <!-- DATA GRID -->
    <div class="grid-container">
        <asp:GridView ID="gvUOM" runat="server"
            CssClass="data-grid"
            AutoGenerateColumns="false"
            OnRowCommand="gvUOM_RowCommand"
            OnRowDataBound="gvUOM_RowDataBound"
            EmptyDataText="No records found."
            GridLines="None">
            <Columns>
                <asp:BoundField DataField="Code" HeaderText="Code" ItemStyle-Width="80px" />
                <asp:BoundField DataField="Name" HeaderText="Name" />
                <asp:BoundField DataField="Category" HeaderText="Category" ItemStyle-Width="150px" />
                <asp:TemplateField HeaderText="ReadyMix" ItemStyle-Width="90px" ItemStyle-HorizontalAlign="Center">
                    <ItemTemplate>
                        <asp:Label ID="lblReadyMix" runat="server"
                            Text='<%# (bool)Eval("ReadyMix") ? "Yes" : "No" %>' />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Block" ItemStyle-Width="80px" ItemStyle-HorizontalAlign="Center">
                    <ItemTemplate>
                        <asp:Label ID="lblBlock" runat="server"
                            Text='<%# (bool)Eval("BlockItem") ? "Yes" : "No" %>' />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Precast" ItemStyle-Width="80px" ItemStyle-HorizontalAlign="Center">
                    <ItemTemplate>
                        <asp:Label ID="lblPrecast" runat="server"
                            Text='<%# (bool)Eval("Precast") ? "Yes" : "No" %>' />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="" ItemStyle-Width="90px" ItemStyle-HorizontalAlign="Center">
                    <ItemTemplate>
                        <asp:LinkButton ID="lnkEdit" runat="server"
                            CommandName="EditRow"
                            CommandArgument='<%# Eval("Code") %>'
                            CssClass="link-btn"
                            CausesValidation="false">Edit</asp:LinkButton>
                        &nbsp;
                        <asp:LinkButton ID="lnkDelete" runat="server"
                            CommandName="DeleteRow"
                            CommandArgument='<%# Eval("Code") %>'
                            CssClass="link-btn delete"
                            CausesValidation="false"
                            OnClientClick="return confirm('Are you sure you want to delete this record?');">Delete</asp:LinkButton>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <EmptyDataRowStyle CssClass="text-center" ForeColor="Gray" />
        </asp:GridView>
    </div>

</asp:Content>

<asp:Content ID="ScriptContent" ContentPlaceHolderID="ScriptContent" runat="server">
    <script type="text/javascript">
        // Show save notification if panel is visible
        var savedMsg = '<%= savedMessage %>';
        if (savedMsg) {
            window.onload = function () {
                showNotification(savedMsg, 'success');
            };
        }
    </script>
</asp:Content>
