<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.UI.Wizards.BXWizardStepStandardHtmlControl" %>
<%@ Import Namespace="Bitrix.UI.Wizards" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="System.Data.SqlClient" %>
<script runat="server">
	protected override void OnWizardInit()
	{
		Bitrix.DataTypes.BXParamsBag<object> bag = new Bitrix.DataTypes.BXParamsBag<object>();
	
		bag["ServerName"] = "localhost";	
		bag["CreateUser"] = "False";
		bag["CreateDB"] = "False";
		
		WizardContext.State["DB"] = bag;
		
		if (WizardContext.State.ContainsKey("Options.ConnectionString"))
			WizardContext.Navigation.Remove(WizardContext.Navigation["db"]);
	}
	
	protected override BXWizardResult OnActionShow(Bitrix.DataTypes.BXCommonBag parameters)
	{
		if (WizardContext.State.ContainsKey("Options.ConnectionString"))
			return Result.Next();
		
		WizardContext.Navigation.Selected = "db";		
	
		UI.Load("DB");
		BXWizardResultView view = Result.Render(GetMessage("Title"));
		view.Buttons.Add("prev", null);
		view.Buttons.Add("finish", GetMessage("Finish"));
		return view;
	}
	
	protected override BXWizardResult OnActionPrevious(Bitrix.DataTypes.BXCommonBag parameters)
	{
		return Result.Previous();
	}
		
	protected override BXWizardResult OnActionFinish(Bitrix.DataTypes.BXCommonBag parameters)
	{
		UI.LoadValues(parameters);
		System.Collections.Generic.List<string> errors = new System.Collections.Generic.List<string>();
		if (!Validate(errors))
		{
			BXWizardResultView view = Result.Render(GetMessage("Title"), errors);
			view.Buttons.Add("prev", null);
			view.Buttons.Add("finish",  GetMessage("Finish"));
			return view;
		}

		UI.Overwrite("DB");
		return Result.Action("prepare", "", null);
	}

	private bool Validate(System.Collections.Generic.List<string> errors)
	{
		string serverName = UI.Data.GetString("ServerName");
		if (string.IsNullOrEmpty(serverName))
			errors.Add(GetMessage("Error.EmptyServerName"));
		
		string dbName = UI.Data.GetString("DBName");
		if (string.IsNullOrEmpty(dbName))
			errors.Add(GetMessage("Error.EmptyDBName"));
		else if (!Regex.IsMatch(dbName, @"^[a-zA-Z0-9_-]+$")) 
			errors.Add(GetMessage("Error.InvalidDBName"));
			

		if (errors.Count > 0)
			return false;
		string checkString = null;
		
		// Verify admin connection info
		if (UI.Data.GetString("CreateUser") == "True" || UI.Data.GetString("CreateDB") == "True")
		{
			SqlConnectionStringBuilder sb = new SqlConnectionStringBuilder();
			sb.DataSource = serverName;
			if (UI.Data.GetString("CreateDB") != "True")
				sb.InitialCatalog = UI.Data.GetString("DBName");
			if (UI.Data.GetBool("AdminWindows"))
				sb.IntegratedSecurity = true;
			else 
			{
				sb.UserID = UI.Data.GetString("AdminLogin", "");
				if (string.IsNullOrEmpty(sb.UserID))
					errors.Add(GetMessage("Error.EmptyAdminLogin"));
				sb.Password = UI.Data.GetString("AdminPassword", "");
			}
			if (sb.IntegratedSecurity || !string.IsNullOrEmpty(sb.UserID))
			using (SqlConnection c = new SqlConnection(sb.ConnectionString))
			{
				try		
				{
					c.Open();
					if (checkString == null)
						checkString = sb.ConnectionString;
				}
				catch
				{
					errors.Add(GetMessage("Error.AdminConnectionFailed"));
				}
			}
		}
		
		// Verify user connection info
		bool userString = false;
		if (UI.Data.GetString("CreateUser") != "True")
		{
			SqlConnectionStringBuilder sb = new SqlConnectionStringBuilder();
			sb.DataSource = serverName;
			if (UI.Data.GetString("CreateUser") == "Windows")
				sb.IntegratedSecurity = true;
			else 
			{
				sb.UserID = UI.Data.GetString("UserLogin", "");
				if (string.IsNullOrEmpty(sb.UserID))
					errors.Add(GetMessage("Error.EmptyUserLogin"));
				sb.Password = UI.Data.GetString("UserPassword", "");
			}
			if (UI.Data.GetString("CreateDB") != "True")
				sb.InitialCatalog = UI.Data.GetString("DBName");
			if (sb.IntegratedSecurity || !string.IsNullOrEmpty(sb.UserID))
			using (SqlConnection c = new SqlConnection(sb.ConnectionString))
			{
				try		
				{
					c.Open();
					if (checkString == null)
					{
						checkString = sb.ConnectionString;
						userString = true;
					}
				}
				catch
				{
					errors.Add(GetMessage("Error.UserConnectionFailed"));
				}
			}
		}
		
		// Check if database exists
		bool? dbExists = null;
		if (checkString != null)
		{
			using (SqlConnection c = new SqlConnection(checkString))
			{
				try		
				{
					c.Open();
					using (SqlCommand cmd = new SqlCommand("SELECT DB_ID(@db)", c))
					{
						cmd.Parameters.Add("@db", System.Data.SqlDbType.NVarChar).Value = UI.Data.GetString("DBName");
						dbExists = !Convert.IsDBNull(cmd.ExecuteScalar());
					}
				}
				catch
				{
				}
			}
		}
		if (dbExists != null)
		{
			if (dbExists.Value && UI.Data.GetString("CreateDB") == "True")
				errors.Add(GetMessage("Error.DBAlreadyExists"));
			else if (!dbExists.Value && UI.Data.GetString("CreateDB") != "True")
				errors.Add(GetMessage("Error.DBDoesntExist"));
			else if (dbExists.Value) 
			{
				// check if db is empty
				if (!UI.Data.GetBool("EmptyDB"))
				{
					try
					{
						using (SqlConnection c = new SqlConnection(checkString))
						{
							c.Open();
							try
							{
								using (SqlCommand cmd = new SqlCommand("SELECT OBJECT_ID('" + UI.Data.GetString("DBName") + ".dbo.b_Users')", c))
								{
									if (!Convert.IsDBNull(cmd.ExecuteScalar()))
										errors.Add(GetMessage("Error.DBNotEmpty"));
								}
							}
							catch
							{
							}
						}
					}
					catch
					{
					}
				}
				
				// check user operations on db
				if (userString)
				{
					try
					{
						using (SqlConnection c = new SqlConnection(checkString))
						{
							c.Open();
							try
							{
								CheckDBOperations(c);
							}
							catch(Exception ex)
							{
								errors.Add(ex.Message);
							}
						}
					}
					catch
					{
					}
				}
			}
			else if (!dbExists.Value) // check db creation operations
			{
				string tempDbName = "Bitrix_Installer_Check_DeleteMe_" + DateTime.Now.Ticks;
				try
				{		
					try
					{
						using (SqlConnection c = new SqlConnection(checkString))
						{
							c.Open();
							using (SqlCommand cmd = new SqlCommand(
								string.Format("CREATE DATABASE [{0}] COLLATE SQL_Latin1_General_CP1_CI_AS", tempDbName),
								c
							))
								cmd.ExecuteNonQuery();
						}
					}
					catch
					{
						errors.Add(GetMessage("Error.CantCreateDB"));
					}
				}
				finally
				{
					try
					{
						using (SqlConnection c = new SqlConnection(checkString))
						{
							c.Open();
							using (SqlCommand cmd = new SqlCommand(
								string.Format("DROP DATABASE [{0}]", tempDbName),
								c
							))
								cmd.ExecuteNonQuery();
						}
					}
					catch
					{
					}
				}
			}
		}
		
		return errors.Count == 0;
	}
	
	private void CheckDBOperations(SqlConnection conn)
	{
		string tableName = "b_Bitrix_Installer_Check_DeleteMe";
		bool tableCreated = false;
		bool tableAltered = false;
		try
		{
			try
			{		
				using (SqlCommand cmd = new SqlCommand("CREATE TABLE " + tableName + " (id int)", conn))
				{
					cmd.ExecuteNonQuery();
					tableCreated = true;
				}
			}
			catch
			{
				throw new Exception(GetMessage("Error.UnableToCreateTable"));
			}
			
			try
			{		
				using (SqlCommand cmd = new SqlCommand("ALTER TABLE " + tableName + " ADD second varchar(100)", conn))
				{
					cmd.ExecuteNonQuery();
					tableAltered = true;
				}
			}
			catch
			{
				throw new Exception(GetMessage("Error.UnableToAlterTable"));
			}
		}
		finally
		{
			try
			{
				using (SqlCommand cmd = new SqlCommand("DROP TABLE " + tableName, conn))
					cmd.ExecuteNonQuery();
			}
			catch
			{
				if (tableCreated && tableAltered)
					throw new Exception(GetMessage("Error.UnableToDropTable"));
			}
		}	
	}
	
