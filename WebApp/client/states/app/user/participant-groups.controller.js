app.controller('ParticipantGroupsController', function ($scope, $mdDialog, $log, $msUI, $translate, utilityService, siteService, event) {
  $log.debug('Loading ParticipantGroupsController...');

  $scope.searchHandler = siteService.model.participantGroups.search;

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

  $scope.searchViewOptions = {
    sort: $scope.sortOptions[0]
  };


  var eventParticipantIndexers = [];
  var eventParticipantIndexerHashMap = {};

  $scope.getParticipantCount = function (participantGroupID) {
    // We assume our model already has loaded any participants - an assumption we inherit from our containers
    // Lazy create an indexer for each participantGroup as a cheap way to count 'em
    if (!eventParticipantIndexerHashMap[participantGroupID]) {
      // create and register an indexer - so we can track the membership count in this group
      var indexer = {
        index: [],
        sort: utilityService.compareByProperties('id'),
        baseFilter: utilityService.filterByPropertyValue('eventID', event.id),
        selectFilter: utilityService.filterByPropertyValue('participantGroupID', participantGroupID)
      };
      // register (& remember to unregister them in $scope.$on("$destroy");
      utilityService.registerIndexer(siteService.eventParticipants, indexer);
      eventParticipantIndexers.push(indexer);
      eventParticipantIndexerHashMap[participantGroupID] = indexer;
    }

    return eventParticipantIndexerHashMap[participantGroupID].index.length;
  };

/*
  $scope.participantGroupFilters = [];

  siteService.model.participantGroups.search($scope.searchViewOptions.baseFilter.serverTerm, "", 0, 999999)
  .then(function (itemsData) {
    console.log("setting up participantGroupsFilters", itemsData);

    angular.forEach(itemsData.ids, function (itemID) {
      var item = itemsData.hashMap[itemID];
      // create and register an indexer - so we can track the membership count in this group
      // (remember to unregister them in $scope.$on("$destroy");
      var indexer = {
        index: [],
        sort: utilityService.compareByProperties('id'),
        baseFilter: $scope.searchViewOptions.baseFilter,
        selectFilter: utilityService.filterByPropertyValue('participantGroupID', itemID)
      };
      utilityService.registerIndexer($scope.model.eventParticipants, indexer);
      // Push a filter on the filter stack
      $scope.participantGroupFilters.push(
      {
        name: item.name,
        indexer: indexer,
        serverTerm: '$participantGroup:' + itemID,
        clientFunction: utilityService.filterByPropertyValue('participantGroupID', itemID)
      });
    });
  });
*/

  $scope.$on("$destroy", function () {
    utilityService.unRegisterIndexer(siteService.eventParticipants, eventParticipantIndexers);
  });







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