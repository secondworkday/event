app.config(['$stateProvider', 'AUTHORIZATION_ROLES', function ($stateProvider, AUTHORIZATION_ROLES) {

  // Now set up the states
  $stateProvider

    .state('app.spa-landing', {
      redirectTo: 'app.user.events',
    })


    .state('app.user', {
        abstract: true,
        templateUrl: '/client/states/app/user/user-container.html',
        controller: 'UserController'
    })

    .state('app.user.team', {
      url: '/team',
      templateUrl: '/client/states/app/user/team.html',
      //!! did we just re-use this controller as a shortcut?
      controller: 'SystemUsersController'
    })
    .state('app.user.participant-groups', {
      url: '/participant-groups',
      templateUrl: '/client/states/app/user/participant-groups.html'
    })
    .state('app.user.locations', {
      url: '/locations',
      templateUrl: '/client/states/app/user/locations.html'
    })

    .state('app.user.events', {
        url: '/events',
        templateUrl: '/client/states/app/user/events.html',
        controller: 'EventsController',
        resolve: {
          //!! - We do this to get the date range associated with an Event - probably better to have the server do that.
          eventSessions: function (siteService, $stateParams) { return siteService.ensureAllEventSessions() }
        }
    })

    .state('app.user.event', {
      abstract: true,
      url: '/event/:eventID',
      templateUrl: '/client/states/app/user/event.html',
      controller: "EventController",
      resolve: {
        event: function ($stateParams, siteService) {
          var eventID = $stateParams.eventID;
          return siteService.events.ensure(eventID);
        },
        //!! document why we need this
        eventSession: function () {
          return null;
        }
      }
    })
    .state('app.user.event.sessions', {
      url: '/sessions',
      data: {
        'selectedTab': 0
      },
      views: {
        'sessions': {
          templateUrl: '/client/states/app/user/event-sessions.html',
          controller: "EventSessionsController"
        }
      }
    })
    .state('app.user.event.schools', {
      url: '/schools',
      data: {
        'selectedTab': 1
      },
      views: {
        'schools': {
          templateUrl: '/client/states/app/user/participant-groups.html',
          controller: "ParticipantGroupsController"
        }
      }
    })
    .state('app.user.event.participants', {
      url: '/participants',
      data: {
        'selectedTab': 2
      },
      params: {
        activityID: undefined
      },
      views: {
        'participants': {
          templateUrl: '/client/states/app/user/event-participants.html',
          controller: "EventParticipantsController"
        }
      },
      resolve: {
        initialSelection: function ($stateParams, utilityService) {
          // careful - might be an invalid or unauthorized ID
          var itemID = $stateParams.activityID;
          if (itemID) {
            return utilityService.model.activityLog.getSet(itemID)
            .then(function (itemIDs) {
              return itemIDs;
            });
          }
          return null; 
        }
      }
    })
    .state('app.user.event.documents', {
      url: '/documents',
      data: {
        'selectedTab': 3
      },
      views: {
        'documents': {
          templateUrl: '/client/states/app/user/documents.html'
        }
      }
    })

    .state("app.user.event.documents.no-show", {
      onEnter: function ($state, $mdDialog, siteService, event) {
        var ev = null; // this should be the $event 
        $mdDialog.show({
          controller: 'NoShowReportDialogController',
          templateUrl: '/client/states/app/user/no-show-report.dialog.html',
          parent: angular.element(document.body),
          targetEvent: ev,
          clickOutsideToClose: true,
          fullscreen: false,
          locals: {
            // this is our event, not the browser $event
            event: event
          },
          resolve: {
            // Fetch eventSessions before we show the dialog
            eventSessionsIndex: function () {
              return siteService.eventSessions.search("$event:" + event.id, "", 0, 999999)
              .then(function (itemsData) {
                return itemsData.ids;
              });
            }
          }
        })
        .then(function () {
          //
        }, function () {
          // $scope.status = 'You cancelled the dialog.';
        })
        .finally(function () {
          $state.go('^');
        });
      }
    })

    .state("app.user.event.documents.generate-reminders", {
      onEnter: function ($state, $mdDialog, siteService, event) {
        var ev = null; // this should be the $event 

        $mdDialog.show({
          controller: 'GenerateRemindersDialogController',
          templateUrl: '/client/states/app/user/generate-reminders.dialog.html',
          parent: angular.element(document.body),
          targetEvent: ev,
          clickOutsideToClose: true,
          fullscreen: false,
          locals: {
            // this is our event, not the browser $event
            event: event
          },
          resolve: {
            // Fetch eventSessions before we show the dialog
            eventSessionsIndex: function () {
              return siteService.eventSessions.search("$event:" + event.id, "", 0, 999999)
              .then(function (itemsData) {
                return itemsData.ids;
              });
            }
          }
        })
        .then(function (selectedData) {
          //if (selectedData.sendEmail) {
          //  sendMail(selectedData.participantGroupID);
          //}
        }, function () {
          // $scope.status = 'You cancelled the dialog.';
        })
        .finally(function () {
          $state.go('^');
        });
      }
    })

    .state('app.user.event.activity-log', {
      url: '/activity-log',
      data: {
        'selectedTab': 4
      },
      views: {
        'activity-log': {
          templateUrl: '/client/states/app/user/activity-log.html',
          controller: 'ActivityLogController'
        }
      }
    })


    .state('app.user.session', {
      url: '/sessions/:eventSessionID',
      templateUrl: '/client/states/app/user/event-session.html',
      controller: 'EventSessionController',
      resolve: {
        eventSession: function ($stateParams, siteService) {
          // careful - might be an invalid or unauthorized ID
          var itemID = $stateParams.eventSessionID;
          //!! should we use ensureXyz here - to rely on cached info if we've got it?
          return siteService.eventSessions.ensure(itemID);
        },
        event: function ($stateParams, siteService, eventSession) {
          var eventID = eventSession.eventID;
          return siteService.events.ensure(eventID);
        }
      }
    })
    .state('app.user.session.participants', {
      url: '/participants',
      templateUrl: '/client/states/app/user/event-participants.html',
      controller: 'EventParticipantsController',
      resolve: {
        initialSelection: function ($stateParams, utilityService) {
          // careful - might be an invalid or unauthorized ID
          var itemID = $stateParams.activityID;
          if (itemID) {
            return utilityService.model.activityLog.getSet(itemID)
            .then(function (itemIDs) {
              return itemIDs;
            });
          }
          return null;
        }
      }
    })



    //.state('app.user.event-participants', {
    //  url: '/event/:eventID/participants',
    //  templateUrl: '/client/states/app/user/participants.html',
    //  controller: "EventParticipantsController"
    //})


    ;// closes $stateProvider
}]);
