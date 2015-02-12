using System;
using System.Web.UI;

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
using System.Web;

public partial class bitrix_admin_controls_VotingSubjectLabel : BXControl
{
    public string _typeName = string.Empty;
    public string TypeName
    {
        get { return _typeName; }
        set { _typeName = value ?? string.Empty; }
    }
    private string _itemId = string.Empty;
    public string ItemId
    {
        get { return _itemId; }
        set { _itemId = value ?? string.Empty; }
    }

    private bool _displayTypeName = false;
    public bool DisplayTypeName
    {
        get { return _displayTypeName; }
        set { _displayTypeName = value; }
    }

    private bool isPrepared = false;
    private void Prepare()
    {
        if (isPrepared)
            return;

        this.VotingSubject.Text = TypeName;
        if (TypeName == "USER")
        {
            int userId;
            if (int.TryParse(ItemId, out userId))
            {
                BXUser u = BXUser.GetById(userId, BXTextEncoder.EmptyTextEncoder);
                if (u == null)
                    this.VotingSubject.Text = "N/A";
                else
                {
                    string name = u.DisplayName;
                    if (string.IsNullOrEmpty(name))
                        name = u.UserName;

                    this.VotingSubject.Text = string.Format("{0}<a href=\"AuthUsersEdit.aspx?id={1}\">{2}</a>", DisplayTypeName ? GetMessage("TypeName.User", true) + " / " : string.Empty, u.UserId.ToString(), HttpUtility.HtmlEncode(name));
                }
            }
            else
                this.VotingSubject.Text = "N/A";
        }
        else if (TypeName == "FORUMTOPIC" || TypeName == "FORUMPOST")
        {
            if (BXModuleManager.IsModuleInstalled("forum") && BXModuleManager.IsModuleInstalled("search"))
            {
                Controls.Remove(this.VotingSubject);
                BXControl label = (BXControl)LoadControl("~/bitrix/admin/controls/Forum/ForumVotingSubjectLabel.ascx");
                label.Attributes["TypeName"] = TypeName;
                label.Attributes["ItemID"] = ItemId;
                label.Attributes["DisplayTypeName"] = DisplayTypeName.ToString();
                Controls.Add(label);
            }
            else
                this.VotingSubject.Text = "N/A";
        }
        else if (TypeName == "IBLOCKELEMENT")
        {
            if (BXModuleManager.IsModuleInstalled("iblock"))
            {
                Controls.Remove(this.VotingSubject);
                BXControl label = (BXControl)LoadControl("~/bitrix/admin/controls/IBlock/IBlockVotingSubjectLabel.ascx");
                label.Attributes["TypeName"] = TypeName;
                label.Attributes["ItemID"] = ItemId;
                label.Attributes["DisplayTypeName"] = DisplayTypeName.ToString();
                Controls.Add(label);
            }
            else
                this.VotingSubject.Text = "N/A";
        }
        else if (TypeName == "BLOGPOST" || TypeName == "BLOGCOMMENT")
        {
            if (BXModuleManager.IsModuleInstalled("blog") && BXModuleManager.IsModuleInstalled("search"))
            {
                Controls.Remove(this.VotingSubject);
                BXControl label = (BXControl)LoadControl("~/bitrix/admin/controls/Blog/BlogVotingSubjectLabel.ascx");
                label.Attributes["TypeName"] = TypeName;
                label.Attributes["ItemID"] = ItemId;
                label.Attributes["DisplayTypeName"] = DisplayTypeName.ToString();
                Controls.Add(label);
            }
        }
        else
            this.VotingSubject.Text = TypeName;

        isPrepared = true;
    }

    protected override void OnDataBinding(EventArgs e)
    {
        base.OnDataBinding(e);
        isPrepared = false;
        Prepare();
    }
}