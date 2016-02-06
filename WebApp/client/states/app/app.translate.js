app.config(['$translateProvider', function ($translateProvider) {
  $translateProvider.translations('demo', {
    'PARTICIPANT_GROUP': 'Club',
    'LOCATION': 'Location'
  });

  $translateProvider.translations('ale', {
    'PARTICIPANT_GROUP': 'School',
    'LOCATION': 'Store'
  });

  $translateProvider.preferredLanguage('demo');
}]);