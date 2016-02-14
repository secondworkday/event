app.controller('SystemTenantAccountController', function ($scope, $log, utilityService, tenant) {
  $log.debug('Loading SystemTenantAccountController...');

  $scope.tenant = tenant;


  // This is our selected tenant, set below after we ensure we have all its data loaded
  $scope.tenant = tenant;

  $scope.deleteTenant = function (tenant) {
    utilityService.deleteTenant(tenant.id)
        .then(function (successData) {
          $scope.tenant = null;
        }, function (failureData) {
          alert(failureData.errorMessage);
        });
  }

  $scope.updateTenantName = function (tenantName) {
    return utilityService.editTenant($scope.tenant.id, { name: tenantName })
        .then(function (successData) {
          return true;
        }, function (failureData) {
          return failureData.StatusCodeDescription;
        });
  };

  $scope.impersonateSystem = function (tenant) {
    return utilityService.impersonateSystem(tenant)
        .then(function (successData) {
          //!! switch to the TenantAdmin configure page ...
          $state.go('master.admin.settings');
          return true;
        }, function (failureData) {
          return failureData.StatusCodeDescription;
        });
  };

  $scope.modifyAccountOption = function (tenant, optionName, isAssigned) {
    utilityService.modifyTenantGroupAccountOption(tenant, optionName, isAssigned)
        .then(function (successData) {
        }, function (failureData) {
          alert(failureData.errorMessage);
        });
  }


  $scope.unauthorizeTenantAdmin = function (user) {
    return utilityService.modifyUserSystemRole(user, "TenantAdmin", false);
  };


  // init

  //!! what to grab?
  //utilityService.getUsers();
  //utilityService.getGroups($scope.tenant);


});
