app.config(['$stateProvider', 'AUTHORIZATION_ROLES', function ($stateProvider, AUTHORIZATION_ROLES) {

  // Now set up the states
  $stateProvider

    .state('app.spa-landing', {
      redirectTo: 'app.event-session-volunteer',
    })

    .state('app.event-session-volunteer', {
      abstract: true,
      templateUrl: '/client/states/app/event-session-volunteer/event-session-volunteer-container.html',
      controller: 'EventSessionController',
      resolve: {
        eventSession: function ($stateParams, utilityService, siteService) {

          var authenticatedIdentity = utilityService.model.authenticatedIdentity;

          if (authenticatedIdentity.type != 'eventSession') {
            return null;
          }

          var eventSessionID = authenticatedIdentity.id;
          return siteService.ensureEventSession(eventSessionID);
        },
        event: function (siteService, eventSession) {
          var eventID = eventSession.eventID;
          return siteService.ensureEvent(eventID);
        }
      }
    })

    .state('app.event-session-volunteer.check-in', {
      url: '/check-in',
      templateUrl: '/client/states/app/event-session-volunteer/check-in.html',
    })





  ;// closes $stateProvider
}]);
