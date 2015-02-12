using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bitrix.UI;
using Bitrix.Services.Promo;
using System.Text;
using Bitrix.Services.Js;
using System.Web.Script.Serialization;
using Bitrix.Security;
using Bitrix.Services;
using Bitrix.Services.Text;


public partial class bitrix_admin_PromoRuleEdit : BXAdminPage
{
    private BXNestedTypeInfo[] conditionInfos = null;
    protected BXNestedTypeInfo[] ConditionInfos
    {
        get { return this.conditionInfos ?? (this.conditionInfos = BXPromoManager.GetConditionInfos()); }
    }

    private string conditionInfosJson = null;
    protected string ConditionInfosJson
    {
        get 
        {
            if (this.conditionInfosJson != null)
                return this.conditionInfosJson;

            if (ConditionInfos.Length == 0)
                return (this.conditionInfosJson = "[]");

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.RegisterConverters(new JavaScriptConverter[] { new BXPromoConditionTypeInfoJsonConv() });
            return this.conditionInfosJson = serializer.Serialize(ConditionInfos);
        }
    }

    private BXPromoConditionConfigEditor[] conditionConfigEditors = null;
    protected BXPromoConditionConfigEditor[] ConditionConfigEditors
    {
        get 
        {
            if (this.conditionConfigEditors != null)
                return this.conditionConfigEditors;

            this.conditionConfigEditors = BXPromoManager.CreateConditionConfigEditors();
            if (Entity != null)
            {
                BXPromoConditionConfig config = Entity.GetConditionConfig();
                if(config != null)
                    for (int i = 0; i < this.conditionConfigEditors.Length; i++)
                    {
                        BXPromoConditionConfigEditor ed = this.conditionConfigEditors[i];
                        if (ed.Info != null
                            && string.Equals(ed.Info.TypeInfo.Name, config.Type.FullName, StringComparison.OrdinalIgnoreCase))
                        {
                            ed.Config = config;
                            break;
                        }
                    }
            }
            return this.conditionConfigEditors;
        }
    }

    protected string GetConditionConfigEditorsJson()
    {
        if (ConditionConfigEditors.Length == 0)
            return "[]";

        return PrepareConditionConfigEditorJsonSerializer().Serialize(ConditionConfigEditors);
    }

    private JavaScriptSerializer PrepareConditionConfigEditorJsonSerializer()
    {
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        List<JavaScriptConverter> convLst = null;
        for (int i = 0; i < ConditionConfigEditors.Length; i++)
        {
            BXPromoConditionConfigEditor ed = ConditionConfigEditors[i];
            JavaScriptConverter[] edConvAry = ed.CreateJsonConverters();
            if (edConvAry != null && edConvAry.Length > 0)
            {
                for (int j = 0; j < edConvAry.Length; j++)
                {
                    JavaScriptConverter edConv = edConvAry[j];
                    if (edConv == null)
                        continue;

                    if (convLst == null 
                        || convLst.FindIndex(delegate(JavaScriptConverter obj) { return obj.GetType() == edConv.GetType(); }) < 0)
                        (convLst ?? (convLst = new List<JavaScriptConverter>())).Add(edConv);
                }
            }
        }
        if(convLst != null && convLst.Count > 0)
            serializer.RegisterConverters(convLst);
        return serializer;
    }

    private BXPromoConditionConfig clientConditionConfig = null;
    protected BXPromoConditionConfig ClientConditionConfig
    {
        get 
        {
            if (this.clientConditionConfig != null)
                return this.clientConditionConfig;

            BXPromoConditionConfigEditor ed = PrepareConditionConfigEditorJsonSerializer().Deserialize<BXPromoConditionConfigEditor>(ClientConditionData.Value);
            if(ed != null)
                this.clientConditionConfig = (BXPromoConditionConfig)ed.Config;

            return this.clientConditionConfig;
        }
    }

    private BXNestedTypeInfo[] actionInfos = null;
    protected BXNestedTypeInfo[] ActionInfos
    {
        get { return this.actionInfos ?? (this.actionInfos = BXPromoManager.GetActionInfos()); }
    }

