app.controller('EventParticipantsController', function ($scope, $mdDialog, $log, $msUI, utilityService, siteService, event, eventSession) {
  $log.debug('Loading EventParticipantsController...');

  $scope.searchHandler = siteService.model.eventParticipants.search;
  $scope.demandParticipantGroup = siteService.demandParticipantGroup;
  $scope.demandEventSession = siteService.demandEventSession;


  $scope.event = event;
  // (optional - provided when we're looking at just one session, null when we're not)
  $scope.eventSession = eventSession;


  $scope.searchViewOptions = { };


  // Establish our Base filtering (evaluatuating in order of most restrictive to least restrictive)
  if ($scope.eventSession) {
    // filter to one EventSession
    $scope.searchViewOptions.baseFilters = { serverTerm: '$eventSession:' + $scope.eventSession.id, clientFunction: utilityService.filterByPropertyValue('eventSessionID', $scope.eventSession.id) };
  } else if ($scope.event) {
    // filter to one Event
    $scope.searchViewOptions.baseFilters = { serverTerm: '$event:' + $scope.event.id, clientFunction: utilityService.filterByPropertyValue('eventID', $scope.event.id) };
  } else {
    // no filtering
  }



  $scope.sortOptions = [
    { name: 'First Name', serverTerm: 'Participant.FirstName', clientFunction: utilityService.compareByProperties('firstName', 'id') },
    { name: 'Last Name', serverTerm: 'Participant.LastName', clientFunction: utilityService.compareByProperties('lastName', 'id') },
    //!! this is currently broken - as we don't really want to sort by the ParticipantGroup ID
    { name: 'School', serverTerm: 'Participant.ParticipantGroup.Name', clientFunction: utilityService.compareByProperties('participantGroupName', 'id') },
    { name: 'Grade', serverTerm: 'ExEventParticipant.item.Grade', clientFunction: utilityService.compareByProperties('grade', 'id') }
  ];

  $scope.searchViewOptions.sort = $scope.sortOptions[0];

  $scope.filterOptions = [
    //{ name: 'Active', serverTerm: '$Active', clientFunction: filterByStateFactory("Active") },
    //{ name: 'Disabled', serverTerm: '$Disabled', clientFunction: filterByStateFactory("Disabled") },
    { name: 'All' }
  ];

  //!! fixup our event filter. Need a more robust way to handle this


  $scope.eventParticipantStateFilters = [
    {
      name: 'Not Checked-In',
      indexer: {
        index: [],
        sort: utilityService.compareByProperties('id'),
        filter: utilityService.filterByPropertyHasValue('!checkInTimestamp')
      },
      serverTerm: '$notCheckedIn',
      clientFunction: utilityService.filterByPropertyHasValue('!checkInTimestamp')
    },
    {
      name: 'Checked-In',
      indexer: {
        index: [],
        sort: utilityService.compareByProperties('id'),
        filter: function (item) {
          return item.checkInTimestamp && !item.checkOutTimestamp;
        }
      },
      serverTerm: '$checkedIn',
      clientFunction: function (item) {
        return item.checkInTimestamp && !item.checkOutTimestamp;
      }
    }
    // {
    //   name: 'Checked-Out',
    //   indexer: {
    //     index: [],
    //     sort: utilityService.compareByProperties('id'),
    //     filter: utilityService.filterByPropertyHasValue('checkOutTimestamp')
    //   },
    //   serverTerm: '$checkedOut',
    //   clientFunction: utilityService.filterByPropertyHasValue('checkOutTimestamp')
    // }
  ];
  angular.forEach($scope.eventParticipantStateFilters, function (filter) {
    utilityService.registerIndexer($scope.model.eventParticipants, filter.indexer);
  });



  $scope.participantGroupFilters = [];

  siteService.model.participantGroups.search($scope.searchViewOptions.baseFilters.serverTerm, "", 0, 99)
  .then(function (itemsData) {
    console.log("search participantGroups", itemsData);

    angular.forEach(itemsData.ids, function (itemID) {
      var item = itemsData.hashMap[itemID];
      // create and register an indexer - so we can track the membership count in this group
      // (remember to unregister them in $scope.$on("$destroy");
      var indexer = {
        index: [],
        sort: utilityService.compareByProperties('id'),
        filter: utilityService.filterByPropertyValue('participantGroupID', itemID)
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

/*
  $scope.redmondParticipantsIndexer = {
    index: [],
    sort: utilityService.compareByProperties('id'),
    filter: function (item) {
      return item.participantGroupID === 18;
    }
  };
  utilityService.registerIndexer($scope.model.eventParticipants, $scope.redmondParticipantsIndexer);
*/
  $scope.$on("$destroy", function () {
    //!! utilityService.unRegisterIndexer($scope.model.eventParticipants, $scope.redmondParticipantsIndexer);

    angular.forEach($scope.participantGroupFilters, function (filter) {
      utilityService.unRegisterIndexer($scope.model.eventParticipants, filter.indexer);
    });
    angular.forEach($scope.eventParticipantStateFilters, function (filter) {
      utilityService.unRegisterIndexer($scope.model.eventParticipants, filter.indexer);
    });
  });


  $scope.toggle = function (item, list) {
    var idx = list.indexOf(item);
    if (idx > -1) list.splice(idx, 1);
    else list.push(item);
  };
  $scope.exists = function (item, list) {
    return list.indexOf(item) > -1;
  };

  $scope.searchViewOptions.stackFilters = [];


  $scope.showUploadParticipantsDialog = function (ev, event) {
    $mdDialog.show({
      controller: 'UploadParticipantsDialogController',
      templateUrl: '/client/states/app/user/upload-participants.dialog.html',
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



  $scope.checkIn = function (eventParticipant) {
    siteService.checkInEventParticipant(eventParticipant)
    .then(function (successData) {
      // success
      return successData;
    }, function (failureData) {
      // failure
      $log.debug(failureData.errorMessage);
      return failureData;
    });
  };

  $scope.checkOut = function (eventParticipant) {
    siteService.checkOutEventParticipant(eventParticipant)
    .then(function (successData) {
      // success
      return successData;
    }, function (failureData) {
      // failure
      $log.debug(failureData.errorMessage);
      return failureData;
    });
  };

  $scope.download = function () {

    var searchExpression = utilityService.buildSearchExpression(
      $scope.searchViewOptions.baseFilters,
      $scope.searchViewOptions.stackFilters,

      $scope.searchViewOptions.filter,
      $scope.searchViewOptions.selectFilter,
      $scope.searchViewOptions.userFilter,
      $scope.searchViewOptions.userSearch);

    var query = {
      type: 'eventParticipants',
      searchExpression: searchExpression
    };
    utilityService.download(query);
  };
});