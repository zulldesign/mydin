using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Linq;
using Bitrix;
using Bitrix.Configuration;
using Bitrix.DataLayer;
using Bitrix.IO;
using Bitrix.Modules;
using Bitrix.Security;
using Bitrix.Services.Image;
using Bitrix.Services.Text;
using Bitrix.Services.User;
using Bitrix.UI;

public partial class bitrix_admin_AuthUsersEdit : BXAdminPage
{
	int userId = -1;
	protected BXUser user;
	bool currentUserCanModifySelfUser = false;
	bool currentUserCanModifyUser = false;
	bool currentUserCanDeleteUser = false;
	bool currentUserCanCreateUser = false;

	int[] rolesToView;
	int[] rolesToModify;
	int[] rolesToDelete;
	int[] rolesToCreate;
	int[] rolesToViewAndModify;
	int[] userRoles;

	bool missingProvider = false;
	List<BXUserProfileAdminFacade> profileEditors;


	private Control WalkThrowControlsSearch(Control cntrl, string name)
	{
		foreach (Control subCntrl in cntrl.Controls)
		{
			if (!String.IsNullOrEmpty(subCntrl.ID) && subCntrl.ID.Equals(name, StringComparison.InvariantCultureIgnoreCase))
				return subCntrl;

			Control cntrl1 = WalkThrowControlsSearch(subCntrl, name);
			if (cntrl1 != null)
				return cntrl1;
		}

		return null;
	}

	private void LoadData()
	{
		ddlSite.Items.Clear();

		BXSiteCollection siteColl = BXSite.GetList(null, null, null, null, BXTextEncoder.EmptyTextEncoder);
		foreach (BXSite site in siteColl)
			ddlSite.Items.Add(new ListItem("[" + site.Id + "] " + site.Name, site.Id));

		if (user == null)
		{
			ddProviderName.Visible = true;
			lbProviderName.Visible = false;
			rfvProviderName.Visible = true;
			rfvProviderName.Enabled = true;
			tbUserName.Visible = true;
			lbUserName.Visible = false;
			rfvUserName.Enabled = true;

			rfvNewPassword.Enabled = true;
			rfvNewPassword.Visible = true;
			rfvNewPasswordConf.Enabled = true;
			rfvNewPasswordConf.Visible = true;

			cvPassword.Enabled = false;

			trLastActivityDate.Visible = false;
			trLastLoginDate.Visible = false;
			trCreationDate.Visible = false;
			trLastLockoutDate.Visible = false;
			trIsLockedOut.Visible = false;
			trLastPasswordChangedDate.Visible = false;
			trPassword.Visible = false;

			cbIsApproved.Checked = true;

			ddProviderName.Attributes["onchange"] = "CheckQuestionAndAnswer(this)";
			foreach (ProviderBase pr in Membership.Providers)
				ddProviderName.Items.Add(new ListItem(("BXSqlMembershipProvider".Equals(pr.Name, StringComparison.InvariantCultureIgnoreCase) ? GetMessageRaw("String.InnerProvider") : pr.Name), pr.Name));
			ddProviderName.SelectedValue = BXUserManager.Provider.Name;

			trPasswordQuestion.Style["display"] = (BXUserManager.Provider.RequiresQuestionAndAnswer ? "" : "none");
			trPasswordAnswer.Style["display"] = (BXUserManager.Provider.RequiresQuestionAndAnswer ? "" : "none");
		}
		else
		{
			Page.Title = string.Format(GetMessage("PageTitle.ModificationOfUser"), userId);
			((BXAdminMasterPage)Page.Master).Title = Page.Title;

			ddProviderName.Visible = false;
			lbProviderName.Visible = true;
			rfvProviderName.Enabled = false;
			rfvProviderName.Visible = false;
			tbUserName.Visible = false;
			lbUserName.Visible = true;
			rfvUserName.Enabled = false;

			rfvNewPassword.Enabled = false;
			rfvNewPassword.Visible = false;
			rfvNewPasswordConf.Enabled = false;
			rfvNewPasswordConf.Visible = false;

			cvPassword.Enabled = true;

			trLastActivityDate.Visible = true;
			trLastLoginDate.Visible = true;
			trCreationDate.Visible = true;
			trIsLockedOut.Visible = true;
			trLastPasswordChangedDate.Visible = true;
			trPassword.Visible = !user.IsBuiltInProvider;

			tbEmail.Text = user.Email;
			lbProviderName.Text = HttpUtility.HtmlEncode(("BXSqlMembershipProvider".Equals(user.ProviderName, StringComparison.InvariantCultureIgnoreCase) ? GetMessageRaw("String.InnerProvider") : user.ProviderName));
			lbUserName.Text = HttpUtility.HtmlEncode(user.UserName);
			tbPasswordQuestion.Text = user.PasswordQuestion;
			tbAnswer.Text = "";

			lbCreationDate.Text = user.CreationDate.ToString();
			lbLastLoginDate.Text = user.LastLoginDate.ToString();
			lbLastActivityDate.Text = user.LastActivityDate.ToString();
			lbLastLockoutDate.Text = user.LastLockoutDate.ToString();
			lbLastPasswordChangedDate.Text = user.LastPasswordChangedDate.ToString();

			cbIsApproved.Checked = user.IsApproved;
			cbIsLockedOut.Checked = user.IsLockedOut;
			cbIsLockedOut.Enabled = user.IsLockedOut;
			hfIsLockedOut.Value = (user.IsLockedOut ? "Y" : "N");

			tbComment.Text = user.Comment;

			tbDisplayName.Text = user.DisplayName;
			tbFirstName.Text = user.FirstName;
			tbSecondName.Text = user.SecondName;
			tbLastName.Text = user.LastName;
			tbBirthdayDate.Text = user.BirthdayDate != DateTime.MinValue ? user.BirthdayDate.ToString("d") : String.Empty;
			ddlSite.SelectedValue = user.SiteId;
			aifImage.ImageFile = user.Image;
			ddlGender.SelectedValue = user.Gender.ToString();

			BXRoleWorkCollection roleWorksTmp = BXRoleManager.GetAllRolesForUser(user.UserId);
			foreach (BXRoleWork roleWorkTmp in roleWorksTmp)
			{
				Control cntrl = WalkThrowControlsSearch(Form, String.Format("tbCheck_{0}", roleWorkTmp.RoleId.ToString()));
				if (cntrl == null)
					continue;
				(cntrl as CheckBox).Checked = true;

				cntrl = WalkThrowControlsSearch(Form, String.Format("tbCheck_{0}_old", roleWorkTmp.RoleId.ToString()));
				(cntrl as HiddenField).Value = "Y";

				cntrl = WalkThrowControlsSearch(Form, String.Format("tbActiveFrom_{0}", roleWorkTmp.RoleId.ToString()));
				(cntrl as TextBox).Text = roleWorkTmp.WorkActiveFrom;

				cntrl = WalkThrowControlsSearch(Form, String.Format("tbActiveFrom_{0}_old", roleWorkTmp.RoleId.ToString()));
				(cntrl as HiddenField).Value = roleWorkTmp.WorkActiveFrom;

				cntrl = WalkThrowControlsSearch(Form, String.Format("tbActiveTo_{0}", roleWorkTmp.RoleId.ToString()));
				(cntrl as TextBox).Text = roleWorkTmp.WorkActiveTo;

				cntrl = WalkThrowControlsSearch(Form, String.Format("tbActiveTo_{0}_old", roleWorkTmp.RoleId.ToString()));
				(cntrl as HiddenField).Value = roleWorkTmp.WorkActiveTo;
			}

			trLastLockoutDate.Visible = cbIsLockedOut.Checked;

			if (!missingProvider)
			{
				trPasswordQuestion.Style["display"] = (Membership.Providers[user.ProviderName].RequiresQuestionAndAnswer ? "" : "none");
				trPasswordAnswer.Style["display"] = (Membership.Providers[user.ProviderName].RequiresQuestionAndAnswer ? "" : "none");
			}
			else
			{
				trPasswordQuestion.Style["display"] = "none";
				trPasswordAnswer.Style["display"] = "none";
				trPassword.Style["display"] = "none";
				trNewPassword.Style["display"] = "none";
				trNewPasswordConf.Style["display"] = "none";
			}
		}

		foreach (BXUserProfileAdminFacade p in profileEditors)
			p.Load(user);

		lbPasswordHint.Text =
			"<br/>"
			+ (
				BXUserManager.Provider.MinRequiredNonAlphanumericCharacters > 0
				? string.Format(GetMessage("FormattedPasswordHintText"), BXUserManager.Provider.MinRequiredPasswordLength, BXUserManager.Provider.MinRequiredNonAlphanumericCharacters)
				: string.Format(GetMessage("FormattedPasswordHintTextSmall"), BXUserManager.Provider.MinRequiredPasswordLength)
			);
	}

