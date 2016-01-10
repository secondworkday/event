app.config(['$stateProvider', 'AUTHORIZATION_ROLES', function ($stateProvider, AUTHORIZATION_ROLES) {

  // Now set up the states
  $stateProvider

  // PUBLIC --------------------------------------------------------------- //

  .state('app', {
      abstract: true,
      templateUrl: '/client/app/app.html',
      controller: 'AppController',
      data: {
          allowedRoles: [AUTHORIZATION_ROLES.anonymous]
      }
  })
  .state('public.home', {
      url: '/home',
      templateUrl: '/client/states/public/home.html'
  })
  .state('public.account', {
      url: '/account',
      templateUrl: '/client/states/public/account.html'
  })
  .state('public.system-admin', {
      url: '/system-admin',
      templateUrl: '/client/states/public/system-admin.html'
  })
  .state('public.users', {
      url: '/users',
      templateUrl: '/client/states/public/users.html'
  });// closes $stateProvider
}]);
