(function() {
    'use strict';

    angular
        .module('myApp')
        .controller('UserSettingsController', UserSettingsController);

    UserSettingsController.$inject = ['$scope', '$log', '$state', '$rootScope', '$mdDialog', '$mdToast', 'FileUploader', 'siteService', 'utilityService', 'user'];

    /* @ngInject */
    function UserSettingsController($scope, $log, $state, $rootScope, $mdDialog, $mdToast, FileUploader, siteService, utilityService, user) {
        $log.debug("Loading UserSettingsController...");

        $scope.sampleActivities = [
          {
            title: "Login",
            category: "Access",
            IPAddress: "50.142.8.188",
            timestamp: "2 hours ago"
          },
          {
            title: "Logout",
            category: "Access",
            IPAddress: "50.142.8.188",
            timestamp: "5 days ago"
          },
          {
            title: "Removed Job Opportunity from Plan",
            category: "Career Step",
            IPAddress: "50.142.8.188",
            timestamp: "5 days ago"
          },
          {
            title: "Clicked Apply in Job Opportunity",
            category: "Career Step",
            IPAddress: "50.142.8.188",
            timestamp: "5 days ago"
          },
          {
            title: "Saved Job Opportunity to Plan",
            category: "Career Step",
            IPAddress: "50.142.8.188",
            timestamp: "5 days ago"
          },
          {
            title: "Saved Job Opportunity to Plan",
            category: "Career Step",
            IPAddress: "50.142.8.188",
            timestamp: "5 days ago"
          },
          {
            title: "Added Occupation to Shortlist",
            category: "Career Step",
            IPAddress: "50.142.8.188",
            timestamp: "5 days ago"
          },
          {
            title: "Added Work Period to Profile",
            category: "Career Profile",
            IPAddress: "50.142.8.188",
            timestamp: "5 days ago"
          },
          {
            title: "Login",
            category: "Access",
            IPAddress: "50.142.8.188",
            timestamp: "5 days ago"
          }
        ];

        $scope.sampleRoles = [
          {
            name: "Job Seeker",
            enabled: true
          },
          {
            name: "Counselor",
            enabled: true
          },
          {
            name: "TenantGroup Admin",
            enabled: false
          },
          {
            name: "Database Admin",
            enabled: false
          },
          {
            name: "Ops Admin",
            enabled: false
          },
          {
            name: "System Admin",
            enabled: false
          },
          {
            name: "Security Admin",
            enabled: false
          }
        ];

        //!! hmm - consider - this would be a good place to register for detailed User notifications

        // stash our user - now available for child states
        $scope.user = user;

        // store the previous state so the back button works no matter where you came from
        $scope.back = $rootScope.previousState;

        $scope.impersonateUser = function (user) {
          return utilityService.impersonateUser(user)
              .then(function (successData) {
                // !! $state.go('master.admin.settings');

                $state.go('app.user.events', {}, { reload: true });


                return true;
              }, function (failureData) {
                return failureData.StatusCodeDescription;
              });
        };

        $scope.deleteUser = function(user) {
          utilityService.deleteUser(user);
          $state.go($scope.back);
        };

        $scope.uploader = new FileUploader({
          url: '/upload.ashx',
          autoUpload: true,
          removeAfterUpload: true,
          formData: [{ type: 'userProfilePhoto' }]
        });

        $scope.setProfilePhotoID = function (profilePhotoID) {
          //!! I'm not understanding something here - this function is called twice, with the second time homeID always being 1. So only remember the first one
          if (!$scope.uploader.profilePhotoID) {
            $scope.uploader.profilePhotoID = profilePhotoID;
          }
        };

        $scope.uploader.onAfterAddingFile = function (fileItem) {
          console.info('onAfterAddingFile', fileItem);
          fileItem.formData.push({ id: $scope.uploader.profilePhotoID });
          //!! I'm not understanding something here - clear our homeID value so we remember the next one correctly
          $scope.uploader.profilePhotoID = undefined;
        };
        
        $scope.showSetPasswordDialog = function ($event) {
          var parentEl = angular.element(document.body);
          $mdDialog.show({
            parent: parentEl,
            targetEvent: $event,
            templateUrl: '/client/app/system/site-admin/user-settings-set-password.dialog.html',
            locals: {
              user: $scope.user
            },
            controller: SetPasswordDialogController
          });
          function SetPasswordDialogController($scope, $mdDialog, $mdToast, utilityService, user) {

            $scope.user = user;

            $scope.setPassword = function (formData) {
              //!! TODO make this come alive
              
              utilityService.setUserPassword(user, formData.password)
              .then(function () {
                $mdDialog.hide();
                $mdToast.show($mdToast.simple().textContent('Password successfully set for ' + user.displayName));       
              })
              .catch(function (failureData) {
                $log.warn(failureData.errorMessage, failureData);
                $scope.error = failureData.errorMessage;
              })
              .finally(function () {
                // nothing here?
              });
            };
                     
            
            $scope.cancelDialog = function () {
              $log.debug("You canceled the dialog.");
              $mdDialog.hide();
            };
          }
        };        



    }
})();