	protected void Page_Init(object sender, EventArgs e)
	{
		aifImage.NewImageUploadingHint = string.Format(GetMessage("Hint.ImageMaxSize"), BXConfigurationUtility.Options.User.AvatarMaxWidth, BXConfigurationUtility.Options.User.AvatarMaxHeight);

		userId = base.GetRequestInt("id");
		if (userId > 0)
			hfUserId.Value = userId.ToString();
		Int32.TryParse(hfUserId.Value, out userId);
		if (userId > 0)
		{
			BXUserCollection userCol = Bitrix.Security.BXUser.GetList(
				new BXFilter(new BXFilterItem(Bitrix.Security.BXUser.Fields.UserId, BXSqlFilterOperators.Equal, userId)),
				null,
				new BXSelectAdd(Bitrix.Security.BXUser.Fields.CustomFields.DefaultFields),
				null,
				BXTextEncoder.EmptyTextEncoder
			);

			if ((user = userCol.Count > 0 ? userCol[0] : null) == null)
			{
				userId = 0;
				hfUserId.Value = userId.ToString();
			}
		}

		currentUserCanModifySelfUser = userId > 0 && ((this.BXUser.Identity as BXIdentity).Id == userId) && this.BXUser.IsCanOperate(BXRoleOperation.Operations.UserModifySelf);

		if (userId > 0)
		{
			if (Membership.Providers[user.ProviderName] == null)
				missingProvider = true;

			userRoles = (from r in user.GetRoles() orderby r.RoleId select r.RoleId).Distinct().ToArray();
			rolesToView = GetRoleIds(BXRoleOperation.Operations.UserView);
			if (!currentUserCanModifySelfUser && !CheckRoles(rolesToView))
				BXAuthentication.AuthenticationRequired();
		}
		else
		{
			userRoles = new int[0];
			rolesToCreate = GetRoleIds(BXRoleOperation.Operations.UserCreate);
			if (!CheckRoles(rolesToCreate))
				BXAuthentication.AuthenticationRequired();
		}

		rolesToCreate = rolesToCreate ?? GetRoleIds(BXRoleOperation.Operations.UserCreate);
		rolesToModify = rolesToModify ?? GetRoleIds(BXRoleOperation.Operations.UserModify);
		rolesToView = rolesToView ??  GetRoleIds(BXRoleOperation.Operations.UserView);
		rolesToDelete = rolesToDelete ?? GetRoleIds(BXRoleOperation.Operations.UserDelete);

		rolesToViewAndModify = 
			(rolesToView.Length == 1 && rolesToView[0] == 0 || rolesToModify.Length == 1 && rolesToModify[0] == 0)
			? new[] { 0 }
			: rolesToView.Union(rolesToModify).OrderBy(x => x).Distinct().ToArray(); 

		currentUserCanModifyUser = CheckRoles(rolesToModify);
		currentUserCanCreateUser = CheckRoles(rolesToCreate);
		currentUserCanDeleteUser = CheckRoles(rolesToDelete);

		IBXCustomFieldList fl = CustomFieldList1 as IBXCustomFieldList;
		if (user != null)
			fl.Load(user.CustomValues);

		#region Load Profile Extensions
		profileEditors = new List<BXUserProfileAdminFacade>();
		int index = BXTabControl1.Tabs.IndexOf(NotesTab);
		foreach (BXUserProfileExtensionProvider provider in Bitrix.Security.BXUser.GetProfileExtensionProviders())
		{
			BXUserProfileAdminFacade facade = provider.CreateAdminFacade();
			if (facade == null)
				continue;

			profileEditors.Add(facade);

			ExtensionTab.Visible = true;
			if (facade.EditorControl != null)
				ExtensionTab.Controls.Add(facade.EditorControl);

			facade.SetValidationGroup(BXTabControl1.ValidationGroup);
		}
		#endregion

		PrepareForInsertScript();
	}

