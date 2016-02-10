app.controller('ReportController', function ($scope, $log, $mdDialog, utilityService, siteService) {
  var participants = siteService.model.participants;
  var participantGroups = siteService.model.participantGroups;

  // hmmm.. should we assume we can always get to the right $parent?
  $scope.eventParticipantGroupsIndex = $scope.$parent.eventParticipantGroupsIndex;
  $scope.eventSessionsIndex = $scope.$parent.eventSessionsIndex;
  $scope.eventParticipantsIndex = $scope.$parent.eventParticipantsIndex;


  $scope.showGenerateRemindersDialog = function (ev) {
    $mdDialog.show({
      controller: GenerateRemindersDialogController,
      templateUrl: '/client/states/app/user/generate-reminders.dialog.html',
      parent: angular.element(document.body),
      targetEvent: ev,
      clickOutsideToClose: true,
      fullscreen: false,
      locals: {
        participantGroupsIndex: $scope.eventParticipantGroupsIndex,
        eventSessionsIndex: $scope.eventSessionsIndex,
        eventParticipantsIndex: $scope.eventParticipantsIndex
      }
    })
    .then(function () {
      // $scope.status = 'You said the information was "' + answer + '".';
    }, function () {
      // $scope.status = 'You cancelled the dialog.';
    });
  }
  function GenerateRemindersDialogController($scope, $mdDialog, $filter, utilityService, participantGroupsIndex, eventSessionsIndex, eventParticipantsIndex) {
    $scope.participantGroups = siteService.model.participantGroups;
    $scope.eventSessions = siteService.model.eventSessions;
    $scope.eventParticipants = siteService.model.eventParticipants;

    $scope.participantGroupsIndex = participantGroupsIndex;
    $scope.eventSessionsIndex = eventSessionsIndex;
    $scope.eventParticipantsIndex = eventParticipantsIndex;

    $scope.sessionParticipantGroupsIndex = [];

    // find unique participantGroups for selected eventSession
    $scope.eventSessionIDChanged = function () {
      var flags = [];
      $scope.sessionParticipantGroupsIndex = [];

      $.each($scope.eventParticipantsIndex, function (index, value) {
        var participant = $scope.eventParticipants.hashMap[value];
        if ($scope.formData.eventSessionID == participant.eventSessionID) {
          if (!flags[participant.participantGroupID]) {
            flags[participant.participantGroupID] = true;
            $scope.sessionParticipantGroupsIndex.push(participant.participantGroupID);
          }
        }
      });
    }

    $scope.hide = function () {
      $mdDialog.hide();
    };
    $scope.cancel = function () {
      $mdDialog.cancel();
    };

    $scope.downloadReport = function (participantGroupID) {
      var query = {
        type: 'reminderFormForSchool',
        participantGroupID: participantGroupID
      };
      utilityService.download(query);
    }

    $scope.downloadReminderForm = function (eventSessionID, participantGroupID) {
      var query = {
        type: 'reminderFormForEventParticipant',
        participantGroupID: participantGroupID,
        eventSessionID: eventSessionID
      };
      utilityService.download(query);
    }
  }
});

app.controller('EventController', function ($scope, $log, $state, $mdDialog, $msUI, utilityService, siteService, event, eventSessionsIndex, eventParticipantsIndex, eventParticipantGroupsIndex) {
  $scope.event = event;
  $scope.eventSessionsIndex = eventSessionsIndex;
  $scope.eventParticipantsIndex = eventParticipantsIndex;
  $scope.eventParticipantGroupsIndex = eventParticipantGroupsIndex;

  $scope.getParticipantCount = function (participantGroupID) {
    return $scope.eventParticipantsIndex.filter(function (ep) {
      var pID = $scope.model.eventParticipants.hashMap[ep].participantID;
      var pgID = $scope.model.participants.hashMap[pID].participantGroupID;
      return pgID == participantGroupID;
    }).length;
  };
});

