app.config(['$stateProvider', 'AUTHORIZATION_ROLES', function ($stateProvider, AUTHORIZATION_ROLES) {

  // Now set up the states
  $stateProvider

  // PUBLIC --------------------------------------------------------------- //

  .state('public', {
      abstract: true,
      templateUrl: '/client/public/public.html',
      controller: 'PublicController',
      data: {
          allowedRoles: [AUTHORIZATION_ROLES.anonymous]
      }
  })
  .state('public.landing', {
      url: '/landing',
      templateUrl: '/client/states/public/landing.html'
  })
  .state('public.login', {
      url: '/login',
      templateUrl: '/client/states/public/login.html'
  });// closes $stateProvider
}]);
