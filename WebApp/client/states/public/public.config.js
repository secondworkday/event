app.config(['$stateProvider', 'AUTHORIZATION_ROLES', function ($stateProvider, AUTHORIZATION_ROLES) {

  // Now set up the states
  $stateProvider

  // PUBLIC --------------------------------------------------------------- //

  .state('public', {
      abstract: true,
      templateUrl: '/client/states/public/public.html',
      controller: 'PublicController',
      data: {
          allowedRoles: [AUTHORIZATION_ROLES.anonymous]
      }
  })
  .state('public.landing', {
      url: '/',
      templateUrl: '/client/states/public/landing.html',
  })
  .state('public.create-account', {
      url: '/create-account',
      templateUrl: '/client/states/public/create-account.html',
      controller: 'CreateAccountController'
  })
  .state('public.admin-sign-in', {
      url: '/sign-in',
      templateUrl: '/client/states/public/admin-sign-in.html',
      controller: 'SignInController'
  })
  .state('public.volunteer-sign-in', {
      url: '/volunteer',
      templateUrl: '/client/states/public/volunteer-sign-in.html',
      controller: 'VolunteerSignInController'
  })
  .state('public.reset-password', {
      url: '/reset-password/:authCode',
      templateUrl: '/client/states/public/reset-password.html',
      controller: 'ResetPasswordController'
  })
  .state('public.wait', {
      url: '/wait',
      templateUrl: '/client/states/public/wait.html'
  })
  .state('public.reconnecting', {
    url: '/reconnecting',
    templateUrl: '/client/states/public/connection/reconnecting.html'
  })
  .state('public.disconnected', {
    url: '/disconnected',
    templateUrl: '/client/states/public/connection/disconnected.html'
  });// closes $stateProvider
}]);
