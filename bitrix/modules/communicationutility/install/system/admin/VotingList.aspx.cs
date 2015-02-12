using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;
using Bitrix.UI;
using Bitrix.CommunicationUtility;
using Bitrix.CommunicationUtility.Rating;
using Bitrix.Search;
using Bitrix;
using Bitrix.Security;
using Bitrix.DataLayer;
using Bitrix.Services.Text;
using Bitrix.Services.Rating;
using Bitrix.Services;
using Bitrix.Modules;

public partial class bitrix_admin_VotingList : BXAdminPage
{
    protected override void OnInit(EventArgs e)
    {
        if (!BXPrincipal.Current.IsCanOperate(BXRatingVoting.Operations.RatingVotingAdminManagement))
            BXAuthentication.AuthenticationRequired();

        base.OnInit(e);
    }

    protected override void OnLoad(EventArgs e)
    {
        MasterTitle = Page.Title = GetMessage("PageTitle");
        base.OnLoad(e);
    }

    protected override void OnPreRender(EventArgs e)
    {
        BXPage.Scripts.RequireUtils();

        foreach (ListItem l in BoundEntityTypeId.Values)
        {
            if (!BXModuleManager.IsModuleInstalled("forum") && (l.Value == "FORUMPOST" || l.Value == "FORUMTOPIC"))
                l.Enabled = false;
            else if (!BXModuleManager.IsModuleInstalled("blog") && (l.Value == "BLOGPOST" || l.Value == "BLOGCOMMENT"))
                l.Enabled = false;
            else if (!BXModuleManager.IsModuleInstalled("iblock") && (l.Value == "IBLOCKELEMENT"))
                l.Enabled = false;
        }
        ClientScript.RegisterStartupScript(GetType(), "PrepareGridLegend", "Bitrix.VotingListGridLegend.prepare();", true);
        base.OnPreRender(e);
    }

    private struct SearchMatch
    {
        public void Set(string _id, string _moduleId)
        {
            this.Id = _id;
            this.ModuleId = _moduleId;
        }
        public string Id;
        public string ModuleId;
    }

    private static bool TypeFilterExist(BXFormFilterItem f)
    {
        if (f.filterName == "BoundEntityTypeId" && 
            !String.IsNullOrEmpty(f.filterValue.ToString()))
            return true;
        else
            return false;
    }

    private static bool VotingSubjectContainsExist(BXFormFilterItem f)
    {
        if (f.filterName == "VotingSubjectContains" && f.filterValue.ToString()!="")
            return true;
        else
            return false;
    }

    private bool TryGetModuleId(string boundEntityTypeId, out string moduleId)
    {
        if (string.IsNullOrEmpty(boundEntityTypeId))
        {
            moduleId = string.Empty;
            return false;
        }

        switch (boundEntityTypeId)
        {
            case "FORUMPOST":
            case "FORUMTOPIC":
                {
                    moduleId = "forum";
                    return true;
                }
            case "IBLOCKELEMENT":
                {
                    moduleId = "iblock";
                    return true;
                }
            case "BLOGPOST":
                {
                    moduleId = "blog";
                    return true;
                }
            case "BLOGCOMMENT":
                {
                    //поиск по комментариям не поддерживается
                    ErrorMessage.AddErrorText(GetMessage("Error.SearchInCommentsIsNotSupported"));
                    moduleId = string.Empty; 
                    return false;
                }
            case "USER":
                {
                    moduleId = "user";
                    return true;
                }
            default:
                {
                    moduleId = string.Empty;
                    return false; 
                }
        }
    }

    protected List<BXSearchResult> SearchItems(string ModuleId, string searchQuery)
    {
        BXSearchQuery q = new BXSearchQuery();

        q.FieldsToSelect.Add(BXSearchField.Id);
        q.FieldsToSelect.Add(BXSearchField.ModuleId);
        q.FieldsToSelect.Add(BXSearchField.ItemId);
        q.FieldsToSelect.Add(BXSearchField.Body);
        q.FieldsToSelect.Add(BXSearchField.Title);
        q.PagingOptions = new BXPagingOptions(0, int.MaxValue);

        BXSite site = BXSite.Current;

        List<BXSearchResult> result = new List<BXSearchResult>();
        q.QueryExpression = new BXSearchExpression(site.LanguageId, searchQuery);
        q.Filter = new BXSearchContentGroupFilter(
            new BXFormFilter(new BXFormFilterItem("moduleId", ModuleId, BXSqlFilterOperators.Equal))
        );

        foreach (BXSearchResult r in q.Execute())
            result.Add(r);

        return result.Count > 0 ? result : null;
    }

    private bool typeSelectError = false;