	private int[] GetRoleIds(string operation)
	{
		return
			BXUser.GetOperatableExternalIds(new[] { operation })
			.Where(x => x == "0" || x.StartsWith("r"))
			.Select(x => int.Parse(x.StartsWith("r") ? x.Substring(1) : x)) 
			.OrderBy(x => x)
			.Distinct()
			.ToArray();
	}

	private bool CheckRoles(int[] roles)
	{
		if (roles.Length == 0)
			return false;
		if (roles.Length == 1 && roles[0] == 0 || userRoles.Length == 0)
			return true;
		if (roles.Length < userRoles.Length)
			return false;

		int i = 0;
		for (int j = 0; j < roles.Length; j++)
		{
			if (userRoles[i] == roles[j])
			{
				i++;
				if (i == userRoles.Length)
					return true;
			}
		}
		return false;
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		((BXAdminMasterPage)Page.Master).Title = Page.Title;

		if (userId > 0)
			BXRoleManager.SynchronizeProviderUserRoles(user.UserName, user.ProviderName);

		var filter = new BXFormFilter(
			new BXFormFilterItem("Active", true, BXSqlFilterOperators.Equal)
		);
		var roles = userId > 0 ? rolesToViewAndModify : rolesToCreate;
		if (roles.Length != 1 || roles[0] != 0)
			filter.Add(new BXFormFilterItem("Id", roles, BXSqlFilterOperators.In));

		BXRoleCollection rolesTmp = BXRoleManager.GetList(
			filter,
			new BXOrderBy_old("RoleName", "Asc")
		);

		foreach (int i in new[] { 1, 3, 2 })
		{
			var r = rolesTmp.Find(x => x.RoleId == i);
			if (r != null)
			{
				rolesTmp.Remove(r);
				rolesTmp.Insert(0, r);
			}
		}


		foreach (BXRole roleTmp in rolesTmp)
		{
			HtmlTableRow r1 = new HtmlTableRow();
			r1.VAlign = "top";

			HtmlTableCell c1 = new HtmlTableCell();
			CheckBox cb = new CheckBox();
			cb.ID = String.Format("tbCheck_{0}", roleTmp.RoleId.ToString());
			cb.Checked = false;
			c1.Controls.Add(cb);
			HiddenField hf = new HiddenField();
			hf.ID = String.Format("tbCheck_{0}_old", roleTmp.RoleId.ToString());
			hf.Value = "N";
			c1.Controls.Add(hf);
			r1.Cells.Add(c1);

			c1 = new HtmlTableCell();
			c1.InnerHtml = String.Format("<a href=\"AuthRolesEdit.aspx?name={0}\">{1}</a>", Server.UrlEncode(roleTmp.RoleName), Server.HtmlEncode(roleTmp.Title));
			r1.Cells.Add(c1);

			c1 = new HtmlTableCell();
			c1.Align = "center";
			c1.Style["padding-right"] = "10px";
			c1.Style["padding-left"] = "10px";
			TextBox tb1 = new TextBox();
			tb1.ID = String.Format("tbActiveFrom_{0}", roleTmp.RoleId.ToString());
			c1.InnerHtml = "c&nbsp;";
			c1.Controls.Add(tb1);
			hf = new HiddenField();
			hf.ID = String.Format("tbActiveFrom_{0}_old", roleTmp.RoleId.ToString());
			hf.Value = "";
			c1.Controls.Add(hf);
			r1.Cells.Add(c1);

			c1 = new HtmlTableCell();
			c1.Align = "center";
			tb1 = new TextBox();
			tb1.ID = String.Format("tbActiveTo_{0}", roleTmp.RoleId.ToString());
			c1.InnerHtml = string.Format("{0}&nbsp;", GetMessage("TabCellInnerHtml.To"));
			c1.Controls.Add(tb1);
			hf = new HiddenField();
			hf.ID = String.Format("tbActiveTo_{0}_old", roleTmp.RoleId.ToString());
			hf.Value = "";
			c1.Controls.Add(hf);
			r1.Cells.Add(c1);

			tblRoles.Rows.Add(r1);
		}

		if (!Page.IsPostBack)
			LoadData();

		if (userId > 0)
		{
			if (!missingProvider)
			{
				trPasswordQuestion.Style["display"] = (Membership.Providers[user.ProviderName].RequiresQuestionAndAnswer ? "" : "none");
				trPasswordAnswer.Style["display"] = (Membership.Providers[user.ProviderName].RequiresQuestionAndAnswer ? "" : "none");
			}
			else
			{
				trPasswordQuestion.Style["display"] = "none";
				trPasswordAnswer.Style["display"] = "none";
				trPassword.Style["display"] = "none";
				trNewPassword.Style["display"] = "none";
				trNewPasswordConf.Style["display"] = "none";
			}
		}
		else
		{
			if (!String.IsNullOrEmpty(ddProviderName.SelectedValue))
			{
				trPasswordQuestion.Style["display"] = (Membership.Providers[(ddProviderName.SelectedValue.Equals(GetMessageRaw("String.InnerProvider"), StringComparison.InvariantCultureIgnoreCase) ? "BXSqlMembershipProvider" : ddProviderName.SelectedValue)].RequiresQuestionAndAnswer ? "" : "none");
				trPasswordAnswer.Style["display"] = (Membership.Providers[(ddProviderName.SelectedValue.Equals(GetMessageRaw("String.InnerProvider"), StringComparison.InvariantCultureIgnoreCase) ? "BXSqlMembershipProvider" : ddProviderName.SelectedValue)].RequiresQuestionAndAnswer ? "" : "none");
			}
			else
			{
				trPasswordQuestion.Style["display"] = (Membership.Provider.RequiresQuestionAndAnswer ? "" : "none");
				trPasswordAnswer.Style["display"] = (Membership.Provider.RequiresQuestionAndAnswer ? "" : "none");
			}
		}
		DeleteUserSeparator.Visible = DeleteUserButton.Visible = userId > 0 && currentUserCanDeleteUser;
		AddUserSeparator.Visible = AddUserButton.Visible = userId > 0 && currentUserCanCreateUser;
		BXTabControl1.ShowSaveButton = BXTabControl1.ShowApplyButton = (userId <= 0 && currentUserCanCreateUser) || (userId > 0 && (currentUserCanModifyUser || currentUserCanModifySelfUser));
	}

