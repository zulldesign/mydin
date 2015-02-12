<%@ Application Language="C#" %>
<%@ Import Namespace="SiteUpdater" %>
<%@ Import Namespace="Bitrix" %>

<script RunAt="server">
	void Application_Start(object sender, EventArgs e)
	{
		BXApplicationHelper.ApplicationStart(this);
	}

	void Application_End(object sender, EventArgs e)
	{
		BXApplicationHelper.ApplicationEnd(this);
	}

	public void Session_OnStart()
	{
		BXApplicationHelper.SessionStart();
	}

	public void Session_OnEnd()
	{
		BXApplicationHelper.SessionEnd();
	}

	protected void RoleManager_OnGetRoles(object sender, RoleManagerEventArgs args)
	{
		BXApplicationHelper.RoleManagerGetRoles(args);
	}
</script>