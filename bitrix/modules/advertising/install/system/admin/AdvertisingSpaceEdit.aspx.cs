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
using Bitrix.Services.Text;
using Bitrix;
using Bitrix.DataLayer;
using Bitrix.Modules;
using System.Collections.Generic;
using Bitrix.Security;
using Bitrix.Configuration;
using Bitrix.Advertising;

public enum AdvertisingSpaceEditorMode
{
    Creation = 1,
    Modification = 2
}

public enum AdvertisingSpaceEditorError
{
    None                            = 0,
    AdvertisingSpaceIsNotFound      = -1,
    AdvertisingSpaceCreation        = -2,
    AdvertisingSpaceModification    = -3,
    AdvertisingSpaceDeleting        = -4
}

public partial class bitrix_admin_AdvertisingSpaceEdit : BXAdminPage
{
	private int? _advertisingSpaceId = null;
	protected int AdvertisingSpaceId
	{
        get 
        {
            if (_advertisingSpaceId.HasValue)
                return _advertisingSpaceId.Value;

            string advertisingSpaceIdStr = Request.QueryString["id"];
            if (string.IsNullOrEmpty(advertisingSpaceIdStr))
                _advertisingSpaceId = 0;
            else
            {
                try
                {
                    _advertisingSpaceId = Convert.ToInt32(advertisingSpaceIdStr);
                }
                catch (Exception /*exc*/)
                {
                    _advertisingSpaceId = 0;
                }
            }
            return _advertisingSpaceId.Value; 
        }
	}

    private AdvertisingSpaceEditorMode _editorMode = AdvertisingSpaceEditorMode.Creation;
    protected AdvertisingSpaceEditorMode EditorMode
    {
        get { return _editorMode; }
    }

    private BXAdvertisingSpace _advertisingSpace;
    protected BXAdvertisingSpace AdvertisingSpace
    {
        get { return _advertisingSpace; }
    }

    private string _errorMessage = string.Empty;
    protected string ErrorMessage
    {
        get { return _errorMessage; }
    }

    private AdvertisingSpaceEditorError _editorError = AdvertisingSpaceEditorError.None;
    protected AdvertisingSpaceEditorError EditorError
    {
        get { return _editorError; }
    }

	protected override string BackUrl
	{
		get { return !string.IsNullOrEmpty(base.BackUrl) ? base.BackUrl : "AdvertisingSpaceList.aspx"; }
	}

    protected override void OnInit(EventArgs e)
    {

        if (!BXPrincipal.Current.IsCanOperate(BXAdvertisingModule.Operations.SpaceManagement))
            BXAuthentication.AuthenticationRequired();

        base.OnInit(e);
    }

    protected override void OnLoad(EventArgs e)
    {
        if(IsPostBack)
            Validate();

        TryLoadAdvertisingSpace();
        BXAdvertisingSpace space = AdvertisingSpace;
        string title = EditorMode == AdvertisingSpaceEditorMode.Modification ? string.Format(GetMessage("PageTitle.EditAdvertisingSpace"), space != null ? HttpUtility.HtmlEncode(space.Name) : "?") : GetMessage("PageTitle.CreateAdvertisingSpace");

        MasterTitle = title;
        Page.Title = title;

        base.OnLoad(e);
    }

