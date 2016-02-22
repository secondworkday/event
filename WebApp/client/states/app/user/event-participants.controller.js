app.controller('EventParticipantsController', function ($scope, $translate, $mdDialog, $log, $msUI, utilityService, siteService, event, eventSession) {
  $log.debug('Loading EventParticipantsController...');

  $scope.searchHandler = siteService.model.eventParticipants.search;
  $scope.demandParticipantGroup = siteService.demandParticipantGroup;
  $scope.demandEventSession = siteService.demandEventSession;


  //!! multi-select stuff
  //!! I think we'll want to move this insde ms-search-view

  //!! We need a separate Index because we'll need the ability to "select" items that aren't yet loaded into the view ...
  $scope.$selectedIndex = [];

  $scope.toggleList = function (list, item) {
    var idx = list.indexOf(item);
    if (idx > -1) {
      // toggle off...
      list.splice(idx, 1);
    }
    else {
      // toggle on...
      list.push(item);
    }
  };
  $scope.listContains = function (list, item) {
    return list.indexOf(item) > -1;
  };

  $scope.toggleSelectedIndex = function (item) {
    $scope.toggleList($scope.$selectedIndex, item.id);
  };
  $scope.selectedIndexContains = function (item) {
    return $scope.listContains($scope.$selectedIndex, item.id);
  };

  $scope.unselectAll = function () {
    $scope.$selectedIndex = [];
  };

  $scope.toggleSelectAll = function () {
    if ($scope.$selectedIndex && $scope.$selectedIndex.length > 0) {
      $scope.$selectedIndex = [];
    } else {
      //!! fetch 'em and select all
    }
  };







  $scope.event = event;
  // (optional - provided when we're looking at just one session, null when we're not)
  $scope.eventSession = eventSession;


  $scope.searchViewOptions = { };


  // Establish our Base filtering (evaluatuating in order of most restrictive to least restrictive)
  if ($scope.eventSession) {
    // filter to one EventSession
    $scope.searchViewOptions.baseFilter = { serverTerm: '$eventSession:' + $scope.eventSession.id, clientFunction: utilityService.filterByPropertyValue('eventSessionID', $scope.eventSession.id) };
  } else if ($scope.event) {
    // filter to one Event
    $scope.searchViewOptions.baseFilter = { serverTerm: '$event:' + $scope.event.id, clientFunction: utilityService.filterByPropertyValue('eventID', $scope.event.id) };
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


  // Create an Indexer for the EventSessions in this Event. Useful for filtering & providing the options when selection an EventSession in the UI.
  $scope.eventSessionsIndexer = {
    index: [],
    //!! probably want to sort based on chronological order of when the sessions happen
    sort: utilityService.compareByProperties('id'),
    filter: function (item) {
      // only interested in EventSessions in our Event
      return item.eventID === $scope.event.id;
    }
  };
  utilityService.registerIndexer($scope.model.eventSessions, $scope.eventSessionsIndexer);

  //!! quick hack to watch for EventSession changes.
  //   problem is ng-init values are only setup once, but we are initializing based on a property value that's mutable
  // A probably better than this is to have a controller inside the ng-repeat
  // B or perhaps have a directive that does this see: http://stackoverflow.com/questions/20024156/how-to-watch-changes-on-models-created-by-ng-repeat
  $scope.watchEventParticipantEventSession = function (repeatScope, watchExpression) {
    repeatScope.$watch(watchExpression, function (newValue, oldVaue, scope) {
      if (newValue !== oldVaue) {
        scope["eventParticipantEventSession"] = $scope.model.eventSessions.hashMap[newValue];
      }
    });
  };



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




  // ** If we're not scoped to a single EventSession, provide filters to allow the User to filter by EventSession

  $scope.eventSessionFilters = [];
  if (!$scope.eventSession) {
    siteService.model.eventSessions.search($scope.searchViewOptions.baseFilter.serverTerm, "", 0, 999999)
    .then(function (itemsData) {
      console.log("setting up eventSessionFilters", itemsData);

      // ** Create an "Unregistered" EventSession indexer
      var indexer = {
        index: [],
        sort: utilityService.compareByProperties('id'),
        filter: utilityService.filterByPropertyHasValue('!eventSessionID')
      };
      utilityService.registerIndexer($scope.model.eventParticipants, indexer);
      // Push a filter on the filter stack
      $scope.eventSessionFilters.push(
      {
        name: "Unregistered",
        indexer: indexer,
        serverTerm: '$eventSession:',
        clientFunction: utilityService.filterByPropertyHasValue('!eventSessionID')
      });

      // ** Create indexers for all the other EventSessions
      angular.forEach(itemsData.ids, function (itemID) {
        var item = itemsData.hashMap[itemID];
        // create and register an indexer - so we can track the membership count in this group
        // (remember to unregister them in $scope.$on("$destroy");
        var indexer = {
          index: [],
          sort: utilityService.compareByProperties('id'),
          filter: utilityService.filterByPropertyValue('eventSessionID', itemID)
        };
        utilityService.registerIndexer($scope.model.eventParticipants, indexer);
        // Push a filter on the filter stack
        $scope.eventSessionFilters.push(
        {
          name: item.name,
          indexer: indexer,
          serverTerm: '$eventSession:' + itemID,
          clientFunction: utilityService.filterByPropertyValue('eventSessionID', itemID)
        });
      });
    });
  }















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

    utilityService.unRegisterIndexer($scope.model.eventSessions, $scope.eventSessionsIndexer);

    angular.forEach($scope.participantGroupFilters, function (filter) {
      utilityService.unRegisterIndexer($scope.model.eventParticipants, filter.indexer);
    });

    angular.forEach($scope.eventSessionFilters, function (filter) {
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
        event: event,
        eventSessionsIndex: $scope.eventSessionsIndex
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
      controller: AddEditParticipantsDialogController,
      templateUrl: '/client/states/app/user/add-participants.dialog.html',
      locals: {
        event: event,
        eventSessionsIndex: $scope.eventSessionsIndex,
        eventParticipant: null,
        newOrEdit: "New"
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

  $scope.showEditParticipantsDialog = function (ev, event, eventParticipant) {
    $mdDialog.show({
      controller: AddEditParticipantsDialogController,
      templateUrl: '/client/states/app/user/add-participants.dialog.html',
      locals: {
        event: event,
        eventSessionsIndex: $scope.eventSessionsIndex,
        eventParticipant: angular.copy(eventParticipant),
        newOrEdit: "Edit"
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

  function AddEditParticipantsDialogController($scope, $mdDialog, $translate, event, eventSessionsIndex, eventParticipant, newOrEdit) {

    var PARTICIPANT = "Event Participant";
    $translate('PARTICIPANT').then(function (participant_text) {
      PARTICIPANT = participant_text;
    });

    $scope.eventSessions = siteService.model.eventSessions;
    $scope.participantGroups = siteService.model.participantGroups;

    $scope.event = event;
    $scope.eventSessionsIndex = eventSessionsIndex;
    $scope.newOrEdit = newOrEdit;

    if ($scope.newOrEdit == 'Edit') {
      $scope.formData = eventParticipant;
    }

    $scope.formInput = {
      genders: [ "Male", "Female" ]
    };

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
    $scope.createEventParticipant = function (event, formData) {
      siteService.createEventParticipant(event, formData);
      $mdDialog.hide();
    };
    $scope.editEventParticipant = function (formData) {
      siteService.editEventParticipant(formData);
      $mdDialog.hide();
    };

    $scope.showDeleteConfirmationDialog = function (ev, eventParticipant) {
      $mdDialog.hide($scope.formData);

      var confirm = $mdDialog.confirm()
        .title("Delete " + PARTICIPANT)
        .textContent("Would you like to delete " + PARTICIPANT + " '" + eventParticipant.firstName + " " + eventParticipant.lastName + "'?")
        .ariaLabel("Delete event participant")
        .targetEvent(ev)
        .ok("yes")
        .cancel("no");

      $mdDialog.show(confirm).then(function () {
        deleteEventParticipant(eventParticipant);
      });
    };

    function deleteEventParticipant(item) {
      siteService.deleteEventParticipant(item)
      .then(function (successData) {
        // success
        $msUI.showToast(PARTICIPANT + " Deleted");
        $log.debug("Event Participant Deleted.");
        return successData;
      }, function (failureData) {
        // failure
        $msUI.showToast(failureData.errorMessage);
        $log.debug(failureData.errorMessage);
        return failureData;
      });
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

  //!! TODO -- wire up the ability to Undo a check in
  $scope.undoCheckIn = function (eventParticipant) {
    siteService.undoCheckInEventParticipant(eventParticipant)
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


  $scope.deleteEventParticipants = function () {

    var itemIDs = $scope.$selectedIndex;

    siteService.deleteEventParticipants(itemIDs)
    .then(function (successData) {
      // success
      //!! fix the pluralization here - or perhaps improve upon the Toast

      var PARTICIPANT = $translate.instant('PARTICIPANT');
      $msUI.showToast(itemIDs.length + " " + PARTICIPANT + "(s) Deleted");
      $log.debug(itemIDs.length + " Event Participant(s) Deleted.");
      return successData;
    }, function (failureData) {
      // failure
      $msUI.showToast(failureData.errorMessage);
      $log.debug(failureData.errorMessage);
      return failureData;
    });
  };


  $scope.download = function () {

    var searchExpression = utilityService.buildSearchExpression(
      $scope.searchViewOptions.baseFilter,
      $scope.searchViewOptions.stackFilters,

      $scope.searchViewOptions.filter,
      $scope.searchViewOptions.selectFilter,
      $scope.searchViewOptions.objectFilter,
      $scope.searchViewOptions.userSearch);

    var query = {
      type: 'eventParticipants',
      searchExpression: searchExpression
    };
    utilityService.download(query);
  };

  $scope.showBulkEditParticipantsDialog = function ($event) {
    var parentEl = angular.element(document.body);
    $mdDialog.show({
      parent: parentEl,
      targetEvent: $event,
      templateUrl: '/client/states/app/user/bulk-edit-participants.dialog.html',
      locals: {
        event: event,
        eventSessionsIndex: $scope.eventSessionsIndex
      },
      controller: BulkEditParticipantsDialog
    });
    function BulkEditParticipantsDialog($scope, $mdDialog, $translate, event, eventSessionsIndex) {
      $scope.eventSessions = siteService.model.eventSessions;
      $scope.participantGroups = siteService.model.participantGroups;

      $scope.event = event;
      $scope.eventSessionsIndex = eventSessionsIndex;

      $scope.cancel = function () {
        $log.debug("You canceled the dialog.");
        $mdDialog.hide();
      };
      $scope.apply = function () {
        $log.debug("You applied the bulk edit.");
        //!! TODO add function to actually apply the bulk edit to all the records
        $mdDialog.hide();
      };
    }
  };




  // init

  // load our eventSessions indexer
  var searchExpression = "$event:" + $scope.event.id;
  siteService.model.eventSessions.search(searchExpression, "", 0, 99999);

  // pre-load all our baseFilters participants - which loads up our Indexers
  var searchExpression = utilityService.buildSearchExpression(
    $scope.searchViewOptions.baseFilter);
  $scope.searchHandler(searchExpression, "", 0, 99999);

});
