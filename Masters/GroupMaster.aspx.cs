using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Masters_GroupMaster : System.Web.UI.Page
{
    public string savedMessage = "";

    private string ConStr
    {
        get { return ConfigurationManager.ConnectionStrings["constr"].ConnectionString; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
            BindGrid();
    }

    private void BindGrid()
    {
        var groups = new List<GroupItem>();
        try
        {
            using (SqlConnection con = new SqlConnection(ConStr))
            {
                string query = @"SELECT CategoryCode, CategoryDescription, CategoryPrefix,
                                        Remarks
                                 FROM Group_Master";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        groups.Add(new GroupItem
                        {
                            CategoryCode = dr["CategoryCode"].ToString(),
                            CategoryDesc = dr["CategoryDescription"].ToString(),
                            CategoryPrefix = dr["CategoryPrefix"].ToString(),
                            Remarks = dr["Remarks"].ToString(),
                            
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

        gvGroup.DataSource = groups;
        gvGroup.DataBind();
    }

    protected void btnSave_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid) return;
        string code = txtCategoryCode.Text.Trim();

        try
        {
            using (SqlConnection con = new SqlConnection(ConStr))
            {
                con.Open();

                // Duplicate check on Description 
                string checkQuery = "SELECT COUNT(*) FROM Group_Master WHERE CategoryDescription = @desc";
                using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
                {
                    checkCmd.Parameters.AddWithValue("@desc", txtCategoryDesc.Text.Trim());
                    int count = (int)checkCmd.ExecuteScalar();
                    if (count > 0)
                    {
                        RegisterAlert("Category Description already exists!");
                        return;
                    }
                }

                string insertQuery = @"INSERT INTO Group_Master
                    ( CategoryDescription, CategoryPrefix, Remarks
                     )
                    VALUES
                    ( @desc, @prefix, @remarks)";

                using (SqlCommand cmd = new SqlCommand(insertQuery, con))
                {
                    AddGroupParams(cmd, code);
                    cmd.ExecuteNonQuery();
                }
            }

            savedMessage = "Group saved successfully!";
            ClearForm();
            BindGrid();
        }
        catch (Exception ex)
        {
            RegisterAlert("Error saving group: " + ex.Message);
        }
    }

    protected void btnUpdate_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid) return;
        string code = hfEditCode.Value;
        if (string.IsNullOrEmpty(code))
            code = txtCategoryCode.Text.Trim().ToUpper();

        try
        {

            using (SqlConnection con = new SqlConnection(ConStr))
            {
                con.Open();
                // Ensure no other record already has this description
                string checkQuery = "SELECT COUNT(*) FROM Group_Master WHERE CategoryDescription = @desc AND CategoryCode <> @code";
                using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
                {
                    checkCmd.Parameters.AddWithValue("@desc", txtCategoryDesc.Text.Trim());
                    checkCmd.Parameters.AddWithValue("@code", code);
                    int count = (int)checkCmd.ExecuteScalar();
                    if (count > 0)
                    {
                        RegisterAlert("Category Description already exists!");
                        return;
                    }
                }

                string updateQuery = @"UPDATE Group_Master SET
                    CategoryDescription = @desc,
                    CategoryPrefix      = @prefix,
                    Remarks             = @remarks
                    WHERE CategoryCode  = @code";

                using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                {
                    AddGroupParams(cmd, code);
                  
                    cmd.ExecuteNonQuery();
                }
            }

            savedMessage = "Group updated successfully!";
            ClearForm();
            BindGrid();
        }
        catch (Exception ex)
        {
            RegisterAlert("Error updating group: " + ex.Message);
        }
    }

    protected void btnDelete_Click(object sender, EventArgs e)
    {
        string code = hfEditCode.Value;

        if (string.IsNullOrEmpty(code))
            code = txtCategoryCode.Text.Trim();

        try
        {
            using (SqlConnection con = new SqlConnection(ConStr))
            {
                con.Open();

                // CHECK WHETHER SUB GROUP EXISTS
                string checkQuery = "SELECT COUNT(*) FROM SubGroup_Master WHERE GroupCode = @code";

                using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
                {
                    checkCmd.Parameters.AddWithValue("@code", code);

                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (count > 0)
                    {
                        RegisterAlert("Cannot delete this Group because Sub Groups exist!");
                        return;
                    }
                }

                // DELETE ONLY IF NO SUB GROUP EXISTS
                string deleteQuery = "DELETE FROM Group_Master WHERE CategoryCode = @code";

                using (SqlCommand cmd = new SqlCommand(deleteQuery, con))
                {
                    cmd.Parameters.AddWithValue("@code", code);
                    cmd.ExecuteNonQuery();
                }
            }

            savedMessage = "Group deleted successfully!";
            ClearForm();
            BindGrid();
        }
        catch (Exception ex)
        {
            RegisterAlert("Error deleting group: " + ex.Message);
        }
    }

    protected void btnClear_Click(object sender, EventArgs e)
    {
        ClearForm();
    }

    protected void gvGroup_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        string code = e.CommandArgument.ToString();

        if (e.CommandName == "EditRow")
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConStr))
                {
                    string query = @"SELECT CategoryCode, CategoryDescription, CategoryPrefix,
                                            Remarks
                                     FROM Group_Master
                                     WHERE CategoryCode = @code";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@code", code);
                        con.Open();
                        SqlDataReader dr = cmd.ExecuteReader();
                        if (dr.Read())
                        {
                            txtCategoryCode.Text = dr["CategoryCode"].ToString();
                            txtCategoryDesc.Text = dr["CategoryDescription"].ToString();
                            txtCategoryPrefix.Text = dr["CategoryPrefix"].ToString();
                            txtRemarks.Text = dr["Remarks"].ToString();
                           
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
                    con.Open();

                    // CHECK WHETHER SUB GROUP EXISTS
                    string checkQuery = "SELECT COUNT(*) FROM SubGroup_Master WHERE GroupCode = @code";

                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
                    {
                        checkCmd.Parameters.AddWithValue("@code", code);

                        int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                        if (count > 0)
                        {
                            RegisterAlert("Cannot delete this Group because Sub Groups exist!");
                            return;
                        }
                    }

                    // DELETE ONLY IF NO SUB GROUP EXISTS
                    string deleteQuery = "DELETE FROM Group_Master WHERE CategoryCode = @code";

                    using (SqlCommand cmd = new SqlCommand(deleteQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@code", code);
                        cmd.ExecuteNonQuery();
                    }
                }

                savedMessage = "Group deleted successfully!";
                BindGrid();
            }
            catch (Exception ex)
            {
                RegisterAlert("Error deleting record: " + ex.Message);
            }
        }
    }

    // Shared helper: adds all GROUP parameters to a command (INSERT or UPDATE)
    private void AddGroupParams(SqlCommand cmd, string code)
    {
        cmd.Parameters.AddWithValue("@code", code);
        cmd.Parameters.AddWithValue("@desc", txtCategoryDesc.Text.Trim());
        cmd.Parameters.AddWithValue("@prefix", txtCategoryPrefix.Text.Trim().ToUpper());
        cmd.Parameters.AddWithValue("@remarks", txtRemarks.Text.Trim());
       
    }

    private void RegisterAlert(string message)
    {
        string safe = message.Replace("'", "\\'");
        Page.ClientScript.RegisterStartupScript(GetType(), "alert",
            "alert('" + safe + "');", true);
    }

    private void ClearForm()
    {
        txtCategoryCode.Text = "";
        txtCategoryDesc.Text = "";
        txtCategoryPrefix.Text = "";
        txtRemarks.Text = "";
        txtInvAccCode.Text = "";
        txtInvAccDesc.Text = "";
        txtCostAccCode.Text = "";
        txtCostAccDesc.Text = "";
        txtIncomeAccCode.Text = "";
        txtIncomeAccDesc.Text = "";
        hfEditCode.Value = "";
        txtCategoryCode.Enabled = true;
    }

    private GroupItem BuildItem(string code)
    {
        return new GroupItem
        {
            CategoryCode = code,
            CategoryDesc = txtCategoryDesc.Text.Trim(),
            CategoryPrefix = txtCategoryPrefix.Text.Trim().ToUpper(),
            Remarks = txtRemarks.Text.Trim(),
            InvAccCode = txtInvAccCode.Text.Trim(),
            InvAccDesc = txtInvAccDesc.Text.Trim(),
            CostAccCode = txtCostAccCode.Text.Trim(),
            CostAccDesc = txtCostAccDesc.Text.Trim(),
            IncomeAccCode = txtIncomeAccCode.Text.Trim(),
            IncomeAccDesc = txtIncomeAccDesc.Text.Trim()
        };
    }

    [Serializable]
    public class GroupItem
    {
        public string CategoryCode { get; set; }
        public string CategoryDesc { get; set; }
        public string CategoryPrefix { get; set; }
        public string Remarks { get; set; }
        public string InvAccCode { get; set; }
        public string InvAccDesc { get; set; }
        public string CostAccCode { get; set; }
        public string CostAccDesc { get; set; }
        public string IncomeAccCode { get; set; }
        public string IncomeAccDesc { get; set; }
    }
}