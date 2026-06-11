using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;

public partial class Masters_VendorMaster : System.Web.UI.Page
{
    string ConStr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

    // PAGE LOAD

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if (!IsPostBack)
            {
                hfView.Value = "list";
                BindGridData();
            }
            else
            {
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
                        Page.ClientScript.RegisterStartupScript(GetType(), "view",
                            "applyView(); restoreTab();", true);
                    }
                    else
                    {
                        ShowMsg("Invalid vendor code in URL argument.", "error");
                    }
                }
                else
                {
                    // Bind grid data on every postback so it stays current.
                    // Do NOT register a "view" script here — each button handler
                    // owns its own navigation script to avoid first-write-wins
                    // collision with Page.ClientScript (duplicate keys are silently dropped).
                    BindGridData();
                }
            }
        }
        catch (Exception ex)
        {
            ShowMsg("Page load error: " + ex.Message, "error");
        }
    }

    // GRID DATA

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
        catch (SqlException ex)
        {
            hfGridData.Value = "[]";
            ShowMsg("Database error loading grid: " + ex.Message, "error");
        }
        catch (Exception ex)
        {
            hfGridData.Value = "[]";
            ShowMsg("Error loading vendor list: " + ex.Message, "error");
        }
    }


    // ADD NEW

    protected void btnAddNew_Click(object sender, EventArgs e)
    {
        try
        {
            ClearForm();
            hfView.Value = "form";
            hfVendorCode.Value = "0";
            LoadChecklistTemplate(0);
            Page.ClientScript.RegisterStartupScript(GetType(), "view",
                "applyView(); switchTab('tab-details');", true);
        }
        catch (Exception ex)
        {
            ShowMsg("Error opening new vendor form: " + ex.Message, "error");
        }
    }

    // BACK TO LIST

    protected void btnBackToList_Click(object sender, EventArgs e)
    {
        try
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
            Page.ClientScript.RegisterStartupScript(GetType(), "view",
                "applyView(); renderGrid();", true);
        }
        catch (Exception ex)
        {
            ShowMsg("Error returning to list: " + ex.Message, "error");
        }
    }


    // RADIO FILTER CHANGED

    protected void rbFilter_Changed(object sender, EventArgs e)
    {
        try
        {
            BindGridData();
            Page.ClientScript.RegisterStartupScript(GetType(), "view",
                "applyView(); renderGrid();", true);
        }
        catch (Exception ex)
        {
            ShowMsg("Error applying filter: " + ex.Message, "error");
        }
    }


    // SEARCH CODE BUTTON

    protected void btnPopupSelect_Click(object sender, EventArgs e)
    {
        try
        {
            int vc;
            if (!int.TryParse(hfVendorCode.Value, out vc) || vc == 0)
            {
                ShowMsg("Invalid vendor selection.", "error");
                return;
            }
            LoadVendor(vc);
            BindSubTabs(vc);
            hfView.Value = "form";
            Page.ClientScript.RegisterStartupScript(GetType(), "view",
                "applyView(); switchTab('tab-details');", true);
        }
        catch (Exception ex)
        {
            ShowMsg("Error loading vendor: " + ex.Message, "error");
        }
    }

    protected void btnSearchCode_Click(object sender, EventArgs e)
    {
        try
        {
            string code = txtCode.Text.Trim();
            if (string.IsNullOrEmpty(code))
            {
                ShowMsg("Please enter a vendor code to search.", "error");
                return;
            }
            int vc;
            if (!int.TryParse(code, out vc))
            {
                ShowMsg("Vendor code must be a number.", "error");
                return;
            }
            LoadVendor(vc);
            BindSubTabs(vc);
            hfView.Value = "form";
            Page.ClientScript.RegisterStartupScript(GetType(), "view",
                "applyView(); switchTab('tab-details');", true);
        }
        catch (Exception ex)
        {
            ShowMsg("Error searching vendor: " + ex.Message, "error");
        }
    }

    // SAVE

    protected void btnSave_Click(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                ShowMsg("Vendor Name is required.", "error");
                return;
            }

            using (SqlConnection con = new SqlConnection(ConStr))
            {
                con.Open();

                // Duplicate name check
                try
                {
                    string chk = "SELECT COUNT(*) FROM Vendor_Master WHERE VendorName=@n AND IsDeleted=0";
                    using (SqlCommand c = new SqlCommand(chk, con))
                    {
                        c.Parameters.AddWithValue("@n", txtName.Text.Trim());
                        if (Convert.ToInt32(c.ExecuteScalar()) > 0)
                        {
                            ShowMsg("Vendor Name already exists.", "error");
                            return;
                        }
                    }
                }
                catch (SqlException ex)
                {
                    ShowMsg("Error checking duplicate vendor: " + ex.Message, "error");
                    return;
                }

                int newCode;
                try
                {
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

                    using (SqlCommand cmd = new SqlCommand(ins, con))
                    {
                        AddAllParams(cmd);
                        newCode = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
                catch (SqlException ex)
                {
                    ShowMsg("Database error saving vendor: " + ex.Message, "error");
                    return;
                }

                hfVendorCode.Value = newCode.ToString();
                txtCode.Text = newCode.ToString();

                try
                {
                    SaveBranchMapping(con, newCode);
                }
                catch (Exception ex)
                {
                    ShowMsg("Vendor saved but branch mapping failed: " + ex.Message, "error");
                }

                try
                {
                    InitChecklist(con, newCode);
                }
                catch (Exception ex)
                {
                    ShowMsg("Vendor saved but checklist init failed: " + ex.Message, "error");
                }
            }

            ShowMsg("Vendor saved successfully. Code: " + hfVendorCode.Value, "success");
            BindGridData();

            int vc2;
            if (int.TryParse(hfVendorCode.Value, out vc2))
                BindSubTabs(vc2);
        }
        catch (Exception ex)
        {
            ShowMsg("Unexpected error saving vendor: " + ex.Message, "error");
        }
    }


    // UPDATE

    protected void btnUpdate_Click(object sender, EventArgs e)
    {
        try
        {
            int vendorCode;
            if (!int.TryParse(hfVendorCode.Value, out vendorCode) || vendorCode == 0)
            {
                ShowMsg("No vendor selected for update.", "error");
                return;
            }
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                ShowMsg("Vendor Name is required.", "error");
                return;
            }

            using (SqlConnection con = new SqlConnection(ConStr))
            {
                con.Open();

                // Duplicate check (exclude self)
                try
                {
                    string chk = "SELECT COUNT(*) FROM Vendor_Master WHERE VendorName=@n AND VendorCode<>@c AND IsDeleted=0";
                    using (SqlCommand c = new SqlCommand(chk, con))
                    {
                        c.Parameters.AddWithValue("@n", txtName.Text.Trim());
                        c.Parameters.AddWithValue("@c", vendorCode);
                        if (Convert.ToInt32(c.ExecuteScalar()) > 0)
                        {
                            ShowMsg("Vendor Name already exists for another vendor.", "error");
                            return;
                        }
                    }
                }
                catch (SqlException ex)
                {
                    ShowMsg("Error checking duplicate vendor name: " + ex.Message, "error");
                    return;
                }

                try
                {
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
                }
                catch (SqlException ex)
                {
                    ShowMsg("Database error updating vendor: " + ex.Message, "error");
                    return;
                }

                try
                {
                    using (SqlCommand del = new SqlCommand(
                        "DELETE FROM Vendor_Branch_Mapping WHERE VendorCode=@vc", con))
                    {
                        del.Parameters.AddWithValue("@vc", vendorCode);
                        del.ExecuteNonQuery();
                    }
                    SaveBranchMapping(con, vendorCode);
                }
                catch (Exception ex)
                {
                    ShowMsg("Vendor updated but branch mapping failed: " + ex.Message, "error");
                }
            }

            ShowMsg("Vendor updated successfully.", "success");
            BindGridData();
            BindSubTabs(vendorCode);
        }
        catch (Exception ex)
        {
            ShowMsg("Unexpected error updating vendor: " + ex.Message, "error");
        }
    }


    // DELETE

    protected void btnDelete_Click(object sender, EventArgs e)
    {
        try
        {
            int vendorCode;
            if (!int.TryParse(hfVendorCode.Value, out vendorCode) || vendorCode == 0)
            {
                ShowMsg("No vendor selected for deletion.", "error");
                return;
            }

            using (SqlConnection con = new SqlConnection(ConStr))
            {
                string q = "DELETE FROM Vendor_Master WHERE VendorCode=@vc";
                using (SqlCommand cmd = new SqlCommand(q, con))
                {
                    cmd.Parameters.AddWithValue("@vc", vendorCode);
                    con.Open();
                    int rows = cmd.ExecuteNonQuery();
                    if (rows == 0)
                    {
                        ShowMsg("Vendor not found or already deleted.", "error");
                        return;
                    }
                }
            }

            ShowMsg("Vendor deleted successfully.", "success");
            ClearForm();
            hfView.Value = "list";
            BindGridData();
            Page.ClientScript.RegisterStartupScript(GetType(), "view",
                "applyView(); renderGrid();", true);
        }
        catch (SqlException ex)
        {
            ShowMsg("Database error deleting vendor: " + ex.Message, "error");
        }
        catch (Exception ex)
        {
            ShowMsg("Unexpected error deleting vendor: " + ex.Message, "error");
        }
    }

    // =============================================
    // CLEAR
    // =============================================
    protected void btnClear_Click(object sender, EventArgs e)
    {
        try
        {
            ClearForm();
            hfVendorCode.Value = "0";
            Page.ClientScript.RegisterStartupScript(GetType(), "view",
                "switchTab('tab-details');", true);
        }
        catch (Exception ex)
        {
            ShowMsg("Error clearing form: " + ex.Message, "error");
        }
    }

    // =============================================
    // PRINT
    // =============================================
    protected void btnPrint_Click(object sender, EventArgs e)
    {
        try
        {
            Page.ClientScript.RegisterStartupScript(GetType(), "prnt", "window.print();", true);
        }
        catch (Exception ex)
        {
            ShowMsg("Error triggering print: " + ex.Message, "error");
        }
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
        catch (System.Threading.ThreadAbortException)
        {
            // Response.End() always throws ThreadAbortException — this is normal, ignore it
        }
        catch (SqlException ex)
        {
            ShowMsg("Database error exporting data: " + ex.Message, "error");
        }
        catch (Exception ex)
        {
            ShowMsg("Error exporting to Excel: " + ex.Message, "error");
        }
    }

    // =============================================
    // NAME HISTORY — ADD / EDIT
    // =============================================
    protected void btnAddHistory_Click(object sender, EventArgs e)
    {
        try
        {
            int vc;
            if (!int.TryParse(hfVendorCode.Value, out vc) || vc == 0)
            {
                ShowMsg("Please save the vendor first before adding name history.", "error");
                return;
            }
            if (string.IsNullOrWhiteSpace(txtHistoryName.Text))
            {
                ShowMsg("History Name is required.", "error");
                return;
            }

            int slno;
            int.TryParse(hfNHSLNO.Value, out slno);

            using (SqlConnection con = new SqlConnection(ConStr))
            {
                con.Open();
                if (slno > 0)
                {
                    try
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
                    catch (SqlException ex)
                    {
                        ShowMsg("Database error updating history record: " + ex.Message, "error");
                        return;
                    }
                }
                else
                {
                    try
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
                    catch (SqlException ex)
                    {
                        ShowMsg("Database error adding history record: " + ex.Message, "error");
                        return;
                    }
                }
            }

            txtHistoryName.Text = "";
            txtCHQHistoryName.Text = "";
            txtReplaceDate.Text = "";
            BindNameHistory(vc);
            ShowMsg("Name history saved successfully.", "success");
        }
        catch (Exception ex)
        {
            ShowMsg("Unexpected error saving name history: " + ex.Message, "error");
        }

        Page.ClientScript.RegisterStartupScript(GetType(), "tab",
            "applyView(); switchTab('tab-namehistory');", true);
    }

    // =============================================
    // NAME HISTORY GRID — ROW COMMAND
    // =============================================
    protected void gvNameHistory_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        try
        {
            int slno;
            if (!int.TryParse(e.CommandArgument.ToString(), out slno))
            {
                ShowMsg("Invalid history record ID.", "error");
                return;
            }
            int vc;
            int.TryParse(hfVendorCode.Value, out vc);

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
                            else
                            {
                                ShowMsg("History record not found.", "error");
                            }
                        }
                    }
                }
                catch (SqlException ex)
                {
                    ShowMsg("Database error loading history record: " + ex.Message, "error");
                }
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
                            con.Open();
                            int rows = cmd.ExecuteNonQuery();
                            if (rows == 0)
                                ShowMsg("History record not found or already deleted.", "error");
                            else
                                ShowMsg("History record deleted.", "success");
                        }
                    }
                }
                catch (SqlException ex)
                {
                    ShowMsg("Database error deleting history record: " + ex.Message, "error");
                }
            }

            BindNameHistory(vc);
        }
        catch (Exception ex)
        {
            ShowMsg("Unexpected error in name history: " + ex.Message, "error");
        }

        Page.ClientScript.RegisterStartupScript(GetType(), "tab",
            "applyView(); switchTab('tab-namehistory');", true);
    }

    // =============================================
    // UPLOAD ATTACHMENT
    // =============================================
    protected void btnUpload_Click(object sender, EventArgs e)
    {
        try
        {
            int vc;
            if (!int.TryParse(hfVendorCode.Value, out vc) || vc == 0)
            {
                ShowMsg("Please save the vendor first before uploading attachments.", "error");
                return;
            }
            if (!fuAttach.HasFile)
            {
                ShowMsg("Please choose a file to upload.", "error");
                return;
            }
            if (fuAttach.PostedFile.ContentLength > 3 * 1024 * 1024)
            {
                ShowMsg("File size exceeds the 3MB maximum limit.", "error");
                return;
            }

            string folder = "";
            try
            {
                folder = Server.MapPath("~/App_Data/VendorAttachments/" + vc + "/");
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
            }
            catch (Exception ex)
            {
                ShowMsg("Error creating upload folder: " + ex.Message, "error");
                return;
            }

            string fname = "";
            string virtualPath = "";
            try
            {
                fname = DateTime.Now.Ticks + "_" + Path.GetFileName(fuAttach.FileName);
                string fpath = Path.Combine(folder, fname);
                fuAttach.SaveAs(fpath);
                virtualPath = "~/App_Data/VendorAttachments/" + vc + "/" + fname;
            }
            catch (Exception ex)
            {
                ShowMsg("Error saving file to disk: " + ex.Message, "error");
                return;
            }

            try
            {
                using (SqlConnection con = new SqlConnection(ConStr))
                {
                    string q = "INSERT INTO Vendor_Attachments(VendorCode,Remarks,FilePath) VALUES(@vc,@rm,@fp)";
                    using (SqlCommand cmd = new SqlCommand(q, con))
                    {
                        cmd.Parameters.AddWithValue("@vc", vc);
                        cmd.Parameters.AddWithValue("@rm", txtAttachRemarks.Text.Trim());
                        cmd.Parameters.AddWithValue("@fp", virtualPath);
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException ex)
            {
                ShowMsg("File saved but database record failed: " + ex.Message, "error");
                return;
            }

            txtAttachRemarks.Text = "";
            BindAttachments(vc);
            ShowMsg("File uploaded successfully.", "success");
        }
        catch (Exception ex)
        {
            ShowMsg("Unexpected error uploading file: " + ex.Message, "error");
        }

        Page.ClientScript.RegisterStartupScript(GetType(), "tab",
            "applyView(); switchTab('tab-attachments');", true);
    }

    // =============================================
    // ATTACHMENTS GRID — ROW COMMAND
    // =============================================
    protected void gvAttachments_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        try
        {
            int attachId;
            if (!int.TryParse(e.CommandArgument.ToString(), out attachId))
            {
                ShowMsg("Invalid attachment ID.", "error");
                return;
            }
            int vc;
            int.TryParse(hfVendorCode.Value, out vc);

            if (e.CommandName == "DeleteAtt")
            {
                string filePath = "";
                try
                {
                    using (SqlConnection con = new SqlConnection(ConStr))
                    {
                        using (SqlCommand cmd = new SqlCommand(
                            "SELECT FilePath FROM Vendor_Attachments WHERE AttachID=@id", con))
                        {
                            cmd.Parameters.AddWithValue("@id", attachId);
                            con.Open();
                            object res = cmd.ExecuteScalar();
                            filePath = res != null ? res.ToString() : "";
                        }
                    }
                }
                catch (SqlException ex)
                {
                    ShowMsg("Database error retrieving attachment path: " + ex.Message, "error");
                    return;
                }

                // Try to delete physical file — don't stop if it fails (file may already be gone)
                if (!string.IsNullOrEmpty(filePath))
                {
                    try
                    {
                        string physicalPath = Server.MapPath(filePath);
                        if (File.Exists(physicalPath))
                            File.Delete(physicalPath);
                    }
                    catch (Exception ex)
                    {
                        ShowMsg("Warning: Could not delete physical file: " + ex.Message, "error");
                        // Continue to remove DB record regardless
                    }
                }

                try
                {
                    using (SqlConnection con = new SqlConnection(ConStr))
                    {
                        using (SqlCommand cmd = new SqlCommand(
                            "DELETE FROM Vendor_Attachments WHERE AttachID=@id", con))
                        {
                            cmd.Parameters.AddWithValue("@id", attachId);
                            con.Open();
                            int rows = cmd.ExecuteNonQuery();
                            if (rows == 0)
                                ShowMsg("Attachment not found or already deleted.", "error");
                            else
                                ShowMsg("Attachment deleted successfully.", "success");
                        }
                    }
                }
                catch (SqlException ex)
                {
                    ShowMsg("Database error deleting attachment record: " + ex.Message, "error");
                    return;
                }

                BindAttachments(vc);
            }
        }
        catch (Exception ex)
        {
            ShowMsg("Unexpected error processing attachment: " + ex.Message, "error");
        }

        Page.ClientScript.RegisterStartupScript(GetType(), "tab",
            "applyView(); switchTab('tab-attachments');", true);
    }

    // =============================================
    // VIEW / DOWNLOAD ATTACHMENT
    // =============================================

    protected void btnViewDownload_Click(object sender, EventArgs e)
    {
        try
        {
            int vendorCode;
            if (!int.TryParse(hfVendorCode.Value, out vendorCode) || vendorCode == 0)
            {
                ShowMsg("Please select a vendor.", "error");
                return;
            }

            // Collect selected attachment IDs
            List<int> selectedAttachIds = new List<int>();
            foreach (GridViewRow row in gvAttachments.Rows)
            {
                CheckBox chkSelect = (CheckBox)row.FindControl("chkSelect");
                if (chkSelect != null && chkSelect.Checked)
                {
                    int attachId = Convert.ToInt32(gvAttachments.DataKeys[row.RowIndex].Value);
                    selectedAttachIds.Add(attachId);
                }
            }

            if (selectedAttachIds.Count == 0)
            {
                ShowMsg("Please select at least one file to download.", "error");
                return;
            }

            if (selectedAttachIds.Count > 1)
            {
                ShowMsg("Please select only one file at a time. Multiple file download is not supported in this version.", "error");
                return;
            }

            // Get file path for the single selected attachment
            string filePath = "";
            using (SqlConnection con = new SqlConnection(ConStr))
            {
                string q = "SELECT FilePath FROM Vendor_Attachments WHERE AttachID = @id AND VendorCode = @vc";
                using (SqlCommand cmd = new SqlCommand(q, con))
                {
                    cmd.Parameters.AddWithValue("@id", selectedAttachIds[0]);
                    cmd.Parameters.AddWithValue("@vc", vendorCode);
                    con.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null)
                        filePath = result.ToString();
                }
            }

            if (string.IsNullOrEmpty(filePath))
            {
                ShowMsg("File not found in database.", "error");
                return;
            }

            string physicalPath = Server.MapPath(filePath);
            if (!File.Exists(physicalPath))
            {
                ShowMsg("File not found on server.", "error");
                return;
            }

            // Download the file
            Response.Clear();
            Response.ContentType = "application/octet-stream";
            Response.AppendHeader("Content-Disposition", "attachment; filename=\"" + Path.GetFileName(physicalPath) + "\"");
            Response.TransmitFile(physicalPath);
            Response.Flush();
            Response.End();
        }
        catch (System.Threading.ThreadAbortException)
        {
            // Expected when Response.End() is called
        }
        catch (Exception ex)
        {
            ShowMsg("Error downloading file: " + ex.Message, "error");
        }
    }





    // =============================================
    // CHECKLIST SAVE
    // =============================================
    protected void btnSaveChecklist_Click(object sender, EventArgs e)
    {
        try
        {
            int vc;
            if (!int.TryParse(hfVendorCode.Value, out vc) || vc == 0)
            {
                ShowMsg("Please save the vendor first before saving the checklist.", "error");
                return;
            }

            using (SqlConnection con = new SqlConnection(ConStr))
            {
                con.Open();
                foreach (GridViewRow row in gvChecklist.Rows)
                {
                    try
                    {
                        int checkId = Convert.ToInt32(gvChecklist.DataKeys[row.RowIndex].Value);
                        CheckBox cb = (CheckBox)row.FindControl("chkItem");
                        TextBox tb = (TextBox)row.FindControl("txtCLRemarks");

                        if (cb == null)
                        {
                            ShowMsg("Checklist row " + (row.RowIndex + 1) + ": checkbox control not found.", "error");
                            continue;
                        }

                        bool isChecked = cb.Checked;
                        string remarks = tb != null ? tb.Text.Trim() : "";

                        string q = "UPDATE Vendor_Checklist SET IsChecked=@ic, Remarks=@rm WHERE CheckID=@cid";
                        using (SqlCommand cmd = new SqlCommand(q, con))
                        {
                            cmd.Parameters.AddWithValue("@ic", isChecked);
                            cmd.Parameters.AddWithValue("@rm", remarks);
                            cmd.Parameters.AddWithValue("@cid", checkId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch (SqlException ex)
                    {
                        ShowMsg("Database error saving checklist row " + (row.RowIndex + 1) + ": " + ex.Message, "error");
                    }
                    catch (Exception ex)
                    {
                        ShowMsg("Error on checklist row " + (row.RowIndex + 1) + ": " + ex.Message, "error");
                    }
                }
            }

            ShowMsg("Checklist saved successfully.", "success");
            BindChecklist(vc);
        }
        catch (Exception ex)
        {
            ShowMsg("Unexpected error saving checklist: " + ex.Message, "error");
        }

        Page.ClientScript.RegisterStartupScript(GetType(), "tab",
            "applyView(); switchTab('tab-checklist');", true);
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

                        try { rblInvoicingQty.SelectedValue = dr["InvoicingQty"].ToString(); }
                        catch { rblInvoicingQty.SelectedValue = "Supplier"; }

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

                        try
                        {
                            if (dr["LicenseExpDate"] != DBNull.Value)
                                txtLicenseExpDate.Text = Convert.ToDateTime(dr["LicenseExpDate"]).ToString("yyyy-MM-dd");
                            if (dr["EvalExpDate"] != DBNull.Value)
                                txtEvalExpDate.Text = Convert.ToDateTime(dr["EvalExpDate"]).ToString("yyyy-MM-dd");
                            if (dr["TRNDate"] != DBNull.Value)
                                txtTRNDate.Text = Convert.ToDateTime(dr["TRNDate"]).ToString("yyyy-MM-dd");
                        }
                        catch (Exception ex)
                        {
                            ShowMsg("Warning: Error parsing date fields: " + ex.Message, "error");
                        }

                        txtVATRegNo.Text = dr["VATRegNo"].ToString();
                    }
                    else
                    {
                        ShowMsg("Vendor code " + vendorCode + " not found.", "error");
                    }
                }
            }

            LoadBranchMapping(vendorCode);
        }
        catch (SqlException ex)
        {
            ShowMsg("Database error loading vendor: " + ex.Message, "error");
        }
        catch (Exception ex)
        {
            ShowMsg("Error loading vendor: " + ex.Message, "error");
        }
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
        catch (SqlException ex)
        {
            ShowMsg("Error loading name history: " + ex.Message, "error");
        }
        catch (Exception ex)
        {
            ShowMsg("Unexpected error loading name history: " + ex.Message, "error");
        }
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
        catch (SqlException ex)
        {
            ShowMsg("Error loading attachments: " + ex.Message, "error");
        }
        catch (Exception ex)
        {
            ShowMsg("Unexpected error loading attachments: " + ex.Message, "error");
        }
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
        catch (SqlException ex)
        {
            ShowMsg("Error loading checklist: " + ex.Message, "error");
        }
        catch (Exception ex)
        {
            ShowMsg("Unexpected error loading checklist: " + ex.Message, "error");
        }
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
        catch (SqlException ex)
        {
            ShowMsg("Error loading audit data: " + ex.Message, "error");
        }
        catch (Exception ex)
        {
            ShowMsg("Unexpected error loading audit data: " + ex.Message, "error");
        }
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
                    try
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
                    catch (Exception ex)
                    {
                        ShowMsg("Warning: Error loading branch '" + r["Branch"] + "' mapping: " + ex.Message, "error");
                    }
                }
            }
        }
        catch (SqlException ex)
        {
            ShowMsg("Database error loading branch mapping: " + ex.Message, "error");
        }
        catch (Exception ex)
        {
            ShowMsg("Unexpected error loading branch mapping: " + ex.Message, "error");
        }
    }

    private void SaveBranchMapping(SqlConnection con, int vc)
    {
        string[] branches = { "CMP", "PRC", "RMC" };
        DropDownList[] payTerms = { ddlCMPPayTerm, ddlPRCPayTerm, ddlRMCPayTerm };
        DropDownList[] taxTypes = { ddlCMPTaxType, ddlPRCTaxType, ddlRMCTaxType };
        RadioButtonList[] creditReqs = { rblCMPCreditReq, rblPRCCreditReq, rblRMCCreditReq };
        TextBox[] creditAmts = { txtCMPCreditAmt, txtPRCCreditAmt, txtRMCCreditAmt };
        RadioButtonList[] statuses = { rblCMPStatus, rblPRCStatus, rblRMCStatus };

        string ins = @"INSERT INTO Vendor_Branch_Mapping
            (VendorCode,Branch,PaymentTerms,TaxType,CreditLimitRequired,CreditLimitAmount,VendorStatus)
            VALUES(@vc,@br,@pt,@tt,@cr,@ca,@vs)";

        for (int i = 0; i < 3; i++)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(ins, con))
                {
                    cmd.Parameters.AddWithValue("@vc", vc);
                    cmd.Parameters.AddWithValue("@br", branches[i]);
                    cmd.Parameters.AddWithValue("@pt", payTerms[i].SelectedValue);
                    cmd.Parameters.AddWithValue("@tt", taxTypes[i].SelectedValue);
                    cmd.Parameters.AddWithValue("@cr", creditReqs[i].SelectedValue == "1");
                    decimal amt;
                    decimal.TryParse(creditAmts[i].Text, out amt);
                    cmd.Parameters.AddWithValue("@ca", amt);
                    cmd.Parameters.AddWithValue("@vs", statuses[i].SelectedValue);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                ShowMsg("Error saving branch mapping for " + branches[i] + ": " + ex.Message, "error");
            }
            catch (Exception ex)
            {
                ShowMsg("Unexpected error saving branch " + branches[i] + ": " + ex.Message, "error");
            }
        }
    }

    private void InitChecklist(SqlConnection con, int vc)
    {
        try
        {
            using (SqlCommand chk = new SqlCommand(
                "SELECT COUNT(*) FROM Vendor_Checklist WHERE VendorCode=@vc", con))
            {
                chk.Parameters.AddWithValue("@vc", vc);
                if (Convert.ToInt32(chk.ExecuteScalar()) > 0) return;
            }
            string ins = "INSERT INTO Vendor_Checklist(VendorCode,ItemID,IsChecked,Remarks) SELECT @vc,ItemID,0,'' FROM Vendor_Checklist_Items";
            using (SqlCommand cmd = new SqlCommand(ins, con))
            {
                cmd.Parameters.AddWithValue("@vc", vc);
                cmd.ExecuteNonQuery();
            }
        }
        catch (SqlException ex)
        {
            ShowMsg("Database error initialising checklist: " + ex.Message, "error");
        }
        catch (Exception ex)
        {
            ShowMsg("Unexpected error initialising checklist: " + ex.Message, "error");
        }
    }

    private void LoadChecklistTemplate(int vc)
    {
        try
        {
            if (vc > 0) { BindChecklist(vc); return; }
            using (SqlConnection con = new SqlConnection(ConStr))
            {
                string q = @"SELECT 0 AS CheckID, 0 AS IsChecked, '' AS Remarks,
                             Description, ROW_NUMBER() OVER (ORDER BY ItemID) AS RowNum
                             FROM Vendor_Checklist_Items";
                SqlDataAdapter da = new SqlDataAdapter(q, con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvChecklist.DataSource = dt;
                gvChecklist.DataBind();
            }
        }
        catch (SqlException ex)
        {
            ShowMsg("Database error loading checklist template: " + ex.Message, "error");
        }
        catch (Exception ex)
        {
            ShowMsg("Error loading checklist template: " + ex.Message, "error");
        }
    }

    private void AddAllParams(SqlCommand cmd)
    {
        try
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
        catch (Exception ex)
        {
            throw new Exception("Error mapping form fields to SQL parameters: " + ex.Message, ex);
        }
    }

    private void ClearForm()
    {
        try
        {
            txtCode.Text = ""; txtName.Text = ""; txtFullNameLL.Text = "";
            txtPrintInCheque.Text = "";
            if (ddlAccountGroup.Items.Count > 0) ddlAccountGroup.SelectedIndex = 0;
            try { rblInvoicingQty.SelectedValue = "Supplier"; } catch { }
            if (ddlBranch.Items.Count > 0) ddlBranch.SelectedIndex = 0;
            if (ddlVendorType.Items.Count > 0) ddlVendorType.SelectedIndex = 0;
            txtOriginCode.Text = ""; txtOriginCountry.Text = "";
            if (ddlState.Items.Count > 0) ddlState.SelectedIndex = 0;
            txtPOBox.Text = ""; txtAddress.Text = ""; txtCity.Text = "";
            txtTelephoneNo.Text = ""; txtFaxNo.Text = "";
            txtEmail.Text = ""; txtWebSite.Text = "";
            txtVendorRemarks.Text = ""; txtEvalRemarks.Text = "";
            txtCP1Name.Text = ""; txtCP1Desig.Text = ""; txtCP1Mobile.Text = ""; txtCP1Email.Text = "";
            txtCP2Name.Text = ""; txtCP2Desig.Text = ""; txtCP2Mobile.Text = ""; txtCP2Email.Text = "";
            txtBusinessType.Text = ""; txtTradeLicenseNo.Text = "";
            if (ddlEvalCategory.Items.Count > 0) ddlEvalCategory.SelectedIndex = 0;
            txtLicenseExpDate.Text = ""; txtEvalExpDate.Text = "";
            txtVATRegNo.Text = ""; txtTRNDate.Text = "";
            txtVerifiedBy.Text = ""; txtVerifiedDateTime.Text = "";
            txtApprovedBy.Text = ""; txtApprovedDateTime.Text = "";

            // Branch mapping defaults
            try
            {
                if (ddlCMPPayTerm.Items.Count > 0) { ddlCMPPayTerm.SelectedIndex = 0; ddlPRCPayTerm.SelectedIndex = 0; ddlRMCPayTerm.SelectedIndex = 0; }
                if (ddlCMPTaxType.Items.Count > 0) { ddlCMPTaxType.SelectedIndex = 0; ddlPRCTaxType.SelectedIndex = 0; ddlRMCTaxType.SelectedIndex = 0; }
                rblCMPCreditReq.SelectedValue = "0"; rblPRCCreditReq.SelectedValue = "0"; rblRMCCreditReq.SelectedValue = "0";
                txtCMPCreditAmt.Text = "0"; txtPRCCreditAmt.Text = "0"; txtRMCCreditAmt.Text = "0";
                rblCMPStatus.SelectedValue = "InActive"; rblPRCStatus.SelectedValue = "InActive"; rblRMCStatus.SelectedValue = "InActive";
            }
            catch (Exception ex)
            {
                ShowMsg("Warning: Could not reset branch mapping defaults: " + ex.Message, "error");
            }

            // Clear sub grids
            try
            {
                gvNameHistory.DataSource = null; gvNameHistory.DataBind();
                gvAttachments.DataSource = null; gvAttachments.DataBind();
                gvAudit.DataSource = null; gvAudit.DataBind();
            }
            catch (Exception ex)
            {
                ShowMsg("Warning: Could not clear sub-grids: " + ex.Message, "error");
            }

            hfNHSLNO.Value = "0";
            LoadChecklistTemplate(0);
        }
        catch (Exception ex)
        {
            ShowMsg("Error clearing form: " + ex.Message, "error");
        }
    }

    private void SetDDL(DropDownList ddl, object val)
    {
        try
        {
            if (ddl == null || val == null || val == DBNull.Value) return;
            string s = val.ToString();
            if (string.IsNullOrEmpty(s)) return;
            ListItem li = ddl.Items.FindByValue(s);
            if (li != null) ddl.SelectedValue = s;
        }
        catch (Exception ex)
        {
            ShowMsg("Warning: Could not set dropdown value: " + ex.Message, "error");
        }
    }

    private object ParseDate(string s)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(s)) return DBNull.Value;
            DateTime d;
            if (DateTime.TryParse(s, out d)) return d;
            return DBNull.Value;
        }
        catch
        {
            return DBNull.Value;
        }
    }

    private void ShowMsg(string msg, string type)
    {
        // Escape for JS string literal
        string safe = msg.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\r", "").Replace("\n", " ");

        // Use Page.ClientScript so it works with or without a ScriptManager control,
        // and does not conflict with ScriptManager-keyed view/tab scripts.
        Page.ClientScript.RegisterStartupScript(
            GetType(),
            "msg_" + Guid.NewGuid().ToString("N"),
            "alert('" + safe + "','" + type + "');",
            true
        );
    }
}