    private string actionInfosJson = null;
    protected string ActionInfosJson
    {
        get
        {
            if (this.actionInfosJson != null)
                return this.actionInfosJson;

            if (ActionInfos.Length == 0)
                return (this.actionInfosJson = "[]");

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.RegisterConverters(new JavaScriptConverter[] { new BXPromoActionTypeInfoJsonConv() });
            return this.actionInfosJson = serializer.Serialize(ActionInfos);
        }
    }

    private BXPromoActionConfigEditor[] actionConfigEditors = null;
    protected BXPromoActionConfigEditor[] ActionConfigEditors
    {
        get
        {
            if (this.actionConfigEditors != null)
                return this.actionConfigEditors;

            this.actionConfigEditors = BXPromoManager.CreateActionConfigEditors();
            if (Entity != null)
            {
                BXPromoActionConfig config = Entity.GetActionConfig();
                if (config != null)
                    for (int i = 0; i < this.actionConfigEditors.Length; i++)
                    {
                        BXPromoActionConfigEditor ed = this.actionConfigEditors[i];
                        if (ed.Info != null
                            && string.Equals(ed.Info.TypeInfo.Name, config.Type.FullName, StringComparison.OrdinalIgnoreCase))
                        {
                            ed.Config = config;
                            break;
                        }
                    }
            }
            return this.actionConfigEditors;
        }
    }

    protected string GetActionConfigEditorsJson()
    {
        if (ActionConfigEditors.Length == 0)
            return "[]";

        return PrepareActionConfigEditorJsonSerializer().Serialize(ActionConfigEditors);
    }

    private JavaScriptSerializer PrepareActionConfigEditorJsonSerializer()
    {
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        List<JavaScriptConverter> convLst = null;
        for (int i = 0; i < ActionConfigEditors.Length; i++)
        {
            BXPromoActionConfigEditor ed = ActionConfigEditors[i];
            JavaScriptConverter[] edConvAry = ed.CreateJsonConverters();
            if (edConvAry != null && edConvAry.Length > 0)
            {
                for (int j = 0; j < edConvAry.Length; j++)
                {
                    JavaScriptConverter edConv = edConvAry[j];
                    if (edConv == null)
                        continue;

                    if (convLst == null
                        || convLst.FindIndex(delegate(JavaScriptConverter obj) { return obj.GetType() == edConv.GetType(); }) < 0)
                        (convLst ?? (convLst = new List<JavaScriptConverter>())).Add(edConv);
                }
            }
        }
        if (convLst != null && convLst.Count > 0)
            serializer.RegisterConverters(convLst);
        return serializer;
    }

    private BXPromoActionConfig clientActionConfig = null;
    protected BXPromoActionConfig ClientActionConfig
    {
        get
        {
            if (this.clientActionConfig != null)
                return this.clientActionConfig;

            BXPromoActionConfigEditor ed = PrepareActionConfigEditorJsonSerializer().Deserialize<BXPromoActionConfigEditor>(ClientActionData.Value);
            if (ed != null)
                this.clientActionConfig = (BXPromoActionConfig)ed.Config;

            return this.clientActionConfig;
        }
    }

    protected override void OnInit(EventArgs e)
    {
        if (!BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.PromotionManagement))
            BXAuthentication.AuthenticationRequired();

        base.OnInit(e);

        ListItemCollection boundEntityTypeItems = BoundEntityType.Items;
        boundEntityTypeItems.Add(new ListItem(GetMessageRaw("NotSelectedMasculine"), string.Empty));

        foreach (BXTypeInfo info in BXPromoManager.GetPromotableEntityTypeInfos())
            boundEntityTypeItems.Add(new ListItem(info.Description, info.Name));


        ListItemCollection conditionTypeItems = ConditionType.Items;
        conditionTypeItems.Add(new ListItem(GetMessageRaw("NotSelectedNeuter"), string.Empty));

