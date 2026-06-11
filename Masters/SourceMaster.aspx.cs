using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

public partial class Masters_SourceMaster : System.Web.UI.Page
{
    string ConStr =
        ConfigurationManager.ConnectionStrings["ConStr"].ConnectionString;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            BindGrid();
        }
    }

    // =========================
    // GRID BIND
    // =========================
    private void BindGrid()
    {
        try
        {
            using (SqlConnection con =
                new SqlConnection(ConStr))
            {
                string query = @"SELECT
                                    SourceCode,
                                    SourceDescription,
                                    StationID,
                                    WastageTripLocation,
                                    IsActive
                                 FROM Source_Master
                                 ORDER BY SourceCode";

                using (SqlDataAdapter da =
                    new SqlDataAdapter(query, con))
                {
                    DataTable dt = new DataTable();

                    da.Fill(dt);

                    gvSource.DataSource = dt;

                    gvSource.DataBind();
                }
            }
        }
        catch (Exception ex)
        {
            ShowMessage(ex.Message);
        }
    }

    // =========================
    // SAVE
    // =========================
    protected void btnSave_Click(object sender, EventArgs e)
    {
        try
        {
            using (SqlConnection con =
                new SqlConnection(ConStr))
            {
                con.Open();

                if (string.IsNullOrWhiteSpace(txtDescription.Text))
                {
                    ShowMessage("Please enter Source Description.");
                    txtDescription.Focus();
                    return;
                }

                // DUPLICATE CHECK
                string checkQuery =
                    @"SELECT COUNT(*)
                      FROM Source_Master
                      WHERE SourceDescription=@SourceDescription";

                using (SqlCommand checkCmd =
                    new SqlCommand(checkQuery, con))
                {
                    checkCmd.Parameters.AddWithValue(
                        "@SourceDescription",
                        txtDescription.Text.Trim());

                    int count =
                        Convert.ToInt32(
                            checkCmd.ExecuteScalar());

                    if (count > 0)
                    {
                        ShowMessage(
                            "Source Description already exists.");

                        return;
                    }
                }

                // INSERT
                string query = @"INSERT INTO Source_Master
                                (
                                    SourceDescription,
                                    StationID,
                                    WastageTripLocation,
                                    IsActive
                                )
                                VALUES
                                (
                                    @SourceDescription,
                                    @StationID,
                                    @WastageTripLocation,
                                    @IsActive
                                )";

                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue(
                        "@SourceDescription",
                        txtDescription.Text.Trim());

                    cmd.Parameters.AddWithValue(
                        "@StationID",
                        ddlStation.SelectedValue);

                    cmd.Parameters.AddWithValue(
                        "@WastageTripLocation",
                        rblWastage.SelectedValue == "1");

                    cmd.Parameters.AddWithValue(
                        "@IsActive",
                        rblActive.SelectedValue == "1");

                    cmd.ExecuteNonQuery();
                }
            }

            ShowMessage("Saved Successfully");

            ClearForm();

            BindGrid();
        }
        catch (Exception ex)
        {
            ShowMessage(ex.Message);
        }
    }

    // =========================
    // UPDATE
    // =========================
    protected void btnUpdate_Click(object sender, EventArgs e)
    {
        try
        {
            using (SqlConnection con =
                new SqlConnection(ConStr))
            {
                con.Open();

                // DUPLICATE CHECK
                string checkQuery =
                    @"SELECT COUNT(*)
                      FROM Source_Master
                      WHERE SourceDescription=@SourceDescription
                      AND SourceCode<>@SourceCode";

                using (SqlCommand checkCmd =
                    new SqlCommand(checkQuery, con))
                {
                    checkCmd.Parameters.AddWithValue(
                        "@SourceDescription",
                        txtDescription.Text.Trim());

                    checkCmd.Parameters.AddWithValue(
                        "@SourceCode",
                        txtCode.Text);

                    int count =
                        Convert.ToInt32(
                            checkCmd.ExecuteScalar());

                    if (count > 0)
                    {
                        ShowMessage(
                            "Source Description already exists.");

                        return;
                    }
                }

                // UPDATE
                string query = @"UPDATE Source_Master
                                 SET
                                    SourceDescription=@SourceDescription,
                                    StationID=@StationID,
                                    WastageTripLocation=@WastageTripLocation,
                                    IsActive=@IsActive
                                 WHERE SourceCode=@SourceCode";

                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue(
                        "@SourceCode",
                        txtCode.Text);

                    cmd.Parameters.AddWithValue(
                        "@SourceDescription",
                        txtDescription.Text.Trim());

                    cmd.Parameters.AddWithValue(
                        "@StationID",
                        ddlStation.SelectedValue);

                    cmd.Parameters.AddWithValue(
                        "@WastageTripLocation",
                        rblWastage.SelectedValue == "1");

                    cmd.Parameters.AddWithValue(
                        "@IsActive",
                        rblActive.SelectedValue == "1");

                    cmd.ExecuteNonQuery();
                }
            }

            ShowMessage("Updated Successfully");

            ClearForm();

            BindGrid();
        }
        catch (Exception ex)
        {
            ShowMessage(ex.Message);
        }
    }

    // =========================
    // DELETE
    // =========================
    protected void btnDelete_Click(object sender, EventArgs e)
    {
        try
        {
            using (SqlConnection con =
                new SqlConnection(ConStr))
            {
                string query =
                    @"DELETE FROM Source_Master
                      WHERE SourceCode=@SourceCode";

                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue(
                        "@SourceCode",
                        txtCode.Text);

                    con.Open();

                    cmd.ExecuteNonQuery();
                }
            }

            ShowMessage("Deleted Successfully");

            ClearForm();

            BindGrid();
        }
        catch (Exception ex)
        {
            ShowMessage(ex.Message);
        }
    }

    // =========================
    // EDIT
    // =========================
    protected void gvSource_RowCommand(
        object sender,
        GridViewCommandEventArgs e)
    {
        try
        {
            if (e.CommandName == "EditRow")
            {
                string code =
                    e.CommandArgument.ToString();

                using (SqlConnection con =
                    new SqlConnection(ConStr))
                {
                    string query = @"SELECT *
                                     FROM Source_Master
                                     WHERE SourceCode=@SourceCode";

                    using (SqlCommand cmd =
                        new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue(
                            "@SourceCode",
                            code);

                        con.Open();

                        SqlDataReader dr =
                            cmd.ExecuteReader();

                        if (dr.Read())
                        {
                            txtCode.Text =
                                dr["SourceCode"].ToString();

                            txtDescription.Text =
                                dr["SourceDescription"].ToString();

                            ddlStation.SelectedValue =
                                dr["StationID"].ToString();

                            rblWastage.SelectedValue =
                                Convert.ToBoolean(
                                    dr["WastageTripLocation"])
                                    ? "1"
                                    : "0";

                            rblActive.SelectedValue =
                                Convert.ToBoolean(
                                    dr["IsActive"])
                                    ? "1"
                                    : "0";
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ShowMessage(ex.Message);
        }
    }

    // =========================
    // SEARCH
    // =========================
    protected void txtSearch_TextChanged(
        object sender,
        EventArgs e)
    {
        try
        {
            using (SqlConnection con =
                new SqlConnection(ConStr))
            {
                string query = @"SELECT *
                                 FROM Source_Master
                                 WHERE
                                    SourceDescription LIKE @Search
                                    OR StationID LIKE @Search";

                using (SqlDataAdapter da =
                    new SqlDataAdapter(query, con))
                {
                    da.SelectCommand.Parameters.AddWithValue(
                        "@Search",
                        "%" + txtSearch.Text.Trim() + "%");

                    DataTable dt = new DataTable();

                    da.Fill(dt);

                    gvSource.DataSource = dt;

                    gvSource.DataBind();
                }
            }
        }
        catch (Exception ex)
        {
            ShowMessage(ex.Message);
        }
    }

    // =========================
    // CLEAR
    // =========================
    protected void btnClear_Click(object sender, EventArgs e)
    {
        ClearForm();
    }

    private void ClearForm()
    {
        txtCode.Text = "";

        txtDescription.Text = "";

        ddlStation.SelectedIndex = 0;

        rblWastage.SelectedValue = "0";

        rblActive.SelectedValue = "1";
    }

    // =========================
    // MESSAGE
    // =========================
    private void ShowMessage(string message)
    {
        ClientScript.RegisterStartupScript(
            this.GetType(),
            "alert",
            "alert('" + message.Replace("'", "") + "');",
            true);
    }
}
