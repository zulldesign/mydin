using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Bitrix.UI;
using System.Globalization;
using Bitrix.Services;

public partial class bitrix_ui_Calendar : BXControl, IBXCalendar
{
    //FIELDS
    private string textBoxId;
	
    protected void Page_Load(object sender, EventArgs e)
    {
        BXCalendarHelper.RegisterScriptFiles();
    }
	protected string UnixTimeStamp()
	{
		TimeSpan ts = DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1).ToUniversalTime();
		return ((long)ts.TotalSeconds).ToString();
	}

    private TextBox txtDate;
    public TextBox DateField
    {
        get 
        {
            if (txtDate == null)
            {
				TextBox control = BXControlUtility.FindControl(this, TextBoxId) as TextBox;
                if (control != null)
                    txtDate = control;
                else
                    txtDate = new TextBox();
            }

            return txtDate; 
        }
    }

	protected override void Render(HtmlTextWriter writer)
	{
		ScriptManager sm = ScriptManager.GetCurrent(Page);
		if (sm == null)
			throw new Exception("Calendar Control requires ScriptManager with EnableScriptGlobalization set to true to work correctly");
		base.Render(writer);
	}

    #region IBXCalendar Members

    public DateTime Date
    {
        get
        {
            if (!string.IsNullOrEmpty(DateField.Text))
            {
                DateTime date;
                if (DateTime.TryParse(DateField.Text.Trim(), out date))
                    return date;
            }


            return DateTime.MinValue;
        }
        set
        {
            if (value == DateTime.MaxValue || value == DateTime.MinValue)
                DateField.Text = String.Empty;
            else
                DateField.Text = value.ToString();
        }
    }

    #endregion



    public string TextBoxId
    {
        get { return textBoxId; }
        set { textBoxId = value; }
    }
}