        foreach (BXNestedTypeInfo info in ConditionInfos)
            conditionTypeItems.Add(new ListItem(info.TypeInfo.Description, info.TypeInfo.Name));

        ListItemCollection actionTypeItems = ActionType.Items;
        actionTypeItems.Add(new ListItem(GetMessageRaw("NotSelectedFeminine"), string.Empty));

        foreach (BXNestedTypeInfo info in ActionInfos)
            actionTypeItems.Add(new ListItem(info.TypeInfo.Description, info.TypeInfo.Name));
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        if (EntityId == 0)
            this.entity = new BXPromoRule();
        else if ((this.entity = BXPromoRule.GetById(EntityId, BXTextEncoder.EmptyTextEncoder)) == null)
        {
            this.editorError = PromoRuleEditorError.IsNotFound;
            this.errorMessageText = GetMessageRaw("Message.PromoRuleIsNotFound");
            return;
        }
        EditorMode = this.entity.IsNew ? BXAdminPageEditorMode.Creation : BXAdminPageEditorMode.Modification;		

        if (!IsPostBack)
        {
			for (int i = 0; i < ConditionType.Items.Count; i++ )
			{
				if (string.Equals(ConditionType.Items[i].Value, entity.ConditionTypeFullName, StringComparison.OrdinalIgnoreCase))
				{
					ConditionType.SelectedIndex = i;
					break;
				}
			}

			for (int i = 0; i < ActionType.Items.Count; i++ )
			{
				if (string.Equals(ActionType.Items[i].Value, entity.ActionTypeFullName, StringComparison.OrdinalIgnoreCase))
				{
					ActionType.SelectedIndex = i;
					break;
				}
			}

            if (!this.entity.IsNew)
            {
                Active.Checked = this.entity.Active;
                Name.Text = this.entity.Name;
                ListItem boundTypeItem = BoundEntityType.Items.FindByValue(this.entity.BoundEntityTypeId);
                if (boundTypeItem != null)
                    boundTypeItem.Selected = true;

                XmlId.Text = this.entity.XmlId;
            }
            else
            {
                Active.Checked = true;
                Name.Text = GetMessageRaw("PromoRuleDefaultName");
                ListItem boundTypeItem = BoundEntityType.Items.FindByValue("USER");
                if (boundTypeItem != null)
                    boundTypeItem.Selected = true;
            }
            InitialConditionData.Value = GetConditionConfigEditorsJson();
            InitialActionData.Value = GetActionConfigEditorsJson();
        }

