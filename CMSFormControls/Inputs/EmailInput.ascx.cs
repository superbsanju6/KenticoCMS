using System;
using System.Data;
using System.Collections;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using CMS.FormControls;
using CMS.GlobalHelper;

public partial class CMSFormControls_Inputs_EmailInput : FormEngineUserControl
{
    #region "Properties"

    /// <summary>
    /// Gets or sets the enabled state of the control.
    /// </summary>
    public override bool Enabled
    {
        get
        {
            return base.Enabled;
        }
        set
        {
            base.Enabled = value;
            txtEmailInput.Enabled = value;
        }
    }


    /// <summary>
    /// Gets or sets field value.
    /// </summary>
    public override object Value
    {
        get
        {
            return txtEmailInput.Text.Trim();
        }
        set
        {
            txtEmailInput.Text = (string)value;
        }
    }


    /// <summary>
    /// Gets ClientID of the textbox with emailinput.
    /// </summary>
    public override string ValueElementID
    {
        get
        {
            return txtEmailInput.ClientID;
        }
    }


    private bool mAllowMultipleAddresses = false;

    /// <summary>
    /// Gets or sets if multiple e-mail addresses can be entered.
    /// </summary>
    public bool AllowMultipleAddresses
    {
        get
        {
            return mAllowMultipleAddresses;
        }
        set
        {
            mAllowMultipleAddresses = value;
        }
    }


    /// <summary>
    /// Gets or sets string that should be used to delimit multiple addresses (default separator is semicolon - ;).
    /// </summary>
    public string EmailSeparator
    {
        get;
        set;
    }

    #endregion


    #region "Methods"

    /// <summary>
    /// Page load.
    /// </summary>
    protected void Page_Load(object sender, EventArgs e)
    {
        // Set control style and css class
        if (!string.IsNullOrEmpty(ControlStyle))
        {
            txtEmailInput.Attributes.Add("style", ControlStyle);
        }
        if (!string.IsNullOrEmpty(CssClass))
        {
            txtEmailInput.CssClass = CssClass;
            CssClass = null;
        }

        // Set additional properties
        AllowMultipleAddresses = ValidationHelper.GetBoolean(GetValue("allowmultipleaddresses"), AllowMultipleAddresses);
        EmailSeparator = ValidationHelper.GetString(GetValue("emailseparator"), EmailSeparator);
    }


    /// <summary>
    /// Returns true if user control is valid.
    /// </summary>
    public override bool IsValid()
    {
        if (string.IsNullOrEmpty(txtEmailInput.Text.Trim()))
        {
            return true;
        }

        // Check if valid e-mail addresses were entered
        bool validEmails = (AllowMultipleAddresses ? ValidationHelper.AreEmails(txtEmailInput.Text.Trim(), EmailSeparator) : ValidationHelper.IsEmail(txtEmailInput.Text.Trim()));
        if (validEmails)
        {
            if (FieldInfo != null)
            {
                // Check regular expresion
                if (!string.IsNullOrEmpty(FieldInfo.RegularExpression))
                {
                    if (new Validator().IsRegularExp(txtEmailInput.Text.Trim(), FieldInfo.RegularExpression, "error").Result == "error")
                    {
                        ValidationError = FieldInfo.ValidationErrorMessage;
                        return false;
                    }
                }

                // Check min lenght
                if ((FieldInfo.MinStringLength > 0) && (txtEmailInput.Text.Trim().Length < FieldInfo.MinStringLength))
                {
                    ValidationError = FieldInfo.ValidationErrorMessage;
                    return false;
                }

                // Check max lenght
                if ((FieldInfo.MaxStringLength > 0) && (txtEmailInput.Text.Length > FieldInfo.MaxStringLength))
                {
                    ValidationError = FieldInfo.ValidationErrorMessage;
                    return false;
                }
            }

            return true;
        }
        else
        {
            ValidationError = GetString("EmailInput.ValidationError");
            return false;
        }
    }

    #endregion
}