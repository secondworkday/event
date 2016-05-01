app.config(['$stateProvider', 'AUTHORIZATION_ROLES', function ($stateProvider, AUTHORIZATION_ROLES) {

  // Now set up the states
  $stateProvider

  // PUBLIC --------------------------------------------------------------- //

  .state('public', {
      abstract: true,
      templateUrl: '/client/app/public/public.html',
      controller: 'PublicController',
      data: {
          stateMapName: "Public master page",
          stateMapComment: "abstract, no authentication",
          allowedRoles: [AUTHORIZATION_ROLES.anonymous]
      }
  })

  .state('public.landing', {
    url: '/',
    redirectTo: 'public.sign-in',
  })

  .state('public.sign-in', {
      url: '/sign-in',
      templateUrl: '/client/app/public/sign-in.html',
      controller: 'SignInController',
      data: {
          stateMapName: "Sign In"
          // <%= SignInPageDemoDataJson %>
          //!! TODO do we need to pull in this demo data json another way??
      }
  })
  .state('public.job-seeker-create-account', {
      url: '/create-account',
      templateUrl: '/client/app/public/job-seeker-create-account.html',
      controller: 'JobSeekerCreateAccountController',
      data: {
          stateMapName: "Job Seeker Create Account"
      }
  })
  .state('public.privacy-policy', {
      url: '/privacy-policy',
      templateUrl: '/client/app/public/privacy-policy.html',
      data: {
          stateMapName: "Privacy Policy"
      }
  })
  .state('public.reset-password', {
      url: '/reset-password/:authCode',
      templateUrl: '/client/app/public/reset-password.html',
      controller: 'ResetPasswordController'
  })
  .state('public.wait', {
      url: '/wait',
      templateUrl: '/client/app/public/wait.html',
      controller: 'WaitController',
      data: {
          stateMapName: "Please Wait"
      }
  })

  .state('public.reconnecting', {
    url: '/reconnecting',
    templateUrl: '/client/app/public/connection/reconnecting.html',
    controller: 'ReconnectingController',
    data: {
      stateMapName: "SignalR Reconnecting"
    }
  })
  .state('public.disconnected', {
    url: '/disconnected',
    templateUrl: '/client/app/public/connection/disconnected.html',
    controller: 'DisconnectedController',
    data: {
      stateMapName: "SignalR Disconnected"
    }
  })


  ;// closes $stateProvider
}]);
