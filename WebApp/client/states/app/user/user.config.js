app.config(['$stateProvider', 'AUTHORIZATION_ROLES', function ($stateProvider, AUTHORIZATION_ROLES) {

  // Now set up the states
  $stateProvider

    .state('app.user', {
        abstract: true,
        templateUrl: '/client/states/app/user/user-container.html',
        controller: 'UserController'
    })
    .state('app.user.home', {
        url: '/home',
        templateUrl: '/client/states/app/user/home.html',
    })
    .state('app.user.event-old', {
      url: '/event-old',
      templateUrl: '/client/states/app/user/event-old.html',
    })
/*
    .state('app.user.event', {
      url: '/event/:eventID/',
      templateUrl: '/client/states/app/user/event.html',
      controller: "EventController",
      resolve: {
        event: function (siteService, $stateParams) {
          var eventID = $stateParams.eventID;
          return siteService.ensureEvent(eventID);
        },
        eventSessionsIndex: function (siteService, $stateParams) {
          var eventID = $stateParams.eventID;
          return siteService.ensureEventSessions(eventID);
        },
        eventParticipantsIndex: function (siteService, $stateParams) {
          var eventID = $stateParams.eventID;
          return siteService.ensureEventParticipantsIndex(eventID);
        },
        eventParticipantGroupsIndex: function (siteService, eventParticipantsIndex) {
          return siteService.ensureEventParticipantGroups(siteService, eventParticipantsIndex);
        }
      }
    })
*/
    .state('app.user.event', {
      abstract: true,
      url: '/event/:eventID',
      templateUrl: '/client/states/app/user/event.html',
      controller: "EventController",
      resolve: {
        event: function ($stateParams, siteService) {
          var eventID = $stateParams.eventID;
          return siteService.ensureEvent(eventID);
        },
        eventSessionsIndex: function (siteService, $stateParams) {
          var eventID = $stateParams.eventID;
          return siteService.ensureEventSessions(eventID);
        },
        eventParticipantsIndex: function (siteService, $stateParams) {
          var eventID = $stateParams.eventID;
          return siteService.ensureEventParticipantsIndex(eventID);
        },
        eventParticipantGroupsIndex: function (siteService, eventParticipantsIndex) {
          return siteService.ensureEventParticipantGroups(siteService, eventParticipantsIndex);
        }
      },
      data: {
        pageTitle: 'Back',
        backState: 'app.job-seeker.career-step.home'
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
          templateUrl: '/client/states/app/user/schools.html',
          controller: "SchoolsController"
        }
      }
    })
    .state('app.user.event.participants', {
      url: '/participants',
      data: {
        'selectedTab': 2
      },
      views: {
        'participants': {
          templateUrl: '/client/states/app/user/participants.html',
          controller: "EventParticipantsController"
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


    //.state('app.user.event-participants', {
    //  url: '/event/:eventID/participants',
    //  templateUrl: '/client/states/app/user/participants.html',
    //  controller: "EventParticipantsController"
    //})


    ;// closes $stateProvider
}]);
