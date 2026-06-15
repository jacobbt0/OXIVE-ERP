using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net.Mail;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Masters_EmployeeSalaryDetails : System.Web.UI.Page
{
    private string ConStr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            BindEmployeeDropdown();
            ClearForm();
            hfEmpCode.Value = "";
            hfCurrentTab.Value = "working";
        }
    }

    private void BindEmployeeDropdown()
    {
        try
        {
            using (SqlConnection con = new SqlConnection(ConStr))
            {
                string query = "SELECT EmpCode, EmployeeName FROM Employee_Info ORDER BY EmpCode";
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                ddlEmployee.DataSource = dt;
                ddlEmployee.DataValueField = "EmpCode";
                ddlEmployee.DataTextField = "EmployeeName";
                ddlEmployee.DataBind();
                ddlEmployee.Items.Insert(0, new ListItem("-- Select Employee --", ""));
            }
        }
        catch (Exception ex)
        {
            ShowMessage("Error loading employees: " + ex.Message, true);
        }
    }

    protected void ddlEmployee_SelectedIndexChanged(object sender, EventArgs e)
    {
        string empCode = ddlEmployee.SelectedValue;
        if (!string.IsNullOrEmpty(empCode))
            LoadEmployeeData(empCode);
        else
            ClearForm();
    }

    protected void btnSearch_Click(object sender, EventArgs e)
    {
        string empCode = ddlEmployee.SelectedValue;
        if (!string.IsNullOrEmpty(empCode))
            LoadEmployeeData(empCode);
        else
            ShowMessage("Please select an employee.", true);
    }

    private void LoadEmployeeData(string empCode)
    {
        hfEmpCode.Value = empCode;
        using (SqlConnection con = new SqlConnection(ConStr))
        {
            string query = "SELECT * FROM EmpSalary_Master WHERE EmpCode = @code";
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@code", empCode);
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    txtWorkingLocation.Text = dr["WorkingLocation"] == DBNull.Value ? "" : dr["WorkingLocation"].ToString();
                    txtDivision.Text = dr["Division"] == DBNull.Value ? "" : dr["Division"].ToString();

                    string empStatus = dr["EmployeeStatus"] == DBNull.Value ? "Active" : dr["EmployeeStatus"].ToString();
                    ddlEmpStatus.SelectedValue = empStatus;

                    txtJoinDate.Text = dr["JoinDate"] == DBNull.Value ? "" : Convert.ToDateTime(dr["JoinDate"]).ToString("yyyy-MM-dd");
                    txtJoinType.Text = dr["JoinType"] == DBNull.Value ? "" : dr["JoinType"].ToString();

                    string contractType = dr["ContractType"] == DBNull.Value ? "UNLIMITED" : dr["ContractType"].ToString();
                    ddlContractType.SelectedValue = contractType;

                    txtLastProvisionDate.Text = dr["LastProvisionDate"] == DBNull.Value ? "" : Convert.ToDateTime(dr["LastProvisionDate"]).ToString("yyyy-MM-dd");
                    txtNormalWorkingHrs.Text = dr["NormalWorkingHrs"] == DBNull.Value ? "" : dr["NormalWorkingHrs"].ToString();
                    txtBreakHours.Text = dr["BreakHours"] == DBNull.Value ? "" : dr["BreakHours"].ToString();
                    txtOvertimeRate.Text = dr["EmpOvertimeRate"] == DBNull.Value ? "" : dr["EmpOvertimeRate"].ToString();
                    txtIncentiveRate.Text = dr["EmpIncentiveRate"] == DBNull.Value ? "" : dr["EmpIncentiveRate"].ToString();
                    txtLastESOBDate.Text = dr["LastESOBDate"] == DBNull.Value ? "" : Convert.ToDateTime(dr["LastESOBDate"]).ToString("yyyy-MM-dd");
                    txtReAgreementDate.Text = dr["ReAgreementDate"] == DBNull.Value ? "" : Convert.ToDateTime(dr["ReAgreementDate"]).ToString("yyyy-MM-dd");
                    txtEmployeeType.Text = dr["EmployeeType"] == DBNull.Value ? "" : dr["EmployeeType"].ToString();
                    txtGradeSub.Text = dr["GradeSub"] == DBNull.Value ? "" : dr["GradeSub"].ToString();

                    chkFoodAllowancePaid.Checked = dr["FoodAllowancePaidByComp"] != DBNull.Value && Convert.ToBoolean(dr["FoodAllowancePaidByComp"]);
                    txtFoodAllowanceDed.Text = dr["FoodAllowanceDedAmount"] == DBNull.Value ? "" : dr["FoodAllowanceDedAmount"].ToString();
                    txtTripEmpFixedSalary.Text = dr["TripEmpFixedSalary"] == DBNull.Value ? "" : dr["TripEmpFixedSalary"].ToString();
                    txtAirSector.Text = dr["AirSector"] == DBNull.Value ? "" : dr["AirSector"].ToString();
                    txtAirTicketInterval.Text = dr["AirTicketInterval"] == DBNull.Value ? "" : dr["AirTicketInterval"].ToString();
                    txtAirTicketTotalCount.Text = dr["AirTicketTotalCount"] == DBNull.Value ? "" : dr["AirTicketTotalCount"].ToString();
                    txtNoChildTicket.Text = dr["NoChildTicket"] == DBNull.Value ? "" : dr["NoChildTicket"].ToString();
                    txtNoInfantTicket.Text = dr["NoInfantTicket"] == DBNull.Value ? "" : dr["NoInfantTicket"].ToString();

                    chkPensions.Checked = dr["Pensions"] != DBNull.Value && Convert.ToBoolean(dr["Pensions"]);
                    chkExtraBenefit.Checked = dr["ExtraBenefitAmtYN"] != DBNull.Value && Convert.ToBoolean(dr["ExtraBenefitAmtYN"]);

                    txtRemarks.Text = dr["Remarks"] == DBNull.Value ? "" : dr["Remarks"].ToString();
                }
                else
                {
                    ClearForm();
                    ShowMessage("No salary data found for this employee. You can add new.", false);
                }
                dr.Close();
            }
        }
        BindSalaryComponents(empCode);
        BindProvisionAccounts(empCode);
        BindAttachments(empCode);
    }

    private void BindSalaryComponents(string empCode)
    {
        using (SqlConnection con = new SqlConnection(ConStr))
        {
            string query = "SELECT * FROM EmpSalary_Components WHERE EmpCode = @code ORDER BY ID";
            SqlDataAdapter da = new SqlDataAdapter(query, con);
            da.SelectCommand.Parameters.AddWithValue("@code", empCode);
            DataTable dt = new DataTable();
            da.Fill(dt);
            gvSalaryComponents.DataSource = dt;
            gvSalaryComponents.DataBind();
        }
    }

    private void BindProvisionAccounts(string empCode)
    {
        using (SqlConnection con = new SqlConnection(ConStr))
        {
            string query = "SELECT * FROM EmpProvision_Accounts WHERE EmpCode = @code ORDER BY ID";
            SqlDataAdapter da = new SqlDataAdapter(query, con);
            da.SelectCommand.Parameters.AddWithValue("@code", empCode);
            DataTable dt = new DataTable();
            da.Fill(dt);
            gvProvision.DataSource = dt;
            gvProvision.DataBind();
        }
    }

    private void BindAttachments(string empCode)
    {
        using (SqlConnection con = new SqlConnection(ConStr))
        {
            string query = "SELECT ID, FileName, Remarks FROM EmpSalary_Attachments WHERE EmpCode = @code ORDER BY ID";
            SqlDataAdapter da = new SqlDataAdapter(query, con);
            da.SelectCommand.Parameters.AddWithValue("@code", empCode);
            DataTable dt = new DataTable();
            da.Fill(dt);
            gvAttachments.DataSource = dt;
            gvAttachments.DataBind();
        }
    }

    protected void btnAddSalaryRow_Click(object sender, EventArgs e)
    {
        DataTable dt = GetSalaryComponentTable();
        dt.Rows.Add(null, "", "", "", "", "", "", "", "", "", "", "", "");
        gvSalaryComponents.DataSource = dt;
        gvSalaryComponents.DataBind();
        ViewState["SalaryComponents"] = dt;
    }

    private DataTable GetSalaryComponentTable()
    {
        DataTable dt = new DataTable();
        dt.Columns.Add("ID", typeof(int));
        dt.Columns.Add("SalaryType", typeof(string));
        dt.Columns.Add("AccountCode", typeof(string));
        dt.Columns.Add("AccountName", typeof(string));
        dt.Columns.Add("DailyAmount", typeof(decimal));
        dt.Columns.Add("HourlyAmount", typeof(decimal));
        dt.Columns.Add("SalaryN", typeof(decimal));
        dt.Columns.Add("LedgerType", typeof(string));
        dt.Columns.Add("DeductionType", typeof(string));
        dt.Columns.Add("EmployerDrAc", typeof(string));
        dt.Columns.Add("EmployerDrAcName", typeof(string));
        dt.Columns.Add("EmployerCrAc", typeof(string));
        dt.Columns.Add("EmployerCrAcName", typeof(string));
        return dt;
    }

    protected void gvSalaryComponents_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName == "RemoveRow")
        {
            int id = Convert.ToInt32(e.CommandArgument);
            if (id > 0)
            {
                using (SqlConnection con = new SqlConnection(ConStr))
                {
                    string del = "DELETE FROM EmpSalary_Components WHERE ID=@id";
                    using (SqlCommand cmd = new SqlCommand(del, con))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            BindSalaryComponents(hfEmpCode.Value);
        }
    }

    protected void btnAddProvisionRow_Click(object sender, EventArgs e)
    {
        DataTable dt = GetProvisionTable();
        dt.Rows.Add(null, "", "", "", "", "");
        gvProvision.DataSource = dt;
        gvProvision.DataBind();
        ViewState["ProvisionAccounts"] = dt;
    }

    private DataTable GetProvisionTable()
    {
        DataTable dt = new DataTable();
        dt.Columns.Add("ID", typeof(int));
        dt.Columns.Add("ProvisionType", typeof(string));
        dt.Columns.Add("ProvisionalAccount", typeof(string));
        dt.Columns.Add("ProAccountName", typeof(string));
        dt.Columns.Add("ExpenseAccount", typeof(string));
        dt.Columns.Add("ExpAccountName", typeof(string));
        return dt;
    }

    protected void gvProvision_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName == "RemoveProv")
        {
            int id = Convert.ToInt32(e.CommandArgument);
            if (id > 0)
            {
                using (SqlConnection con = new SqlConnection(ConStr))
                {
                    string del = "DELETE FROM EmpProvision_Accounts WHERE ID=@id";
                    using (SqlCommand cmd = new SqlCommand(del, con))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            BindProvisionAccounts(hfEmpCode.Value);
        }
    }

    protected void btnUploadAttachment_Click(object sender, EventArgs e)
    {
        string empCode = hfEmpCode.Value;
        if (string.IsNullOrEmpty(empCode))
        {
            ShowMessage("Please select an employee first.", true);
            return;
        }
        if (!fuAttachment.HasFile)
        {
            ShowMessage("Please choose a file.", true);
            return;
        }
        string folder = Server.MapPath("~/App_Data/EmployeeSalaryAttachments/" + empCode + "/");
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
        string fileName = DateTime.Now.Ticks + "_" + Path.GetFileName(fuAttachment.FileName);
        string filePath = Path.Combine(folder, fileName);
        fuAttachment.SaveAs(filePath);
        string virtualPath = "~/App_Data/EmployeeSalaryAttachments/" + empCode + "/" + fileName;

        using (SqlConnection con = new SqlConnection(ConStr))
        {
            string sql = "INSERT INTO EmpSalary_Attachments (EmpCode, FileName, FilePath, Remarks, UploadedBy) VALUES (@ec, @fn, @fp, @rm, @ub)";
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@ec", empCode);
                cmd.Parameters.AddWithValue("@fn", fileName);
                cmd.Parameters.AddWithValue("@fp", virtualPath);
                cmd.Parameters.AddWithValue("@rm", txtAttachRemarks.Text.Trim());
                cmd.Parameters.AddWithValue("@ub", Page.User.Identity.Name ?? "Admin");
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
        txtAttachRemarks.Text = "";
        BindAttachments(empCode);
        ShowMessage("File uploaded successfully.", false);
    }


    protected void gvAttachments_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName == "DeleteAtt")
        {
            int id = Convert.ToInt32(e.CommandArgument);
            using (SqlConnection con = new SqlConnection(ConStr))
            {
                string getPath = "SELECT FilePath FROM EmpSalary_Attachments WHERE ID=@id";
                using (SqlCommand cmd = new SqlCommand(getPath, con))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    con.Open();
                    object result = cmd.ExecuteScalar();
                    string path = result == null ? "" : result.ToString();
                    if (!string.IsNullOrEmpty(path))
                    {
                        string physical = Server.MapPath(path);
                        if (File.Exists(physical)) File.Delete(physical);
                    }
                }
                string del = "DELETE FROM EmpSalary_Attachments WHERE ID=@id";
                using (SqlCommand cmd = new SqlCommand(del, con))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
            BindAttachments(hfEmpCode.Value);
        }
    }


    protected void btnSave_Click(object sender, EventArgs e)
    {
        string empCode = hfEmpCode.Value;
        if (string.IsNullOrEmpty(empCode))
        {
            ShowMessage("Please select an employee.", true);
            return;
        }

        using (SqlConnection con = new SqlConnection(ConStr))
        {
            con.Open();
            string check = "SELECT COUNT(*) FROM EmpSalary_Master WHERE EmpCode=@code";
            using (SqlCommand chk = new SqlCommand(check, con))
            {
                chk.Parameters.AddWithValue("@code", empCode);
                int exists = (int)chk.ExecuteScalar();
                if (exists > 0)
                {
                    ShowMessage("Record already exists. Use Update instead.", true);
                    return;
                }
            }

            string insertHeader = @"
                INSERT INTO EmpSalary_Master
                (EmpCode, JoinDate, JoinType, ContractType, LastProvisionDate, NormalWorkingHrs, BreakHours,
                 EmpOvertimeRate, EmpIncentiveRate, LastESOBDate, ReAgreementDate, EmployeeType, Division,
                 GradeSub, EmployeeStatus, AirSector, AirTicketInterval, AirTicketTotalCount, NoChildTicket,
                 NoInfantTicket, Pensions, ExtraBenefitAmtYN, FoodAllowancePaidByComp, FoodAllowanceDedAmount,
                 TripEmpFixedSalary, Remarks, CreatedBy)
                VALUES
                (@code, @jd, @jt, @ct, @lpd, @nwh, @bh, @eor, @eir, @lesob, @rad, @et, @div, @gs, @es,
                 @airsec, @airint, @airtot, @child, @infant, @pen, @extra, @foodpaid, @foodded, @tripsal,
                 @rem, @cb)";
            using (SqlCommand cmd = new SqlCommand(insertHeader, con))
            {
                cmd.Parameters.AddWithValue("@code", empCode);
                cmd.Parameters.AddWithValue("@jd", ParseDate(txtJoinDate.Text));
                cmd.Parameters.AddWithValue("@jt", txtJoinType.Text.Trim());
                cmd.Parameters.AddWithValue("@ct", ddlContractType.SelectedValue);
                cmd.Parameters.AddWithValue("@lpd", ParseDate(txtLastProvisionDate.Text));
                cmd.Parameters.AddWithValue("@nwh", ToDecimal(txtNormalWorkingHrs.Text));
                cmd.Parameters.AddWithValue("@bh", ToDecimal(txtBreakHours.Text));
                cmd.Parameters.AddWithValue("@eor", ToDecimal(txtOvertimeRate.Text));
                cmd.Parameters.AddWithValue("@eir", ToDecimal(txtIncentiveRate.Text));
                cmd.Parameters.AddWithValue("@lesob", ParseDate(txtLastESOBDate.Text));
                cmd.Parameters.AddWithValue("@rad", ParseDate(txtReAgreementDate.Text));
                cmd.Parameters.AddWithValue("@et", txtEmployeeType.Text.Trim());
                cmd.Parameters.AddWithValue("@div", txtDivision.Text.Trim());
                cmd.Parameters.AddWithValue("@gs", txtGradeSub.Text.Trim());
                cmd.Parameters.AddWithValue("@es", ddlEmpStatus.SelectedValue);
                cmd.Parameters.AddWithValue("@airsec", txtAirSector.Text.Trim());
                cmd.Parameters.AddWithValue("@airint", ToInt(txtAirTicketInterval.Text));
                cmd.Parameters.AddWithValue("@airtot", ToInt(txtAirTicketTotalCount.Text));
                cmd.Parameters.AddWithValue("@child", ToInt(txtNoChildTicket.Text));
                cmd.Parameters.AddWithValue("@infant", ToInt(txtNoInfantTicket.Text));
                cmd.Parameters.AddWithValue("@pen", chkPensions.Checked);
                cmd.Parameters.AddWithValue("@extra", chkExtraBenefit.Checked);
                cmd.Parameters.AddWithValue("@foodpaid", chkFoodAllowancePaid.Checked);
                cmd.Parameters.AddWithValue("@foodded", ToDecimal(txtFoodAllowanceDed.Text));
                cmd.Parameters.AddWithValue("@tripsal", ToDecimal(txtTripEmpFixedSalary.Text));
                cmd.Parameters.AddWithValue("@rem", txtRemarks.Text.Trim());
                cmd.Parameters.AddWithValue("@cb", Page.User.Identity.Name ?? "Admin");
                cmd.ExecuteNonQuery();
            }

            SaveSalaryComponents(con, empCode);
            SaveProvisionAccounts(con, empCode);
        }

        ShowMessage("Salary details saved successfully.", false);
        LoadEmployeeData(empCode);
    }

    protected void btnUpdate_Click(object sender, EventArgs e)
    {
        string empCode = hfEmpCode.Value;
        if (string.IsNullOrEmpty(empCode))
        {
            ShowMessage("Please select an employee.", true);
            return;
        }

        using (SqlConnection con = new SqlConnection(ConStr))
        {
            con.Open();
            string updateHeader = @"
                UPDATE EmpSalary_Master SET
                JoinDate=@jd, JoinType=@jt, ContractType=@ct, LastProvisionDate=@lpd, NormalWorkingHrs=@nwh,
                BreakHours=@bh, EmpOvertimeRate=@eor, EmpIncentiveRate=@eir, LastESOBDate=@lesob,
                ReAgreementDate=@rad, EmployeeType=@et, Division=@div, GradeSub=@gs, EmployeeStatus=@es,
                AirSector=@airsec, AirTicketInterval=@airint, AirTicketTotalCount=@airtot, NoChildTicket=@child,
                NoInfantTicket=@infant, Pensions=@pen, ExtraBenefitAmtYN=@extra, FoodAllowancePaidByComp=@foodpaid,
                FoodAllowanceDedAmount=@foodded, TripEmpFixedSalary=@tripsal, Remarks=@rem, ModifiedBy=@mb,
                ModifiedDate=GETDATE()
                WHERE EmpCode=@code";
            using (SqlCommand cmd = new SqlCommand(updateHeader, con))
            {
                cmd.Parameters.AddWithValue("@code", empCode);
                cmd.Parameters.AddWithValue("@jd", ParseDate(txtJoinDate.Text));
                cmd.Parameters.AddWithValue("@jt", txtJoinType.Text.Trim());
                cmd.Parameters.AddWithValue("@ct", ddlContractType.SelectedValue);
                cmd.Parameters.AddWithValue("@lpd", ParseDate(txtLastProvisionDate.Text));
                cmd.Parameters.AddWithValue("@nwh", ToDecimal(txtNormalWorkingHrs.Text));
                cmd.Parameters.AddWithValue("@bh", ToDecimal(txtBreakHours.Text));
                cmd.Parameters.AddWithValue("@eor", ToDecimal(txtOvertimeRate.Text));
                cmd.Parameters.AddWithValue("@eir", ToDecimal(txtIncentiveRate.Text));
                cmd.Parameters.AddWithValue("@lesob", ParseDate(txtLastESOBDate.Text));
                cmd.Parameters.AddWithValue("@rad", ParseDate(txtReAgreementDate.Text));
                cmd.Parameters.AddWithValue("@et", txtEmployeeType.Text.Trim());
                cmd.Parameters.AddWithValue("@div", txtDivision.Text.Trim());
                cmd.Parameters.AddWithValue("@gs", txtGradeSub.Text.Trim());
                cmd.Parameters.AddWithValue("@es", ddlEmpStatus.SelectedValue);
                cmd.Parameters.AddWithValue("@airsec", txtAirSector.Text.Trim());
                cmd.Parameters.AddWithValue("@airint", ToInt(txtAirTicketInterval.Text));
                cmd.Parameters.AddWithValue("@airtot", ToInt(txtAirTicketTotalCount.Text));
                cmd.Parameters.AddWithValue("@child", ToInt(txtNoChildTicket.Text));
                cmd.Parameters.AddWithValue("@infant", ToInt(txtNoInfantTicket.Text));
                cmd.Parameters.AddWithValue("@pen", chkPensions.Checked);
                cmd.Parameters.AddWithValue("@extra", chkExtraBenefit.Checked);
                cmd.Parameters.AddWithValue("@foodpaid", chkFoodAllowancePaid.Checked);
                cmd.Parameters.AddWithValue("@foodded", ToDecimal(txtFoodAllowanceDed.Text));
                cmd.Parameters.AddWithValue("@tripsal", ToDecimal(txtTripEmpFixedSalary.Text));
                cmd.Parameters.AddWithValue("@rem", txtRemarks.Text.Trim());
                cmd.Parameters.AddWithValue("@mb", Page.User.Identity.Name ?? "Admin");
                cmd.ExecuteNonQuery();
            }

            using (SqlCommand delComp = new SqlCommand("DELETE FROM EmpSalary_Components WHERE EmpCode=@code", con))
            { delComp.Parameters.AddWithValue("@code", empCode); delComp.ExecuteNonQuery(); }
            SaveSalaryComponents(con, empCode);

            using (SqlCommand delProv = new SqlCommand("DELETE FROM EmpProvision_Accounts WHERE EmpCode=@code", con))
            { delProv.Parameters.AddWithValue("@code", empCode); delProv.ExecuteNonQuery(); }
            SaveProvisionAccounts(con, empCode);
        }

        ShowMessage("Salary details updated successfully.", false);
        LoadEmployeeData(empCode);
    }

    private void SaveSalaryComponents(SqlConnection con, string empCode)
    {
        foreach (GridViewRow row in gvSalaryComponents.Rows)
        {
            TextBox txtSalType = (TextBox)row.FindControl("txtSalType");
            TextBox txtAccCode = (TextBox)row.FindControl("txtAccCode");
            TextBox txtAccName = (TextBox)row.FindControl("txtAccName");
            TextBox txtDaily = (TextBox)row.FindControl("txtDaily");
            TextBox txtHourly = (TextBox)row.FindControl("txtHourly");
            TextBox txtSalaryN = (TextBox)row.FindControl("txtSalaryN");
            TextBox txtLedgerType = (TextBox)row.FindControl("txtLedgerType");
            TextBox txtDedType = (TextBox)row.FindControl("txtDedType");
            TextBox txtEmpDrAc = (TextBox)row.FindControl("txtEmpDrAc");
            TextBox txtEmpDrName = (TextBox)row.FindControl("txtEmpDrName");
            TextBox txtEmpCrAc = (TextBox)row.FindControl("txtEmpCrAc");
            TextBox txtEmpCrName = (TextBox)row.FindControl("txtEmpCrName");

            if (string.IsNullOrWhiteSpace(txtSalType.Text)) continue;

            string sql = @"INSERT INTO EmpSalary_Components
                (EmpCode, SalaryType, AccountCode, AccountName, DailyAmount, HourlyAmount, SalaryN,
                 LedgerType, DeductionType, EmployerDrAc, EmployerDrAcName, EmployerCrAc, EmployerCrAcName)
                VALUES (@ec, @st, @ac, @an, @da, @ha, @sn, @lt, @dt, @eda, @edan, @eca, @ecan)";
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@ec", empCode);
                cmd.Parameters.AddWithValue("@st", txtSalType.Text.Trim());
                cmd.Parameters.AddWithValue("@ac", txtAccCode.Text.Trim());
                cmd.Parameters.AddWithValue("@an", txtAccName.Text.Trim());
                cmd.Parameters.AddWithValue("@da", ToDecimal(txtDaily.Text));
                cmd.Parameters.AddWithValue("@ha", ToDecimal(txtHourly.Text));
                cmd.Parameters.AddWithValue("@sn", ToDecimal(txtSalaryN.Text));
                cmd.Parameters.AddWithValue("@lt", txtLedgerType.Text.Trim());
                cmd.Parameters.AddWithValue("@dt", txtDedType.Text.Trim());
                cmd.Parameters.AddWithValue("@eda", txtEmpDrAc.Text.Trim());
                cmd.Parameters.AddWithValue("@edan", txtEmpDrName.Text.Trim());
                cmd.Parameters.AddWithValue("@eca", txtEmpCrAc.Text.Trim());
                cmd.Parameters.AddWithValue("@ecan", txtEmpCrName.Text.Trim());
                cmd.ExecuteNonQuery();
            }
        }
    }

    private void SaveProvisionAccounts(SqlConnection con, string empCode)
    {
        foreach (GridViewRow row in gvProvision.Rows)
        {
            TextBox txtProvType = (TextBox)row.FindControl("txtProvType");
            TextBox txtProvAcc = (TextBox)row.FindControl("txtProvAcc");
            TextBox txtProvAccName = (TextBox)row.FindControl("txtProvAccName");
            TextBox txtExpAcc = (TextBox)row.FindControl("txtExpAcc");
            TextBox txtExpAccName = (TextBox)row.FindControl("txtExpAccName");

            if (string.IsNullOrWhiteSpace(txtProvType.Text)) continue;

            string sql = @"INSERT INTO EmpProvision_Accounts
                (EmpCode, ProvisionType, ProvisionalAccount, ProAccountName, ExpenseAccount, ExpAccountName)
                VALUES (@ec, @pt, @pa, @pan, @ea, @ean)";
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@ec", empCode);
                cmd.Parameters.AddWithValue("@pt", txtProvType.Text.Trim());
                cmd.Parameters.AddWithValue("@pa", txtProvAcc.Text.Trim());
                cmd.Parameters.AddWithValue("@pan", txtProvAccName.Text.Trim());
                cmd.Parameters.AddWithValue("@ea", txtExpAcc.Text.Trim());
                cmd.Parameters.AddWithValue("@ean", txtExpAccName.Text.Trim());
                cmd.ExecuteNonQuery();
            }
        }
    }

    protected void btnClear_Click(object sender, EventArgs e)
    {
        ClearForm();
        hfEmpCode.Value = "";
        ddlEmployee.SelectedIndex = 0;
        gvSalaryComponents.DataSource = null; gvSalaryComponents.DataBind();
        gvProvision.DataSource = null; gvProvision.DataBind();
        gvAttachments.DataSource = null; gvAttachments.DataBind();
    }

    protected void btnClearForm_Click(object sender, EventArgs e)
    {
        ClearForm();
    }

    private void ClearForm()
    {
        txtWorkingLocation.Text = "";
        txtDivision.Text = "";
        ddlEmpStatus.SelectedIndex = 0;
        txtJoinDate.Text = "";
        txtJoinType.Text = "";
        ddlContractType.SelectedIndex = 0;
        txtLastProvisionDate.Text = "";
        txtNormalWorkingHrs.Text = "";
        txtBreakHours.Text = "";
        txtOvertimeRate.Text = "";
        txtIncentiveRate.Text = "";
        txtLastESOBDate.Text = "";
        txtReAgreementDate.Text = "";
        txtEmployeeType.Text = "";
        txtGradeSub.Text = "";
        chkFoodAllowancePaid.Checked = false;
        txtFoodAllowanceDed.Text = "";
        txtTripEmpFixedSalary.Text = "";
        txtAirSector.Text = "";
        txtAirTicketInterval.Text = "";
        txtAirTicketTotalCount.Text = "";
        txtNoChildTicket.Text = "";
        txtNoInfantTicket.Text = "";
        chkPensions.Checked = false;
        chkExtraBenefit.Checked = false;
        txtRemarks.Text = "";
        txtAttachRemarks.Text = "";
    }

    protected void btnPrint_Click(object sender, EventArgs e)
    {
        Page.ClientScript.RegisterStartupScript(GetType(), "print", "window.print();", true);
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

    private object ToInt(string val)
    {
        if (string.IsNullOrWhiteSpace(val)) return DBNull.Value;
        int i;
        if (int.TryParse(val, out i)) return i;
        return DBNull.Value;
    }

    private void ShowMessage(string msg, bool isError)
    {
        lblMessage.ForeColor = isError ? System.Drawing.Color.Red : System.Drawing.Color.Green;
        lblMessage.Text = msg;
        ClientScript.RegisterStartupScript(GetType(), "msg", "showMessage('" + msg.Replace("'", "\\'") + "', " + isError.ToString().ToLower() + ");", true);
    }
}