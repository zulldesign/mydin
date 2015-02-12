using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bitrix.UI;
using Bitrix.Services.Rating;
using System.Text;
using Bitrix.Services.Js;
using System.Web.Script.Serialization;
using Bitrix.Security;
using Bitrix.Services.Text;

public enum RatingEditorError
{
    None = 0,
    IsNotFound = -1,
    Creation = -2,
    Modification = -3,
    Deleting = -4
}

public partial class bitrix_admin_RatingEdit : BXAdminPage
{
    private BXRatingComponentInfo[] componentInfos = null;
    protected BXRatingComponentInfo[] ComponentInfos
    {
        get 
        {
            if (this.componentInfos == null)
            {
                this.componentInfos = BXRatingManager.GetRatingComponentInfos();
                Array.Sort<BXRatingComponentInfo>(this.componentInfos,
                    delegate(BXRatingComponentInfo x, BXRatingComponentInfo y)
                    {
                        return x.Sort != y.Sort ? (x.Sort - y.Sort) : string.Compare(x.TypeInfo.Description, y.TypeInfo.Description, StringComparison.CurrentCulture);
                    }
                    );
            }
            return this.componentInfos;
        }
    }

    private string componentInfosJson = null;
    protected string ComponentInfosJson
    {
        get 
        {
            if (this.componentInfosJson != null)
                return this.componentInfosJson;

            if (ComponentInfos.Length == 0)
                return (this.componentInfosJson = "[]");

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.RegisterConverters(new JavaScriptConverter[] { new BXRatingComponentInfoJsonConv() });
            return this.componentInfosJson = serializer.Serialize(ComponentInfos);
        }
    }

    private BXRatingComponentConfigEditor[] componentConfigEditors = null;
    protected BXRatingComponentConfigEditor[] ComponentConfigEditors
    {
        get 
        {
            if (this.componentConfigEditors != null)
                return this.componentConfigEditors;

            this.componentConfigEditors = BXRatingManager.CreateRatingComponentConfigEditors();

            if(Entity != null)
                for (int i = 0; i < this.componentConfigEditors.Length; i++)
                {
                    BXRatingComponentConfigEditor ed = this.componentConfigEditors[i];
                    BXRatingComponentConfig config = ed.Info != null ? Entity.GetComponentConfig(ed.Info.TypeInfo.Name) : null;
                    if (config == null)
                        ed.IsSelected = false;
                    else
                    {
                        ed.Config = config;
                        ed.IsSelected = true;
                    }
                }
            return this.componentConfigEditors;
        }
    }

    protected string GetComponentConfigEditorsJson()
    {
        if (ComponentConfigEditors.Length == 0)
            return "[]";

        return PrepareComponentConfigEditorJsonSerializer().Serialize(ComponentConfigEditors);
    }

    private JavaScriptSerializer PrepareComponentConfigEditorJsonSerializer()
    {
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        List<JavaScriptConverter> converters = new List<JavaScriptConverter>();
        for (int i = 0; i < ComponentConfigEditors.Length; i++)
        {
            BXRatingComponentConfigEditor ed = ComponentConfigEditors[i];
            JavaScriptConverter[] edConverters = ed.CreateJsonConverters();
            if (edConverters != null && edConverters.Length > 0)
            {
                for (int j = 0; j < edConverters.Length; j++)
                {
                    JavaScriptConverter edConverter = edConverters[j];
                    if (edConverter == null)
                        continue;

                    if (converters.FindIndex(delegate(JavaScriptConverter obj) { return obj.GetType() == edConverter.GetType(); }) < 0)
                        converters.Add(edConverter);
                }
            }
        }
        serializer.RegisterConverters(converters);
        return serializer;
    }

    private List<BXRatingComponentConfig> clientComponentData = null;
    protected List<BXRatingComponentConfig> ClientComponentConfigList
    {
        get 
        {
            if (this.clientComponentData != null)
                return this.clientComponentData;

            this.clientComponentData = new List<BXRatingComponentConfig>();
            List<BXRatingComponentConfigEditor> clientConfigEdList = PrepareComponentConfigEditorJsonSerializer().Deserialize<List<BXRatingComponentConfigEditor>>(ClientComponentDataField.Value);
            foreach (BXRatingComponentConfigEditor clientEd in clientConfigEdList)
                if (clientEd.IsSelected)
                    this.clientComponentData.Add(clientEd.Config);

            return this.clientComponentData;
        }
    }

