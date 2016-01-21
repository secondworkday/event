app.controller('SignInController', function ($scope, $state, $mdToast, $mdDialog, $log, webUtilityService, utilityService, siteService) {
  $log.debug('Loading SignInController...');

  $scope.stateData = $state.current.data;

  $scope.signIn = function (credentials) {
    utilityService.signIn(credentials)
      .then(function () {
        // success
        $state.go('app.user.home', {}, { reload: true });
        // $state.go('app.system.users', {}, { reload: true });
      }, function (failureData) {
        // failure
        alert(failureData);
      });
  };


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


  $scope.showDemoAccountDialog = function ($event) {
     var parentEl = angular.element(document.body);
     $mdDialog.show({
       parent: parentEl,
       targetEvent: $event,
       templateUrl: '/client/states/public/sign-in-demo-account.dialog.html',
       locals:{
         stateData : $scope.stateData,
         working : $scope.working
       },
       controller: DemoAccountDialogController
    });
    function DemoAccountDialogController($scope, $mdDialog, siteService, stateData, SIGNIN_INFO) {
      $scope.signinData = SIGNIN_INFO;
      if ($scope.signinData && $scope.signinData.demoTenants) {
        $scope.demoTenant = $scope.signinData.demoTenants[0];
      }

      $scope.working = false;
      $scope.loginAsDemoJobSeeker = function (user) {
        $mdDialog.hide();
        utilityService.signIn(user)
        .then(function () {
          $state.go('app.job-seeker.career-step.home', {}, { reload: true });
        });

      };
      $scope.loginAsDemoCounselor = function (user) {
        $mdDialog.hide();
        utilityService.signIn(user)
        .then(function () {
          $state.go('app.counselor.home', {}, { reload: true });
        });

      };
      $scope.cancelDialog = function() {
        $log.debug( "You canceled the dialog." );
        $mdDialog.hide();
      };
      $scope.createNewDemoJobSeeker = function() {
        $scope.working = true;

        siteService.createDemoJobSeeker()
        .then(function (newUserCred) {
          // hmm. Not sure what to call newUserCred. It contains the user's email - which on a demo user is enough to get us signed in! Score
          utilityService.signIn(newUserCred)
          .then(function () {
            // success
            $state.go('app.job-seeker.onboard.intro', {}, { reload: true });
          }, function (errorMessage) {
            // failure (signIn)
            $log.debug(errorMessage);
          });
        }, function (hubError) {
          // failure (createDemoJobSeeker)
          $log.debug(hubError.errorMessage);
        }).finally(function () {
          $log.debug("createDemoJobSeeker() finally");
          $mdDialog.hide();
        });
      };
      $scope.createNewDemoCounselor = function() {
        $scope.working = true;

        siteService.createDemoCounselor()
        .then(function (newUserCred) {
          // hmm. Not sure what to call newUserCred. It contains the user's email - which on a demo user is enough to get us signed in! Score
          utilityService.signIn(newUserCred)
          .then(function () {
            // success
            $state.go('app.counselor.home', {}, { reload: true });
          }, function (errorMessage) {
            // failure (signIn)
            $log.debug(errorMessage);
          });
        }, function (hubError) {
          // failure (createDemoCounselor)
          $log.debug(hubError.errorMessage);
        }).finally(function () {
          $log.debug("createDemoCounselor() finally");
          $mdDialog.hide();
        });
      };
    }
  };

});
