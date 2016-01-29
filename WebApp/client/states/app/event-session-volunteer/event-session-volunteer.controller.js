app.controller('EventSessionVolunteerController', function ($scope, $log, $state, $mdDialog, $msUI, utilityService, siteService) {
  $log.debug('Loading EventSessionVolunteerController...');
  $log.debug('Loading ' + $state.name + '...');

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

    $scope.deleteEvent = function (event) {
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
            return successData;
          }, function (failureData) {
            // failure
            $msUI.showToast(failureData.errorMessage);
            $log.debug(failureData.errorMessage);
            return failureData;
          });

          $mdDialog.hide(formData);
          $state.go('app.user.event.sessions');
        };
    }

    $scope.showAddTeamMemberDialog = function (ev) {
        $mdDialog.show({
            controller: AddTeamMemberDialogController,
            templateUrl: '/client/states/app/user/add-team-member.dialog.html',
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
    function AddTeamMemberDialogController($scope, $mdDialog) {
        $scope.hide = function () {
            $mdDialog.hide();
        };
        $scope.cancel = function () {
            $mdDialog.cancel();
        };
        $scope.addTeamMember = function (formData) {
            $mdDialog.hide(formData);
        };
    }

    $scope.showAddParticipantGroupDialog = function (ev) {
        $mdDialog.show({
            controller: AddSchoolDialogController,
            templateUrl: '/client/states/app/user/add-school.dialog.html',
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
    function AddSchoolDialogController($scope, $mdDialog) {
        $scope.hide = function () {
            $mdDialog.hide();
        };
        $scope.cancel = function () {
            $mdDialog.cancel();
        };
        $scope.addSchool = function (formData) {
            siteService.createParticipantGroup(formData);
            $mdDialog.hide(formData);
        };
    }

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

    $scope.showAddSessionDialog = function (ev) {
        $mdDialog.show({
            controller: AddSessionDialogController,
            templateUrl: '/client/states/app/user/add-session.dialog.html',
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
    function AddSessionDialogController($scope, $mdDialog) {
        $scope.hide = function () {
            $mdDialog.hide();
        };
        $scope.cancel = function () {
            $mdDialog.cancel();
        };
        $scope.addSession = function (formData) {
            $mdDialog.hide(formData);
        };
    }

});
