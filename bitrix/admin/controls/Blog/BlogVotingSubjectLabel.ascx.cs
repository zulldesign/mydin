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
using Bitrix.CommunicationUtility.Rating;
using Bitrix.Search;
using Bitrix;
using Bitrix.DataLayer;
using Bitrix.Blog;
using System.Web;

public partial class bitrix_admin_controls_BlogVotingSubjectLabel : BXControl
{
    protected override void OnPreRender(EventArgs e)
    {
        this.VotingSubject.Text = GetSubject();
        base.OnPreRender(e);
    }

    public string FindUrl(string moduleId, string Id, int ItemGroup)
    {
        BXSearchQuery q = new BXSearchQuery();
        q.FieldsToSelect.Add(BXSearchField.Id);
        q.FieldsToSelect.Add(BXSearchField.ModuleId);
        q.FieldsToSelect.Add(BXSearchField.ItemId);
        q.FieldsToSelect.Add(BXSearchField.Body);
        q.FieldsToSelect.Add(BXSearchField.Title);

        BXSite site = BXSite.Current;

        BXSearchContentGroupFilter filter = new BXSearchContentGroupFilter(BXFilterExpressionCombiningLogic.And);
        filter.Add(new BXFormFilterItem("moduleId", moduleId, BXSqlFilterOperators.Equal));
        filter.Add(new BXFormFilterItem("itemId", Id, BXSqlFilterOperators.Equal));
        if (ItemGroup != 0)
            filter.Add(new BXFormFilterItem("itemGroup", ItemGroup, BXSqlFilterOperators.Equal));
        q.Filter = filter;
        BXSearchResultCollection coll = q.Execute();
        if (coll.Count > 0)
        {
            BXSearchResult r = coll[0];
            if (r.Urls.Length > 0 && r.Urls[0].Length > 0)
                return r.Urls[0];
            else return string.Empty;
        }
        else
            return string.Empty;
    }

    public string GetSubject()
    {
        string typeName = Attributes["TypeName"],
            itemId = Attributes["ItemID"];

        bool displayTypeName;
        if (!bool.TryParse(Attributes["DisplayTypeName"], out displayTypeName))
            displayTypeName = false;

        if (typeName == "BLOGPOST")
        {
            BXBlogPost p = BXBlogPost.GetById(itemId, BXTextEncoder.EmptyTextEncoder);
            if (p == null)
                return "N/A";

            BXBlog b = BXBlog.GetById(p.BlogId, BXTextEncoder.EmptyTextEncoder);
            if (b == null)
                return "N/A";

            string type = displayTypeName ? GetMessage("TypeName.BlogPost", true) + " / " : string.Empty,
                title = HttpUtility.HtmlEncode(b.Name + " / " + p.Title);

            string url = FindUrl("blog", "p" + p.Id, 0);
            if (url.Length > 0)
            {
                url = url.Replace("#BlogSlug#", b.Slug);
            }

            return url.Length > 0
                ? string.Format("{0}<a href='{1}'>{2}</a>", type, HttpUtility.HtmlAttributeEncode(url), title)
                : string.Concat(type, title);
        }
        else if (typeName == "BLOGCOMMENT")
        {
            BXBlogCommentChain chain = new BXBlogCommentChain();
            BXBlogComment c = BXBlogComment.GetById(itemId, BXTextEncoder.EmptyTextEncoder);
            if (c == null)
                return "N/A";

            BXBlog b = BXBlog.GetById(c.BlogId, BXTextEncoder.EmptyTextEncoder);
            if (b == null)
                return "N/A";

            BXBlogPost p = BXBlogPost.GetById(c.PostId, BXTextEncoder.EmptyTextEncoder);
            if (p == null)
                return "N/A";

            string type = displayTypeName ? GetMessage("TypeName.BlogComment", true) + " / " : string.Empty,
            title = BXWordBreakingProcessor.Break(b.Name + " / " + p.Title, 30, true),
            text = BXWordBreakingProcessor.Break(chain.StripBBCode(c.Content.Length <= 64 ? c.Content : c.Content.Substring(0, 61) + "..."), 30, true);

            string url = FindUrl("blog", "p" + p.Id, 0);
            if (url.Length > 0)
            {
                url = url.Replace("#BlogSlug#", b.Slug);
            }

            if (url.Length > 0)
                return string.Format(
                    "{0}<a href='{1}'>{2}</a><p style='font-size:100%;'><i>{3}</i></p>",
                    type,
                    HttpUtility.HtmlAttributeEncode(url),
                    title,
                    text);
            else
                return string.Format(
                    "{0}{1}<p style='font-size:100%;'><i>{2}</i></p>",
                    type,
                    title,
                    text); 
        }
        return "N/A";
    }
}