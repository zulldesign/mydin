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
using Bitrix.Security;
using System.Collections.Generic;
using Bitrix.DataLayer;
using Bitrix.Modules;

public partial class bitrix_admin_AuthTasksEdit : BXAdminPage
{
	int taskId = -1;

	BXRoleTask task;

	bool currentUserCanModifySettings = false;

    List<CheckBox> cbOperationCollection;

    private CheckBox FindOperationCheckBox(int operId)
    {
        foreach (CheckBox cb in cbOperationCollection)
        {
            if (cb.ID == "cbOper_" + operId.ToString()) return cb;
        }
        return null;
    }


	private void LoadData()
	{
		lbTasks.Items.Clear();

		BXRoleTaskCollection tasks = BXRoleTaskManager.GetList(null, new BXOrderBy_old("TaskName", "Asc"));
		foreach (BXRoleTask tmpTask in tasks)
			if (task == null || !String.Equals(task.TaskName, tmpTask.TaskName))
				lbTasks.Items.Add(new ListItem(tmpTask.Title, tmpTask.TaskName));

		if (task == null)
		{
			taskId = -1;
			hfTaskId.Value = taskId.ToString();

			trTasksCount.Visible = false;
			trOperationsCount.Visible = false;
		}
		else
		{
			Page.Title = string.Format(GetMessage("FormattedPageTitle.ModificationOfTask"), taskId);
			((BXAdminMasterPage)Page.Master).Title = Page.Title;

			trTasksCount.Visible = true;
			trOperationsCount.Visible = true;

			tbTaskName.Text = task.TaskName;
			tbTaskTitle.Text = task.TaskTitle;
			tbComment.Text = task.Comment;
			lbTasksCount.Text = task.TasksCount.ToString();
			lbOperationsCount.Text = task.OperationsCount.ToString();

			string[] subTasksTmp = BXRoleTaskManager.GetList(
				new BXFormFilter(
					new BXFormFilterItem("Parent.Id", task.TaskId, BXSqlFilterOperators.Equal)
				),
				new BXOrderBy_old("TaskName", "Asc")
			).ToStringArray();

			List<string> subTasksTmp1 = new List<string>(subTasksTmp);
                
			foreach (ListItem item in lbTasks.Items)
				if (subTasksTmp1.Contains(item.Value))
					item.Selected = true;

			BXRoleOperationCollection subOperationsTmp = BXRoleOperationManager.GetList(
				new BXFormFilter(
					new BXFormFilterItem("Task.Id", task.TaskId, BXSqlFilterOperators.Equal)
				),
				new BXOrderBy_old("OperationName", "Asc")
			);
                foreach (BXRoleOperation op in subOperationsTmp)
                {

                    CheckBox cb = FindOperationCheckBox(op.OperationId);

                    if (cb!=null)
                    cb.Checked = true;


                }

		}
	}

