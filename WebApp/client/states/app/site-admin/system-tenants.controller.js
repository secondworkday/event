app.controller('SystemTenantsController', function ($scope, $mdDialog, $log, utilityService, siteService) {
  $log.debug('Loading SystemTenantsController...');



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
    //{ name: 'Active', serverTerm: '$Active', clientFunction: filterByStateFactory("Active") },
    //{ name: 'Disabled', serverTerm: '$Disabled', clientFunction: filterByStateFactory("Disabled") },
    { name: 'All' }
  ];

  $scope.searchViewOptions = {
    sort: $scope.sortOptions[0],
    filter: $scope.filterOptions[0]
  };


  $scope.searchTenants = function (searchExpression, sortExpression, startIndex, rowCount) {
    return utilityService.searchTenants(searchExpression, sortExpression, startIndex, rowCount);
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
      $scope.createTenant = function(newTenantName) {
        // TODO add create Tenant code
        $log.warn( "You tried to create a Tenant called " + newTenantName + ", but tenant creation is not yet working." );
        $mdDialog.hide();
      };
      $scope.cancelDialog = function() {
        $log.debug( "You canceled the dialog." );
        $mdDialog.hide();
      };
    }
  };

});