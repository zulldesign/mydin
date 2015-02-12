using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Bitrix;
using Bitrix.Services;
using Bitrix.Services.Text;
using Bitrix.UI;


public partial class bitrix_ui_TimeInterval : BXControl, ITimeInterval
{
	protected void Page_Load(object sender, EventArgs e)
    {
		
        //BXPage.RegisterScriptInclude(string.Concat("~/bitrix/controls/Main/CalendarJS.aspx?", HttpUtility.UrlEncode(BXLoc.CurrentLocale), "&cultureName=", HttpUtility.UrlEncode(CultureInfo.CurrentCulture.Name)));
    }

    public bool IsStartDateEmpty()
    {
        return string.IsNullOrEmpty(txtStartDate.Text.Trim());
    }

    public void SetEmptyStartDate() 
    {
        txtStartDate.Text = string.Empty;
    }

    public bool IsEndDateEmpty()
    {
        return string.IsNullOrEmpty(txtEndDate.Text.Trim());
    }

    public void SetEmptyEndDate()
    {
        txtEndDate.Text = string.Empty;
    }

    public DateTime StartDate
    {
        get
        {
            if (!string.IsNullOrEmpty(txtStartDate.Text.Trim()))
            {
                DateTime startDate;
                if (DateTime.TryParse(txtStartDate.Text.Trim(), out startDate))
                    return startDate;
            }


            return DateTime.MinValue;
        }
        set
        {
			if (value.ToString(CultureInfo.InvariantCulture.DateTimeFormat).Equals(DateTime.MinValue.ToString(CultureInfo.InvariantCulture.DateTimeFormat)))
				txtStartDate.Text = String.Empty;
			else
			{
				if (value.TimeOfDay == TimeSpan.Zero)
					txtStartDate.Text = value.ToString("d");
				else
					txtStartDate.Text = value.ToString();
			}
        }
    }

	protected override void OnPreRender(EventArgs e)
	{
		base.OnPreRender(e);
		BXCalendarHelper.RegisterScriptFiles();
	}

    public DateTime EndDate
    {
        get
        {
            if (!string.IsNullOrEmpty(txtEndDate.Text.Trim()))
            {
                DateTime startDate;
                if (DateTime.TryParse(txtEndDate.Text.Trim(), out startDate))
                    return startDate;
            }


            return DateTime.MaxValue;
        }
        set
        {
			if (value.ToString(CultureInfo.InvariantCulture.DateTimeFormat).Equals(DateTime.MaxValue.ToString(CultureInfo.InvariantCulture.DateTimeFormat)))
				txtEndDate.Text = String.Empty;
			else
			{
				if (value.TimeOfDay == TimeSpan.Zero)
					txtEndDate.Text = value.ToString("d");
				else
					txtEndDate.Text = value.ToString();
			}
        }
    }

    public string Str
    {
        set
        {
            if (string.IsNullOrEmpty(value))
                return;

			DateTime[] i = BXStringUtility.StringToTimeInterval(value);
            StartDate = i[0];
            EndDate = i[1];
        }
    }

    protected string UnixTimeStamp()
    {
        TimeSpan ts = DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1).ToUniversalTime();
        return ((long)ts.TotalSeconds).ToString();
    }

	protected override void Render(HtmlTextWriter writer)
	{
		ScriptManager sm = ScriptManager.GetCurrent(Page);
		if (sm == null)
			throw new Exception("Time Interval Control requires ScriptManager with EnableScriptGlobalization set to true to work correctly");
		base.Render(writer);
	}
}
