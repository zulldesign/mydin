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
using Bitrix.IO;
using Bitrix.Security;
using System.Security;
using Bitrix.Services;
using System.Collections.Generic;
using Bitrix.Modules;
using Bitrix.DataTypes;

public partial class bitrix_dialogs_Security : BXDialogPage
{
	string curPath;

	void LoadAuthorizationState()
	{
		BXFileAuthorizationState state = BXFileAuthorizationManager.BuildAuthorizationState(curPath);
		List<int> changedRoles = new List<int>();

		//Проходим по всем путям, от которых можно унаследовать права
		foreach (KeyValuePair<string, BXParamsBag<Dictionary<int, bool>>> path in state)
		{
			if (BXPathComparer.IsSubDir(path.Key, curPath))
			{
				bool thisPath = BXPathComparer.Instance.Equals(path.Key, curPath);
				//Проходим по всем операциям и ролям
				foreach (KeyValuePair<string, Dictionary<int, bool>> op in path.Value)
				{
					//Находим операцию в списке операций
					int i = Array.FindIndex(BXFileOperation.Operations, delegate(string value)
					{
						return string.Equals(op.Key, value, StringComparison.InvariantCultureIgnoreCase);
					});
					if (i == -1)
						continue;

					string operation = BXFileOperation.Operations[i];

					foreach (KeyValuePair<int, bool> r in op.Value)
					{
						//Находим роль в списке ролей
						BXOperationsEditRoleInfo roleInfo = null;
						if (!OperationsEdit.Roles.TryGetValue(r.Key.ToString(), out roleInfo))
							continue;

						BXOperationsEditOperationInfo operationInfo = null;
						if (!roleInfo.Operations.TryGetValue(operation, out operationInfo))
							roleInfo.Operations.Add(operation, operationInfo = new BXOperationsEditOperationInfo());

						if (thisPath)
						{
							//если права заданы на уровне самого файла, то отмечаем их отдельно
							operationInfo.State = op.Value[r.Key] ? BXOperationsEditOperationState.Allowed : BXOperationsEditOperationState.Denied;
						}
						else
						{
							//иначе наследуем
							operationInfo.State = BXOperationsEditOperationState.Inherited;
							operationInfo.InheritedState = op.Value[r.Key] ? BXOperationsEditInheritedOperationState.Allowed : BXOperationsEditInheritedOperationState.Denied;
						}

						int pos = changedRoles.BinarySearch(r.Key);
						if (pos < 0)
							changedRoles.Insert(~pos, r.Key);
					}
				}
			}
			//BXOperationsEditRoleInfo role = OperationsEdit.Roles[roleIdString];

			//Устанавливаем стартовое значение - все операции наследуют запрещение
			//foreach (string op in BXFileOperation.Operations)
			//	role.Operations[op] = new BXOperationsEditOperationInfo(BXOperationsEditOperationState.Inherited, BXOperationsEditInheritedOperationState.Denied);
		}

		foreach (int roleId in changedRoles)
			OperationsEdit.State.SetRoleFromData(roleId.ToString());
	}
	new void Validate()
	{
		OperationsEdit.State.Validate(false);
		BXSecureIO.DemandSecurity(curPath);
	}
	new void Save()
	{
		bool changed = false;
		Dictionary<string, BXAuthorizationFile> fileCache = new Dictionary<string, BXAuthorizationFile>();
		foreach (BXRole role in BXRoleManager.GetList(null, null))
		{
			string roleId = role.RoleId.ToString();
			bool deleteAll = !OperationsEdit.State.ContainsRole(roleId);
			foreach(string op in BXFileOperation.Operations)
			{
				if (deleteAll || OperationsEdit.State[roleId, op] == BXOperationsEditOperationState.Inherited)
					BXFileAuthorizationManager.DeleteLocalRight(curPath, role.RoleName, op, fileCache, false);
				else
					BXFileAuthorizationManager.SetLocalRight(curPath, role.RoleName, op, OperationsEdit.State[roleId, op] == BXOperationsEditOperationState.Allowed, fileCache, false);
			}
		}
		try
		{
			foreach (KeyValuePair<string, BXAuthorizationFile> file in fileCache)
			{
				file.Value.PlaceAllAndNotAuthorizedFirst();
				file.Value.RemoveEmptyLocations();

				List<string> messages = new List<string>();


				BXCommand command = new BXCommand("Bitrix.Security.OnBeforeSaveAuthorizationFile");
				command.Parameters.Add("path", file.Key);
				command.Parameters.Add("authorizationFile", file.Value);
				command.Send();
				foreach (KeyValuePair<string, BXCommandResult> kvp in command.CommandResultDictionary)
					if (kvp.Value.CommandResult == BXCommandResultType.Cancel)
						messages.Add(kvp.Value.Result.ToString());
				if (messages.Count > 0)
					throw new BXEventException(messages);

				command = new BXCommand("Bitrix.Security.OnSaveAuthorizationFile");
				command.Parameters.Add("path", file.Key);
				command.Parameters.Add("authorizationFile", file.Value);
				command.Send();
				foreach (KeyValuePair<string, BXCommandResult> kvp in command.CommandResultDictionary)
					if (kvp.Value.CommandResult == BXCommandResultType.Error)
						messages.Add(kvp.Value.Result.ToString());
				if (messages.Count > 0)
					throw new BXEventException(messages);

				if (file.Value.Save(file.Key))
					changed = true;

				command = new BXCommand("Bitrix.Security.OnAfterSaveAuthorizationFile");
				command.Parameters.Add("path", file.Key);
				command.Parameters.Add("authorizationFile", file.Value);
				command.Send();
				foreach (KeyValuePair<string, BXCommandResult> kvp in command.CommandResultDictionary)
					if (kvp.Value.CommandResult == BXCommandResultType.Error)
						messages.Add(kvp.Value.Result.ToString());
				if (messages.Count > 0)
					throw new BXEventException(messages);
			}
		}
		finally
		{
			if (changed)
				new BXCommand("Bitrix.Security.FileAuthorizationStateChanged").Send();
		}
	}

