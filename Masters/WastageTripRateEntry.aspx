<%@ Page Title="Coming Soon" Language="C#" MasterPageFile="~/Masters/MastersSite.master" AutoEventWireup="true" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="PageTitle" runat="server">Coming Soon</asp:Content>
<asp:Content ID="NavTitleContent" ContentPlaceHolderID="NavTitle" runat="server">
    <asp:Literal ID="litTitle" runat="server">Module Under Construction</asp:Literal>
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="form-card" style="text-align:center; padding:40px;">
        <div style="font-size:48px; margin-bottom:16px;">&#9881;</div>
        <div style="font-size:20px; font-weight:700; color:#1e4d7b; margin-bottom:8px;">Module Under Construction</div>
        <div style="font-size:14px; color:#888;">This module is being developed and will be available soon.</div>
        <div style="margin-top:20px;">
            <a href="../Default.aspx" class="btn btn-primary" style="text-decoration:none;">&#8592; Back to Dashboard</a>
        </div>
    </div>
</asp:Content>
