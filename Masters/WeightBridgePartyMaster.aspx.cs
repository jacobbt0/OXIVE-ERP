
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using System.Xml.Linq;

public partial class Masters_WeightBridgePartyMaster : System.Web.UI.Page
{
    string ConStr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;

    protected void Page_Load(object sender, EventArgs e)
    {

    }

protected void btnSave_Click(object sender, EventArgs e)
    {
        try
        {
            using (SqlConnection con = new SqlConnection(ConStr))
            {
                con.Open();

                string query = @"INSERT INTO WeightBridgePartyMaster
            (
                Name,
                FullName,
                POBox,
                Address,
                City,
                Telephone,
                FaxNo,
                Email,
                Website,
                ContactPerson,
                Designation,
                MobileNo,
                Remarks,
                Status
            )
            VALUES
            (
                @Name,
                @FullName,
                @POBox,
                @Address,
                @City,
                @Telephone,
                @FaxNo,
                @Email,
                @Website,
                @ContactPerson,
                @Designation,
                @MobileNo,
                @Remarks,
                @Status
            )";

                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@Name", txtName.Text.Trim());
                cmd.Parameters.AddWithValue("@FullName", txtFullName.Text.Trim());
                cmd.Parameters.AddWithValue("@POBox", txtPOBox.Text.Trim());
                cmd.Parameters.AddWithValue("@Address", txtAddress.Text.Trim());
                cmd.Parameters.AddWithValue("@City", txtCity.Text.Trim());
                cmd.Parameters.AddWithValue("@Telephone", txtPhone.Text.Trim());
                cmd.Parameters.AddWithValue("@FaxNo", txtFax.Text.Trim());
                cmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                cmd.Parameters.AddWithValue("@Website", txtWebsite.Text.Trim());
                cmd.Parameters.AddWithValue("@ContactPerson", txtContactPerson.Text.Trim());
                cmd.Parameters.AddWithValue("@Designation", txtDesignation.Text.Trim());
                cmd.Parameters.AddWithValue("@MobileNo", txtMobile.Text.Trim());
                cmd.Parameters.AddWithValue("@Remarks", txtRemarks.Text.Trim());

                if (rbActive.Checked == true)
                {
                    cmd.Parameters.AddWithValue("@Status", "Active");
                }
                else
                {
                    cmd.Parameters.AddWithValue("@Status", "Inactive");
                }

                int result = cmd.ExecuteNonQuery();

                con.Close();

                if (result > 0)
                {
                    ClientScript.RegisterStartupScript(
                        this.GetType(),
                        "msg",
                        "alert('Saved Successfully');",
                        true);

                    ClearFields();
                }
            }
        }
        catch (Exception ex)
        {
            ClientScript.RegisterStartupScript(
                this.GetType(),
                "msg",
                "alert('" + ex.Message.Replace("'", "") + "');",
                true);
        }
    }



    protected void btnUpdate_Click(object sender, EventArgs e)
    {
        try
        {
            using (SqlConnection con = new SqlConnection(ConStr))
            {
                string query = @"UPDATE WeightBridgePartyMaster
                                SET
                                Name=@Name,
                                FullName=@FullName,
                                POBox=@POBox,
                                Address=@Address,
                                City=@City,
                                Telephone=@Telephone,
                                FaxNo=@FaxNo,
                                Email=@Email,
                                Website=@Website,
                                ContactPerson=@ContactPerson,
                                Designation=@Designation,
                                MobileNo=@MobileNo,
                                Remarks=@Remarks,
                                Status=@Status
                                WHERE Code=@Code";

                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@Code", txtCode.Text.Trim());
                cmd.Parameters.AddWithValue("@Name", txtName.Text.Trim());
                cmd.Parameters.AddWithValue("@FullName", txtFullName.Text.Trim());
                cmd.Parameters.AddWithValue("@POBox", txtPOBox.Text.Trim());
                cmd.Parameters.AddWithValue("@Address", txtAddress.Text.Trim());
                cmd.Parameters.AddWithValue("@City", txtCity.Text.Trim());
                cmd.Parameters.AddWithValue("@Telephone", txtPhone.Text.Trim());
                cmd.Parameters.AddWithValue("@FaxNo", txtFax.Text.Trim());
                cmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                cmd.Parameters.AddWithValue("@Website", txtWebsite.Text.Trim());
                cmd.Parameters.AddWithValue("@ContactPerson", txtContactPerson.Text.Trim());
                cmd.Parameters.AddWithValue("@Designation", txtDesignation.Text.Trim());
                cmd.Parameters.AddWithValue("@MobileNo", txtMobile.Text.Trim());
                cmd.Parameters.AddWithValue("@Remarks", txtRemarks.Text.Trim());

                cmd.Parameters.AddWithValue("@Status",
                    rbActive.Checked ? "Active" : "Inactive");

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();

                ClientScript.RegisterStartupScript(
    this.GetType(),
    "msg",
    "alert('Saved Successfully');",
    true);
            }
        }
        catch (Exception ex)
        {
            ClientScript.RegisterStartupScript(
    this.GetType(),
    "msg",
    "alert('Saved Successfully');",
    true);
        }
    }