app.controller('UserController', function ($scope, $log, $state, $mdDialog, $msUI, utilityService, siteService) {
  $log.debug('Loading UserController...');
  $scope.participantGroups = siteService.model.participantGroups;

  $scope.sampleEvents = [
    {
      name: "Spring OSB",
      description: "Event description goes here lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam.",
      teamMembers: [
        { firstName: "Jenny", lastName: "Boone", email: "jenny.b@email.com" },
        { firstName: "Benson", lastName: "Green", email: "b.green@email.com" },
        { firstName: "Larry", lastName: "Munson", email: "larry.munson@email.com" }
      ],
      schools: [
        { name: "Eastside Elementary", totalStudents: 2195, participants: 50 },
        { name: "Westview", totalStudents: 489, participants: 12 }
      ],
      participants: [
        { firstName: "Sam", lastName: "Homeyer", grade: "Fourth grader", school: "Westview", registered: false },
        { firstName: "Jean", lastName: "Wilkins", grade: "Third grader", school: "Eastside Elementary", registered: true },
        { firstName: "Tom", lastName: "Sawyer", grade: "Fifth grader", school: "Westview", registered: false },
        { firstName: "Andrew", lastName: "Wiggin", grade: "Kindergarten", school: "Westview", registered: false }
      ],
      sessions: [
        { name: "OSB Kickoff", date: "2016-04-01T18:00:00.000Z", location: "Fred Meyer at 17667 NE 76th St" }
      ]
    }
  ];

  $scope.searchParticipantGroups = siteService.model.participantGroups.search;
  $scope.searchParticipants = siteService.model.participants.search;
  $scope.searchEvents = siteService.model.events.search;

  $scope.sortOptions = [
    { name: 'Name', serverTerm: 'item.name', clientFunction: utilityService.localeCompareByPropertyThenByID('name') },
  ];

  var filterByStateFactory = function (includeState) {
    var includeStateLocal = includeState;
    return function (item) {
      return item.state === includeStateLocal;
    };
  };

  $scope.filterOptions = [
    { name: 'Active', serverTerm: '$Active', clientFunction: filterByStateFactory("Active") },
    { name: 'Disabled', serverTerm: '$Disabled', clientFunction: filterByStateFactory("Disabled") },
    { name: 'All' }
  ];

  $scope.searchViewOptions = {
    sort: $scope.sortOptions[0],
    filter: $scope.filterOptions[2],
    participantGroupSearch: null
  };

  $scope.generateRandomEvent = function () {
    siteService.generateRandomEvent();
  }

  deleteEvent = function (event) {
    siteService.deleteEvent(event)
    .then(function (successData) {
      // success
      $msUI.showToast("Event Deleted");
      $log.debug("Task completed.");
      return successData;
    }, function (failureData) {
      // failure
      $msUI.showToast(failureData.errorMessage);
      $log.debug(failureData.errorMessage);
      return failureData;
    });
  }



  $scope.searchParticipantViewOptions = {
    sort: $scope.sortOptions[0],
    filter: $scope.filterOptions[2],
    participantSearch: null
  };

  $scope.showCreateEventDialog = function (ev) {
    $mdDialog.show({
      controller: CreateEventDialog,
      templateUrl: '/client/states/app/user/create-event.dialog.html',
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
  function CreateEventDialog($scope, $mdDialog) {
    $scope.hide = function () {
      $mdDialog.hide();
    };
    $scope.cancel = function () {
      $mdDialog.cancel();
    };

    $scope.generateRandomEvent = function () {
      siteService.generateRandomEvent();
      $mdDialog.hide($scope.formData);
    };

    $scope.createEvent = function () {

      siteService.createEvent($scope.formData)
      .then(function (successData) {
        // success
        $msUI.showToast("Event Created");
        $log.debug("Task completed.");
        //$state.go('app.user.event.sessions({ eventID: ' + successData + '})');
        return successData;
      }, function (failureData) {
        // failure
        $msUI.showToast(failureData.errorMessage);
        $log.debug(failureData.errorMessage);
        return failureData;
      });

      $mdDialog.hide($scope.formData);
      //$state.go('app.user.event.sessions');
    };
  }

  $scope.showEditEventDialog = function (event) {
    $mdDialog.show({
      controller: EditEventDialogController,
      templateUrl: '/client/states/app/user/edit-event.dialog.html',
      parent: angular.element(document.body),
      //targetEvent: ev,
      clickOutsideToClose: true,
      fullscreen: false,
      locals: {
        event: angular.copy(event)
      }
    })
    .then(function (successData) {
      // $scope.status = 'You said the information was "' + answer + '".';
    }, function () {
      // $scope.status = 'You cancelled the dialog.';
    });
  }
  function EditEventDialogController($scope, $mdDialog, event) {

    $scope.event = event;

    $scope.hide = function () {
      $mdDialog.hide();
    };
    $scope.cancel = function () {
      $mdDialog.cancel();
    };

    $scope.editEvent = function () {

      siteService.editEvent($scope.event.id, $scope.event)
      .then(function (successData) {
        // success
        $msUI.showToast("Event Updated");
        $log.debug("Edit event completed.");
        return successData;
      }, function (failureData) {
        // failure
        $msUI.showToast(failureData.errorMessage);
        $log.debug(failureData.errorMessage);
        return failureData;
      });

      $mdDialog.hide();
    };
  }

  $scope.showDeleteConfirmationDialog = function (ev, event) {
    var confirm = $mdDialog.confirm()
      .title("Delete Event")
      .textContent("Would you like to delete event '" + event.name + "'?")
      .ariaLabel("Delete event")
      .targetEvent(ev)
      .ok("yes")
      .cancel("no");

    $mdDialog.show(confirm).then(function () {
      deleteEvent(event);
    });
  };

  $scope.showAddUserDialog = function (ev) {
    $mdDialog.show({
      controller: AddUserDialogController,
      templateUrl: '/client/states/app/user/add-user.dialog.html',
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
  function AddUserDialogController($scope, $mdDialog) {
    $scope.hide = function () {
      $mdDialog.hide();
    };
    $scope.cancel = function () {
      $mdDialog.cancel();
    };
    $scope.addUser = function (formData) {
      $mdDialog.hide(formData);
    };
  }

  $scope.showAddParticipantGroupDialog = function (ev) {
    $mdDialog.show({
      controller: AddEditParticipantGroupDialogController,
      templateUrl: '/client/states/app/user/add-participant-group.dialog.html',
      parent: angular.element(document.body),
      targetEvent: ev,
      clickOutsideToClose: true,
      fullscreen: false,
      locals: {
        newOrEdit: "New",
        participantGroup : null
      }
    })
    .then(function () {
      // $scope.status = 'You said the information was "' + answer + '".';
    }, function () {
      // $scope.status = 'You cancelled the dialog.';
    });
  }

  $scope.showEditParticipantGroupDialog = function (ev, participantGroup) {
    $mdDialog.show({
      controller: AddEditParticipantGroupDialogController,
      templateUrl: '/client/states/app/user/add-participant-group.dialog.html',
      parent: angular.element(document.body),
      targetEvent: ev,
      clickOutsideToClose: true,
      fullscreen: false,
      locals: {
        newOrEdit: "Edit",
        participantGroup: angular.copy(participantGroup)
      }
    })
    .then(function () {
      // $scope.status = 'You said the information was "' + answer + '".';
    }, function () {
      // $scope.status = 'You cancelled the dialog.';
    });
  }

  function AddEditParticipantGroupDialogController($scope, $mdDialog, $translate, newOrEdit, participantGroup) {
    var PARTICIPANT_GROUP = "Participant Group";
    $translate('PARTICIPANT_GROUP').then(function (pg_text) {
      PARTICIPANT_GROUP = pg_text;
    });

    $scope.newOrEdit = newOrEdit;

    if ($scope.newOrEdit == 'Edit') {
      $scope.formData = participantGroup;
    }

    $scope.hide = function () {
      $mdDialog.hide();
    };
    $scope.cancel = function () {
      $mdDialog.cancel();
    };
    $scope.addParticipantGroup = function (formData) {
      siteService.createParticipantGroup(formData);
      $mdDialog.hide(formData);
    };

    $scope.editParticipantGroup = function (formData) {
      siteService.editParticipantGroup(formData.id, formData);
      $mdDialog.hide(formData);
    };

    $scope.showDeleteConfirmationDialog = function (ev, participantGroup) {
      $mdDialog.hide($scope.formData);

      var confirm = $mdDialog.confirm()
        .title("Delete " + PARTICIPANT_GROUP)
        .textContent("Would you like to delete '" + participantGroup.name + "'?")
        .ariaLabel("Delete particpant group")
        .targetEvent(ev)
        .ok("yes")
        .cancel("no");

      $mdDialog.show(confirm).then(function () {
        siteService.deleteParticipantGroup(participantGroup)
          .then(function (successData) {
            // success
            $msUI.showToast(PARTICIPANT_GROUP + " Deleted");
            $log.debug("Participant Group Deleted.");
            return successData;
          }, function (failureData) {
            // failure
            $msUI.showToast(failureData.errorMessage);
            $log.debug(failureData.errorMessage);
            return failureData;
          });
      });
    };
  }

});
