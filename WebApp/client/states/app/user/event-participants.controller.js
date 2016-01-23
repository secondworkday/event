app.controller('EventParticipantsController', function ($scope, $mdDialog, $log, $msUI, utilityService, siteService) {
  $log.debug('Loading EventParticipantsController...');

  $scope.searchHandler = siteService.model.eventParticipants.search;
  $scope.demandParticipantGroup = siteService.demandParticipantGroup;
  $scope.demandEventSession = siteService.demandEventSession;



  $scope.sortOptions = [
    { name: 'First Name', serverTerm: 'Participant.FirstName', clientFunction: utilityService.localeCompareByPropertyThenByID('firstName') },
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
