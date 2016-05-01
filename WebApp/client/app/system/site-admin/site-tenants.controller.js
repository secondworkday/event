(function() {
    'use strict';

    angular
        .module('myApp')
        .controller('SiteTenantsController', SiteTenantsController);

    SiteTenantsController.$inject = ['$scope', '$mdDialog', '$log', '$msUI', 'utilityService', 'siteService'];

    /* @ngInject */
    function SiteTenantsController($scope, $mdDialog, $log, $msUI, utilityService, siteService) {
      $log.debug('Loading SiteTenantsController...');

      $scope.searchTenants = utilityService.model.tenantGroups.search;


      $scope.sortOptions = [
        { name: 'Name', serverTerm: 'item.name', clientFunction: utilityService.compareByProperties('name', 'id') },
        { name: 'Name Descending', serverTerm: 'item.name DESC', clientFunction: utilityService.compareByProperties('-name', '-id') }
      ];

      var filterByStateFactory = function (includeState) {
        var includeStateLocal = includeState;
        return function (item) {
          return item.state === includeStateLocal;
        };
      };

      $scope.filterOptions = [
        // '^' means items with null parents, aka topmost items
        { name: 'Tenants', serverTerm: '^', clientFunction: utilityService.filterByPropertyHasValue("!parentID") }

        //{ name: 'Active', serverTerm: '$Active', clientFunction: filterByStateFactory("Active") },
        //{ name: 'Disabled', serverTerm: '$Disabled', clientFunction: filterByStateFactory("Disabled") },

      ];

      $scope.searchViewOptions = {
        sort: $scope.sortOptions[0],
        selectFilter: $scope.filterOptions[0],
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
           templateUrl: '/client/app/system/site-admin/new-tenant.dialog.html',
           controller: NewTenantDialogController
        });
        function NewTenantDialogController($scope, $mdDialog) {
          $scope.createTenant = function (newTenantName) {


            // delegate to siteService if we can, in case it wants to create an ExtendedTenant
            var createTenantHandler = siteService.createTenant || utilityService.createTenant;
            var data = {};
            createTenantHandler(newTenantName, data)
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
    }
})();
