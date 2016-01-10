<%@ Page Title="" Language="C#" MasterPageFile="~/AngularJS-SignalR.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebApp.Bootstrap.Default" %>
<%@ Register Namespace="MS.WebUtility" Assembly="MS.WebUtility" TagPrefix="util" %>

<asp:Content ContentPlaceHolderID="head" runat="server">

    <base href="/">

    <script>

        app.constant('TEMPLATE_URL', {} );

        app.config(['$locationProvider', '$stateProvider', '$urlRouterProvider', function ($locationProvider, $stateProvider, $urlRouterProvider) {

          $locationProvider.html5Mode(true);

            $urlRouterProvider.otherwise("/");

            // Now set up the states
            $stateProvider
              .state('bootstrap', {
                  url: "/",
                  templateUrl: "<%=VersionedUri.MapPath("/bootstrap/bootstrap.html")%>",
                  controller: "BootstrapController",
                  data: <%=SiteConfigData%>
                  // auth data specified in SiteConfigData -- would we be better calling it public.bootstrat?
                  //data: { allowedRoles: [AUTHORIZATION_ROLES.anonymous] }
              })
            ;
        }]);
    </script>


    <!-- Le styles 
    <link href="/content/bootstrap.css" rel="stylesheet">
    -->
    <style type="text/css">

    </style>

    <script src="<%=VersionedUri.MapPath("/js/ms-utility.js")%>"></script>

    <script src="<%=VersionedUri.MapPath("/Bootstrap/bootstrap.js")%>"></script>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
</asp:Content>
