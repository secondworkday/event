app.controller('SystemTenantsController', function ($scope, $mdDialog, $log, $msUI, utilityService, siteService) {
  $log.debug('Loading SystemTenantsController...');

  $scope.searchTenants = utilityService.searchTenants;


  $scope.sortOptions = [
    { name: 'Name', serverTerm: 'item.name', clientFunction: utilityService.localeCompareByPropertyThenByID('name') },
    { name: 'Name Descending', serverTerm: 'item.name DESC', clientFunction: utilityService.localeCompareByPropertyThenByIDDescending('name') }
  ];

  var filterByStateFactory = function (includeState) {
    var includeStateLocal = includeState;
    return function (item) {
      return item.state === includeStateLocal;
    };
  };

  $scope.filterOptions = [
    // We're calling utilityService.searchTenants, so that serves the function of the serverTerm parameter to fetch only top-level TenantGroups
    { name: 'Tenants', clientFunction: utilityService.filterByPropertyHasValue("!parentID") }

    //{ name: 'Active', serverTerm: '$Active', clientFunction: filterByStateFactory("Active") },
    //{ name: 'Disabled', serverTerm: '$Disabled', clientFunction: filterByStateFactory("Disabled") },

  ];

  $scope.searchViewOptions = {
    sort: $scope.sortOptions[0],
    filter: $scope.filterOptions[0],
    userSearch: null
  };


  $scope.download = function () {
    var query = {
      type: 'tenants'
    };
    utilityService.download(query);
  };





  $scope.showNewTenantDialog = function ($event) {
     var parentEl = angular.element(document.body);
     $mdDialog.show({
       parent: parentEl,
       targetEvent: $event,
       templateUrl: '/client/states/app/site-admin/system-new-tenant.dialog.html',
       controller: NewTenantDialogController
    });
    function NewTenantDialogController($scope, $mdDialog) {
      $scope.createTenant = function (newTenantName) {

        var data = {};
        utilityService.createTenant(newTenantName, data)
        .then(function (successData) {
          // success
          $msUI.showToast("Tenant Created");
          $log.debug("Task completed.");
          return successData;
        }, function (failureData) {
          // failure
          $msUI.showToast(failureData.errorMessage);
          $log.debug(failureData.errorMessage);
          return failureData;
        });

        $mdDialog.hide();
      };
      $scope.cancelDialog = function() {
        $log.debug( "You canceled the dialog." );
        $mdDialog.hide();
      };
    }
  };

});
