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
  .state('app.home', {
      url: '/home',
      templateUrl: '/client/states/app/home.html'
  })
  .state('app.account', {
      url: '/account',
      templateUrl: '/client/states/app/account.html'
  })
  .state('app.system-admin', {
      url: '/system-admin',
      templateUrl: '/client/states/app/system-admin.html'
  })
  .state('app.users', {
      url: '/users',
      templateUrl: '/client/states/app/users.html'
  });// closes $stateProvider
}]);
