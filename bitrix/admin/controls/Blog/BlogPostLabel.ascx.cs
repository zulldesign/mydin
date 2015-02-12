using System;

using Bitrix.UI;
using System.ComponentModel;
using Bitrix.Services;
using System.Web.UI.WebControls;
using Bitrix.Services.Js;
using Bitrix.IO;
using Bitrix.Services.Text;
using Bitrix.Modules;
using Bitrix.Main;
using Bitrix.Security;
using Bitrix.CommunicationUtility;
using Bitrix.Search;
using Bitrix;
using Bitrix.DataLayer;
using Bitrix.Blog;
using System.Web;
using System.Web.UI;

public partial class bitrix_admin_controls_BlogPostLabel : BXControl
{
	public override void RenderControl(HtmlTextWriter writer)
	{
        string postId = Attributes["PostID"],
			postTitle = Attributes["PostTitle"];

		string postUrl = string.Empty;

		BXSearchQuery q = new BXSearchQuery();
		q.FieldsToSelect.Add(BXSearchField.Id);
		q.FieldsToSelect.Add(BXSearchField.ModuleId);
		q.FieldsToSelect.Add(BXSearchField.ItemId);
		q.FieldsToSelect.Add(BXSearchField.Body);
		q.FieldsToSelect.Add(BXSearchField.Title);
		q.FieldsToSelect.Add(BXSearchField.Param1);
		q.FieldsToSelect.Add(BXSearchField.Param2);

		BXSearchContentGroupFilter f = new BXSearchContentGroupFilter(BXFilterExpressionCombiningLogic.And);
		f.Add(new BXFormFilterItem("moduleId", "blog", BXSqlFilterOperators.Equal));
		f.Add(new BXFormFilterItem("itemId", "p" + postId, BXSqlFilterOperators.Equal));

		q.Filter = f;
		BXSearchResultCollection c = q.Execute();
		if (c.Count > 0)
			postUrl = c.Count > 0 && c[0].Urls.Length > 0 ? c[0].Urls[0] : string.Empty;       

		if(postUrl.Length > 0)
			writer.WriteLine(@"<a target=""_blank"" href=""" +  HttpUtility.HtmlAttributeEncode(postUrl) + @""">" + HttpUtility.HtmlEncode(postTitle) + @"</a>");
		else
			writer.WriteLine(HttpUtility.HtmlEncode(postTitle));
	}
}