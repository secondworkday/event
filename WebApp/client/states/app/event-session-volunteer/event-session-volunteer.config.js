app.config(['$stateProvider', 'AUTHORIZATION_ROLES', function ($stateProvider, AUTHORIZATION_ROLES) {

  // Now set up the states
  $stateProvider

    .state('app.event-session-volunteer', {
        abstract: true,
        templateUrl: '/client/states/app/event-session-volunteer/event-session-volunteer-container.html',
        controller: 'EventSessionVolunteerController'
    })

    .state('app.event-session-volunteer.check-in', {
        url: '/check-in',
        templateUrl: '/client/states/app/event-session-volunteer/check-in.html',
    })





    ;// closes $stateProvider
}]);
