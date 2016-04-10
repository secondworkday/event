app.controller('SignInController', function ($scope, $state, $mdToast, $mdDialog, $log, webUtilityService, utilityService, siteService) {
  $log.debug('Loading SignInController...');

  $scope.stateData = $state.current.data;

  // this triggers the spinner for 'signing you in...'
  $scope.working = false;

  $scope.signIn = function (credentials) {
    $scope.working = true;
    utilityService.signIn(credentials)
      .then(function () {
        // success
        $state.go('app.user.events', {}, { reload: true });
        // $state.go('app.system.users', {}, { reload: true });
      }, function (failureData) {
        // failure
        $scope.working = false;
        $scope.errorMessage = failureData.errorMessage;
      });
  };


  $scope.showDemoAccountDialog = function ($event) {
    var parentEl = angular.element(document.body);
    $mdDialog.show({
      parent: parentEl,
      targetEvent: $event,
      templateUrl: '/client/states/public/sign-in-demo-account.dialog.html',
      locals: {
        stateData: $scope.stateData,
        working: $scope.working
      },
      controller: DemoAccountDialogController
    });
    function DemoAccountDialogController($scope, $mdDialog, siteService, stateData, SIGNIN_INFO, APP_ROLE_TRANSLATION) {
      $scope.signinData = SIGNIN_INFO;
      $scope.APP_ROLE_TRANSLATION = APP_ROLE_TRANSLATION;

      if ($scope.signinData && $scope.signinData.demoTenants) {
        $scope.demoTenant = $scope.signinData.demoTenants[0];
      }

      $scope.working = false;
      $scope.loginAsDemoUser = function (user) {
        $mdDialog.hide();
        utilityService.signIn(user)
        .then(function () {
          $state.go('app.spa-landing', {}, { reload: true });
        });
      };
      $scope.cancelDialog = function () {
        $log.debug("You canceled the dialog.");
        $mdDialog.hide();
      };

      $scope.createDemoUser = function (appRole) {
        $scope.working = true;

        utilityService.createDemoUser(appRole)
        .then(function (newUserCred) {
          // hmm. Not sure what to call newUserCred. It contains the user's email - which on a demo user is enough to get us signed in! Score
          utilityService.signIn(newUserCred)
          .then(function () {
            // success
            $state.go('app.spa-landing', {}, { reload: true });
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
    }
  };

  $scope.showForgotPasswordDialog = function ($event) {
    var parentEl = angular.element(document.body);
    $mdDialog.show({
      parent: parentEl,
      targetEvent: $event,
      templateUrl: '/client/states/public/forgot-password.dialog.html',
      controller: ForgotPasswordDialogController
    });
    function ForgotPasswordDialogController($scope, webUtilityService) {

      $scope.cancel = function () {
        $log.debug("You canceled the dialog.");
        $mdDialog.hide();
      };

      //!! Ok - there must be a way better way than passing fnHide as a parameter here. I'm missing something really basic.
      $scope.sendPasswordResetEmail = function (email, fnHide) {
        webUtilityService.sendResetPasswordEmail(email)
          .then(function (successData) {
            $mdDialog.hide();
            $mdToast.showSimple("Password reset email sent to " + email);

            //var myAlert = $alert({ title: 'Password Reset', content: 'Password reset email sent to ' + email + '.', placement: 'top', type: 'info', show: true });
            //fnHide();
          }, function (failureData) {
            $mdDialog.hide();
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
    }
  };

  $scope.showDemoToolsDialog = function ($event) {
    var parentEl = angular.element(document.body);
    $mdDialog.show({
      parent: parentEl,
      targetEvent: $event,
      templateUrl: '/client/app/public/sign-in-demo-tools.dialog.html',
      resolve: {
        demoTenants: function (utilityService) {
          return utilityService.getDemoTenants();
        }
      },
      locals: {
        stateData: $scope.stateData,
        working: $scope.working
      },
      controller: DemoToolsDialogController
    });
    function DemoToolsDialogController($scope, $mdDialog, $filter, siteService, stateData, demoTenants, ROLES) {
      $scope.demoTenants = demoTenants;
      $scope.working = false;
      $scope.ROLES = ROLES;
      if ($scope.demoTenants) {
        var index = $scope.demoTenants.length - 1;
        $scope.demoTenant = $scope.demoTenants[index];
      }
      $scope.setDemoTenant = function(selectedTenantID) {
        for (var i = 0; i < $scope.demoTenants.length; i++) {
          if ($scope.demoTenants[i].id === selectedTenantID) {
            $scope.demoTenant = $scope.demoTenants[i];
          }
        }
      };
      $scope.signInAsDemoUser = function (user) {
        $scope.working = true;
        utilityService.signIn(user)
        .then(function () {
          $state.go('app.user-signin', {}, { reload: true });
        });
      };
      $scope.cancelDialog = function () {
        $log.debug("You canceled the dialog.");
        $mdDialog.hide();
      };
      $scope.createDemoTenant = function () {
        $scope.working = true;
        // delegate to siteService if we can, in case it wants to create an ExtendedTenant
        var createDemoTenantHandler = siteService.createDemoTenant || utilityService.createDemoTenant;
        //!! no need, as we're calling "Demo" method var data = { isDemo: true };
        var data = { };
        createDemoTenantHandler($scope.newTenantName, data)
        .then(utilityService.getDemoTenants)
        .then(function(demoTenants) {
            $scope.demoTenants = demoTenants;
            $scope.demoTenant = $filter('filter')(demoTenants, { name: $scope.newTenantName })[0];
        })
        .catch(function (failureData) {
          $log.warn(failureData.errorMessage, failureData);
        })
        .finally(function () {
          $scope.showNewDemoTenantSettings = false;
          $scope.working = false;
        });
      };
      $scope.createDemoUser = function (appRole) {
        $scope.working = true;
        // delegate to siteService if we can, in case it wants to create an ExtendedUser
        var createDemoUserHandler = siteService.createDemoUser || utilityService.createDemoUser;
        createDemoUserHandler($scope.demoTenant, appRole)
        .then(utilityService.signIn)
        .then(function () {
          // $state.go('app.job-seeker.onboard.intro', {}, { reload: true });
          $state.go('app.user-created', {}, { reload: true });
        })
        .catch(function (failureData) {
          $log.warn(failureData.errorMessage, failureData);
        })
        .finally(function () {
          // $mdDialog.hide();
        });
      };
      $scope.createNewDemoCounselor = function () {
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
          // $mdDialog.hide();
        });
      };
    }
  };

});