    protected void ItemGrid_Select(object sender, BXSelectEventArgs e)
    {
        if (typeSelectError != true)
            typeSelectError = false;

        List<RatingVotingWrapper> list = new List<RatingVotingWrapper>();
        
        BXFilter f = new BXFilter(ItemFilter.CurrentFilter, BXRatingVoting.Fields);
        //f.Add(new BXFilterItem(BXRatingVoting.Fields.TotalVotes, BXSqlFilterOperators.Greater, 0));

        //если задан тип элемента и поисковая фраза - ищем поиском
        BXFilterItem searchFilter = null;

        if (ItemFilter.CurrentFilter.Exists(TypeFilterExist) && ItemFilter.CurrentFilter.Exists(VotingSubjectContainsExist))
        {
            #region запускаем поиск по введенной строке
            //ErrorMessage.Visible = false;
            List<BXSearchResult> results = new List<BXSearchResult>();

            string boundEntityTypeId = string.Empty,
                moduleSearchId = string.Empty, 
                searchQuery = string.Empty;
            BXFormFilterItem tmp = null;
            foreach (BXFormFilterItem f1 in ItemFilter.CurrentFilter)
            {
                if (f1.filterName == "BoundEntityTypeId")
                    boundEntityTypeId = f1.filterValue.ToString();
                else if (f1.filterName == "VotingSubjectContains")
                {
                    searchQuery = f1.filterValue.ToString();
                    //удаляем фильтр из списка по VotingSubjectContains, 
                    //чтобы не создавать лишних условий в фильтре
                    tmp = f1;
                }
            }

            ItemFilter.CurrentFilter.Remove(tmp);
            //если опознан модуль и есть результаты поиска по словосочетанию
            if (TryGetModuleId(boundEntityTypeId, out moduleSearchId))
                switch (moduleSearchId)
                {
                    case "iblock":
                        List<int> iblockElementIds = new List<int>();
                        results = SearchItems(moduleSearchId, searchQuery);
                        foreach (BXSearchResult r in results)
                        {
                            int var_i;
                            int.TryParse(r.Content.ItemId, out var_i);
                            iblockElementIds.Add(var_i);
                        }
                        searchFilter = new BXFilterItem(BXRatingVoting.Fields.BoundEntityId, BXSqlFilterOperators.In, iblockElementIds);
                        f.Add(searchFilter);
                        break;
                    case "blog":
                        List<int> blogPostIds = new List<int>();
                        results = SearchItems(moduleSearchId, searchQuery);
                        foreach (BXSearchResult r in results)
                        {
                            int var_i;
                            string var_s = r.Content.ItemId;
                            var_s = var_s.Remove(0, 1);
                            int.TryParse(var_s, out var_i);
                            blogPostIds.Add(var_i);
                        }
                        searchFilter = new BXFilterItem(BXRatingVoting.Fields.BoundEntityId, BXSqlFilterOperators.In, blogPostIds);
                        f.Add(searchFilter);
                        break;
                    case "forum":
                        List<int> forumPostIds = new List<int>();
                        results = SearchItems(moduleSearchId, searchQuery);
                        foreach (BXSearchResult r in results)
                        {
                            int var_i;
                            int.TryParse(r.Content.ItemId, out var_i);
                            forumPostIds.Add(var_i);
                        }
                        searchFilter = new BXFilterItem(BXRatingVoting.Fields.BoundEntityId, BXSqlFilterOperators.In, forumPostIds);
                        f.Add(searchFilter);
                        break;
                    case "user":
                        List<int> userIds = new List<int>();
                        BXUserCollection users = BXUserManager.GetList(new BXFormFilter(new BXFormFilterItem("username", searchQuery, BXSqlFilterOperators.Equal)), new BXOrderBy_old());
                        if (users.Count > 0)
                            searchFilter = new BXFilterItem(BXRatingVoting.Fields.BoundEntityId, BXSqlFilterOperators.Equal, users[0].UserId);
                        else
                        {
                            e.Data = list;
                            return;
                        }
                        break;
                    default:
                        e.Data = list;
                        return;
                }
            #endregion
        }
        else if (ItemFilter.CurrentFilter.Exists(VotingSubjectContainsExist) && !typeSelectError)
        {
            //ErrorMessage.Visible = true;
            ErrorMessage.AddErrorText(GetMessage("Error.ChooseSubjectType"));
            typeSelectError = true;
            e.Data = list;
            return;
        }

        BXSelect s = new BXSelect(
            BXSelectFieldPreparationMode.Normal,
            BXRatingVoting.Fields.Id,
            BXRatingVoting.Fields.Active,
            BXRatingVoting.Fields.BoundEntityTypeId,
            BXRatingVoting.Fields.BoundEntityId,      //за что или за кого голосуют
            BXRatingVoting.Fields.CreatedUtc,         //дата создания
            BXRatingVoting.Fields.LastCalculatedUtc,  //дата последней модификации голосования
            BXRatingVoting.Fields.TotalValue,         //суммарное значение голосов
            BXRatingVoting.Fields.TotalPositiveVotes, //количество голосов "за"
            BXRatingVoting.Fields.TotalNegativeVotes, //количество голосов "против"
            BXRatingVoting.Fields.TotalVotes,         //количество голосов
            BXRatingVoting.Fields.XmlId
        );

        BXRatingVotingCollection col = BXRatingVoting.GetList(
            f,
            new BXOrderBy(BXRatingVoting.Fields, string.IsNullOrEmpty(e.SortExpression) ? "CreatedUtc DESC" : e.SortExpression),
            s,
            new BXQueryParams(e.PagingOptions),
            BXTextEncoder.EmptyTextEncoder
        );

        int recalculateId = 0;

        if (!String.IsNullOrEmpty(Request["recalculate"]) && Request["recalculate"] == "Y")
            int.TryParse(Request["RecalculateId"], out recalculateId);

        foreach (BXRatingVoting item in col)
        {
            if (recalculateId > 0 && item.Id == recalculateId) 
                item.Calculate(true);
            list.Add(new RatingVotingWrapper(item, this));
        }
        e.Data = list;
    }

