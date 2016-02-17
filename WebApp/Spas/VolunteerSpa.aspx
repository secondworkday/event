<%@ Page Title="" Language="C#" MasterPageFile="~/AngularJS-SignalR.Master" AutoEventWireup="true" CodeBehind="VolunteerSpa.aspx.cs" Inherits="WebApp.Spas.VolunteerSpaPage" %>
<%@ Register Namespace="MS.WebUtility" Assembly="MS.WebUtility" TagPrefix="util" %>
<%@ Register Namespace="App.Library" Assembly="AppLibrary" TagPrefix="lib" %>


<asp:Content ContentPlaceHolderID="head" runat="server">
    <base href="/">

    <link rel="shortcut icon" type="image/x-icon" href="/favicon.ico"/>

    <link rel="stylesheet" href="<%=VersionedUri.MapPath("/client/app.css")%>">


    <link rel="apple-touch-icon" sizes="57x57" href="/images/app-icon/apple-icon-57x57px.png" />
    <link rel="apple-touch-icon" sizes="72x72" href="/images/app-icon/apple-icon-72x72px.png" />
    <link rel="apple-touch-icon" sizes="114x114" href="/images/app-icon/apple-icon-114x114px.png" />
    <link rel="apple-touch-icon" sizes="144x144" href="/images/app-icon/apple-icon-144x144px.png" />
    <script>
        // We like to define all our URLs in an ASPX file, so we can run VersionedUri on them which generates a cache-breaking parameter of the form ?t={time value}
        app.constant('TEMPLATE_URL', {
        });

        app.constant('CATEGORY_IDS', {
            userProfilePhotoResourceID: "<%=MS.Utility.EPCategory.ProfilePhotoResource.ID%>",
            tenantLogoResourceID: "<%=MS.Utility.EPCategory.TenantLogoResource.ID%>",
        });

      app.constant('APP_ROLE_TRANSLATION', {
        Admin: 'TENANT_ADMIN_ROLE',
        EventPlanner: 'EVENT_PLANNER_ROLE',
        EventSessionVolunteer: 'SESSION_VOLUNTEER_ROLE'
      });


      app.constant('AUTHORIZATION_ROLES', {
        // Other
        anonymous: "Anonymous",
        authenticated: "Authenticated",
        // SystemRoles
        operationsAdmin: "OperationsAdmin",
        tenantAdmin: "TenantAdmin",
        securityAdmin: "SecurityAdmin",
        systemAdmin: "SystemAdmin",

        // AppRoles

        admin: "Admin",
        eventPlanner: "EventPlanner",
        eventSessionVolunteer: "EventSessionVolunteer"
      });


        app.config(['$locationProvider', '$stateProvider', '$urlRouterProvider', '$mdThemingProvider', 'AUTHORIZATION_ROLES', function ($locationProvider, $stateProvider, $urlRouterProvider, $mdThemingProvider, AUTHORIZATION_ROLES) {

            $locationProvider.html5Mode(true);
            $urlRouterProvider.otherwise("/");

            //Now set up the theme
            $mdThemingProvider.theme('default')
              .primaryPalette('indigo')
              .accentPalette('pink');
        }]);


    </script>


    <% if (WebUtilityContext.Current.SiteBaseDomain.StartsWith("dev.") || WebUtilityContext.Current.SiteBaseDomain.StartsWith("rolling.")) { %>

      <%--On Dev (dev.*) or Rolling (rolling.*) sites, we don't want to emit anything when optimizations are off. That allows Charles to work --%>
      <%: Scripts.Render("~/bundles/volunteerSpaTemplates") %>

    <% } else { %>

      <%--Everywhere else, we always want to bundler our templates even when BundleTable.EnableOptimizations is off --%>
      <script src="<%: BundleTable.Bundles.ResolveBundleUrl("~/bundles/volunteerSpaTemplates") %>" type="text/javascript"></script>

    <% } %>


    <%: Scripts.Render("~/bundles/volunteerSpaJS") %>



    <%--favicons for iOS, Android, and Win8--%>
    <link rel="apple-touch-icon" sizes="57x57" href="/apple-touch-icon-57x57.png?v=YAB9gwBN7x">
    <link rel="apple-touch-icon" sizes="60x60" href="/apple-touch-icon-60x60.png?v=YAB9gwBN7x">
    <link rel="apple-touch-icon" sizes="72x72" href="/apple-touch-icon-72x72.png?v=YAB9gwBN7x">
    <link rel="apple-touch-icon" sizes="76x76" href="/apple-touch-icon-76x76.png?v=YAB9gwBN7x">
    <link rel="apple-touch-icon" sizes="114x114" href="/apple-touch-icon-114x114.png?v=YAB9gwBN7x">
    <link rel="apple-touch-icon" sizes="120x120" href="/apple-touch-icon-120x120.png?v=YAB9gwBN7x">
    <link rel="apple-touch-icon" sizes="144x144" href="/apple-touch-icon-144x144.png?v=YAB9gwBN7x">
    <link rel="apple-touch-icon" sizes="152x152" href="/apple-touch-icon-152x152.png?v=YAB9gwBN7x">
    <link rel="apple-touch-icon" sizes="180x180" href="/apple-touch-icon-180x180.png?v=YAB9gwBN7x">
    <link rel="icon" type="image/png" href="/favicon-32x32.png?v=YAB9gwBN7x" sizes="32x32">
    <link rel="icon" type="image/png" href="/favicon-194x194.png?v=YAB9gwBN7x" sizes="194x194">
    <link rel="icon" type="image/png" href="/favicon-96x96.png?v=YAB9gwBN7x" sizes="96x96">
    <link rel="icon" type="image/png" href="/android-chrome-192x192.png?v=YAB9gwBN7x" sizes="192x192">
    <link rel="icon" type="image/png" href="/favicon-16x16.png?v=YAB9gwBN7x" sizes="16x16">
    <link rel="manifest" href="/manifest.json?v=YAB9gwBN7x">
    <link rel="shortcut icon" href="/favicon.ico?v=YAB9gwBN7x">
    <meta name="msapplication-TileColor" content="#2b5797">
    <meta name="msapplication-TileImage" content="/mstile-144x144.png?v=YAB9gwBN7x">
    <meta name="theme-color" content="#ffffff">
    <meta name="spa" content="event-session-volunteer">

</asp:Content>
<asp:Content ContentPlaceHolderID="body" runat="server">
</asp:Content>
