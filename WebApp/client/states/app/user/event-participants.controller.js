app.controller('EventParticipantsController', function ($scope, $mdDialog, $log, $msUI, utilityService, siteService) {
  $log.debug('Loading EventParticipantsController...');

  $scope.searchHandler = siteService.model.eventParticipants.search;
  $scope.demandParticipantGroup = siteService.demandParticipantGroup;
  $scope.demandEventSession = siteService.demandEventSession;



  $scope.sortOptions = [
    { name: 'First Name', serverTerm: 'Participant.FirstName', clientFunction: utilityService.localeCompareByPropertyThenByID('firstName') },
    { name: 'Last Name', serverTerm: 'Participant.LastName', clientFunction: utilityService.localeCompareByPropertyThenByID('lastName') },
    { name: 'School', serverTerm: 'Participant.ParticipantGroup.Name', clientFunction: utilityService.localeCompareByPropertyThenByID('participantGroupID') },
    { name: 'Grade', serverTerm: 'ExEventParticipant.item.Grade', clientFunction: utilityService.localeCompareByPropertyThenByID('grade') }
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
