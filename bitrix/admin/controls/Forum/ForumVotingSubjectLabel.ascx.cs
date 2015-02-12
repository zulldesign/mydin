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
using Bitrix.Forum;
using System.Web;

public partial class bitrix_admin_controls_ForumVotingSubjectLabel : BXControl
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
            if (r.Urls.Length>0 && r.Urls[0].Length>0)
                return r.Urls[0];
            else return string.Empty;
        }
        else
            return string.Empty;
    }

    public string GetSubject()
    {
        string itemId = Attributes["ItemID"];

        bool displayTypeName;
        if (!bool.TryParse(Attributes["DisplayTypeName"], out displayTypeName))
            displayTypeName = false;

        BXForumPostChain stripHelper = new BXForumPostChain();
        switch (Attributes["TypeName"])
        {
            case "FORUMTOPIC":
                {
                    BXForumTopic t = BXForumTopic.GetById(itemId, BXTextEncoder.EmptyTextEncoder);
                    if (t == null)
                        return "N/A";

                    BXForum f = BXForum.GetById(t.ForumId, BXTextEncoder.EmptyTextEncoder);
                    if (f == null)
                        return "N/A";

                    string type = displayTypeName ? GetMessage("TypeName.ForumTopic", true) + " / " : string.Empty,
                        title = HttpUtility.HtmlEncode(f.Name + " / " + t.Name);
                    string searchUrl = FindUrl("forum", itemId, 2);
                    if (searchUrl.Length > 0)
                        return string.Format(
                            "{0}<a href='{1}'>{2}</a>", 
                            type,
                            HttpUtility.HtmlAttributeEncode(searchUrl),
                            title);
                    else
                       return string.Concat(type, title);
                }
            case "FORUMPOST":
                {
                    BXForumPost p = BXForumPost.GetById(itemId, BXTextEncoder.EmptyTextEncoder);
                    if (p == null)
                        return "N/A";

                    BXForum f = BXForum.GetById(p.ForumId, BXTextEncoder.EmptyTextEncoder);
                    if (f == null)
                        return "N/A";

                    BXForumTopic t = BXForumTopic.GetById(p.TopicId, BXTextEncoder.EmptyTextEncoder);
                    if (t == null)
                        return "N/A";

                    string type = displayTypeName ? GetMessage("TypeName.ForumPost", true) + " / " : string.Empty,
                        title = BXWordBreakingProcessor.Break(f.Name + " / " + t.Name, 30, true),
                        text = BXWordBreakingProcessor.Break(stripHelper.StripBBCode(p.Post.Length <= 64 ? p.Post : p.Post.Substring(0, 61) + "..."), 30, true);

                    string searchUrl = FindUrl("forum", itemId, 1);
                    if (searchUrl.Length > 0)
                        return string.Format("{0}<a href='{1}'>{2}</a><p style='font-size:100%;'><i>{3}</i></p>", type, HttpUtility.HtmlAttributeEncode(searchUrl), title, text);
                    else
                        return string.Format("{0}{1}<p style='font-size:100%;'><i>{2}</i></p>", type, title, text);
                }
            default:
                return "N/A";
        }        
    }
}