</script>
<script type="text/javascript">
	function ShowTableRow(row, visible)
	{
		row.style.display = visible ? "" : "none";
	}

	function RefreshGui() 
	{
		var obj;
		obj = document.getElementById("dbsetup_createuser_1");
		var createUser = obj && obj.checked;
		
		obj = document.getElementById("dbsetup_createuser_2");
		var windowsAuthUser = obj && obj.checked;
		
		obj = document.getElementById("dbsetup_createdb_1");
		var createDB = obj && obj.checked;

		obj = document.getElementById("dbsetup_windowsadmin");
		var windowsAuthAdmin = obj && obj.checked;

		for (var i = 0; i < 2; i++) 
		{
			obj = document.getElementById("dbsetup_row_user_" + i);
			if (obj)
				obj.style.display = !windowsAuthUser ? "" : "none";
		}

		for (var i = 0; i < 4; i++) 
		{
			obj = document.getElementById("dbsetup_row_admin_" + i);
			if (obj)
				obj.style.display = (createDB || createUser) && (i < 2 || !windowsAuthAdmin) ?  "" : "none";
		}

		obj = document.getElementById("dbsetup_dbexists");
		if (obj)
			obj.style.display = !createDB ? "" : "none";

		obj = document.getElementById("dbsetup_dbnew");
		if (obj)
			obj.style.display = createDB ? "" : "none";
			
		for (var i = 0; i < 2; i++) 
		{
			obj = document.getElementById("dbsetup_row_createdb_" + i);
			if (obj)
				obj.style.display = !createDB ?  "" : "none";
		}
	}