	void PrepareForInsertScript()
	{
		if (!ClientScript.IsClientScriptBlockRegistered(GetType(), "InsertScript"))
		{
			StringBuilder sscript = new StringBuilder();
			sscript.AppendLine("var arMembershipProviders = {");
			bool fl = false;
			foreach (ProviderBase pr in Membership.Providers)
			{
				sscript.AppendLine(
					string.Format(
						"{0}\"{1}\":\"{2}\"",
						(fl ? ", " : ""),
						pr.Name,
						((pr as MembershipProvider).RequiresQuestionAndAnswer ? "Y" : "N")
					)
				);
				fl = true;
			}
			sscript.AppendLine("};");

			sscript.AppendLine("function CheckQuestionAndAnswer(sbox)");
			sscript.AppendLine("{");
			sscript.AppendLine("	if (sbox.selectedIndex < 0) return;");
			sscript.AppendLine(string.Format("	document.getElementById(\"{0}\").style[\"display\"] = ((arMembershipProviders[sbox[sbox.selectedIndex].value] == 'Y') ? \"\" : \"none\");", trPasswordQuestion.ClientID));
			sscript.AppendLine(string.Format("	document.getElementById(\"{0}\").style[\"display\"] = ((arMembershipProviders[sbox[sbox.selectedIndex].value] == 'Y') ? \"\" : \"none\");", trPasswordAnswer.ClientID));
			sscript.AppendLine("}");

			sscript.AppendLine("function CheckProviderName(oSrc, args)");
			sscript.AppendLine("{");
			sscript.AppendLine("	args.IsValid = (arMembershipProviders[args.Value] != null);");
			sscript.AppendLine("}");

			sscript.AppendLine("function CheckNewPasswordConf(oSrc, args)");
			sscript.AppendLine("{");
			sscript.AppendLine(
				string.Format(
					"	args.IsValid = ((document.getElementById('{0}').value.length > 0) && (document.getElementById('{1}').value.length > 0) || (document.getElementById('{0}').value.length <= 0) && (document.getElementById('{1}').value.length <= 0));",					
					tbNewPassword.ClientID,
					tbNewPasswordConf.ClientID
				)
			);
			sscript.AppendLine("}");

			sscript.AppendLine("function CheckPassword(oSrc, args)");
			sscript.AppendLine("{");
			sscript.AppendLine(
				string.Format(
					"	args.IsValid = ((document.getElementById('{0}').value.length <= 0) && (document.getElementById('{1}').value.length <= 0) || (document.getElementById('{2}').value.length > 0));",					
					tbNewPassword.ClientID,
					tbAnswer.ClientID,
					tbPassword.ClientID
				)
			);
			sscript.AppendLine("}");

			ClientScript.RegisterClientScriptBlock(GetType(), "InsertScript", sscript.ToString(), true);
		}

		/*
		if (!ClientScript.IsStartupScriptRegistered(GetType(), "StartupScript"))
		{
			StringBuilder sscript1 = new StringBuilder();
			sscript1.AppendLine(String.Format("CheckQuestionAndAnswer(document.{0}.{1});", Form.ClientID, ddProviderName.ClientID));
			ClientScript.RegisterStartupScript(GetType(), "StartupScript", sscript1.ToString(), true);
		}
		*/

		ddProviderName.Attributes["onchange"] = "CheckQuestionAndAnswer(this)";
	}

