app.controller('SystemSettingsController', function ($scope, $log, utilityService, siteService) {
  $log.debug('Loading SystemSettingsController...');

  $scope.settings = [
    {name: 'Dev Site', enabled: false},
    {name: 'Enable Demo Enhancement Features', enabled: false},
    {name: 'Allow Create New Tenants', enabled: false},
    {name: 'Allow Create New Users', enabled: false},
    {name: 'Enable SimplyHired', enabled: false},
    {name: 'Enable Indeed', enabled: false},
    {name: 'Enable US.Jobs', enabled: false}
  ];

});