</script>

<table border="0" class="data-table">
	<tr>
		<td colspan="2" class="header"><%= GetMessage("Header.DBSettings") %></td>
	</tr>
	<tr>
		<td nowrap align="right" valign="top" width="40%"><span style="color: red">*</span><%= GetMessage("Label.ServerName") %>:</td>
		<td width="60%" valign="top">
			<% UI.InputText("ServerName", null); %><br />
			<small><%= GetMessage("Hint.ServerName") %></small>
		</td>
	</tr>
	<tr>
		<td align="right" valign="top"><%= GetMessage("Label.User") %>:</td>
		<td valign="top">
			<% 
			UI.RadioButtonList(
				"CreateUser",
				new ListItem[] 
				{
					new ListItem(GetMessage("Option.UserExisting"), "False"),
					new ListItem(GetMessage("Option.UserNew"), "True"),
					new ListItem(GetMessage("Option.UserWindows"), "Windows")
				},
				new KeyValuePair<string, string>[]
				{
					new KeyValuePair<string, string>("id", "dbsetup_createuser"),
					new KeyValuePair<string, string>("onclick", "RefreshGui();")
				}
			); 
			%>
		</td>
	</tr>
	<tr id="dbsetup_row_user_0">
		<td nowrap align="right" valign="top"><span style="color: red">*</span><%= GetMessage("Label.UserLogin") %>:</td>
		<td valign="top">
			<% UI.InputText("UserLogin", null); %><br />
			<small><%= GetMessage("Hint.UserLogin") %></small>
		</td>
	</tr>
	<tr id="dbsetup_row_user_1">
		<td nowrap align="right" valign="top"><%= GetMessage("Label.UserPassword") %>:</td>
		<td valign="top">
			<% UI.InputPassword("UserPassword", null); %><br />
			<small><%= GetMessage("Hint.UserPassword") %></small>
		</td>
	</tr>
	<tr>
		<td nowrap align="right" valign="top"><%= GetMessage("Label.DB") %>:</td>
		<td valign="top">
			<% 
			UI.RadioButtonList(
				"CreateDB",
				new ListItem[] 
				{
					new ListItem(GetMessage("Option.DBExisting"), "False"),
					new ListItem(GetMessage("Option.DBNew"), "True")
				},
				new KeyValuePair<string, string>[]
				{
					new KeyValuePair<string, string>("id", "dbsetup_createdb"),
					new KeyValuePair<string, string>("onclick", "RefreshGui();")
				}
			); 
			%>
		</td>
	</tr>
	<tr>
		<td nowrap align="right" valign="top">
			<div id="dbsetup_dbexists">
				<span style="color: red">*</span><%= GetMessage("Label.DBName") %>:</div>
			<div id="dbsetup_dbnew" style="display: none">
				<span style="color: red">*</span><%= GetMessage("Label.NewDBName") %>:</div>
		</td>
		<td valign="top">
			<% UI.InputText("DBName", null); %><br />
			<small><%= GetMessage("Hint.DBName") %></small>
		</td>
	</tr>
	<tr id="dbsetup_row_admin_0">
		<td colspan="2" class="header"><%= GetMessage("Header.Admin") %></td>
	</tr>
	<tr id="dbsetup_row_admin_1">
		<td nowrap align="right" valign="top">&nbsp;</td>
		<td valign="top">
			<% 
			UI.CheckBox(
				"AdminWindows", 
				GetMessage("CheckBox.AdminWindows"), 
				new KeyValuePair<string, string>[]
				{
					new KeyValuePair<string, string>("id", "dbsetup_windowsadmin"),
					new KeyValuePair<string, string>("onclick", "RefreshGui();")
				}
			); 
			%>
		</td>
	</tr>
	<tr id="dbsetup_row_admin_2">
		<td nowrap align="right" valign="top"><span style="color: red">*</span><%= GetMessage("Label.AdminLogin") %>:</td>
		<td valign="top">
			<% UI.InputText("AdminLogin", null); %><br />
			<small><%= GetMessage("Hint.AdminLogin") %></small>
		</td>
	</tr>
	<tr id="dbsetup_row_admin_3">
		<td nowrap align="right" valign="top"><%= GetMessage("Label.AdminPassword") %>:</td>
		<td valign="top">
			<% UI.InputPassword("AdminPassword", null); %><br />
			<small><%= GetMessage("Hint.AdminPassword") %></small>
		</td>
	</tr>
	<tr id="dbsetup_row_createdb_0">
		<td colspan="2" class="header"><%= GetMessage("Header.Additional") %></td>
	</tr>
	<tr id="dbsetup_row_createdb_1">
		<td nowrap align="right" valign="top">&nbsp;</td>
		<td valign="top">
			<% UI.CheckBox("EmptyDB", GetMessage("CheckBox.EmptyDB"), null); %>
		</td>
	</tr>
</table>
<script type="text/javascript">window.setTimeout(function() { RefreshGui(); }, 0);</script>