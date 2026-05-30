using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Masters_SubGroupMaster : System.Web.UI.Page
{
    public string savedMessage = "";

    private string ConStr
    {
        get { return ConfigurationManager.ConnectionStrings["constr"].ConnectionString; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        // SubGroupCode is IDENTITY (auto-generated) — always disable the field and its validator
        txtSubGroupCode.Enabled = false;
        rfvSubCode.Enabled = false;

        if (!IsPostBack)
        {
            LoadGroupDropdown();
            BindGrid();
        }
    }

    // ─── Dropdown ────────────────────────────────────────────────────────────

    private void LoadGroupDropdown()
    {
        ddlGroupCode.Items.Clear();
        ddlGroupCode.Items.Add(new ListItem("-- Select Group --", ""));

        try
        {
            using (SqlConnection con = new SqlConnection(ConStr))
            {
                string query = "SELECT CategoryCode, CategoryDescription FROM Group_Master ORDER BY CategoryDescription";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        string code = dr["CategoryCode"].ToString();
                        string desc = dr["CategoryDescription"].ToString();
                        ddlGroupCode.Items.Add(new ListItem(code + " - " + desc, code));
                    }
                    dr.Close();
                }
            }
        }
        catch (Exception ex)
        {
            RegisterAlert("Error loading group list: " + ex.Message);
        }
    }

    // ─── Grid ────────────────────────────────────────────────────────────────

    private void BindGrid()
    {
        var list = new List<SubGroupItem>();
        try
        {
            using (SqlConnection con = new SqlConnection(ConStr))
            {
                string query = @"SELECT SubGroupCode, GroupCode, SubGroupDesc,
                                        PrecastCodePattern, ItemCodingGroup, Remarks,
                                        BudgetCode, BudgetDesc,
                                        InvAccCode, InvAccDesc,
                                        CostAccCode, CostAccDesc,
                                        IncomeAccCode, IncomeAccDesc
                                 FROM SubGroup_Master
                                 ORDER BY GroupCode, SubGroupDesc";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        list.Add(new SubGroupItem
                        {
                            SubGroupCode = dr["SubGroupCode"].ToString(),
                            GroupCode = dr["GroupCode"].ToString(),
                            SubGroupDesc = dr["SubGroupDesc"].ToString(),
                            PrecastCodePattern = dr["PrecastCodePattern"] != DBNull.Value && (bool)dr["PrecastCodePattern"],
                            ItemCodingGroup = dr["ItemCodingGroup"].ToString(),
                            Remarks = dr["Remarks"].ToString(),
                            BudgetCode = dr["BudgetCode"].ToString(),
                            BudgetDesc = dr["BudgetDesc"].ToString(),
                            InvAccCode = dr["InvAccCode"].ToString(),
                            InvAccDesc = dr["InvAccDesc"].ToString(),
                            CostAccCode = dr["CostAccCode"].ToString(),
                            CostAccDesc = dr["CostAccDesc"].ToString(),
                            IncomeAccCode = dr["IncomeAccCode"].ToString(),
                            IncomeAccDesc = dr["IncomeAccDesc"].ToString()
                        });
                    }
                    dr.Close();
                }
            }
        }
        catch (Exception ex)
        {
            RegisterAlert("Error loading data: " + ex.Message);
        }

        gvSubGroup.DataSource = list;
        gvSubGroup.DataBind();
    }

    // ─── Save ────────────────────────────────────────────────────────────────

    protected void btnSave_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid) return;

        // Step 1: Unique description check
        try
        {
            using (SqlConnection con = new SqlConnection(ConStr))
            {
                string checkQuery = "SELECT COUNT(*) FROM SubGroup_Master WHERE SubGroupDesc = @desc";
                using (SqlCommand cmd = new SqlCommand(checkQuery, con))
                {
                    cmd.Parameters.AddWithValue("@desc", txtSubGroupDesc.Text.Trim());
                    con.Open();
                    int count = (int)cmd.ExecuteScalar();
                    if (count > 0)
                    {
                        RegisterAlert("Sub Group Description already exists!");
                        return;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            RegisterAlert("Error checking description: " + ex.Message);
            return;
        }

        // Step 2: Insert (SubGroupCode is IDENTITY — not included)
        try
        {
            using (SqlConnection con = new SqlConnection(ConStr))
            {
                string insertQuery = @"INSERT INTO SubGroup_Master
                    (GroupCode, SubGroupDesc, PrecastCodePattern, ItemCodingGroup, Remarks,
                     BudgetCode, BudgetDesc,
                     InvAccCode, InvAccDesc, CostAccCode, CostAccDesc,
                     IncomeAccCode, IncomeAccDesc)
                    VALUES
                    (@groupCode, @desc, @precast, @itemCoding, @remarks,
                     @budgetCode, @budgetDesc,
                     @invAccCode, @invAccDesc, @costAccCode, @costAccDesc,
                     @incomeAccCode, @incomeAccDesc)";

                using (SqlCommand cmd = new SqlCommand(insertQuery, con))
                {
                    AddSubGroupParams(cmd);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            savedMessage = "Sub Group saved successfully!";
            ClearForm();
            BindGrid();
        }
        catch (Exception ex)
        {
            RegisterAlert("Error saving sub group: " + ex.Message);
        }
    }

    // ─── Update ──────────────────────────────────────────────────────────────

    protected void btnUpdate_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid) return;
        string subCode = hfEditCode.Value;
        if (string.IsNullOrEmpty(subCode)) return;

        // Step 1: Unique description check (exclude current record)
        try
        {
            using (SqlConnection con = new SqlConnection(ConStr))
            {
                string checkQuery = "SELECT COUNT(*) FROM SubGroup_Master WHERE SubGroupDesc = @desc AND SubGroupCode <> @subCode";
                using (SqlCommand cmd = new SqlCommand(checkQuery, con))
                {
                    cmd.Parameters.AddWithValue("@desc", txtSubGroupDesc.Text.Trim());
                    cmd.Parameters.AddWithValue("@subCode", subCode);
                    con.Open();
                    int count = (int)cmd.ExecuteScalar();
                    if (count > 0)
                    {
                        RegisterAlert("Sub Group Description already exists!");
                        return;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            RegisterAlert("Error checking description: " + ex.Message);
            return;
        }

        // Step 2: Update
        try
        {
            using (SqlConnection con = new SqlConnection(ConStr))
            {
                string updateQuery = @"UPDATE SubGroup_Master SET
                    GroupCode          = @groupCode,
                    SubGroupDesc       = @desc,
                    PrecastCodePattern = @precast,
                    ItemCodingGroup    = @itemCoding,
                    Remarks            = @remarks,
                    BudgetCode         = @budgetCode,
                    BudgetDesc         = @budgetDesc,
                    InvAccCode         = @invAccCode,
                    InvAccDesc         = @invAccDesc,
                    CostAccCode        = @costAccCode,
                    CostAccDesc        = @costAccDesc,
                    IncomeAccCode      = @incomeAccCode,
                    IncomeAccDesc      = @incomeAccDesc
                    WHERE SubGroupCode = @subCode";

                using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                {
                    AddSubGroupParams(cmd);
                    cmd.Parameters.AddWithValue("@subCode", subCode);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            savedMessage = "Sub Group updated successfully!";
            ClearForm();
            BindGrid();
        }
        catch (Exception ex)
        {
            RegisterAlert("Error updating sub group: " + ex.Message);
        }
    }

    // ─── Delete ──────────────────────────────────────────────────────────────

    protected void btnDelete_Click(object sender, EventArgs e)
    {
        string subCode = hfEditCode.Value;
        if (string.IsNullOrEmpty(subCode)) return;

        try
        {
            using (SqlConnection con = new SqlConnection(ConStr))
            {
                string deleteQuery = "DELETE FROM SubGroup_Master WHERE SubGroupCode = @subCode";
                using (SqlCommand cmd = new SqlCommand(deleteQuery, con))
                {
                    cmd.Parameters.AddWithValue("@subCode", subCode);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            savedMessage = "Sub Group deleted.";
            ClearForm();
            BindGrid();
        }
        catch (Exception ex)
        {
            RegisterAlert("Error deleting sub group: " + ex.Message);
        }
    }

    protected void btnClear_Click(object sender, EventArgs e)
    {
        ClearForm();
    }

    // ─── Grid Row Commands ───────────────────────────────────────────────────

    protected void gvSubGroup_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        string subCode = e.CommandArgument.ToString();

        if (e.CommandName == "EditRow")
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConStr))
                {
                    string query = @"SELECT SubGroupCode, GroupCode, SubGroupDesc,
                                            PrecastCodePattern, ItemCodingGroup, Remarks,
                                            BudgetCode, BudgetDesc,
                                            InvAccCode, InvAccDesc,
                                            CostAccCode, CostAccDesc,
                                            IncomeAccCode, IncomeAccDesc
                                     FROM SubGroup_Master
                                     WHERE SubGroupCode = @subCode";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@subCode", subCode);
                        con.Open();
                        SqlDataReader dr = cmd.ExecuteReader();
                        if (dr.Read())
                        {
                            // Re-load dropdown so selection can be set
                            LoadGroupDropdown();

                            ddlGroupCode.SelectedValue = dr["GroupCode"].ToString();
                            txtSubGroupCode.Text = dr["SubGroupCode"].ToString();
                            txtSubGroupDesc.Text = dr["SubGroupDesc"].ToString();
                            bool precast = dr["PrecastCodePattern"] != DBNull.Value && (bool)dr["PrecastCodePattern"];
                            rbPrecastYes.Checked = precast;
                            rbPrecastNo.Checked = !precast;
                            txtItemCodingGroup.Text = dr["ItemCodingGroup"].ToString();
                            txtRemarks.Text = dr["Remarks"].ToString();
                            txtBudgetCode.Text = dr["BudgetCode"].ToString();
                            txtBudgetDesc.Text = dr["BudgetDesc"].ToString();
                            txtInvAccCode.Text = dr["InvAccCode"].ToString();
                            txtInvAccDesc.Text = dr["InvAccDesc"].ToString();
                            txtCostAccCode.Text = dr["CostAccCode"].ToString();
                            txtCostAccDesc.Text = dr["CostAccDesc"].ToString();
                            txtIncomeAccCode.Text = dr["IncomeAccCode"].ToString();
                            txtIncomeAccDesc.Text = dr["IncomeAccDesc"].ToString();
                            hfEditCode.Value = dr["SubGroupCode"].ToString();
                        }
                        dr.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                RegisterAlert("Error loading record: " + ex.Message);
            }
        }
        else if (e.CommandName == "DeleteRow")
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConStr))
                {
                    string deleteQuery = "DELETE FROM SubGroup_Master WHERE SubGroupCode = @subCode";
                    using (SqlCommand cmd = new SqlCommand(deleteQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@subCode", subCode);
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
                BindGrid();
            }
            catch (Exception ex)
            {
                RegisterAlert("Error deleting record: " + ex.Message);
            }
        }
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private void AddSubGroupParams(SqlCommand cmd)
    {
        cmd.Parameters.AddWithValue("@groupCode", ddlGroupCode.SelectedValue);
        cmd.Parameters.AddWithValue("@desc", txtSubGroupDesc.Text.Trim());
        cmd.Parameters.AddWithValue("@precast", rbPrecastYes.Checked);
        cmd.Parameters.AddWithValue("@itemCoding", txtItemCodingGroup.Text.Trim());
        cmd.Parameters.AddWithValue("@remarks", txtRemarks.Text.Trim());
        cmd.Parameters.AddWithValue("@budgetCode", txtBudgetCode.Text.Trim());
        cmd.Parameters.AddWithValue("@budgetDesc", txtBudgetDesc.Text.Trim());
        cmd.Parameters.AddWithValue("@invAccCode", txtInvAccCode.Text.Trim());
        cmd.Parameters.AddWithValue("@invAccDesc", txtInvAccDesc.Text.Trim());
        cmd.Parameters.AddWithValue("@costAccCode", txtCostAccCode.Text.Trim());
        cmd.Parameters.AddWithValue("@costAccDesc", txtCostAccDesc.Text.Trim());
        cmd.Parameters.AddWithValue("@incomeAccCode", txtIncomeAccCode.Text.Trim());
        cmd.Parameters.AddWithValue("@incomeAccDesc", txtIncomeAccDesc.Text.Trim());
    }

    private void RegisterAlert(string message)
    {
        string safe = message.Replace("'", "\\'");
        Page.ClientScript.RegisterStartupScript(GetType(), "alert",
            "alert('" + safe + "');", true);
    }

    private void ClearForm()
    {
        LoadGroupDropdown();
        txtSubGroupCode.Text = "";
        txtSubGroupDesc.Text = "";
        rbPrecastNo.Checked = true;
        rbPrecastYes.Checked = false;
        txtItemCodingGroup.Text = "";
        txtRemarks.Text = "";
        txtBudgetCode.Text = "";
        txtBudgetDesc.Text = "";
        txtInvAccCode.Text = "";
        txtInvAccDesc.Text = "";
        txtCostAccCode.Text = "";
        txtCostAccDesc.Text = "";
        txtIncomeAccCode.Text = "";
        txtIncomeAccDesc.Text = "";
        hfEditCode.Value = "";
    }

    // ─── Model ───────────────────────────────────────────────────────────────

    [Serializable]
    public class SubGroupItem
    {
        public string SubGroupCode { get; set; }
        public string GroupCode { get; set; }
        public string SubGroupDesc { get; set; }
        public bool PrecastCodePattern { get; set; }
        public string ItemCodingGroup { get; set; }
        public string Remarks { get; set; }
        public string BudgetCode { get; set; }
        public string BudgetDesc { get; set; }
        public string InvAccCode { get; set; }
        public string InvAccDesc { get; set; }
        public string CostAccCode { get; set; }
        public string CostAccDesc { get; set; }
        public string IncomeAccCode { get; set; }
        public string IncomeAccDesc { get; set; }
    }
}