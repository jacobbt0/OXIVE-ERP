<%@ Page Title="Dashboard" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="PageTitle" runat="server">Dashboard</asp:Content>
<asp:Content ID="NavTitleContent" ContentPlaceHolderID="NavTitle" runat="server">Dashboard</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="form-card">
        <div class="section-header">&#127968; Welcome to OXIVEERP</div>
        <p style="color:#4a5568; font-size:14px; margin-top:10px;">
            Welcome to OXIVE ERP System. Please use the left menu to navigate to the required module.
        </p>

        <div style="display:flex; gap:16px; margin-top:20px; flex-wrap:wrap;">
            <div style="background:#edf4ff; border:1px solid #b3d1f7; border-radius:6px; padding:16px 24px; min-width:160px; text-align:center;">
                <div style="font-size:28px; margin-bottom:6px;">&#128230;</div>
                <div style="font-weight:700; color:#1e4d7b; font-size:14px;">Raw Material</div>
                <div style="font-size:12px; color:#6c757d; margin-top:4px;">Masters &amp; Entries</div>
            </div>
            <div style="background:#edf7f0; border:1px solid #a8d9b5; border-radius:6px; padding:16px 24px; min-width:160px; text-align:center;">
                <div style="font-size:28px; margin-bottom:6px;">&#9881;</div>
                <div style="font-weight:700; color:#1e6b3a; font-size:14px;">Production</div>
                <div style="font-size:12px; color:#6c757d; margin-top:4px;">Batching &amp; Mix</div>
            </div>
            <div style="background:#fff7ed; border:1px solid #f5c999; border-radius:6px; padding:16px 24px; min-width:160px; text-align:center;">
                <div style="font-size:28px; margin-bottom:6px;">&#128200;</div>
                <div style="font-weight:700; color:#b45309; font-size:14px;">Accounts</div>
                <div style="font-size:12px; color:#6c757d; margin-top:4px;">Ledger &amp; Vouchers</div>
            </div>
        </div>

        <div style="margin-top:20px; font-size:12px; color:#888;">
            <strong>Financial Year:</strong> 01-Jan-2026 To 31-Dec-2026 &nbsp;|&nbsp;
            <strong>User:</strong> STUDY
        </div>
    </div>
</asp:Content>
