app.controller('EventSessionsController', function ($scope, $mdDialog, $log, $msUI, utilityService, siteService) {
  $log.debug('Loading EventSessionsController...');

  $scope.searchHandler = siteService.model.eventSessions.search;
  $scope.demandParticipantGroup = siteService.demandParticipantGroup;
  $scope.demandEventSession = siteService.demandEventSession;



  $scope.sortOptions = [
    { name: 'Session Date', serverTerm: 'item.StartDate', clientFunction: utilityService.localeCompareByPropertyThenByID('startDate') },
    { name: 'Session Date Descending', serverTerm: 'item.StartDate DESC', clientFunction: utilityService.localeCompareByPropertyThenByIDDescending('startDate') },
    { name: 'Session Name', serverTerm: 'item.Name', clientFunction: utilityService.localeCompareByPropertyThenByID('name') },
    { name: 'Session Name Descending', serverTerm: 'item.Name DESC', clientFunction: utilityService.localeCompareByPropertyThenByIDDescending('name') }
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
    { name: 'All', serverTerm: '$event:Nnn' }
  ];

  //!! fixup our event filter. Need a more robust way to handle this
  $scope.filterOptions[0].serverTerm = '$event:' + $scope.event.id;

  $scope.searchViewOptions = {
    sort: $scope.sortOptions[0],
    filter: $scope.filterOptions[0]
  };

  $scope.setEventSessionState = function (eventSession, stateName) {
    siteService.setEventSessionState(eventSession, stateName);
  };

  $scope.download = function () {
    var query = {
      type: 'tenants'
    };
    utilityService.download(query);
  };
});
