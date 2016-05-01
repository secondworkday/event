(function() {
    'use strict';

    angular
        .module('myApp')
        .controller('SiteUsersController', SiteUsersController);

    SiteUsersController.$inject = ['$scope', '$state', '$mdDialog', '$log', 'FileUploader', '$msUI', 'utilityService', 'siteService', 'tenant'];

    /* @ngInject */
    function SiteUsersController($scope, $state, $mdDialog, $log, FileUploader, $msUI, utilityService, siteService, tenant) {
      $log.debug('Loading SiteUsersController...');

      $scope.tenant = tenant;

      $scope.searchUsers = utilityService.model.users.search;

      $scope.sortOptions = [
        { name: 'First Name', serverTerm: 'item.firstName', clientFunction: utilityService.compareByProperties('firstName', 'id') },
        { name: 'First Name Descending', serverTerm: 'item.firstName DESC', clientFunction: utilityService.compareByProperties('firstName', '-id') },
        { name: 'Last Name', serverTerm: 'item.lastName', clientFunction: utilityService.compareByProperties('lastName', 'id') },
        { name: 'Last Name Descending', serverTerm: 'item.lastName DESC', clientFunction: utilityService.compareByProperties('lastName', '-id') },
        { name: 'Email', serverTerm: 'item.email', clientFunction: utilityService.compareByProperties('email', 'id') }
      ];


      var filterByStateFactory = function (includeState) {
        var includeStateLocal = includeState;
        return function (item) {
          return item.state === includeStateLocal;
        };
      };

      $scope.filterOptions = [
        { name: 'Active', serverTerm: '$Active', clientFunction: filterByStateFactory("Active") },
        { name: 'Disabled', serverTerm: '$Disabled', clientFunction: filterByStateFactory("Disabled") },
        { name: 'All' }
      ];

      $scope.searchViewOptions = {
        sort: $scope.sortOptions[2],
        selectFilter: $scope.filterOptions[0],
        userSearch: null
      };


      // Establish our Base filtering (evaluatuating in order of most restrictive to least restrictive)
      if ($scope.tenant) {
        // filter to one EventSession
        $scope.searchViewOptions.baseFilter = { serverTerm: '$tid:' + $scope.tenant.id, clientFunction: utilityService.filterByPropertyValue('tenantGroupID', $scope.tenant.id) };
      } else {
        // no filtering
      }



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





  $scope.deleteUser = function (user) {
    //$log.debug("Deleted user " + user.email);
    utilityService.deleteUser(user)
        .then(function (successData) {
          // success
          $msUI.showToast("User Deleted");
          $log.debug("User Deleted.");
          return successData;
        }, function (failureData) {
          // failure
          $msUI.showToast(failureData.errorMessage);
          $log.debug(failureData.errorMessage);
          return failureData;
        });
  };

      $scope.showDeleteConfirmationDialog = function (ev, user) {
        var confirm = $mdDialog.confirm()
          .title("Delete User")
          .textContent("Would you like to delete user '" + user.firstName + " " + user.lastName + "'?")
          .ariaLabel("Delete user")
          .targetEvent(ev)
          .ok("yes")
          .cancel("no");

        $mdDialog.show(confirm).then(function () {
          $scope.deleteUser(user);
        });
      };

      $scope.download = function () {
        var query = {
          type: 'users'
        };
        utilityService.download(query);
      };


      $scope.showNewUserDialog = function ($event) {
        $msUI.showModalDialog({
          targetEvent: $event,
          templateUrl: '/client/app/system/new-user.dialog.html',
          controller: 'NewUserDialogController'
        })
        .then(function (result) {
          //!! how do we select/highlight this fella?
          console.log(result);
        });
      };


      $scope.showInviteUserDialog = function ($event) {
        var parentEl = angular.element(document.body);
        $mdDialog.show({
          parent: parentEl,
          targetEvent: $event,
          templateUrl: '/client/app/system/new-user-invite.dialog.html',
          controller: 'NewUserDialogController'
        });
      };        


    }
})();