	private string GetErrorMessage(MembershipCreateStatus status)
	{
		switch (status)
		{
			case MembershipCreateStatus.DuplicateUserName:
				return GetMessageRaw("Message.DuplicateUserName");

			case MembershipCreateStatus.DuplicateEmail:
				return GetMessageRaw("Message.DuplicateEmail");

			case MembershipCreateStatus.InvalidPassword:
				return string.Format(
					GetMessageRaw("Message.InsecurePassword"),
					string.Format(
						GetMessageRaw("FormattedPasswordHintText"),
						BXUserManager.Provider.MinRequiredPasswordLength,
						BXUserManager.Provider.MinRequiredNonAlphanumericCharacters
					)
				);
			case MembershipCreateStatus.InvalidEmail:
				return GetMessageRaw("Message.InvalidEmail");

			case MembershipCreateStatus.InvalidAnswer:
				return GetMessageRaw("Message.InvalidAnswer");

			case MembershipCreateStatus.InvalidQuestion:
				return GetMessageRaw("Message.InvalidQuestion");

			case MembershipCreateStatus.InvalidUserName:
				return GetMessageRaw("Message.InvalidUserName");

			case MembershipCreateStatus.ProviderError:
				return GetMessageRaw("Message.ProviderError");

			case MembershipCreateStatus.UserRejected:
				return GetMessageRaw("Message.UserRejected");

			default:
				return GetMessageRaw("Message.Default");
		}
	}

	private bool SaveUser()
	{
		if (!Page.IsValid)
			return false;

		if (!(userId > 0 ? UpdateUser() : CreateUser()))
			return false;
		try
		{
			foreach (BXUserProfileAdminFacade p in profileEditors)
				p.Save(user, null);
		}
		catch (BXEventException e)
		{
			foreach (string s in e.Messages)
				userValidationSummary.AddErrorMessage(s);
			return false;
		}
		catch (Exception e)
		{
			userValidationSummary.AddErrorMessage(Encode(e.Message));
			return false;
		}
		return true;
	}

	private bool CreateUser()
	{
		try
		{
			if (!currentUserCanCreateUser)
				throw new Exception(GetMessageRaw("ExceptionText.YouDontHaveRightsToCreateNewUser"));

			DateTime birthdayDate;
			DateTime.TryParse(tbBirthdayDate.Text, out birthdayDate);

			string providerNameTmp = ddProviderName.SelectedValue;

			user = new BXUser(BXTextEncoder.EmptyTextEncoder);
			user.UserName = tbUserName.Text;
			user.ProviderName = providerNameTmp;
			user.Password = tbNewPassword.Text;
			user.Email = tbEmail.Text;
			if (Membership.Providers[providerNameTmp].RequiresQuestionAndAnswer)
			{
				user.PasswordQuestion = tbPasswordQuestion.Text;
				user.PasswordAnswer = tbAnswer.Text;
			}
			user.IsApproved = cbIsApproved.Checked;
			user.DisplayName = tbDisplayName.Text;
			user.FirstName = tbFirstName.Text;
			user.SecondName = tbSecondName.Text;
			user.LastName = tbLastName.Text;
			user.SiteId = ddlSite.SelectedValue;
			user.BirthdayDate = birthdayDate;
			user.Comment = tbComment.Text;

			user.Gender = Enum.IsDefined(typeof(BXUserGender), ddlGender.SelectedValue) ? (BXUserGender)Enum.Parse(typeof(BXUserGender), ddlGender.SelectedValue) : BXUserGender.Unknown;
			user.CustomValues.Override(CustomFieldList1.Save());

			BXFile f = SaveFile();
			if (f != null)
				user.ImageId = f.Id;

			try
			{
				try
				{
					user.Create();
				}
				catch (MembershipCreateUserException ex)
				{
					throw new Exception(GetErrorMessage(ex.StatusCode), ex);
				}

			}
			catch
			{
				user = null;
				if (f != null)
					f.Delete();
				throw;
			}



			userId = user.UserId;
			hfUserId.Value = userId.ToString();


			if (rolesToCreate.Length > 0)
			{
				var filter = new BXFormFilter(
					new BXFormFilterItem("Active", true, BXSqlFilterOperators.Equal)
				);
				if (rolesToCreate.Length > 1 || rolesToCreate[0] != 0)
					filter.Add(new BXFormFilterItem("Id", rolesToCreate, BXSqlFilterOperators.In));

				BXRoleCollection rolesTmp = BXRoleManager.GetList(
						filter,
						new BXOrderBy_old("RoleName", "Asc")
				);
				
				foreach (BXRole roleTmp in rolesTmp)
				{
					Control cntrl = WalkThrowControlsSearch(Form, String.Format("tbCheck_{0}", roleTmp.RoleId.ToString()));
					bool cb = (cntrl as CheckBox).Checked;

					cntrl = WalkThrowControlsSearch(Form, String.Format("tbCheck_{0}_old", roleTmp.RoleId.ToString()));
					bool cbOld = ((cntrl as HiddenField).Value == "Y");

					cntrl = WalkThrowControlsSearch(Form, String.Format("tbActiveFrom_{0}", roleTmp.RoleId.ToString()));
					string from = (cntrl as TextBox).Text;

					cntrl = WalkThrowControlsSearch(Form, String.Format("tbActiveFrom_{0}_old", roleTmp.RoleId.ToString()));
					string fromOld = (cntrl as HiddenField).Value;

					cntrl = WalkThrowControlsSearch(Form, String.Format("tbActiveTo_{0}", roleTmp.RoleId.ToString()));
					string to = (cntrl as TextBox).Text;

					cntrl = WalkThrowControlsSearch(Form, String.Format("tbActiveTo_{0}_old", roleTmp.RoleId.ToString()));
					string toOld = (cntrl as HiddenField).Value;

					if (cb && (!cbOld || !from.Equals(fromOld, StringComparison.InvariantCultureIgnoreCase) || !to.Equals(toOld, StringComparison.InvariantCultureIgnoreCase)))
					{
						if (cbOld)
							user.RemoveFromRole(roleTmp.RoleName);
						user.AddToRole(roleTmp.RoleName, from, to);
					}
					else
					{
						if (!cb && cbOld)
							user.RemoveFromRole(roleTmp.RoleName);
					}
				}
			}
			//BXCustomEntityManager.SaveEntity(Bitrix.Security.BXUser.GetCustomFieldsKey(), user.UserId, CustomFieldList1.Save());

			return true;
		}
		catch (BXEventException e)
		{
			foreach (string s in e.Messages)
				userValidationSummary.AddErrorMessage(s/*, "vgInnerForm", "tbUserName"*/);
		}
		catch (Exception e)
		{
			userValidationSummary.AddErrorMessage(e.Message/*, "vgInnerForm", "tbUserName"*/);
		}

		return false;
	}

