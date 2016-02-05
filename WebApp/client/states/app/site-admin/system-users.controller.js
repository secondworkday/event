app.controller('SystemUsersController', function ($scope, $mdDialog, $log, FileUploader, $msUI, utilityService, siteService) {
  $log.debug('Loading SystemUsersController...');

  $scope.searchUsers = utilityService.model.users.search;

  $scope.sortOptions = [
    { name: 'First Name', serverTerm: 'item.firstName', clientFunction: utilityService.localeCompareByPropertyThenByID('firstName') },
    { name: 'First Name Descending', serverTerm: 'item.firstName DESC', clientFunction: utilityService.localeCompareByPropertyThenByIDDescending('firstName') },
    { name: 'Last Name', serverTerm: 'item.lastName', clientFunction: utilityService.localeCompareByPropertyThenByID('lastName') },
    { name: 'Last Name Descending', serverTerm: 'item.lastName DESC', clientFunction: utilityService.localeCompareByPropertyThenByIDDescending('lastName') },
    { name: 'Email', serverTerm: 'item.email', clientFunction: utilityService.localeCompareByPropertyThenByID('email') }
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
    filter: $scope.filterOptions[0],
    userSearch: null
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







  $scope.download = function () {
    var query = {
      type: 'users'
    };
    utilityService.download(query);
  };








  $scope.showNewSystemUserDialog = function ($event) {
    var parentEl = angular.element(document.body);
    $mdDialog.show({
      parent: parentEl,
      targetEvent: $event,
      templateUrl: '/client/states/app/site-admin/system-new-user.dialog.html',
      controller: NewSystemUserDialogController
    });
    function NewSystemUserDialogController($scope, $mdDialog) {

      $scope.tenantGroups = utilityService.model.tenantGroups;

      // Kick off a search to load up our TenantGroup cache
      $scope.tenantGroups.search("", "", 0, 9999);


      $scope.createUser = function () {
        utilityService.createUser($scope.formData)
        .then(function (successData) {
          // success
          $msUI.showToast("User Created");
          $log.debug("User created.");
          return successData;
        }, function (failureData) {
          // failure
          $msUI.showToast(failureData.errorMessage);
          $log.debug(failureData.errorMessage);
          return failureData;
        });
        $mdDialog.hide();
      };
      $scope.cancelDialog = function () {
        $log.debug("You canceled the dialog.");
        $mdDialog.hide();
      };
    }
  };

  $scope.showSystemUserDialog = function ($event) {
     var parentEl = angular.element(document.body);
     $mdDialog.show({
       parent: parentEl,
       targetEvent: $event,
       templateUrl: '/client/states/app/site-admin/system-user.dialog.html',
       controller: SystemUserDialogController
    });
     function SystemUserDialogController($scope, $mdDialog, $msUI) {
      $scope.save = function(formData) {
        // TODO add save User code
        $log.warn("You tried to save:");
        $log.warn(formData);
        $mdDialog.hide();
      };
      $scope.cancelDialog = function() {
        $log.debug( "You canceled the dialog." );
        $mdDialog.hide();
      };
    }
  };



});
