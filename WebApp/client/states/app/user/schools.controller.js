app.controller('SchoolsController', function ($scope, $mdDialog, $log, $msUI, utilityService, siteService, eventParticipantGroupsIndex) {
  $log.debug('Loading SchoolsController...');

  $scope.searchHandler = siteService.model.participantGroups.search;
  $scope.demandParticipantGroup = siteService.demandParticipantGroup;
  $scope.demandEventSession = siteService.demandEventSession;



  $scope.sortOptions = [
    { name: 'Name', serverTerm: 'item.Name', clientFunction: utilityService.localeCompareByPropertyThenByID('name') },
    { name: 'Name Descending', serverTerm: 'item.Name DESC', clientFunction: utilityService.localeCompareByPropertyThenByIDDescending('name') }
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

});