        MasterTitle = Page.Title = EditorMode == BXAdminPageEditorMode.Creation ? GetMessage("PageTitle.Creation") : string.Concat(GetMessage("PageTitle.Modification"), " #", EntityId.ToString());
    }

    protected override void OnPreRender(EventArgs e)
    {
        base.OnPreRender(e);

        if (EditorError != PromoRuleEditorError.None)
        {
            errorMessage.AddErrorMessage(ErrorMessageText);
            if (EntityId > 0 && EditorError == PromoRuleEditorError.IsNotFound)
                TabControl.Visible = false;
        }

        BXPage.Scripts.RequireUtils();

        for (int i = 0; i < ConditionConfigEditors.Length; i++)
            RegisterScriptIncludeBatch(ConditionConfigEditors[i].GetClientSideFilePaths());

        for (int i = 0; i < ActionConfigEditors.Length; i++)
            RegisterScriptIncludeBatch(ActionConfigEditors[i].GetClientSideFilePaths());

        Page.ClientScript.RegisterOnSubmitStatement(GetType(), "SaveSettings", "Bitrix.PromoRuleEditor.getInstance().saveSettings();");

        if (EditorMode != BXAdminPageEditorMode.Modification)
        {
            AddButton.Visible = false;
            DeleteButton.Visible = false;
        }
    }

    protected override void Render(HtmlTextWriter writer)
    {
        if (ConditionInfos.Length > 0 && ClientScript != null)
            for (int i = 0; i < ConditionInfos.Length; i++)
                ClientScript.RegisterForEventValidation(ConditionType.UniqueID, ConditionInfos[i].TypeInfo.Name);
        base.Render(writer);
    }

    protected void ValidateDropDownList(Object source, ServerValidateEventArgs aruments)
    {
        aruments.IsValid = !string.IsNullOrEmpty(aruments.Value);
    }

    private int? entityId = null;
    protected int EntityId
    {
        get
        {
            if (this.entityId.HasValue)
                return entityId.Value;

            string str = Request.QueryString["id"];
            if (string.IsNullOrEmpty(str))
                this.entityId = 0;
            else
            {
                try
                {
                    this.entityId = Convert.ToInt32(Request.QueryString["id"]);
                }
                catch
                {
                    this.entityId = 0;
                }
            }
            return this.entityId.Value;
        }
    }

    protected override string BackUrl
    {
        get { return !string.IsNullOrEmpty(base.BackUrl) ? base.BackUrl : "PromoRuleList.aspx"; }
    }

    private BXPromoRule entity = null;
    protected BXPromoRule Entity
    {
        get { return this.entity; }
    }

    private void TrySave()
    {
        if (this.editorError != PromoRuleEditorError.None)
            return;

        if (this.entity == null)
        {
            this.editorError = PromoRuleEditorError.IsNotFound;
            this.errorMessageText = GetMessageRaw("Message.PromoRuleIsNotFound");
            return;
        }

        try
        {
            this.entity.Active = Active.Checked;
            this.entity.Name = Name.Text;
            this.entity.BoundEntityTypeId = BoundEntityType.SelectedValue;
            this.entity.SetConditionConfig(ClientConditionConfig);
            this.entity.SetActionConfig(ClientActionConfig);
            this.entity.XmlId = XmlId.Text;

            this.entity.Save();
            this.entityId = this.entity.Id;
        }
        catch (Exception exc)
        {
            this.errorMessageText = exc.Message;
            this.editorError = this.entity.IsNew ? PromoRuleEditorError.Creation : PromoRuleEditorError.Modification;
        }
    }

    private BXAdminPageEditorMode _editorMode = BXAdminPageEditorMode.Creation;
    protected BXAdminPageEditorMode EditorMode
    {
        get { return _editorMode; }
        private set { _editorMode = value; }
    }

    private string errorMessageText = string.Empty;
    protected string ErrorMessageText
    {
        get { return this.errorMessageText; }
    }

    private PromoRuleEditorError editorError = PromoRuleEditorError.None;
    protected PromoRuleEditorError EditorError
    {
        get { return this.editorError; }
    }

    protected void OnToolBarButtonClick(object sender, CommandEventArgs e)
    {
        if (e.CommandName == "delete")
        {
            try
            {
                if (Entity != null)
                    Entity.Delete();

                GoBack("~/bitrix/admin/PromoRuleList.aspx");
            }
            catch (Exception ex)
            {
                this.errorMessageText = ex.Message;
                this.editorError = PromoRuleEditorError.Deleting;
            }
        }
    }

    protected void OnTabControlCommand(object sender, BXTabControlCommandEventArgs e)
    {
        switch (e.CommandName)
        {
            case "save":
                {
                    if (IsValid && EditorError == PromoRuleEditorError.None)
                    {
                        TrySave();
                        if (EditorError == PromoRuleEditorError.None)
                            GoBack("~/bitrix/admin/PromoRuleList.aspx");
                    }
                }
                break;
            case "apply":
                {
                    if (IsValid && EditorError == PromoRuleEditorError.None)
                    {
                        TrySave();
                        if (EditorError == PromoRuleEditorError.None && Entity != null)
                            Response.Redirect(string.Format("PromoRuleEdit.aspx?id={0}&tabindex={1}", Entity.Id.ToString(), TabControl.SelectedIndex));
                    }
                }
                break;
            case "cancel":
                GoBack();
                break;
        }
    }
}

public enum PromoRuleEditorError
{
    None = 0,
    IsNotFound = -1,
    Creation = -2,
    Modification = -3,
    Deleting = -4
}