app.config(['$stateProvider', 'AUTHORIZATION_ROLES', function ($stateProvider, AUTHORIZATION_ROLES) {

  // Now set up the states
  $stateProvider

  // PUBLIC --------------------------------------------------------------- //

  .state('public', {
      abstract: true,
      templateUrl: '/client/states/public/public.html',
      controller: 'PublicController',
      data: {
          stateMapName: "Public master page",
          stateMapComment: "abstract, no authentication",
          allowedRoles: [AUTHORIZATION_ROLES.anonymous]
      }
  })
  .state('public.signin', {
      url: '/signin',
      templateUrl: '/client/states/public/signin.html',
      controller: 'SignInController',
      data: {
          stateMapName: "Sign In"
          // <%= SignInPageDemoDataJson %>
          //!! TODO do we need to pull in this demo data json another way??
      }
  })
  .state('public.job-seeker-signup', {
      url: '/signup',
      templateUrl: '/client/states/public/job-seeker-signup.html',
      controller: 'JobSeekerSignupController',
      data: {
          stateMapName: "Job Seeker Sign Up"
      }
  })
  .state('public.privacy-policy', {
      url: '/privacy-policy',
      templateUrl: '/client/states/public/privacy-policy.html',
      data: {
          stateMapName: "Privacy Policy"
      }
  })
  .state('public.wait', {
      url: '/wait',
      templateUrl: '/client/states/public/wait.html',
      controller: 'WaitController',
      data: {
          stateMapName: "Please Wait"
      }
  })

  .state('public.reconnecting', {
    url: '/reconnecting',
    templateUrl: '/client/states/public/connection/reconnecting.html',
    controller: 'ReconnectingController',
    data: {
      stateMapName: "SignalR Reconnecting"
    }
  })
  .state('public.disconnected', {
    url: '/disconnected',
    templateUrl: '/client/states/public/connection/disconnected.html',
    controller: 'DisconnectedController',
    data: {
      stateMapName: "SignalR Disconnected"
    }
  })


  ;// closes $stateProvider
}]);
