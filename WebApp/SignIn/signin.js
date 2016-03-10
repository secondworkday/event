
app.controller('WelcomeController', function ($scope, $state, $stateParams, $modal, $window, utilityService, siteService) {

  // should be not authenticated (although perhaps we're authenticated - not sure if that matters)
  $scope.formData = {};
  // User has an invitation authToken
  // Might be valid or invalid

  // If valid, need to onboard user by accepting their password
  // call to set password, similar to res

  var authCode = $stateParams.authCode;

  $scope.createUser = function () {
    siteService.redeemUserSignupInvitation(authCode, $scope.formData.password, $scope.formData)
    .then(function () {
      $window.location.href = '/';
    });
  };

  //!! if we can successfully create a new, then we should additionally log them in.



  // init
  $scope.createUserAuthCode = $stateParams.authCode;
  $scope.createUserAuthToken = null;

  utilityService.findAuthToken($scope.createUserAuthCode)
  .then(function (authTokenData) {
    $scope.createUserAuthToken = authTokenData;
  });

});


app.controller('SignInController', function ($scope, $state, $modal, $http, $window, utilityService) {

  $scope.formData = {};

  $scope.signIn = function (credentials) {
    utilityService.signIn(credentials)
    .then(function () {
      $window.location.href = '/dashboard';

      //$state.go('master.dashboard', {}, { reload: true });
    });
  };

  $scope.createJobSeeker = function () {
    utilityService.torqCreateDemoUser("JobSeeker")
    .then(function (newUserCred) {
      // hmm. Not sure what to call newUserCred. It contains the user's email - which on a demo user is enough to get us signed in! Score
      utilityService.signIn(newUserCred)
      .then(function () {
        $state.go('master.js.tabs.careers', {}, { reload: true });
        //!!$window.location.href = '/home';
      });

      //$state.go('master.dashboard', {}, { reload: true });
    });
  };

  $scope.demoSignIn = function (credentials) {
    utilityService.signIn(credentials)
      .then(function () {
        $state.go('master.js.tabs.careers');
        //!!$window.location.href = '/home';
      });

  };


  // The server provides a set of Guest users via the ASPX page
  var demoTenants = $state.current.data.demo.demoTenants;
  if (demoTenants && demoTenants.length > 0) {
    $scope.demoUsers = demoTenants[0].users;

    $scope.jobSeekerUsers = $scope.demoUsers.filter(function (user) {
      return user.appRoles.indexOf("JobSeeker") != -1;
    })

  }




    $scope.roles = [
        { name: 'System Administrator', filter: { systemRoles: 'SystemAdmin' } },
        { name: 'Administrator', filter: { systemRoles: 'TenantAdmin' } },
        { name: 'Radiologist', filter: { appRoles: 'Radiologist' } },
        { name: 'Technologist', filter: { appRoles: 'Technologist' } }
    ];

    $scope.demo = false;

    // init
    // (nothing to do)
});


app.controller('ResetPasswordController', function ($scope, $stateParams, $mdToast, utilityService, webUtilityService) {

    $scope.popover = { title: 'Password Reset', content: 'Hello<br />message!', email: 'Jeremy@Mercer' };

    //!! Ok - there must be a way better way than passing fnHide as a parameter here. I'm missing something really basic.
    $scope.sendPasswordResetEmail = function (email, fnHide) {
      webUtilityService.sendResetPasswordEmail(email)
        .then(function (successData) {
          $mdToast.showSimple("Yessir.");

          //var myAlert = $alert({ title: 'Password Reset', content: 'Password reset email sent to ' + email + '.', placement: 'top', type: 'info', show: true });
          //fnHide();
        }, function (failureData) {

          if (failureData.StatusCode == 403) {
            $mdToast.showSimple(failureData.errorMessage || failureData.StatusCodeDescription);

            //var myAlert = $alert({ title: 'Password Reset', content: failureData.StatusCodeDescription, placement: 'top', container: 'body', type: 'info', duration: 3, show: true });
            //fnHide();
            return;
          }

          //!! TODO this should be displayed as an error on the email field
          //var myAlert = $alert({ title: 'Password Reset', content: failureData.StatusCodeDescription, placement: 'top', type: 'info', show: true });

          $mdToast.showSimple(failureData.errorMessage);

        });
    };

    $scope.resetPassword = function () {
      utilityService.resetPassword(authCode, $scope.formData.password)
        .then(function (successData) {
          console.log("success", successData);
        }, function (failureData) {
          // failure
          console.log("failure", failureData);
        }, function (progressData) {
          // progress
          console.log("progress", progressData);
        });
    };

  // init
  var authCode = $stateParams.authCode;

});