    private void TryLoadAdvertisingSpace()
	{
        int id = AdvertisingSpaceId;
        if (id <= 0)
        {
            _editorMode = AdvertisingSpaceEditorMode.Creation;
            _advertisingSpace = new BXAdvertisingSpace();
        }
        else
        {
            _editorMode = AdvertisingSpaceEditorMode.Modification;
            if ((_advertisingSpace = BXAdvertisingSpace.GetById(id, BXTextEncoder.EmptyTextEncoder)) == null)
            {
                _errorMessage = string.Format(GetMessageRaw("Error.UnableToFindAdvertisingSpace"), id);
                _editorError = AdvertisingSpaceEditorError.AdvertisingSpaceIsNotFound;
                return;
            }
        }

        if (!IsPostBack)
        {
            AdvertisingSpaceCode.Text = _advertisingSpace.Code;
            AdvertisingSpaceActive.Checked = _advertisingSpace.Active;
            AdvertisingSpaceName.Text = _advertisingSpace.Name;
            AdvertisingSpaceDescription.Text = _advertisingSpace.Description;
            AdvertisingSpaceSort.Text = _advertisingSpace.Sort.ToString();
            AdvertisingSpaceXmlId.Text = _advertisingSpace.XmlId;
        }
        else
        {
            _advertisingSpace.Code = AdvertisingSpaceCode.Text;
            _advertisingSpace.Active = AdvertisingSpaceActive.Checked;
            _advertisingSpace.Name = AdvertisingSpaceName.Text;
            _advertisingSpace.Description = AdvertisingSpaceDescription.Text;
            try
            {
                _advertisingSpace.Sort = Convert.ToInt32(AdvertisingSpaceSort.Text);
            }
            catch (Exception /*exc*/)
            {
            }
            _advertisingSpace.XmlId = AdvertisingSpaceXmlId.Text;
        }
	}

    private void TrySaveAdvertisingSpace()
    {
        if (_advertisingSpace == null)
            return;

        try
        {
            BXUser user = BXIdentity.Current != null ? BXIdentity.Current.User : null;
            if (user != null)
            {
                if (EditorMode == AdvertisingSpaceEditorMode.Creation)
                    _advertisingSpace.AuthorId = user.UserId;
                _advertisingSpace.LastModificationAuthorId = user.UserId;
            }
            _advertisingSpace.Save();
        }
        catch (Exception exc)
        {
            _errorMessage = exc.Message;
            _editorError = EditorMode == AdvertisingSpaceEditorMode.Creation ? AdvertisingSpaceEditorError.AdvertisingSpaceCreation : AdvertisingSpaceEditorError.AdvertisingSpaceModification; 
        }
    }

    protected override void OnPreRender(EventArgs e)
    {
        if (EditorMode != AdvertisingSpaceEditorMode.Modification)
        {
            AddButton.Visible = false;
            DeleteButton.Visible = false;
        }

        if (EditorError != AdvertisingSpaceEditorError.None)
        {
            errorMessage.AddErrorMessage(ErrorMessage);
            if (EditorMode == AdvertisingSpaceEditorMode.Modification)
            {
                if (EditorError == AdvertisingSpaceEditorError.AdvertisingSpaceIsNotFound)
                    TabControl.Visible = false;
            }
        }

        base.OnPreRender(e);
    }


    protected void OnAdvertisingSpaceEdit(object sender, BXTabControlCommandEventArgs e)
	{
		switch (e.CommandName)
		{
            case "save":
                {
                    if (IsValid)
                    {
                        TrySaveAdvertisingSpace();
                        if (EditorError == AdvertisingSpaceEditorError.None)
                            GoBack(); 
                    }
                }
                break;
            case "apply":
                {
                    if (IsValid)
                    {
                        TrySaveAdvertisingSpace();
                        if (EditorError == AdvertisingSpaceEditorError.None)
                            Response.Redirect(string.Format("AdvertisingSpaceEdit.aspx?id={0}&tabindex={1}", AdvertisingSpaceId.ToString(), TabControl.SelectedIndex));
                    }
                }
                break;
			case "cancel":
				GoBack();
				break;
		}
	}

	protected void OnToolBarButtonClick(object sender, CommandEventArgs e)
	{
        if (!IsValid)
            return;

		if (e.CommandName == "delete")
		{
			try
			{
                BXAdvertisingSpace space = AdvertisingSpace;
                if (space != null)
                    space.Delete();

				GoBack();
			}
			catch (Exception ex)
			{
                _errorMessage = ex.Message;
                _editorError = AdvertisingSpaceEditorError.AdvertisingSpaceDeleting;
			}
		}
	}
}
