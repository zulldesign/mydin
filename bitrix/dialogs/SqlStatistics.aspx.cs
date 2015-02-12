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

public partial class bitrix_dialogs_SqlStatistics : BXDialogPage
{
	protected override void OnInit(EventArgs e)
	{
		base.OnInit(e);

        Behaviour.Settings.MinWidth = Behaviour.Settings.Width = 720;
        Behaviour.Settings.MinHeight = Behaviour.Settings.Height = 600;
        Behaviour.Settings.Resizeable = true;

		Title = GetMessageRaw("Title");
		DescriptionParagraphs.Add("<span id=\"BXSqlStatisticsDescription\"></span>");
		DescriptionIconClass = "bx-debug-info";

		Behaviour.ButonSetLayout = BXPageAsDialogButtonSetLayout.Continue;
	}
}
