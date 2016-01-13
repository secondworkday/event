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

    ;// closes $stateProvider
}]);