	protected void Page_Init(object sender, EventArgs e)
	{
        cbOperationCollection = new List<CheckBox>();
		taskId = base.GetRequestInt("id");
		hfTaskId.Value = Request.Form[hfTaskId.UniqueID];
		if (taskId > 0)
			hfTaskId.Value = taskId.ToString();
		Int32.TryParse(hfTaskId.Value, out taskId);
		if (taskId > 0)
		{
			task = BXRoleTaskManager.GetById(taskId);
			if (task == null)
			{
				taskId = 0;
				hfTaskId.Value = taskId.ToString();
			}
		}

		if (taskId <= 0)
		{
			string taskName = base.GetRequestString("name");
			if (!String.IsNullOrEmpty(taskName))
			{
				task = BXRoleTaskManager.GetByName(taskName);
				if (task != null)
				{
					taskId = task.TaskId;
					hfTaskId.Value = taskId.ToString();
				}
			}
		}

		if (!this.BXUser.IsCanOperate(BXRoleOperation.Operations.AuthorizationSettingsView))
			BXAuthentication.AuthenticationRequired();

		currentUserCanModifySettings = this.BXUser.IsCanOperate(BXRoleOperation.Operations.AuthorizationSettingsManage);
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		((BXAdminMasterPage)Page.Master).Title = Page.Title;

		string success = base.GetRequestString("success");
		if ("Y".Equals(success, StringComparison.InvariantCultureIgnoreCase))
			successMessage.Visible = true;
		else
			successMessage.Visible = false;

		string moduleIdTmp = null;
		BXRoleOperationCollection operationsTmp = BXRoleOperationManager.GetList(null, new BXOrderBy_old("ModuleId", "Asc", "OperationName", "Asc"));
		HtmlTableCell c1;
        HtmlTableRow r1;
        foreach (BXRoleOperation operationTmp in operationsTmp)
		{

			if (String.IsNullOrEmpty(moduleIdTmp) || !moduleIdTmp.Equals(operationTmp.ModuleId, StringComparison.InvariantCultureIgnoreCase))
			{
				moduleIdTmp = operationTmp.ModuleId;
				r1 = new HtmlTableRow();
				r1.VAlign = "top";

				c1 = new HtmlTableCell();
				r1.Attributes["class"] = "heading";
				c1.Attributes["width"] = "100%";
                c1.ColSpan = 2;
                try
                {
                    string moduleName = BXModuleManager.GetModule(moduleIdTmp).Name;
                    c1.InnerHtml = Encode(moduleName != null ? string.Concat(moduleName, ":") : string.Format(GetMessageRaw("TableCellInnerHtml.Module"), moduleIdTmp));
                }
                catch
                {
                    c1.InnerHtml = Encode(string.Format(GetMessageRaw("TableCellInnerHtml.Module"), moduleIdTmp));
                }
				if (string.Equals(moduleIdTmp, "main", StringComparison.OrdinalIgnoreCase))
				{
					c1.InnerHtml =
						@"<a href=""#remark1"" style=""vertical-align:super; text-decoration:none""><span class=""required"">1</span></a>"
						+ c1.InnerHtml;
				}

                r1.Cells.Add(c1);
                tblOperations.Rows.Add(r1);
            }
            r1 = new HtmlTableRow();

            r1.VAlign = "top";

            c1 = new HtmlTableCell();
            c1.Attributes["width"] = "40%";
            c1.Attributes["class"] = "field-name";
            CheckBox cb = new CheckBox();
            cb.ID = "cbOper_" + operationTmp.OperationId.ToString();

            cb.Checked = false;

            cbOperationCollection.Add(cb);
            c1.Controls.Add(cb);
            r1.Cells.Add(c1);

            c1 = new HtmlTableCell();
            
            c1.Attributes["width"] = "60%";
            c1.InnerHtml = Server.HtmlEncode(operationTmp.OperationFriendlyName);
		    r1.Cells.Add(c1);



			tblOperations.Rows.Add(r1);
			
		}

		if (!Page.IsPostBack)
			LoadData();
	}

	protected void Page_LoadComplete(object sender, EventArgs e)
	{
		DeleteTaskButton.Visible = AddTaskButton.Visible = taskId > 0 && currentUserCanModifySettings;
		BXTabControl1.ShowSaveButton = BXTabControl1.ShowApplyButton = currentUserCanModifySettings;
	}

	protected void BXTabControl1_Command(object sender, BXTabControlCommandEventArgs e)
	{
		bool noRedirect = false;
		bool successAction = true;
		if (e.CommandName == "save")
		{
			if (!SaveTask())
			{
				successAction = false;
				noRedirect = true;
			}
		}
		else if (e.CommandName == "apply")
		{
			if (!SaveTask())
				successAction = false;
			noRedirect = true;
		}

		if (!noRedirect)
		{
			Page.Response.Redirect("AuthTasksList.aspx");
		}
		else
		{
			if (successAction)
			{
				successMessage.Visible = true;
				LoadData();
			}
		}
	}

