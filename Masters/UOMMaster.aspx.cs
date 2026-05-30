using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Masters_UOMMaster : System.Web.UI.Page
{
    public string savedMessage = "";

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            BindGrid();
            GenerateNextCode();
        }
    }

    // BIND GRID FROM SQL SERVER
    private void BindGrid()
    {
        string constr =
            ConfigurationManager.ConnectionStrings["constr"]
            .ConnectionString;

        using (SqlConnection con =
            new SqlConnection(constr))
        {
            string query =
                "SELECT * FROM UOM_Master ORDER BY Code";

            using (SqlCommand cmd =
                new SqlCommand(query, con))
            {
                using (SqlDataAdapter sda =
                    new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();

                    sda.Fill(dt);

                    gvUOM.DataSource = dt;
                    gvUOM.DataBind();
                }
            }
        }
    }

    // SAVE / UPDATE
   
   
protected void btnSave_Click(object sender, EventArgs e)
    {
        try
        {
            if (!Page.IsValid)
                return;

            string name =
                txtName.Text.Trim().ToUpper();

            string category =
                ddlCategory.SelectedValue;

            bool readyMix =
                chkReadyMix.Checked;

            bool block =
                chkBlock.Checked;

            bool precast =
                chkPrecast.Checked;

            string editCode =
                hfEditCode.Value;

            string constr =
                ConfigurationManager
                .ConnectionStrings["constr"]
                .ConnectionString;

            using (SqlConnection con =
                new SqlConnection(constr))
            {
                con.Open();

                // =========================
                // CHECK DUPLICATE NAME
                // =========================

                string checkQuery = "";

                if (!string.IsNullOrEmpty(editCode))
                {
                    // While editing, ignore current record

                    checkQuery = @"
                    SELECT COUNT(*)
                    FROM UOM_Master
                    WHERE UPPER(Name)=@Name
                    AND Code<>@Code";
                }
                else
                {
                    // While inserting

                    checkQuery = @"
                    SELECT COUNT(*)
                    FROM UOM_Master
                    WHERE UPPER(Name)=@Name";
                }

                using (SqlCommand checkCmd =
                    new SqlCommand(checkQuery, con))
                {
                    checkCmd.Parameters.AddWithValue(
                        "@Name",
                        name.ToUpper());

                    if (!string.IsNullOrEmpty(editCode))
                    {
                        checkCmd.Parameters.AddWithValue(
                            "@Code",
                            editCode);
                    }

                    int count =
                        Convert.ToInt32(
                            checkCmd.ExecuteScalar());

                    if (count > 0)
                    {
                        Page.ClientScript
                            .RegisterStartupScript(
                                GetType(),
                                "dup",
                                "alert('Name already exists!');",
                                true);
                     
                        return;
                    }
                }

                // =========================
                // UPDATE
                // =========================

                if (!string.IsNullOrEmpty(editCode))
                {
                    string updateQuery = @"
                    UPDATE UOM_Master
                    SET
                        Name = @Name,
                        Category = @Category,
                        ReadyMix = @ReadyMix,
                        BlockItem = @Block,
                        Precast = @Precast
                    WHERE Code = @Code";

                    using (SqlCommand cmd =
                        new SqlCommand(updateQuery, con))
                    {
                        cmd.Parameters.AddWithValue(
                            "@Code",
                            editCode);

                        cmd.Parameters.AddWithValue(
                            "@Name",
                            name);

                        cmd.Parameters.AddWithValue(
                            "@Category",
                            category);

                        cmd.Parameters.AddWithValue(
                            "@ReadyMix",
                            readyMix);

                        cmd.Parameters.AddWithValue(
                            "@Block",
                            block);

                        cmd.Parameters.AddWithValue(
                            "@Precast",
                            precast);

                        cmd.ExecuteNonQuery();
                    }

                    savedMessage =
                        "Record updated successfully!";
                }

                // =========================
                // INSERT
                // =========================

                else
                {
                    string insertQuery = @"
                    INSERT INTO UOM_Master
                    (
                        Name,
                        Category,
                        ReadyMix,
                        BlockItem,
                        Precast
                    )
                    VALUES
                    (
                        @Name,
                        @Category,
                        @ReadyMix,
                        @Block,
                        @Precast
                    )";

                    using (SqlCommand cmd =
                        new SqlCommand(insertQuery, con))
                    {
                        cmd.Parameters.AddWithValue(
                            "@Name",
                            name);

                        cmd.Parameters.AddWithValue(
                            "@Category",
                            category);

                        cmd.Parameters.AddWithValue(
                            "@ReadyMix",
                            readyMix);

                        cmd.Parameters.AddWithValue(
                            "@Block",
                            block);

                        cmd.Parameters.AddWithValue(
                            "@Precast",
                            precast);

                        cmd.ExecuteNonQuery();
                    }

                    savedMessage =
                        "Record saved successfully!";
                }
            }

            ClearForm();

            BindGrid();

            Page.ClientScript.RegisterStartupScript(
                GetType(),
                "msg",
                "alert('" + savedMessage + "');",
                true);
        }
        catch (SqlException ex)
        {
            Page.ClientScript.RegisterStartupScript(
                GetType(),
                "sqlerr",
                "alert('Database Error : " +
                ex.Message.Replace("'", "") +
                "');",
                true);
        }
        catch (Exception ex)
        {
            Page.ClientScript.RegisterStartupScript(
                GetType(),
                "err",
                "alert('Error : " +
                ex.Message.Replace("'", "") +
                "');",
                true);
        }
    }



    // CANCEL
    protected void btnCancel_Click(object sender, EventArgs e)
    {
        ClearForm();
    }

    // GENERATE NEXT CODE
    private void GenerateNextCode()
    {
        try
        {
            string constr =
           ConfigurationManager.ConnectionStrings["constr"]
           .ConnectionString;

            using (SqlConnection con =
                new SqlConnection(constr))
            {
                string query =
                    "SELECT ISNULL(MAX(Code),0) + 1 FROM UOM_Master";

                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    con.Open();

                    txtCode.Text =
                        cmd.ExecuteScalar().ToString();

                    txtCode.Enabled = false;
                }
            }
        }
        catch (Exception ex)
        {
            Page.ClientScript.RegisterStartupScript(
                GetType(),
                "err",
                "alert('Error : " + ex.Message.Replace("'", "") + "');",
                true
            );
        }
    }

    // GRIDVIEW EDIT / DELETE
    protected void gvUOM_RowCommand(
        object sender,
        GridViewCommandEventArgs e)
    {
        try
        {
            string code =
            e.CommandArgument.ToString();

        string constr =
            ConfigurationManager.ConnectionStrings["constr"]
            .ConnectionString;

            using (SqlConnection con =
                new SqlConnection(constr))
            {
                con.Open();

                // EDIT

                if (e.CommandName == "EditRow")
                {
                    string query =
                        "SELECT * FROM UOM_Master WHERE Code=@Code";

                    using (SqlCommand cmd =
                        new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Code", code);

                        SqlDataReader dr =
                            cmd.ExecuteReader();

                        if (dr.Read())
                        {
                            txtCode.Text =
                                dr["Code"].ToString();

                            txtName.Text =
                                dr["Name"].ToString();

                            ddlCategory.SelectedValue =
                                dr["Category"].ToString();

                            chkReadyMix.Checked =
                                Convert.ToBoolean(
                                    dr["ReadyMix"]);

                            chkBlock.Checked =
                                Convert.ToBoolean(
                                    dr["BlockItem"]);

                            chkPrecast.Checked =
                                Convert.ToBoolean(
                                    dr["Precast"]);

                            txtCode.Enabled = false;

                            hfEditCode.Value =
                                dr["Code"].ToString();
                        }
                    }
                }
           

            // DELETE

            else if (e.CommandName == "DeleteRow")
            {
                string deleteQuery =
                    "DELETE FROM UOM_Master WHERE Code=@Code";

                using (SqlCommand cmd =
                    new SqlCommand(deleteQuery, con))
                {
                    cmd.Parameters.AddWithValue("@Code", code);

                    cmd.ExecuteNonQuery();
                }

                BindGrid();
            }
        }
        }
        catch (Exception ex)
        {
            Page.ClientScript.RegisterStartupScript(
                GetType(),
                "err",
                "alert('Error : " + ex.Message.Replace("'", "") + "');",
                true
            );
        }
    }
         

