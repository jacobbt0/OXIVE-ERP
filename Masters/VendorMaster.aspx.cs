using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;

public partial class Masters_VendorMaster : System.Web.UI.Page
{
    string ConStr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

    // =============================================
    // PAGE LOAD
    // =============================================
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            hfView.Value = "list";
            BindGridData();
        }
        else
        {
            // Restore view after postback
            string ev = Request.Form["__EVENTTARGET"] ?? "";
            string ea = Request.Form["__EVENTARGUMENT"] ?? "";

            if (ea.StartsWith("open:"))
            {
                string code = ea.Replace("open:", "");
                int vendorCode;
                if (int.TryParse(code, out vendorCode))
                {
                    hfView.Value = "form";
                    LoadVendor(vendorCode);
                    BindSubTabs(vendorCode);
                    ScriptManager.RegisterStartupScript(this, GetType(), "view",
                        "currentView='form'; applyView(); restoreTab();", true);
                }
            }
            else
            {
                // Re-bind grid data on every postback so JS can re-render
                BindGridData();
                if (hfView.Value == "form")
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "view",
                        "currentView='form'; applyView(); restoreTab();", true);
                }
                else
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "view",
                        "currentView='list'; applyView(); renderGrid();", true);
                }
            }
        }
    }

    // =============================================
    // GRID DATA — serialized as JSON for JS rendering
    // =============================================
    private void BindGridData()
    {
        try
        {
            using (SqlConnection con = new SqlConnection(ConStr))
            {
                string q = @"
                    SELECT
                        vm.VendorCode   AS Code,
                        vm.VendorName   AS Name,
                        ISNULL(vm.TelephoneNo,'')   AS Phone,
                        ISNULL(vm.VendorType,'')    AS VendorType,
                        ISNULL(
                            (SELECT TOP 1 PaymentTerms FROM Vendor_Branch_Mapping
                             WHERE VendorCode=vm.VendorCode AND Branch='CMP'),'') AS PayTerm,
                        ISNULL(
                            (SELECT TOP 1 CAST(CreditLimitAmount AS NVARCHAR) FROM Vendor_Branch_Mapping
                             WHERE VendorCode=vm.VendorCode AND Branch='CMP'),'0') AS CreditLimit,
                        ISNULL(
                            (SELECT TOP 1 VendorStatus FROM Vendor_Branch_Mapping
                             WHERE VendorCode=vm.VendorCode AND Branch='CMP'),'') AS Status,
                        ISNULL(vm.Address,'')       AS Address,
                        ISNULL(vm.City,'')          AS City,
                        ISNULL(vm.CP1Name,'')       AS CP1Name,
                        ISNULL(vm.CP1Desig,'')      AS CP1Desig,
                        ISNULL(vm.CP1Mobile,'')     AS CP1Mobile,
                        ISNULL(vm.VATRegNo,'')      AS VATRegNo
                    FROM Vendor_Master vm
                    WHERE vm.IsDeleted = 0
                    ORDER BY vm.VendorCode";

                SqlDataAdapter da = new SqlDataAdapter(q, con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                hfGridData.Value = JsonConvert.SerializeObject(dt);
            }
        }
        catch (Exception ex)
        {
            hfGridData.Value = "[]";
            ShowMsg(ex.Message, "error");
        }
    }

    // =============================================
    // ADD NEW
    // =============================================
    protected void btnAddNew_Click(object sender, EventArgs e)
    {
        ClearForm();
        hfView.Value = "form";
        hfVendorCode.Value = "0";
        LoadChecklistTemplate(0);
        ScriptManager.RegisterStartupScript(this, GetType(), "view",
            "currentView='form'; applyView(); switchTab('tab-details');", true);
    }

    // =============================================
    // BACK TO LIST
    // =============================================
    protected void btnBackToList_Click(object sender, EventArgs e)
    {
        string ea = Request.Form["__EVENTARGUMENT"] ?? "";
        if (ea.StartsWith("open:"))
        {
            // handled in Page_Load
            return;
        }
        hfView.Value = "list";
        ClearForm();
        BindGridData();
        ScriptManager.RegisterStartupScript(this, GetType(), "view",
            "currentView='list'; applyView(); renderGrid();", true);
    }

    // =============================================
    // RADIO FILTER CHANGED
    // =============================================
    protected void rbFilter_Changed(object sender, EventArgs e)
    {
        BindGridData();
        ScriptManager.RegisterStartupScript(this, GetType(), "view",
            "currentView='list'; applyView(); renderGrid();", true);
    }

    // =============================================
    // SEARCH CODE BUTTON
    // =============================================
    protected void btnSearchCode_Click(object sender, EventArgs e)
    {
        string code = txtCode.Text.Trim();
        if (!string.IsNullOrEmpty(code))
        {
            int vc;
            if (int.TryParse(code, out vc))
            {
                LoadVendor(vc);
                BindSubTabs(vc);
                hfView.Value = "form";
                ScriptManager.RegisterStartupScript(this, GetType(), "view",
                    "currentView='form'; applyView(); switchTab('tab-details');", true);
            }
        }
    }

    // =============================================
    // SAVE
    // =============================================
    protected void btnSave_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            ShowMsg("Vendor Name is required.", "error"); return;
        }
        try
        {
            using (SqlConnection con = new SqlConnection(ConStr))
            {
                con.Open();

                // Duplicate check
                string chk = "SELECT COUNT(*) FROM Vendor_Master WHERE VendorName=@n AND IsDeleted=0";
                using (SqlCommand c = new SqlCommand(chk, con))
                {
                    c.Parameters.AddWithValue("@n", txtName.Text.Trim());
                    if (Convert.ToInt32(c.ExecuteScalar()) > 0)
                    {
                        ShowMsg("Vendor Name already exists.", "error"); return;
                    }
                }

                string ins = @"INSERT INTO Vendor_Master
                    (VendorName,FullNameLL,PrintInCheque,AccountGroup,InvoicingQty,Branch,VendorType,
                     SupplierOriginCountryCode,SupplierOriginCountry,StateID,POBox,Address,City,
                     TelephoneNo,FaxNo,Email,WebSite,VendorRemarks,EvalRemarks,
                     CP1Name,CP1Desig,CP1Mobile,CP1Email,CP2Name,CP2Desig,CP2Mobile,CP2Email,
                     BusinessType,TradeLicenseNo,EvaluationCategory,LicenseExpDate,EvalExpDate,VATRegNo,TRNDate)
                    OUTPUT INSERTED.VendorCode
                    VALUES
                    (@n,@fl,@pc,@ag,@iq,@br,@vt,
                     @orc,@or,@st,@pb,@ad,@ci,
                     @te,@fx,@em,@ws,@vr,@er,
                     @c1n,@c1d,@c1m,@c1e,@c2n,@c2d,@c2m,@c2e,
                     @bt,@tl,@ec,@ld,@ed,@vat,@trd)";

                int newCode;
                using (SqlCommand cmd = new SqlCommand(ins, con))
                {
                    AddAllParams(cmd);
                    newCode = Convert.ToInt32(cmd.ExecuteScalar());
                }

                hfVendorCode.Value = newCode.ToString();
                txtCode.Text = newCode.ToString();

                // Save branch mapping
                SaveBranchMapping(con, newCode);

                // Init checklist
                InitChecklist(con, newCode);
            }
            ShowMsg("Vendor saved successfully. Code: " + hfVendorCode.Value, "success");
            BindGridData();
            int vc2; int.TryParse(hfVendorCode.Value, out vc2);
            BindSubTabs(vc2);
        }
        catch (Exception ex) { ShowMsg(ex.Message, "error"); }
    }

    // =============================================
    // UPDATE
    // =============================================
    protected void btnUpdate_Click(object sender, EventArgs e)
    {
        int vendorCode;
        if (!int.TryParse(hfVendorCode.Value, out vendorCode) || vendorCode == 0)
        {
            ShowMsg("No vendor selected.", "error"); return;
        }
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            ShowMsg("Vendor Name is required.", "error"); return;
        }
        try
        {
            using (SqlConnection con = new SqlConnection(ConStr))
            {
                con.Open();

                // Duplicate check (exclude self)
                string chk = "SELECT COUNT(*) FROM Vendor_Master WHERE VendorName=@n AND VendorCode<>@c AND IsDeleted=0";
                using (SqlCommand c = new SqlCommand(chk, con))
                {
                    c.Parameters.AddWithValue("@n", txtName.Text.Trim());
                    c.Parameters.AddWithValue("@c", vendorCode);
                    if (Convert.ToInt32(c.ExecuteScalar()) > 0)
                    {
                        ShowMsg("Vendor Name already exists.", "error"); return;
                    }
                }

                string upd = @"UPDATE Vendor_Master SET
                    VendorName=@n, FullNameLL=@fl, PrintInCheque=@pc, AccountGroup=@ag,
                    InvoicingQty=@iq, Branch=@br, VendorType=@vt,
                    SupplierOriginCountryCode=@orc, SupplierOriginCountry=@or,
                    StateID=@st, POBox=@pb, Address=@ad, City=@ci,
                    TelephoneNo=@te, FaxNo=@fx, Email=@em, WebSite=@ws,
                    VendorRemarks=@vr, EvalRemarks=@er,
                    CP1Name=@c1n, CP1Desig=@c1d, CP1Mobile=@c1m, CP1Email=@c1e,
                    CP2Name=@c2n, CP2Desig=@c2d, CP2Mobile=@c2m, CP2Email=@c2e,
                    BusinessType=@bt, TradeLicenseNo=@tl, EvaluationCategory=@ec,
                    LicenseExpDate=@ld, EvalExpDate=@ed, VATRegNo=@vat, TRNDate=@trd
                    WHERE VendorCode=@vc";

                using (SqlCommand cmd = new SqlCommand(upd, con))
                {
                    AddAllParams(cmd);
                    cmd.Parameters.AddWithValue("@vc", vendorCode);
                    cmd.ExecuteNonQuery();
                }

                // Delete existing branch mapping and re-save
                using (SqlCommand del = new SqlCommand(
                    "DELETE FROM Vendor_Branch_Mapping WHERE VendorCode=@vc", con))
                {
                    del.Parameters.AddWithValue("@vc", vendorCode);
                    del.ExecuteNonQuery();
                }
                SaveBranchMapping(con, vendorCode);
            }
            ShowMsg("Vendor updated successfully.", "success");
            BindGridData();
            BindSubTabs(vendorCode);
        }
        catch (Exception ex) { ShowMsg(ex.Message, "error"); }
    }

    // =============================================
    // DELETE
    // =============================================
    protected void btnDelete_Click(object sender, EventArgs e)
    {
        int vendorCode;
        if (!int.TryParse(hfVendorCode.Value, out vendorCode) || vendorCode == 0)
        {
            ShowMsg("No vendor selected.", "error"); return;
        }
        try
        {
            using (SqlConnection con = new SqlConnection(ConStr))
            {
                string q = "UPDATE Vendor_Master SET IsDeleted=1 WHERE VendorCode=@vc";
                using (SqlCommand cmd = new SqlCommand(q, con))
                {
                    cmd.Parameters.AddWithValue("@vc", vendorCode);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            ShowMsg("Vendor deleted.", "success");
            ClearForm();
            hfView.Value = "list";
            BindGridData();
            ScriptManager.RegisterStartupScript(this, GetType(), "view",
                "currentView='list'; applyView(); renderGrid();", true);
        }
        catch (Exception ex) { ShowMsg(ex.Message, "error"); }
    }

    // =============================================
    // CLEAR
    // =============================================
    protected void btnClear_Click(object sender, EventArgs e)
    {
        ClearForm();
        hfVendorCode.Value = "0";
        ScriptManager.RegisterStartupScript(this, GetType(), "view",
            "switchTab('tab-details');", true);
    }

    // =============================================
    // PRINT
    // =============================================
    protected void btnPrint_Click(object sender, EventArgs e)
    {
        ScriptManager.RegisterStartupScript(this, GetType(), "prnt", "window.print();", true);
    }

    // =============================================
    // EXPORT EXCEL
    // =============================================
    protected void btnExportExcel_Click(object sender, EventArgs e)
    {
        try
        {
            using (SqlConnection con = new SqlConnection(ConStr))
            {
                string q = @"SELECT VendorCode AS Code, VendorName AS Name,
                    TelephoneNo AS Phone, VendorType, Address, City, Email, VATRegNo
                    FROM Vendor_Master WHERE IsDeleted=0 ORDER BY VendorCode";
                SqlDataAdapter da = new SqlDataAdapter(q, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                Response.Clear();
                Response.ContentType = "application/vnd.ms-excel";
                Response.AddHeader("Content-Disposition", "attachment;filename=VendorMaster.xls");
                Response.Charset = "UTF-8";

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("<table border='1'>");
                sb.Append("<tr>");
                foreach (DataColumn col in dt.Columns)
                    sb.Append("<th>" + col.ColumnName + "</th>");
                sb.AppendLine("</tr>");
                foreach (DataRow row in dt.Rows)
                {
                    sb.Append("<tr>");
                    foreach (DataColumn col in dt.Columns)
                        sb.Append("<td>" + row[col] + "</td>");
                    sb.AppendLine("</tr>");
                }
                sb.AppendLine("</table>");
                Response.Write(sb.ToString());
                Response.End();
            }
        }
        catch (Exception ex) { ShowMsg(ex.Message, "error"); }
    }

    // =============================================
    // NAME HISTORY
    // =============================================
    protected void btnAddHistory_Click(object sender, EventArgs e)
    {
        int vc; if (!int.TryParse(hfVendorCode.Value, out vc) || vc == 0)
        { ShowMsg("Save vendor first.", "error"); return; }

        int slno; int.TryParse(hfNHSLNO.Value, out slno);

        try
        {
            using (SqlConnection con = new SqlConnection(ConStr))
            {
                con.Open();
                if (slno > 0) // edit existing
                {
                    string upd = @"UPDATE Vendor_Name_History SET
                        HistoryName=@hn, CHQHistoryName=@cn, ReplaceDate=@rd, HistoryFullName=@hf
                        WHERE SLNO=@sl AND VendorCode=@vc";
                    using (SqlCommand cmd = new SqlCommand(upd, con))
                    {
                        cmd.Parameters.AddWithValue("@hn", txtHistoryName.Text.Trim());
                        cmd.Parameters.AddWithValue("@cn", txtCHQHistoryName.Text.Trim());
                        cmd.Parameters.AddWithValue("@rd", ParseDate(txtReplaceDate.Text));
                        cmd.Parameters.AddWithValue("@hf", txtHistoryName.Text.Trim());
                        cmd.Parameters.AddWithValue("@sl", slno);
                        cmd.Parameters.AddWithValue("@vc", vc);
                        cmd.ExecuteNonQuery();
                    }
                    hfNHSLNO.Value = "0";
                }
                else
                {
                    string ins = @"INSERT INTO Vendor_Name_History(VendorCode,HistoryName,HistoryFullName,CHQHistoryName,ReplaceDate)
                        VALUES(@vc,@hn,@hf,@cn,@rd)";
                    using (SqlCommand cmd = new SqlCommand(ins, con))
                    {
                        cmd.Parameters.AddWithValue("@vc", vc);
                        cmd.Parameters.AddWithValue("@hn", txtHistoryName.Text.Trim());
                        cmd.Parameters.AddWithValue("@hf", txtHistoryName.Text.Trim());
                        cmd.Parameters.AddWithValue("@cn", txtCHQHistoryName.Text.Trim());
                        cmd.Parameters.AddWithValue("@rd", ParseDate(txtReplaceDate.Text));
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            txtHistoryName.Text = ""; txtCHQHistoryName.Text = ""; txtReplaceDate.Text = "";
            BindNameHistory(vc);
            ShowMsg("Name history saved.", "success");
        }
        catch (Exception ex) { ShowMsg(ex.Message, "error"); }

        ScriptManager.RegisterStartupScript(this, GetType(), "tab",
            "currentView='form'; applyView(); switchTab('tab-namehistory');", true);
    }

    protected void gvNameHistory_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        int slno = Convert.ToInt32(e.CommandArgument);
        int vc; int.TryParse(hfVendorCode.Value, out vc);

        if (e.CommandName == "EditNH")
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConStr))
                {
                    string q = "SELECT * FROM Vendor_Name_History WHERE SLNO=@sl";
                    using (SqlCommand cmd = new SqlCommand(q, con))
                    {
                        cmd.Parameters.AddWithValue("@sl", slno);
                        con.Open();
                        SqlDataReader dr = cmd.ExecuteReader();
                        if (dr.Read())
                        {
                            txtHistoryName.Text = dr["HistoryName"].ToString();
                            txtCHQHistoryName.Text = dr["CHQHistoryName"].ToString();
                            if (dr["ReplaceDate"] != DBNull.Value)
                                txtReplaceDate.Text = Convert.ToDateTime(dr["ReplaceDate"]).ToString("yyyy-MM-dd");
                            hfNHSLNO.Value = slno.ToString();
                        }
                    }
                }
            }
            catch (Exception ex) { ShowMsg(ex.Message, "error"); }
        }
        else if (e.CommandName == "DeleteNH")
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConStr))
                {
                    using (SqlCommand cmd = new SqlCommand(
                        "DELETE FROM Vendor_Name_History WHERE SLNO=@sl", con))
                    {
                        cmd.Parameters.AddWithValue("@sl", slno);
                        con.Open(); cmd.ExecuteNonQuery();
                    }
                }
                ShowMsg("History record deleted.", "success");
            }
            catch (Exception ex) { ShowMsg(ex.Message, "error"); }
        }

        BindNameHistory(vc);
        ScriptManager.RegisterStartupScript(this, GetType(), "tab",
            "currentView='form'; applyView(); switchTab('tab-namehistory');", true);
    }

    // =============================================
    // ATTACHMENTS
    // =============================================
    protected void btnUpload_Click(object sender, EventArgs e)
    {
        int vc; if (!int.TryParse(hfVendorCode.Value, out vc) || vc == 0)
        { ShowMsg("Save vendor first.", "error"); return; }

        if (!fuAttach.HasFile)
        { ShowMsg("Please choose a file.", "error"); return; }

        if (fuAttach.PostedFile.ContentLength > 3 * 1024 * 1024)
        { ShowMsg("File exceeds 3MB limit.", "error"); return; }

        try
        {
            string folder = Server.MapPath("~/App_Data/VendorAttachments/" + vc + "/");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            string fname = DateTime.Now.Ticks + "_" + Path.GetFileName(fuAttach.FileName);
            string fpath = Path.Combine(folder, fname);
            fuAttach.SaveAs(fpath);

            string virtualPath = "~/App_Data/VendorAttachments/" + vc + "/" + fname;

            using (SqlConnection con = new SqlConnection(ConStr))
            {
                string q = "INSERT INTO Vendor_Attachments(VendorCode,Remarks,FilePath) VALUES(@vc,@rm,@fp)";
                using (SqlCommand cmd = new SqlCommand(q, con))
                {
                    cmd.Parameters.AddWithValue("@vc", vc);
                    cmd.Parameters.AddWithValue("@rm", txtAttachRemarks.Text.Trim());
                    cmd.Parameters.AddWithValue("@fp", virtualPath);
                    con.Open(); cmd.ExecuteNonQuery();
                }
            }
            txtAttachRemarks.Text = "";
            BindAttachments(vc);
            ShowMsg("File uploaded.", "success");
        }
        catch (Exception ex) { ShowMsg(ex.Message, "error"); }

        ScriptManager.RegisterStartupScript(this, GetType(), "tab",
            "currentView='form'; applyView(); switchTab('tab-attachments');", true);
    }

    protected void gvAttachments_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        int attachId = Convert.ToInt32(e.CommandArgument);
        int vc; int.TryParse(hfVendorCode.Value, out vc);

        if (e.CommandName == "DeleteAtt")
        {
            try
            {
                string filePath = "";
                using (SqlConnection con = new SqlConnection(ConStr))
                {
                    using (SqlCommand cmd = new SqlCommand(
                        "SELECT FilePath FROM Vendor_Attachments WHERE AttachID=@id", con))
                    {
                        cmd.Parameters.AddWithValue("@id", attachId);
                        con.Open();
                        object res = cmd.ExecuteScalar();
                        if (res != null) filePath = res.ToString();
                    }
                }
                // Try delete physical file
                if (!string.IsNullOrEmpty(filePath))
                {
                    try { File.Delete(Server.MapPath(filePath)); } catch { }
                }
                using (SqlConnection con = new SqlConnection(ConStr))
                {
                    using (SqlCommand cmd = new SqlCommand(
                        "DELETE FROM Vendor_Attachments WHERE AttachID=@id", con))
                    {
                        cmd.Parameters.AddWithValue("@id", attachId);
                        con.Open(); cmd.ExecuteNonQuery();
                    }
                }
                ShowMsg("Attachment deleted.", "success");
            }
            catch (Exception ex) { ShowMsg(ex.Message, "error"); }
            BindAttachments(vc);
        }

        ScriptManager.RegisterStartupScript(this, GetType(), "tab",
            "currentView='form'; applyView(); switchTab('tab-attachments');", true);
    }

    protected void btnViewDownload_Click(object sender, EventArgs e)
    {
        ShowMsg("Select a file in the grid to download.", "success");
        ScriptManager.RegisterStartupScript(this, GetType(), "tab",
            "currentView='form'; applyView(); switchTab('tab-attachments');", true);
    }

    // =============================================
    // CHECKLIST SAVE
    // =============================================
    protected void btnSaveChecklist_Click(object sender, EventArgs e)
    {
        int vc; if (!int.TryParse(hfVendorCode.Value, out vc) || vc == 0)
        { ShowMsg("Save vendor first.", "error"); return; }

        try
        {
            using (SqlConnection con = new SqlConnection(ConStr))
            {
                con.Open();
                foreach (GridViewRow row in gvChecklist.Rows)
                {
                    int checkId = Convert.ToInt32(gvChecklist.DataKeys[row.RowIndex].Value);
                    CheckBox cb = (CheckBox)row.FindControl("chkItem");
                    TextBox tb = (TextBox)row.FindControl("txtCLRemarks");
                    bool isChecked = cb != null && cb.Checked;
                    string remarks = tb != null ? tb.Text.Trim() : "";

                    string q = @"UPDATE Vendor_Checklist SET IsChecked=@ic, Remarks=@rm
                                 WHERE CheckID=@cid";
                    using (SqlCommand cmd = new SqlCommand(q, con))
                    {
                        cmd.Parameters.AddWithValue("@ic", isChecked);
                        cmd.Parameters.AddWithValue("@rm", remarks);
                        cmd.Parameters.AddWithValue("@cid", checkId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            ShowMsg("Checklist saved.", "success");
            BindChecklist(vc);
        }
        catch (Exception ex) { ShowMsg(ex.Message, "error"); }

        ScriptManager.RegisterStartupScript(this, GetType(), "tab",
            "currentView='form'; applyView(); switchTab('tab-checklist');", true);
    }

    // =============================================
    // HELPERS
    // =============================================
    private void LoadVendor(int vendorCode)
    {
        try
        {
            using (SqlConnection con = new SqlConnection(ConStr))
            {
                string q = "SELECT * FROM Vendor_Master WHERE VendorCode=@vc AND IsDeleted=0";
                using (SqlCommand cmd = new SqlCommand(q, con))
                {
                    cmd.Parameters.AddWithValue("@vc", vendorCode);
                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        hfVendorCode.Value = vendorCode.ToString();
                        txtCode.Text = vendorCode.ToString();
                        txtName.Text = dr["VendorName"].ToString();
                        txtFullNameLL.Text = dr["FullNameLL"].ToString();
                        txtPrintInCheque.Text = dr["PrintInCheque"].ToString();
                        SetDDL(ddlAccountGroup, dr["AccountGroup"]);
                        rblInvoicingQty.SelectedValue = dr["InvoicingQty"].ToString();
                        SetDDL(ddlBranch, dr["Branch"]);
                        SetDDL(ddlVendorType, dr["VendorType"]);
                        txtOriginCode.Text = dr["SupplierOriginCountryCode"].ToString();
                        txtOriginCountry.Text = dr["SupplierOriginCountry"].ToString();
                        SetDDL(ddlState, dr["StateID"]);
                        txtPOBox.Text = dr["POBox"].ToString();
                        txtAddress.Text = dr["Address"].ToString();
                        txtCity.Text = dr["City"].ToString();
                        txtTelephoneNo.Text = dr["TelephoneNo"].ToString();
                        txtFaxNo.Text = dr["FaxNo"].ToString();
                        txtEmail.Text = dr["Email"].ToString();
                        txtWebSite.Text = dr["WebSite"].ToString();
                        txtVendorRemarks.Text = dr["VendorRemarks"].ToString();
                        txtEvalRemarks.Text = dr["EvalRemarks"].ToString();
                        txtCP1Name.Text = dr["CP1Name"].ToString();
                        txtCP1Desig.Text = dr["CP1Desig"].ToString();
                        txtCP1Mobile.Text = dr["CP1Mobile"].ToString();
                        txtCP1Email.Text = dr["CP1Email"].ToString();
                        txtCP2Name.Text = dr["CP2Name"].ToString();
                        txtCP2Desig.Text = dr["CP2Desig"].ToString();
                        txtCP2Mobile.Text = dr["CP2Mobile"].ToString();
                        txtCP2Email.Text = dr["CP2Email"].ToString();
                        txtBusinessType.Text = dr["BusinessType"].ToString();
                        txtTradeLicenseNo.Text = dr["TradeLicenseNo"].ToString();
                        SetDDL(ddlEvalCategory, dr["EvaluationCategory"]);
                        if (dr["LicenseExpDate"] != DBNull.Value)
                            txtLicenseExpDate.Text = Convert.ToDateTime(dr["LicenseExpDate"]).ToString("yyyy-MM-dd");
                        if (dr["EvalExpDate"] != DBNull.Value)
                            txtEvalExpDate.Text = Convert.ToDateTime(dr["EvalExpDate"]).ToString("yyyy-MM-dd");
                        txtVATRegNo.Text = dr["VATRegNo"].ToString();
                        if (dr["TRNDate"] != DBNull.Value)
                            txtTRNDate.Text = Convert.ToDateTime(dr["TRNDate"]).ToString("yyyy-MM-dd");
                    }
                }
            }
            LoadBranchMapping(vendorCode);
        }
        catch (Exception ex) { ShowMsg(ex.Message, "error"); }
    }

    private void BindSubTabs(int vendorCode)
    {
        if (vendorCode <= 0) return;
        BindNameHistory(vendorCode);
        BindAttachments(vendorCode);
        BindChecklist(vendorCode);
        BindAudit(vendorCode);
    }

    private void BindNameHistory(int vc)
    {
        try
        {
            using (SqlConnection con = new SqlConnection(ConStr))
            {
                string q = "SELECT * FROM Vendor_Name_History WHERE VendorCode=@vc ORDER BY SLNO";
                SqlDataAdapter da = new SqlDataAdapter(q, con);
                da.SelectCommand.Parameters.AddWithValue("@vc", vc);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvNameHistory.DataSource = dt;
                gvNameHistory.DataBind();
            }
        }
        catch { }
    }

    private void BindAttachments(int vc)
    {
        try
        {
            using (SqlConnection con = new SqlConnection(ConStr))
            {
                string q = "SELECT * FROM Vendor_Attachments WHERE VendorCode=@vc ORDER BY AttachID";
                SqlDataAdapter da = new SqlDataAdapter(q, con);
                da.SelectCommand.Parameters.AddWithValue("@vc", vc);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvAttachments.DataSource = dt;
                gvAttachments.DataBind();
            }
        }
        catch { }
    }

    private void BindChecklist(int vc)
    {
        try
        {
            using (SqlConnection con = new SqlConnection(ConStr))
            {
                string q = @"SELECT vc.CheckID, vc.IsChecked, vc.Remarks, vci.Description,
                             ROW_NUMBER() OVER (ORDER BY vc.ItemID) AS RowNum
                             FROM Vendor_Checklist vc
                             JOIN Vendor_Checklist_Items vci ON vc.ItemID = vci.ItemID
                             WHERE vc.VendorCode=@vc";
                SqlDataAdapter da = new SqlDataAdapter(q, con);
                da.SelectCommand.Parameters.AddWithValue("@vc", vc);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvChecklist.DataSource = dt;
                gvChecklist.DataBind();
            }
        }
        catch { }
    }

    private void BindAudit(int vc)
    {
        try
        {
            using (SqlConnection con = new SqlConnection(ConStr))
            {
                string q = "SELECT VendorCode, VendorName, CreatedDate FROM Vendor_Master WHERE VendorCode=@vc";
                SqlDataAdapter da = new SqlDataAdapter(q, con);
                da.SelectCommand.Parameters.AddWithValue("@vc", vc);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvAudit.DataSource = dt;
                gvAudit.DataBind();
            }
        }
        catch { }
    }

    private void LoadBranchMapping(int vc)
    {
        try
        {
            using (SqlConnection con = new SqlConnection(ConStr))
            {
                string q = "SELECT * FROM Vendor_Branch_Mapping WHERE VendorCode=@vc";
                SqlDataAdapter da = new SqlDataAdapter(q, con);
                da.SelectCommand.Parameters.AddWithValue("@vc", vc);
                DataTable dt = new DataTable();
                da.Fill(dt);
                foreach (DataRow r in dt.Rows)
                {
                    string br = r["Branch"].ToString();
                    if (br == "CMP")
                    {
                        SetDDL(ddlCMPPayTerm, r["PaymentTerms"]);
                        SetDDL(ddlCMPTaxType, r["TaxType"]);
                        rblCMPCreditReq.SelectedValue = Convert.ToBoolean(r["CreditLimitRequired"]) ? "1" : "0";
                        txtCMPCreditAmt.Text = r["CreditLimitAmount"].ToString();
                        rblCMPStatus.SelectedValue = r["VendorStatus"].ToString();
                    }
                    else if (br == "PRC")
                    {
                        SetDDL(ddlPRCPayTerm, r["PaymentTerms"]);
                        SetDDL(ddlPRCTaxType, r["TaxType"]);
                        rblPRCCreditReq.SelectedValue = Convert.ToBoolean(r["CreditLimitRequired"]) ? "1" : "0";
                        txtPRCCreditAmt.Text = r["CreditLimitAmount"].ToString();
                        rblPRCStatus.SelectedValue = r["VendorStatus"].ToString();
                    }
                    else if (br == "RMC")
                    {
                        SetDDL(ddlRMCPayTerm, r["PaymentTerms"]);
                        SetDDL(ddlRMCTaxType, r["TaxType"]);
                        rblRMCCreditReq.SelectedValue = Convert.ToBoolean(r["CreditLimitRequired"]) ? "1" : "0";
                        txtRMCCreditAmt.Text = r["CreditLimitAmount"].ToString();
                        rblRMCStatus.SelectedValue = r["VendorStatus"].ToString();
                    }
                }
            }
        }
        catch { }
    }

    private void SaveBranchMapping(SqlConnection con, int vc)
    {
        string[] branches = { "CMP", "PRC", "RMC" };
        DropDownList[] payTerms = { ddlCMPPayTerm, ddlPRCPayTerm, ddlRMCPayTerm };
        DropDownList[] taxTypes = { ddlCMPTaxType, ddlPRCTaxType, ddlRMCTaxType };
        RadioButtonList[] creditReqs = { rblCMPCreditReq, rblPRCCreditReq, rblRMCCreditReq };
        TextBox[] creditAmts = { txtCMPCreditAmt, txtPRCCreditAmt, txtRMCCreditAmt };
        RadioButtonList[] statuses = { rblCMPStatus, rblPRCStatus, rblRMCStatus };

        string ins = @"INSERT INTO Vendor_Branch_Mapping(VendorCode,Branch,PaymentTerms,TaxType,CreditLimitRequired,CreditLimitAmount,VendorStatus)
                       VALUES(@vc,@br,@pt,@tt,@cr,@ca,@vs)";
        for (int i = 0; i < 3; i++)
        {
            using (SqlCommand cmd = new SqlCommand(ins, con))
            {
                cmd.Parameters.AddWithValue("@vc", vc);
                cmd.Parameters.AddWithValue("@br", branches[i]);
                cmd.Parameters.AddWithValue("@pt", payTerms[i].SelectedValue);
                cmd.Parameters.AddWithValue("@tt", taxTypes[i].SelectedValue);
                cmd.Parameters.AddWithValue("@cr", creditReqs[i].SelectedValue == "1");
                decimal amt; decimal.TryParse(creditAmts[i].Text, out amt);
                cmd.Parameters.AddWithValue("@ca", amt);
                cmd.Parameters.AddWithValue("@vs", statuses[i].SelectedValue);
                cmd.ExecuteNonQuery();
            }
        }
    }

    private void InitChecklist(SqlConnection con, int vc)
    {
        // Check if checklist already exists for this vendor
        using (SqlCommand chk = new SqlCommand(
            "SELECT COUNT(*) FROM Vendor_Checklist WHERE VendorCode=@vc", con))
        {
            chk.Parameters.AddWithValue("@vc", vc);
            if (Convert.ToInt32(chk.ExecuteScalar()) > 0) return;
        }
        // Insert one row per checklist item
        string ins = "INSERT INTO Vendor_Checklist(VendorCode,ItemID,IsChecked,Remarks) SELECT @vc,ItemID,0,'' FROM Vendor_Checklist_Items";
        using (SqlCommand cmd = new SqlCommand(ins, con))
        {
            cmd.Parameters.AddWithValue("@vc", vc);
            cmd.ExecuteNonQuery();
        }
    }

    private void LoadChecklistTemplate(int vc)
    {
        if (vc > 0) { BindChecklist(vc); return; }
        try
        {
            using (SqlConnection con = new SqlConnection(ConStr))
            {
                string q = "SELECT 0 AS CheckID, 0 AS IsChecked, '' AS Remarks, Description, ROW_NUMBER() OVER (ORDER BY ItemID) AS RowNum FROM Vendor_Checklist_Items";
                SqlDataAdapter da = new SqlDataAdapter(q, con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvChecklist.DataSource = dt;
                gvChecklist.DataBind();
            }
        }
        catch { }
    }

    private void AddAllParams(SqlCommand cmd)
    {
        cmd.Parameters.AddWithValue("@n", txtName.Text.Trim());
        cmd.Parameters.AddWithValue("@fl", txtFullNameLL.Text.Trim());
        cmd.Parameters.AddWithValue("@pc", txtPrintInCheque.Text.Trim());
        cmd.Parameters.AddWithValue("@ag", ddlAccountGroup.SelectedValue);
        cmd.Parameters.AddWithValue("@iq", rblInvoicingQty.SelectedValue);
        cmd.Parameters.AddWithValue("@br", ddlBranch.SelectedValue);
        cmd.Parameters.AddWithValue("@vt", ddlVendorType.SelectedValue);
        cmd.Parameters.AddWithValue("@orc", txtOriginCode.Text.Trim());
        cmd.Parameters.AddWithValue("@or", txtOriginCountry.Text.Trim());
        cmd.Parameters.AddWithValue("@st", ddlState.SelectedValue);
        cmd.Parameters.AddWithValue("@pb", txtPOBox.Text.Trim());
        cmd.Parameters.AddWithValue("@ad", txtAddress.Text.Trim());
        cmd.Parameters.AddWithValue("@ci", txtCity.Text.Trim());
        cmd.Parameters.AddWithValue("@te", txtTelephoneNo.Text.Trim());
        cmd.Parameters.AddWithValue("@fx", txtFaxNo.Text.Trim());
        cmd.Parameters.AddWithValue("@em", txtEmail.Text.Trim());
        cmd.Parameters.AddWithValue("@ws", txtWebSite.Text.Trim());
        cmd.Parameters.AddWithValue("@vr", txtVendorRemarks.Text.Trim());
        cmd.Parameters.AddWithValue("@er", txtEvalRemarks.Text.Trim());
        cmd.Parameters.AddWithValue("@c1n", txtCP1Name.Text.Trim());
        cmd.Parameters.AddWithValue("@c1d", txtCP1Desig.Text.Trim());
        cmd.Parameters.AddWithValue("@c1m", txtCP1Mobile.Text.Trim());
        cmd.Parameters.AddWithValue("@c1e", txtCP1Email.Text.Trim());
        cmd.Parameters.AddWithValue("@c2n", txtCP2Name.Text.Trim());
        cmd.Parameters.AddWithValue("@c2d", txtCP2Desig.Text.Trim());
        cmd.Parameters.AddWithValue("@c2m", txtCP2Mobile.Text.Trim());
        cmd.Parameters.AddWithValue("@c2e", txtCP2Email.Text.Trim());
        cmd.Parameters.AddWithValue("@bt", txtBusinessType.Text.Trim());
        cmd.Parameters.AddWithValue("@tl", txtTradeLicenseNo.Text.Trim());
        cmd.Parameters.AddWithValue("@ec", ddlEvalCategory.SelectedValue);
        cmd.Parameters.AddWithValue("@ld", ParseDate(txtLicenseExpDate.Text));
        cmd.Parameters.AddWithValue("@ed", ParseDate(txtEvalExpDate.Text));
        cmd.Parameters.AddWithValue("@vat", txtVATRegNo.Text.Trim());
        cmd.Parameters.AddWithValue("@trd", ParseDate(txtTRNDate.Text));
    }

    private void ClearForm()
    {
        txtCode.Text = ""; txtName.Text = ""; txtFullNameLL.Text = "";
        txtPrintInCheque.Text = "";
        if (ddlAccountGroup.Items.Count > 0) ddlAccountGroup.SelectedIndex = 0;
        rblInvoicingQty.SelectedValue = "Supplier";
        if (ddlBranch.Items.Count > 0) ddlBranch.SelectedIndex = 0;
        if (ddlVendorType.Items.Count > 0) ddlVendorType.SelectedIndex = 0;
        txtOriginCode.Text = ""; txtOriginCountry.Text = "";
        if (ddlState.Items.Count > 0) ddlState.SelectedIndex = 0;
        txtPOBox.Text = ""; txtAddress.Text = ""; txtCity.Text = "";
        txtTelephoneNo.Text = ""; txtFaxNo.Text = ""; txtEmail.Text = "";
        txtWebSite.Text = ""; txtVendorRemarks.Text = ""; txtEvalRemarks.Text = "";
        txtCP1Name.Text = ""; txtCP1Desig.Text = ""; txtCP1Mobile.Text = ""; txtCP1Email.Text = "";
        txtCP2Name.Text = ""; txtCP2Desig.Text = ""; txtCP2Mobile.Text = ""; txtCP2Email.Text = "";
        txtBusinessType.Text = ""; txtTradeLicenseNo.Text = "";
        if (ddlEvalCategory.Items.Count > 0) ddlEvalCategory.SelectedIndex = 0;
        txtLicenseExpDate.Text = ""; txtEvalExpDate.Text = ""; txtVATRegNo.Text = ""; txtTRNDate.Text = "";
        txtVerifiedBy.Text = ""; txtVerifiedDateTime.Text = "";
        txtApprovedBy.Text = ""; txtApprovedDateTime.Text = "";
        // Branch mapping defaults
        if (ddlCMPPayTerm.Items.Count > 0) { ddlCMPPayTerm.SelectedIndex = 0; ddlPRCPayTerm.SelectedIndex = 0; ddlRMCPayTerm.SelectedIndex = 0; }
        if (ddlCMPTaxType.Items.Count > 0) { ddlCMPTaxType.SelectedIndex = 0; ddlPRCTaxType.SelectedIndex = 0; ddlRMCTaxType.SelectedIndex = 0; }
        rblCMPCreditReq.SelectedValue = "0"; rblPRCCreditReq.SelectedValue = "0"; rblRMCCreditReq.SelectedValue = "0";
        txtCMPCreditAmt.Text = "0"; txtPRCCreditAmt.Text = "0"; txtRMCCreditAmt.Text = "0";
        rblCMPStatus.SelectedValue = "InActive"; rblPRCStatus.SelectedValue = "InActive"; rblRMCStatus.SelectedValue = "InActive";
        // Clear sub grids
        gvNameHistory.DataSource = null; gvNameHistory.DataBind();
        gvAttachments.DataSource = null; gvAttachments.DataBind();
        gvAudit.DataSource = null; gvAudit.DataBind();
        hfNHSLNO.Value = "0";
        LoadChecklistTemplate(0);
    }

    private void SetDDL(DropDownList ddl, object val)
    {
        if (val == null || val == DBNull.Value) return;
        string s = val.ToString();
        ListItem li = ddl.Items.FindByValue(s);
        if (li != null) ddl.SelectedValue = s;
    }

    private object ParseDate(string s)
    {
        DateTime d;
        if (DateTime.TryParse(s, out d)) return d;
        return DBNull.Value;
    }

    private void ShowMsg(string msg, string type)
    {
        string safe = msg.Replace("'", "\\'").Replace("\r", "").Replace("\n", " ");
        ScriptManager.RegisterStartupScript(this, GetType(), "msg",
           "showNotification('" + safe + "','" + type + "');", true);
    }
}