    private BXAdminPageEditorMode _editorMode = BXAdminPageEditorMode.Creation;
    protected BXAdminPageEditorMode EditorMode
    {
        get { return _editorMode; }
        private set { _editorMode = value; }
    }

    protected override void OnInit(EventArgs e)
    {
        if (!BXPrincipal.Current.IsCanOperate(BXRoleOperation.Operations.RatingManagement))
            BXAuthentication.AuthenticationRequired();

        base.OnInit(e);

        ListItemCollection boundEntityTypeItems = BoundEntityType.Items;
        boundEntityTypeItems.Add(new ListItem(GetMessageRaw("NotSelectedMasculine"), string.Empty));
        foreach (BXRatableEntityTypeInfo typeInfo in BXRatingManager.GetRatableEntityTypeInfos())
            boundEntityTypeItems.Add(new ListItem(typeInfo.Description, typeInfo.Name));

        ListItemCollection calculationMethodItems = CalculationMethod.Items;
        calculationMethodItems.Add(new ListItem(GetMessageRaw("RatingCalculationMethod.Sum"), BXRatingCalculationMethod.Sum.ToString("G")));
        calculationMethodItems.Add(new ListItem(GetMessageRaw("RatingCalculationMethod.Avg"), BXRatingCalculationMethod.Avg.ToString("G")));

        //ListItemCollection refreshMethodItems = RefreshMethod.Items;
        //refreshMethodItems.Add(new ListItem(GetMessageRaw("RatingRefreshMethod.Instant"), BXRatingRefreshMethod.Instant.ToString("G")));
        //refreshMethodItems.Add(new ListItem(GetMessageRaw("RatingRefreshMethod.Periodic"), BXRatingRefreshMethod.Periodic.ToString("G")));
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        if (EntityId == 0)
            this.entity = new BXRating();
        else if ((this.entity = BXRating.GetById(EntityId, BXTextEncoder.EmptyTextEncoder)) == null)
        {
            this.editorError = RatingEditorError.IsNotFound;
            this.errorMessageText = GetMessageRaw("Message.RatingIsNotFound");
            return;
        }
        EditorMode = this.entity.IsNew ? BXAdminPageEditorMode.Creation : BXAdminPageEditorMode.Modification;

        if (!IsPostBack)
        {
            if (!this.entity.IsNew)
            {
                Active.Checked = this.entity.Active;
                Name.Text = this.entity.Name;
                CurValCustomFieldName.Text = this.entity.CurValCustomFieldName;
                PrevValCustomFieldName.Text = this.entity.PrevValCustomFieldName;
                ListItem boundTypeItem = BoundEntityType.Items.FindByValue(this.entity.BoundEntityTypeId);
                if (boundTypeItem != null)
                    boundTypeItem.Selected = true;

                ListItem calculationMethItem = CalculationMethod.Items.FindByValue(this.entity.CalculationMethod.ToString("G"));
                if (calculationMethItem != null)
                    calculationMethItem.Selected = true;

                //ListItem refreshMethItem = RefreshMethod.Items.FindByValue(this.entity.RefreshMethod.ToString("G"));
                //if (refreshMethItem != null)
                //    refreshMethItem.Selected = true;

                XmlId.Text = this.entity.XmlId;
            }
            else
            {
                Active.Checked = true;
                CurValCustomFieldName.Text = "Cur#RatingId#";
                PrevValCustomFieldName.Text = "Prev#RatingId#";
                ListItem boundTypeItem = BoundEntityType.Items.FindByValue("USER");
                if (boundTypeItem != null)
                {
                    boundTypeItem.Selected = true;
                    Name.Text = GetMessageRaw("UserRating");
                }
            }
            InitialComponentData.Value = GetComponentConfigEditorsJson();
        }

        MasterTitle = Page.Title = EditorMode == BXAdminPageEditorMode.Creation ? GetMessage("PageTitle.Creation") : string.Concat(GetMessage("PageTitle.Modification"), " #", EntityId.ToString());
    }