// GRIDVIEW ROW STYLE
protected void gvUOM_RowDataBound(
        object sender,
        GridViewRowEventArgs e)
    {
        if (e.Row.RowType ==
            DataControlRowType.DataRow)
        {
            Label lblReadyMix =
                e.Row.FindControl("lblReadyMix")
                as Label;

            Label lblBlock =
                e.Row.FindControl("lblBlock")
                as Label;

            Label lblPrecast =
                e.Row.FindControl("lblPrecast")
                as Label;

            if (lblReadyMix != null &&
                lblReadyMix.Text == "Yes")
            {
                lblReadyMix.ForeColor =
                    System.Drawing.Color.DarkGreen;
            }

            if (lblBlock != null &&
                lblBlock.Text == "Yes")
            {
                lblBlock.ForeColor =
                    System.Drawing.Color.DarkGreen;
            }

            if (lblPrecast != null &&
                lblPrecast.Text == "Yes")
            {
                lblPrecast.ForeColor =
                    System.Drawing.Color.DarkGreen;
            }
        }
    }

    // CLEAR FORM
    private void ClearForm()
    {
        try { 
        txtName.Text = "";

        ddlCategory.SelectedIndex = 0;

        chkReadyMix.Checked = false;
        chkBlock.Checked = false;
        chkPrecast.Checked = false;

        hfEditCode.Value = "";

        GenerateNextCode();
        }
        catch (Exception ex)
        {
            Page.ClientScript.RegisterStartupScript(
                GetType(),
                "err",
                "alert('Error : " + ex.Message.Replace("'", "") + "');",
                true
            );
        }
    }
}