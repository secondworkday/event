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

    ;// closes $stateProvider
}]);
