app.controller('EventSessionsController', function ($scope, $mdDialog, $log, $msUI, $translate, utilityService, siteService) {
  $log.debug('Loading EventSessionsController...');

  $scope.searchHandler = siteService.model.eventSessions.search;
  $scope.demandParticipantGroup = siteService.demandParticipantGroup;
  $scope.demandEventSession = siteService.demandEventSession;
  $scope.setEventSessionCheckInOpen = siteService.setEventSessionCheckInOpen;



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

  //!! TODO improve this so that it automatically checks every 10 seconds or so
  // it's a cheap fix to set the eventSession state to Upcoming, Live, or Past
  // but we need to make this more robust with a timer or something
  $scope.setEventSessionState = function(eventSession) {
    var currentDate = new Date();
    var eventStart = new Date(eventSession.startDate);
    var eventEnd = new Date(eventSession.endDate);
    var state;
    if (eventEnd < currentDate) {
      state = "Past";
    } else if (eventStart > currentDate) {
      state = "Upcoming";
    } else if (eventStart < currentDate && eventEnd > currentDate) {
      state = "Live";
    }
    return state;
  };



  // $scope.setEventSessionState = function (eventSession, stateName) {
  //   siteService.setEventSessionState(eventSession, stateName);
  // };

  $scope.setEventSessionCheckInOpen = function (eventSession) {
    siteService.setEventSessionCheckInOpen(eventSession, eventSession.checkInOpen);
  };


  $scope.download = function () {
    var query = {
      type: 'tenants'
    };
    utilityService.download(query);
  };

  $scope.showAddSessionDialog = function (ev) {
    $mdDialog.show({
      controller: AddSessionDialogController,
      templateUrl: '/client/states/app/user/add-session.dialog.html',
      parent: angular.element(document.body),
      targetEvent: ev,
      clickOutsideToClose: true,
      fullscreen: false,
      locals: {
        eventSession: null,
        eventID: $scope.event.id,
        newOrEdit: "New"
      }
    })
    .then(function () {
      // $scope.status = 'You said the information was "' + answer + '".';
    }, function () {
      // $scope.status = 'You cancelled the dialog.';
    });
  };

  function AddSessionDialogController($scope, $mdDialog, eventSession, eventID, newOrEdit) {
    var EVENT_SESSION = "Event Session";
    $translate('EVENT_SESSION').then(function (event_session_text) {
      EVENT_SESSION = event_session_text;
    });
    var SESSION_MANAGER = "Session Manager";
    $translate('SESSION_MANAGER').then(function (session_manager_text) {
      SESSION_MANAGER = session_manager_text;
    });

    // these should be replaced with any of the users (aka "Team" members) who are eligible
    // to be Session Managers (aka Store Leaders)
    $scope.sampleUsers = [
      {firstName: "Sam", lastName: "Adams", isSessionManager: false},
      {firstName: "Jack", lastName: "Daniels", isSessionManager: false},
      {firstName: "Johnny", lastName: "Walker", isSessionManager: false},
      {firstName: "Lindsy", lastName: "Jones", isSessionManager: false}
    ];

    $scope.newOrEdit = newOrEdit;
    $scope.formInput = {
      timeChoice: [
        { display: "12:00", value: 0 }, { display: "12:30", value: 0.5 },
        { display: "1:00", value: 1 }, { display: "1:30", value: 1.5 },
        { display: "2:00", value: 2 }, { display: "2:30", value: 2.5 },
        { display: "3:00", value: 3 }, { display: "3:30", value: 3.5 },
        { display: "4:00", value: 4 }, { display: "4:30", value: 4.5 },
        { display: "5:00", value: 5 }, { display: "5:30", value: 5.5 },
        { display: "6:00", value: 6 }, { display: "6:30", value: 6.5 },
        { display: "7:00", value: 7 }, { display: "7:30", value: 7.5 },
        { display: "8:00", value: 8 }, { display: "8:30", value: 8.5 },
        { display: "9:00", value: 9 }, { display: "9:30", value: 9.5 },
        { display: "10:00", value: 10 }, { display: "10:30", value: 10.5 },
        { display: "11:00", value: 11 }, { display: "11:30", value: 11.5 }
      ],
      ampm: ["AM", "PM"]
    };


    function differentDay(dateTime1, dateTime2) {
      var date1 = new Date(dateTime1.getFullYear(), dateTime1.getMonth(), dateTime1.getDate(), 0, 0, 0);
      var date2 = new Date(dateTime2.getFullYear(), dateTime2.getMonth(), dateTime2.getDate(), 0, 0, 0);

      if (date1.getTime() != date2.getTime()) {
        return true;
      }
      return false;
    }

    $scope.startTimeChanged = function () {
      // hack
      $scope.formData.startTime = Number($scope.formData.startTime);

      //make core 'startDate' reflect control values
      var theDate = $scope.formData.startDate;
      var minutes = ($scope.formData.startTime % 1) * 60;
      var hour = (minutes == 30) ? $scope.formData.startTime - 0.5 : $scope.formData.startTime;
      if ($scope.formData.startTimeAmPm == "PM") {
        hour += 12;
      }

      $scope.formData.startDate = new Date(theDate.getFullYear(), theDate.getMonth(), theDate.getDate(), hour, minutes, 0, 0);

      // update endDate if change in startDate invalidated time range
      if (differentDay($scope.formData.startDate, $scope.formData.endDate)) {
        $scope.formData.endDate = $scope.formData.startDate;
        $scope.endTimeChanged();
      }

      if (!endTimeGreaterThanStartTime()) {
        tweakEndDate();
      }
    };

    function endTimeGreaterThanStartTime() {
      if ($scope.formData.startTimeAmPm == "PM" && $scope.formData.endTimeAmPm == "AM") {
        return false;
      }
      if ($scope.formData.startTimeAmPm == $scope.formData.endTimeAmPm) {
        if ($scope.formData.startTime >= $scope.formData.endTime) {
          return false;
        }
      }
      return true;
    }

    $scope.endTimeChanged = function () {
      // hack?? - md-option appears to set selected item as a string
      $scope.formData.endTime = Number($scope.formData.endTime);

      var theDate = $scope.formData.endDate;
      var minutes = ($scope.formData.endTime % 1) * 60;
      var hour = (minutes == 30) ? $scope.formData.endTime - 0.5 : $scope.formData.endTime;
      if ($scope.formData.endTimeAmPm == "PM") {
        hour += 12;
      }

      $scope.formData.endDate = new Date(theDate.getFullYear(), theDate.getMonth(), theDate.getDate(), hour, minutes, 0, 0);

      if ($scope.formData.startDate > $scope.formData.endDate) {
        tweakEndDate();
      }
    };

    function tweakEndDate() {
      $scope.formData.endTime = $scope.formData.startTime + 1;
      $scope.formData.endTimeAmPm = $scope.formData.startTimeAmPm;

      if ($scope.formData.endTime >= 12) {
        if ($scope.formData.endTimeAmPm == "AM") {
          $scope.formData.endTime -= 12;
          $scope.formData.endTimeAmPm = "PM";
        }
        else {
          $scope.formData.endTime = $scope.formData.startTime;
        }
      }
      $scope.endTimeChanged();
    }

    function initializeTime() {
      $scope.formData = {};
      $scope.formData.startDate = new Date();
      $scope.formData.startTime = 4;
      $scope.formData.startTimeAmPm = "PM";

      $scope.formData.endDate = $scope.formData.startDate;
      $scope.formData.endTime = 8;
      $scope.formData.endTimeAmPm = "PM";
    }

    function updateTimeDisplay() {
      $scope.formData.startTime = $scope.formData.startDate.getHours();
      $scope.formData.startTimeAmPm = "AM";
      if ($scope.formData.startTime >= 12) {
        $scope.formData.startTime -= 12;
        $scope.formData.startTimeAmPm = "PM";
      }
      if ($scope.formData.startDate.getMinutes() > 0) {
        $scope.formData.startTime += 0.5;
      }

      $scope.formData.endTime = $scope.formData.endDate.getHours();
      $scope.formData.endTimeAmPm = "AM";
      if ($scope.formData.endTime >= 12) {
        $scope.formData.endTime -= 12;
        $scope.formData.endTimeAmPm = "PM";
      }
      if ($scope.formData.endDate.getMinutes() > 0) {
        $scope.formData.endTime += 0.5;
      }
    }

    // initialize
    if (newOrEdit == "New") {
      initializeTime();
      $scope.startTimeChanged();
      $scope.endTimeChanged();
    }
    else if (newOrEdit == "Edit") {
      $scope.formData = eventSession;
      $scope.formData.startDate = new Date(eventSession.startDate);
      $scope.formData.endDate = new Date(eventSession.endDate);
      updateTimeDisplay();
    }



    $scope.hide = function () {
      $mdDialog.hide();
    };
    $scope.cancel = function () {
      $mdDialog.cancel();
    };

    $scope.createEventSession = function (formData) {
      formData.eventID = eventID;

      siteService.createEventSession(formData)
      .then(function (successData) {
        // success
        $msUI.showToast(EVENT_SESSION + " Created");
        $log.debug("Event Session Created.");
        //$state.go('app.user.event.sessions({ eventID: ' + successData + '})');
        return successData;
      }, function (failureData) {
        // failure
        $msUI.showToast(failureData.errorMessage);
        $log.debug(failureData.errorMessage);
        return failureData;
      });

      $mdDialog.hide($scope.formData);
    };

    $scope.editEventSession = function () {
      siteService.editEventSession($scope.formData.id, $scope.formData)
        .then(function (successData) {
          // success
          $msUI.showToast(EVENT_SESSION + " Updated");
          $log.debug("Edit eventSession completed.");
          return successData;
        }, function (failureData) {
          // failure
          $msUI.showToast(failureData.errorMessage);
          $log.debug(failureData.errorMessage);
          return failureData;
        });

      $mdDialog.hide($scope.formData);
    };

    $scope.showDeleteConfirmationDialog = function (ev, eventSession) {
      $mdDialog.hide($scope.formData);

      var confirm = $mdDialog.confirm()
        .title("Delete " + EVENT_SESSION)
        .textContent("Would you like to delete " + EVENT_SESSION + " '" + eventSession.name + "'?")
        .ariaLabel("Delete event session")
        .targetEvent(ev)
        .ok("yes")
        .cancel("no");

      $mdDialog.show(confirm).then(function () {
        deleteEventSession(eventSession);
      });
    };

    function deleteEventSession (item) {
      siteService.deleteEventSession(item)
      .then(function (successData) {
        // success
        $msUI.showToast("Session Deleted");
        $log.debug("Session Deleted.");
        return successData;
      }, function (failureData) {
        // failure
        $msUI.showToast(failureData.errorMessage);
        $log.debug(failureData.errorMessage);
        return failureData;
      });
    }


  }

  $scope.showEditEventSessionDialog = function (ev, eventSession) {
    $mdDialog.show({
      controller: AddSessionDialogController,
      templateUrl: '/client/states/app/user/add-session.dialog.html',
      parent: angular.element(document.body),
      targetEvent: ev,
      clickOutsideToClose: true,
      fullscreen: false,
      locals: {
        eventSession: angular.copy(eventSession),
        eventID: $scope.event.id,
        newOrEdit: "Edit"
      }
    })
      .then(function () {
        // $scope.status = 'You said the information was "' + answer + '".';
      }, function () {
        // $scope.status = 'You cancelled the dialog.';
      });
  };

  //function EditSessionDialogController($scope, $mdDialog, eventSession) {

  //  $scope.eventSession = eventSession;
  //  $scope.eventSession.startDate = new Date($scope.eventSession.startDate);
  //  $scope.eventSession.endDate = new Date($scope.eventSession.endDate);

  //  $scope.cancel = function () {
  //    $mdDialog.cancel();
  //  };

  //  $scope.editEventSession = function () {
  //    siteService.editEventSession($scope.eventSession.id, $scope.eventSession)
  //      .then(function (successData) {
  //        // success
  //        $msUI.showToast("EventSession Updated");
  //        $log.debug("Edit eventSession completed.");
  //        return successData;
  //      }, function (failureData) {
  //        // failure
  //        $msUI.showToast(failureData.errorMessage);
  //        $log.debug(failureData.errorMessage);
  //        return failureData;
  //      });

  //    $mdDialog.hide(formData);
  //  };
  //}

});
