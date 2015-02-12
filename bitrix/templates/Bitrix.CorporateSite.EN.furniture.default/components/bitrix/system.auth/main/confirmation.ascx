

<bx:IncludeComponent
 id="systemconfirmation"
 runat="server"
 componentname="bitrix:system.confirmation"
 template=".default"
 UserId="<%$ Request:UserId%>"
 ActivationToken="<%$ Request:ActivationToken%>"
 />