	private bool UpdateUser()
	{
		try
		{
			if (!currentUserCanModifyUser && !currentUserCanModifySelfUser)
				throw new Exception(GetMessageRaw("ExceptionText.YouDontHaveRightsToModifyUser"));

			if (!this.ExistUserEmail(tbEmail.Text))
				throw new Exception(GetMessageRaw("Message.DuplicateEmail"));

			user.Email = tbEmail.Text;
			user.Comment = tbComment.Text;	
			user.IsApproved = cbIsApproved.Checked;
			user.DisplayName = tbDisplayName.Text;
			user.FirstName = tbFirstName.Text;
			user.SecondName = tbSecondName.Text;
			user.LastName = tbLastName.Text;
			user.SiteId = ddlSite.SelectedValue;

			DateTime birthdayDate;
			DateTime.TryParse(tbBirthdayDate.Text, out birthdayDate);
			user.BirthdayDate = birthdayDate;

			user.Gender = Enum.IsDefined(typeof(BXUserGender), ddlGender.SelectedValue) ? (BXUserGender)Enum.Parse(typeof(BXUserGender), ddlGender.SelectedValue) : BXUserGender.Unknown;
			user.CustomValues.Override(CustomFieldList1.Save());

			if (aifImage.DeleteFile)
				user.ImageId = 0;
			BXFile f = SaveFile();
			if (f != null)
				user.ImageId = f.Id;

			try
			{
				user.Update();
			}
			catch
			{
				if (f != null)
					f.Delete();
				throw;
			}

			if (!String.IsNullOrEmpty(tbNewPassword.Text))
			{
				try
				{
					if (BXUserManager.Provider.MinRequiredPasswordLength > 0 && tbNewPassword.Text.Length < BXUserManager.Provider.MinRequiredPasswordLength)
						throw new PublicException(string.Format(GetMessageRaw("ExceptionText.PasswordLength"), BXUserManager.Provider.MinRequiredPasswordLength.ToString()));
					if (BXUserManager.Provider.MinRequiredNonAlphanumericCharacters > 0 && !CheckAlphanumerics(tbNewPassword.Text, BXUserManager.Provider.MinRequiredNonAlphanumericCharacters))
						throw new PublicException(string.Format(GetMessageRaw("ExceptionText.NonAlphanumerics"), BXUserManager.Provider.MinRequiredNonAlphanumericCharacters));
					
					if (!(user.IsBuiltInProvider ? BXUserManager.Provider.ChangePassword(user.UserName, tbNewPassword.Text) : user.ChangePassword(tbPassword.Text, tbNewPassword.Text)))
						throw new PublicException(GetMessageRaw("ExceptionText.ChangeOfPasswordFailed"));
				}
				catch (PublicException)
				{
					throw;
				}
				catch (Exception ex)
				{
					throw new ArgumentException(GetMessageRaw("ExceptionText.ChangeOfPasswordFailed"), ex);
				}
			}

			MembershipProvider provider = BXUserManager.GetProvider(user.ProviderName);
			if (provider != null && provider.RequiresQuestionAndAnswer && !String.IsNullOrEmpty(tbAnswer.Text))
			{
				bool res = user.ChangePasswordQuestionAndAnswer(
					(!String.IsNullOrEmpty(tbNewPassword.Text) ? tbNewPassword.Text : tbPassword.Text),
					tbPasswordQuestion.Text,
					tbAnswer.Text
				);
				if (!res)
					throw new ArgumentException(GetMessageRaw("ExceptionText.ChangeOfSecretQuestionAndAnswerFailed"));
			}
				

			if (hfIsLockedOut.Value == "Y" && !cbIsLockedOut.Checked)
				if (!user.UnlockUser())
					throw new ArgumentException(GetMessageRaw("ExceptionText.DeblockingOfUserFailed"));

			if (currentUserCanModifyUser && rolesToModify.Length > 0)
			{
				var filter = new BXFormFilter(
					new BXFormFilterItem("Active", true, BXSqlFilterOperators.Equal)
				);
				if (rolesToModify.Length > 1 || rolesToModify[0] != 0)
					filter.Add(new BXFormFilterItem("Id", rolesToModify, BXSqlFilterOperators.In));

				BXRoleCollection rolesTmp = BXRoleManager.GetList(
						filter,
						new BXOrderBy_old("RoleName", "Asc")
				);

				foreach (BXRole roleTmp in rolesTmp)
				{
					Control cntrl = WalkThrowControlsSearch(Form, String.Format("tbCheck_{0}", roleTmp.RoleId));
					bool cb = (cntrl as CheckBox).Checked;

					cntrl = WalkThrowControlsSearch(Form, String.Format("tbCheck_{0}_old", roleTmp.RoleId));
					bool cbOld = ((cntrl as HiddenField).Value == "Y");

					cntrl = WalkThrowControlsSearch(Form, String.Format("tbActiveFrom_{0}", roleTmp.RoleId));
					string from = (cntrl as TextBox).Text;

					cntrl = WalkThrowControlsSearch(Form, String.Format("tbActiveFrom_{0}_old", roleTmp.RoleId));
					string fromOld = (cntrl as HiddenField).Value;

					cntrl = WalkThrowControlsSearch(Form, String.Format("tbActiveTo_{0}", roleTmp.RoleId));
					string to = (cntrl as TextBox).Text;

					cntrl = WalkThrowControlsSearch(Form, String.Format("tbActiveTo_{0}_old", roleTmp.RoleId));
					string toOld = (cntrl as HiddenField).Value;

					if (cb && (!cbOld || !from.Equals(fromOld, StringComparison.InvariantCultureIgnoreCase) || !to.Equals(toOld, StringComparison.InvariantCultureIgnoreCase)))
					{
						if (cbOld)
							user.RemoveFromRole(roleTmp.RoleName);
						user.AddToRole(roleTmp.RoleName, from, to);
					}
					else
					{
						if (!cb && cbOld)
							user.RemoveFromRole(roleTmp.RoleName);
					}
				}
			}

			//BXCustomEntityManager.SaveEntity("user", user.UserId, CustomFieldList1.Save());

			return true;
		}
		catch (BXEventException e)
		{
			foreach (string s in e.Messages)
				userValidationSummary.AddErrorMessage(s/*, "vgInnerForm", "tbUserName"*/);
		}
		catch (Exception e)
		{
			userValidationSummary.AddErrorMessage(e.Message/*, "vgInnerForm", "tbUserName"*/);
		}
		return false;
	}

