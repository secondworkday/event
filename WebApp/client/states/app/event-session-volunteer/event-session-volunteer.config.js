app.config(['$stateProvider', 'AUTHORIZATION_ROLES', function ($stateProvider, AUTHORIZATION_ROLES) {

  // Now set up the states
  $stateProvider


    // safe landing zone in the spa - inspect to see if we're authenticated or not, and where to go next
    .state('app.spa-landing', {
      url: '/',
      controller: function ($scope, $state, utilityService, identity) {
        var model = utilityService.model;

        if (identity.type == 'eventSession') {
          $state.go('app.event-session.check-in', { eventSessionID: identity.id });
        }
        else {
          $state.go('public.welcome');
        }
      }
    })

    // authenticated volunteer - but not bound to a specific session - need to choose one
    .state('public.welcome', {
      url: '/welcome',
      template: 'Hi there and welcome'
    })


    // authenticated volunteer - but not bound to a specific session - need to choose one
    .state('app.event-sessions', {
      url: '/sessions',
    })



    .state('app.spa-landing2', {
      url: '/',
      redirectTo: 'app.event-session-volunteer',
    })



    .state('app.event-session', {
      abstract: true,
      templateUrl: '/client/states/app/event-session-volunteer/event-session-volunteer-container.html',
      controller: 'EventSessionController',
      resolve: {
        eventSession: function ($stateParams, utilityService, siteService) {

          var authenticatedIdentity = utilityService.model.authenticatedIdentity;

          if (authenticatedIdentity.itemType != 'eventSession') {
            return null;
          }

          var eventSessionID = authenticatedIdentity.itemID;
          return siteService.ensureEventSession(eventSessionID);
        },
        event: function (siteService, eventSession) {
          var eventID = eventSession.eventID;
          return siteService.ensureEvent(eventID);
        }
      }
    })

    .state('app.event-session.check-in', {
      url: '/check-in',
      controller: 'EventParticipantsController',
      resolve: {
        initialSelection: function () {
          return null;
        }
      },
      templateUrl: '/client/states/app/event-session-volunteer/check-in.html'
    })





  ;// closes $stateProvider
}]);