    protected void ItemGrid_SelectCount(object sender, BXSelectCountEventArgs e)
    {
        //ItemFilter.CurrentFilter.Add(new BXFormFilterItem("TotalVotes", 0, BXSqlFilterOperators.Greater));
        e.Count = BXRatingVoting.Count(new BXFilter(ItemFilter.CurrentFilter, BXRatingVoting.Fields));
    }

    protected void ItemGrid_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType != DataControlRowType.DataRow)
            return;

        BXGridViewRow row = (BXGridViewRow)e.Row;
        RatingVotingWrapper wrapper = (RatingVotingWrapper)row.DataItem;

        row.UserData.Add("ID", wrapper.ID);
    }

    protected void GridView1_Update(object sender, BXUpdateEventArgs e)
    {
        BXRatingVoting v = BXRatingVoting.GetById(e.Keys["ID"]);
        v.XmlId = e.NewValues["XmlId"].ToString();
        v.Update();
    }

    protected void ItemGrid_Delete(object sender, BXDeleteEventArgs e)
    {
        //if (!BXPrincipal.Current.IsCanOperate())
        //    return;
        BXRatingVotingCollection col;
        try
        {
            BXFilter filter = (e.Keys == null)
                ? new BXFilter(ItemFilter.CurrentFilter, BXRatingVoting.Fields)
                : new BXFilter(new BXFilterItem(BXRatingVoting.Fields.Id, BXSqlFilterOperators.Equal, e.Keys["ID"]));
            col = BXRatingVoting.GetList(filter, null);
        }
        catch (Exception ex)
        {
            ErrorMessage.AddErrorMessage(ex.Message);
            return;
        }
        bool errorTextAdded = false;
        foreach (BXRatingVoting item in col)
        {
            try
            {
                item.Delete();
                e.DeletedCount++;
            }
            catch (Exception ex2)
            {
                if (!errorTextAdded)
                {
                    ErrorMessage.AddErrorMessage(GetMessage("RatingVoting.DeleteFailure"));
                    errorTextAdded = true;
                }
            }
        }

    }
}

public class RatingVotingWrapper
{
    private BXRatingVoting charge = null;
    private BXAdminPage parentPage = null;
    public RatingVotingWrapper(BXRatingVoting charge, BXAdminPage parentPage)
    {
        if (charge == null)
            throw new ArgumentNullException("charge");
        this.charge = charge;

        if (parentPage == null)
            throw new ArgumentNullException("parentPage");
        this.parentPage = parentPage;
    }

    public string RatedItemId
    {
        get { return this.charge.RatedItem.Identity; }
    }

    public string ID
    {
        get { return this.charge.Id.ToString(); }
    }


    public string Created
    {
        get { return this.charge.CreatedUtc.ToLocalTime().ToString("g"); }
    }

    public string LastCalculated
    {
        get { return this.charge.LastCalculatedUtc.ToLocalTime().ToString("g"); }
    }

    public string Active
    {
        get { return this.charge.Active ? this.parentPage.GetMessageRaw("Kernel.Yes") : this.parentPage.GetMessageRaw("Kernel.No"); }
    }

    public string TotalValue
    {
        get 
        {
            if (this.charge.TotalValue > 0)
                return this.charge.TotalValue.ToString();
            else
                return "-";
        }
    }

    public string TotalVotes
    {
        get 
        {
            if (this.charge.TotalVotes > 0)
                return String.Format("<a href=\"VoteList.aspx?VotingId={0}\">{1}</a>", this.charge.Id, this.charge.TotalVotes);
            else
                return "-";
        }
    }

    public string TotalPositiveVotes
    {
        get 
        {
            if (this.charge.TotalPositiveVotes > 0)
            {
                return String.Format("<a href=\"VoteList.aspx?VotingId={0}&PositiveVotes=Y\">{1}</a>", this.charge.Id, this.charge.TotalPositiveVotes);
            }
            else
                return "-";
        }
    }

    public string TotalNegativeVotes
    {
        get
        {
            if (this.charge.TotalNegativeVotes > 0)
            {
                return String.Format("<a href=\"VoteList.aspx?VotingId={0}&NegativeVotes=Y\">{1}</a>", this.charge.Id, this.charge.TotalNegativeVotes);
            }
            else
                return "-";
        }
    }

    public string TypeName
    {
        get 
        {
            return this.charge.RatedItem.TypeName;
        }
    }

    public string XmlId
    {
        get { return this.charge.XmlId; }
    }
}