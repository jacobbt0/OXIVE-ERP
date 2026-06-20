using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Masters_EmployeeProfile : System.Web.UI.Page
{
    private string ConStr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            BindMasterDropdowns();
            BindGrid();
            hfView.Value = "list";
            ClientScript.RegisterStartupScript(GetType(), "init", "togglePanels(true);", true);
            hfCurrentTab.Value = "info";
            btnSave.Visible = true;
            btnUpdate.Visible = false;
            btnDelete.Visible = false;
            txtARNo.Enabled = true;
            // Load available components dropdown (for add)
            LoadAvailableComponentsDropdown("");
        }
    }

    // ===================== Master Dropdowns =====================
    private void BindMasterDropdowns()
    {
        using (SqlConnection con = new SqlConnection(ConStr))
        {
            // Nationality
            string sql = "SELECT NationalityID, NationalityName FROM Nationality_Master ORDER BY NationalityName";
            SqlDataAdapter da = new SqlDataAdapter(sql, con);
            DataTable dt = new DataTable();
            da.Fill(dt);
            ddlNationality.DataSource = dt;
            ddlNationality.DataValueField = "NationalityID";
            ddlNationality.DataTextField = "NationalityName";
            ddlNationality.DataBind();
            ddlNationality.Items.Insert(0, new ListItem("-- Select --", ""));

            // Designation
            sql = "SELECT DesignationID, DesignationName FROM Designation_Master ORDER BY DesignationName";
            da = new SqlDataAdapter(sql, con);
            dt = new DataTable();
            da.Fill(dt);
            ddlDesignation.DataSource = dt;
            ddlDesignation.DataValueField = "DesignationID";
            ddlDesignation.DataTextField = "DesignationName";
            ddlDesignation.DataBind();
            ddlDesignation.Items.Insert(0, new ListItem("-- Select --", ""));

            // Status
            sql = "SELECT StatusID, StatusName FROM EmployeeStatus_Master ORDER BY StatusName";
            da = new SqlDataAdapter(sql, con);
            dt = new DataTable();
            da.Fill(dt);
            ddlStatus.DataSource = dt;
            ddlStatus.DataValueField = "StatusID";
            ddlStatus.DataTextField = "StatusName";
            ddlStatus.DataBind();
            ddlStatus.Items.Insert(0, new ListItem("-- Select --", ""));

            // VisaStatus
            sql = "SELECT VisaStatusID, VisaStatusName FROM VisaStatus_Master ORDER BY VisaStatusName";
            da = new SqlDataAdapter(sql, con);
            dt = new DataTable();
            da.Fill(dt);
            ddlVisaStatus.DataSource = dt;
            ddlVisaStatus.DataValueField = "VisaStatusID";
            ddlVisaStatus.DataTextField = "VisaStatusName";
            ddlVisaStatus.DataBind();
            ddlVisaStatus.Items.Insert(0, new ListItem("-- Select --", ""));

            // Company
            sql = "SELECT CompanyID, CompanyName FROM Company_Master ORDER BY CompanyName";
            da = new SqlDataAdapter(sql, con);
            dt = new DataTable();
            da.Fill(dt);
            ddlCompany.DataSource = dt;
            ddlCompany.DataValueField = "CompanyID";
            ddlCompany.DataTextField = "CompanyName";
            ddlCompany.DataBind();
            ddlCompany.Items.Insert(0, new ListItem("-- Select --", ""));
        }
    }

    // ===================== Employee List Grid =====================
    private void BindGrid(string search = "")
    {
        StringBuilder sql = new StringBuilder();
        sql.AppendLine("SELECT");
        sql.AppendLine("    em.ARNo,");
        sql.AppendLine("    em.EmployeeName,");
        sql.AppendLine("    nm.NationalityName AS Nationality,");
        sql.AppendLine("    dm.DesignationName AS Designation,");
        sql.AppendLine("    esm.StatusName AS Status");
        sql.AppendLine("FROM Employee_Master em");
        sql.AppendLine("LEFT JOIN Nationality_Master nm ON em.NationalityID = nm.NationalityID");
        sql.AppendLine("LEFT JOIN Designation_Master dm ON em.DesignationID = dm.DesignationID");
        sql.AppendLine("LEFT JOIN EmployeeStatus_Master esm ON em.StatusID = esm.StatusID");
        sql.AppendLine("WHERE 1 = 1");

        if (!string.IsNullOrEmpty(search))
        {
            sql.AppendLine("    AND (em.ARNo LIKE @search OR em.EmployeeName LIKE @search)");
        }
        sql.AppendLine("ORDER BY em.EmployeeName");

        using (SqlConnection con = new SqlConnection(ConStr))
        {
            SqlDataAdapter da = new SqlDataAdapter(sql.ToString(), con);
            if (!string.IsNullOrEmpty(search))
            {
                da.SelectCommand.Parameters.AddWithValue("@search", "%" + search + "%");
            }
            DataTable dt = new DataTable();
            da.Fill(dt);
            gvEmployees.DataSource = dt;
            gvEmployees.DataBind();
        }
    }

    protected void btnSearch_Click(object sender, EventArgs e)
    {
        BindGrid(txtSearch.Text.Trim());
    }

    protected void btnRefresh_Click(object sender, EventArgs e)
    {
        txtSearch.Text = "";
        BindGrid();
    }

    protected void gvEmployees_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvEmployees.PageIndex = e.NewPageIndex;
        BindGrid(txtSearch.Text.Trim());
    }

    protected void gvEmployees_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        string arNo = e.CommandArgument.ToString();
        if (e.CommandName == "EditRow")
        {
            LoadEmployee(arNo);
            hfView.Value = "detail";
            ClientScript.RegisterStartupScript(GetType(), "showDetail", "togglePanels(false);", true);
            hfARNo.Value = arNo;
            btnSave.Visible = false;
            btnUpdate.Visible = true;
            btnDelete.Visible = true;
            txtARNo.Enabled = false;
        }
        else if (e.CommandName == "DeleteRow")
        {
            DeleteEmployee(arNo);
            BindGrid(txtSearch.Text.Trim());
        }
    }

    // ===================== Load Employee Data =====================
    private void LoadEmployee(string arNo)
    {
        using (SqlConnection con = new SqlConnection(ConStr))
        {
            con.Open();

            // Employee_Master
            string sqlMaster = "SELECT * FROM Employee_Master WHERE ARNo = @arNo";
            SqlCommand cmdMaster = new SqlCommand(sqlMaster, con);
            cmdMaster.Parameters.AddWithValue("@arNo", arNo);
            SqlDataReader dr = cmdMaster.ExecuteReader();
            if (dr.Read())
            {
                txtARNo.Text = dr["ARNo"].ToString();
                txtEmployeeName.Text = dr["EmployeeName"].ToString();
                SetDropDownValue(ddlNationality, dr["NationalityID"]);
                SetDropDownValue(ddlDesignation, dr["DesignationID"]);
                txtJoinDate.Text = dr["JoinDate"] == DBNull.Value ? "" : Convert.ToDateTime(dr["JoinDate"]).ToString("yyyy-MM-dd");
                SetDropDownValue(ddlStatus, dr["StatusID"]);
            }
            dr.Close();

            // Employee_Visa_Info
            string sqlVisa = "SELECT * FROM Employee_Visa_Info WHERE ARNo = @arNo";
            SqlCommand cmdVisa = new SqlCommand(sqlVisa, con);
            cmdVisa.Parameters.AddWithValue("@arNo", arNo);
            dr = cmdVisa.ExecuteReader();
            if (dr.Read())
            {
                SetDropDownValue(ddlVisaStatus, dr["VisaStatusID"]);
                txtPassportNo.Text = dr["PassportNo"].ToString();
                txtPassportExpiry.Text = dr["PassportExpiry"] == DBNull.Value ? "" : Convert.ToDateTime(dr["PassportExpiry"]).ToString("yyyy-MM-dd");
                chkPassportWithCompany.Checked = dr["PassportWithCompany"] != DBNull.Value && Convert.ToBoolean(dr["PassportWithCompany"]);
                txtVisitVisaExpiry.Text = dr["VisitVisaExpiry"] == DBNull.Value ? "" : Convert.ToDateTime(dr["VisitVisaExpiry"]).ToString("yyyy-MM-dd");
                txtIDNumber.Text = dr["IDNumber"].ToString();
                txtIDExpiryDate.Text = dr["IDExpiryDate"] == DBNull.Value ? "" : Convert.ToDateTime(dr["IDExpiryDate"]).ToString("yyyy-MM-dd");
                SetDropDownValue(ddlCompany, dr["VisaUnderCompanyID"]);
            }
            dr.Close();
        }

        // Load salary components (from normalized table)
        LoadSalaryComponents(arNo);
        // Load available components dropdown
        LoadAvailableComponentsDropdown(arNo);
    }

    // Salary Components 
    private void LoadSalaryComponents(string arNo)
    {
        using (SqlConnection con = new SqlConnection(ConStr))
        {
            string sql = @"
                SELECT 
                    esc.ID,
                    scm.ComponentName,
                    esc.Amount
                FROM Employee_Salary_Components esc
                INNER JOIN Salary_Component_Master scm ON esc.ComponentID = scm.ComponentID
                WHERE esc.ARNo = @arNo
                ORDER BY scm.ComponentName";
            SqlDataAdapter da = new SqlDataAdapter(sql, con);
            da.SelectCommand.Parameters.AddWithValue("@arNo", arNo);
            DataTable dt = new DataTable();
            da.Fill(dt);
            gvSalary.DataSource = dt;
            gvSalary.DataBind();
            lblTotal.Text = "";
        }
    }

    private void LoadAvailableComponentsDropdown(string arNo)
    {
        using (SqlConnection con = new SqlConnection(ConStr))
        {
            string sql = "";
            if (string.IsNullOrEmpty(arNo))
            {
                // Show all components for new employee
                sql = "SELECT ComponentID, ComponentName FROM Salary_Component_Master ORDER BY ComponentName";
            }
            else
            {
                sql = @"
                    SELECT ComponentID, ComponentName 
                    FROM Salary_Component_Master 
                    WHERE ComponentID NOT IN (
                        SELECT ComponentID FROM Employee_Salary_Components WHERE ARNo = @arNo
                    )
                    ORDER BY ComponentName";
            }
            SqlDataAdapter da = new SqlDataAdapter(sql, con);
            if (!string.IsNullOrEmpty(arNo))
            {
                da.SelectCommand.Parameters.AddWithValue("@arNo", arNo);
            }
            DataTable dt = new DataTable();
            da.Fill(dt);
            ddlComponentToAdd.DataSource = dt;
            ddlComponentToAdd.DataValueField = "ComponentID";
            ddlComponentToAdd.DataTextField = "ComponentName";
            ddlComponentToAdd.DataBind();
            ddlComponentToAdd.Items.Insert(0, new ListItem("-- Select Component --", ""));
        }
    }

    protected void gvSalary_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        string arNo = hfARNo.Value;
        if (string.IsNullOrEmpty(arNo))
        {
            ShowMessage("Please select an employee first.", true);
            return;
        }

        int id = Convert.ToInt32(e.CommandArgument);

        if (e.CommandName == "SaveRow")
        {
            // Find the row with matching ID
            foreach (GridViewRow row in gvSalary.Rows)
            {
                if (gvSalary.DataKeys[row.RowIndex].Value.ToString() == id.ToString())
                {
                    TextBox txtAmount = (TextBox)row.FindControl("txtAmount");
                    decimal amount;
                    if (!decimal.TryParse(txtAmount.Text, out amount))
                    {
                        ShowMessage("Invalid amount format.", true);
                        return;
                    }
                    UpdateSalaryComponent(id, amount);
                    break;
                }
            }
            LoadSalaryComponents(arNo);
            ShowMessage("Component updated.", false);
        }
        else if (e.CommandName == "DeleteRow")
        {
            DeleteSalaryComponent(id);
            LoadSalaryComponents(arNo);
            LoadAvailableComponentsDropdown(arNo);
            ShowMessage("Component deleted.", false);
        }
    }

    private void UpdateSalaryComponent(int id, decimal amount)
    {
        using (SqlConnection con = new SqlConnection(ConStr))
        {
            string sql = "UPDATE Employee_Salary_Components SET Amount = @amount WHERE ID = @id";
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@amount", amount);
                cmd.Parameters.AddWithValue("@id", id);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }

    private void DeleteSalaryComponent(int id)
    {
        using (SqlConnection con = new SqlConnection(ConStr))
        {
            string sql = "DELETE FROM Employee_Salary_Components WHERE ID = @id";
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@id", id);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }

    protected void btnAddComponent_Click(object sender, EventArgs e)
    {
        string arNo = hfARNo.Value;
        if (string.IsNullOrEmpty(arNo))
        {
            ShowMessage("Please select an employee first.", true);
            return;
        }

        if (string.IsNullOrEmpty(ddlComponentToAdd.SelectedValue))
        {
            ShowMessage("Please select a component to add.", true);
            return;
        }

        int componentId = Convert.ToInt32(ddlComponentToAdd.SelectedValue);

        using (SqlConnection con = new SqlConnection(ConStr))
        {
            string sql = @"
                INSERT INTO Employee_Salary_Components (ARNo, ComponentID, Amount )
                VALUES (@arNo, @compId, 0 )";
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@arNo", arNo);
                cmd.Parameters.AddWithValue("@compId", componentId);
                
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
        LoadSalaryComponents(arNo);
        LoadAvailableComponentsDropdown(arNo);
        ShowMessage("Component added with amount 0. Please edit the amount.", false);
    }

    protected void btnCalculateTotal_Click(object sender, EventArgs e)
    {
        string arNo = hfARNo.Value;
        if (string.IsNullOrEmpty(arNo))
        {
            ShowMessage("Please select an employee first.", true);
            return;
        }

        decimal total = 0;
        foreach (GridViewRow row in gvSalary.Rows)
        {
            TextBox txtAmount = (TextBox)row.FindControl("txtAmount");
            decimal amt;
            if (decimal.TryParse(txtAmount.Text, out amt))
                total += amt;
        }
        lblTotal.Text = "Total Salary: " + total.ToString("N2");
    }

    // ===================== Save / Update / Delete / Cancel =====================
    protected void btnAddNew_Click(object sender, EventArgs e)
    {
        ClearForm();
        hfView.Value = "detail";
        ClientScript.RegisterStartupScript(GetType(), "showDetail", "togglePanels(false);", true);
        // Load available components (all)
        LoadAvailableComponentsDropdown("");
    }

    protected void btnSave_Click(object sender, EventArgs e)
    {
        string arNo = txtARNo.Text.Trim();
        if (string.IsNullOrEmpty(arNo))
        {
            ShowMessage("AR No is required.", true);
            return;
        }

        using (SqlConnection con = new SqlConnection(ConStr))
        {
            con.Open();

            // Check existence
            string checkSql = "SELECT COUNT(*) FROM Employee_Master WHERE ARNo = @arNo";
            using (SqlCommand checkCmd = new SqlCommand(checkSql, con))
            {
                checkCmd.Parameters.AddWithValue("@arNo", arNo);
                int count = (int)checkCmd.ExecuteScalar();
                if (count > 0)
                {
                    ShowMessage("Employee with this AR No already exists. Use Update.", true);
                    return;
                }
            }

            // Insert Employee_Master
            string insertMaster = @"
                INSERT INTO Employee_Master
                    (ARNo, EmployeeName, NationalityID, DesignationID, JoinDate, StatusID, CreatedBy)
                VALUES
                    (@arNo, @name, @nat, @des, @join, @status, @user)";
            using (SqlCommand cmd = new SqlCommand(insertMaster, con))
            {
                cmd.Parameters.AddWithValue("@arNo", arNo);
                cmd.Parameters.AddWithValue("@name", txtEmployeeName.Text.Trim());
                cmd.Parameters.AddWithValue("@nat", GetIntOrNull(ddlNationality.SelectedValue));
                cmd.Parameters.AddWithValue("@des", GetIntOrNull(ddlDesignation.SelectedValue));
                cmd.Parameters.AddWithValue("@join", ParseDate(txtJoinDate.Text));
                cmd.Parameters.AddWithValue("@status", GetIntOrNull(ddlStatus.SelectedValue));
                cmd.Parameters.AddWithValue("@user", Page.User.Identity.Name ?? "Admin");
                cmd.ExecuteNonQuery();
            }

            // Insert Employee_Visa_Info
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
                cmd.Parameters.AddWithValue("@visaStatus", GetIntOrNull(ddlVisaStatus.SelectedValue));
                cmd.Parameters.AddWithValue("@passport", txtPassportNo.Text.Trim());
                cmd.Parameters.AddWithValue("@passExp", ParseDate(txtPassportExpiry.Text));
                cmd.Parameters.AddWithValue("@passWithCo", chkPassportWithCompany.Checked);
                cmd.Parameters.AddWithValue("@visitExp", ParseDate(txtVisitVisaExpiry.Text));
                cmd.Parameters.AddWithValue("@idNo", txtIDNumber.Text.Trim());
                cmd.Parameters.AddWithValue("@idExp", ParseDate(txtIDExpiryDate.Text));
                cmd.Parameters.AddWithValue("@company", GetIntOrNull(ddlCompany.SelectedValue));
                cmd.ExecuteNonQuery();
            }

            // Salary components are added separately – but we may need to insert them if the user added some before save.
            // We'll rely on the "Add Component" button to insert, but we need to ensure any added components are saved.
            // Since we don't have a way to save components in the grid yet, we'll insert components only when the user clicks Add Component.
            // But if the user added components before saving, they are already inserted. So nothing more to do here.
        }

        ShowMessage("Employee added successfully.", false);
        ClearForm();
        BindGrid(txtSearch.Text.Trim());
        hfView.Value = "list";
        ClientScript.RegisterStartupScript(GetType(), "backToList", "togglePanels(true);", true);
    }

    protected void btnUpdate_Click(object sender, EventArgs e)
    {
        string arNo = hfARNo.Value;
        if (string.IsNullOrEmpty(arNo))
        {
            ShowMessage("No employee selected for update.", true);
            return;
        }

        using (SqlConnection con = new SqlConnection(ConStr))
        {
            con.Open();

            // Update Employee_Master
            string updateMaster = @"
                UPDATE Employee_Master SET
                    EmployeeName = @name,
                    NationalityID = @nat,
                    DesignationID = @des,
                    JoinDate = @join,
                    StatusID = @status
                WHERE ARNo = @arNo";
            using (SqlCommand cmd = new SqlCommand(updateMaster, con))
            {
                cmd.Parameters.AddWithValue("@arNo", arNo);
                cmd.Parameters.AddWithValue("@name", txtEmployeeName.Text.Trim());
                cmd.Parameters.AddWithValue("@nat", GetIntOrNull(ddlNationality.SelectedValue));
                cmd.Parameters.AddWithValue("@des", GetIntOrNull(ddlDesignation.SelectedValue));
                cmd.Parameters.AddWithValue("@join", ParseDate(txtJoinDate.Text));
                cmd.Parameters.AddWithValue("@status", GetIntOrNull(ddlStatus.SelectedValue));
                cmd.ExecuteNonQuery();
            }

            // Update Employee_Visa_Info (delete and re-insert)
            string deleteVisa = "DELETE FROM Employee_Visa_Info WHERE ARNo = @arNo";
            using (SqlCommand cmd = new SqlCommand(deleteVisa, con))
            {
                cmd.Parameters.AddWithValue("@arNo", arNo);
                cmd.ExecuteNonQuery();
            }
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
                cmd.Parameters.AddWithValue("@visaStatus", GetIntOrNull(ddlVisaStatus.SelectedValue));
                cmd.Parameters.AddWithValue("@passport", txtPassportNo.Text.Trim());
                cmd.Parameters.AddWithValue("@passExp", ParseDate(txtPassportExpiry.Text));
                cmd.Parameters.AddWithValue("@passWithCo", chkPassportWithCompany.Checked);
                cmd.Parameters.AddWithValue("@visitExp", ParseDate(txtVisitVisaExpiry.Text));
                cmd.Parameters.AddWithValue("@idNo", txtIDNumber.Text.Trim());
                cmd.Parameters.AddWithValue("@idExp", ParseDate(txtIDExpiryDate.Text));
                cmd.Parameters.AddWithValue("@company", GetIntOrNull(ddlCompany.SelectedValue));
                cmd.ExecuteNonQuery();
            }

            // Salary components are managed separately – no changes needed here.
        }

        ShowMessage("Employee updated successfully.", false);
        ClearForm();
        BindGrid(txtSearch.Text.Trim());
        hfView.Value = "list";
        ClientScript.RegisterStartupScript(GetType(), "backToList", "togglePanels(true);", true);
    }

    protected void btnDelete_Click(object sender, EventArgs e)
    {
        string arNo = hfARNo.Value;
        if (string.IsNullOrEmpty(arNo))
        {
            ShowMessage("No employee selected for deletion.", true);
            return;
        }
        DeleteEmployee(arNo);
        ClearForm();
        BindGrid(txtSearch.Text.Trim());
        hfView.Value = "list";
        ClientScript.RegisterStartupScript(GetType(), "backToList", "togglePanels(true);", true);
    }

    private void DeleteEmployee(string arNo)
    {
        using (SqlConnection con = new SqlConnection(ConStr))
        {
            con.Open();
            // Delete child records
            string delVisa = "DELETE FROM Employee_Visa_Info WHERE ARNo = @arNo";
            using (SqlCommand cmd = new SqlCommand(delVisa, con))
            {
                cmd.Parameters.AddWithValue("@arNo", arNo);
                cmd.ExecuteNonQuery();
            }
            string delSalary = "DELETE FROM Employee_Salary_Components WHERE ARNo = @arNo";
            using (SqlCommand cmd = new SqlCommand(delSalary, con))
            {
                cmd.Parameters.AddWithValue("@arNo", arNo);
                cmd.ExecuteNonQuery();
            }
            string delMaster = "DELETE FROM Employee_Master WHERE ARNo = @arNo";
            using (SqlCommand cmd = new SqlCommand(delMaster, con))
            {
                cmd.Parameters.AddWithValue("@arNo", arNo);
                cmd.ExecuteNonQuery();
            }
        }
        ShowMessage("Employee deleted successfully.", false);
    }

    protected void btnCancel_Click(object sender, EventArgs e)
    {
        ClearForm();
        hfView.Value = "list";
        ClientScript.RegisterStartupScript(GetType(), "backToList", "togglePanels(true);", true);
    }

    // ===================== Helpers =====================
    private void SetDropDownValue(DropDownList ddl, object value)
    {
        if (value == null || value == DBNull.Value) return;
        string val = value.ToString();
        ListItem li = ddl.Items.FindByValue(val);
        if (li != null) ddl.SelectedValue = val;
    }

    private void ClearForm()
    {
        txtARNo.Text = "";
        txtEmployeeName.Text = "";
        ddlNationality.SelectedIndex = 0;
        ddlDesignation.SelectedIndex = 0;
        txtJoinDate.Text = "";
        ddlStatus.SelectedIndex = 0;
        ddlVisaStatus.SelectedIndex = 0;
        txtPassportNo.Text = "";
        txtPassportExpiry.Text = "";
        chkPassportWithCompany.Checked = false;
        txtVisitVisaExpiry.Text = "";
        txtIDNumber.Text = "";
        txtIDExpiryDate.Text = "";
        ddlCompany.SelectedIndex = 0;
        // Clear salary grid
        gvSalary.DataSource = null;
        gvSalary.DataBind();
        lblTotal.Text = "";
        ddlComponentToAdd.Items.Clear();
        ddlComponentToAdd.Items.Insert(0, new ListItem("-- Select Component --", ""));
        hfARNo.Value = "";
        txtARNo.Enabled = true;
        btnSave.Visible = true;
        btnUpdate.Visible = false;
        btnDelete.Visible = false;
    }

    private object GetIntOrNull(string val)
    {
        if (string.IsNullOrEmpty(val)) return DBNull.Value;
        int i;
        if (int.TryParse(val, out i)) return i;
        return DBNull.Value;
    }

    private object ParseDate(string dateStr)
    {
        if (string.IsNullOrWhiteSpace(dateStr)) return DBNull.Value;
        DateTime dt;
        if (DateTime.TryParse(dateStr, out dt)) return dt;
        return DBNull.Value;
    }

    private object ToDecimal(string val)
    {
        if (string.IsNullOrWhiteSpace(val)) return DBNull.Value;
        decimal d;
        if (decimal.TryParse(val, out d)) return d;
        return DBNull.Value;
    }

    private void ShowMessage(string msg, bool isError)
    {
        lblMessage.ForeColor = isError ? System.Drawing.Color.Red : System.Drawing.Color.Green;
        lblMessage.Text = msg;
        ClientScript.RegisterStartupScript(GetType(), "msg", "showMessage('" + msg.Replace("'", "\\'") + "', " + isError.ToString().ToLower() + ");", true);
    }
}