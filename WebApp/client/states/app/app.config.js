app.config(['$stateProvider', 'AUTHORIZATION_ROLES', function ($stateProvider, AUTHORIZATION_ROLES) {

  // Now set up the states
  $stateProvider

    .state('app', {
        abstract: true,
        url: "?foobar&debug&facade",
        templateUrl: '/client/states/app/app.html',
        controller: 'AppController',
        resolve: {
            identity: function ($q, $stateParams, utilityService) {
                return utilityService.whenConnected()
                .then(function () {
                    return $q.when(utilityService.model.authenticatedIdentity);
                    //return utilityService.model.authenticatedIdentity;
                });
            },
            userIdentity: function ($q, utilityService, identity) {
                // (inject 'identity' so we can be sure we're connected and our identity is extablished)
                // (not sure if we also need to reference 'identity' to have it stick)
                var myIdentity = identity;
                return $q.when(utilityService.model.authenticatedUser);
            },
            tenant: function (utilityService, identity) {
              // by default we want to restrict access to the logged in user's tenant. 
              // resolving this here allows shared components to adhere to this.
              return utilityService.ensureTenantGroup(utilityService.model.authenticatedGroup.id);
            }
        },
        data: {
            allowedRoles: [AUTHORIZATION_ROLES.authenticated]
        }
    })

    ;// closes $stateProvider
}]);
