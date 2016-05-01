app.config(['$stateProvider', 'AUTHORIZATION_ROLES', function ($stateProvider, AUTHORIZATION_ROLES) {

  // Now set up the states
  $stateProvider

    .state('app', {
        abstract: true,
        url: "?foobar&debug&facade",
        templateUrl: '/client/app/app.html',
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
              return utilityService.tenantGroups.ensure(utilityService.model.authenticatedGroup.id);
            }
        },
        data: {
            allowedRoles: [AUTHORIZATION_ROLES.authenticated]
        }
    })


    .state('app.user-signin', {
      resolve: {
        redirect: ['$state', 'utilityService', 'identity', function ($state, utilityService, identity) {
          if (utilityService.listContains(identity.systemRoles, "SystemAdmin")) {
            $state.go('app.system.site-admin.dashboard');
          } else if (utilityService.listContains(identity.systemRoles, "OperationsAdmin")) {
            $state.go('app.system.ops-admin.dashboard');
          } else if (utilityService.listContains(identity.systemRoles, "DatabaseAdmin")) {
            $state.go('app.system.database-admin.dashboard');
          } else if (utilityService.listContains(identity.appRoles, "Admin")) {
            $state.go('app.counselor.home');
          } else if (utilityService.listContains(identity.appRoles, "JobCounselor")) {
            $state.go('app.counselor.home');
          } else {
            $state.go('app.job-seeker.career-step.home');
          }
        }]
      }
    })
    .state('app.user-created', {
      resolve: {
        redirect: ['$state', 'utilityService', 'identity', function ($state, utilityService, identity) {
          if (identity.roles && identity.roles.length > 0) {
            $state.go('app.user-signin');
          } else {
            $state.go('app.job-seeker.onboard.intro');
          }
        }]
      }
    })






    ;// closes $stateProvider
}]);
