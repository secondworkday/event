app.controller('ParticipantGroupsController', function ($scope, $mdDialog, $log, $msUI, $translate, utilityService, siteService, eventParticipantGroupsIndex) {
  $log.debug('Loading ParticipantGroupsController...');

  $scope.searchHandler = siteService.model.participantGroups.search;
  $scope.demandParticipantGroup = siteService.demandParticipantGroup;
  $scope.demandEventSession = siteService.demandEventSession;

  var PARTICIPANT_GROUP = "Participant Group";
  $translate('PARTICIPANT_GROUP').then(function (participantGroupText) {
    PARTICIPANT_GROUP = participantGroupText;
  })
    // sort UI depends on $translate service
    .then(function () { 
      $scope.sortOptions = [
        { name: PARTICIPANT_GROUP + ' Name', serverTerm: 'item.Name', clientFunction: utilityService.compareByProperties('name', 'id') },
        { name: PARTICIPANT_GROUP + ' Name Descending', serverTerm: 'item.Name DESC', clientFunction: utilityService.compareByProperties('name', 'id') },
        { name: 'Contact Name', serverTerm: 'item.ContactName', clientFunction: utilityService.compareByProperties('contactName', 'id') },
        { name: 'Contact Name Descending', serverTerm: 'item.ContactName DESC', clientFunction: utilityService.compareByProperties('contactName', 'id') }
      ];
    });

  

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
    selectFilter: $scope.filterOptions[0]
  };


  $scope.showUploadParticipantGroupsDialog = function (ev, event) {
    $mdDialog.show({
      controller: 'UploadParticipantGroupsDialogController',
      templateUrl: '/client/states/app/user/upload-participant-groups.dialog.html',
      locals: {
        event: event
      },
      parent: angular.element(document.body),
      targetEvent: ev,
      clickOutsideToClose: true,
      fullscreen: false
    })
    .then(function () {
      // $scope.status = 'You said the information was "' + answer + '".';
    }, function () {
      // $scope.status = 'You cancelled the dialog.';
    });
  }

  $scope.showAddParticipantsDialog = function (ev, event) {
    $mdDialog.show({
      controller: AddParticipantsDialogController,
      templateUrl: '/client/states/app/user/add-participants.dialog.html',
      locals: {
        event: event
      },
      parent: angular.element(document.body),
      targetEvent: ev,
      clickOutsideToClose: true,
      fullscreen: false
    })
    .then(function () {
      // $scope.status = 'You said the information was "' + answer + '".';
    }, function () {
      // $scope.status = 'You cancelled the dialog.';
    });
  }
  function AddParticipantsDialogController($scope, $mdDialog, event) {

    $scope.participantGroups = siteService.model.participantGroups;
    $scope.event = event;

    $scope.hide = function () {
      $mdDialog.hide();
    };
    $scope.cancel = function () {
      $mdDialog.cancel();
    };

    $scope.generateRandomParticipants = function (participantGroupID, numberOfParticipants) {
      siteService.generateRandomParticipants(participantGroupID, numberOfParticipants);
      $mdDialog.hide();
    };
    $scope.createParticipant = function (formData) {
      siteService.createParticipant(formData);
      $mdDialog.hide();
    }
  }





  $scope.download = function () {

    var searchExpression = utilityService.buildSearchExpression(
      $scope.searchViewOptions.baseFilter,
      $scope.searchViewOptions.stackFilters,

      $scope.searchViewOptions.filter,
      $scope.searchViewOptions.selectFilter,
      $scope.searchViewOptions.objectFilter,
      $scope.searchViewOptions.userSearch);



    var query = {
      type: 'participantGroups',
      searchExpression: searchExpression
    };
    utilityService.download(query);
  };

});