	private bool ExistUserEmail(string email)
	{
		try{
			var userList = Bitrix.Security.BXUser.GetList(
				new BXFilter(
					new BXFilterItem(Bitrix.Security.BXUser.Fields.Email, BXSqlFilterOperators.Equal, email)
				), 
				null
			);
			if (userList.Count == 1)
			{
				if(userList[0].UserId != user.UserId)
					return false;
			}
			else if(userList.Count > 1)
				return false;
			return true;
		}
		catch(Exception)
		{}

		return false;
	}

	private bool CheckAlphanumerics(string value, int required)
	{
		int found = 0;
		for (int i = value.Length - 1; i >= 0; i--)
		{
			if (!char.IsLetterOrDigit(value, i))
			{
				found++;
				if (found >= required)
					return true;
			}
		}
		return false;
	}

	protected BXFile SaveFile()
	{
		FileUpload fu = aifImage.Upload;

		if (!fu.HasFile)
			return null;

		HttpPostedFile postedFile = fu.PostedFile;
		BXFileValidationResult result = BXFile.ValidateAsImage(
			postedFile.FileName,
			postedFile.ContentType,
			postedFile.InputStream,
			BXConfigurationUtility.Options.User.AvatarMaxSizeKB * 1024,
			BXConfigurationUtility.Options.User.AvatarMaxWidth,
			BXConfigurationUtility.Options.User.AvatarMaxHeight
			);

		if (result == BXFileValidationResult.Valid)
		{
			BXFile f = new BXFile(fu.PostedFile, "user", "main", null);
            f.DemandFileUpload();
			f.Save();
			return f;
		}

		if ((result & BXFileValidationResult.InvalidContentType) == 0
			&& (result & BXFileValidationResult.InvalidExtension) == 0
			&& (result & BXFileValidationResult.InvalidImage) == 0
			&& ((result & BXFileValidationResult.InvalidWidth) != 0 || (result & BXFileValidationResult.InvalidHeight) != 0)
			)
		{
			using (MemoryStream destinationStream = new MemoryStream())
			{
				string postedFileName = postedFile.FileName,
					postedFileMimeType = postedFile.ContentType;

				using (System.Drawing.Image image = System.Drawing.Image.FromStream(postedFile.InputStream))
				using (System.Drawing.Image scaledImage = BXImageUtility.Resize(image, BXConfigurationUtility.Options.User.AvatarMaxWidth, BXConfigurationUtility.Options.User.AvatarMaxHeight))
				{
					ImageFormat format = image.RawFormat;
					BXImageFormatInfo fInfo = new BXImageFormatInfo(format);
					/*
					 * GIF в PNG всегда - из-за возможной потери качества при преобразовании в 256 цветов
					 * JPG в PNG если площадь изображения не превышает 128x128
					 */
					if (format.Guid == ImageFormat.Gif.Guid || (format.Guid == ImageFormat.Jpeg.Guid && scaledImage.Width * scaledImage.Height <= 16384))
						fInfo = new BXImageFormatInfo(ImageFormat.Png);
					postedFileName = fInfo.AppendFileExtension(postedFileName);
					postedFileMimeType = fInfo.GetMimeType();
					scaledImage.Save(destinationStream, fInfo.Format);
				}

				BXFile f = new BXFile(null, destinationStream, postedFileName, "user", "main", null, postedFileMimeType);
				if ((result & BXFileValidationResult.InvalidSize) != 0 && f.FileSize <= BXConfigurationUtility.Options.User.AvatarMaxSizeKB * 1024)
					result &= ~BXFileValidationResult.InvalidSize;
				if ((result & BXFileValidationResult.InvalidWidth) != 0 && f.Width <= BXConfigurationUtility.Options.User.AvatarMaxWidth)
					result &= ~BXFileValidationResult.InvalidWidth;
				if ((result & BXFileValidationResult.InvalidHeight) != 0 && f.Height <= BXConfigurationUtility.Options.User.AvatarMaxHeight)
					result &= ~BXFileValidationResult.InvalidHeight;

				if (result == BXFileValidationResult.Valid)
				{
                    f.DemandFileUpload();
					f.Save();
					return f;
				}
			}
		}

		StringBuilder fError = new StringBuilder();
		if ((result & BXFileValidationResult.InvalidContentType) == BXFileValidationResult.InvalidContentType)
			fError.Append(GetMessageRaw("InvalidType"));

		if ((result & BXFileValidationResult.InvalidExtension) == BXFileValidationResult.InvalidExtension)
		{
			if (fError.Length != 0)
				fError.Append(", ");
			fError.Append(GetMessageRaw("InvalidExtension"));
		}

		if ((result & BXFileValidationResult.InvalidImage) == BXFileValidationResult.InvalidImage)
		{
			if (fError.Length != 0)
				fError.Append(", ");
			fError.Append(GetMessageRaw("InvalidImage"));
		}

		if ((result & BXFileValidationResult.InvalidSize) != 0)
		{
			if (fError.Length != 0)
				fError.Append(", ");
			fError.AppendFormat(GetMessageRaw("InvalidSize"), BXStringUtility.BytesToString(BXConfigurationUtility.Options.User.AvatarMaxSizeKB * 1024));
		}

		if ((result & BXFileValidationResult.InvalidWidth) != 0)
		{
			if (fError.Length != 0)
				fError.Append(", ");
			fError.AppendFormat(GetMessageRaw("InvalidWidth"), BXConfigurationUtility.Options.User.AvatarMaxWidth);
		}

		if ((result & BXFileValidationResult.InvalidHeight) != 0)
		{
			if (fError.Length != 0)
				fError.Append(", ");
			fError.AppendFormat(GetMessageRaw("InvalidHeight"), BXConfigurationUtility.Options.User.AvatarMaxHeight);
		}

		throw new Exception(string.Format(GetMessageRaw("Error.InvalidImage"), fError.ToString()));
	}

