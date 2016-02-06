app.config(['$translateProvider', function ($translateProvider) {
  $translateProvider.translations('demo', {
    'PARTICIPANT_GROUP': 'Club',
    'PARTICIPANT_GROUPS': 'Clubs',
    'LOCATION': 'Location',
    'LOCATIONS': 'Locations'
  });

  $translateProvider.translations('ale', {
    'PARTICIPANT_GROUP': 'School',
    'PARTICIPANT_GROUPS': 'Schools',
    'LOCATION': 'Store',
    'LOCATIONS': 'Stores'
  });

  $translateProvider.preferredLanguage('demo');
}]);