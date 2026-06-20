using System;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Text;
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

   

    // =============================================
    // UPLOAD & PREVIEW
    // =============================================
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

    // =============================================
    // HELPERS (Excel reading)
    // =============================================
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

    // =============================================
    // SAVE TO  TABLE Employee_Salary
    // =============================================
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
        StringBuilder duplicateList = new StringBuilder();

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

                // Check if ARNo already exists in Employee_Master
                string checkSql = "SELECT COUNT(*) FROM Employee_Master WHERE ARNo = @arNo";
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

                // ---- Get or insert master IDs ----
                object nationalityID = GetOrInsertNationality(con, row["Nationality"].ToString());
                object designationID = GetOrInsertDesignation(con, row["Designation"].ToString());
                object statusID = GetOrInsertStatus(con, row["Status"].ToString());
                object visaStatusID = GetOrInsertVisaStatus(con, row["VisaStatus"].ToString());
                object companyID = GetOrInsertCompany(con, row["VisaUnderCompany"].ToString());

                // ---- 1. Insert Employee_Master ----
                string insertMaster = @"
                    INSERT INTO Employee_Master
                    (ARNo, EmployeeName, NationalityID, DesignationID, JoinDate, StatusID, CreatedBy)
                    VALUES
                    (@arNo, @name, @nat, @des, @join, @status, @user)";
                using (SqlCommand cmd = new SqlCommand(insertMaster, con))
                {
                    cmd.Parameters.AddWithValue("@arNo", arNo);
                    cmd.Parameters.AddWithValue("@name", row["EmployeeName"] ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@nat", nationalityID);
                    cmd.Parameters.AddWithValue("@des", designationID);
                    cmd.Parameters.AddWithValue("@join", row["JoinDate"] == DBNull.Value ? DBNull.Value : row["JoinDate"]);
                    cmd.Parameters.AddWithValue("@status", statusID);
                    cmd.Parameters.AddWithValue("@user", Page.User.Identity.Name ?? "Admin");
                    cmd.ExecuteNonQuery();
                }

                // ---- 2. Insert Employee_Visa_Info ----
                string insertVisa = @"
                    INSERT INTO Employee_Visa_Info
                    (ARNo, VisaStatusID, PassportNo, PassportExpiry, PassportWithCompany,
                     VisitVisaExpiry, IDNumber, IDExpiryDate, VisaUnderCompanyID)
                    VALUES
                    (@arNo, @visaStatus, @passport, @passExp, @passWithCo,
                     @visitExp, @idNo, @idExp, @company)";
                using (SqlCommand cmd = new SqlCommand(insertVisa, con))
                {
                    cmd.Parameters.AddWithValue("@arNo", arNo);
                    cmd.Parameters.AddWithValue("@visaStatus", visaStatusID);
                    cmd.Parameters.AddWithValue("@passport", row["PassportNo"] ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@passExp", row["PassportExpiry"] == DBNull.Value ? DBNull.Value : row["PassportExpiry"]);
                    bool passportWithCompany = row["PassportWithCompany"] != DBNull.Value && row["PassportWithCompany"].ToString().ToUpper() == "YES";
                    cmd.Parameters.AddWithValue("@passWithCo", passportWithCompany);
                    cmd.Parameters.AddWithValue("@visitExp", row["VisitVisaExpiry"] == DBNull.Value ? DBNull.Value : row["VisitVisaExpiry"]);
                    cmd.Parameters.AddWithValue("@idNo", row["IDNumber"] ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@idExp", row["IDExpiryDate"] == DBNull.Value ? DBNull.Value : row["IDExpiryDate"]);
                    cmd.Parameters.AddWithValue("@company", companyID);
                    cmd.ExecuteNonQuery();
                }

                //  Insert Salary Components 
                // Get Component IDs for each allowance type
                int basicCompId = GetComponentID(con, "Basic Salary");
                int foodCompId = GetComponentID(con, "Food Allowance");
                int houseCompId = GetComponentID(con, "House Allowance");
                int visaCompId = GetComponentID(con, "Visa Allowance");
                int mobileCompId = GetComponentID(con, "Mobile Allowance");
                int transportCompId = GetComponentID(con, "Transport Allowance");
                int otherCompId = GetComponentID(con, "Other Allowance");

                // Insert only if amount > 0 
                InsertSalaryComponent(con, arNo, basicCompId, row["BasicSalary"]);
                InsertSalaryComponent(con, arNo, foodCompId, row["FoodAllowance"]);
                InsertSalaryComponent(con, arNo, houseCompId, row["HouseAllowance"]);
                InsertSalaryComponent(con, arNo, visaCompId, row["VisaAllowance"]);
                InsertSalaryComponent(con, arNo, mobileCompId, row["MobileAllowance"]);
                InsertSalaryComponent(con, arNo, transportCompId, row["TransportAllowance"]);
                InsertSalaryComponent(con, arNo, otherCompId, row["OtherAllowance"]);

                inserted++;
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

    // Helper to insert a salary component row
    private void InsertSalaryComponent(SqlConnection con, string arNo, int componentId, object amountObj)
    {
        if (componentId == 0) return; // component not found
        decimal amount = 0;
        if (amountObj != DBNull.Value)
            decimal.TryParse(amountObj.ToString(), out amount);
        if (amount == 0) return; // skip zero amounts

        string sql = @"
            INSERT INTO Employee_Salary_Components (ARNo, ComponentID, Amount)
            VALUES (@arNo, @compId, @amount)";
        using (SqlCommand cmd = new SqlCommand(sql, con))
        {
            cmd.Parameters.AddWithValue("@arNo", arNo);
            cmd.Parameters.AddWithValue("@compId", componentId);
            cmd.Parameters.AddWithValue("@amount", amount);
            cmd.ExecuteNonQuery();
        }
    }

    // Helper to get component ID by name
    private int GetComponentID(SqlConnection con, string componentName)
    {
        string sql = "SELECT ComponentID FROM Salary_Component_Master WHERE ComponentName = @name";
        using (SqlCommand cmd = new SqlCommand(sql, con))
        {
            cmd.Parameters.AddWithValue("@name", componentName);
            object result = cmd.ExecuteScalar();
            if (result != null)
                return Convert.ToInt32(result);
            else
                return 0;
        }
    }

    // =============================================
    // MASTER TABLE HELPERS
    // =============================================
    private object GetOrInsertNationality(SqlConnection con, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return DBNull.Value;

        string select = "SELECT NationalityID FROM Nationality_Master WHERE NationalityName = @name";
        using (SqlCommand cmd = new SqlCommand(select, con))
        {
            cmd.Parameters.AddWithValue("@name", name.Trim());
            object result = cmd.ExecuteScalar();
            if (result != null)
                return result;
        }

        string insert = "INSERT INTO Nationality_Master (NationalityName) VALUES (@name); SELECT SCOPE_IDENTITY()";
        using (SqlCommand cmd = new SqlCommand(insert, con))
        {
            cmd.Parameters.AddWithValue("@name", name.Trim());
            return cmd.ExecuteScalar();
        }
    }

    private object GetOrInsertDesignation(SqlConnection con, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return DBNull.Value;
        string select = "SELECT DesignationID FROM Designation_Master WHERE DesignationName = @name";
        using (SqlCommand cmd = new SqlCommand(select, con))
        {
            cmd.Parameters.AddWithValue("@name", name.Trim());
            object result = cmd.ExecuteScalar();
            if (result != null)
                return result;
        }
        string insert = "INSERT INTO Designation_Master (DesignationName) VALUES (@name); SELECT SCOPE_IDENTITY()";
        using (SqlCommand cmd = new SqlCommand(insert, con))
        {
            cmd.Parameters.AddWithValue("@name", name.Trim());
            return cmd.ExecuteScalar();
        }
    }

    private object GetOrInsertStatus(SqlConnection con, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return DBNull.Value;
        string select = "SELECT StatusID FROM EmployeeStatus_Master WHERE StatusName = @name";
        using (SqlCommand cmd = new SqlCommand(select, con))
        {
            cmd.Parameters.AddWithValue("@name", name.Trim());
            object result = cmd.ExecuteScalar();
            if (result != null)
                return result;
        }
        string insert = "INSERT INTO EmployeeStatus_Master (StatusName) VALUES (@name); SELECT SCOPE_IDENTITY()";
        using (SqlCommand cmd = new SqlCommand(insert, con))
        {
            cmd.Parameters.AddWithValue("@name", name.Trim());
            return cmd.ExecuteScalar();
        }
    }

    private object GetOrInsertVisaStatus(SqlConnection con, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return DBNull.Value;
        string select = "SELECT VisaStatusID FROM VisaStatus_Master WHERE VisaStatusName = @name";
        using (SqlCommand cmd = new SqlCommand(select, con))
        {
            cmd.Parameters.AddWithValue("@name", name.Trim());
            object result = cmd.ExecuteScalar();
            if (result != null)
                return result;
        }
        string insert = "INSERT INTO VisaStatus_Master (VisaStatusName) VALUES (@name); SELECT SCOPE_IDENTITY()";
        using (SqlCommand cmd = new SqlCommand(insert, con))
        {
            cmd.Parameters.AddWithValue("@name", name.Trim());
            return cmd.ExecuteScalar();
        }
    }

    private object GetOrInsertCompany(SqlConnection con, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return DBNull.Value;
        string select = "SELECT CompanyID FROM Company_Master WHERE CompanyName = @name";
        using (SqlCommand cmd = new SqlCommand(select, con))
        {
            cmd.Parameters.AddWithValue("@name", name.Trim());
            object result = cmd.ExecuteScalar();
            if (result != null)
                return result;
        }
        string insert = "INSERT INTO Company_Master (CompanyName) VALUES (@name); SELECT SCOPE_IDENTITY()";
        using (SqlCommand cmd = new SqlCommand(insert, con))
        {
            cmd.Parameters.AddWithValue("@name", name.Trim());
            return cmd.ExecuteScalar();
        }
    }

    // =============================================
    // CLEAR
    // =============================================
    protected void btnClear_Click(object sender, EventArgs e)
    {
        ViewState["PreviewData"] = null;
        gvPreview.DataSource = null;
        gvPreview.DataBind();
        btnSave.Enabled = false;
        lblMessage.Text = "";
        ShowMessage("Form cleared.", false);
    }

    // =============================================
    // MESSAGE HELPER
    // =============================================
    private void ShowMessage(string msg, bool isError)
    {
        lblMessage.ForeColor = isError ? System.Drawing.Color.Red : System.Drawing.Color.Green;
        lblMessage.Text = msg;
        ClientScript.RegisterStartupScript(GetType(), "msg",
            "showMessage('" + msg.Replace("'", "\\'") + "', " + isError.ToString().ToLower() + ");", true);
    }
}