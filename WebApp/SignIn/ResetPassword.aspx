<%@ Page Title="" Language="C#" MasterPageFile="~/Angular-Strap.Master" AutoEventWireup="true" CodeBehind="ResetPassword.aspx.cs" Inherits="WebApp.ResetPasswordPage" %>
<%@ Register Namespace="MS.WebUtility" Assembly="MS.WebUtility" TagPrefix="ms_webutil" %>

<asp:Content ContentPlaceHolderID="head" runat="server">

    <script>

        app.constant('TEMPLATE_URL', {} );

    </script>


    <script src="<%=VersionedUri.MapPath("/js/ms-utility.js")%>"></script>
    <script src="<%=VersionedUri.MapPath("/SignIn/reset-password.js")%>"></script>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="container" ng-controller="ResetPasswordController">
        <div class="form-signin">

            <%=TenantName%>

            <h4 class="form-signin-heading">Reset your password</h4>
            <form role="form">
                <div class="form-group">
                    <label for="newPassword">New Password</label>
                    <input type="password" id="newPassword" ng-model="newPassword" class="form-control" placeholder="enter a new password">
                </div>
                <div class="form-group">
                    <label for="confirmPassword">Confirm Password</label>
                    <input type="password" id="confirmPassword" ng-model="confirmPassword" class="form-control" placeholder="confirm password">
                </div>

                <button type="button" ng-click="resetPassword('<%=AuthCode%>', newPassword)" class="btn btn-default btn-block">Reset Password</button>
            </form>
        </div>
    </div>
    <!-- /container -->
</asp:Content>