	protected void Page_Init(object sender, EventArgs e)
	{
		try
		{
			if (string.IsNullOrEmpty(Request["path"]))
				throw new InvalidOperationException(GetMessageRaw("Exception.InvalisParameterAreDetected"));
			curPath = BXPath.ToVirtualRelativePath(Request["path"]);
			if (!BXSecureIO.FileOrDirectoryExists(curPath))
				throw new InvalidOperationException(GetMessageRaw("Exception.PageDoesntExist"));

			BXSecureIO.DemandSecurity(curPath);

			OperationsEdit.FillStandardRoles(false);
			foreach (string op in BXFileOperation.Operations)
				OperationsEdit.Operations[op] = BXFileAuthorizationManager.GetActionTypeDisplayName(op);
		}
		catch (System.Threading.ThreadAbortException)
		{
			//...игнорируем, вызвано Close();
		}
		catch (BXSecureIOException ex)
		{
			Close(ex.Message, BXDialogGoodbyeWindow.LayoutType.Error, -1);
		}
		catch (InvalidOperationException ex)
		{
			Close(ex.Message, BXDialogGoodbyeWindow.LayoutType.Error, -1);
		}
		catch (Exception ex)
		{
			Close(GetMessage("GoodbyeMessage.AnErrorHasOccurred"), BXDialogGoodbyeWindow.LayoutType.Error, -1);
			BXLogService.LogAll(ex, 0, BXLogMessageType.Error, AppRelativeVirtualPath);
		}
	}
	protected void Page_LoadComplete(object sender, EventArgs e)
	{
		if (!IsPostBack)
			LoadAuthorizationState();

        Behaviour.Settings.MinWidth = 532;
        Behaviour.Settings.MinHeight = 554;
        Behaviour.Settings.Width = 532;
        Behaviour.Settings.Height = 554;
        Behaviour.Settings.Resizeable = false;

		DescriptionIconClass = "bx-access-page";

		DescriptionParagraphs.Add(string.Format(
			"{0} {1} <i>{2}</i>",
			GetMessage("DlgDescription.ModificationOfAccessRightsFor"),
			BXSecureIO.FileExists(curPath) ? GetMessage("DlgDescription.File") : GetMessage("DlgDescription.Folder"),
			Encode(curPath)
		));
	}
	protected void Behaviour_Save(object sender, EventArgs e)
	{
		try
		{
			Validate();
			Save();

			Close(GetMessage("GoodbyeMessage.ModificationIsCompletedSuccessfully"));
		}
		catch (System.Threading.ThreadAbortException)
		{
			//...игнорируем, вызвано Reload();
		}
		catch (BXEventException ex)
		{
			ShowError(ex.Message);
		}
		catch (BXSecureIOException ex)
		{
			ShowError(ex.Message);
		}
		catch (InvalidOperationException ex)
		{
			ShowError(ex.Message);
		}
		catch (Exception ex)
		{
			ShowError(ex.Message);
			BXLogService.LogAll(ex, 0, BXLogMessageType.Error, AppRelativeVirtualPath);
		}
	}
}