	private bool SaveTask()
	{
		if (Page.IsValid)
		{
			if (taskId > 0)
				return UpdateTask();
			else
				return CreateTask();
		}
		return false;
	}

	private bool CreateTask()
	{
		try
		{
			if (!currentUserCanModifySettings)
				throw new Exception(GetMessageRaw("ExceptionText.YouDontHaveRightsToCreateTask"));

			task = BXRoleTaskManager.Create(tbTaskName.Text, tbTaskTitle.Text, tbComment.Text);
			if (task == null)
				throw new Exception(GetMessageRaw("ExceptionText.CreationOfTaskFailed"));

			taskId = task.TaskId;
			hfTaskId.Value = taskId.ToString();

            BXRoleOperationCollection operationsTmp =
            BXRoleOperationManager.GetList(null, null);

            List<string> addOper = new List<string>();

            foreach (BXRoleOperation op in operationsTmp)
            {
                CheckBox cb = FindOperationCheckBox(op.OperationId);

                if (cb != null)
                {
                    if (cb.Checked)
                    {
                        addOper.Add(op.OperationName);
                    }
                }

            }

            if (addOper.Count > 0) task.AddToOperations(addOper.ToArray());

			return true;
		}
		catch (Exception e)
		{
			errorMessage.AddErrorMessage(e.Message);
		}
		return false;
	}

	private bool UpdateTask()
	{
		try
		{
			if (!currentUserCanModifySettings)
				throw new Exception(GetMessageRaw("ExceptionText.YouDontHaveRightsToModifyTask"));

			task.TaskName = tbTaskName.Text;
			task.Comment = tbComment.Text;
			task.TaskTitle = tbTaskTitle.Text;

			task.Update();

			List<string> taskNamesTmp1 = new List<string>();
			foreach (ListItem item in lbTasks.Items)
				if (item.Selected)
					taskNamesTmp1.Add(item.Value);

			task.RemoveFromTasks();
			if (taskNamesTmp1.Count > 0)
			{
				string[] taskNamesTmp = taskNamesTmp1.ToArray();
				task.AddToTasks(taskNamesTmp);
			}


            BXRoleOperationCollection operationsTmp = 
                BXRoleOperationManager.GetList(null, null);

            List<string> addOper = new List<string>();

            foreach (BXRoleOperation op in operationsTmp)
            {
                CheckBox cb = FindOperationCheckBox(op.OperationId);

                if (cb != null)
                {
                    if (cb.Checked)
                    {
                        addOper.Add(op.OperationName);
                    }
                }

            }

            task.RemoveFromOperations();
            if(addOper.Count>0) task.AddToOperations(addOper.ToArray());

			return true;
		}
		catch (Exception e)
		{
			errorMessage.AddErrorMessage(e.Message);
		}

		return false;
	}

	protected void BXContextMenuToolbar1_CommandClick(object sender, CommandEventArgs e)
	{
		switch (e.CommandName.ToLower())
		{
			case "deletetask":
				try
				{
					if (!this.BXUser.IsCanOperate(BXRoleOperation.Operations.AuthorizationSettingsManage))
						throw new Exception(GetMessageRaw("ExceptionText.YouDontHaveRightsToDeleteTask"));

					if (task != null)
					{
						if (!BXRoleTaskManager.Delete(taskId))
							throw new Exception(GetMessageRaw("ExceptionText.DeletionOfTaskFailed"));
					}
					else
					{
						throw new Exception(GetMessageRaw("ExceptionText.TaskIsNotFound"));
					}
					Response.Redirect("AuthTasksList.aspx");
				}
				catch (BXEventException ex)
				{
					foreach (string s in ex.Messages)
						errorMessage.AddErrorMessage(s);
				}
				catch (Exception ex)
				{
					errorMessage.AddErrorMessage(ex.Message);
				}

				break;

			default:
				break;
		}
	}
}
