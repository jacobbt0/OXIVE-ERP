using System;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI.WebControls;

public partial class Masters_EmployeeMasterUpload : System.Web.UI.Page
{
    private string ConStr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
    private DataTable dtPreview = null;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            btnSave.Enabled = false;
            gvPreview.DataSource = null;
            gvPreview.DataBind();
        }
        else if (ViewState["PreviewData"] != null)
        {
            dtPreview = (DataTable)ViewState["PreviewData"];
        }
    }

    protected void btnDownloadTemplate_Click(object sender, EventArgs e)
    {
        string csv = "S.N.,Status,VISA,Employee Name,AR NO,Nationality,Passport No,Passport Expiry,Passport With Company,Join Date,Visa Under Company,Visit Visa Expiry,ID Number,ID Expiry,Designation,BASIC,FOOD ALLOWANCE,HOUSE ALLOWANCE,VISA ALLOWANCE,MOBILE ALLOWANCE,TRANSPORT ALLOWANCE,Other ALLOWANCE,TOTAL\n" +
                     "1,Active,DONE,John Doe,EMP001,USA,AB123456,2028-12-31,Yes,2025-01-01,Company A,2025-12-31,ID123,2026-12-31,Manager,5000,500,500,500,500,500,500,8000";
        Response.Clear();
        Response.ContentType = "text/csv";
        Response.AddHeader("Content-Disposition", "attachment; filename=EmployeeMasterTemplate.csv");
        Response.Write(csv);
        Response.End();
    }

    protected void btnUpload_Click(object sender, EventArgs e)
    {
        if (!fuExcel.HasFile)
        {
            ShowMessage("Please select an Excel file first.", true);
            return;
        }

        string extension = Path.GetExtension(fuExcel.FileName).ToLower();
        if (extension != ".xlsx" && extension != ".xls")
        {
            ShowMessage("Only Excel files (.xlsx, .xls) are allowed.", true);
            return;
        }

        string tempDir = Server.MapPath("~/App_Data/Temp/");
        if (!Directory.Exists(tempDir))
            Directory.CreateDirectory(tempDir);

        string filePath = tempDir + Guid.NewGuid().ToString() + extension;

        try
        {
            fuExcel.SaveAs(filePath);

            string connStr = "";
            if (extension == ".xlsx")
                connStr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + filePath + ";Extended Properties='Excel 12.0 Xml;HDR=YES;IMEX=1'";
            else
                connStr = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + filePath + ";Extended Properties='Excel 8.0;HDR=YES;IMEX=1'";

            using (OleDbConnection conn = new OleDbConnection(connStr))
            {
                conn.Open();
                DataTable sheets = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                string sheetName = sheets.Rows[0]["TABLE_NAME"].ToString();
                string query = "SELECT * FROM [" + sheetName + "]";

                using (OleDbDataAdapter da = new OleDbDataAdapter(query, conn))
                {
                    DataTable rawData = new DataTable();
                    da.Fill(rawData);

                    dtPreview = new DataTable();
                    dtPreview.Columns.Add("SerialNo", typeof(string));
                    dtPreview.Columns.Add("Status", typeof(string));
                    dtPreview.Columns.Add("VisaStatus", typeof(string));
                    dtPreview.Columns.Add("EmployeeName", typeof(string));
                    dtPreview.Columns.Add("ARNo", typeof(string));
                    dtPreview.Columns.Add("Nationality", typeof(string));
                    dtPreview.Columns.Add("PassportNo", typeof(string));
                    dtPreview.Columns.Add("PassportExpiry", typeof(DateTime));
                    dtPreview.Columns.Add("PassportWithCompany", typeof(string));
                    dtPreview.Columns.Add("JoinDate", typeof(DateTime));
                    dtPreview.Columns.Add("VisaUnderCompany", typeof(string));
                    dtPreview.Columns.Add("VisitVisaExpiry", typeof(DateTime));
                    dtPreview.Columns.Add("IDNumber", typeof(string));
                    dtPreview.Columns.Add("IDExpiryDate", typeof(DateTime));
                    dtPreview.Columns.Add("Designation", typeof(string));
                    dtPreview.Columns.Add("BasicSalary", typeof(decimal));
                    dtPreview.Columns.Add("FoodAllowance", typeof(decimal));
                    dtPreview.Columns.Add("HouseAllowance", typeof(decimal));
                    dtPreview.Columns.Add("VisaAllowance", typeof(decimal));
                    dtPreview.Columns.Add("MobileAllowance", typeof(decimal));
                    dtPreview.Columns.Add("TransportAllowance", typeof(decimal));
                    dtPreview.Columns.Add("OtherAllowance", typeof(decimal));
                    dtPreview.Columns.Add("TotalSalary", typeof(decimal));

                    for (int i = 0; i < rawData.Rows.Count; i++)
                    {
                        DataRow row = rawData.Rows[i];
                        if (row[0] == null || string.IsNullOrWhiteSpace(row[0].ToString()))
                            continue;

                        DataRow newRow = dtPreview.NewRow();

                        newRow["SerialNo"] = GetString(row, 0);
                        newRow["Status"] = GetString(row, 1);
                        newRow["VisaStatus"] = GetString(row, 2);
                        newRow["EmployeeName"] = GetString(row, 3);
                        newRow["ARNo"] = GetString(row, 4);
                        newRow["Nationality"] = GetString(row, 5);
                        newRow["PassportNo"] = GetString(row, 6);
                        newRow["PassportExpiry"] = ParseDate(GetString(row, 7));
                        newRow["PassportWithCompany"] = GetString(row, 8);
                        newRow["JoinDate"] = ParseDate(GetString(row, 9));
                        newRow["VisaUnderCompany"] = GetString(row, 10);
                        newRow["VisitVisaExpiry"] = ParseDate(GetString(row, 11));
                        newRow["IDNumber"] = GetString(row, 12);
                        newRow["IDExpiryDate"] = ParseDate(GetString(row, 13));
                        newRow["Designation"] = GetString(row, 14);
                        newRow["BasicSalary"] = ParseDecimal(GetString(row, 15));
                        newRow["FoodAllowance"] = ParseDecimal(GetString(row, 16));
                        newRow["HouseAllowance"] = ParseDecimal(GetString(row, 17));
                        newRow["VisaAllowance"] = ParseDecimal(GetString(row, 18));
                        newRow["MobileAllowance"] = ParseDecimal(GetString(row, 19));
                        newRow["TransportAllowance"] = ParseDecimal(GetString(row, 20));
                        newRow["OtherAllowance"] = ParseDecimal(GetString(row, 21));
                        newRow["TotalSalary"] = ParseDecimal(GetString(row, 22));

                        dtPreview.Rows.Add(newRow);
                    }
                }
            }

            if (File.Exists(filePath))
                File.Delete(filePath);

            if (dtPreview.Rows.Count == 0)
            {
                ShowMessage("No data rows found in the Excel file.", true);
                return;
            }

            ViewState["PreviewData"] = dtPreview;
            gvPreview.DataSource = dtPreview;
            gvPreview.DataBind();
            btnSave.Enabled = true;
            ShowMessage("Loaded " + dtPreview.Rows.Count + " records. Review before saving.", false);
        }
        catch (Exception ex)
        {
            ShowMessage("Error reading Excel: " + ex.Message, true);
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    private string GetString(DataRow row, int colIndex)
    {
        object val = row[colIndex];
        return val == null ? "" : val.ToString().Trim();
    }

    private object ParseDate(string dateStr)
    {
        if (string.IsNullOrWhiteSpace(dateStr))
            return DBNull.Value;
        dateStr = dateStr.Replace('.', '-');
        DateTime dt;
        if (DateTime.TryParse(dateStr, out dt))
            return dt;
        else
            return DBNull.Value;
    }

    private object ParseDecimal(string val)
    {
        if (string.IsNullOrWhiteSpace(val))
            return DBNull.Value;
        decimal d;
        if (decimal.TryParse(val, out d))
            return d;
        else
            return DBNull.Value;
    }

    protected void gvPreview_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        if (ViewState["PreviewData"] != null)
        {
            dtPreview = (DataTable)ViewState["PreviewData"];
            gvPreview.DataSource = dtPreview;
            gvPreview.PageIndex = e.NewPageIndex;
            gvPreview.DataBind();
        }
    }

    protected void btnSave_Click(object sender, EventArgs e)
    {
        if (ViewState["PreviewData"] == null)
        {
            ShowMessage("No data to save. Please upload a file first.", true);
            return;
        }

        DataTable data = (DataTable)ViewState["PreviewData"];
        int inserted = 0;
        int skipped = 0;
        System.Text.StringBuilder duplicateList = new System.Text.StringBuilder();

        using (SqlConnection con = new SqlConnection(ConStr))
        {
            con.Open();
            foreach (DataRow row in data.Rows)
            {
                string arNo = row["ARNo"] != DBNull.Value ? row["ARNo"].ToString().Trim() : "";
                if (string.IsNullOrEmpty(arNo))
                {
                    skipped++;
                    duplicateList.AppendLine(" - Row with empty AR No (Employee ID)");
                    continue;
                }

                // Check if ARNo already exists
                string checkSql = "SELECT COUNT(*) FROM Employee_Info WHERE ARNo = @arNo";
                using (SqlCommand checkCmd = new SqlCommand(checkSql, con))
                {
                    checkCmd.Parameters.AddWithValue("@arNo", arNo);
                    int count = (int)checkCmd.ExecuteScalar();
                    if (count > 0)
                    {
                        skipped++;
                        duplicateList.AppendLine(" - " + arNo + " (" + row["EmployeeName"] + ")");
                        continue;
                    }
                }

                // Insert new record
                try
                {
                    string insertSql = @"
                    INSERT INTO Employee_Info
                    (SerialNo, Status, VisaStatus, EmployeeName, ARNo, Nationality,
                     PassportNo, PassportExpiry, PassportWithCompany, JoinDate, VisaUnderCompany,
                     VisitVisaExpiry, IDNumber, IDExpiryDate, Designation,
                     BasicSalary, FoodAllowance, HouseAllowance, VisaAllowance,
                     MobileAllowance, TransportAllowance, OtherAllowance, TotalSalary,
                     CreatedBy)
                    VALUES
                    (@SerialNo, @Status, @VisaStatus, @EmployeeName, @ARNo, @Nationality,
                     @PassportNo, @PassportExpiry, @PassportWithCompany, @JoinDate, @VisaUnderCompany,
                     @VisitVisaExpiry, @IDNumber, @IDExpiryDate, @Designation,
                     @BasicSalary, @FoodAllowance, @HouseAllowance, @VisaAllowance,
                     @MobileAllowance, @TransportAllowance, @OtherAllowance, @TotalSalary,
                     @CreatedBy)";

                    using (SqlCommand cmd = new SqlCommand(insertSql, con))
                    {
                        cmd.Parameters.AddWithValue("@SerialNo", row["SerialNo"] ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Status", row["Status"] ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@VisaStatus", row["VisaStatus"] ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@EmployeeName", row["EmployeeName"] ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ARNo", arNo);
                        cmd.Parameters.AddWithValue("@Nationality", row["Nationality"] ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@PassportNo", row["PassportNo"] ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@PassportExpiry", row["PassportExpiry"] == DBNull.Value ? DBNull.Value : row["PassportExpiry"]);
                        cmd.Parameters.AddWithValue("@PassportWithCompany", row["PassportWithCompany"] ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@JoinDate", row["JoinDate"] == DBNull.Value ? DBNull.Value : row["JoinDate"]);
                        cmd.Parameters.AddWithValue("@VisaUnderCompany", row["VisaUnderCompany"] ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@VisitVisaExpiry", row["VisitVisaExpiry"] == DBNull.Value ? DBNull.Value : row["VisitVisaExpiry"]);
                        cmd.Parameters.AddWithValue("@IDNumber", row["IDNumber"] ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@IDExpiryDate", row["IDExpiryDate"] == DBNull.Value ? DBNull.Value : row["IDExpiryDate"]);
                        cmd.Parameters.AddWithValue("@Designation", row["Designation"] ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@BasicSalary", row["BasicSalary"] ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@FoodAllowance", row["FoodAllowance"] ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@HouseAllowance", row["HouseAllowance"] ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@VisaAllowance", row["VisaAllowance"] ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@MobileAllowance", row["MobileAllowance"] ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@TransportAllowance", row["TransportAllowance"] ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@OtherAllowance", row["OtherAllowance"] ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@TotalSalary", row["TotalSalary"] ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@CreatedBy", Page.User.Identity.Name ?? "Admin");

                        cmd.ExecuteNonQuery();
                        inserted++;
                    }
                }
                catch (Exception ex)
                {
                    skipped++;
                    duplicateList.AppendLine(" - " + arNo + " (Error: " + ex.Message + ")");
                }
            }
        }

        // Build alert message
        string alertMsg = "";
        if (inserted > 0)
            alertMsg = inserted + " record(s) inserted successfully.\n";
        if (skipped > 0)
        {
            alertMsg += skipped + " record(s) skipped.\n";
            if (duplicateList.Length > 0)
                alertMsg += "Duplicates / Issues:\n" + duplicateList.ToString();
        }

        if (skipped > 0)
            ShowMessage(alertMsg.Replace("\n", "<br/>"), true);
        else if (inserted > 0)
            ShowMessage(alertMsg.Replace("\n", "<br/>"), false);
        else
            ShowMessage("No records were inserted.", true);

        if (inserted > 0)
        {
            ViewState["PreviewData"] = null;
            gvPreview.DataSource = null;
            gvPreview.DataBind();
            btnSave.Enabled = false;
        }
    }

    protected void btnClear_Click(object sender, EventArgs e)
    {
        ViewState["PreviewData"] = null;
        gvPreview.DataSource = null;
        gvPreview.DataBind();
        btnSave.Enabled = false;
        lblMessage.Text = "";
        ShowMessage("Form cleared.", false);
    }

    private void ShowMessage(string msg, bool isError)
    {
        lblMessage.ForeColor = isError ? System.Drawing.Color.Red : System.Drawing.Color.Green;
        lblMessage.Text = msg;
        ClientScript.RegisterStartupScript(GetType(), "msg",
            "showMessage('" + msg.Replace("'", "\\'") + "', " + isError.ToString().ToLower() + ");", true);
    }
}