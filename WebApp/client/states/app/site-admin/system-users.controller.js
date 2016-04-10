app.controller('SystemUsersController', function ($scope, $state, $mdDialog, $log, FileUploader, $msUI, utilityService, siteService, tenant) {
  $log.debug('Loading SystemUsersController...');

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








  $scope.showNewSystemUserDialog = function ($event) {
    var parentEl = angular.element(document.body);
    $mdDialog.show({
      parent: parentEl,
      targetEvent: $event,
      templateUrl: '/client/states/app/site-admin/system-new-user.dialog.html',
      controller: NewSystemUserDialogController
    });
    function NewSystemUserDialogController($scope, $filter, $mdDialog, utilityService, APP_ROLE_ITEMS, APP_ROLE_TRANSLATION) {

      $scope.APP_ROLE_TRANSLATION = APP_ROLE_TRANSLATION;

      var model = utilityService.model;
      var authenticatedIdentity = model.authenticatedIdentity;
      $scope.authenticatedUser = model.authenticatedUser;
      $scope.authenticatedIdentity = authenticatedIdentity;

      $scope.tenantGroups = utilityService.model.tenantGroups;

      //!! if we're a SystemAdmin, we allow the selection of a tenantGroupID, unless one is specified.

      //!! if we're a SystemAdmin or TenantAdmin, we allow

      $scope.formData = {
        appRoles: [],
        systemRoles: []
      };


      // AppRoles and SystemRoles

      $scope.appRoleItems = APP_ROLE_ITEMS;

      var systemRoleSourceItems = [
          { name: 'Security Admin', value: 'SecurityAdmin', visible: ["SecurityAdmin", "OperationsAdmin", "SystemAdmin"], enabled: ["SecurityAdmin"] },
          { name: 'Operations Admin', value: 'OperationsAdmin', visible: ["SecurityAdmin", "OperationsAdmin", "DatabaseAdmin", "SystemAdmin"], enabled: ["SecurityAdmin"] },
          { name: 'Database Admin', value: 'DatabaseAdmin', visible: ["SecurityAdmin", "OperationsAdmin", "DatabaseAdmin", "SystemAdmin"], enabled: ["OperationsAdmin"] },
          { name: 'System Admin', value: 'SystemAdmin', visible: ["SecurityAdmin", "OperationsAdmin", "SystemAdmin"], enabled: ["SecurityAdmin"] },
          // Known by SystemAdmins as a 'Tenant Admin', known by TenantAdmins as 'Admin' handled elsewhere
          { name: 'Tenant Admin', value: 'TenantAdmin', visible: ["SystemAdmin"], enabled: ["SystemAdmin"] },
      ];

      $scope.systemRoleItems = [];
      $.each(systemRoleSourceItems, function (index, systemRoleSourceItem) {
        var isVisible = $filter('intersect')(authenticatedIdentity.roles, systemRoleSourceItem.visible).length > 0;
        if (isVisible) {
          var isEnabled = $filter('intersect')(authenticatedIdentity.roles, systemRoleSourceItem.enabled).length > 0;
          $scope.systemRoleItems.push({ name: systemRoleSourceItem.name, value: systemRoleSourceItem.value, enabled: isEnabled });
        }
      });

      $scope.toggleList = function (list, item) {
        var idx = list.indexOf(item);
        if (idx > -1) {
          // toggle off...
          list.splice(idx, 1);
        }
        else {
          // toggle on...
          list.push(item);
        }
      };
      $scope.listContains = function (list, item) {
        return list.indexOf(item) > -1;
      };









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

  $scope.showEditUserDialog = function ($event, user) {
    var parentEl = angular.element(document.body);
    $mdDialog.show({
      parent: parentEl,
      targetEvent: $event,
      templateUrl: '/client/states/app/user/edit-user.dialog.html',
      locals: {
        user: user,
        newOrEdit: "Edit"
      },
      controller: EditUserDialogController
    });
    function EditUserDialogController($scope, $filter, $translate, utilityService, APP_ROLE_ITEMS, APP_ROLE_TRANSLATION, user, newOrEdit) {
      if (newOrEdit == "Edit") {
        $scope.user = user;
      }

      $scope.APP_ROLE_TRANSLATION = APP_ROLE_TRANSLATION;

      var model = utilityService.model;
      var authenticatedIdentity = model.authenticatedIdentity;
      $scope.authenticatedUser = model.authenticatedUser;
      $scope.authenticatedIdentity = authenticatedIdentity;

      $scope.tenantGroups = utilityService.model.tenantGroups;


      // AppRoles and SystemRoles

      $scope.appRoleItems = APP_ROLE_ITEMS;
      $.each($scope.appRoleItems, function (index, roleItem) {
        $scope.appRoleItems[index].checked = userHasRole($scope.user.appRoles, roleItem.value);
      });

      var systemRoleSourceItems = [
          { name: 'Security Admin', value: 'SecurityAdmin', visible: ["SecurityAdmin", "OperationsAdmin", "SystemAdmin"], enabled: ["SecurityAdmin"] },
          { name: 'Operations Admin', value: 'OperationsAdmin', visible: ["SecurityAdmin", "OperationsAdmin", "DatabaseAdmin", "SystemAdmin"], enabled: ["SecurityAdmin"] },
          { name: 'Database Admin', value: 'DatabaseAdmin', visible: ["SecurityAdmin", "OperationsAdmin", "DatabaseAdmin", "SystemAdmin"], enabled: ["OperationsAdmin"] },
          { name: 'System Admin', value: 'SystemAdmin', visible: ["SecurityAdmin", "OperationsAdmin", "SystemAdmin"], enabled: ["SecurityAdmin"] },
          // Known by SystemAdmins as a 'Tenant Admin', known by TenantAdmins as 'Admin' handled elsewhere
          { name: 'Tenant Admin', value: 'TenantAdmin', visible: ["SystemAdmin"], enabled: ["SystemAdmin"] },
      ];

      $scope.systemRoleItems = [];
      $.each(systemRoleSourceItems, function (index, systemRoleSourceItem) {
        var isVisible = $filter('intersect')(authenticatedIdentity.roles, systemRoleSourceItem.visible).length > 0;
        if (isVisible) {
          var isEnabled = $filter('intersect')(authenticatedIdentity.roles, systemRoleSourceItem.enabled).length > 0;
          $scope.systemRoleItems.push({
            name: systemRoleSourceItem.name, value: systemRoleSourceItem.value, enabled: isEnabled, checked: userHasRole($scope.user.systemRoles, systemRoleSourceItem.value)
          });
        }
      });

      $scope.updateUserRole = function (userRoles, role) {
        // add
        if (role.checked) {
          if (userRoles.indexOf(role.value) == -1) {
            userRoles.push(role.value);
          }
        }
          // remove from array
        else {
          var index = userRoles.indexOf(role.value);
          if (index >= 0) {
            userRoles.splice(index, 1);
          }
        }
      };

      function userHasRole(roleScope, roleName) {
        for (i = 0; i < roleScope.length; i++) {
          if (roleScope[i] == roleName) {
            return true;
          }
        }
        return false;
      };

      $scope.cancel = function () {
        $log.debug("You canceled the dialog.");
        $mdDialog.hide();
      };
      //$scope.deleteUser = function () {
      //  $log.debug("You deleted the user.");
      //  //!! TODO add function to actually delete the user
      //  $mdDialog.hide();
      //};
      $scope.apply = function () {
        utilityService.editUser(user.id, user)
        .then(function (successData) {
          // success
          $msUI.showToast("User Updated");
          $log.debug("User Updated.");
          return successData;
        }, function (failureData) {
          // failure
          $msUI.showToast(failureData.errorMessage);
          $log.debug(failureData.errorMessage);
          return failureData;
        });

        $mdDialog.hide();
      };
    }
  };



});