	protected void BXTabControl1_Command(object sender, Bitrix.UI.BXTabControlCommandEventArgs e)
	{
		bool successAction = true;
		if (e.CommandName == "save")
		{
			if (!SaveUser())
				successAction = false;
		}
		else if (e.CommandName == "apply")
		{
			if (!SaveUser())
				successAction = false;
			else
				Reload(BXTabControl1.SelectedIndex, "id=" + user.UserId);				
		}

		if (successAction)
			GoBack("AuthUsersList.aspx");
	}

	protected void cvProviderName_ServerValidate(object source, ServerValidateEventArgs args)
	{
		args.IsValid = (Membership.Providers[ddProviderName.SelectedValue] != null);
	}

	protected void cfNewPasswordConf_ServerValidate(object source, ServerValidateEventArgs args)
	{
		args.IsValid = (!String.IsNullOrEmpty(tbNewPassword.Text) && !String.IsNullOrEmpty(tbNewPasswordConf.Text)
			|| String.IsNullOrEmpty(tbNewPassword.Text) && String.IsNullOrEmpty(tbNewPasswordConf.Text));
	}

	protected void cvPassword_ServerValidate(object source, ServerValidateEventArgs args)
	{
		args.IsValid = (String.IsNullOrEmpty(tbNewPassword.Text)
			&& String.IsNullOrEmpty(tbAnswer.Text)
			|| !String.IsNullOrEmpty(tbPassword.Text));
	}

	protected void BXContextMenuToolbar1_CommandClick(object sender, CommandEventArgs e)
	{
		switch (e.CommandName.ToLower())
		{
			case "deleteuser":
				try
				{
					if (!this.BXUser.IsCanOperate(BXRoleOperation.Operations.UserDelete))
						throw new Exception(GetMessageRaw("ExceptionText.YouDontHaveRightsToDeleteUser"));

					if (user != null)
					{
						if (BXUserManager.Delete(userId))
						{
							if (!user.ProviderName.Equals(BXUserManager.Provider.Name, StringComparison.InvariantCultureIgnoreCase))
							{
								if (!Membership.Providers[user.ProviderName].DeleteUser(user.UserName, true))
									throw new Exception(GetMessageRaw("ExceptionText.DeletionOfUserFailed"));
							}
						}
						else
						{
							throw new Exception(GetMessageRaw("ExceptionText.DeletionOfUserFailed"));
						}
					}
					else
					{
						throw new Exception(GetMessageRaw("ExceptionText.UserIsNotFound"));
					}
					Response.Redirect("AuthUsersList.aspx");
				}
				catch (BXEventException ex)
				{
					foreach (string s in ex.Messages)
						userValidationSummary.AddErrorMessage(s);
				}
				catch (Exception ex)
				{
					userValidationSummary.AddErrorMessage(ex.Message);
				}

				break;

			default:
				break;
		}
	}
}