    protected override void OnPreRender(EventArgs e)
    {
        base.OnPreRender(e);

        if (EditorError != RatingEditorError.None)
        {
            errorMessage.AddErrorMessage(ErrorMessageText);
            if (EntityId > 0 && EditorError == RatingEditorError.IsNotFound)
                TabControl.Visible = false;
        }

        BXPage.Scripts.RequireUtils();

        for (int i = 0; i < ComponentConfigEditors.Length; i++)
        {
            string[] paths = ComponentConfigEditors[i].GetClientSideFilePaths();
            if (paths == null || paths.Length == 0)
                continue;
            for (int j = 0; j < paths.Length; j++)
                BXPage.RegisterScriptInclude(paths[j]);
        }
        Page.ClientScript.RegisterOnSubmitStatement(GetType(), "SaveSettings", "Bitrix.RatingEditor.getInstance().saveSettings();");

        if (EditorMode != BXAdminPageEditorMode.Modification)
        {
            AddButton.Visible = false;
            DeleteButton.Visible = false;
        }
    }

    protected void ValidateBoundEntityType(Object source, ServerValidateEventArgs aruments)
    {
        aruments.IsValid = !string.IsNullOrEmpty(aruments.Value);
    }

    protected void ValidateSelectedComponents(Object source, ServerValidateEventArgs aruments)
    {
        aruments.IsValid = ClientComponentConfigList.Count > 0;
    }

    protected override string BackUrl
    {
        get { return !string.IsNullOrEmpty(base.BackUrl) ? base.BackUrl : "RatingList.aspx"; }
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
                catch (Exception /*exc*/)
                {
                    this.entityId = 0;
                }
            }
            return this.entityId.Value;
        }
    }

    private BXRating entity = null;
    protected BXRating Entity
    {
        get { return this.entity; }
    }

    private void TrySave()
    {
        if (this.editorError != RatingEditorError.None)
            return;

        if (this.entity == null)
        {
            this.editorError = RatingEditorError.IsNotFound;
            this.errorMessageText = GetMessageRaw("Message.RatingIsNotFound");
            return;
        }

        try
        {
            this.entity.Active = Active.Checked;
            this.entity.Name = Name.Text;
            this.entity.BoundEntityTypeId = BoundEntityType.SelectedValue;
            this.entity.CurValCustomFieldName = CurValCustomFieldName.Text;
            this.entity.PrevValCustomFieldName = PrevValCustomFieldName.Text;
            this.entity.CalculationMethod = (BXRatingCalculationMethod)Enum.Parse(typeof(BXRatingCalculationMethod), CalculationMethod.SelectedValue);
            //this.entity.RefreshMethod = (BXRatingRefreshMethod)Enum.Parse(typeof(BXRatingRefreshMethod), RefreshMethod.SelectedValue);
            this.entity.RefreshMethod = BXRatingRefreshMethod.Instant;
            this.entity.SetComponentConfigList(ClientComponentConfigList);
            this.entity.XmlId = XmlId.Text;

            BXRatableEntityTypeInfo info = BXRatingManager.GetRatableEntityTypeInfo(this.entity.BoundEntityTypeId);
            if (info != null)
                this.entity.CustomPropertyEntityId = info.CustomPropertyEntityId;

            this.entity.Save();
            this.entityId = this.entity.Id;
        }
        catch (Exception exc)
        {
            this.errorMessageText = exc.Message;
            this.editorError = this.entity.IsNew ? RatingEditorError.Creation : RatingEditorError.Modification;
        }
    }

    private string errorMessageText = string.Empty;
    protected string ErrorMessageText
    {
        get { return this.errorMessageText; }
    }

    private RatingEditorError editorError = RatingEditorError.None;
    protected RatingEditorError EditorError
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

                GoBack("~/bitrix/admin/RatingList.aspx");
            }
            catch (Exception ex)
            {
                this.errorMessageText = ex.Message;
                this.editorError = RatingEditorError.Deleting;
            }
        }
    }

    protected void OnTabControlCommand(object sender, BXTabControlCommandEventArgs e)
    {
        switch (e.CommandName)
        {
            case "save":
                {
                    if (IsValid && EditorError == RatingEditorError.None)
                    {
                        TrySave();
                        if (EditorError == RatingEditorError.None)
                            GoBack("~/bitrix/admin/RatingList.aspx");
                    }
                }
                break;
            case "apply":
                {
                    if (IsValid && EditorError == RatingEditorError.None)
                    {
                        TrySave();
                        if (EditorError == RatingEditorError.None && Entity != null)
                            Response.Redirect(string.Format("RatingEdit.aspx?id={0}&tabindex={1}", Entity.Id.ToString(), TabControl.SelectedIndex));
                    }
                }
                break;
            case "cancel":
                GoBack();
                break;
        }
    }
}
