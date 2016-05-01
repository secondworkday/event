app.config(['$stateProvider', 'AUTHORIZATION_ROLES', function ($stateProvider, AUTHORIZATION_ROLES) {

  $stateProvider
    .state('app.system', {
      url: '/system',
      templateUrl: "/client/app/system/system.html",
      controller: 'SystemController'
    }); // closes $stateProvider
}]);
