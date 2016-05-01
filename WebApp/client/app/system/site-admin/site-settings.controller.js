(function() {
    'use strict';

    angular
        .module('myApp')
        .controller('SiteSettingsController', SiteSettingsController);

    SiteSettingsController.$inject = ['$scope', '$log'];

    /* @ngInject */
    function SiteSettingsController($scope, $log) {
      $log.debug('Loading SiteSettingsController...');

      $scope.settings = [
        {name: 'Dev site', enabled: false},
        {name: 'Enable demo features', enabled: false},
        {name: 'Allow site to send emails', enabled: false},
        {name: 'Allow create new tenants', enabled: false},
        {name: 'Allow create new users', enabled: false}
      ];

      $scope.jobBoards = [{ name: 'Simply Hired', availability: 'on', id: 1, code: 'SimplyHired'},
                          { name: 'US.Jobs', availability: 'on', id: 2, code: 'USjobs'},
                          { name: 'Indeed', availability: 'on', id: 3, code: 'Indeed'},
                          { name: 'Career Builder', availability: 'optional', id: 4, code: 'CareerBuilder'},
                          { name: 'Career Coach', availability: 'off', id: 5, code: 'CareerCoach'},
                          { name: 'Wanted', availability: 'off', id: 6, code: 'Wanted'},
                          { name: 'MAXOutreach', availability: 'off', id: 8, code: 'MaxOutreach'}];

    }
})();