    protected void btnDelete_Click(object sender, EventArgs e)
    {
        try
        {
            using (SqlConnection con = new SqlConnection(ConStr))
            {
                string query = "DELETE FROM WeightBridgePartyMaster WHERE Code=@Code";

                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@Code", txtCode.Text.Trim());

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();

                ClientScript.RegisterStartupScript(
     this.GetType(),
     "msg",
     "alert('Deleted Successfully');",
     true);

                ClearFields();
            }
        }
        catch (Exception ex)
        {
            ClientScript.RegisterStartupScript(
    this.GetType(),
    "msg",
    "alert('Saved Successfully');",
    true);
        }
    }

    protected void btnClear_Click(object sender, EventArgs e)
    {
        ClearFields();
    }

    private void ClearFields()
    {
        txtCode.Text = "";
        txtName.Text = "";
        txtFullName.Text = "";
        txtPOBox.Text = "";
        txtAddress.Text = "";
        txtCity.Text = "";
        txtPhone.Text = "";
        txtFax.Text = "";
        txtEmail.Text = "";
        txtWebsite.Text = "";
        txtContactPerson.Text = "";
        txtDesignation.Text = "";
        txtMobile.Text = "";
        txtRemarks.Text = "";

        rbActive.Checked = true;
    }

    protected void btnSearchPopup_Click(object sender, EventArgs e)
    {
        LoadSearchGrid();

        pnlPopup.Style["display"] = "block";
    }

    private void LoadSearchGrid()
    {
        using (SqlConnection con = new SqlConnection(ConStr))
        {
            string query = @"SELECT
                            Code,
                            Name,
                            Status
                            FROM WeightBridgePartyMaster";

            SqlDataAdapter da = new SqlDataAdapter(query, con);

            DataTable dt = new DataTable();

            da.Fill(dt);

            gvSearch.DataSource = dt;
            gvSearch.DataBind();
        }
    }

    protected void gvSearch_SelectedIndexChanged(object sender, EventArgs e)
    {
        GridViewRow row = gvSearch.SelectedRow;

        txtCode.Text = row.Cells[0].Text;

        LoadData(txtCode.Text);

        pnlPopup.Style["display"] = "none";
    }

    private void LoadData(string code)
    {
        using (SqlConnection con = new SqlConnection(ConStr))
        {
            string query = @"SELECT * FROM WeightBridgePartyMaster
                            WHERE Code=@Code";

            SqlCommand cmd = new SqlCommand(query, con);

            cmd.Parameters.AddWithValue("@Code", code);

            con.Open();

            SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                txtCode.Text = dr["Code"].ToString();
                txtName.Text = dr["Name"].ToString();
                txtFullName.Text = dr["FullName"].ToString();
                txtPOBox.Text = dr["POBox"].ToString();
                txtAddress.Text = dr["Address"].ToString();
                txtCity.Text = dr["City"].ToString();
                txtPhone.Text = dr["Telephone"].ToString();
                txtFax.Text = dr["FaxNo"].ToString();
                txtEmail.Text = dr["Email"].ToString();
                txtWebsite.Text = dr["Website"].ToString();
                txtContactPerson.Text = dr["ContactPerson"].ToString();
                txtDesignation.Text = dr["Designation"].ToString();
                txtMobile.Text = dr["MobileNo"].ToString();
                txtRemarks.Text = dr["Remarks"].ToString();

                string status = dr["Status"].ToString();

                if (status == "Active")
                {
                    rbActive.Checked = true;
                }
                else
                {
                    rbInactive.Checked = true;
                }
            }

            con.Close();
        }
    }
}

