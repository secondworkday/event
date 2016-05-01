(function () {
  'use strict';

  angular
    .module('myApp')
    .controller('NewUserDialogController', NewUserDialogController);

  NewUserDialogController.$inject = ['$scope', '$filter', '$log', '$mdDialog', '$msUI', 'utilityService', 'APP_ROLE_ITEMS', 'APP_ROLE_TRANSLATION'];

  /* @ngInject */
  function NewUserDialogController($scope, $filter, $log, $mdDialog, $msUI, utilityService, APP_ROLE_ITEMS, APP_ROLE_TRANSLATION) {
    $log.debug('Loading NewUserDialogController...');


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
        $msUI.showToast("User Created");
        $mdDialog.hide(successData);
      })
      .catch(function (failureData) {
        $msUI.showToast(failureData.errorMessage);
        $mdDialog.cancel(failureData);
      });
    };
    
    $scope.inviteUser = function (formData) {
      //!! TODO make this come alive
      $log.debug('Trying to invite new user: ', formData);
      $mdDialog.hide();
    };    

    $scope.cancelDialog = $mdDialog.cancel;
  }
})();
